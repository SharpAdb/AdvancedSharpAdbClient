using NSubstitute;
using System;
using System.IO;
using Xunit;

namespace AdvancedSharpAdbClient.DeviceCommands.Tests
{
    public partial class PackageManagerTests
    {
        protected static DeviceData Device { get; } = new()
        {
            Serial = "169.254.109.177:5555",
            State = DeviceState.Online
        };

        [Fact]
        public void ConstructorNullTest()
        {
            _ = Assert.Throws<ArgumentNullException>(() => new PackageManager(null, default));
            _ = Assert.Throws<ArgumentNullException>(() => new PackageManager(null, new DeviceData { Serial = "169.254.109.177:5555" }));
            _ = Assert.Throws<ArgumentOutOfRangeException>(() => new PackageManager(Substitute.For<IAdbClient>(), default));
        }

        [Theory]
        [InlineData("package:/system/app/Gallery2/Gallery2.apk=com.android.gallery3d", "com.android.gallery3d", "/system/app/Gallery2/Gallery2.apk")]
        [InlineData("package:mwc2015.be", "mwc2015.be", "")]
        public void PackagesPropertyTest(string command, string packageName, string path)
        {
            DummyAdbClient client = new();
            client.Commands["shell:pm list packages -f"] = command;
            PackageManager manager = new(client, Device);

            Assert.True(manager.Packages.ContainsKey(packageName));
            Assert.Equal(path, manager.Packages[packageName]);
        }

        [Fact]
        public void InstallRemotePackageTest()
        {
            DummyAdbClient adbClient = new();

            adbClient.Commands["shell:pm list packages -f"] = "package:/system/app/Gallery2/Gallery2.apk=com.android.gallery3d";
            adbClient.Commands["shell:pm install \"/data/base.apk\""] = "Success";
            adbClient.Commands["shell:pm install -r -t \"/data/base.apk\""] = "Success";

            PackageManager manager = new(adbClient, Device);

            manager.InstallRemotePackage("/data/base.apk", new InstallProgress(PackageInstallProgressState.Installing));

            Assert.Equal(2, adbClient.ReceivedCommands.Count);
            Assert.Equal("shell:pm install \"/data/base.apk\"", adbClient.ReceivedCommands[1]);

            adbClient.ReceivedCommands.Clear();

            manager.InstallRemotePackage("/data/base.apk", new InstallProgress(PackageInstallProgressState.Installing), "-r", "-t");

            Assert.Single(adbClient.ReceivedCommands);
            Assert.Equal("shell:pm install -r -t \"/data/base.apk\"", adbClient.ReceivedCommands[0]);
        }

        [Fact]
        public void InstallPackageTest()
        {
            DummySyncService syncService = new();

            DummyAdbClient adbClient = new();

            adbClient.Commands["shell:pm list packages -f"] = "package:/system/app/Gallery2/Gallery2.apk=com.android.gallery3d";
            adbClient.Commands["shell:pm install \"/data/local/tmp/base.apk\""] = "Success";
            adbClient.Commands["shell:rm \"/data/local/tmp/base.apk\""] = string.Empty;

            PackageManager manager = new(adbClient, Device, (c, d) => syncService);

            manager.InstallPackage("Assets/TestApp/base.apk",
                new InstallProgress(
                    PackageInstallProgressState.Preparing,
                    PackageInstallProgressState.Uploading,
                    PackageInstallProgressState.Installing,
                    PackageInstallProgressState.PostInstall,
                    PackageInstallProgressState.Finished));

            Assert.Equal(3, adbClient.ReceivedCommands.Count);
            Assert.Equal("shell:pm install \"/data/local/tmp/base.apk\"", adbClient.ReceivedCommands[1]);
            Assert.Equal("shell:rm \"/data/local/tmp/base.apk\"", adbClient.ReceivedCommands[2]);

            Assert.Single(syncService.UploadedFiles);
            Assert.True(syncService.UploadedFiles.ContainsKey("/data/local/tmp/base.apk"));
        }

