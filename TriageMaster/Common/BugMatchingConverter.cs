using System;
using System.Windows.Data;
using System.Windows.Media;

namespace TriageMaster.Common
{
    public class BugMatchingConverter :IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int matchingPercentage = System.Convert.ToInt32(value);

            if (matchingPercentage >= Helper.WeakMatch && matchingPercentage < Helper.AverageMatch)
            {
                return Brushes.LightPink;
            }
            else if (matchingPercentage >= Helper.AverageMatch && matchingPercentage < Helper.StrongMatch)
            {
                return Brushes.HotPink;
            }
            else
            {
                return Brushes.DeepPink;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
