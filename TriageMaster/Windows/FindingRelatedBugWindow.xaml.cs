using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using STaRZ.TFSLibrary;
using TriageMaster.Common;

namespace TriageMaster.Windows
{
    /// <summary>
    /// Interaction logic for FindingRelatedBugWindow.xaml
    /// </summary>
    public partial class FindingRelatedBugWindow : Window
    {
        #region [Public Properties]

        private List<TFSDefectOrBugWorkItem> bugOrDefectWorkItems;

        public List<TFSDefectOrBugWorkItem> BugOrDefectWorkItems
        {
            get { return bugOrDefectWorkItems; }
            set 
            { 
                bugOrDefectWorkItems = value;

                if (bugOrDefectWorkItems != null)
                {                   
                    LoadRelatedBugOrDefect();
                }
            }
        }

        #endregion

        #region [Public Events]
        
        public event FindRelatedBugHandler RelatedBugs;
        public delegate void FindRelatedBugHandler(object sender, bool shouldRefresh);

        public event RefreshMainWindowHandler RefreshWindow;
        public delegate void RefreshMainWindowHandler(object sender, bool shouldRefresh);

        public event WindowLoadedHandler WindowLoaded;
        public delegate void WindowLoadedHandler(object sender, bool isLoaded);
        
        #endregion

        #region [Local Variables]

        bool isFieldValueChanged = false;
        int selectedWorkItemIndex = -1;

        #endregion

        #region [Constructor(s)]
        
        public FindingRelatedBugWindow()
        {
            InitializeComponent();            
        }

        #endregion

        #region [Events]

        private void dgWIs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgWIs.SelectedItem == null)
            {
                return;
            }

