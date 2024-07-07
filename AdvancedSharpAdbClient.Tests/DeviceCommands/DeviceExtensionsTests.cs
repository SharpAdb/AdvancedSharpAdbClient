using NSubstitute;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace AdvancedSharpAdbClient.DeviceCommands.Tests
{
    /// <summary>
    /// Tests the <see cref="DeviceExtensions"/> class.
    /// </summary>
    public partial class DeviceExtensionsTests
    {
        protected static DeviceData Device { get; } = new()
        {
            Serial = "169.254.109.177:5555",
            State = DeviceState.Online
        };

        [Fact]
        public void ExecuteServerCommandTest()
        {
            const string command = nameof(command);
            static bool predicate(string x) => true;
            IShellOutputReceiver receiver = new FunctionOutputReceiver(predicate);

            IAdbClient client = Substitute.For<IAdbClient>();
            client.When(x => x.ExecuteRemoteCommand(Arg.Any<string>(), Device))
                .Do(x =>
                {
                    Assert.Equal(command, x.ArgAt<string>(0));
                    Assert.Equal(Device, x.ArgAt<DeviceData>(1));
                });
            client.When(x => x.ExecuteRemoteCommand(Arg.Any<string>(), Device, Arg.Any<IShellOutputReceiver>(), Arg.Any<Encoding>()))
                .Do(x =>
                {
                    Assert.Equal(command, x.ArgAt<string>(0));
                    Assert.Equal(Device, x.ArgAt<DeviceData>(1));
                    Assert.Equal(receiver, x.ArgAt<IShellOutputReceiver>(2));
                    Assert.Equal(AdbClient.Encoding, x.ArgAt<Encoding>(3));
                });

            client.ExecuteShellCommand(Device, command);
            client.ExecuteShellCommand(Device, command, receiver);
            client.ExecuteShellCommand(Device, command, predicate);
        }

        /// <summary>
        /// Tests the <see cref="DeviceExtensions.ClearInput(IAdbClient, DeviceData, int)"/> method.
        /// </summary>
        [Fact]
        public void ClearInputTest()
        {
            DummyAdbClient client = new();
            client.Commands["shell:input keyevent KEYCODE_MOVE_END"] = string.Empty;
            client.Commands["shell:input keyevent KEYCODE_DEL KEYCODE_DEL KEYCODE_DEL"] = string.Empty;

            client.ClearInput(Device, 3);

            Assert.Equal(2, client.ReceivedCommands.Count);
            Assert.Equal("shell:input keyevent KEYCODE_MOVE_END", client.ReceivedCommands[0]);
            Assert.Equal("shell:input keyevent KEYCODE_DEL KEYCODE_DEL KEYCODE_DEL", client.ReceivedCommands[1]);
        }

        /// <summary>
        /// Tests the <see cref="DeviceExtensions.ClickBackButton(IAdbClient, DeviceData)"/> method.
        /// </summary>
        [Fact]
        public void ClickBackButtonTest()
        {
            DummyAdbClient client = new();
            client.Commands["shell:input keyevent KEYCODE_BACK"] = string.Empty;

            client.ClickBackButton(Device);

            Assert.Single(client.ReceivedCommands);
            Assert.Equal("shell:input keyevent KEYCODE_BACK", client.ReceivedCommands[0]);
        }

        /// <summary>
        /// Tests the <see cref="DeviceExtensions.ClickHomeButton(IAdbClient, DeviceData)"/> method.
        /// </summary>
        [Fact]
        public void ClickHomeButtonTest()
        {
            DummyAdbClient client = new();
            client.Commands["shell:input keyevent KEYCODE_HOME"] = string.Empty;

            client.ClickHomeButton(Device);

            Assert.Single(client.ReceivedCommands);
            Assert.Equal("shell:input keyevent KEYCODE_HOME", client.ReceivedCommands[0]);
        }

        /// <summary>
        /// Tests the <see cref="DeviceExtensions.Stat(IAdbClient, DeviceData, string)"/> method.
        /// </summary>
        [Fact]
        public void StatTest()
        {
            const string remotePath = "/test";
            FileStatistics stats = new();

            IAdbClient client = Substitute.For<IAdbClient>();
            ISyncService mock = Substitute.For<ISyncService>();
            mock.Stat(Arg.Any<string>())
                .Returns(x =>
                {
                    Assert.Equal(remotePath, x.ArgAt<string>(0));
                    return stats;
                });

            Factories.SyncServiceFactory = (c, d) =>
            {
                Factories.Reset();
                Assert.Equal(d, Device);
                return mock;
            };

            Assert.Equal(stats, client.Stat(Device, remotePath));
        }

        /// <summary>
        /// Tests the <see cref="DeviceExtensions.GetDirectoryListing(IAdbClient, DeviceData, string)"/> method.
        /// </summary>
        [Fact]
        public void GetDirectoryListingTest()
        {
            const string remotePath = "/test";
            IEnumerable<FileStatistics> stats = [new()];

            IAdbClient client = Substitute.For<IAdbClient>();
            ISyncService mock = Substitute.For<ISyncService>();
            mock.GetDirectoryListing(Arg.Any<string>())
                .Returns(x =>
                {
                    Assert.Equal(remotePath, x.ArgAt<string>(0));
                    return stats;
                });

            Factories.SyncServiceFactory = (c, d) =>
            {
                Factories.Reset();
                Assert.Equal(d, Device);
                return mock;
            };

            Assert.Equal(stats, client.GetDirectoryListing(Device, remotePath));
        }

        /// <summary>
        /// Tests the <see cref="DeviceExtensions.GetEnvironmentVariables(IAdbClient, DeviceData)"/> method.
        /// </summary>
        [Fact]
        public void GetEnvironmentVariablesTest()
        {
            DummyAdbClient adbClient = new();

            adbClient.Commands[$"shell:{EnvironmentVariablesReceiver.PrintEnvCommand}"] = "a=b";

            Dictionary<string, string> variables = adbClient.GetEnvironmentVariables(Device);
            Assert.NotNull(variables);
            Assert.Single(variables.Keys);
            Assert.True(variables.ContainsKey("a"));
            Assert.Equal("b", variables["a"]);
        }

        /// <summary>
        /// Tests the <see cref="DeviceExtensions.UninstallPackage(IAdbClient, DeviceData, string, string[])"/> method.
        /// </summary>
        [Fact]
        public void UninstallPackageTests()
        {
            DummyAdbClient adbClient = new();

            adbClient.Commands["shell:pm uninstall com.example"] = "Success";

            adbClient.UninstallPackage(Device, "com.example");

            Assert.Single(adbClient.ReceivedCommands);
            Assert.Equal("shell:pm uninstall com.example", adbClient.ReceivedCommands[0]);
        }

        /// <summary>
        /// Tests the <see cref="DeviceExtensions.GetPackageVersion(IAdbClient, DeviceData, string)"/> method.
        /// </summary>
        [Theory]
        [InlineData(
            """
            Activity Resolver Table:
              Non-Data Actions:
                  com.android.providers.contacts.DUMP_DATABASE:
                    310a0bd8 com.android.providers.contacts/.debug.ContactsDumpActivity

            Receiver Resolver Table:
              Schemes:
                  package:
                    31f30b31 com.android.providers.contacts/.PackageIntentReceiver (4 filters)

            Registered ContentProviders:
              com.android.providers.contacts/.debug.DumpFileProvider:
                Provider{2b000d84 com.android.providers.contacts/.debug.DumpFileProvider}

            ContentProvider Authorities:
              [com.android.voicemail]:
                Provider{316ea633 com.android.providers.contacts/.VoicemailContentProvider}
                  applicationInfo=ApplicationInfo{1327df0 com.android.providers.contacts}

            Key Set Manager:
              [com.android.providers.contacts]
                  Signing KeySets: 3

            Packages:
              Package [com.android.providers.contacts] (3d5205d5):
                versionCode=22 targetSdk=22
                versionName=5.1-eng.buildbot.20151117.204057
                splits=[base]

            Shared users:
              SharedUser [android.uid.shared] (3341dee):
                userId=10002 gids=[3003, 1028, 1015]
                grantedPermissions:
                  android.permission.WRITE_SETTINGS
            """, 22, "5.1-eng.buildbot.20151117.204057", "com.example")]
        [InlineData(
            """
            Activity Resolver Table:
              Schemes:
                  package:
                    423fa100 jp.co.cyberagent.stf/.IconActivity filter 427ae628

              Non-Data Actions:
                  jp.co.cyberagent.stf.ACTION_IDENTIFY:
                    423fa4d8 jp.co.cyberagent.stf/.IdentityActivity filter 427c76a8

            Service Resolver Table:
              Non-Data Actions:
                  jp.co.cyberagent.stf.ACTION_STOP:
                    423fc3d8 jp.co.cyberagent.stf/.Service filter 427e4ca8
                  jp.co.cyberagent.stf.ACTION_START:
                    423fc3d8 jp.co.cyberagent.stf/.Service filter 427e4ca8

            Packages:
              Package [jp.co.cyberagent.stf] (428c8c10):
                userId=10153 gids=[3003, 1015, 1023, 1028]
                sharedUser=null
                pkg=Package{42884220 jp.co.cyberagent.stf}
                codePath=/data/app/jp.co.cyberagent.stf-1.apk
                resourcePath=/data/app/jp.co.cyberagent.stf-1.apk
                nativeLibraryPath=/data/app-lib/jp.co.cyberagent.stf-1
                versionCode=4
                applicationInfo=ApplicationInfo{4287f2e0 jp.co.cyberagent.stf}
                flags=[ HAS_CODE ALLOW_CLEAR_USER_DATA ALLOW_BACKUP ]
                versionName=2.1.0
                dataDir=/data/data/jp.co.cyberagent.stf
                targetSdk=22
                supportsScreens=[small, medium, large, xlarge, resizeable, anyDensity]
                timeStamp=2017-09-08 15:52:21
                firstInstallTime=2017-09-08 15:52:21
                lastUpdateTime=2017-09-08 15:52:21
                signatures=PackageSignatures{419a7e60 [41bb3628]}
                permissionsFixed=true haveGids=true installStatus=1
                pkgFlags=[ HAS_CODE ALLOW_CLEAR_USER_DATA ALLOW_BACKUP ]
                packageOnlyForOwnerUser: false
                componentsOnlyForOwerUser:
                User 0:  installed=true stopped=true notLaunched=true enabled=0
                grantedPermissions:
                  android.permission.READ_EXTERNAL_STORAGE
                  android.permission.READ_PHONE_STATE
                  android.permission.DISABLE_KEYGUARD
                  android.permission.WRITE_EXTERNAL_STORAGE
                  android.permission.INTERNET
                  android.permission.CHANGE_WIFI_STATE
                  android.permission.MANAGE_ACCOUNTS
                  android.permission.ACCESS_WIFI_STATE
                  android.permission.GET_ACCOUNTS
                  android.permission.ACCESS_NETWORK_STATE
                  android.permission.WAKE_LOCK
            mPackagesOnlyForOwnerUser:
              package : com.android.mms
              package : com.android.phone
              package : com.sec.knox.containeragent
            mComponentsOnlyForOwnerUser:
              package : com.android.contacts
                cmp : com.android.contacts.activities.DialtactsActivity

            mEnforceCopyingLibPackages:

            mSkippingApks:

            mSettings.mPackages:
            the number of packages is 223
            mPackages:
            the number of packages is 223
            End!!!!
            """, 4, "2.1.0", "jp.co.cyberagent.stf")]
        [InlineData(
            """
            Activity Resolver Table:
              Schemes:
                  package:
                    de681a8 jp.co.cyberagent.stf/.IconActivity filter 2863eca
                      Action: "jp.co.cyberagent.stf.ACTION_ICON"
                      Category: "android.intent.category.DEFAULT"
                      Scheme: "package"

              Non-Data Actions:
                  jp.co.cyberagent.stf.ACTION_IDENTIFY:
                    69694c1 jp.co.cyberagent.stf/.IdentityActivity filter 30bda35
                      Action: "jp.co.cyberagent.stf.ACTION_IDENTIFY"
                      Category: "android.intent.category.DEFAULT"

            Service Resolver Table:
              Non-Data Actions:
                  jp.co.cyberagent.stf.ACTION_STOP:
                    db65466 jp.co.cyberagent.stf/.Service filter 7c0646c
                      Action: "jp.co.cyberagent.stf.ACTION_START"
                      Action: "jp.co.cyberagent.stf.ACTION_STOP"
                      Category: "android.intent.category.DEFAULT"
                  jp.co.cyberagent.stf.ACTION_START:
                    db65466 jp.co.cyberagent.stf/.Service filter 7c0646c
                      Action: "jp.co.cyberagent.stf.ACTION_START"
                      Action: "jp.co.cyberagent.stf.ACTION_STOP"
                      Category: "android.intent.category.DEFAULT"

            Key Set Manager:
              [jp.co.cyberagent.stf]
                  Signing KeySets: 57

            Packages:
              Package [jp.co.cyberagent.stf] (13d33a7):
                userId=11261
                pkg=Package{6f61054 jp.co.cyberagent.stf}
                codePath=/data/app/jp.co.cyberagent.stf-Q3jXaNJMy6AIVndbPuclbg==
                resourcePath=/data/app/jp.co.cyberagent.stf-Q3jXaNJMy6AIVndbPuclbg==
                legacyNativeLibraryDir=/data/app/jp.co.cyberagent.stf-Q3jXaNJMy6AIVndbPuclbg==/lib
                primaryCpuAbi=null
                secondaryCpuAbi=null
                versionCode=4 minSdk=9 targetSdk=22
                versionName=2.1.0
                splits=[base]
                apkSigningVersion=2
                applicationInfo=ApplicationInfo{4b6bbfd jp.co.cyberagent.stf}
                flags=[ HAS_CODE ALLOW_CLEAR_USER_DATA ALLOW_BACKUP ]
                dataDir=/data/user/0/jp.co.cyberagent.stf
                supportsScreens=[small, medium, large, xlarge, resizeable, anyDensity]
                timeStamp=2017-09-08 22:06:05
                firstInstallTime=2017-09-08 22:06:07
                lastUpdateTime=2017-09-08 22:06:07
                signatures=PackageSignatures{1c350f2 [37b7ecb5]}
                installPermissionsFixed=true installStatus=1
                pkgFlags=[ HAS_CODE ALLOW_CLEAR_USER_DATA ALLOW_BACKUP ]
                requested permissions:
                  android.permission.DISABLE_KEYGUARD
                  android.permission.READ_PHONE_STATE
                  android.permission.WAKE_LOCK
                  android.permission.INTERNET
                  android.permission.ACCESS_NETWORK_STATE
                  android.permission.WRITE_EXTERNAL_STORAGE
                  android.permission.GET_ACCOUNTS
                  android.permission.MANAGE_ACCOUNTS
                  android.permission.CHANGE_WIFI_STATE
                  android.permission.ACCESS_WIFI_STATE
                  android.permission.READ_EXTERNAL_STORAGE
                install permissions:
                  android.permission.MANAGE_ACCOUNTS: granted=true
                  android.permission.INTERNET: granted=true
                  android.permission.READ_EXTERNAL_STORAGE: granted=true
                  android.permission.READ_PHONE_STATE: granted=true
                  android.permission.CHANGE_WIFI_STATE: granted=true
                  android.permission.ACCESS_NETWORK_STATE: granted=true
                  android.permission.DISABLE_KEYGUARD: granted=true
                  android.permission.GET_ACCOUNTS: granted=true
                  android.permission.WRITE_EXTERNAL_STORAGE: granted=true
                  android.permission.ACCESS_WIFI_STATE: granted=true
                  android.permission.WAKE_LOCK: granted=true
                User 0: ceDataInode=409220 installed=true hidden=false suspended=false stopped=true notLaunched=true enabled=0 instant=false
                  gids=[3003]
                  runtime permissions:
                User 10: ceDataInode=0 installed=true hidden=false suspended=false stopped=true notLaunched=true enabled=0 instant=false
                  gids=[3003]
                  runtime permissions:

            Package Changes:
              Sequence number=45
              User 0:
                seq=6, package=com.google.android.gms
                seq=9, package=be.brusselsairport.appyflight
                seq=11, package=com.android.vending
                seq=13, package=app.qrcode
                seq=15, package=com.android.chrome
                seq=16, package=com.google.android.apps.docs
                seq=17, package=com.google.android.inputmethod.latin
                seq=18, package=com.google.android.music
                seq=20, package=com.google.android.apps.walletnfcrel
                seq=21, package=com.google.android.youtube
                seq=22, package=com.google.android.calendar
                seq=44, package=jp.co.cyberagent.stf
              User 10:
                seq=10, package=com.android.vending
                seq=14, package=com.google.android.apps.walletnfcrel
                seq=15, package=com.android.chrome
                seq=16, package=com.google.android.apps.docs
                seq=17, package=com.google.android.inputmethod.latin
                seq=18, package=com.google.android.music
                seq=19, package=com.google.android.youtube
                seq=22, package=com.google.android.calendar
                seq=44, package=jp.co.cyberagent.stf


            Dexopt state:
              [jp.co.cyberagent.stf]
                Instruction Set: arm64
                  path: /data/app/jp.co.cyberagent.stf-Q3jXaNJMy6AIVndbPuclbg==/base.apk
                  status: /data/app/jp.co.cyberagent.stf-Q3jXaNJMy6AIVndbPuclbg==/oat/arm64/base.odex[status=kOatUpToDate, compilati
                  on_filter=quicken]


            Compiler stats:
              [jp.co.cyberagent.stf]
                 base.apk - 1084
            """, 4, "2.1.0", "jp.co.cyberagent.stf")]
        public void GetPackageVersionTest(string command, int versionCode, string versionName, string packageName)
        {
            DummyAdbClient adbClient = new();

            adbClient.Commands[$"shell:dumpsys package {packageName}"] = command;

            VersionInfo version = adbClient.GetPackageVersion(Device, packageName);

            Assert.Equal(versionCode, version.VersionCode);
            Assert.Equal(versionName, version.VersionName);

            Assert.Single(adbClient.ReceivedCommands);
            Assert.Equal($"shell:dumpsys package {packageName}", adbClient.ReceivedCommands[0]);
        }

        /// <summary>
        /// Tests the <see cref="DeviceExtensions.ListProcesses(IAdbClient, DeviceData)"/> method.
        /// </summary>
        [Fact]
        public void ListProcessesTest()
        {
            DummyAdbClient adbClient = new();

            adbClient.Commands["shell:SDK=\"$(/system/bin/getprop ro.build.version.sdk)\"\nif [ $SDK -lt 24 ]; then\n/system/bin/ls /proc/\nelse\n/system/bin/ls -1 /proc/\nfi"] =
                """
                1
                2
                3
                acpi
                asound
                """;
            adbClient.Commands["shell:cat /proc/1/stat /proc/2/stat /proc/3/stat"] =
                """
                1 (init) S 0 0 0 0 -1 1077944576 2680 83280 0 179 0 67 16 39 20 0 1 0 2 17735680 143 18446744073709551615 134512640 135145076 4288071392 4288070744 134658736 0 0 0 65536 18446744071580117077 0 0 17 1 0 0 0 0 0 135152736 135165080 142131200 4288073690 4288073696 4288073696 4288073714 0
                2 (kthreadd) S 0 0 0 0 -1 2129984 0 0 0 0 0 0 0 0 20 0 1 0 2 0 0 18446744073709551615 0 0 0 0 0 0 0 2147483647 0 18446744071579254310 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0
                3 (ksoftirqd/0) S 2 0 0 0 -1 69238848 0 0 0 0 0 23 0 0 20 0 1 0 7 0 0 18446744073709551615 0 0 0 0 0 0 0 2147483647 0 18446744071579284070 0 0 17 0 0 0 0 0 0 0 0 0 0 0 0 0 0
                """;
            adbClient.Commands["shell:cat /proc/1/cmdline /proc/1/stat /proc/2/cmdline /proc/2/stat /proc/3/cmdline /proc/3/stat"] =
                """
                1 (init) S 0 0 0 0 -1 1077944576 2680 83280 0 179 0 67 16 39 20 0 1 0 2 17735680 143 18446744073709551615 134512640 135145076 4288071392 4288070744 134658736 0 0 0 65536 18446744071580117077 0 0 17 1 0 0 0 0 0 135152736 135165080 142131200 4288073690 4288073696 4288073696 4288073714 0

                2 (kthreadd) S 0 0 0 0 -1 2129984 0 0 0 0 0 0 0 0 20 0 1 0 2 0 0 18446744073709551615 0 0 0 0 0 0 0 2147483647 0 18446744071579254310 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0

                3 (ksoftirqd/0) S 2 0 0 0 -1 69238848 0 0 0 0 0 23 0 0 20 0 1 0 7 0 0 18446744073709551615 0 0 0 0 0 0 0 2147483647 0 18446744071579284070 0 0 17 0 0 0 0 0 0 0 0 0 0 0 0 0 0
                """;

            List<AndroidProcess> processes = adbClient.ListProcesses(Device);

            Assert.Equal(3, processes.Count);
            Assert.Equal("init", processes[0].Name);
            Assert.Equal("kthreadd", processes[1].Name);
            Assert.Equal("ksoftirqd/0", processes[2].Name);
        }
    }
}
