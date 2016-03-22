using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace STaRZ.TFSLibrary
{
    public class AllowedValuesForMultipleUpdateWindow
    {
        public AllowedValuesCollection RootCauseList { get; set; }
        public AllowedValuesCollection AssignedToList { get; set; }
        public AllowedValuesCollection StateList { get; set; }
        public AllowedValuesCollection PriorityList { get; set; }
        public AllowedValuesCollection SeverityList { get; set; }
    }
}
