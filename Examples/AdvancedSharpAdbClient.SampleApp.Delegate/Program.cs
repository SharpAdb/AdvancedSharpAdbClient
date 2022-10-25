using ProcessForUWP.Desktop.Helpers;
using System.Threading;

Communication.InitializeAppServiceConnection();
EventWaitHandle WaitHandle = new AutoResetEvent(false);
_ = WaitHandle.WaitOne();