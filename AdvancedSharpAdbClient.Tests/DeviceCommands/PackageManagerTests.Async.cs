using AdvancedSharpAdbClient.Tests;
using System.IO;
using Xunit;

namespace AdvancedSharpAdbClient.DeviceCommands.Tests
{
    public partial class PackageManagerTests
    {
        [Fact]
        public async void InstallRemotePackageAsyncTest()
        {
            DummyAdbClient adbClient = new();

            adbClient.Commands["pm list packages -f"] = "package:/system/app/Gallery2/Gallery2.apk=com.android.gallery3d";
            adbClient.Commands["pm install \"/data/test.apk\""] = "Success";
            adbClient.Commands["pm install -r \"/data/test.apk\""] = "Success";

            DeviceData device = new()
            {
                State = DeviceState.Online
            };

            PackageManager manager = new(adbClient, device);
            await manager.InstallRemotePackageAsync("/data/test.apk", false);

            Assert.Equal(2, adbClient.ReceivedCommands.Count);
            Assert.Equal("pm install \"/data/test.apk\"", adbClient.ReceivedCommands[1]);

            await manager.InstallRemotePackageAsync("/data/test.apk", true);

            Assert.Equal(3, adbClient.ReceivedCommands.Count);
            Assert.Equal("pm install -r \"/data/test.apk\"", adbClient.ReceivedCommands[2]);
        }

        [Fact]
        public void InstallPackageAsyncTest()
        {
            DummySyncService syncService = new();
            lock (FactoriesTests.locker)
            {
                Factories.SyncServiceFactory = (c, d) => syncService;

                DummyAdbClient adbClient = new();

                adbClient.Commands["pm list packages -f"] = "package:/system/app/Gallery2/Gallery2.apk=com.android.gallery3d";
                adbClient.Commands["pm install \"/data/local/tmp/test.txt\""] = "Success";
                adbClient.Commands["rm \"/data/local/tmp/test.txt\""] = string.Empty;

                DeviceData device = new()
                {
                    State = DeviceState.Online
                };

                PackageManager manager = new(adbClient, device);
                manager.InstallPackageAsync("Assets/test.txt", false).Wait();
                Assert.Equal(3, adbClient.ReceivedCommands.Count);
                Assert.Equal("pm install \"/data/local/tmp/test.txt\"", adbClient.ReceivedCommands[1]);
                Assert.Equal("rm \"/data/local/tmp/test.txt\"", adbClient.ReceivedCommands[2]);

                Assert.Single(syncService.UploadedFiles);
                Assert.True(syncService.UploadedFiles.ContainsKey("/data/local/tmp/test.txt"));

                Factories.Reset();
            }
        }

        [Fact]
        public async void InstallMultipleRemotePackageAsyncTest()
        {
            DummyAdbClient adbClient = new();

            adbClient.Commands["pm list packages -f"] = "package:/system/app/Gallery2/Gallery2.apk=com.android.gallery3d";
            adbClient.Commands["pm install-create"] = "Success: created install session [936013062]";
            adbClient.Commands["pm install-create -r"] = "Success: created install session [936013062]";
            adbClient.Commands["pm install-create -p com.google.android.gms"] = "Success: created install session [936013062]";
            adbClient.Commands["pm install-create -r -p com.google.android.gms"] = "Success: created install session [936013062]";
            adbClient.Commands["pm install-write 936013062 base.apk \"/data/base.apk\""] = "Success";
            adbClient.Commands["pm install-write 936013062 splitapp0.apk \"/data/split-dpi.apk\""] = "Success";
            adbClient.Commands["pm install-write 936013062 splitapp1.apk \"/data/split-abi.apk\""] = "Success";
            adbClient.Commands["pm install-commit 936013062"] = "Success";

            DeviceData device = new()
            {
                State = DeviceState.Online
            };

            PackageManager manager = new(adbClient, device);
            await manager.InstallMultipleRemotePackageAsync("/data/base.apk", new string[] { "/data/split-dpi.apk", "/data/split-abi.apk" }, false);

            Assert.Equal(6, adbClient.ReceivedCommands.Count);
            Assert.Equal("pm install-create", adbClient.ReceivedCommands[1]);
            Assert.Equal("pm install-write 936013062 base.apk \"/data/base.apk\"", adbClient.ReceivedCommands[2]);
            Assert.Equal("pm install-write 936013062 splitapp0.apk \"/data/split-dpi.apk\"", adbClient.ReceivedCommands[3]);
            Assert.Equal("pm install-write 936013062 splitapp1.apk \"/data/split-abi.apk\"", adbClient.ReceivedCommands[4]);
            Assert.Equal("pm install-commit 936013062", adbClient.ReceivedCommands[5]);

            await manager.InstallMultipleRemotePackageAsync("/data/base.apk", new string[] { "/data/split-dpi.apk", "/data/split-abi.apk" }, true);

            Assert.Equal(11, adbClient.ReceivedCommands.Count);
            Assert.Equal("pm install-create -r", adbClient.ReceivedCommands[6]);
            Assert.Equal("pm install-write 936013062 base.apk \"/data/base.apk\"", adbClient.ReceivedCommands[7]);
            Assert.Equal("pm install-write 936013062 splitapp0.apk \"/data/split-dpi.apk\"", adbClient.ReceivedCommands[8]);
            Assert.Equal("pm install-write 936013062 splitapp1.apk \"/data/split-abi.apk\"", adbClient.ReceivedCommands[9]);
            Assert.Equal("pm install-commit 936013062", adbClient.ReceivedCommands[10]);

            await manager.InstallMultipleRemotePackageAsync(new string[] { "/data/split-dpi.apk", "/data/split-abi.apk" }, "com.google.android.gms", false);

            Assert.Equal(15, adbClient.ReceivedCommands.Count);
            Assert.Equal("pm install-create -p com.google.android.gms", adbClient.ReceivedCommands[11]);
            Assert.Equal("pm install-write 936013062 splitapp0.apk \"/data/split-dpi.apk\"", adbClient.ReceivedCommands[12]);
            Assert.Equal("pm install-write 936013062 splitapp1.apk \"/data/split-abi.apk\"", adbClient.ReceivedCommands[13]);
            Assert.Equal("pm install-commit 936013062", adbClient.ReceivedCommands[14]);

            await manager.InstallMultipleRemotePackageAsync(new string[] { "/data/split-dpi.apk", "/data/split-abi.apk" }, "com.google.android.gms", true);

            Assert.Equal(19, adbClient.ReceivedCommands.Count);
            Assert.Equal("pm install-create -r -p com.google.android.gms", adbClient.ReceivedCommands[15]);
            Assert.Equal("pm install-write 936013062 splitapp0.apk \"/data/split-dpi.apk\"", adbClient.ReceivedCommands[16]);
            Assert.Equal("pm install-write 936013062 splitapp1.apk \"/data/split-abi.apk\"", adbClient.ReceivedCommands[17]);
            Assert.Equal("pm install-commit 936013062", adbClient.ReceivedCommands[18]);
        }

