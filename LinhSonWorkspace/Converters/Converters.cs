using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace LinhSonWorkspace.Converters
{
    /// <summary>
    /// Converts booking/workspace status to a colored SolidColorBrush for display.
    /// </summary>
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var status = value?.ToString() ?? "";

            return status switch
            {
                "Available" => new SolidColorBrush(Color.FromRgb(16, 185, 129)),   // Emerald
                "Maintenance" => new SolidColorBrush(Color.FromRgb(245, 158, 11)), // Amber
                "Inactive" => new SolidColorBrush(Color.FromRgb(107, 114, 128)),   // Gray

                "Pending" => new SolidColorBrush(Color.FromRgb(245, 158, 11)),     // Amber
                "Confirmed" => new SolidColorBrush(Color.FromRgb(59, 130, 246)),   // Blue
                "CheckedIn" => new SolidColorBrush(Color.FromRgb(139, 92, 246)),   // Violet
                "Completed" => new SolidColorBrush(Color.FromRgb(16, 185, 129)),   // Emerald
                "Cancelled" => new SolidColorBrush(Color.FromRgb(239, 68, 68)),    // Red

                _ => new SolidColorBrush(Color.FromRgb(107, 114, 128))             // Default gray
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts boolean to Visibility (true = Visible, false = Collapsed).
    /// </summary>
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                // If parameter is "Inverse", reverse the logic
                if (parameter?.ToString() == "Inverse")
                    return boolValue ? Visibility.Collapsed : Visibility.Visible;

                return boolValue ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Formats decimal currency values to Vietnamese format.
    /// </summary>
    public class CurrencyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal amount)
            {
                return amount.ToString("N0") + " VNĐ";
            }
            return "0 VNĐ";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
