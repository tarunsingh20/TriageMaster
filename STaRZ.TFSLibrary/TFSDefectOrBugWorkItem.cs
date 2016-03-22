
using Microsoft.TeamFoundation.WorkItemTracking.Client;
namespace STaRZ.TFSLibrary
{
    public class TFSDefectOrBugWorkItem
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string AreaPath { get; set; }
        public string IterationPath { get; set; }
        public string State { get; set; }
        public string Severity { get; set; }
        public string Priority { get; set; }
        public string AssignedTo { get; set; }
        public string CreatedDate { get; set; }
        public string RootCause { get; set; }
        public string Comment { get; set; }
        public string Matching { get; set; }
        public WorkItem WorkItemObj {get;set;}
    }    
}
