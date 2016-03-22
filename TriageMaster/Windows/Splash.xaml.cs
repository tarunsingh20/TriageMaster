using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Media.Animation;
using TriageMaster.Common;

namespace TriageMaster.Windows
{
    /// <summary>
    /// Interaction logic for Splash.xaml
    /// </summary>
    public partial class Splash : Window
    {
        #region [Local Variables]

        private delegate void ShowDelegate(string txt);
        private delegate void HideDelegate();
        private BackgroundWorker backgroundWorker;

        ShowDelegate showDelegate;
        HideDelegate hideDelegate;
        Storyboard Showboard;
        Storyboard Hideboard;

        int step = 0;
        int maxSteps = 4;
        int highestPercentageReached = 0;

        #endregion

        #region [Constructors]

        public Splash()
        {
            backgroundWorker = new BackgroundWorker();
            InitializeComponent();
            InitializeBackgroundWorker();

            showDelegate = new ShowDelegate(this.ShowText);
            hideDelegate = new HideDelegate(this.HideText);
            Showboard = this.Resources["ShowStoryBoard"] as Storyboard;
            Hideboard = this.Resources["HideStoryBoard"] as Storyboard;

            backgroundWorker.RunWorkerAsync();
        }

        #endregion

        #region [Events]

        // Set up the BackgroundWorker object by  
        // attaching event handlers.  
        private void InitializeBackgroundWorker()
        {
            backgroundWorker.DoWork += new DoWorkEventHandler(BackgroundWorker_DoWork);
            backgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BackgroundWorker_RunWorkerCompleted);
            backgroundWorker.ProgressChanged += new ProgressChangedEventHandler(backgroundWorker_ProgressChanged);

            backgroundWorker.WorkerSupportsCancellation = true;
            backgroundWorker.WorkerReportsProgress = true;
        }

        private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //this.Close();
        }

        private void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.Close();
        }

        // This event handler is where the actual, 
        // potentially time-consuming work is done. 
        private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            // Get the BackgroundWorker that raised this event.
            BackgroundWorker worker = sender as BackgroundWorker;

            // Assign the result of the computation 
            // to the Result property of the DoWorkEventArgs 
            // object. This is will be available to the  
            // RunWorkerCompleted eventhandler.            
            LoadResources(worker, e);
        }

        private void LoadResources(BackgroundWorker worker, DoWorkEventArgs e)
        {

            if (step >= maxSteps)
            {
                return;
            }
            if (worker.CancellationPending)
            {
                e.Cancel = true;
            }
            else
            {
                switch (step)
                {
                    // No process has been done. Connect to TFS now.
                    case 0:
                        this.Dispatcher.Invoke(showDelegate, "Initializing...");
                        Thread.Sleep(1000);
                        this.Dispatcher.Invoke(hideDelegate);
                        step++;
                        break;

                    case 1:
                        this.Dispatcher.Invoke(showDelegate, "Connecting to Team Foundation Server...");
                        //We were not able to connect to TFS, go back and throw error on the Options form
                        if (Helper.TfsWrapper.TfsServerAuthenticated == false)
                        {
                            Helper.SettingsError = Helper.ErrorType.TFSWrapperError;
                            worker.CancelAsync();
                        }
                        this.Dispatcher.Invoke(hideDelegate);
                        step++;
                        break;

                    case 2:
                        this.Dispatcher.Invoke(showDelegate, "Validating TFS project...");
                        if (Helper.TfsWrapper.TfsProjectExist == false)
                        {
                            Helper.SettingsError = Helper.ErrorType.TFSProjectError;
                            worker.CancelAsync();
                        }
                        Thread.Sleep(1000);
                        this.Dispatcher.Invoke(hideDelegate);
                        step++;
                        break;

                    case 3:
                        this.Dispatcher.Invoke(showDelegate, "Getting list of queries...");
                        Thread.Sleep(1000);
                        this.Dispatcher.Invoke(hideDelegate);
                        step++;
                        break;

                    default:
                        break;
                }

                int percentComplete = (int)((float)step / (float)2 * 100);

                if (percentComplete > highestPercentageReached)
                {
                    highestPercentageReached = percentComplete;
                    worker.ReportProgress(percentComplete);
                }

                LoadResources(worker, e);
            }
        }

        #endregion

        #region [Private Methods]

        private void ShowText(string txt)
        {
            txtMessage.Text = txt;
            BeginStoryboard(Showboard);
        }

        private void HideText()
        {
            BeginStoryboard(Hideboard);
        }

        #endregion
    }
}
