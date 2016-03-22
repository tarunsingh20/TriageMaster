using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using STaRZ.TFSLibrary;
using TriageMaster.Common;

namespace TriageMaster.Windows
{
    /// <summary>
    /// Interaction logic for TFSSettings.xaml
    /// </summary>
    public partial class TFSSettings : Window
    {
        #region [Constructors]

        public TFSSettings()
        {
            InitializeComponent();
            SetControls();
            btnLaunch.Content = "Launch";

            if (Helper.TFSConnectionSettings.ConnectAutomatically)
            {
                this.Hide();
                this.StartMainWindow();
                this.Close();
            }
        }

        public TFSSettings(string launchedFrom, Helper.ErrorType errorType = Helper.ErrorType.None)
        {
            string showErrorMessage = string.Empty;

            InitializeComponent();
            SetControls();

            switch (errorType)
            {
                case Helper.ErrorType.TFSWrapperError:
                    showErrorMessage = MessageNotification.TFSSERVERNOTCONNECTEDERROR;
                    break;

                case Helper.ErrorType.TFSStoreError:
                    showErrorMessage = MessageNotification.TFSWORKITEMSTOREINVALIDERROR;
                    break;

                case Helper.ErrorType.TFSProjectError:
                    showErrorMessage = MessageNotification.TFSPROJECTINVALIDERROR;
                    break;

                case Helper.ErrorType.TFSWrongUriError:
                    showErrorMessage = MessageNotification.TFSWRONGURIENTEREDERROR;
                    break;
            }

            if (launchedFrom == "MainWindow")
            {
                btnLaunch.Content = "OK";
            }
            else
            {
                btnLaunch.Content = "Launch";
                ConnectionErrorMsg.Text = showErrorMessage;
                ConnectionErrorMsg.Visibility = System.Windows.Visibility.Visible;
            }
        }

        #endregion

        #region [Events]      

        private void TFSSettingsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            string uri = string.Empty;

            switch (cmbThemes.Text.ToString())
            {
                case "ShinyRed":
                    uri = "/TriageMaster;component/Themes/ShinyRed.xaml";
                    break;

                case "ShinyBlue":
                    uri = "/TriageMaster;component/Themes/ShinyBlue.xaml";
                    break;

                default:
                    uri = "/TriageMaster;component/Themes/ShinyBlue.xaml";
                    //cmbThemes.SelectedIndex = 0;
                    break;
            }

            Application.Current.Resources.MergedDictionaries.Clear();
            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary()
            {
                Source = new Uri(uri, UriKind.RelativeOrAbsolute)
            });            

            InitializeColors();
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            string serverName;
            string teamProjectName;

