using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SimpleSudokuDemo.Core
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        // Converts a boolean to a Visibility value
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool booleanValue)
            {
                // Check if the converter should invert the logic
                bool isInverted = parameter is string paramString && bool.TryParse(paramString, out bool invert) && invert;

                // Return Visibility based on boolean and inversion
                if (isInverted)
                {
                    return booleanValue ? Visibility.Collapsed : Visibility.Visible;
                }
                else
                {
                    return booleanValue ? Visibility.Visible : Visibility.Collapsed;
                }
            }

            // If value is not a boolean, return Visibility.Collapsed by default
            return Visibility.Collapsed;
        }

        // Converts back from Visibility to boolean
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                // Check if the converter should invert the logic
                bool isInverted = parameter is string paramString && bool.TryParse(paramString, out bool invert) && invert;

                // Return boolean based on visibility and inversion
                if (isInverted)
                {
                    return visibility == Visibility.Collapsed;
                }
                else
                {
                    return visibility == Visibility.Visible;
                }
            }

            // Default fallback for ConvertBack
            return false;
        }
    }
}
