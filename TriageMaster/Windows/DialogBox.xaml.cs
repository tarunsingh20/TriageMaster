using System.Diagnostics;
using System.IO;
using System.Windows;
using STaRZ.TFSLibrary;
using TriageMaster.Common;

namespace TriageMaster.Windows
{
    /// <summary>
    /// Interaction logic for DialogBox.xaml
    /// </summary>
    public partial class DialogBox : Window
    {
        #region [Constructors]

        public DialogBox()
        {
            InitializeComponent();
        }

        public DialogBox(string message)
        {
            InitializeComponent();
            Message = message;
            this.Show();
        }

        #endregion

        #region [Properties]

        public string Message
        {
            set
            {
                lblMessage.Text = value;
            }
        }

        #endregion

        #region [Events]

        public static bool Show(string message)
        {
            DialogBox dialogBox = new DialogBox();
            dialogBox.Message = message;
            dialogBox.ShowDialog();
            if (dialogBox.DialogResult == true)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool Show(string message, string title)
        {
            DialogBox dialogBox = new DialogBox();
            dialogBox.Message = message;
            dialogBox.Title = title;
            dialogBox.ShowDialog();
            if (dialogBox.DialogResult == true)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static void ShowError(string message)
        {
            DialogBox dialogBox = new DialogBox();
            dialogBox.Message = message;
            dialogBox.Title = TFSConstants.MessageTypes.Error;
            dialogBox.btnCancel.Visibility = Visibility.Collapsed;
            dialogBox.ShowDialog();
        }

        public static void ShowInfo(string message)
        {
            DialogBox dialogBox = new DialogBox();
            dialogBox.Message = message;
            dialogBox.Title = TFSConstants.MessageTypes.Information;
            dialogBox.btnCancel.Visibility = Visibility.Collapsed;
            dialogBox.ShowDialog();
        }

        public static void ShowWarning(string message)
        {
            DialogBox dialogBox = new DialogBox();
            dialogBox.Message = message;
            dialogBox.Title = TFSConstants.MessageTypes.Warning;
            dialogBox.btnCancel.Visibility = Visibility.Collapsed;
            dialogBox.ShowDialog();
        }

        #endregion

        #region [Private Methods]

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (Log.ExceptionOccured && chkShowErrors.IsChecked == true)
            {
                string file = Path.Combine(Log.LogPath, Helper.LOGFILENAME);
                Process.Start(file);
                Log.ExceptionOccured = false;
            }

            DialogResult = true;
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        #endregion

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (Log.ExceptionOccured)
            {
                chkShowErrors.Visibility = System.Windows.Visibility.Visible;
            }
        }
    }
}
