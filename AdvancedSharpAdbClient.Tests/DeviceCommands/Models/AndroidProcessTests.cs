//-----------------------------------------------------------------------
// <copyright file="AndroidProcessTests.cs" company="Quamotion">
//     Copyright (c) 2015 Quamotion. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System;
using Xunit;

namespace AdvancedSharpAdbClient.DeviceCommands.Models.Tests
{
    /// <summary>
    /// Tests the <see cref="AndroidProcess"/> class.
    /// </summary>
    public class AndroidProcessTests
    {
        /// <summary>
        /// Tests the <see cref="AndroidProcess.Parse(string, bool)"/> method.
        /// </summary>
        [Fact]
        public void ParseTest()
        {
            string line = "1 (init) S 0 0 0 0 -1 1077936384 1467 168323 0 38 12 141 863 249 20 0 1 0 4 2535424 245 4294967295 1 1 0 0 0 0 0 0 65536 4294967295 0 0 17 3 0 0 0 0 0 0 0 0 0 0 0 0 0";

            AndroidProcess process = AndroidProcess.Parse(line);

            Assert.Equal(1, process.ProcessId);
            Assert.Equal(0, process.ParentProcessId);
            Assert.Equal(2535424ul, process.VirtualSize);
            Assert.Equal(245, process.ResidentSetSize);
            Assert.Equal(4294967295ul, process.WChan);
            Assert.Equal(AndroidProcessState.S, process.State);
            Assert.Equal("init", process.Name);
        }

        [Fact]
        public void ParseLongTest()
        {
            AndroidProcess p = AndroidProcess.Parse("10 (rcu_sched) S 2 0 0 0 -1 2129984 0 0 0 0 0 0 0 0 20 0 1 0 9 0 0 18446744073709551615 0 0 0 0 0 0 0 2147483647 0 18446744071579565281 0 0 17 0 0 0 0 0 0 0 0 0 0 0 0 0 0", true);
            Assert.Equal("rcu_sched", p.Name);
        }

        /// <summary>
        /// Tests the parsing of a process where the cmd line output is empty.
        /// </summary>
        [Fact]
        public void ParseWithEmptyTest()
        {
            AndroidProcess p = AndroidProcess.Parse("be.xx.yy.android.test\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\04212 (le.android.test) S 2088 2088 0 0 -1 1077944640 10251 1315 2 0 10 8 0 1 20 0 10 0 15838 1062567936 12163 18446744073709551615 4152340480 4152354824 4289177024 4289174228 4147921093 0 4612 0 38136 18446744073709551615 0 0 17 1 0 0 0 0 0 4152360256 4152360952 4157476864 4289182806 4289182882 4289182882 4289183712 0", true);
            Assert.Equal("be.xx.yy.android.test", p.Name);
        }

        [Fact]
        public void ParseNullTest() =>
            _ = Assert.Throws<ArgumentNullException>(() => AndroidProcess.Parse(null));

        [Fact]
        public void ParseTooFewPartsTest() =>
            _ = Assert.Throws<ArgumentOutOfRangeException>(() => AndroidProcess.Parse("1 (init) S 0 0 0 0 -1 1077944576 2680 83280 0 179 0 67 16 39 20 0 1 0 2 17735680 143 18446744073709551615 134512640 135145076 "));

        [Fact]
        public void ParseWithSpaceTest()
        {
            string line = "194(irq/432-mdm sta) S 2 0 0 0 - 1 2130240 0 0 0 0 0 1 0 0 - 51 0 1 0 172 0 0 4294967295 0 0 0 0 0 0 0 2147483647 0 4294967295 0 0 17 1 50 1 0 0 0 0 0 0 0 0 0 0 0";

            AndroidProcess process = AndroidProcess.Parse(line);

            Assert.Equal(194, process.ProcessId);
            Assert.Equal(2, process.ParentProcessId);
            Assert.Equal(0ul, process.VirtualSize);
            Assert.Equal(172, process.ResidentSetSize);
            Assert.Equal(2147483647ul, process.WChan);
            Assert.Equal(AndroidProcessState.S, process.State);
            Assert.Equal("irq/432-mdm sta", process.Name);
        }

