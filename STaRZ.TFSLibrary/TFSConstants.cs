using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STaRZ.TFSLibrary
{
    public class TFSConstants
    {
        public static class TFSQueryHeader
        {
            public const string QueryHeader = "Please select a Query from the list";
        }

        public static class MessageTypes
        {
            public const string Error = "Error";
            public const string Information = "Information";
            public const string Warning = "Warning";
        }

       public enum Classification
       {
           AreaPath,
           IterationPath
       }

        #region Constants

       public const string EmptyAssignedToName = "((    Clear value     ))";

        #endregion
    }
}
