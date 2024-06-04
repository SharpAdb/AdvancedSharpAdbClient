using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace AdvancedSharpAdbClient.DeviceCommands.Tests
{
    public partial class PackageManagerTests
    {
        [Fact]
        public async Task InstallRemotePackageAsyncTest()
        {
            DummyAdbClient adbClient = new();

            adbClient.Commands["shell:pm list packages -f"] = "package:/system/app/Gallery2/Gallery2.apk=com.android.gallery3d";
            adbClient.Commands["shell:pm install \"/data/base.apk\""] = "Success";
            adbClient.Commands["shell:pm install -r -t \"/data/base.apk\""] = "Success";

            PackageManager manager = new(adbClient, Device);

            await manager.InstallRemotePackageAsync("/data/base.apk", new InstallProgress(PackageInstallProgressState.Installing));

            Assert.Equal(2, adbClient.ReceivedCommands.Count);
            Assert.Equal("shell:pm install \"/data/base.apk\"", adbClient.ReceivedCommands[1]);

            adbClient.ReceivedCommands.Clear();

            await manager.InstallRemotePackageAsync("/data/base.apk", new InstallProgress(PackageInstallProgressState.Installing), default, "-r", "-t");

            Assert.Single(adbClient.ReceivedCommands);
            Assert.Equal("shell:pm install -r -t \"/data/base.apk\"", adbClient.ReceivedCommands[0]);
        }

        [Fact]
        public async Task InstallPackageAsyncTest()
        {
            DummySyncService syncService = new();

            DummyAdbClient adbClient = new();

            adbClient.Commands["shell:pm list packages -f"] = "package:/system/app/Gallery2/Gallery2.apk=com.android.gallery3d";
            adbClient.Commands["shell:pm install \"/data/local/tmp/base.apk\""] = "Success";
            adbClient.Commands["shell:rm \"/data/local/tmp/base.apk\""] = string.Empty;

            PackageManager manager = new(adbClient, Device, (c, d) => syncService);

            await manager.InstallPackageAsync("Assets/TestApp/base.apk",
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
        public async Task InstallMultipleRemotePackageAsyncTest()
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

            await manager.InstallMultipleRemotePackageAsync("/data/base.apk", ["/data/split_config.arm64_v8a.apk", "/data/split_config.xxhdpi.apk"],
                new InstallProgress(
                    PackageInstallProgressState.CreateSession,
                    PackageInstallProgressState.WriteSession,
                    PackageInstallProgressState.Installing));

            Assert.Equal(6, adbClient.ReceivedCommands.Count);
            Assert.Equal("shell:pm install-create", adbClient.ReceivedCommands[1]);
            Assert.Equal("shell:pm install-write 936013062 base.apk \"/data/base.apk\"", adbClient.ReceivedCommands[2]);
            Assert.Contains("shell:pm install-write 936013062 split0.apk \"/data/split_config.arm64_v8a.apk\"", adbClient.ReceivedCommands[3..5]);
            Assert.Contains("shell:pm install-write 936013062 split1.apk \"/data/split_config.xxhdpi.apk\"", adbClient.ReceivedCommands[3..5]);
            Assert.Equal("shell:pm install-commit 936013062", adbClient.ReceivedCommands[5]);

            adbClient.ReceivedCommands.Clear();

            await manager.InstallMultipleRemotePackageAsync("/data/base.apk", ["/data/split_config.arm64_v8a.apk", "/data/split_config.xxhdpi.apk"],
                new InstallProgress(
                    PackageInstallProgressState.CreateSession,
                    PackageInstallProgressState.WriteSession,
                    PackageInstallProgressState.Installing), default, "-r", "-t");

            Assert.Equal(5, adbClient.ReceivedCommands.Count);
            Assert.Equal("shell:pm install-create -r -t", adbClient.ReceivedCommands[0]);
            Assert.Equal("shell:pm install-write 936013062 base.apk \"/data/base.apk\"", adbClient.ReceivedCommands[1]);
            Assert.Contains("shell:pm install-write 936013062 split0.apk \"/data/split_config.arm64_v8a.apk\"", adbClient.ReceivedCommands[2..4]);
            Assert.Contains("shell:pm install-write 936013062 split1.apk \"/data/split_config.xxhdpi.apk\"", adbClient.ReceivedCommands[2..4]);
            Assert.Equal("shell:pm install-commit 936013062", adbClient.ReceivedCommands[4]);

            adbClient.ReceivedCommands.Clear();

            await manager.InstallMultipleRemotePackageAsync(["/data/split_config.arm64_v8a.apk", "/data/split_config.xxhdpi.apk"], "com.google.android.gms",
                new InstallProgress(
                    PackageInstallProgressState.CreateSession,
                    PackageInstallProgressState.WriteSession,
                    PackageInstallProgressState.Installing));

            Assert.Equal(4, adbClient.ReceivedCommands.Count);
            Assert.Equal("shell:pm install-create -p com.google.android.gms", adbClient.ReceivedCommands[0]);
            Assert.Contains("shell:pm install-write 936013062 split0.apk \"/data/split_config.arm64_v8a.apk\"", adbClient.ReceivedCommands[1..3]);
            Assert.Contains("shell:pm install-write 936013062 split1.apk \"/data/split_config.xxhdpi.apk\"", adbClient.ReceivedCommands[1..3]);
            Assert.Equal("shell:pm install-commit 936013062", adbClient.ReceivedCommands[3]);

            adbClient.ReceivedCommands.Clear();

            await manager.InstallMultipleRemotePackageAsync(["/data/split_config.arm64_v8a.apk", "/data/split_config.xxhdpi.apk"], "com.google.android.gms",
                new InstallProgress(
                    PackageInstallProgressState.CreateSession,
                    PackageInstallProgressState.WriteSession,
                    PackageInstallProgressState.Installing), default, "-r", "-t");

            Assert.Equal(4, adbClient.ReceivedCommands.Count);
            Assert.Equal("shell:pm install-create -p com.google.android.gms -r -t", adbClient.ReceivedCommands[0]);
            Assert.Contains("shell:pm install-write 936013062 split0.apk \"/data/split_config.arm64_v8a.apk\"", adbClient.ReceivedCommands[1..3]);
            Assert.Contains("shell:pm install-write 936013062 split1.apk \"/data/split_config.xxhdpi.apk\"", adbClient.ReceivedCommands[1..3]);
            Assert.Equal("shell:pm install-commit 936013062", adbClient.ReceivedCommands[3]);
        }

        [Fact]
        public async Task InstallMultiplePackageAsyncTest()
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

            await manager.InstallMultiplePackageAsync("Assets/TestApp/base.apk", ["Assets/TestApp/split_config.arm64_v8a.apk", "Assets/TestApp/split_config.xxhdpi.apk"],
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
            Assert.Contains("shell:pm install-write 936013062 split0.apk \"/data/local/tmp/split_config.arm64_v8a.apk\"", adbClient.ReceivedCommands[3..5]);
            Assert.Contains("shell:pm install-write 936013062 split1.apk \"/data/local/tmp/split_config.xxhdpi.apk\"", adbClient.ReceivedCommands[3..5]);
            Assert.Equal("shell:pm install-commit 936013062", adbClient.ReceivedCommands[5]);
            Assert.Contains("shell:rm \"/data/local/tmp/split_config.arm64_v8a.apk\"", adbClient.ReceivedCommands[6..8]);
            Assert.Contains("shell:rm \"/data/local/tmp/split_config.xxhdpi.apk\"", adbClient.ReceivedCommands[6..8]);
            Assert.Equal("shell:rm \"/data/local/tmp/base.apk\"", adbClient.ReceivedCommands[8]);

            Assert.Equal(3, syncService.UploadedFiles.Count);
            Assert.True(syncService.UploadedFiles.ContainsKey("/data/local/tmp/base.apk"));
            Assert.True(syncService.UploadedFiles.ContainsKey("/data/local/tmp/split_config.arm64_v8a.apk"));
            Assert.True(syncService.UploadedFiles.ContainsKey("/data/local/tmp/split_config.xxhdpi.apk"));

            syncService.UploadedFiles.Clear();
            adbClient.ReceivedCommands.Clear();

            await manager.InstallMultiplePackageAsync(["Assets/TestApp/split_config.arm64_v8a.apk", "Assets/TestApp/split_config.xxhdpi.apk"], "com.google.android.gms",
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
            Assert.Contains("shell:pm install-write 936013062 split0.apk \"/data/local/tmp/split_config.arm64_v8a.apk\"", adbClient.ReceivedCommands[1..3]);
            Assert.Contains("shell:pm install-write 936013062 split1.apk \"/data/local/tmp/split_config.xxhdpi.apk\"", adbClient.ReceivedCommands[1..3]);
            Assert.Equal("shell:pm install-commit 936013062", adbClient.ReceivedCommands[3]);
            Assert.Contains("shell:rm \"/data/local/tmp/split_config.arm64_v8a.apk\"", adbClient.ReceivedCommands[4..6]);
            Assert.Contains("shell:rm \"/data/local/tmp/split_config.xxhdpi.apk\"", adbClient.ReceivedCommands[4..6]);

            Assert.Equal(2, syncService.UploadedFiles.Count);
            Assert.True(syncService.UploadedFiles.ContainsKey("/data/local/tmp/split_config.arm64_v8a.apk"));
            Assert.True(syncService.UploadedFiles.ContainsKey("/data/local/tmp/split_config.xxhdpi.apk"));
        }

        [Fact]
        public async Task UninstallPackageAsyncTest()
        {
            DummyAdbClient client = new();
            client.Commands["shell:pm list packages -f"] = "package:/system/app/Gallery2/Gallery2.apk=com.android.gallery3d";
            client.Commands["shell:pm uninstall com.android.gallery3d"] = "Success";
            PackageManager manager = new(client, Device);

            // Command should execute correctly; if the wrong command is passed an exception
            // would be thrown.
            await manager.UninstallPackageAsync("com.android.gallery3d");
        }

        [Fact]
        public async Task GetPackageVersionInfoAsyncTest()
        {
            DummyAdbClient client = new();
            client.Commands["shell:dumpsys package com.google.android.gms"] = File.ReadAllText("Assets/DumpSys.GApps.txt");
            PackageManager manager = new(client, Device, skipInit: true);

            VersionInfo versionInfo = await manager.GetVersionInfoAsync("com.google.android.gms");
            Assert.Equal(11062448, versionInfo.VersionCode);
            Assert.Equal("11.0.62 (448-160311229)", versionInfo.VersionName);
        }
    }
}
