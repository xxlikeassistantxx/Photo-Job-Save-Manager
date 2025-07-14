using System;
using Microsoft.Maui.Controls;

namespace Photo_Job_Save_Manager.Converters
{
    public class ZeroOrNullToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return true;
            if (value is int i) return i == 0;
            return false;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) => throw new NotImplementedException();
    }
} 