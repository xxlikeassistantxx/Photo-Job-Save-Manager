using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace Photo_Job_Save_Manager.Converters
{
    public class NullToBoolConverter : IValueConverter
    {
        public static readonly NullToBoolConverter Instance = new NullToBoolConverter();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value != null;
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
} 