        [Theory]
        [InlineData("/init\01 (init) S 0 0 0 0 -1 1077952768 32422 176091 2066 1357 2116 957 1341 886 20 0 1 0 0 48603136 335 18446744073709551615 1 1 0 0 0 0 0 0 66560 0 0 0 17 3 0 0 34 0 0 0 0 0 0 0 0 0 0")]
        [InlineData("10 (migration/0) S 2 0 0 0 -1 69238848 0 0 0 0 0 2651 0 0 -100 0 1 0 2 0 0 18446744073709551615 0 0 0 0 0 0 0 2147483647 0 0 0 0 17 0 99 1 0 0 0 0 0 0 0 0 0 0 0")]
        [InlineData("1054 (fsnotify_mark) S 2 0 0 0 -1 2129984 0 0 0 0 0 0 0 0 20 0 1 0 71 0 0 18446744073709551615 0 0 0 0 0 0 0 2147483647 0 0 0 0 17 5 0 0 0 0 0 0 0 0 0 0 0 0 0")]
        [InlineData("1075 (ecryptfs-kthrea) S 2 0 0 0 -1 2097216 0 0 0 0 2 23 0 0 20 0 1 0 71 0 0 18446744073709551615 0 0 0 0 0 0 0 2147483647 0 0 0 0 17 5 0 0 0 0 0 0 0 0 0 0 0 0 0")]
        [InlineData("11 (watchdog/0) S 2 0 0 0 -1 69239104 0 0 0 0 23 0 0 0 -100 0 1 0 4 0 0 18446744073709551615 0 0 0 0 0 0 0 2147483647 0 0 0 0 17 0 99 1 0 0 0 0 0 0 0 0 0 0 0")]
        [InlineData("1154 (pcie_wq) S 2 0 0 0 -1 69238880 0 0 0 0 0 0 0 0 0 -20 1 0 75 0 0 18446744073709551615 0 0 0 0 0 0 0 2147483647 0 0 0 0 17 4 0 0 0 0 0 0 0 0 0 0 0 0 0")]
        [InlineData("11555 (irq/89-10430000) S 2 0 0 0 -1 2129984 0 0 0 0 0 0 0 0 -51 0 1 0 2804616 0 0 18446744073709551615 0 0 0 0 0 0 0 2147483647 0 0 0 0 17 0 50 1 0 0 0 0 0 0 0 0 0 0 0")]
        [InlineData("11556 (irq/90-10830000) S 2 0 0 0 -1 2129984 0 0 0 0 0 20 0 0 -51 0 1 0 2804616 0 0 18446744073709551615 0 0 0 0 0 0 0 2147483647 0 0 0 0 17 0 50 1 0 0 0 0 0 0 0 0 0 0 0")]
        [InlineData("1156 (disp_det) S 2 0 0 0 -1 69238880 0 0 0 0 0 0 0 0 0 -20 1 0 76 0 0 18446744073709551615 0 0 0 0 0 0 0 2147483647 0 0 0 0 17 7 0 0 0 0 0 0 0 0 0 0 0 0 0")]
        [InlineData("12 (watchdog/1) S 2 0 0 0 -1 69239104 0 0 0 0 0 50 0 0 -100 0 1 0 4 0 0 18446744073709551615 0 0 0 0 0 0 0 2147483647 0 0 0 0 17 1 99 1 0 0 0 0 0 0 0 0 0 0 0")]
        [InlineData("com.android.bluetooth\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\012020 (droid.bluetooth) S 3541 3541 0 0 -1 1077952832 233615 0 14408 0 2009 1518 0 0 20 0 61 0 14618 4045262848 29397 18446744073709551615 1 1 0 0 0 0 4612 1 1073775864 0 0 0 17 0 0 0 30 0 0 0 0 0 0 0 0 0 0")]
        [InlineData("com.android.systemui\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\012060 (ndroid.systemui) S 3541 3541 0 0 -1 1077952832 1927788 0 75927 0 30090 13049 0 0 20 0 73 0 14657 5656006656 65946 18446744073709551615 1 1 0 0 0 0 4612 1 1073775864 0 0 0 17 2 0 0 223 0 0 0 0 0 0 0 0 0 0")]
        [InlineData("1210 (hwrng) S 2 0 0 0 -1 2097216 0 0 0 0 0 1310 0 0 20 0 1 0 77 0 0 18446744073709551615 0 0 0 0 0 0 0 2147483647 0 0 0 0 17 2 0 0 0 0 0 0 0 0 0 0 0 0 0")]
        [InlineData("1212 (g3d_dvfs) S 2 0 0 0 -1 69238880 0 0 0 0 0 0 0 0 0 -20 1 0 77 0 0 18446744073709551615 0 0 0 0 0 0 0 2147483647 0 0 0 0 17 6 0 0 0 0 0 0 0 0 0 0 0 0 0")]
        [InlineData("webview_zygote\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\012122 (webview_zygote) S 3542 3541 0 0 -1 4211008 20885 1118065 1825 46464 17 72 21524 8739 20 0 5 0 14690 1686634496 8058 18446744073709551615 1 1 0 0 0 0 4612 1 1073841400 0 0 0 17 4 0 0 1 0 0 0 0 0 0 0 0 0 0")]
        [InlineData("1214 (kbase_job_fault) S 2 0 0 0 -1 69238880 0 0 0 0 0 0 0 0 0 -20 1 0 77 0 0 18446744073709551615 0 0 0 0 0 0 0 2147483647 0 0 0 0 17 6 0 0 0 0 0 0 0 0 0 0 0 0 0")]
        [InlineData("1218 (coagent1) S 2 0 0 0 -1 2129984 0 0 0 0 0 0 0 0 20 0 1 0 78 0 0 18446744073709551615 0 0 0 0 0 0 0 2147483647 0 0 0 0 17 4 0 0 0 0 0 0 0 0 0 0 0 0 0")]
        [InlineData("/vendor/bin/hw/wpa_supplicant\0-O/data/vendor/wifi/wpa/sockets\0-puse_p2p_group_interface=1\0-g@android:wpa_wlan0\012195 (wpa_supplicant) S 1 12195 0 0 -1 4210944 15790 0 976 0 319 1722 0 0 20 0 1 0 14726 2186141696 1492 18446744073709551615 1 1 0 0 0 0 0 0 1073792251 0 0 0 17 2 0 0 29 0 0 0 0 0 0 0 0 0 0")]
        [InlineData("com.android.systemui:InfinityWallpaper\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\012213 (finityWallpaper) S 3541 3541 0 0 -1 1077952832 245384 0 9865 0 11873 10911 0 0 20 0 26 0 14741 4492955648 31569 18446744073709551615 1 1 0 0 0 0 4612 1 1073775864 0 0 0 17 2 0 0 20 0 0 0 0 0 0 0 0 0 0")]
        [InlineData("1222 (irq/286-muic-ir) S 2 0 0 0 -1 2129984 0 0 0 0 0 8 0 0 -51 0 1 0 78 0 0 18446744073709551615 0 0 0 0 0 0 0 2147483647 0 0 0 0 17 1 50 1 0 0 0 0 0 0 0 0 0 0 0")]
        [InlineData("com.sec.location.nsflp2\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\012240 (location.nsflp2) S 3541 3541 0 0 -1 1077952832 175058 0 11267 0 584 349 0 0 20 0 21 0 14775 3977629696 26643 18446744073709551615 1 1 0 0 0 0 4612 1 1073775864 0 0 0 17 0 0 0 4 0 0 0 0 0 0 0 0 0 0")]
        [InlineData("1226 (bioset) S 2 0 0 0 -1 69238880 0 0 0 0 0 0 0 0 0 -20 1 0 79 0 0 18446744073709551615 0 0 0 0 0 0 0 2147483647 0 0 0 0 17 6 0 0 0 0 0 0 0 0 0 0 0 0 0")]
        [InlineData("1227 (bioset) S 2 0 0 0 -1 69238880 0 0 0 0 0 0 0 0 0 -20 1 0 79 0 0 18446744073709551615 0 0 0 0 0 0 0 2147483647 0 0 0 0 17 6 0 0 0 0 0 0 0 0 0 0 0 0 0")]
        [InlineData("1228 (bioset) S 2 0 0 0 -1 69238880 0 0 0 0 0 0 0 0 0 -20 1 0 79 0 0 18446744073709551615 0 0 0 0 0 0 0 2147483647 0 0 0 0 17 6 0 0 0 0 0 0 0 0 0 0 0 0 0")]
        [InlineData("1229 (bioset) S 2 0 0 0 -1 69238880 0 0 0 0 0 0 0 0 0 -20 1 0 79 0 0 18446744073709551615 0 0 0 0 0 0 0 2147483647 0 0 0 0 17 6 0 0 0 0 0 0 0 0 0 0 0 0 0")]
        [InlineData("1230 (bioset) S 2 0 0 0 -1 69238880 0 0 0 0 0 0 0 0 0 -20 1 0 79 0 0 18446744073709551615 0 0 0 0 0 0 0 2147483647 0 0 0 0 17 6 0 0 0 0 0 0 0 0 0 0 0 0 0")]
        [InlineData("1869 (irq/306-(null)) S 2 0 0 0 -1 2129984 0 0 0 0 0 0 0 0 -51 0 1 0 116 0 0 18446744073709551615 0 0 0 0 0 0 0 2147483647 0 0 0 0 17 0 50 1 0 0 0 0 0 0 0 0 0 0 0")]

        public void ParseTests(string line) => _ = AndroidProcess.Parse(line, true);

        [Fact]
        public void ToStringTest()
        {
            AndroidProcess p = new()
            {
                ProcessId = 1,
                Name = "init"
            };

            Assert.Equal("init (1)", p.ToString());
        }
    }
}
