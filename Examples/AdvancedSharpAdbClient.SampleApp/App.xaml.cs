using AdvancedSharpAdbClient.SampleApp.Common;
using AdvancedSharpAdbClient.SampleApp.Data;
using AdvancedSharpAdbClient.SampleApp.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.System.Profile;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace AdvancedSharpAdbClient.SampleApp
{
    /// <summary>
    /// 提供特定于应用程序的行为，以补充默认的应用程序类。
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// 初始化单一实例应用程序对象。这是执行的创作代码的第一行，
        /// 已执行，逻辑上等同于 main() 或 WinMain()。
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
            if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 6))
            {
                this.FocusVisualKind = AnalyticsInfo.VersionInfo.DeviceFamily == "Xbox" ? FocusVisualKind.Reveal : FocusVisualKind.HighVisibility;
            }
        }

        public static TEnum GetEnum<TEnum>(string text) where TEnum : struct
        {
            if (!typeof(TEnum).GetTypeInfo().IsEnum)
            {
                throw new InvalidOperationException("Generic parameter 'TEnum' must be an enum.");
            }
            return (TEnum)Enum.Parse(typeof(TEnum), text);
        }

        /// <summary>
        /// 在应用程序由最终用户正常启动时进行调用。
        /// 将在启动应用程序以打开特定文件等情况下使用。
        /// </summary>
        /// <param name="e">有关启动请求和过程的详细信息。</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
            //draw into the title bar
            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;

            await EnsureWindow(e);
        }

        private async Task EnsureWindow(IActivatedEventArgs args)
        {
            // No matter what our destination is, we're going to need control data loaded - let's knock that out now.
            // We'll never need to do this again.
            await ControlInfoDataSource.Instance.GetGroupsAsync();

            Frame rootFrame = GetRootFrame();

            ThemeHelper.Initialize();

            if (args.PreviousExecutionState == ApplicationExecutionState.Terminated
                    || args.PreviousExecutionState == ApplicationExecutionState.Suspended)
            {
                try
                {
                    await SuspensionManager.RestoreAsync();
                }
                catch (SuspensionManagerException)
                {
                    //Something went wrong restoring state.
                    //Assume there is no state and continue
                }

                Window.Current.Activate();

                UpdateNavigationBasedOnSelectedPage(rootFrame);
                return;
            }

            Type targetPageType = typeof(NewControlsPage);
            string targetPageArguments = string.Empty;

            if (args.Kind == ActivationKind.Launch)
            {
                targetPageArguments = ((LaunchActivatedEventArgs)args).Arguments;
            }
            else if (args.Kind == ActivationKind.Protocol)
            {
                Match match;

                string targetId = string.Empty;

                switch (((ProtocolActivatedEventArgs)args).Uri?.AbsoluteUri)
                {
                    case string s when IsMatching(s, "(/*)category/(.*)"):
                        targetId = match.Groups[2]?.ToString();
                        if (targetId == "AllControls")
                        {
                            targetPageType = typeof(AllControlsPage);
                        }
                        else if (targetId == "NewControls")
                        {
                            targetPageType = typeof(NewControlsPage);
                        }
                        else if (ControlInfoDataSource.Instance.Groups.Any(g => g.UniqueId == targetId))
                        {
                            targetPageType = typeof(SectionPage);
                        }
                        break;

                    case string s when IsMatching(s, "(/*)item/(.*)"):
                        targetId = match.Groups[2]?.ToString();
                        if (ControlInfoDataSource.Instance.Groups.Any(g => g.Items.Any(i => i.UniqueId == targetId)))
                        {
                            targetPageType = typeof(ItemPage);
                        }
                        break;
                }

                targetPageArguments = targetId;

                bool IsMatching(string parent, string expression)
                {
                    match = Regex.Match(parent, expression);
                    return match.Success;
                }
            }

            rootFrame.Navigate(targetPageType, targetPageArguments);

            if (targetPageType == typeof(NewControlsPage))
            {
                ((Microsoft.UI.Xaml.Controls.NavigationViewItem)((NavigationRootPage)Window.Current.Content).NavigationView.MenuItems[0]).IsSelected = true;
            }
            else if (targetPageType == typeof(ItemPage))
            {
                NavigationRootPage.Current.EnsureNavigationSelection(targetPageArguments);
            }

            // Ensure the current window is active
            Window.Current.Activate();
        }

        private static void UpdateNavigationBasedOnSelectedPage(Frame rootFrame)
        {
            // Check if we brought back an ItemPage
            if (rootFrame.Content is ItemPage itemPage)
            {
                // We did, so bring the selected item back into view
                string name = itemPage.Item.Title;
                if (Window.Current.Content is NavigationRootPage nav)
                {
                    // Finally brings back into view the correct item.
                    // But first: Update page layout!
                    nav.EnsureItemIsVisibleInNavigation(name);
                }
            }
        }

        private Frame GetRootFrame()
        {
            Frame rootFrame;
            if (!(Window.Current.Content is NavigationRootPage rootPage))
            {
                rootPage = new NavigationRootPage();
                rootFrame = (Frame)rootPage.FindName("rootFrame");
                if (rootFrame == null)
                {
                    throw new Exception("Root frame not found");
                }
                SuspensionManager.RegisterFrame(rootFrame, "AppFrame");
                rootFrame.Language = Windows.Globalization.ApplicationLanguages.Languages[0];
                rootFrame.NavigationFailed += OnNavigationFailed;

                Window.Current.Content = rootPage;
            }
            else
            {
                rootFrame = (Frame)rootPage.FindName("rootFrame");
            }

            return rootFrame;
        }

        /// <summary>
        /// 导航到特定页失败时调用
        /// </summary>
        ///<param name="sender">导航失败的框架</param>
        ///<param name="e">有关导航失败的详细信息</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// 在将要挂起应用程序执行时调用。  在不知道应用程序
        /// 无需知道应用程序会被终止还是会恢复，
        /// 并让内存内容保持不变。
        /// </summary>
        /// <param name="sender">挂起的请求的源。</param>
        /// <param name="e">有关挂起请求的详细信息。</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: 保存应用程序状态并停止任何后台活动
            deferral.Complete();
        }
    }
}
