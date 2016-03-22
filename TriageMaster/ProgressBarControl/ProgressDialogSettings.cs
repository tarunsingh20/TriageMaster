using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TriageMaster.ProgressBarControl
{
    public class ProgressDialogSettings
    {
        public static ProgressDialogSettings WithLabelOnly = new ProgressDialogSettings(false, true);
        public static ProgressDialogSettings WithSubLabel = new ProgressDialogSettings(true, true);

        public bool ShowSubLabel { get; set; }

        public bool ShowProgressBarIndeterminate { get; set; }

        public ProgressDialogSettings()
        {
            ShowSubLabel = false;

            ShowProgressBarIndeterminate = true;
        }

        public ProgressDialogSettings(bool showSubLabel, bool showProgressBarIndeterminate)
        {
            ShowSubLabel = showSubLabel;

            ShowProgressBarIndeterminate = showProgressBarIndeterminate;
        }
    }
}
