using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using STaRZ.TFSLibrary;
using TriageMaster.Common;

namespace TriageMaster.Windows
{
    /// <summary>
    /// Interaction logic for MultipleWorkItemDetailsUpdateWindow.xaml
    /// </summary>
    public partial class MultipleWorkItemDetailsUpdateWindow : Window
    {
        #region [Properties]

        private List<int> workItemIDs;

        public List<int> WorkItemIDs
        {
            get { return workItemIDs; }
            set { workItemIDs = value; }
        }

        private bool loadMultipleWorkItemWIndow;

        public bool LoadMultipleWorkItemWIndow
        {
            get { return loadMultipleWorkItemWIndow; }
            set
            {
                loadMultipleWorkItemWIndow = value;

                if (loadMultipleWorkItemWIndow)
                {
                    LoadFieldAllowedValuesToMultipleWorkItemWindow();
                }
            }
        }
        

        #endregion

        #region [Local Variables]

        string workItemList = string.Empty;
        List<TreeModel> areaPathStructure = null;
        List<TreeModel> iterationPathStructure = null;

        AllowedValuesForMultipleUpdateWindow allowedFieldValues = null;

        #endregion

        #region [Public Events]

        public event UpdateWindowHandler IsUpdateWindowLoaded;
        public delegate void UpdateWindowHandler(object sender, bool isLoad);
        List<string> areaHierarchialPath = null;
        List<string> iterationHierarchialPath = null;

        int itemSourceCount = -1;
        #endregion

        #region [Constructors]

        public MultipleWorkItemDetailsUpdateWindow()
        {
            InitializeComponent();

            if (areaHierarchialPath == null)
            {
                areaHierarchialPath = new List<string>();
            }

            if (iterationHierarchialPath == null)
            {
                iterationHierarchialPath = new List<string>();
        }
        }

        #endregion

        #region [Local Methods]        

        private List<TreeModel> BuildAreaTree(IEnumerable<string> areaPaths, int depth = 0)
        {
            return (
                     from s in areaPaths
                     where s.Length > 1
                     let split = s.Split('\\')
                     group s by s.Split('\\')[0] into g
                     select new TreeModel
                     {
                         CompletePath = GetAreaPathOrIterationPath(g.Key, ref depth, Helper.TreeViewIterateMode.AreaPath),
                         Name = g.Key,
                         Children = BuildAreaTree(            // Recursively build children
                         from s in g
                         where s.Length > g.Key.Length + 1
                         select s.Substring(g.Key.Length + 1), depth) // Select remaining components
                     }
                    ).ToList();
        }
        
        private List<TreeModel> BuildIterationTree(IEnumerable<string> iterationPaths, int depth = 0)
        {
            return (
                     from s in iterationPaths
                     where s.Length > 1
                     let split = s.Split('\\')
                     group s by s.Split('\\')[0] into g
                     select new TreeModel
                     {
                        
                         CompletePath = GetAreaPathOrIterationPath(g.Key, ref depth, Helper.TreeViewIterateMode.IterationPath),
                         Name = g.Key,
                         Children = BuildIterationTree(            // Recursively build children
                         from s in g
                         where s.Length > g.Key.Length + 1
                         select s.Substring(g.Key.Length + 1), depth) // Select remaining components
                     }
                    ).ToList();
        }

        private string GetAreaPathOrIterationPath(string name, ref int depth, Helper.TreeViewIterateMode iterateMode)
        {
            string completePath = string.Empty;

            if (iterateMode == Helper.TreeViewIterateMode.AreaPath)
            {
                string[] splits = areaHierarchialPath[itemSourceCount].Split('\\');

                if (depth >= splits.Length)
                {
                    depth = depth - 1;
                }

                if (splits[depth] == name)
                {
                    completePath = areaHierarchialPath[itemSourceCount].ToString();

                    if (itemSourceCount + 1 == areaHierarchialPath.Count)
                    {
                        return completePath;
                    }

                    string[] innersplits = areaHierarchialPath[itemSourceCount + 1].Split('\\');

                    if (innersplits.Length > splits.Length)
                    {
                        depth = depth + 1;
                    }
                    else if (innersplits.Length < splits.Length)
                    {
                        depth = depth - 1;
                    }
                }
            }
            else if (iterateMode == Helper.TreeViewIterateMode.IterationPath)
            {
                string[] splits = iterationHierarchialPath[itemSourceCount].Split('\\');

                if (depth >= splits.Length)
                {
                    depth = depth - 1;
                }

                if (splits[depth] == name)
                {
                    completePath = iterationHierarchialPath[itemSourceCount].ToString();

                    if (itemSourceCount + 1 == iterationHierarchialPath.Count)
                    {
                        return completePath;
                    }

                    string[] innersplits = iterationHierarchialPath[itemSourceCount + 1].Split('\\');

                    if (innersplits.Length > splits.Length)
                    {
                        depth = depth + 1;
                    }
                    else if (innersplits.Length < splits.Length)
                    {
                        depth = depth - 1;
                    }
                }
            }

            itemSourceCount++;
            return completePath;
        }

