using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using TriageMaster.Common;

namespace TriageMaster.Windows
{
    /// <summary>
    /// Interaction logic for BugOrDefectLinkingWindow.xaml
    /// </summary>
    public partial class BugOrDefectLinkingWindow : Window
    {
        #region [Properties]

        private List<int> selectedWorkItemID;

        public List<int> SelectedWorkItemID
        {
            get { return selectedWorkItemID; }
            set { selectedWorkItemID = value; }
        }

        private List<string> selectedWorkItemTitle;

        public List<string> SelectedWorkItemTitle
        {
            get { return selectedWorkItemTitle; }
            set { selectedWorkItemTitle = value; }
        }

        #endregion

        #region [Constructor]

        public BugOrDefectLinkingWindow()
        {
            InitializeComponent();
        }

        #endregion

        #region [Events]

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            //Link workitems.
            try
            {
                if (SelectedWorkItemID.Count != 0 && !string.IsNullOrEmpty(txtWorkItemID.Text.Trim()))
                {
                    btnOK.IsEnabled = false;
                    string linkError = string.Empty;
                    bool IsSucessfullyLinked = Helper.TfsWrapper.LinkToAnExistingItem(cmbLinkType.SelectedItem.ToString(), SelectedWorkItemID, Convert.ToInt32(txtWorkItemID.Text), txtComment.Text.Trim(), out linkError);
                    if (IsSucessfullyLinked)
                    {
                        DialogResult = true;
                        SelectedWorkItemID = null;
                    }
                    else
                    {
                        txtLinkError.Text = linkError;
                    }
                }
            }
            catch (Exception ex)
            {  
                Log.CaptureException(ex);
                Log.LogFileWrite();
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            cmbLinkType.ItemsSource = null;
            cmbLinkType.ItemsSource = Helper.TfsWrapper.LinkTypes();
        }

        private void txtWorkItemID_LostFocus(object sender, RoutedEventArgs e)
        {
            Mouse.SetCursor(Cursors.Wait);
            btnOK.IsEnabled = false;

            try
            {
                if (!string.IsNullOrEmpty(txtWorkItemID.Text.Trim()))
                {
                    btnOK.IsEnabled = true;
                    if (!Helper.TfsWrapper.IsWorkItemExists(txtWorkItemID.Text.Trim(), ""))
                    {
                        txtLinkError.Text = MessageNotification.WorkitemNotExist;
                        txtTitle.Text = txtComment.Text = string.Empty;
                        btnOK.IsEnabled = false;
                        return;
                    }
                    else
                    {
                        if (SelectedWorkItemID.Count == 1 && Convert.ToInt32(txtWorkItemID.Text.Trim()) == SelectedWorkItemID.FirstOrDefault())
                        {
                            txtLinkError.Text = MessageNotification.WorkitemLinkToSelf;
                            txtTitle.Text = txtComment.Text = string.Empty;
                            btnOK.IsEnabled = false;
                            return;
                        }
                        else
                        {
                            string workItemLinkError = Helper.TfsWrapper.FindWorkItemRelationByID(cmbLinkType.SelectedItem.ToString().Trim(), SelectedWorkItemID, Convert.ToInt32(txtWorkItemID.Text.Trim()));

                            if (workItemLinkError != string.Empty)
                            {
                                txtLinkError.Text = workItemLinkError;
                                txtTitle.Text = txtComment.Text = string.Empty;
                                btnOK.IsEnabled = false;
                                return;
                            }
                            else
                            {
                                WorkItem wi = Helper.TfsWrapper.GetWorkItemDetailsByID(Convert.ToInt32(txtWorkItemID.Text.Trim()));
                                if (wi != null)
                                {
                                    txtTitle.Text = wi.Title;
                                }

                                txtLinkError.Text = string.Empty;
                                txtComment.Focus();
                                btnOK.IsEnabled = true;
                                return;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.CaptureException(ex);
                Log.LogFileWrite();
            }
            finally
            {
                Mouse.SetCursor(Cursors.Arrow);               
            }
        }

        private void txtWorkItemID_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text.Trim());
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            SelectedWorkItemID = null;
            selectedWorkItemTitle = null;
        }

        private void cmbLinkType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            txtWorkItemID_LostFocus(null, null);
        }        

        private void txtTitle_LostFocus(object sender, RoutedEventArgs e)
        {            
            Mouse.SetCursor(Cursors.Wait);
            btnOK.IsEnabled = false;

            try
            {
                if (!string.IsNullOrEmpty(txtTitle.Text.Trim()))
                {
                    btnOK.IsEnabled = true;
                    if (!Helper.TfsWrapper.IsWorkItemExists("", txtTitle.Text.Trim()))
                    {
                        txtLinkError.Text = MessageNotification.WorkitemNotExist;
                        txtWorkItemID.Text = txtComment.Text = string.Empty;
                        btnOK.IsEnabled = false;
                        return;
                    }
                    else
                    {
                        if (SelectedWorkItemTitle.Count == 1 && (txtTitle.Text.Trim()) == SelectedWorkItemTitle.FirstOrDefault().ToLower())
                        {
                            txtLinkError.Text = MessageNotification.WorkitemLinkToSelf;
                            txtWorkItemID.Text = txtComment.Text = string.Empty;
                            btnOK.IsEnabled = false;
                            return;
                        }
                        else
                        {

                            WorkItem wi = Helper.TfsWrapper.GetWorkItemDetailsByTitle(txtTitle.Text.Trim());
                            int workItemID = -1;

                            if (wi != null)
                            {
                                workItemID = wi.Id;
                            }

                            string workItemLinkError = Helper.TfsWrapper.FindWorkItemRelationByID(cmbLinkType.SelectedItem.ToString().Trim(), SelectedWorkItemID, workItemID);

                            if (workItemLinkError != string.Empty)
                            {
                                txtLinkError.Text = workItemLinkError;
                                txtWorkItemID.Text = txtComment.Text = string.Empty;
                                btnOK.IsEnabled = false;
                                return;
                            }
                            else
                            {
                                txtWorkItemID.Text = workItemID.ToString().Trim();
                                txtLinkError.Text = string.Empty;
                                txtComment.Focus();
                                btnOK.IsEnabled = true;
                                return;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.CaptureException(ex);
                Log.LogFileWrite();
            }
            finally
            {
                Mouse.SetCursor(Cursors.Arrow);                
            }
        }           

        private void txtWorkItemID_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtWorkItemID.Text == string.Empty)
            {
                txtComment.Text = string.Empty;
            }
        }

        private void txtTitle_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtTitle.Text == string.Empty)
            {
                txtComment.Text = string.Empty;
            }
        }

        #endregion
    }
}
