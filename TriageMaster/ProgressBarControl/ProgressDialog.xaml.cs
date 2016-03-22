using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace TriageMaster.ProgressBarControl
{
    /// <summary>
    /// Interaction logic for ProgressDialog.xaml
    /// </summary>
    public partial class ProgressDialog : Window
    {
        volatile bool _isBusy;
        BackgroundWorker _worker;

        public string Label
        {
            get { return TextLabel.Text; }
            set { TextLabel.Text = value; }
        }

        public string SubLabel
        {
            get { return SubTextLabel.Text; }
            set { SubTextLabel.Text = value; }
        }

        internal ProgressDialogResult Result { get; private set; }

        public ProgressDialog(ProgressDialogSettings settings)
        {
            InitializeComponent();

            double top;
            double right = 0;

            if (settings == null)
                settings = ProgressDialogSettings.WithLabelOnly;

            if (settings.ShowSubLabel)
            {
                top = 38;
                Height = 110;
                SubTextLabel.Visibility = Visibility.Visible;
            }
            else
            {
                top = 22;
                Height = 100;
                SubTextLabel.Visibility = Visibility.Collapsed;
            }
          
            ProgressBar.Margin = new Thickness(0, top, right, 0);
            ProgressBar.IsIndeterminate = settings.ShowProgressBarIndeterminate;
        }

        internal ProgressDialogResult Execute(object operation)
        {
            if (operation == null)
                throw new ArgumentNullException("operation");

            ProgressDialogResult result = null;

            _isBusy = true;

            _worker = new BackgroundWorker();
            _worker.WorkerReportsProgress = true;
            _worker.WorkerSupportsCancellation = true;

            _worker.DoWork +=
                (s, e) =>
                {
                    if (operation is Action)
                        ((Action)operation)();
                    else if (operation is Action<BackgroundWorker>)
                        ((Action<BackgroundWorker>)operation)(s as BackgroundWorker);
                    else if (operation is Action<BackgroundWorker, DoWorkEventArgs>)
                        ((Action<BackgroundWorker, DoWorkEventArgs>)operation)(s as BackgroundWorker, e);
                    else if (operation is Func<object>)
                        e.Result = ((Func<object>)operation)();
                    else if (operation is Func<BackgroundWorker, object>)
                        e.Result = ((Func<BackgroundWorker, object>)operation)(s as BackgroundWorker);
                    else if (operation is Func<BackgroundWorker, DoWorkEventArgs, object>)
                        e.Result = ((Func<BackgroundWorker, DoWorkEventArgs, object>)operation)(s as BackgroundWorker, e);
                    else
                        throw new InvalidOperationException("Operation type is not supoorted");
                };

            _worker.RunWorkerCompleted +=
                (s, e) =>
                {
                    result = new ProgressDialogResult(e);
                    Dispatcher.BeginInvoke(DispatcherPriority.Send, (SendOrPostCallback)delegate
                    {
                        _isBusy = false;
                        Close();
                    }, null);
                };

            _worker.ProgressChanged +=
                (s, e) =>
                {
                    if (!_worker.CancellationPending)
                    {
                        SubLabel = (e.UserState as string) ?? string.Empty;
                        ProgressBar.Value = e.ProgressPercentage;
                    }
                };

            _worker.RunWorkerAsync();

            ShowDialog();

            return result;
        }
       
        void OnClosing(object sender, CancelEventArgs e)
        {
            e.Cancel = _isBusy;
        }

        internal static ProgressDialogResult Execute(Window owner, string label, Action operation)
        {
            return ExecuteInternal(owner, label, (object)operation, null);
        }

        internal static ProgressDialogResult Execute(Window owner, string label, Action operation, ProgressDialogSettings settings)
        {
            return ExecuteInternal(owner, label, (object)operation, settings);
        }

        internal static ProgressDialogResult Execute(Window owner, string label, Action<BackgroundWorker> operation)
        {
            return ExecuteInternal(owner, label, (object)operation, null);
        }

        internal static ProgressDialogResult Execute(Window owner, string label, Action<BackgroundWorker> operation, ProgressDialogSettings settings)
        {
            return ExecuteInternal(owner, label, (object)operation, settings);
        }

        internal static ProgressDialogResult Execute(Window owner, string label, Action<BackgroundWorker, DoWorkEventArgs> operation)
        {
            return ExecuteInternal(owner, label, (object)operation, null);
        }

        internal static ProgressDialogResult Execute(Window owner, string label, Action<BackgroundWorker, DoWorkEventArgs> operation, ProgressDialogSettings settings)
        {
            return ExecuteInternal(owner, label, (object)operation, settings);
        }

        internal static ProgressDialogResult Execute(Window owner, string label, Func<object> operationWithResult)
        {
            return ExecuteInternal(owner, label, (object)operationWithResult, null);
        }

        internal static ProgressDialogResult Execute(Window owner, string label, Func<object> operationWithResult, ProgressDialogSettings settings)
        {
            return ExecuteInternal(owner, label, (object)operationWithResult, settings);
        }

        internal static ProgressDialogResult Execute(Window owner, string label, Func<BackgroundWorker, object> operationWithResult)
        {
            return ExecuteInternal(owner, label, (object)operationWithResult, null);
        }

        internal static ProgressDialogResult Execute(Window owner, string label, Func<BackgroundWorker, object> operationWithResult, ProgressDialogSettings settings)
        {
            return ExecuteInternal(owner, label, (object)operationWithResult, settings);
        }

        internal static ProgressDialogResult Execute(Window owner, string label, Func<BackgroundWorker, DoWorkEventArgs, object> operationWithResult)
        {
            return ExecuteInternal(owner, label, (object)operationWithResult, null);
        }

        internal static ProgressDialogResult Execute(Window owner, string label, Func<BackgroundWorker, DoWorkEventArgs, object> operationWithResult, ProgressDialogSettings settings)
        {
            return ExecuteInternal(owner, label, (object)operationWithResult, settings);
        }

        internal static void Execute(Window owner, string label, Action operation, Action<ProgressDialogResult> successOperation, Action<ProgressDialogResult> failureOperation = null, Action<ProgressDialogResult> cancelledOperation = null)
        {
            ProgressDialogResult result = ExecuteInternal(owner, label, operation, null);

            if (result.Cancelled && cancelledOperation != null)
                cancelledOperation(result);
            else if (result.OperationFailed && failureOperation != null)
                failureOperation(result);
            else if (successOperation != null)
                successOperation(result);
        }

        internal static ProgressDialogResult ExecuteInternal(Window owner, string label, object operation, ProgressDialogSettings settings)
        {
            ProgressDialog dialog = new ProgressDialog(settings);
            dialog.Owner = owner;

            if (!string.IsNullOrEmpty(label))
                dialog.Label = label;

            return dialog.Execute(operation);
        }

        internal static bool CheckForPendingCancellation(BackgroundWorker worker, DoWorkEventArgs e)
        {
            if (worker.WorkerSupportsCancellation && worker.CancellationPending)
                e.Cancel = true;

            return e.Cancel;
        }

        internal static void Report(BackgroundWorker worker, string message)
        {
            if (worker.WorkerReportsProgress)
                worker.ReportProgress(0, message);
        }

        internal static void Report(BackgroundWorker worker, string format, params object[] arg)
        {
            if (worker.WorkerReportsProgress)
                worker.ReportProgress(0, string.Format(format, arg));
        }

        internal static void Report(BackgroundWorker worker, int percentProgress, string message)
        {
            if (worker.WorkerReportsProgress)
                worker.ReportProgress(percentProgress, message);
        }

        internal static void Report(BackgroundWorker worker, int percentProgress, string format, params object[] arg)
        {
            if (worker.WorkerReportsProgress)
                worker.ReportProgress(percentProgress, string.Format(format, arg));
        }

        internal static bool ReportWithCancellationCheck(BackgroundWorker worker, DoWorkEventArgs e, string message)
        {
            if (CheckForPendingCancellation(worker, e))
                return true;

            if (worker.WorkerReportsProgress)
                worker.ReportProgress(0, message);

            return false;
        }

        internal static bool ReportWithCancellationCheck(BackgroundWorker worker, DoWorkEventArgs e, string format, params object[] arg)
        {
            if (CheckForPendingCancellation(worker, e))
                return true;

            if (worker.WorkerReportsProgress)
                worker.ReportProgress(0, string.Format(format, arg));

            return false;
        }

        internal static bool ReportWithCancellationCheck(BackgroundWorker worker, DoWorkEventArgs e, int percentProgress, string message)
        {
            if (CheckForPendingCancellation(worker, e))
                return true;

            if (worker.WorkerReportsProgress)
                worker.ReportProgress(percentProgress, message);

            return false;
        }

        internal static bool ReportWithCancellationCheck(BackgroundWorker worker, DoWorkEventArgs e, int percentProgress, string format, params object[] arg)
        {
            if (CheckForPendingCancellation(worker, e))
                return true;

            if (worker.WorkerReportsProgress)
                worker.ReportProgress(percentProgress, string.Format(format, arg));

            return false;
        }
    }
}
