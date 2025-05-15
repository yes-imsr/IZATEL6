using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace PMMOEdit.Converters
{
    public class PercentageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double decimalValue)
            {
                return decimalValue * 100;
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double percentValue)
            {
                return percentValue / 100;
            }
            return 0;
        }
    }
}