        [Fact]
        public void InstallMultipleRemotePackageTest()
        {
            DummyAdbClient adbClient = new();

            adbClient.Commands["shell:pm list packages -f"] = "package:/system/app/Gallery2/Gallery2.apk=com.android.gallery3d";
            adbClient.Commands["shell:pm install-create"] = "Success: created install session [936013062]";
            adbClient.Commands["shell:pm install-create -r -t"] = "Success: created install session [936013062]";
            adbClient.Commands["shell:pm install-create -p com.google.android.gms"] = "Success: created install session [936013062]";
            adbClient.Commands["shell:pm install-create -p com.google.android.gms -r -t"] = "Success: created install session [936013062]";
            adbClient.Commands["shell:pm install-write 936013062 base.apk \"/data/base.apk\""] = "Success";
            adbClient.Commands["shell:pm install-write 936013062 split0.apk \"/data/split_config.arm64_v8a.apk\""] = "Success";
            adbClient.Commands["shell:pm install-write 936013062 split1.apk \"/data/split_config.xxhdpi.apk\""] = "Success";
            adbClient.Commands["shell:pm install-commit 936013062"] = "Success";

            PackageManager manager = new(adbClient, Device);

            manager.InstallMultipleRemotePackage("/data/base.apk", ["/data/split_config.arm64_v8a.apk", "/data/split_config.xxhdpi.apk"],
                new InstallProgress(
                    PackageInstallProgressState.CreateSession,
                    PackageInstallProgressState.WriteSession,
                    PackageInstallProgressState.Installing));

            Assert.Equal(6, adbClient.ReceivedCommands.Count);
            Assert.Equal("shell:pm install-create", adbClient.ReceivedCommands[1]);
            Assert.Equal("shell:pm install-write 936013062 base.apk \"/data/base.apk\"", adbClient.ReceivedCommands[2]);
            Assert.Equal("shell:pm install-write 936013062 split0.apk \"/data/split_config.arm64_v8a.apk\"", adbClient.ReceivedCommands[3]);
            Assert.Equal("shell:pm install-write 936013062 split1.apk \"/data/split_config.xxhdpi.apk\"", adbClient.ReceivedCommands[4]);
            Assert.Equal("shell:pm install-commit 936013062", adbClient.ReceivedCommands[5]);

            adbClient.ReceivedCommands.Clear();

            manager.InstallMultipleRemotePackage("/data/base.apk", ["/data/split_config.arm64_v8a.apk", "/data/split_config.xxhdpi.apk"],
                new InstallProgress(
                    PackageInstallProgressState.CreateSession,
                    PackageInstallProgressState.WriteSession,
                    PackageInstallProgressState.Installing), "-r", "-t");

            Assert.Equal(5, adbClient.ReceivedCommands.Count);
            Assert.Equal("shell:pm install-create -r -t", adbClient.ReceivedCommands[0]);
            Assert.Equal("shell:pm install-write 936013062 base.apk \"/data/base.apk\"", adbClient.ReceivedCommands[1]);
            Assert.Equal("shell:pm install-write 936013062 split0.apk \"/data/split_config.arm64_v8a.apk\"", adbClient.ReceivedCommands[2]);
            Assert.Equal("shell:pm install-write 936013062 split1.apk \"/data/split_config.xxhdpi.apk\"", adbClient.ReceivedCommands[3]);
            Assert.Equal("shell:pm install-commit 936013062", adbClient.ReceivedCommands[4]);

            adbClient.ReceivedCommands.Clear();

            manager.InstallMultipleRemotePackage(["/data/split_config.arm64_v8a.apk", "/data/split_config.xxhdpi.apk"], "com.google.android.gms",
                new InstallProgress(
                    PackageInstallProgressState.CreateSession,
                    PackageInstallProgressState.WriteSession,
                    PackageInstallProgressState.Installing));

            Assert.Equal(4, adbClient.ReceivedCommands.Count);
            Assert.Equal("shell:pm install-create -p com.google.android.gms", adbClient.ReceivedCommands[0]);
            Assert.Equal("shell:pm install-write 936013062 split0.apk \"/data/split_config.arm64_v8a.apk\"", adbClient.ReceivedCommands[1]);
            Assert.Equal("shell:pm install-write 936013062 split1.apk \"/data/split_config.xxhdpi.apk\"", adbClient.ReceivedCommands[2]);
            Assert.Equal("shell:pm install-commit 936013062", adbClient.ReceivedCommands[3]);

            adbClient.ReceivedCommands.Clear();

            manager.InstallMultipleRemotePackage(["/data/split_config.arm64_v8a.apk", "/data/split_config.xxhdpi.apk"], "com.google.android.gms",
                new InstallProgress(
                    PackageInstallProgressState.CreateSession,
                    PackageInstallProgressState.WriteSession,
                    PackageInstallProgressState.Installing), "-r", "-t");

            Assert.Equal(4, adbClient.ReceivedCommands.Count);
            Assert.Equal("shell:pm install-create -p com.google.android.gms -r -t", adbClient.ReceivedCommands[0]);
            Assert.Equal("shell:pm install-write 936013062 split0.apk \"/data/split_config.arm64_v8a.apk\"", adbClient.ReceivedCommands[1]);
            Assert.Equal("shell:pm install-write 936013062 split1.apk \"/data/split_config.xxhdpi.apk\"", adbClient.ReceivedCommands[2]);
            Assert.Equal("shell:pm install-commit 936013062", adbClient.ReceivedCommands[3]);
        }

