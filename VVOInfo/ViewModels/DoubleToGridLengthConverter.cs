using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data.Converters;

namespace VVOInfo.ViewModels
{
    public class DoubleToGridLengthConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is double width)
            {
                // Convert to absolute pixel value
                return new GridLength(width, GridUnitType.Pixel);
            }

            return GridLength.Auto;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is GridLength gridLength)
            {
                return gridLength.Value;
            }
            return 0.0;
        }
    }
}
