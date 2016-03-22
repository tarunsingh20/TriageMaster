using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TraigeMaster;

namespace STaRZ.Logger
{
    public class Log
    {
        #region [ Variables ]
        private static string msg = string.Empty;
        private static bool hasExceptionOccured = false;
        #endregion

        #region [ Properties ]
        public static string LogPath
        {
            get
            {
                return Path.GetTempPath();
            }
        }
        #endregion

        #region [ Public Methods ]
        public static void CaptureException(string message)
        {
            msg = msg + message;
            hasExceptionOccured = true;
        }
        public static void CaptureException(Exception exception)
        {
            msg = msg + "Time stamp : " + DateTime.Now.ToString() + Environment.NewLine + "Exception : " + exception.Message + Environment.NewLine +
                "Exception : " + exception.StackTrace + Environment.NewLine + Environment.NewLine + Environment.NewLine;
            hasExceptionOccured = true;
        }

        public static bool ExceptionOccured
        {
            get
            {
                return hasExceptionOccured;
            }
            set
            {
                hasExceptionOccured = value;
            }
        }
        public static void LogFileWrite()
        {
            if (ExceptionOccured)
            {
                FileStream fileStream = null;
                StreamWriter streamWriter = null;
                try
                {
                    string logFilePath = Path.Combine(Log.LogPath, "TraigeMasterErrorLog.txt");
                    FileInfo logFileInfo = new FileInfo(logFilePath);
                    fileStream = new FileStream(logFilePath, FileMode.Create);
                    streamWriter = new StreamWriter(fileStream);
                    streamWriter.WriteLine(msg);
                    if (streamWriter != null) streamWriter.Close();
                    if (fileStream != null) fileStream.Close();

                    DisplayMessage("Info", "Log has been written at User's temp folder." +
                                            Environment.NewLine +
                                            "To view the log file, goto Run -> %temp% -> TraigeMasterErrorLog.txt");
                }
                catch (Exception)
                {
                    DisplayMessage("Error info", "Could not Write log file in Temp Folder");
                }
            }
            msg = string.Empty;
        }


        private static void DisplayMessage(string caption, string message)
        {
            DialogBox.ShowInfo(message);
        }

        #endregion
    }
}