        [Fact]
        public void InstallMultiplePackageTest()
        {
            DummySyncService syncService = new();

            DummyAdbClient adbClient = new();

            adbClient.Commands["shell:pm list packages -f"] = "package:/system/app/Gallery2/Gallery2.apk=com.android.gallery3d";
            adbClient.Commands["shell:pm install-create"] = "Success: created install session [936013062]";
            adbClient.Commands["shell:pm install-create -p com.google.android.gms"] = "Success: created install session [936013062]";
            adbClient.Commands["shell:pm install-write 936013062 base.apk \"/data/local/tmp/base.apk\""] = "Success";
            adbClient.Commands["shell:pm install-write 936013062 split0.apk \"/data/local/tmp/split_config.arm64_v8a.apk\""] = "Success";
            adbClient.Commands["shell:pm install-write 936013062 split1.apk \"/data/local/tmp/split_config.xxhdpi.apk\""] = "Success";
            adbClient.Commands["shell:pm install-commit 936013062"] = "Success";
            adbClient.Commands["shell:rm \"/data/local/tmp/base.apk\""] = string.Empty;
            adbClient.Commands["shell:rm \"/data/local/tmp/split_config.arm64_v8a.apk\""] = string.Empty;
            adbClient.Commands["shell:rm \"/data/local/tmp/split_config.xxhdpi.apk\""] = string.Empty;

            PackageManager manager = new(adbClient, Device, (c, d) => syncService);

            manager.InstallMultiplePackage("Assets/TestApp/base.apk", ["Assets/TestApp/split_config.arm64_v8a.apk", "Assets/TestApp/split_config.xxhdpi.apk"],
                new InstallProgress(
                    PackageInstallProgressState.Preparing,
                    PackageInstallProgressState.Uploading,
                    PackageInstallProgressState.CreateSession,
                    PackageInstallProgressState.WriteSession,
                    PackageInstallProgressState.Installing,
                    PackageInstallProgressState.PostInstall,
                    PackageInstallProgressState.Finished));

            Assert.Equal(9, adbClient.ReceivedCommands.Count);
            Assert.Equal("shell:pm install-create", adbClient.ReceivedCommands[1]);
            Assert.Equal("shell:pm install-write 936013062 base.apk \"/data/local/tmp/base.apk\"", adbClient.ReceivedCommands[2]);
            Assert.Equal("shell:pm install-write 936013062 split0.apk \"/data/local/tmp/split_config.arm64_v8a.apk\"", adbClient.ReceivedCommands[3]);
            Assert.Equal("shell:pm install-write 936013062 split1.apk \"/data/local/tmp/split_config.xxhdpi.apk\"", adbClient.ReceivedCommands[4]);
            Assert.Equal("shell:pm install-commit 936013062", adbClient.ReceivedCommands[5]);
            Assert.Equal("shell:rm \"/data/local/tmp/split_config.arm64_v8a.apk\"", adbClient.ReceivedCommands[6]);
            Assert.Equal("shell:rm \"/data/local/tmp/split_config.xxhdpi.apk\"", adbClient.ReceivedCommands[7]);
            Assert.Equal("shell:rm \"/data/local/tmp/base.apk\"", adbClient.ReceivedCommands[8]);

            Assert.Equal(3, syncService.UploadedFiles.Count);
            Assert.True(syncService.UploadedFiles.ContainsKey("/data/local/tmp/base.apk"));
            Assert.True(syncService.UploadedFiles.ContainsKey("/data/local/tmp/split_config.arm64_v8a.apk"));
            Assert.True(syncService.UploadedFiles.ContainsKey("/data/local/tmp/split_config.xxhdpi.apk"));

            syncService.UploadedFiles.Clear();
            adbClient.ReceivedCommands.Clear();

            manager.InstallMultiplePackage(["Assets/TestApp/split_config.arm64_v8a.apk", "Assets/TestApp/split_config.xxhdpi.apk"], "com.google.android.gms",
                new InstallProgress(
                    PackageInstallProgressState.Preparing,
                    PackageInstallProgressState.Uploading,
                    PackageInstallProgressState.CreateSession,
                    PackageInstallProgressState.WriteSession,
                    PackageInstallProgressState.Installing,
                    PackageInstallProgressState.PostInstall,
                    PackageInstallProgressState.Finished));

            Assert.Equal(6, adbClient.ReceivedCommands.Count);
            Assert.Equal("shell:pm install-create -p com.google.android.gms", adbClient.ReceivedCommands[0]);
            Assert.Equal("shell:pm install-write 936013062 split0.apk \"/data/local/tmp/split_config.arm64_v8a.apk\"", adbClient.ReceivedCommands[1]);
            Assert.Equal("shell:pm install-write 936013062 split1.apk \"/data/local/tmp/split_config.xxhdpi.apk\"", adbClient.ReceivedCommands[2]);
            Assert.Equal("shell:pm install-commit 936013062", adbClient.ReceivedCommands[3]);
            Assert.Equal("shell:rm \"/data/local/tmp/split_config.arm64_v8a.apk\"", adbClient.ReceivedCommands[4]);
            Assert.Equal("shell:rm \"/data/local/tmp/split_config.xxhdpi.apk\"", adbClient.ReceivedCommands[5]);

            Assert.Equal(2, syncService.UploadedFiles.Count);
            Assert.True(syncService.UploadedFiles.ContainsKey("/data/local/tmp/split_config.arm64_v8a.apk"));
            Assert.True(syncService.UploadedFiles.ContainsKey("/data/local/tmp/split_config.xxhdpi.apk"));
        }

