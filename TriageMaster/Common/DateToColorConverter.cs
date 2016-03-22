using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace TriageMaster.Common
{
    class DateToColorConverter : IValueConverter
    {
        //Applying color to the grid by comparing Todays date with Created Date of Defect/Bug
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {            
            string rowColor = "White";

            try
            {

                //Forcing today's Datetime to Convert in {0:MM/dd/yyyy} format.
                TimeSpan differentInNumberOfDays = System.DateTime.Parse(DateTime.Now.ToString("MM/dd/yyyy"), System.Globalization.CultureInfo.InvariantCulture).Date - System.DateTime.Parse(string.Format("{0:MM/dd/yyyy}", value), System.Globalization.CultureInfo.InvariantCulture).Date;
                if (Helper.TFSConnectionSettings.PreferenceColorName1 == string.Empty)
                {
                    return (SolidColorBrush)new BrushConverter().ConvertFromString(rowColor);
                }

                if (Helper.TFSConnectionSettings.PreferenceColorName2 == string.Empty)
                {
                    return (SolidColorBrush)new BrushConverter().ConvertFromString(rowColor);
                }

                if (Helper.TFSConnectionSettings.PreferenceColorName3 == string.Empty)
                {
                    return (SolidColorBrush)new BrushConverter().ConvertFromString(rowColor);
                }
                

                if (differentInNumberOfDays.Days <= int.Parse( Helper.TFSConnectionSettings.PreferenceInterval1))   //Preference 1. 
                {
                    rowColor = Helper.TFSConnectionSettings.PreferenceColorName1;

                    if (Helper.TFSConnectionSettings.PreferenceColorName1.Contains(" "))
                        rowColor = Helper.TFSConnectionSettings.PreferenceColorName1.Replace(" ", "");

                    return (SolidColorBrush)new BrushConverter().ConvertFromString(rowColor);
                }
                else if (differentInNumberOfDays.Days <= int.Parse(Helper.TFSConnectionSettings.PreferenceInterval2) &&
                    differentInNumberOfDays.Days > int.Parse(Helper.TFSConnectionSettings.PreferenceInterval1))   //Preference 2. 
                {
                    rowColor = Helper.TFSConnectionSettings.PreferenceColorName2;

                    if (Helper.TFSConnectionSettings.PreferenceColorName2.Contains(" "))
                        rowColor = Helper.TFSConnectionSettings.PreferenceColorName2.Replace(" ", "");

                    return (SolidColorBrush)new BrushConverter().ConvertFromString(rowColor);
                }
                else
                {
                    rowColor = Helper.TFSConnectionSettings.PreferenceColorName3;

                    if (Helper.TFSConnectionSettings.PreferenceColorName3.Contains(" "))
                        rowColor = Helper.TFSConnectionSettings.PreferenceColorName3.Replace(" ", "");

                    return (SolidColorBrush)new BrushConverter().ConvertFromString(rowColor);
                }
               
            }
            catch (Exception ex)
            {                
                Log.CaptureException(ex);
                Log.LogFileWrite();
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
