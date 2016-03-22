using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using STaRZ.CryptoLibrary;
using STaRZ.TFSLibrary;

namespace TriageMaster.Common
{
    public class Helper
    {
        #region TFSQueries

        static List<TFSQueryItem> tfsQueries;

        public static List<TFSQueryItem> TfsQueries
        {
            get
            {
                if (tfsQueries == null && !string.IsNullOrEmpty(TFSConnectionSettings.DefectWorkItemType))
                {
                    tfsQueries = TfsWrapper.LoadQueries(TFSConnectionSettings.DefectWorkItemType);
                }

                return tfsQueries;
            }
            set
            {
                tfsQueries = value;
            }
        }

        #endregion

        #region [TFSWrapper]

        static TFSWrapper tfsWrapper;

        public static TFSWrapper TfsWrapper
        {
            get
            {
                if (tfsWrapper == null)
                {
                    InitializeTfsWrapper();
                }

                return tfsWrapper;
            }
            set
            {
                tfsWrapper = value;
            }
        }

        public static void InitializeTfsWrapper()
        {
            tfsWrapper = new TFSWrapper(new Uri(Helper.TFSConnectionSettings.TfsServer),
                                        new System.Net.NetworkCredential(Helper.TFSConnectionSettings.UserName, Helper.TFSConnectionSettings.Password, Helper.TFSConnectionSettings.Domain), Helper.TFSConnectionSettings.TfsProject);

        }

        #endregion

        #region [Enumerations]

        public enum ErrorType
        {
            None,
            TFSWrongUriError,
            TFSWrapperError,
            TFSStoreError,
            TFSProjectError
        }

        public enum ThemeName
        {
            ShinyRed,
            ShinyBlue
        }

        public enum TreeViewIterateMode
        {
            AreaPath,
            IterationPath
        }

        public enum TFSFieldType
        {
            AreaPath,
            IterationPath,
            AssignedTo,
            Priority,
            Severity,
            State,
            RootCause
        }

        #endregion

        #region [Constants]

        public const int WeakMatch = 55;
        public const int AverageMatch = 70;
        public const int StrongMatch = 90;

        public const int ClearMessageDisplay = 250;

        private const string ColorIndex1 = "3";
        private const string ColorIndex2 = "84";
        private const string ColorIndex3 = "132";

        private const string Interval1 = "2";
        private const string Interval2 = "5";

        private const string ColorName1 = "White";
        private const string ColorName2 = "Light Pink";
        private const string ColorName3 = "Salmon";

        private const string DefaultTheme = "ShinyBlue";

        public const string LOGFILENAME = "TriageMasterErrorLog.txt";
        public const string LOG_WRITTEN = "Log has been written at User's temp folder";
        public const string ERROR_INFO = "Error info";
        public const string TRIAGE_COMMENT = "[Triage Comment]: {0}";

        #endregion

        #region [Options]

        static ErrorType settingsError;
        public static ErrorType SettingsError
        {
            get { return settingsError; }
            set { settingsError = value; }
        }

        static TFSConnectionSettings tfsConSettings;
        public static TFSConnectionSettings TFSConnectionSettings
        {
            get
            {
                if (tfsConSettings == null)
                {
                    GetTFSConnectionSettingsFromConfiguration();
                }
                return tfsConSettings;
            }
            set
            {
                tfsConSettings = value;
            }
        }

        #endregion

        #region [Public Methods]

        #region [TFS Setting Details]