            try
            {
                if (TFSWrapper.ConnectToTFS(out serverName, out teamProjectName))
                {
                    if (!serverName.Equals(string.Empty))
                        txtTFSServer.Text = serverName;

                    if (!teamProjectName.Equals(string.Empty))
                        txtTFSProject.Text = teamProjectName;
                }
                else
                {
                    DialogBox.ShowError("Unable to connect TFS project !!!");
                }
            }
            catch (Exception exc)
            {
                Log.CaptureException(exc);
                Log.LogFileWrite();
            }
        }

        private void btnLaunch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (btnLaunch.Content.ToString().ToLower() != "ok")
                {
                    if (UpdateOptions())
                    {
                        this.Hide();

                        Helper.SettingsError = Helper.ErrorType.None;
                        //re-initialize the TfsWrapper as the settings/credentials might have changed
                        Helper.TfsWrapper = null;

                        ConnectionErrorMsg.Visibility = System.Windows.Visibility.Hidden;

                        this.StartMainWindow();

                        if (!chkSavePassword.IsChecked.Value)
                        {
                            Helper.TFSConnectionSettings.UserName = string.Empty;
                            Helper.TFSConnectionSettings.Password = string.Empty;
                            Helper.TFSConnectionSettings.SaveUserNameAndPassword = false;
                            Helper.TFSConnectionSettings.ConnectAutomatically = false;
                        }

                        Helper.SaveTFSConnectionSettingsToConfiguration();
                        this.Close();
                    }
                }
                else
                {
                    if (cmbThemes.Text.ToString() != Helper.TFSConnectionSettings.Theme)
                    {
                        string themeName = string.Empty;

                        if (cmbThemes.Text.ToString() == Helper.ThemeName.ShinyRed.ToString())
                        {
                            themeName = Helper.ThemeName.ShinyRed.ToString();
                        }
                        else
                        {
                            themeName = Helper.ThemeName.ShinyBlue.ToString();
                        }

                        new MainWindow().ThemeName = themeName;
                    }

                    if (UpdateOptions())
                    {
                        if (!chkSavePassword.IsChecked.Value)
                        {
                            Helper.TFSConnectionSettings.UserName = string.Empty;
                            Helper.TFSConnectionSettings.Password = string.Empty;
                            Helper.TFSConnectionSettings.SaveUserNameAndPassword = false;
                            Helper.TFSConnectionSettings.ConnectAutomatically = false;
                        }

                        Helper.SaveTFSConnectionSettingsToConfiguration();
                        this.Close();
                    }
                }
            }
            catch (Exception exc)
            {
                Log.CaptureException(exc);
                Log.LogFileWrite();
            }
        }

        private void ChkSavePassword_Changed(object sender, RoutedEventArgs e)
        {
            if (chkSavePassword.IsChecked == false)
            {
                chkConnectAuto.IsChecked = false; 
                chkConnectAuto.IsEnabled = false; 
                chkConnectAuto.IsTabStop = false;
            }
            else
            {
                chkConnectAuto.IsEnabled = true;
                chkConnectAuto.IsTabStop = true;
            }
        }        

        #endregion

        #region [Private Methods]

        private void StartMainWindow()
        {
            if (SplashStartup.StartSplash())
            {
                MainWindow mainForm = new MainWindow();
                mainForm.Show();
                this.Close();
            }
        }

        private bool UpdateOptions()
        {
            bool fieldSpecified = true;
            Helper.TFSConnectionSettings.TfsServer = txtTFSServer.Text.Trim();
            Helper.TFSConnectionSettings.TfsProject = txtTFSProject.Text.Trim();
            if (txtUsername.Text.Trim().Contains('\\'))
            {
                string[] credential = txtUsername.Text.Trim().Split('\\');
                Helper.TFSConnectionSettings.Domain = credential[0];
                Helper.TFSConnectionSettings.UserName = credential[1];
            }
            else
            {
                Helper.TFSConnectionSettings.Domain = MessageNotification.DOMAIN;
                Helper.TFSConnectionSettings.UserName = txtUsername.Text.Trim();
            }
            Helper.TFSConnectionSettings.Password = txtPassword.Password;
            Helper.TFSConnectionSettings.SaveUserNameAndPassword = chkSavePassword.IsChecked != null ? chkSavePassword.IsChecked.Value : false;
            Helper.TFSConnectionSettings.ConnectAutomatically = chkConnectAuto.IsChecked != null ? chkConnectAuto.IsChecked.Value : false;
            
            Helper.TFSConnectionSettings.ShowBugOrDefectByAreaPath = chkAreaPath.IsChecked != null ? chkAreaPath.IsChecked.Value : false;
            Helper.TFSConnectionSettings.ShowBugOrDefectByIterationPath = chkIterationPath.IsChecked != null ? chkIterationPath.IsChecked.Value : false;
            Helper.TFSConnectionSettings.DefectWorkItemType = cmbBugOrDefects.Text;

            //Saving TFS ColorCode and theme to the configuration Path.

            Helper.TFSConnectionSettings.PreferenceColorName1 = cmbPreference1.SelectedValue != null ? (cmbPreference1.SelectedValue as ColorCoding).Name.ToString().Trim() : string.Empty;
            Helper.TFSConnectionSettings.PreferenceColorName2 = cmbPreference2.SelectedValue != null ? (cmbPreference2.SelectedValue as ColorCoding).Name.ToString().Trim() : string.Empty;
            Helper.TFSConnectionSettings.PreferenceColorName3 = cmbPreference3.SelectedValue != null ? (cmbPreference3.SelectedValue as ColorCoding).Name.ToString().Trim() : string.Empty;


            Helper.TFSConnectionSettings.PreferenceColorIndex1 = cmbPreference1.SelectedIndex != -1 ? cmbPreference1.SelectedIndex.ToString().Trim() : string.Empty;
            Helper.TFSConnectionSettings.PreferenceColorIndex2 = cmbPreference2.SelectedIndex != -1 ? cmbPreference2.SelectedIndex.ToString().Trim() : string.Empty;
            Helper.TFSConnectionSettings.PreferenceColorIndex3 = cmbPreference3.SelectedIndex != -1 ? cmbPreference3.SelectedIndex.ToString().Trim() : string.Empty;
            Helper.TFSConnectionSettings.Theme = cmbThemes.SelectedValue != null ? cmbThemes.Text.ToString().Trim() : string.Empty;

            Helper.TFSConnectionSettings.PreferenceInterval1 = txtPreference1.Text.ToString().Trim();
            Helper.TFSConnectionSettings.PreferenceInterval2 = txtPreference2.Text.ToString().Trim();

            if (btnLaunch.Content.ToString().ToLower() != "ok")
            {
                if (Helper.TFSConnectionSettings.TfsServer.Length == 0 ||
                    Helper.TFSConnectionSettings.TfsProject.Length == 0 ||
                    Helper.TFSConnectionSettings.UserName.Length == 0 ||
                    Helper.TFSConnectionSettings.Password.Length == 0)
                {
                    ConnectionErrorMsg.Text = "Please check if TFS Server URL/TFS Project/Username/password are specified.";
                    ConnectionErrorMsg.Visibility = System.Windows.Visibility.Visible;
                    fieldSpecified = false;
                }
            }
            
            return fieldSpecified;
        }

        private void CheckGenericTasksXML()
        {
            throw new NotImplementedException();
        }

        private void SetControls()
        {
            txtTFSServer.Text = Helper.TFSConnectionSettings.TfsServer;
            txtTFSProject.Text = Helper.TFSConnectionSettings.TfsProject;
            if (Helper.TFSConnectionSettings.Domain.Length == 0)
            {
                Helper.TFSConnectionSettings.Domain = MessageNotification.DOMAIN;
            }
            txtUsername.Text = Helper.TFSConnectionSettings.Domain + @"\" + Helper.TFSConnectionSettings.UserName;
            txtPassword.Password = Helper.TFSConnectionSettings.Password;
            chkSavePassword.IsChecked = Helper.TFSConnectionSettings.SaveUserNameAndPassword;
            chkConnectAuto.IsChecked = Helper.TFSConnectionSettings.ConnectAutomatically;

            chkIterationPath.IsChecked = Helper.TFSConnectionSettings.ShowBugOrDefectByIterationPath;
            chkAreaPath.IsChecked = Helper.TFSConnectionSettings.ShowBugOrDefectByAreaPath;  

            cmbPreference1.SelectedIndex = Convert.ToInt32(Helper.TFSConnectionSettings.PreferenceColorIndex1);

            cmbPreference2.SelectedIndex = Convert.ToInt32(Helper.TFSConnectionSettings.PreferenceColorIndex2);

            cmbPreference3.SelectedIndex = Convert.ToInt32(Helper.TFSConnectionSettings.PreferenceColorIndex3);

            cmbBugOrDefects.Text = Helper.TFSConnectionSettings.DefectWorkItemType;
            cmbThemes.Text = Helper.TFSConnectionSettings.Theme;

            txtPreference1.Text = Helper.TFSConnectionSettings.PreferenceInterval1;
            txtPreference2.Text = Helper.TFSConnectionSettings.PreferenceInterval2;            

            ConnectionErrorMsg.Visibility = System.Windows.Visibility.Hidden;
        }       
        
        private void InitializeColors()
        {
            cmbPreference1.Items.Clear();
            cmbPreference2.Items.Clear();
            cmbPreference3.Items.Clear();

            // Add some common colors
            AddColor(Colors.Black, "Black");
            AddColor(Colors.Gray, "Gray");
            AddColor(Colors.LightGray, "LightGray");
            AddColor(Colors.White, "White");
            AddColor(Colors.Transparent, "Transparent");
            AddColor(Colors.Red, "Red");
            AddColor(Colors.Green, "Green");
            AddColor(Colors.Blue, "Blue");
            AddColor(Colors.Cyan, "Cyan");
            AddColor(Colors.Magenta, "Magenta");
            AddColor(Colors.Yellow, "Yellow");
            AddColor(Colors.Purple, "Purple");
            AddColor(Colors.Orange, "Orange");
            AddColor(Colors.Brown, "Brown");            

            cmbPreference1.Items.Add(new Separator());
            cmbPreference2.Items.Add(new Separator());
            cmbPreference3.Items.Add(new Separator());

            // Enumerate constant colors from the Colors class
            Type colorsType = typeof(Colors);
            PropertyInfo[] pis = colorsType.GetProperties();
            foreach (PropertyInfo pi in pis)
                AddColor((Color)pi.GetValue(null, null), pi.Name);
            
        }

        private void AddColor(Color color, string name)
        {
            if (!name.StartsWith("#", StringComparison.Ordinal))
            {
                name = FormatColorName(name);
            }
            ColorCoding cvm = new ColorCoding() { Color = color, Name = name };

            cmbPreference1.Items.Add(cvm);
            cmbPreference2.Items.Add(cvm);
            cmbPreference3.Items.Add(cvm);
        }

        private static string FormatColorName(string name)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < name.Length; i++)
            {
                if (i > 0 && char.IsUpper(name[i]))
                {
                    sb.Append(" ");
                }
                sb.Append(name[i]);
            }
            return sb.ToString();
        }

        private void txtPreference_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text.Trim());
        }

        #endregion
    }
}
