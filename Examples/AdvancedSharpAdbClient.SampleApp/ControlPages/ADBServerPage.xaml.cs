using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace AdvancedSharpAdbClient.SampleApp.ControlPages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class ADBServerPage : Page
    {
        public ADBServerPage()
        {
            this.InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            AdbServerStatus status = new AdbServer().GetStatus();
            Control1Output.Text = $"Version: {status.Version}";
            Control2Output.Text = $"IsRunning: {status.IsRunning}";
            Control1Output.Visibility = status.IsRunning ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
