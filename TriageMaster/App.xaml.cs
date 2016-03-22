using System;
using System.Windows;
using TriageMaster.Common;

namespace TriageMaster
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    
    
    public partial class App : Application
    {
        public static string[] commandLineArgs;

        
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (e.Args.Length > 0)
            {
                commandLineArgs = e.Args;
            }
        }
        [STAThread]
        protected override void OnStartup(StartupEventArgs e)
        {
            ApplicationSingleInstance.Make();
            base.OnStartup(e);
        }
    }
}
