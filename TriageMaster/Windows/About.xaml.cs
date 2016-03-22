using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace TriageMaster.Windows
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About : Window
    {
        #region [Constructor]

        public About()
        {
            InitializeComponent();
        }

        #endregion

        #region [Events]

        private void TextBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Process.Start("mailto:tarunsingh20@gmail.com; sudhakarreddy.pr@hotmail.com; zubair.m.ahmed@hotmail.com; rajendrosahu@gmail.com?Subject=Feedback on TriageMaster version 1.1");
            this.Close();
        }

        private void CloseForm(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        #endregion
    }
}
