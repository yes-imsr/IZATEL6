using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace PMMOEdit
{
    public class IntToColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is int intColor)
            {
                byte r = (byte)((intColor >> 16) & 0xFF);
                byte g = (byte)((intColor >> 8) & 0xFF);
                byte b = (byte)(intColor & 0xFF);
                return Color.FromRgb(r, g, b);
            }
            return Colors.White;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is Color color)
            {
                return (color.R << 16) | (color.G << 8) | color.B;
            }
            return 16777215; // White
        }
    }
}