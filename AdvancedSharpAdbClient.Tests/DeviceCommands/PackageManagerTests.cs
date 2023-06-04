using AdvancedSharpAdbClient.Tests;
using Moq;
using System;
using System.IO;
using Xunit;

namespace AdvancedSharpAdbClient.DeviceCommands.Tests
{

    public class PackageManagerTests
    {
        [Fact]
        public void ConstructorNullTest()
        {
            _ = Assert.Throws<ArgumentNullException>(() => new PackageManager(null, null));
            _ = Assert.Throws<ArgumentNullException>(() => new PackageManager(null, new DeviceData()));
            _ = Assert.Throws<ArgumentNullException>(() => new PackageManager(Mock.Of<IAdbClient>(), null));
        }

        [Fact]
        public void PackagesPropertyTest()
        {
            DeviceData device = new()
            {
                State = DeviceState.Online
            };

            DummyAdbClient client = new();
            client.Commands.Add("pm list packages -f", "package:/system/app/Gallery2/Gallery2.apk=com.android.gallery3d");
            PackageManager manager = new(client, device);

            Assert.True(manager.Packages.ContainsKey("com.android.gallery3d"));
            Assert.Equal("/system/app/Gallery2/Gallery2.apk", manager.Packages["com.android.gallery3d"]);
        }

        [Fact]
        public void PackagesPropertyTest2()
        {
            DeviceData device = new()
            {
                State = DeviceState.Online
            };

            DummyAdbClient client = new();
            client.Commands.Add("pm list packages -f", "package:mwc2015.be");
            PackageManager manager = new(client, device);

            Assert.True(manager.Packages.ContainsKey("mwc2015.be"));
            Assert.Null(manager.Packages["mwc2015.be"]);
        }

        [Fact]
        public void InstallRemotePackageTest()
        {
            DummyAdbClient adbClient = new();

            adbClient.Commands.Add("pm list packages -f", "package:/system/app/Gallery2/Gallery2.apk=com.android.gallery3d");
            adbClient.Commands.Add("pm install \"/data/test.apk\"", string.Empty);
            adbClient.Commands.Add("pm install -r \"/data/test.apk\"", string.Empty);

            DeviceData device = new()
            {
                State = DeviceState.Online
            };

            PackageManager manager = new(adbClient, device);
            manager.InstallRemotePackage("/data/test.apk", false);

            Assert.Equal(2, adbClient.ReceivedCommands.Count);
            Assert.Equal("pm install \"/data/test.apk\"", adbClient.ReceivedCommands[1]);

            manager.InstallRemotePackage("/data/test.apk", true);

            Assert.Equal(3, adbClient.ReceivedCommands.Count);
            Assert.Equal("pm install -r \"/data/test.apk\"", adbClient.ReceivedCommands[2]);
        }

        [Fact]
        public void InstallPackageTest()
        {
            lock (FactoriesTests.locker)
            {
                DummySyncService syncService = new();
                Factories.SyncServiceFactory = (c, d) => syncService;

                DummyAdbClient adbClient = new();

                adbClient.Commands.Add("pm list packages -f", "package:/system/app/Gallery2/Gallery2.apk=com.android.gallery3d");
                adbClient.Commands.Add("pm install \"/data/local/tmp/test.txt\"", string.Empty);
                adbClient.Commands.Add("rm \"/data/local/tmp/test.txt\"", string.Empty);

                DeviceData device = new()
                {
                    State = DeviceState.Online
                };

                PackageManager manager = new(adbClient, device);
                manager.InstallPackage("Assets/test.txt", false);
                Assert.Equal(3, adbClient.ReceivedCommands.Count);
                Assert.Equal("pm install \"/data/local/tmp/test.txt\"", adbClient.ReceivedCommands[1]);
                Assert.Equal("rm \"/data/local/tmp/test.txt\"", adbClient.ReceivedCommands[2]);

                Assert.Single(syncService.UploadedFiles);
                Assert.True(syncService.UploadedFiles.ContainsKey("/data/local/tmp/test.txt"));

                Factories.Reset();
            }
        }

        [Fact]
        public void InstallMultipleRemotePackageTest()
        {
            DummyAdbClient adbClient = new();

            adbClient.Commands.Add("pm list packages -f", "package:/system/app/Gallery2/Gallery2.apk=com.android.gallery3d");
            adbClient.Commands.Add("pm install-create", "Success: created install session [936013062]");
            adbClient.Commands.Add("pm install-create -r", "Success: created install session [936013062]");
            adbClient.Commands.Add("pm install-write 936013062 base.apk \"/data/base.apk\"", string.Empty);
            adbClient.Commands.Add("pm install-write 936013062 splitapp0.apk \"/data/split-dpi.apk\"", string.Empty);
            adbClient.Commands.Add("pm install-write 936013062 splitapp1.apk \"/data/split-abi.apk\"", string.Empty);
            adbClient.Commands.Add("pm install-commit 936013062", string.Empty);

            DeviceData device = new()
            {
                State = DeviceState.Online
            };

            PackageManager manager = new(adbClient, device);
            manager.InstallMultipleRemotePackage("/data/base.apk", new string[] { "/data/split-dpi.apk", "/data/split-abi.apk" }, false);

            Assert.Equal(6, adbClient.ReceivedCommands.Count);
            Assert.Equal("pm install-create", adbClient.ReceivedCommands[1]);
            Assert.Equal("pm install-write 936013062 base.apk \"/data/base.apk\"", adbClient.ReceivedCommands[2]);
            Assert.Equal("pm install-write 936013062 splitapp0.apk \"/data/split-dpi.apk\"", adbClient.ReceivedCommands[3]);
            Assert.Equal("pm install-write 936013062 splitapp1.apk \"/data/split-abi.apk\"", adbClient.ReceivedCommands[4]);
            Assert.Equal("pm install-commit 936013062", adbClient.ReceivedCommands[5]);

            manager.InstallMultipleRemotePackage("/data/base.apk", new string[] { "/data/split-dpi.apk", "/data/split-abi.apk" }, true);

            Assert.Equal(11, adbClient.ReceivedCommands.Count);
            Assert.Equal("pm install-create -r", adbClient.ReceivedCommands[6]);
            Assert.Equal("pm install-write 936013062 base.apk \"/data/base.apk\"", adbClient.ReceivedCommands[7]);
            Assert.Equal("pm install-write 936013062 splitapp0.apk \"/data/split-dpi.apk\"", adbClient.ReceivedCommands[8]);
            Assert.Equal("pm install-write 936013062 splitapp1.apk \"/data/split-abi.apk\"", adbClient.ReceivedCommands[9]);
            Assert.Equal("pm install-commit 936013062", adbClient.ReceivedCommands[10]);
        }