        private bool LoadFieldAllowedValuesToMultipleWorkItemWindow()
        {
            try
            {
                string ids = string.Empty;

                foreach (int id in WorkItemIDs)
                {
                    ids += id.ToString().Trim();
                    if (WorkItemIDs.Count > 1)
                        if (WorkItemIDs.IndexOf(id) != WorkItemIDs.Count - 1)
                            ids += ", ";
                }

                workItemList = string.Format(MessageNotification.BugOrDefectWorkItemIDs + "{0}", ids);

                allowedFieldValues = Helper.TfsWrapper.LoadFieldTypeCollection(WorkItemIDs[0], Helper.TFSConnectionSettings.DefectWorkItemType);


                areaHierarchialPath = Helper.TfsWrapper.GetAreaPathOrIterationPath(Helper.TFSFieldType.AreaPath.ToString());
                itemSourceCount = 0;
                areaPathStructure = BuildAreaTree(areaHierarchialPath);
                itemSourceCount = -1;

                iterationHierarchialPath = Helper.TfsWrapper.GetAreaPathOrIterationPath(Helper.TFSFieldType.IterationPath.ToString());
                itemSourceCount = 0;
                iterationPathStructure = BuildIterationTree(iterationHierarchialPath);
                itemSourceCount = -1;
            }
            catch (Exception ex)
            {                
                Log.CaptureException(ex);
                Log.LogFileWrite();
            }

            return true;
        }

        private void ReleaseResources()
        {
            WorkItemIDs = null;
            areaHierarchialPath = null;
            iterationHierarchialPath = null;
            IsUpdateWindowLoaded = null;

            workItemList = string.Empty;
            areaPathStructure = null;
            iterationPathStructure = null;
            allowedFieldValues = null;
        }

        #endregion

        #region [Events]

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            //Saves all the attributes entered by the user
            if (cmbRootCause.Text.ToString() == string.Empty &&
               cmbAssignedTo.Text.ToString() == string.Empty &&
               cmbState.Text.ToString() == string.Empty &&
               cmbPriority.Text.ToString() == string.Empty &&
               cmbSeverity.Text.ToString() == string.Empty &&
               txtArea.Text.ToString() == string.Empty &&
               txtIterationPath.Text.ToString() == string.Empty &&
               txtComment.Text.ToString() == string.Empty)
            {
                DialogBox.ShowWarning(MessageNotification.EveryFieldIsEmpty);
                return;
            }

            bool isWrongPathEntered = false;
            if (!string.IsNullOrEmpty(txtArea.Text))
            {
                if (!areaHierarchialPath.Contains(txtArea.Text.Trim()))
                {                    
                    isWrongPathEntered = true;
                }
            }
            if (!string.IsNullOrEmpty(txtIterationPath.Text))
            {
                if (!iterationHierarchialPath.Contains(txtIterationPath.Text.Trim()))
                {
                    isWrongPathEntered = true;
                }
            }

            if (isWrongPathEntered)
            {
                DialogBox.ShowWarning(MessageNotification.NotValidAreaPathOrIterationPath);
                return;
            }

            try
            {
                BatchSaveError[] errors = Helper.TfsWrapper.
                    SaveWorkItemDetails(WorkItemIDs,
                    new TFSDefectOrBugWorkItem()
                    {
                        RootCause = cmbRootCause.Text.ToString().Trim(),
                        AssignedTo = cmbAssignedTo.Text.ToString().Trim(),
                        State = cmbState.Text.ToString().Trim(),
                        AreaPath = txtArea.Text.ToString().Trim(),
                        IterationPath = txtIterationPath.Text.ToString().Trim(),
                        Priority = cmbPriority.Text.ToString().Trim(),
                        Severity = cmbSeverity.Text.ToString().Trim(),
                        Comment = txtComment.Text.ToString().Trim()
                    }
                    );
                if (errors.Count() == 0)
                {
                    DialogResult = true;
                }
                else
                {
                    string errorMessage = MessageNotification.ErrorMessage;
                    foreach (BatchSaveError error in errors)
                    {
                        errorMessage += Environment.NewLine + error.Exception.Message.ToString().Trim();
                    }
                    DialogBox.ShowError(errorMessage);
                }
            }
            catch (Exception ex)
            {
                Log.CaptureException(ex);
                Log.LogFileWrite();
            }
        }                

