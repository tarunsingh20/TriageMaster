using System;
using System.Collections.Generic;
using System.Drawing;
namespace TriageMaster.Common
{
   public class ExcelSheetHeaderNames : List<ExcelSheetHeaderName> { }

    public class ExcelSheetHeaderName
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string State { get; set; }
        public string AreaPath { get; set; }
        public string IterationPath { get; set; }
        public string CreatedDate { get; set; }
        public string Severity { get; set; }
        public string Priority { get; set; }
        public string AssignedTo { get; set; }
    }
}