        public static void GetTFSConnectionSettingsFromConfiguration()
        {
            Configuration settingConfigurations = ConfigurationManager.OpenExeConfiguration(Assembly.GetExecutingAssembly().Location);
            AppSettingsSection appSettingsSection = settingConfigurations.AppSettings;
            TFSConnectionSettings = new TFSConnectionSettings();

            if (appSettingsSection.Settings.AllKeys.Contains("UserName"))
            {
                TFSConnectionSettings.UserName = appSettingsSection.Settings["UserName"].Value;
            }

            if (appSettingsSection.Settings.AllKeys.Contains("Domain"))
            {
                TFSConnectionSettings.Domain = appSettingsSection.Settings["Domain"].Value;
            }
            else
            {
                TFSConnectionSettings.Domain = MessageNotification.DOMAIN;
            }

            if (appSettingsSection.Settings.AllKeys.Contains("Password"))
            {
                TFSConnectionSettings.Password = CryptoLibrary.ToInsecureString(CryptoLibrary.DecryptString(appSettingsSection.Settings["Password"].Value));
            }

            if (appSettingsSection.Settings.AllKeys.Contains("TFSServer"))
            {
                TFSConnectionSettings.TfsServer = appSettingsSection.Settings["TFSServer"].Value;
            }


            if (appSettingsSection.Settings.AllKeys.Contains("TFSProject"))
            {
                TFSConnectionSettings.TfsProject = appSettingsSection.Settings["TFSProject"].Value;
            }

            if (appSettingsSection.Settings.AllKeys.Contains("DefectWorkItemType"))
            {
                TFSConnectionSettings.DefectWorkItemType = appSettingsSection.Settings["DefectWorkItemType"].Value;
            }

            if (appSettingsSection.Settings.AllKeys.Contains("SaveUserNameAndPassword"))
            {
                TFSConnectionSettings.SaveUserNameAndPassword = (appSettingsSection.Settings["SaveUserNameAndPassword"].Value.ToLower() == "true");
            }
            else
            {
                TFSConnectionSettings.SaveUserNameAndPassword = false;
            }

            if (appSettingsSection.Settings.AllKeys.Contains("AutoConnect"))
            {
                TFSConnectionSettings.ConnectAutomatically = (appSettingsSection.Settings["AutoConnect"].Value.ToLower() == "true");
            }
            else
            {
                TFSConnectionSettings.ConnectAutomatically = false;
            }

            if (appSettingsSection.Settings.AllKeys.Contains("ShowBugOrDefectByIterationPath"))
            {
                TFSConnectionSettings.ShowBugOrDefectByIterationPath = (appSettingsSection.Settings["ShowBugOrDefectByIterationPath"].Value.ToLower() == "true");
            }
            else
            {
                TFSConnectionSettings.ShowBugOrDefectByIterationPath = false;
            }

            if (appSettingsSection.Settings.AllKeys.Contains("ShowBugOrDefectByAreaPath"))
            {
                TFSConnectionSettings.ShowBugOrDefectByAreaPath = (appSettingsSection.Settings["ShowBugOrDefectByAreaPath"].Value.ToLower() == "true");
            }
            else
            {
                TFSConnectionSettings.ShowBugOrDefectByAreaPath = true;
            }

            if (appSettingsSection.Settings.AllKeys.Contains("PreferenceColorIndex1"))
            {
                TFSConnectionSettings.PreferenceColorIndex1 = appSettingsSection.Settings["PreferenceColorIndex1"].Value;
            }
            else
            {
                TFSConnectionSettings.PreferenceColorIndex1 = ColorIndex1;
            }

            if (appSettingsSection.Settings.AllKeys.Contains("PreferenceColorIndex2"))
            {
                TFSConnectionSettings.PreferenceColorIndex2 = appSettingsSection.Settings["PreferenceColorIndex2"].Value;
            }
            else
            {
                TFSConnectionSettings.PreferenceColorIndex2 = ColorIndex2;
            }

            if (appSettingsSection.Settings.AllKeys.Contains("PreferenceColorIndex3"))
            {
                TFSConnectionSettings.PreferenceColorIndex3 = appSettingsSection.Settings["PreferenceColorIndex3"].Value;
            }
            else
            {
                TFSConnectionSettings.PreferenceColorIndex3 = ColorIndex3;
            }

            if (appSettingsSection.Settings.AllKeys.Contains("Theme"))
            {
                TFSConnectionSettings.Theme = appSettingsSection.Settings["Theme"].Value;
            }
            else
            {
                TFSConnectionSettings.Theme = DefaultTheme;
            }

            if (appSettingsSection.Settings.AllKeys.Contains("PreferenceInterval1"))
            {
                TFSConnectionSettings.PreferenceInterval1 = appSettingsSection.Settings["PreferenceInterval1"].Value;
            }
            else
            {
                TFSConnectionSettings.PreferenceInterval1 = Interval1;
            }

            if (appSettingsSection.Settings.AllKeys.Contains("PreferenceInterval2"))
            {
                TFSConnectionSettings.PreferenceInterval2 = appSettingsSection.Settings["PreferenceInterval2"].Value;
            }
            else
            {
                TFSConnectionSettings.PreferenceInterval2 = Interval2;
            }

            if (appSettingsSection.Settings.AllKeys.Contains("PreferenceColorName1"))
            {
                TFSConnectionSettings.PreferenceColorName1 = appSettingsSection.Settings["PreferenceColorName1"].Value;
            }
            else
            {
                TFSConnectionSettings.PreferenceColorName1 = ColorName1;
            }

            if (appSettingsSection.Settings.AllKeys.Contains("PreferenceColorName2"))
            {
                TFSConnectionSettings.PreferenceColorName2 = appSettingsSection.Settings["PreferenceColorName2"].Value;
            }
            else
            {
                TFSConnectionSettings.PreferenceColorName2 = ColorName2;
            }

            if (appSettingsSection.Settings.AllKeys.Contains("PreferenceColorName3"))
            {
                TFSConnectionSettings.PreferenceColorName3 = appSettingsSection.Settings["PreferenceColorName3"].Value;
            }
            else
            {
                TFSConnectionSettings.PreferenceColorName3 = ColorName3;
            }
        }

