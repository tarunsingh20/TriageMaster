using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml;
using Microsoft.Win32;
using STaRZ.TFSLibrary;
using TriageMaster.Common;
using TriageMaster.StringMatching;

namespace TriageMaster.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>    
    
    public partial class MainWindow : Window
    {
        #region Public Properties

        private string themeName;

        public string ThemeName
        {
            get { return themeName; }

            set
            {
                themeName = value;
                if (!string.IsNullOrEmpty(themeName))
                    ChangeApplicationTheme(themeName);
            }
        }       

        #endregion

        #region [Local Variables]

        int WIID { get; set; }

        List<TFSDefectOrBugWorkItem> bugOrDefectWorkItems = null;

        private bool isRefreshedOrSaved = false;
        bool isTriageMasterLoadedFirstTime = false;

        int selectedAreaPathIndex = -1;
        int selectedWorkItemIndex = -1;

        FindingRelatedBugWindow relatedBugWindow;

        string queryString = string.Empty;

        #endregion
        
        #region [Constructor]
        
        public MainWindow()
        {
            InitializeComponent();
        }

        #endregion

        #region [Private Methods]

        private void LoadRelatedBugOrDefect()
        {
            // Load bugs / defects of a particaular query.
            try
            {
                if (cmbQueries.SelectedIndex > 0)
                { 
                    if (bugOrDefectWorkItems == null)
                    {
                        lblMessage.Content = MessageNotification.NoWorkItemPresent;
                        lblMessage.Foreground = Brushes.IndianRed;

                        SetButtonStatus(false);
                        return;
                    }
                    else
                    {
                        if (bugOrDefectWorkItems.Count == 0)
                        {
                            lblMessage.Content = MessageNotification.NoWorkItemPresent;
                            lblMessage.Foreground = Brushes.IndianRed;
                            SetButtonStatus(false);
                            return;
                        }
                    }

                    lblMessage.Content = string.Format("{0} " + MessageNotification.WorkItemPresent, bugOrDefectWorkItems.Count);
                    lblMessage.Foreground = Brushes.Green;

                    SetButtonStatus(true);

                    dgWIs.ItemsSource = bugOrDefectWorkItems;

                    List<string> areaPaths = new List<string>();
                    areaPaths.Insert(0, "All");

                    foreach (TFSDefectOrBugWorkItem queryItem in bugOrDefectWorkItems.GroupBy(ap => ap.AreaPath).Select(g => g.FirstOrDefault()))
                    {
                        areaPaths.Add(queryItem.AreaPath);
                    }
                    lvAreaPathList.ItemsSource = areaPaths;

                    if (isTriageMasterLoadedFirstTime)
                    {
                        SetFocusToTheSelectedWorkItem(0, 0);
                    }
                    else
                    {                       
                        SetFocusToTheSelectedWorkItem(selectedWorkItemIndex, selectedAreaPathIndex);
                    }
                }
            }
            catch (Exception ex)
            {
                btnGetWorkItems.IsEnabled = true;
                
                Log.CaptureException(ex);
                Log.LogFileWrite();
            }
        }

        private void SetFocusToTheSelectedWorkItem(int selectedWorkItemIndex, int selectedAreaPathIndex)
        {
            lvAreaPathList.SelectedIndex = dgWIs.SelectedIndex = -1;
            lvAreaPathList.SelectedIndex = selectedAreaPathIndex;
            dgWIs.SelectedIndex = selectedWorkItemIndex;  
            
        }

        private void ChangeApplicationTheme(string theme)
        {
            string uri = string.Empty;
            try
            {
                switch (theme)
                {
                    case "ShinyRed":
                        uri = "/TriageMaster;component/Themes/ShinyRed.xaml";
                        break;

                    case "ShinyBlue":
                        uri = "/TriageMaster;component/Themes/ShinyBlue.xaml";
                        break;

                    default:
                        uri = "/TriageMaster;component/Themes/ShinyBlue.xaml";
                        break;
                }

                Application.Current.Resources.MergedDictionaries.Clear();
                Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary()
                {
                    Source = new Uri(uri, UriKind.RelativeOrAbsolute)
                });
                this.lvAreaPathList.Foreground = Brushes.Black;
            }
            catch (Exception ex)
            {
                Log.CaptureException(ex);
                Log.LogFileWrite();
            }
        }

        private bool LinkWorkItemToAnotherWorkItem()
        {
            //Link one workitems to another workitems

            bool isLinked = false;
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
                    isLinked = true;
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
            }
            return isLinked;
        }

        private void UpdateMultipleWorkItem()
        {
            // Update multiple workitems 
            selectedAreaPathIndex = lvAreaPathList.SelectedIndex;
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
                                 bugOrDefectWorkItems = Helper.TfsWrapper.GetDefectOrBugWorkItems(queryString);
                                 Dispatcher.Invoke((Action)(() =>
                                 {
                                     isRefreshedOrSaved = true;
                                     LoadRelatedBugOrDefect();
                                 }
                                 ));
                             }
                            ).ContinueWith((task1) =>
                            {
                                lvAreaPathList.Focus();
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
            isRefreshedOrSaved = false;
        }

        void multipleWorkItemDetailsUpdateWindow_IsUpdateWindowLoaded(object sender, bool isLoad)
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
            btnGetWorkItems.IsEnabled = isEnabled;
            //btnLoadFromFile.IsEnabled = isEnabled;
            //btnSettings.IsEnabled = isEnabled;
            //btnAbout.IsEnabled = isEnabled;

            btnExport.IsEnabled = isEnabled; 
            btnLinkItems.IsEnabled = isEnabled; 
            btnUpdateWorkItemDetails.IsEnabled = isEnabled;
            btnSaveWorkItemDetails.IsEnabled = isEnabled;
            btnRefreshWorkItems.IsEnabled = isEnabled; 
            btnFindRelatedBugs.IsEnabled = isEnabled;

            cmFindRelatedBugs.IsEnabled = false;
            cmLinkToExistingItem.IsEnabled = false;
            cmUpdateWorkItemDetails.IsEnabled = false;
        }

        private void OpenFindRelatedBugWindow()
        {
            try
            {
                this.ProgressBar.IsBusy = true;
                this.ProgressBar.BusyContent = MessageNotification.LoadingFindRelatedBugOrDefectWinow;

                SetButtonStatus(false);

                TFSDefectOrBugWorkItem selectedRow = ((TFSDefectOrBugWorkItem)dgWIs.SelectedItem);

                relatedBugWindow = new FindingRelatedBugWindow();

                Task.Factory.StartNew(() =>
                {
                    relatedBugWindow.BugOrDefectWorkItems = FindRelatedBugs(selectedRow);
                    relatedBugWindow.RelatedBugs += relatedBugWindow_RelatedBugs;
                    relatedBugWindow.RefreshWindow += relatedBugWindow_RefreshWindow;
                    relatedBugWindow.WindowLoaded += relatedBugWindow_WindowLoaded;
                    Dispatcher.Invoke((Action)(() =>
                    {
                        relatedBugWindow.ShowDialog();
                    }
                    ));
                }
                   ).ContinueWith((task) =>
                   {
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

        void relatedBugWindow_WindowLoaded(object sender, bool isLoaded)
        {
            if (isLoaded)
            {
                if (relatedBugWindow != null)
                {
                    this.ProgressBar.IsBusy = false;
                    dgWIs.Focus();
                }
            }
        }

        void relatedBugWindow_RefreshWindow(object sender, bool shouldRefresh)
        {
            btnRefreshWorkItems_Click(sender, null);            
        }

        void relatedBugWindow_RelatedBugs(object sender, bool shouldRefresh)
        {
            if (shouldRefresh)
            {
                if (relatedBugWindow != null)
                {
                    relatedBugWindow.BugOrDefectWorkItems = null;
                    TFSDefectOrBugWorkItem selectedRow = ((TFSDefectOrBugWorkItem)dgWIs.SelectedItem);
                    relatedBugWindow.BugOrDefectWorkItems = FindRelatedBugs(selectedRow);
                }
            }
        }

        private List<TFSDefectOrBugWorkItem> FindRelatedBugs(TFSDefectOrBugWorkItem selectedRow)
        {
            // Find realated bugs based on selected workitem
            List<TFSDefectOrBugWorkItem> selectedWorkItems = new List<TFSDefectOrBugWorkItem>();

            try
            {

                if (selectedRow == null)
                    return null;

                List<TFSDefectOrBugWorkItem> allWorkItems = null;

                if (Helper.TFSConnectionSettings.ShowBugOrDefectByAreaPath.ToString().ToLower() == "true" && 
                    Helper.TFSConnectionSettings.ShowBugOrDefectByIterationPath.ToString().ToLower() == "true")
                {
                    allWorkItems = Helper.TfsWrapper.GetWorkItemsBasedOnAreaOrIterationPath(Helper.TFSConnectionSettings.DefectWorkItemType.ToString(), selectedRow.AreaPath, selectedRow.IterationPath);
                }
                else if (Helper.TFSConnectionSettings.ShowBugOrDefectByAreaPath.ToString().ToLower() == "true" &&
                         Helper.TFSConnectionSettings.ShowBugOrDefectByIterationPath.ToString().ToLower() == "false")
                {
                    allWorkItems = Helper.TfsWrapper.GetWorkItemsBasedOnAreaOrIterationPath(Helper.TFSConnectionSettings.DefectWorkItemType.ToString(), selectedRow.AreaPath, string.Empty);
                }
                else if (Helper.TFSConnectionSettings.ShowBugOrDefectByAreaPath.ToString().ToLower() == "false" &&
                    Helper.TFSConnectionSettings.ShowBugOrDefectByIterationPath.ToString().ToLower() == "true")
                {
                    allWorkItems = Helper.TfsWrapper.GetWorkItemsBasedOnAreaOrIterationPath(Helper.TFSConnectionSettings.DefectWorkItemType.ToString(), string.Empty, selectedRow.IterationPath);
                }
                else
                {
                    allWorkItems = Helper.TfsWrapper.GetWorkItemsBasedOnAreaOrIterationPath(Helper.TFSConnectionSettings.DefectWorkItemType.ToString(), string.Empty, string.Empty);
                }

                if (allWorkItems == null)
                {
                    return allWorkItems;
                }

                List<FuzzyStringComparisonOptions> options = new List<FuzzyStringComparisonOptions>();

                options.Add(FuzzyStringComparisonOptions.UseJaccardDistance);
                options.Add(FuzzyStringComparisonOptions.UseHammingDistance);
                options.Add(FuzzyStringComparisonOptions.UseLongestCommonSubsequence);
                options.Add(FuzzyStringComparisonOptions.UseLongestCommonSubstring);

                foreach (TFSDefectOrBugWorkItem workItem in allWorkItems.Where(w => !string.IsNullOrEmpty(w.Title)))
                {
                    int matchPercentage = selectedRow.Title.ApproximatelyEquals(workItem.Title, options.ToArray());

                    if (matchPercentage >= Helper.WeakMatch)
                    {
                        workItem.Matching = matchPercentage.ToString();
                        selectedWorkItems.Add(workItem);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.CaptureException(ex);
                Log.LogFileWrite();
            }
            return selectedWorkItems;
        }        

        #endregion

        #region [Events]

        private void btnAbout_Click(object sender, RoutedEventArgs e)
        {
            new About().ShowDialog();
        }

        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            TFSSettings settingWindow = new TFSSettings("MainWindow");
            settingWindow.ShowDialog();
        }        

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                ChangeApplicationTheme(Helper.TFSConnectionSettings.Theme);

                isTriageMasterLoadedFirstTime = true;

                cmbQueries.ItemsSource = null;
                // Bind all the queries to comobox
                cmbQueries.ItemsSource = Helper.TfsQueries;

                cmbQueries.DisplayMemberPath = "QueryName";
                cmbQueries.SelectedValuePath = "QueryText";

                cmbQueries.SelectedIndex = 0;
                cmbQueries.Focus();

                isTriageMasterLoadedFirstTime = false;
            }
            catch (Exception ex)
            {
                Log.CaptureException(ex);
                Log.LogFileWrite();
            }
        }

        private void lvAreaPathList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (lvAreaPathList.SelectedValue == null || isTriageMasterLoadedFirstTime)
                    return;
                
                string selectedAreaPath = lvAreaPathList.SelectedValue.ToString();

                if (selectedAreaPath == "All")
                {
                    dgWIs.ItemsSource = null;
                    dgWIs.ItemsSource = bugOrDefectWorkItems;

                    lblMessage.Content = string.Format("{0} " + MessageNotification.WorkItemPresent, bugOrDefectWorkItems.Count);
                    lblMessage.Foreground = Brushes.Green;
                }
                else
                {
                    dgWIs.ItemsSource = null;
                    List<TFSDefectOrBugWorkItem> queryAttributes = bugOrDefectWorkItems.Where(q => q.AreaPath == selectedAreaPath).ToList<TFSDefectOrBugWorkItem>();
                    dgWIs.ItemsSource = queryAttributes;

                    lblMessage.Content = string.Format("{0} " + MessageNotification.WorkItemPresent, queryAttributes.Count);
                    lblMessage.Foreground = Brushes.Green;                   
                }
                if (!isRefreshedOrSaved)
                {
                    dgWIs.SelectedIndex = 0;
                }
                else
                {
                    if (lvAreaPathList.SelectedIndex != selectedAreaPathIndex)
                    {
                        dgWIs.SelectedIndex = 0;
                    }
                    else
                    {
                        dgWIs.SelectedIndex = selectedWorkItemIndex;
                    }
                }

                dgWIs.Focus();
                this.ProgressBar.IsBusy = false;

            }
            catch (Exception ex)
            {
                Log.CaptureException(ex);
                Log.LogFileWrite();
            }
        }

        private void cmbQueries_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (isTriageMasterLoadedFirstTime)
                {
                    return;
                }

                lblMessage.Content = string.Empty;

                btnGetWorkItems.IsEnabled = cmbQueries.SelectedIndex > 0;

                if (cmbQueries.SelectedIndex == 0)
                {
                    this.witControl.Content = null;
                    dgWIs.ItemsSource = lvAreaPathList.ItemsSource = bugOrDefectWorkItems = null;
                    SetButtonStatus(false);                    
                }
            }
            catch (Exception ex)
            {
                Log.CaptureException(ex);
                Log.LogFileWrite();
            }
        }

        private void btnGetWorkItems_Click(object sender, RoutedEventArgs e)
        {
            this.ProgressBar.IsBusy = true;
            this.ProgressBar.BusyContent = MessageNotification.Fetching;

            try
            {
                SetButtonStatus(false);

                dgWIs.ItemsSource = null;
                lvAreaPathList.ItemsSource = null;
                this.witControl.WorkItem = null;
                this.lblMessage.Content = string.Empty;

                isTriageMasterLoadedFirstTime = true;

                queryString = GetQueryString(cmbQueries.SelectedValue.ToString());

                Task.Factory.StartNew(() =>
                    {
                        bugOrDefectWorkItems = Helper.TfsWrapper.GetDefectOrBugWorkItems(queryString);
                        Dispatcher.Invoke((Action)(() =>
                            {
                                LoadRelatedBugOrDefect();
                            }
                        ));
                    }
                   ).ContinueWith((task) =>
                    {
                        lvAreaPathList.Focus();
                        dgWIs.Focus();

                        isTriageMasterLoadedFirstTime = false;
                        this.ProgressBar.IsBusy = false;
                    }, TaskScheduler.FromCurrentSynchronizationContext()
                );
            }
            catch (Exception ex)
            {
                Log.CaptureException(ex);
                Log.LogFileWrite();
            }
        }

        private string GetQueryString(string queryString)
        {
           return cmbQueries.SelectedValue.ToString();
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            // Exports all the data to excel sheet.
            try
            {
                ExportToExcel<ExcelSheetHeaderName, ExcelSheetHeaderNames> exportToExcel = new ExportToExcel<ExcelSheetHeaderName, ExcelSheetHeaderNames>();

                List<ExcelSheetHeaderName> datasForExcelSheet = new List<ExcelSheetHeaderName>();

                foreach (TFSDefectOrBugWorkItem item in (List<TFSDefectOrBugWorkItem>)dgWIs.ItemsSource)
                {
                    datasForExcelSheet.Add(
                        new ExcelSheetHeaderName()
                   {
                       Id = item.Id,
                       Title = item.Title,
                       State = item.State,
                       AreaPath = item.AreaPath,
                       IterationPath = item.IterationPath,
                       CreatedDate = item.CreatedDate,
                       Severity = item.Severity,
                       Priority = item.Priority,
                       AssignedTo = item.AssignedTo                       
                   }
                  );
                }
                exportToExcel.dataToPrint = datasForExcelSheet;
                exportToExcel.GenerateReport();
            }
            catch (Exception ex)
            {
                Log.CaptureException(ex);
                Log.LogFileWrite();
            }
        }

        private void dgWIs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (dgWIs.SelectedItem == null)
                {
                    return;
                }

                if (dgWIs.SelectedItems.Count > 1)
                {
                    btnFindRelatedBugs.IsEnabled = false;
                }
                else
                {
                    btnFindRelatedBugs.IsEnabled = true;
                }

                TFSDefectOrBugWorkItem selectedWorkItem = (TFSDefectOrBugWorkItem)dgWIs.SelectedItem;

                this.witControl.WorkItem = selectedWorkItem.WorkItemObj;
                
            }
            catch (Exception ex)
            {               
                Log.CaptureException(ex);
                Log.LogFileWrite();
            }
        }

        private void cmLinkToExistingItem_Click(object sender, RoutedEventArgs e)
        {
            // Link work item to another work item.
            try
            {
                selectedAreaPathIndex = lvAreaPathList.SelectedIndex;
                selectedWorkItemIndex = dgWIs.SelectedIndex;

                if (LinkWorkItemToAnotherWorkItem())
                {
                    this.ProgressBar.IsBusy = true;
                    this.ProgressBar.BusyContent = MessageNotification.Fetching;

                    Task.Factory.StartNew(() =>
                    {
                        bugOrDefectWorkItems = Helper.TfsWrapper.GetDefectOrBugWorkItems(queryString);
                        Dispatcher.Invoke((Action)(() =>
                        {
                            LoadRelatedBugOrDefect();
                        }
                        ));
                    }
                       ).ContinueWith((task) =>
                       {
                           lvAreaPathList.Focus();
                           dgWIs.Focus();
                           this.ProgressBar.IsBusy = false;
                       }, TaskScheduler.FromCurrentSynchronizationContext()
                    );
                }
            }
            catch (Exception ex)
            {
                Log.CaptureException(ex);
                Log.LogFileWrite();
            }
        }

        private void btnSaveWorkItemDetails_Click(object sender, RoutedEventArgs e)
        {
            // Save selected work item details
            try
            {
                this.ProgressBar.IsBusy = true;
                this.ProgressBar.BusyContent = MessageNotification.Saving;

                SetButtonStatus(false);

                selectedAreaPathIndex = lvAreaPathList.SelectedIndex;
                selectedWorkItemIndex = dgWIs.SelectedIndex;

                string triageComment = string.Empty;
                if (!string.IsNullOrEmpty(txtTriageComment.Text))
                {
                    triageComment = txtTriageComment.Text.Trim();
                }

                Task.Factory.StartNew(() =>
                {
                    if (this.witControl.SaveWorkItemDetails(triageComment))
                    { 
                        bugOrDefectWorkItems = Helper.TfsWrapper.GetDefectOrBugWorkItems(queryString);
                        Dispatcher.Invoke((Action)(() =>
                        {
                            txtTriageComment.Clear();
                            isRefreshedOrSaved = true;
                            LoadRelatedBugOrDefect();
                        }
                        ));
                    }
                }
                   ).ContinueWith((task) =>
                   {
                       lvAreaPathList.Focus();
                       dgWIs.Focus();
                       this.ProgressBar.IsBusy = false;
                   }, TaskScheduler.FromCurrentSynchronizationContext()
                );
                isRefreshedOrSaved = false;
            }
            catch (Exception ex)
            {
                Log.CaptureException(ex);
                Log.LogFileWrite();
            }
        }

        private void cmUpdateWorkItemDetails_Click(object sender, RoutedEventArgs e)
        {
            UpdateMultipleWorkItem();
        }

        private void btnLinkItems_Click(object sender, RoutedEventArgs e)
        {
            // Link One or more work items to a single work item
            try
            {
                selectedAreaPathIndex = lvAreaPathList.SelectedIndex;
                selectedWorkItemIndex = dgWIs.SelectedIndex;

                if (LinkWorkItemToAnotherWorkItem())
                {
                    this.ProgressBar.IsBusy = true;
                    this.ProgressBar.BusyContent = MessageNotification.Fetching;

                    Task.Factory.StartNew(() =>
                    {
                        bugOrDefectWorkItems = Helper.TfsWrapper.GetDefectOrBugWorkItems(queryString);
                        Dispatcher.Invoke((Action)(() =>
                        {
                            LoadRelatedBugOrDefect();
                        }
                        ));
                    }
                       ).ContinueWith((task) =>
                       {
                           lvAreaPathList.Focus();
                           dgWIs.Focus();
                           this.ProgressBar.IsBusy = false;
                       }, TaskScheduler.FromCurrentSynchronizationContext()
                    );
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
            // Update mutltiple work items.
            UpdateMultipleWorkItem();
        }

        private void btnRefreshWorkItems_Click(object sender, RoutedEventArgs e)
        {
            // Refresh work items
            try
            {
                this.ProgressBar.IsBusy = true;
                this.ProgressBar.BusyContent = MessageNotification.Fetching;

                selectedAreaPathIndex = lvAreaPathList.SelectedIndex;
                selectedWorkItemIndex = dgWIs.SelectedIndex;

                this.witControl.Content = null;
                dgWIs.ItemsSource = lvAreaPathList.ItemsSource = bugOrDefectWorkItems = null;
                this.lblMessage.Content = string.Empty;
                SetButtonStatus(false);

                Task.Factory.StartNew(() =>
                {
                    bugOrDefectWorkItems = Helper.TfsWrapper.GetDefectOrBugWorkItems(queryString);
                    Dispatcher.Invoke((Action)(() =>
                    {
                        isRefreshedOrSaved = true;
                        LoadRelatedBugOrDefect();
                    }
                    ));
                }
                  ).ContinueWith((task) =>
                  {
                      lvAreaPathList.Focus();
                      dgWIs.Focus();
                      this.ProgressBar.IsBusy = false;
                  }, TaskScheduler.FromCurrentSynchronizationContext()
               );

                isRefreshedOrSaved = false;
            }
            catch (Exception ex)
            {
                Log.CaptureException(ex);
                Log.LogFileWrite();
            }
        }

        private void btnFindRelatedBugs_Click(object sender, RoutedEventArgs e)
        {
            OpenFindRelatedBugWindow();
        }

        private void cmFindRelatedBugs_Click(object sender, RoutedEventArgs e)
        {
            OpenFindRelatedBugWindow();
        }

        private void dgWIs_MouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            cmLinkToExistingItem.IsEnabled = false; 
            cmUpdateWorkItemDetails.IsEnabled = false; 
            cmFindRelatedBugs.IsEnabled = false;

            if (dgWIs.ItemsSource == null)
            {
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

                    if (dgWIs.SelectedItems.Count == 1)
                    {
                        cmFindRelatedBugs.IsEnabled = true;
                    }
                }
            }
            catch (Exception ex)
            {             
                Log.CaptureException(ex);
                Log.LogFileWrite();
            }
        }        

        private void btnLoadFromFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Configure open file dialog box
                OpenFileDialog fileDialog = new OpenFileDialog();
                fileDialog.DefaultExt = ".wiq";
                fileDialog.Filter = "Work Item Query Files | *.wiq";
                Nullable<bool> result = fileDialog.ShowDialog();

                #region Process open file dialog box results

                if (result == true)
                {
                    string filename = fileDialog.FileName;
                    using (StreamReader reader = new System.IO.StreamReader(filename))
                    {
                        TFSQueryItem queryItem = new TFSQueryItem();
                        queryItem.QueryName = "Custom Query:: " + filename;
                        string xmlString = reader.ReadToEnd();
                        if (!string.IsNullOrEmpty(xmlString))
                        {
                            XmlDocument queryDoc = new XmlDocument();
                            queryDoc.LoadXml(xmlString);
                            var queryNode = queryDoc.GetElementsByTagName("Wiql");
                            if (queryNode.Count > 0 && queryNode[0].InnerText.ToLower().Contains(Helper.TFSConnectionSettings.DefectWorkItemType.ToLower()))
                            {
                                queryItem.QueryText = queryNode[0].InnerText;
                                //Add the Custom Query Item to the DataSource for Combo Box
                                List<TFSQueryItem> itemList = (List<TFSQueryItem>)cmbQueries.ItemsSource;
                                //Insert the new query Item only if it is not duplicate

                                foreach (TFSQueryItem item in itemList.Where(i => i.QueryName != queryItem.QueryText))
                                {
                                    itemList.Insert(itemList.Count, queryItem);
                                    break;
                                }

                                //rebind the ComboBox
                                cmbQueries.ItemsSource = null;
                                cmbQueries.ItemsSource = itemList;
                                cmbQueries.DisplayMemberPath = "QueryName";
                                cmbQueries.SelectedValuePath = "QueryText";
                                // Set the Selected Item to the one that was added                            
                                cmbQueries.SelectedIndex = cmbQueries.Items.Count - 1;

                                btnGetWorkItems.IsEnabled = true;

                                lvAreaPathList.Focus();
                                dgWIs.Focus();
                            }
                            else
                            {
                                DialogBox.ShowWarning(string.Format(MessageNotification.QueryFileNotHaveBugOrDefect, Helper.TFSConnectionSettings.DefectWorkItemType.ToLower()));
                            }
                        }
                    }
                }

                #endregion
            }
            catch (Exception ex)
            {
                Log.CaptureException(ex);
                Log.LogFileWrite();
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {         
            relatedBugWindow = null;
            bugOrDefectWorkItems = null;
        }

        #endregion
    }
}
