using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using Photo_Job_Save_Manager.Models;
using System.Diagnostics;

namespace Photo_Job_Save_Manager.Converters
{
    public class FieldTypeToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Photo_Job_Save_Manager.Models.JobFieldType fieldType && parameter is string expectedType)
            {
                return fieldType.ToString().Equals(expectedType, StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 