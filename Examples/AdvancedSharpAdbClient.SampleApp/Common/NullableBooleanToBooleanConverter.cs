using System;
using Windows.UI.Xaml.Data;

namespace AdvancedSharpAdbClient.SampleApp.Common
{
    public class NullableBooleanToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value is bool? ? (bool)value : (object)false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value is bool ? (bool)value : (object)false;
        }
    }
}
