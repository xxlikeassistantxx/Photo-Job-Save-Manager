using System;
using System.Collections;
using Microsoft.Maui.Controls;

namespace Photo_Job_Save_Manager.Converters
{
    public class NullOrEmptyToTemplateConverter : IValueConverter
    {
        // ConverterParameter: "PickerTemplate,EntryTemplate"
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (parameter is not string paramStr) return null;
            var templates = paramStr.Split(',');
            if (templates.Length != 2) return null;
            var pickerTemplate = Application.Current.MainPage.Resources[templates[0].Trim()] as DataTemplate;
            var entryTemplate = Application.Current.MainPage.Resources[templates[1].Trim()] as DataTemplate;

            bool hasValues = value is IEnumerable enumerable && enumerable.GetEnumerator().MoveNext();
            return hasValues ? pickerTemplate : entryTemplate;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) => throw new NotImplementedException();
    }
} 