        [Fact]
        public void InstallMultiplePackageTest()
        {
            lock (FactoriesTests.locker)
            {
                DummySyncService syncService = new();
                Factories.SyncServiceFactory = (c, d) => syncService;

                DummyAdbClient adbClient = new();

                adbClient.Commands.Add("pm list packages -f", "package:/system/app/Gallery2/Gallery2.apk=com.android.gallery3d");
                adbClient.Commands.Add("pm install-create", "Success: created install session [936013062]");
                adbClient.Commands.Add("pm install-write 936013062 base.apk \"/data/local/tmp/test.txt\"", string.Empty);
                adbClient.Commands.Add("pm install-write 936013062 splitapp0.apk \"/data/local/tmp/gapps.txt\"", string.Empty);
                adbClient.Commands.Add("pm install-write 936013062 splitapp1.apk \"/data/local/tmp/logcat.bin\"", string.Empty);
                adbClient.Commands.Add("pm install-commit 936013062", string.Empty);
                adbClient.Commands.Add("rm \"/data/local/tmp/test.txt\"", string.Empty);
                adbClient.Commands.Add("rm \"/data/local/tmp/gapps.txt\"", string.Empty);
                adbClient.Commands.Add("rm \"/data/local/tmp/logcat.bin\"", string.Empty);

                DeviceData device = new()
                {
                    State = DeviceState.Online
                };

                PackageManager manager = new(adbClient, device);
                manager.InstallMultiplePackage("Assets/test.txt", new string[] { "Assets/gapps.txt", "Assets/logcat.bin" }, false);
                Assert.Equal(9, adbClient.ReceivedCommands.Count);
                Assert.Equal("pm install-create", adbClient.ReceivedCommands[1]);
                Assert.Equal("pm install-write 936013062 base.apk \"/data/local/tmp/test.txt\"", adbClient.ReceivedCommands[2]);
                Assert.Equal("pm install-write 936013062 splitapp0.apk \"/data/local/tmp/gapps.txt\"", adbClient.ReceivedCommands[3]);
                Assert.Equal("pm install-write 936013062 splitapp1.apk \"/data/local/tmp/logcat.bin\"", adbClient.ReceivedCommands[4]);
                Assert.Equal("pm install-commit 936013062", adbClient.ReceivedCommands[5]);
                Assert.Equal("rm \"/data/local/tmp/gapps.txt\"", adbClient.ReceivedCommands[6]);
                Assert.Equal("rm \"/data/local/tmp/logcat.bin\"", adbClient.ReceivedCommands[7]);
                Assert.Equal("rm \"/data/local/tmp/test.txt\"", adbClient.ReceivedCommands[8]);

                Assert.Equal(3, syncService.UploadedFiles.Count);
                Assert.True(syncService.UploadedFiles.ContainsKey("/data/local/tmp/test.txt"));
                Assert.True(syncService.UploadedFiles.ContainsKey("/data/local/tmp/gapps.txt"));
                Assert.True(syncService.UploadedFiles.ContainsKey("/data/local/tmp/logcat.bin"));

                Factories.Reset();
            }
        }

        [Fact]
        public void UninstallPackageTest()
        {
            DeviceData device = new()
            {
                State = DeviceState.Online
            };

            DummyAdbClient client = new();
            client.Commands.Add("pm list packages -f", "package:/system/app/Gallery2/Gallery2.apk=com.android.gallery3d");
            client.Commands.Add("pm uninstall com.android.gallery3d", "Success");
            PackageManager manager = new(client, device);

            // Command should execute correctly; if the wrong command is passed an exception
            // would be thrown.
            manager.UninstallPackage("com.android.gallery3d");
        }

        [Fact]
        public void GetPackageVersionInfoTest()
        {
            DeviceData device = new()
            {
                State = DeviceState.Online
            };

            DummyAdbClient client = new();
            client.Commands.Add("dumpsys package com.google.android.gms", File.ReadAllText("Assets/gapps.txt"));
            PackageManager manager = new(client, device, skipInit: true);

            VersionInfo versionInfo = manager.GetVersionInfo("com.google.android.gms");
            Assert.Equal(11062448, versionInfo.VersionCode);
            Assert.Equal("11.0.62 (448-160311229)", versionInfo.VersionName);
        }
    }
}
