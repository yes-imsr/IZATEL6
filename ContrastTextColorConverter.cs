using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace PMMOEdit
{
    public class ContrastTextColorConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is int colorValue)
            {
                byte r = (byte)((colorValue >> 16) & 0xFF);
                byte g = (byte)((colorValue >> 8) & 0xFF);
                byte b = (byte)(colorValue & 0xFF);
                
                double brightness = (0.299 * r + 0.587 * g + 0.114 * b);
                if (brightness > 160)
                {
                    return Brushes.Black;
                }
            }
            return Brushes.White;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
