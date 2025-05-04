using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WPF_personnel_app
{
    public partial class App : Application
    {
        // Application-level logic can be added here if needed.
    }

    /// <summary>
    /// Converts null values to Visibility.Collapsed or Visibility.Visible.
    /// Use the "inverse" parameter to invert the behavior.
    /// </summary>
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isInverse = parameter?.ToString() == "inverse";
            bool isNull = value == null;

            return isInverse ? (isNull ? Visibility.Visible : Visibility.Collapsed)
                : (isNull ? Visibility.Collapsed : Visibility.Visible);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("ConvertBack is not implemented for NullToVisibilityConverter.");
        }
    }

    /// <summary>
    /// Enables or disables a control based on the status string.
    /// </summary>
    public class StatusToEnabledConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string status = value as string;
            string expectedStatus = parameter as string;

            return status == expectedStatus;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("ConvertBack is not implemented for StatusToEnabledConverter.");
        }
    }
}