            try
            {
                TFSDefectOrBugWorkItem selectedWorkItem = (TFSDefectOrBugWorkItem)dgWIs.SelectedItem;
                this.witControl.WorkItem = selectedWorkItem.WorkItemObj;
            }
            catch (Exception ex)
            {
                Log.CaptureException(ex);
                Log.LogFileWrite();
            }
        }

        private void dgWIs_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {            
            if (dgWIs.ItemsSource == null)
            {
                cmLinkToExistingItem.IsEnabled = false;
                cmUpdateWorkItemDetails.IsEnabled = false;
                return;
            }

            try
            {
                var hit = VisualTreeHelper.HitTest((Visual)sender, e.GetPosition((IInputElement)sender));
                DependencyObject cell = VisualTreeHelper.GetParent(hit.VisualHit);
                while (cell != null && !(cell is System.Windows.Controls.DataGridCell)) cell = VisualTreeHelper.GetParent(cell);
                System.Windows.Controls.DataGridCell targetCell = cell as System.Windows.Controls.DataGridCell;

                if (targetCell != null)
                {
                    cmLinkToExistingItem.IsEnabled = true;
                    cmUpdateWorkItemDetails.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                Log.CaptureException(ex);
                Log.LogFileWrite();
            }
        }

        private void btnUpdateWorkItemDetails_Click(object sender, RoutedEventArgs e)
        {
            UpdateMultipleWorkItem();
        }

        private void btnLinkItems_Click(object sender, RoutedEventArgs e)
        {
            //Link Workitems.
            selectedWorkItemIndex = dgWIs.SelectedIndex;

            if (LinkWorkItemToAnotherWorkItem())
            {
                this.ProgressBar.IsBusy = true;
                this.ProgressBar.BusyContent = MessageNotification.Fetching;

                Task.Factory.StartNew(() =>
                {                    
                    Dispatcher.Invoke((Action)(() =>
                    {
                        isFieldValueChanged = true;
                        LoadFindRelatedBugWindowAgain();
                    }
                    ));
                }
                  ).ContinueWith((task2) =>
                  {
                      dgWIs.SelectedIndex = selectedWorkItemIndex;
                      dgWIs.Focus();
                      this.ProgressBar.IsBusy = false;
                  }, TaskScheduler.FromCurrentSynchronizationContext()
              );
            }
        }

        private void cmLinkToExistingItem_Click(object sender, RoutedEventArgs e)
        {
            //Link workitems.
            selectedWorkItemIndex = dgWIs.SelectedIndex;            

            if (LinkWorkItemToAnotherWorkItem())
            {
                this.ProgressBar.IsBusy = true;
                this.ProgressBar.BusyContent = MessageNotification.Fetching;

                Task.Factory.StartNew(() =>
                {
                    Dispatcher.Invoke((Action)(() =>
                    {
                        isFieldValueChanged = true;
                        LoadFindRelatedBugWindowAgain();
                    }
                    ));
                }
                  ).ContinueWith((task2) =>
                   {
                       dgWIs.SelectedIndex = selectedWorkItemIndex;
                       dgWIs.Focus();
                       this.ProgressBar.IsBusy = false;
                   }, TaskScheduler.FromCurrentSynchronizationContext()
              );
            }
        }

        private void cmUpdateWorkItemDetails_Click(object sender, RoutedEventArgs e)
        {
            //Update multiple workitems.
            UpdateMultipleWorkItem();
        }

        private void btnSaveWorkItemDetails_Click(object sender, RoutedEventArgs e)
        {
            //Save all work item details.
            selectedWorkItemIndex = dgWIs.SelectedIndex;

            SetButtonStatus(false);

            this.ProgressBar.IsBusy = true;
            this.ProgressBar.BusyContent = MessageNotification.Saving;

            string triageComment = string.Empty;
            if (!string.IsNullOrEmpty(txtTriageComment.Text.Trim()))
            {
                triageComment = txtTriageComment.Text.Trim();
            }

            Task.Factory.StartNew(() =>
            {
                if (this.witControl.SaveWorkItemDetails(triageComment))
                {
                    Dispatcher.Invoke((Action)(() =>
                    {
                        txtTriageComment.Clear();
                        isFieldValueChanged = true;
                        LoadFindRelatedBugWindowAgain();
                    }
                    ));
                }
            }
               ).ContinueWith((task) =>
               {
                   SetButtonStatus(true);
                   if (dgWIs.SelectedIndex != selectedWorkItemIndex)
                   {
                       dgWIs.SelectedIndex = -1;
                       if (dgWIs.Items.Count == selectedWorkItemIndex)
                       {
                           dgWIs.SelectedIndex = selectedWorkItemIndex - 1;
                       }
                       else
                       {
                           dgWIs.SelectedIndex = selectedWorkItemIndex;
                       }
                   }
                   else
                   {
                       dgWIs.SelectedIndex = selectedWorkItemIndex;
                   }
                   dgWIs.Focus();
                   this.ProgressBar.IsBusy = false;

               }, TaskScheduler.FromCurrentSynchronizationContext()
            );
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (RefreshWindow != null && isFieldValueChanged)
            {
                isFieldValueChanged = false;
                RefreshWindow(sender, true);
            }
            else
            {
                WindowLoaded(sender, true);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (WindowLoaded != null)
            {
                WindowLoaded(null, true);
            }
        }

        #endregion

        #region [Private Methods]

        private void LoadRelatedBugOrDefect()
        {
            // Load all workitems matched with the selected work item title in Main window.
            this.Dispatcher.Invoke((Action)(() =>
            {
                lblMessage.Content = string.Empty;
                try
                {
                    if (BugOrDefectWorkItems != null)
                    {
                        SetButtonStatus(true);

                        dgWIs.ItemsSource = BugOrDefectWorkItems.OrderByDescending(m => int.Parse(m.Matching)).ToList(); ;
                        dgWIs.SelectedIndex = 0;
                        dgWIs.Focus();
                    }
                    if (BugOrDefectWorkItems.Count > 0)
                    {
                        lblMessage.Content = string.Format("{0} " + MessageNotification.RelatedBugsOrDefectsPresent, BugOrDefectWorkItems.Count);
                        lblMessage.Foreground = Brushes.Green;
                    }
                }
                catch (Exception ex)
                {
                    Log.CaptureException(ex);
                    Log.LogFileWrite();
                }
            }
            ));
        }

        private bool LinkWorkItemToAnotherWorkItem()
        {
            try
            {
                BugOrDefectLinkingWindow linkToExistingWindow = new BugOrDefectLinkingWindow();

                List<int> selectedWorkItemId = new List<int>();
                List<string> selectedWorkItemTitle = new List<string>();

                if (dgWIs.SelectedItems.Count == 1)
                {
                    selectedWorkItemId.Add(Convert.ToInt32(((TFSDefectOrBugWorkItem)dgWIs.SelectedItem).Id));
                }
                else
                {
                    List<TFSDefectOrBugWorkItem> lists = dgWIs.SelectedItems.Cast<TFSDefectOrBugWorkItem>().ToList();
                    foreach (TFSDefectOrBugWorkItem selectedWIItem in lists)
                    {
                        selectedWorkItemId.Add(Convert.ToInt32(selectedWIItem.Id));
                    }
                }

                if (dgWIs.SelectedItems.Count == 1)
                {
                    selectedWorkItemTitle.Add(((TFSDefectOrBugWorkItem)dgWIs.SelectedItem).Title);
                }
                else
                {
                    List<TFSDefectOrBugWorkItem> lists = dgWIs.SelectedItems.Cast<TFSDefectOrBugWorkItem>().ToList();
                    foreach (TFSDefectOrBugWorkItem selectedWIItem in lists)
                    {
                        selectedWorkItemTitle.Add(selectedWIItem.Title);
                    }
                }

                if (selectedWorkItemId != null)
                {
                    linkToExistingWindow.SelectedWorkItemID = selectedWorkItemId;
                }

                if (selectedWorkItemTitle != null)
                {
                    linkToExistingWindow.SelectedWorkItemTitle = selectedWorkItemTitle;
                }

                if (linkToExistingWindow.ShowDialog() == true)
                {
                    DialogBox.ShowInfo(MessageNotification.WorkItemLinked);
                    return true;
                }
                else
                {
                    dgWIs.Focus();
                }

            }
            catch (Exception ex)
            {
                Log.CaptureException(ex);
                Log.LogFileWrite();
                return false;
            }
            return false;
        }

        private void UpdateMultipleWorkItem()
        {
            selectedWorkItemIndex = dgWIs.SelectedIndex;

            SetButtonStatus(false);

            try
            {

                List<int> selectedWorkItemId = new List<int>();

                if (dgWIs.SelectedItems.Count == 1)
                {
                    selectedWorkItemId.Add(Convert.ToInt32(((TFSDefectOrBugWorkItem)dgWIs.SelectedItem).Id));
                }
                else
                {
                    List<TFSDefectOrBugWorkItem> lists = dgWIs.SelectedItems.Cast<TFSDefectOrBugWorkItem>().ToList();
                    foreach (TFSDefectOrBugWorkItem selectedWIItem in lists)
                    {
                        selectedWorkItemId.Add(Convert.ToInt32(selectedWIItem.Id));
                    }
                }

                this.ProgressBar.IsBusy = true;
                this.ProgressBar.BusyContent = MessageNotification.LoadingUpdateWinow;

                MultipleWorkItemDetailsUpdateWindow multipleWorkItemDetailsUpdateWindow = new MultipleWorkItemDetailsUpdateWindow();

                bool isupdated = false;
                Task.Factory.StartNew(() =>
                {
                    multipleWorkItemDetailsUpdateWindow.WorkItemIDs = selectedWorkItemId;
                    multipleWorkItemDetailsUpdateWindow.IsUpdateWindowLoaded += multipleWorkItemDetailsUpdateWindow_IsUpdateWindowLoaded;
                    multipleWorkItemDetailsUpdateWindow.LoadMultipleWorkItemWIndow = true;

                    Dispatcher.Invoke((Action)(() =>
                    {
                        multipleWorkItemDetailsUpdateWindow.ShowDialog();

                        if (multipleWorkItemDetailsUpdateWindow.DialogResult == true)
                        {
                            DialogBox.ShowInfo(MessageNotification.WorkitemsUpdated);
                            isupdated = true;
                        }
                        else
                        {
                            dgWIs.Focus();
                        }
                    }
                    ));
                }).ContinueWith((task) =>
                {
                    if (isupdated)
                    {
                        this.ProgressBar.IsBusy = true;
                        this.ProgressBar.BusyContent = MessageNotification.Fetching;

                        Task.Factory.StartNew(() =>
                        {
                            Dispatcher.Invoke((Action)(() =>
                            {
                                isFieldValueChanged = true;
                                LoadFindRelatedBugWindowAgain();
                            }
                            ));
                        }
                       ).ContinueWith((task1) =>
                       {
                           dgWIs.SelectedIndex = selectedWorkItemIndex;
                           dgWIs.Focus();
                           this.ProgressBar.IsBusy = false;
                       }, TaskScheduler.FromCurrentSynchronizationContext()
                    );
                    }
                    SetButtonStatus(true);
                }, TaskScheduler.FromCurrentSynchronizationContext()
                );
            }
            catch (Exception ex)
            {
                Log.CaptureException(ex);
                Log.LogFileWrite();
            }
        }

        private void multipleWorkItemDetailsUpdateWindow_IsUpdateWindowLoaded(object sender, bool isLoad)
        {
            if (isLoad)
            {
                if (this.ProgressBar.IsBusy)
                {
                    this.ProgressBar.IsBusy = false;
                }
            }
        }    

        private void SetButtonStatus(bool isEnabled)
        {
            btnLinkItems.IsEnabled = isEnabled;
            btnUpdateWorkItemDetails.IsEnabled = isEnabled;
            btnSaveWorkItemDetails.IsEnabled = isEnabled;

            cmLinkToExistingItem.IsEnabled = false;
            cmUpdateWorkItemDetails.IsEnabled = false;
        }

        private void LoadFindRelatedBugWindowAgain()
        {
            dgWIs.ItemsSource = null;
            this.witControl.WorkItem = null;
            SetButtonStatus(false);

            if (RelatedBugs != null)
            {
                RelatedBugs(null,true);
            }
        }

        #endregion       
       
    }
}