        [Fact]
        public void InstallMultiplePackageAsyncTest()
        {
            DummySyncService syncService = new();
            lock (FactoriesTests.locker)
            {
                Factories.SyncServiceFactory = (c, d) => syncService;

                DummyAdbClient adbClient = new();

                adbClient.Commands["pm list packages -f"] = "package:/system/app/Gallery2/Gallery2.apk=com.android.gallery3d";
                adbClient.Commands["pm install-create"] = "Success: created install session [936013062]";
                adbClient.Commands["pm install-create -p com.google.android.gms"] = "Success: created install session [936013062]";
                adbClient.Commands["pm install-write 936013062 base.apk \"/data/local/tmp/test.txt\""] = "Success";
                adbClient.Commands["pm install-write 936013062 splitapp0.apk \"/data/local/tmp/gapps.txt\""] = "Success";
                adbClient.Commands["pm install-write 936013062 splitapp1.apk \"/data/local/tmp/logcat.bin\""] = "Success";
                adbClient.Commands["pm install-commit 936013062"] = "Success";
                adbClient.Commands["rm \"/data/local/tmp/test.txt\""] = string.Empty;
                adbClient.Commands["rm \"/data/local/tmp/gapps.txt\""] = string.Empty;
                adbClient.Commands["rm \"/data/local/tmp/logcat.bin\""] = string.Empty;

                DeviceData device = new()
                {
                    State = DeviceState.Online
                };

                PackageManager manager = new(adbClient, device);
                manager.InstallMultiplePackageAsync("Assets/test.txt", new string[] { "Assets/gapps.txt", "Assets/logcat.bin" }, false).Wait();
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

                syncService.UploadedFiles.Clear();
                manager.InstallMultiplePackageAsync(new string[] { "Assets/gapps.txt", "Assets/logcat.bin" }, "com.google.android.gms", false).Wait();
                Assert.Equal(15, adbClient.ReceivedCommands.Count);
                Assert.Equal("pm install-create -p com.google.android.gms", adbClient.ReceivedCommands[9]);
                Assert.Equal("pm install-write 936013062 splitapp0.apk \"/data/local/tmp/gapps.txt\"", adbClient.ReceivedCommands[10]);
                Assert.Equal("pm install-write 936013062 splitapp1.apk \"/data/local/tmp/logcat.bin\"", adbClient.ReceivedCommands[11]);
                Assert.Equal("pm install-commit 936013062", adbClient.ReceivedCommands[12]);
                Assert.Equal("rm \"/data/local/tmp/gapps.txt\"", adbClient.ReceivedCommands[6]);
                Assert.Equal("rm \"/data/local/tmp/logcat.bin\"", adbClient.ReceivedCommands[7]);

                Assert.Equal(2, syncService.UploadedFiles.Count);
                Assert.True(syncService.UploadedFiles.ContainsKey("/data/local/tmp/gapps.txt"));
                Assert.True(syncService.UploadedFiles.ContainsKey("/data/local/tmp/logcat.bin"));

                Factories.Reset();
            }
        }

        [Fact]
        public async void UninstallPackageAsyncTest()
        {
            DeviceData device = new()
            {
                State = DeviceState.Online
            };

            DummyAdbClient client = new();
            client.Commands["pm list packages -f"] = "package:/system/app/Gallery2/Gallery2.apk=com.android.gallery3d";
            client.Commands["pm uninstall com.android.gallery3d"] = "Success";
            PackageManager manager = new(client, device);

            // Command should execute correctly; if the wrong command is passed an exception
            // would be thrown.
            await manager.UninstallPackageAsync("com.android.gallery3d");
        }

        [Fact]
        public async void GetPackageVersionInfoAsyncTest()
        {
            DeviceData device = new()
            {
                State = DeviceState.Online
            };

            DummyAdbClient client = new();
            client.Commands["dumpsys package com.google.android.gms"] = File.ReadAllText("Assets/gapps.txt");
            PackageManager manager = new(client, device, skipInit: true);

            VersionInfo versionInfo = await manager.GetVersionInfoAsync("com.google.android.gms");
            Assert.Equal(11062448, versionInfo.VersionCode);
            Assert.Equal("11.0.62 (448-160311229)", versionInfo.VersionName);
        }
    }
}
