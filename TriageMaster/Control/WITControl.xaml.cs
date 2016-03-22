using System.Windows.Controls;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.TeamFoundation.WorkItemTracking.WpfControls;
using TriageMaster.Common;

namespace TriageMaster.Control
{
    /// <summary>
    /// Interaction logic for WITControl.xaml
    /// </summary>
    public partial class WITControl : UserControl
    {
        #region [Local Variables]

        private WorkItemControl witControl;

        #endregion

        #region [Properties]

        private WorkItem _workItem;

        public WorkItem WorkItem
        {
            set
            {
                _workItem = value;

                witControl = new WorkItemControl();
                witControl.Item = _workItem;

                this.Content = witControl;
            }
        }

        #endregion

        #region [Constructors]

        public WITControl()
        {
            InitializeComponent();
        }

        #endregion

        #region [Public Methods]

        public bool SaveWorkItemDetails(string triageComment = "")
        {
            bool isSaved = false;
            try
            {
                if (witControl != null)
                {
                    WorkItem wi = witControl.Item;
                    if (triageComment != string.Empty)
                    {
                        wi.History = string.Format(Helper.TRIAGE_COMMENT, triageComment);
                    }
                    wi.Save();
                    isSaved = true;
                }
            }
            catch (System.Exception)
            {
                isSaved = false;
            }
            return isSaved;
        }

        #endregion
    }
}
