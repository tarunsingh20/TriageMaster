using System.Configuration;

namespace TriageMaster.Common
{
    public class TFSConnectionSettings
    {
        public bool ConnectAutomatically { get; set; }
        public bool SaveUserNameAndPassword { get; set; }
        public bool ShowBugOrDefectByIterationPath { get; set; }
        public bool ShowBugOrDefectByAreaPath { get; set; }
        public string TfsServer { get; set; }
        public string TfsProject { get; set; }
        public string Domain { get; set; }
        public string UserName { get; set; }
        public string DefectWorkItemType { get; set; }
        public string Password { get; set; }
        public string PreferenceColorIndex1 { get; set; }
        public string PreferenceColorIndex2 { get; set; }
        public string PreferenceColorIndex3 { get; set; }
        public string PreferenceColorName1 { get; set; }
        public string PreferenceColorName2 { get; set; }
        public string PreferenceColorName3 { get; set; }
        public string PreferenceInterval1 { get; set; }
        public string PreferenceInterval2 { get; set; }
        public string PreferenceInterval3 { get; set; }
        public string Theme { get; set; }
        public Configuration ConfigurationSetting { get; set; }
        AppSettingsSection AppSettingsSection { get; set; }

        public TFSConnectionSettings()
        {
            this.ConnectAutomatically = false;
            this.SaveUserNameAndPassword = false;
            this.ShowBugOrDefectByAreaPath = true;
            this.ShowBugOrDefectByIterationPath = false;

            this.Domain = string.Empty;
            this.UserName = string.Empty;
            this.Password = string.Empty;
            this.TfsServer = string.Empty;
            this.TfsProject = string.Empty;
            this.DefectWorkItemType = "Bug";

            this.PreferenceColorIndex3 = string.Empty;
            this.PreferenceColorIndex1 = string.Empty;
            this.PreferenceColorIndex2 = string.Empty;
            this.PreferenceColorName1 = string.Empty;
            this.PreferenceColorName2 = string.Empty;
            this.PreferenceColorName3 = string.Empty;
            this.Theme = "ShinyBlue";

            this.PreferenceInterval1 = string.Empty;
            this.PreferenceInterval3 = string.Empty;
            this.PreferenceInterval2 = string.Empty;
        }

        public TFSConnectionSettings(string tfsServer, string tfsProject, string domain, string userName, string password, bool saveUserPwd,
            bool autoConnect, string preferenceColorIndex1, string preferenceColorIndex2, string preferenceColorIndex3, string theme, string preferenceInterval1,
            string preferenceInterval3, string preferenceInterval2, string preferenceColorName1, string preferenceColorName2, string preferenceColorName3,
            string defectWorkItemType = "Bug", bool showBugOrDefectByAreaPath = true, bool showBugOrDefectByIterationPath = false)
        {
            this.ConnectAutomatically = autoConnect;
            this.SaveUserNameAndPassword = saveUserPwd;
            this.Domain = domain;
            this.UserName = userName;
            this.Password = password;
            this.TfsServer = tfsServer;
            this.TfsProject = tfsProject;
            this.DefectWorkItemType = defectWorkItemType;

            this.ShowBugOrDefectByAreaPath = showBugOrDefectByAreaPath;
            this.ShowBugOrDefectByIterationPath = showBugOrDefectByIterationPath;

            this.PreferenceColorIndex1 = preferenceColorIndex1;
            this.PreferenceColorIndex3 = preferenceColorIndex2;
            this.PreferenceColorIndex2 = preferenceColorIndex3;

            this.PreferenceColorName1 = preferenceColorName1;
            this.PreferenceColorName2 = preferenceColorName2;
            this.PreferenceColorName3 = preferenceColorName3;

            this.Theme = theme;

            this.PreferenceInterval1 = preferenceInterval1;
            this.PreferenceInterval3 = preferenceInterval3;
            this.PreferenceInterval2 = preferenceInterval2;
        }

    }
}
