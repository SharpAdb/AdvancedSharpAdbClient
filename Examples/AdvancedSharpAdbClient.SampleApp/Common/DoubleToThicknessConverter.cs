using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace AdvancedSharpAdbClient.SampleApp.Common
{
    internal class DoubleToThicknessConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value is double? ? new Thickness((double)value) : false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
