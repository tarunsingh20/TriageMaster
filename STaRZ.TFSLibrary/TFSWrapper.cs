using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using System.Xml.Linq;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Server;
using Microsoft.TeamFoundation.TestManagement.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace STaRZ.TFSLibrary
{
    public class TFSWrapper
    {
        #region [Local Variables]

        TfsTeamProjectCollection tfsTeamProjectCollection = null;
        string currentProjectName = string.Empty;

        ITestManagementTeamProject sourceProject;
        WorkItemStore workItemStore = null;

        #endregion

        #region [Properties]

        public string ConnectedUserDisplayName
        {
            get;
            set;
        }

        public bool TfsProjectExist
        {
            get;
            set;
        }

        public bool WorkItemStoreFound
        {
            get;
            set;
        }

        public bool TfsServerAuthenticated
        {
            get;
            set;
        }        

        public string WorkItemType { get; set; }

        #endregion

        #region [Constructor]
        
        public TFSWrapper(Uri tfsUri, NetworkCredential credential, string projectName)
        {
            currentProjectName = projectName;

            try
            {
                tfsTeamProjectCollection = new TfsTeamProjectCollection(tfsUri, credential);
                tfsTeamProjectCollection.EnsureAuthenticated();
                TfsServerAuthenticated = tfsTeamProjectCollection.HasAuthenticated;
            }
            catch (Exception)
            {
                TfsServerAuthenticated = false;
            }
            if (TfsServerAuthenticated == true)
            {
                //Connects with TFS server and fetch server and project details.
                workItemStore = (WorkItemStore)tfsTeamProjectCollection.GetService(typeof(WorkItemStore));

                WorkItemStoreFound = (workItemStore != null);

                if (WorkItemStoreFound)
                {
                    if (workItemStore.Projects.Contains(projectName))
                    {
                        TfsProjectExist = true;
                        ITestManagementService service = tfsTeamProjectCollection.GetService<ITestManagementService>();
                        sourceProject = service.GetTeamProject(projectName);
                    }
                }

                ConnectedUserDisplayName = tfsTeamProjectCollection.AuthorizedIdentity.DisplayName;
            }
        }

        #endregion

        #region [Public Methods]

        public List<TFSQueryItem> LoadQueries(string defectType)
        {
            try
            {
                List<TFSQueryItem> queries = null;
                if (TfsServerAuthenticated)
                {
                    if (queries == null)
                        queries = new List<TFSQueryItem>();

                    TFSQueryItem firstItem = new TFSQueryItem();
                    firstItem.QueryName = TFSConstants.TFSQueryHeader.QueryHeader;
                    firstItem.QueryText = "";
                    queries.Add(firstItem);

                    // return all the queries which contains DefectType "Bug/Defect".
                    foreach (ITestCaseQuery queryItem in sourceProject.Queries.Where(q => q.QueryText.ToLower().Contains(defectType.ToLower())))
                    {
                        TFSQueryItem item = new TFSQueryItem();
                        item.QueryName = queryItem.Name;
                        item.QueryText = queryItem.QueryText;
                        queries.Add(item);
                    }                   
                }
                return queries; 
            }
            catch (Exception)
            {
                return null;
            }
        }

        public List<TFSDefectOrBugWorkItem> GetDefectOrBugWorkItems(string queryText)
        {
            try
            {
                Query query = new Query(workItemStore, queryText, new Dictionary<string, string>() { { "project", currentProjectName } });
                var workItemCollection = query.RunQuery();               
                List<TFSDefectOrBugWorkItem> workItemAttributes = new List<TFSDefectOrBugWorkItem>();

                // retuns workitem fields for every work item that presents under a query.
                foreach (WorkItem Item in workItemCollection)
                {

                    TFSDefectOrBugWorkItem queryItemAttribute = new TFSDefectOrBugWorkItem();

                    queryItemAttribute.Id = Item.Id.ToString();
                    queryItemAttribute.Title = Item.Title;
                    queryItemAttribute.AreaPath = Item.AreaPath;
                    queryItemAttribute.IterationPath = Item.IterationPath;
                    queryItemAttribute.State = Item.State;

                    if (Item.Fields.Contains("Severity"))
                    {
                        queryItemAttribute.Severity = Item.Fields["Severity"].Value != null ? Item.Fields["Severity"].Value.ToString() : string.Empty;
                    }

                    if (Item.Fields.Contains("Priority"))
                    {
                        queryItemAttribute.Priority = Item.Fields["Priority"].Value != null ? Item.Fields["Priority"].Value.ToString() : string.Empty;
                    }
                    queryItemAttribute.AssignedTo = Item.Fields["Assigned To"].Value.ToString();
                    queryItemAttribute.CreatedDate = string.Format("{0:MM/dd/yyyy}", Item.CreatedDate);
                    queryItemAttribute.WorkItemObj = Item;

                    workItemAttributes.Add(queryItemAttribute);
                }
                return workItemAttributes; 
            }
            catch (Exception ex)
            {                
                throw;
                // If the Query is not formed properly, then just return an empty list of defect/bug.
                //return new List<TFSDefectOrBugWorkItem>();
            }
        }

        public List<TFSDefectOrBugWorkItem> GetWorkItemsBasedOnAreaOrIterationPath(string workItemType, string areaPath = "", string iterationPath = "")
        {
            string queryText = string.Empty;

            try
            {
                //WorkItemStore workItem = workItemStore;               
                Query query = null;

                queryText = "SELECT * FROM WorkItems WHERE [System.TeamProject] = @project AND [System.WorkItemType] = @workItemType ";

                if (areaPath == string.Empty && iterationPath == string.Empty)
                {                    
                    query = new Query(workItemStore, queryText, new Dictionary<string, string>() { { "project", currentProjectName }, { "workItemType", workItemType } });
                }
                else if (areaPath != string.Empty && iterationPath == string.Empty)
                {                 
                    queryText = queryText + "AND [System.AreaPath] = @areaPath";
                    query = new Query(workItemStore, queryText, new Dictionary<string, string>() { { "project", currentProjectName }, { "workItemType", workItemType }, { "areaPath", areaPath } });
                }
                else if (areaPath == string.Empty && iterationPath != string.Empty)
                {                 
                    queryText = queryText + "AND [System.IterationPath] = @iterationPath";
                    query = new Query(workItemStore, queryText, new Dictionary<string, string>() { { "project", currentProjectName }, { "workItemType", workItemType }, { "iterationPath", iterationPath } });
                }
                else
                {
                    queryText = queryText + "AND [System.AreaPath] = @areaPath AND [System.IterationPath] = @iterationPath";
                    query = new Query(workItemStore, queryText, new Dictionary<string, string>() { { "project", currentProjectName }, { "workItemType", workItemType }, { "areaPath", areaPath }, { "iterationPath", iterationPath } });
                }

                var workItemCollection = query.RunQuery();

                List<TFSDefectOrBugWorkItem> workItemAttributes = new List<TFSDefectOrBugWorkItem>();

                // retun workitems based on Areapath or Iteration path match.
                foreach (WorkItem Item in workItemCollection)
                {
                    TFSDefectOrBugWorkItem queryItemAttribute = new TFSDefectOrBugWorkItem();

                    queryItemAttribute.Id = Item.Id.ToString();
                    queryItemAttribute.Title = Item.Title;
                    queryItemAttribute.AreaPath = Item.AreaPath;
                    queryItemAttribute.IterationPath = Item.IterationPath;
                    queryItemAttribute.State = Item.State;

                    if (Item.Fields.Contains("Severity"))
                    {
                        queryItemAttribute.Severity = Item.Fields["Severity"].Value != null ? Item.Fields["Severity"].Value.ToString() : string.Empty;
                    }

                    if (Item.Fields.Contains("Priority"))
                    {
                        queryItemAttribute.Priority = Item.Fields["Priority"].Value != null ? Item.Fields["Priority"].Value.ToString() : string.Empty;
                    }
                    queryItemAttribute.AssignedTo = Item.Fields["Assigned To"].Value.ToString();
                    queryItemAttribute.CreatedDate = string.Format("{0:MM/dd/yyyy}", Item.CreatedDate);
                    queryItemAttribute.WorkItemObj = Item;

                    workItemAttributes.Add(queryItemAttribute);
                }

                return workItemAttributes; 
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                queryText = string.Empty;
            }
        }

        public List<WorkItem> GetAllWorkItemDetails()
        {
            List<WorkItem> allWorkItems = null;            

            try
            {
                WorkItemStore workItem = workItemStore;
                Query query = null;

                string queryText = "SELECT * FROM WorkItems WHERE [System.TeamProject] = @project";                
                query = new Query(workItemStore, queryText, new Dictionary<string, string>() { { "project", currentProjectName } });                

                var workItemCollection = query.RunQuery();

                allWorkItems = workItemCollection.Cast<WorkItem>().ToList();                
            }
            catch (Exception)
            {
                return null;
            }

            return allWorkItems; // retun all workitems under a particular project.
        }

        public static bool ConnectToTFS(out string serverName, out string teamProjectName)
        {
            TfsTeamProjectCollection projectCollection;
            ITestManagementTeamProject project = null;

            serverName = string.Empty;
            teamProjectName = string.Empty;

            try
            {
                TeamProjectPicker tpp = new TeamProjectPicker(TeamProjectPickerMode.SingleProject, false);
                DialogResult dlgRes = tpp.ShowDialog();

                if (dlgRes == System.Windows.Forms.DialogResult.OK)
                {
                    // Connects with tfs and picks server name and project name.
                    if (tpp.SelectedTeamProjectCollection != null)
                    {
                        projectCollection = tpp.SelectedTeamProjectCollection;

                        string projectName = tpp.SelectedProjects[0].Name;             // Uncomment for VS 2012/2013 & comment for VS 2010
                        //string projectName = tpp.SelectedProjects.GetValue(0).ToString();// Uncomment for VS 2010 & comment for VS 2012/2013

                        project = projectCollection.GetService<ITestManagementService>().GetTeamProject(projectName);
                        serverName = projectCollection.Uri.ToString();

                        teamProjectName = project.TeamProjectName;  // Uncomment for VS 2012/2013 & comment for VS 2010
                        //teamProjectName = project.WitProject.Name;    // Uncomment for VS 2010 & comment for VS 2012/2013
                    }
                }

                return true; 
            }
            catch (Exception)
            {
                return false;
            }
        }
        
        public bool IsWorkItemExists(string workItemId = "", string workItemTitle = "")
        {
            bool isExist = false;

            try
            {
                WorkItemStore workItem = workItemStore;
                Query query = null;
                string queryText = "SELECT [System.ID] FROM WorkItems WHERE [System.TeamProject] = '" + currentProjectName + "' ";

                if (workItemId != string.Empty && workItemTitle == string.Empty)
                {
                    queryText = queryText + "AND [System.ID] = '" + Convert.ToInt32(workItemId) + "'";
                }
                else if (workItemId == string.Empty && workItemTitle != string.Empty)
                {
                    queryText = queryText + "AND [System.Title] = '" + workItemTitle + "'";
                }
               
                query = new Query(workItemStore, queryText);
                var workItemCollection = query.RunQuery();
                isExist = workItemCollection.Count > 0 ? true : false;

            }
            catch (Exception)
            {
                return isExist;
            }

            //retuns true if workitem exist by passing its ID or title.
            return isExist;
        }

        public WorkItem GetWorkItemDetailsByID(int workItemId)
        {
            var service = tfsTeamProjectCollection.GetService<WorkItemStore>();
            return service.GetWorkItem(workItemId);
        }

        public WorkItem GetWorkItemDetailsByTitle(string title)
        {
            try
            {
                WorkItemStore workItem = workItemStore;
                Query query = null;

                string queryText = "SELECT * FROM WorkItems WHERE [System.TeamProject] = @project AND [System.Title] = @title";
                query = new Query(workItemStore, queryText, new Dictionary<string, string>() { { "project", currentProjectName }, { "title", title } });

                var workItemCollection = query.RunQuery();

                return workItemCollection.Cast<WorkItem>().FirstOrDefault();
            }
            catch (Exception)
            {                
                return null;
            }
        }

        public List<string> GetAreaPathOrIterationPath(string fieldType)
        {
            var server = tfsTeamProjectCollection.GetService<ICommonStructureService>();

            var projectInfo = server.GetProjectFromName(currentProjectName);
            var nodes = server.ListStructures(projectInfo.Uri);

            string name = string.Empty;
            int startIndex = currentProjectName.Length;
            int NumberOfElementsToBeRemoved = 0;

            switch (fieldType)
            {
                case "AreaPath":
                    name = "Area";
                    NumberOfElementsToBeRemoved = name.Length + 1;      //Length of Area + '/'
                    break;
                case "IterationPath":
                    name = "Iteration";
                    NumberOfElementsToBeRemoved = name.Length + 1;     //Length of Iteration + '/'
                    break;
                default:
                    break;
            }

            var nodesXml = server.GetNodesXml(
                nodes
                    .Where(node => node.Name == name)
                    .Select(node => node.Uri).ToArray(),
                true);

            //Removing "\\" from Area path list and \\Area by specifying its Index and Count as it has added to the Area path.           
            List<string> areaPathOrIterationPathList = XElement.Parse(nodesXml.OuterXml)
            .Descendants("Node")
            .Select(xe => xe.Attribute("Path").Value.Substring(1).Remove(startIndex, NumberOfElementsToBeRemoved)).ToList();

            //return area path or Iteration path
            return areaPathOrIterationPathList;
        }        

        public List<string> LinkTypes()
        {
            List<string> linkType = null;

            if (workItemStore != null)
            {
                linkType = new List<string>();

                foreach (WorkItemLinkType type in workItemStore.WorkItemLinkTypes)
                {
                    linkType.Add(type.ForwardEnd.Name);
                    linkType.Add(type.ReverseEnd.Name);
                }
                linkType.Remove("Child");   
             
            }

            //return all the link types.
            return linkType.OrderBy(o => o.Trim()).GroupBy(x => x.Trim()).Select(g => g.FirstOrDefault()).ToList<string>();
        }

        public bool LinkToAnExistingItem(string selectedLinkType, List<int> selectedWorkItemId, int linkedWorkItemId, string comment, out string linkError)
        {
            bool isLinked = false;
            int workIdCount = 0;

            linkError = string.Empty;
            try
            {
                if (workItemStore != null)
                {
                    // Linke one or more work item to a single work item.
                    foreach (int selectedWIId in selectedWorkItemId)
                    {
                        WorkItem wi = workItemStore.GetWorkItem(selectedWIId);

                        RelatedLink link = new RelatedLink(workItemStore.WorkItemLinkTypes.LinkTypeEnds[selectedLinkType], linkedWorkItemId);
                        link.Comment = comment;
                        wi.Links.Add(link);
                        wi.Save();
                        ++workIdCount;                        
                    }
                    if (selectedWorkItemId.Count == workIdCount)
                    {
                        isLinked = true;
                    }
                }
            }
            catch (Exception ex)
            {
                linkError = ex.Message.ToString();
                isLinked = false;
            }
            return isLinked;
        }

        public BatchSaveError[] SaveWorkItemDetails(List<int> WIIDs, TFSDefectOrBugWorkItem workItem)
        {
            BatchSaveError[] errorLists = null;

            try
            {               
                WorkItem[] workItems = new WorkItem[WIIDs.Count];

                for (int i = 0; i < WIIDs.Count; i++)
                {
                    WorkItem wi = workItemStore.GetWorkItem(WIIDs[i]);

                    if (workItem.RootCause.Trim().Length != 0)
                    {
                        wi.Fields["Root Cause"].Value = workItem.RootCause;
                    }
                    if (workItem.AssignedTo.Trim() == TFSConstants.EmptyAssignedToName)
                    {
                        wi.Fields["Assigned To"].Value = string.Empty;
                    }
                    else if (workItem.AssignedTo.Trim().Length != 0)
                    {
                        wi.Fields["Assigned To"].Value = workItem.AssignedTo;
                    }
                    if (workItem.Comment.Trim().Length != 0)
                    {
                        wi.Fields["History"].Value = workItem.Comment;
                    }
                    if (workItem.State.Trim().Length != 0)
                    {
                        wi.State = workItem.State;
                    }
                    if (workItem.AreaPath.Trim().Length != 0)
                    {
                        wi.AreaPath = workItem.AreaPath;
                    }
                    if (workItem.IterationPath.Trim().Length != 0)
                    {
                        wi.IterationPath = workItem.IterationPath;
                    }
                    if (workItem.Priority.Trim().Length != 0 && wi.Fields.Contains("Priority"))
                    {
                        wi.Fields["Priority"].Value = workItem.Priority;
                    }
                    if (workItem.Severity.Trim().Length != 0 && wi.Fields.Contains("Severity"))
                    {
                        wi.Fields["Severity"].Value = workItem.Severity;

                    }
                    if (workItem.Comment.Trim().Length != 0)
                    {
                        wi.History = workItem.Comment;
                    }

                    workItems[i] = wi;
                }

                if (workItemStore != null)
                {
                    // Save all the workitem details at a single shot.
                    errorLists = workItemStore.BatchSave(workItems);
                }
            }
            catch (Exception)
            {
                return null;
            }
            return errorLists;
        }

        public string FindWorkItemRelationByID(string selectedLinkType, List<int> SelectedWorkItemID, int linkedWorkItemId)
        {
            string linkError = string.Empty;

            foreach (int selectedWIId in SelectedWorkItemID)
            {
                WorkItem wi = workItemStore.GetWorkItem(selectedWIId);

                foreach (WorkItemLink item in wi.WorkItemLinks.Cast<WorkItemLink>().ToList().Where(i => i.LinkTypeEnd.Name == selectedLinkType && i.TargetId == linkedWorkItemId))
                {
                    linkError = string.Format("The current work item already contains links to the following work items:{0}", item.TargetId);                    
                    return linkError;
                }

                foreach (WorkItemLink item in wi.WorkItemLinks.Cast<WorkItemLink>().ToList().Where(i => i.SourceId == selectedWIId && i.TargetId == linkedWorkItemId && i.LinkTypeEnd.Name == selectedLinkType))
                {
                    linkError = string.Format("Adding a {0} link to work item {1} would result in a circular relationship. To create this link, evaluate the existing links, and remove one of the other links in the cycle.", selectedLinkType, linkedWorkItemId);
                    return linkError;
                }
            }            
            return linkError;
        }

        public AllowedValuesForMultipleUpdateWindow LoadFieldTypeCollection(int selectedWIId, string defectType = "Bug")
        {
            // returns all the possible fields for multiple attributes of workitems.
            try
            {

                AllowedValuesForMultipleUpdateWindow valueCollections = new AllowedValuesForMultipleUpdateWindow();

                FieldFilterList filters = new FieldFilterList();

                WorkItem wi = workItemStore.GetWorkItem(selectedWIId);

                if (wi == null)
                {
                    return valueCollections;
                }

                if (workItemStore.Projects[currentProjectName].WorkItemTypes[defectType].FieldDefinitions.Contains("Microsoft.VSTS.CMMI.RootCause"))
                {
                    valueCollections.RootCauseList = workItemStore.Projects[currentProjectName].WorkItemTypes[defectType].FieldDefinitions["Microsoft.VSTS.CMMI.RootCause"].AllowedValues;
                }

                if (workItemStore.Projects[currentProjectName].WorkItemTypes[defectType].FieldDefinitions.Contains("Microsoft.VSTS.Common.Priority"))
                {
                    valueCollections.PriorityList = workItemStore.Projects[currentProjectName].WorkItemTypes[defectType].FieldDefinitions["Microsoft.VSTS.Common.Priority"].AllowedValues;
                }

                if (workItemStore.Projects[currentProjectName].WorkItemTypes[defectType].FieldDefinitions.Contains("Microsoft.VSTS.Common.Severity"))
                {
                    valueCollections.SeverityList = workItemStore.Projects[currentProjectName].WorkItemTypes[defectType].FieldDefinitions["Microsoft.VSTS.Common.Severity"].AllowedValues;
                }

                if (workItemStore.Projects[currentProjectName].WorkItemTypes[defectType].FieldDefinitions.Contains("System.State"))
                {
                    filters.Add(new FieldFilter(workItemStore.Projects[currentProjectName].WorkItemTypes[defectType].FieldDefinitions["System.State"], wi.Fields["System.State"].Value));
                }

                if (workItemStore.Projects[currentProjectName].WorkItemTypes[defectType].FieldDefinitions.Contains("System.State"))
                {
                    valueCollections.StateList = workItemStore.Projects[currentProjectName].WorkItemTypes[defectType].FieldDefinitions["System.State"].FilteredAllowedValues(filters);
                }
                if (workItemStore.Projects[currentProjectName].WorkItemTypes[defectType].FieldDefinitions.Contains("System.AssignedTo"))
                {
                    valueCollections.AssignedToList = workItemStore.Projects[currentProjectName].WorkItemTypes[defectType].FieldDefinitions["System.AssignedTo"].FilteredAllowedValues(filters);
                }


                return valueCollections;

            }
            catch (Exception)
            {
                return null;
            }
        }

        #endregion
    }
}