        [Fact]
        public void UninstallPackageTest()
        {
            DummyAdbClient client = new();
            client.Commands["shell:pm list packages -f"] = "package:/system/app/Gallery2/Gallery2.apk=com.android.gallery3d";
            client.Commands["shell:pm uninstall com.android.gallery3d"] = "Success";
            PackageManager manager = new(client, Device);

            // Command should execute correctly; if the wrong command is passed an exception
            // would be thrown.
            manager.UninstallPackage("com.android.gallery3d");
        }

        [Fact]
        public void GetPackageVersionInfoTest()
        {
            DummyAdbClient client = new();
            client.Commands["shell:dumpsys package com.google.android.gms"] = File.ReadAllText("Assets/DumpSys.GApps.txt");
            PackageManager manager = new(client, Device, skipInit: true);

            VersionInfo versionInfo = manager.GetVersionInfo("com.google.android.gms");
            Assert.Equal(11062448, versionInfo.VersionCode);
            Assert.Equal("11.0.62 (448-160311229)", versionInfo.VersionName);
        }

        private struct InstallProgress(params PackageInstallProgressState[] states) : IProgress<InstallProgressEventArgs>
        {
            private PackageInstallProgressState? state;
            private int packageFinished;
            private int packageRequired;
            private double uploadProgress;

            private int step = 0;

            public void Report(InstallProgressEventArgs value)
            {
                if (value.State == state)
                {
                    Assert.True(uploadProgress <= value.UploadProgress, $"{nameof(value.UploadProgress)}: {value.UploadProgress} is less than {uploadProgress}.");
                    Assert.True(packageFinished <= value.PackageFinished, $"{nameof(value.PackageFinished)}: {value.PackageFinished} is less than {packageFinished}.");
                }
                else
                {
                    Assert.Equal(states[step++], value.State);
                }

                if (value.State is
                    PackageInstallProgressState.CreateSession
                    or PackageInstallProgressState.Installing
                    or PackageInstallProgressState.Finished)
                {
                    Assert.Equal(0, value.UploadProgress);
                    Assert.Equal(0, value.PackageRequired);
                    Assert.Equal(0, value.PackageFinished);
                }
                else
                {
                    if (packageRequired == 0)
                    {
                        packageRequired = value.PackageRequired;
                    }
                    else
                    {
                        Assert.Equal(packageRequired, value.PackageRequired);
                    }
                }

                state = value.State;
                packageFinished = value.PackageFinished;
                uploadProgress = value.UploadProgress;
            }
        }
    }
}
