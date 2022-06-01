using AdvancedSharpAdbClient.SampleApp.Data;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace AdvancedSharpAdbClient.SampleApp.Common
{
    public class ImageLoader
    {
        public static string GetSource(DependencyObject obj)
        {
            return (string)obj.GetValue(SourceProperty);
        }

        public static void SetSource(DependencyObject obj, string value)
        {
            obj.SetValue(SourceProperty, value);
        }

        // Using a DependencyProperty as the backing store for Path.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.RegisterAttached("Source", typeof(string), typeof(ImageLoader), new PropertyMetadata(string.Empty, OnPropertyChanged));

        private static async void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Image image)
            {
                ControlInfoDataItem item = await ControlInfoDataSource.Instance.GetItemAsync(e.NewValue?.ToString());
                if (item?.ImageIconPath != null)
                {
                    Uri imageUri = new Uri(item.ImageIconPath, UriKind.Absolute);
                    BitmapImage imageBitmap = new BitmapImage(imageUri);
                    image.Source = imageBitmap;
                }
            }
        }
    }
}
