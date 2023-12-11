using Xunit;

namespace AdvancedSharpAdbClient.DeviceCommands.Receivers.Tests
{
    /// <summary>
    /// Tests the <see cref="ProcessOutputReceiver"/> class.
    /// </summary>
    public class ProcessOutputReceiverTests
    {
        [Fact]
        public void ProcessOutputReceiverTest()
        {
            ProcessOutputReceiver receiver = new();
            receiver.AddOutput("/init\0--second-stage\01 (init) S 0 0 0 0 -1 1077944576 5343 923316 0 1221 2 48 357 246 20 0 1 0 3 24002560 201 18446744073709551615 134512640 135874648 4293296896 4293296356 135412421 0 0 0 65536 18446744071580341721 0 0 17 3 0 0 0 0 0 135882176 135902816 152379392 4293300171 4293300192 4293300192 4293300210 0");
            receiver.AddOutput("10 (rcu_sched) S 2 0 0 0 -1 2129984 0 0 0 0 0 0 0 0 20 0 1 0 9 0 0 18446744073709551615 0 0 0 0 0 0 0 2147483647 0 18446744071579565281 0 0 17 0 0 0 0 0 0 0 0 0 0 0 0 0 0");
            receiver.AddOutput("be.xx.yy.android.test\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\04212 (le.android.test) S 2088 2088 0 0 -1 1077944640 10251 1315 2 0 10 8 0 1 20 0 10 0 15838 1062567936 12163 18446744073709551615 4152340480 4152354824 4289177024 4289174228 4147921093 0 4612 0 38136 18446744073709551615 0 0 17 1 0 0 0 0 0 4152360256 4152360952 4157476864 4289182806 4289182882 4289182882 4289183712 0");
            receiver.Flush();

            Assert.Equal(3, receiver.Processes.Count);
            Assert.Equal("/init", receiver.Processes[0].Name);
            Assert.Equal("rcu_sched", receiver.Processes[1].Name);
            Assert.Equal("be.xx.yy.android.test", receiver.Processes[2].Name);
        }
    }
}
