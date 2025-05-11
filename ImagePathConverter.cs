using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.Globalization;
using System.IO;

namespace PMMOEdit
{
    public class ImagePathConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not string path || string.IsNullOrEmpty(path))
                return null;

            try
            {
                // Handle both file system paths and resource paths
                if (path.StartsWith("pmmo:") || path.StartsWith("minecraft:"))
                {
                    // For Minecraft resource paths, return a placeholder or null
                    // In a real app, you might have a mapping of resource paths to embedded resources
                    return null;
                }
                else if (File.Exists(path))
                {
                    // For file system paths, load the image
                    return new Bitmap(path);
                }
                
                return null;
            }
            catch
            {
                // Return null if image loading fails
                return null;
            }
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // This converter doesn't support conversion back
            return null;
        }
    }
}