        public static void SaveTFSConnectionSettingsToConfiguration()
        {
            //write the values to config file
            Configuration settingConfigurations = ConfigurationManager.OpenExeConfiguration(Assembly.GetExecutingAssembly().Location);
            AppSettingsSection appSettingsSection = settingConfigurations.AppSettings;

            if (appSettingsSection.Settings.AllKeys.Contains("Domain"))
            {
                appSettingsSection.Settings["Domain"].Value = TFSConnectionSettings.Domain;
            }
            else
            {
                appSettingsSection.Settings.Add("Domain", TFSConnectionSettings.Domain);
            }

            if (appSettingsSection.Settings.AllKeys.Contains("UserName"))
            {
                appSettingsSection.Settings["UserName"].Value = TFSConnectionSettings.UserName;
            }
            else
            {
                appSettingsSection.Settings.Add("UserName", TFSConnectionSettings.UserName);
            }

            if (appSettingsSection.Settings.AllKeys.Contains("Password"))
            {
                if (TFSConnectionSettings.Password.Length > 0)
                {
                    appSettingsSection.Settings["Password"].Value = CryptoLibrary.EncryptString(CryptoLibrary.ToSecureString(TFSConnectionSettings.Password));
                }
                else
                {
                    appSettingsSection.Settings["Password"].Value = string.Empty;
                }
            }
            else
            {
                if (TFSConnectionSettings.Password.Length > 0)
                {
                    appSettingsSection.Settings.Add("Password", CryptoLibrary.EncryptString(CryptoLibrary.ToSecureString(TFSConnectionSettings.Password)));
                }
                else
                {
                    appSettingsSection.Settings.Add("Password", string.Empty);
                }
            }

            if (appSettingsSection.Settings.AllKeys.Contains("TFSServer"))
            {
                appSettingsSection.Settings["TFSServer"].Value = TFSConnectionSettings.TfsServer;
            }
            else
            {
                appSettingsSection.Settings.Add("TFSServer", TFSConnectionSettings.TfsServer);
            }

            if (appSettingsSection.Settings.AllKeys.Contains("TFSProject"))
            {
                appSettingsSection.Settings["TFSProject"].Value = TFSConnectionSettings.TfsProject;
            }
            else
            {
                appSettingsSection.Settings.Add("TFSProject", TFSConnectionSettings.TfsProject);
            }

            if (appSettingsSection.Settings.AllKeys.Contains("DefectWorkItemType"))
            {
                appSettingsSection.Settings["DefectWorkItemType"].Value = TFSConnectionSettings.DefectWorkItemType;
            }
            else
            {
                appSettingsSection.Settings.Add("DefectWorkItemType", TFSConnectionSettings.DefectWorkItemType);
            }


            if (appSettingsSection.Settings.AllKeys.Contains("SaveUserNameAndPassword"))
            {
                appSettingsSection.Settings["SaveUserNameAndPassword"].Value = TFSConnectionSettings.SaveUserNameAndPassword ? "true" : "false";
            }
            else
            {
                appSettingsSection.Settings.Add("SaveUserNameAndPassword", TFSConnectionSettings.SaveUserNameAndPassword ? "true" : "false");
            }

            if (appSettingsSection.Settings.AllKeys.Contains("AutoConnect"))
            {
                appSettingsSection.Settings["AutoConnect"].Value = TFSConnectionSettings.ConnectAutomatically ? "true" : "false";
            }
            else
            {
                appSettingsSection.Settings.Add("AutoConnect", TFSConnectionSettings.ConnectAutomatically ? "true" : "false");
            }

            if (appSettingsSection.Settings.AllKeys.Contains("ShowBugOrDefectByIterationPath"))
            {
                appSettingsSection.Settings["ShowBugOrDefectByIterationPath"].Value = TFSConnectionSettings.ShowBugOrDefectByIterationPath ? "true" : "false";
            }
            else
            {
                appSettingsSection.Settings.Add("ShowBugOrDefectByIterationPath", TFSConnectionSettings.ShowBugOrDefectByIterationPath ? "true" : "false");
            }

            if (appSettingsSection.Settings.AllKeys.Contains("ShowBugOrDefectByAreaPath"))
            {
                appSettingsSection.Settings["ShowBugOrDefectByAreaPath"].Value = TFSConnectionSettings.ShowBugOrDefectByAreaPath ? "true" : "false";
            }
            else
            {
                appSettingsSection.Settings.Add("ShowBugOrDefectByAreaPath", TFSConnectionSettings.ShowBugOrDefectByAreaPath ? "true" : "false");
            }

            if (appSettingsSection.Settings.AllKeys.Contains("PreferenceColorIndex1"))
            {
                appSettingsSection.Settings["PreferenceColorIndex1"].Value = TFSConnectionSettings.PreferenceColorIndex1;
            }
            else
            {
                appSettingsSection.Settings.Add("PreferenceColorIndex1", TFSConnectionSettings.PreferenceColorIndex1);
            }

            if (appSettingsSection.Settings.AllKeys.Contains("PreferenceColorIndex3"))
            {
                appSettingsSection.Settings["PreferenceColorIndex3"].Value = TFSConnectionSettings.PreferenceColorIndex3;
            }
            else
            {
                appSettingsSection.Settings.Add("PreferenceColorIndex3", TFSConnectionSettings.PreferenceColorIndex3);
            }

            if (appSettingsSection.Settings.AllKeys.Contains("PreferenceColorIndex2"))
            {
                appSettingsSection.Settings["PreferenceColorIndex2"].Value = TFSConnectionSettings.PreferenceColorIndex2;
            }
            else
            {
                appSettingsSection.Settings.Add("PreferenceColorIndex2", TFSConnectionSettings.PreferenceColorIndex2);
            }

            if (appSettingsSection.Settings.AllKeys.Contains("Theme"))
            {
                appSettingsSection.Settings["Theme"].Value = TFSConnectionSettings.Theme;
            }
            else
            {
                appSettingsSection.Settings.Add("Theme", TFSConnectionSettings.Theme);
            }

            if (appSettingsSection.Settings.AllKeys.Contains("PreferenceInterval1"))
            {
                appSettingsSection.Settings["PreferenceInterval1"].Value = TFSConnectionSettings.PreferenceInterval1 ;
            }
            else
            {
                appSettingsSection.Settings.Add("PreferenceInterval1", TFSConnectionSettings.PreferenceInterval1);
            }

            if (appSettingsSection.Settings.AllKeys.Contains("PreferenceInterval2"))
            {
                appSettingsSection.Settings["PreferenceInterval2"].Value = TFSConnectionSettings.PreferenceInterval2;
            }
            else
            {
                appSettingsSection.Settings.Add("PreferenceInterval2", TFSConnectionSettings.PreferenceInterval2);
            }

            if (appSettingsSection.Settings.AllKeys.Contains("PreferenceColorName1"))
            {
                appSettingsSection.Settings["PreferenceColorName1"].Value = TFSConnectionSettings.PreferenceColorName1;
            }
            else
            {
                appSettingsSection.Settings.Add("PreferenceColorName1", TFSConnectionSettings.PreferenceColorName1);
            }

            if (appSettingsSection.Settings.AllKeys.Contains("PreferenceColorName2"))
            {
                appSettingsSection.Settings["PreferenceColorName2"].Value = TFSConnectionSettings.PreferenceColorName2;
            }
            else
            {
                appSettingsSection.Settings.Add("PreferenceColorName2", TFSConnectionSettings.PreferenceColorName2);
            }

            if (appSettingsSection.Settings.AllKeys.Contains("PreferenceColorName3"))
            {
                appSettingsSection.Settings["PreferenceColorName3"].Value = TFSConnectionSettings.PreferenceColorName3;
            }
            else
            {
                appSettingsSection.Settings.Add("PreferenceColorName3", TFSConnectionSettings.PreferenceColorName3);
            }

            settingConfigurations.Save();
        }

        #endregion        

        #endregion
    }
}
