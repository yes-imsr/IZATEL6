using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace PMMOEdit.Converters
{
    /// <summary>
    /// Converts decimal values (0-1) to percentage display values (0-100) and back.
    /// Used with the FormatString attribute in XAML, like FormatString="F0 '%'" where:
    /// - "F" is the standard .NET format specifier for fixed-point notation
    /// - The number after F specifies decimal places (F0 = no decimals, F1 = one decimal)
    /// - '%' is appended as a literal character
    /// </summary>
    public class PercentageConverter : IValueConverter
    {
        /// <summary>
        /// Converts a decimal value (0-1) to percentage (0-100) for display
        /// </summary>
        /// <param name="value">The source value (typically 0-1)</param>
        /// <param name="targetType">The target type</param>
        /// <param name="parameter">Optional parameter</param>
        /// <param name="culture">The culture info</param>
        /// <returns>The value multiplied by 100 for percentage display</returns>
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is double doubleValue)
            {
                return doubleValue * 100;
            }
            else if (value is float floatValue)
            {
                return floatValue * 100;
            }
            else if (value is decimal decimalValue)
            {
                return (double)decimalValue * 100;
            }
            
            return 0;
        }

        /// <summary>
        /// Converts a percentage (0-100) back to decimal value (0-1) for storage
        /// </summary>
        /// <param name="value">The display value (typically 0-100)</param>
        /// <param name="targetType">The target type</param>
        /// <param name="parameter">Optional parameter</param>
        /// <param name="culture">The culture info</param>
        /// <returns>The value divided by 100 to get the decimal representation</returns>
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is double doubleValue)
            {
                return doubleValue / 100;
            }
            else if (value is string stringValue && double.TryParse(stringValue, out double parsedValue))
            {
                return parsedValue / 100;
            }
            
            return 0;
        }
    }
}