        private void tvAreaPaths_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            try
            {
                var trv = sender as System.Windows.Controls.TreeView;
                var trvItem = trv.SelectedItem as TreeModel;
                if (trvItem == null) return;

                txtArea.Text = trvItem.CompletePath.ToString();               
                txtArea.CaretIndex = txtArea.Text.Length;
                PopupArea.IsOpen = false;
            }
            catch (Exception ex)
            {
                Log.CaptureException(ex);
                Log.LogFileWrite();
            }
        }

        private void txtArea_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (PopupIteration.IsOpen || PopupArea.IsOpen)
            {
                PopupIteration.IsOpen = false;
                PopupArea.IsOpen = false;
                return;
            }
        }

        private void txtIterationPath_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (PopupIteration.IsOpen || PopupArea.IsOpen)
            {
                PopupIteration.IsOpen = false;
                PopupArea.IsOpen = false;
                return;
            }
        }

        private void tvIterationPath_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            try
            {
                var trv = sender as System.Windows.Controls.TreeView;
                var trvItem = trv.SelectedItem as TreeModel;
                if (trvItem == null) return;

                txtIterationPath.Text = trvItem.CompletePath.ToString();                
                txtIterationPath.CaretIndex = txtIterationPath.Text.Length;               
                PopupIteration.IsOpen = false;
            }
            catch (Exception ex)
            {
                Log.CaptureException(ex);
                Log.LogFileWrite();
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            ReleaseResources();
            this.Close();
        }

        private void tvAreaPaths_MouseUp(object sender, MouseButtonEventArgs e)
        {
            PopupArea.IsOpen = false;
        }

        private void tvIterationPaths_MouseUp(object sender, MouseButtonEventArgs e)
        {
            PopupIteration.IsOpen = false;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            ReleaseResources();
            this.Close();
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            //Loads all the attributes while loading MultipleWorkItemUpdateWindow.
            tbWorkItemIDs.Text = workItemList;

            if (allowedFieldValues != null)
            {
                cmbRootCause.ItemsSource = allowedFieldValues.RootCauseList;

                List<string> assignedList = allowedFieldValues.AssignedToList.Cast<string>().ToList();
                assignedList.Insert(0, TFSConstants.EmptyAssignedToName);
                cmbAssignedTo.ItemsSource = assignedList;

                cmbState.ItemsSource = allowedFieldValues.StateList;

                cmbPriority.ItemsSource = allowedFieldValues.PriorityList;
                cmbSeverity.ItemsSource = allowedFieldValues.SeverityList;
            }

            areaHierarchialPath = Helper.TfsWrapper.GetAreaPathOrIterationPath(Helper.TFSFieldType.AreaPath.ToString());
            itemSourceCount = 0;
            tvAreaPaths.ItemsSource = areaPathStructure;
            itemSourceCount = -1;

            iterationHierarchialPath = Helper.TfsWrapper.GetAreaPathOrIterationPath(Helper.TFSFieldType.IterationPath.ToString());
            itemSourceCount = 0;
            tvIterationPaths.ItemsSource = iterationPathStructure;
            itemSourceCount = -1;

            if (IsUpdateWindowLoaded != null)
            {
                IsUpdateWindowLoaded(null, true);
            }
        }
        
        private void btnOpenIterationPath_Click(object sender, RoutedEventArgs e)
        {
            bool isPopupWindowModeChanged = false;

            if (PopupIteration.IsOpen)
            {
                PopupIteration.IsOpen = false;
                isPopupWindowModeChanged = true;
            }

            if (PopupArea.IsOpen)
            {
                PopupArea.IsOpen = false;
                PopupIteration.IsOpen = true;
                isPopupWindowModeChanged = true;
            }

            if (isPopupWindowModeChanged)
            {
                return;
            }

            try
            {
                PopupIteration.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                PopupIteration.StaysOpen = true;
                PopupIteration.Height = tvAreaPaths.Height;
                PopupIteration.Width = txtArea.Width;
                PopupIteration.IsOpen = true;
            }
            catch (Exception ex)
            {
                Log.CaptureException(ex);
                Log.LogFileWrite();
            }
        }

        private void btnOpenAreaPath_Click(object sender, RoutedEventArgs e)
        {
            bool isPopupWindowModeChanged = false;

            if (PopupArea.IsOpen)
            {
                PopupArea.IsOpen = false;
                isPopupWindowModeChanged = true;
            }

            if (PopupIteration.IsOpen)
            {
                PopupIteration.IsOpen = false;
                PopupArea.IsOpen = true;
                isPopupWindowModeChanged = true;
            }

            if (isPopupWindowModeChanged)
            {
                return;
            }
            try
            {
                PopupArea.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                PopupArea.StaysOpen = true;
                PopupArea.Height = tvAreaPaths.Height;
                PopupArea.Width = txtArea.Width;
                PopupArea.IsOpen = true;
            }
            catch (Exception ex)
            {
                Log.CaptureException(ex);
                Log.LogFileWrite();
            }
        }

        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            PopupArea.IsOpen = false;
            PopupIteration.IsOpen = false;
        }

        #endregion   
    }
}
