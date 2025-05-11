using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace PMMOEdit
{
    public class StringStartsWithConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not string strValue || parameter is not string prefix)
                return false;
                
            return strValue.StartsWith(prefix);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    
    public class StringDoesNotStartWithConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not string strValue || parameter is not string prefix)
                return false;
                
            return !strValue.StartsWith(prefix);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
