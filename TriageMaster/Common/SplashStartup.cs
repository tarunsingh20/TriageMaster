using System;
using TriageMaster.Windows;

namespace TriageMaster.Common
{
    public static class SplashStartup
    {
        [STAThread]
        public static bool StartSplash()
        {
            bool returnValue = true;
            Splash splashWindow = new Splash();
            splashWindow.ShowDialog();

            if (Helper.SettingsError != Helper.ErrorType.None)
            {
                returnValue = false;
                TFSSettings settingsForm = new TFSSettings("SplashWindow", Helper.SettingsError);
                settingsForm.ShowDialog();
            }

            return returnValue;
        }
    }
}
