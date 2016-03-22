using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TriageMaster.Windows;

namespace TriageMaster.Common
{
    public class Log
    {
        #region [Local Variables]

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
                    string logFilePath = Path.Combine(Log.LogPath, Helper.LOGFILENAME);

                    FileInfo logFileInfo = new FileInfo(logFilePath);                   
                    fileStream = new FileStream(logFilePath, FileMode.Append,FileAccess.Write);
                    streamWriter = new StreamWriter(fileStream);
                    streamWriter.WriteLine("-------------------------------------------------------------------------" + Environment.NewLine + msg);


                    if (streamWriter != null) streamWriter.Close();
                    if (fileStream != null) fileStream.Close();

                    DisplayMessage(Helper.LOG_WRITTEN);
                }
                catch (Exception)
                {
                    DisplayMessage(Helper.LOG_WRITTEN);
                }
            }
            msg = string.Empty;
        }


        private static void DisplayMessage(string message)
        {
            DialogBox.ShowError(message);
        }

        #endregion
    }
}
