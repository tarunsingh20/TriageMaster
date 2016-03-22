
namespace TriageMaster.Common
{
    public static class MessageNotification
    {
        #region [Link Window Constants]

        public const string UpdateFieldsCannotBeEmpty = "The above field(s) cannot be empty.";
        public const string ErrorMessage = "Following error(s) have been occured while saving the work item(s).";
        public const string WorkitemNotExist = "The work item does not exist, or you do not have permission to access it.";
        public const string WorkitemLinkToSelf = "Team Foundation does not support linking a work item to itself.";
        public const string LinkConfirmationMessage = "Do you want to save the changes?";
        public const string WorkItemLinked = "Workitem(s) are linked.";

        #endregion
        
        #region [Update Window Constants]

        public const string BugOrDefectWorkItemIDs = "Bug / Defect Work item ID(s) to update: ";
        public const string UpdateConfirmationMessage = "Do you want to save the changes?";
        public const string EveryFieldIsEmpty = "There are no updates to save!";
        public const string WorkitemsUpdated = "Work item(s) updated.";
        public const string WorkitemsNotUpdated = " No Work item(s) updated.";
        public const string NotValidAreaPathOrIterationPath = "The area or iteration provided for field 'Area Path or Iteration Path' could not be found.";

        #endregion

        #region [Load Query From Wiq File]

        public const string QueryFileNotHaveBugOrDefect = "The Selected Query File does not return any {0}";

        #endregion

        #region [Mainwindow]

        public const string NoWorkItemPresent = "No Bug(s)/ Defect(s) found...!";
        public const string WorkItemPresent = "Bug(s)/ Defect(s) found...";
        public const string TriageMasterInstance = "Another instance of TriageMaster is already running....";

        #endregion

        #region [Find related bug]

        public const string NoRelatedBugsOrDefectsPresent = "No Related Bug(s)/ Defect(s) found...!";
        public const string RelatedBugsOrDefectsPresent = "Related Bug(s)/ Defect(s) found...";

        #endregion


        #region [Busy Indicator]

        public const string LoadingUpdateWinow = "Loading Update multiple fields window...";
        public const string LoadingFindRelatedBugOrDefectWinow = "Searching related Bug(s) / Defect(s)...";
        public const string Saving = "Saving Bug / Defect...";
        public const string Linking = "Linking Bug(s) / Defect(s)...";
        public const string Fetching = "Please wait while Bug(s) / Defect(s) are being retrieved...";

        #endregion

        #region [Login Window]

        public const string DOMAIN = "xxxx";
        public const string TFSSERVERNOTCONNECTEDERROR = "Unable to connect to TFS Server. Please check if the credentials, TFS server URL and project are correct.";
        public const string TFSPROJECTINVALIDERROR = "Could not find the TFS project specified on the server, please verify the TFS project name and try again.";
        public const string TFSWORKITEMSTOREINVALIDERROR = "Could not retrieve the work item store on the TFS server. Verify that you have installed the correct version of TriageMaster.";
        public const string TFSWRONGURIENTEREDERROR = "Unable to connect to TFS Server. Please check if TFS server URL is correct.";       

        #endregion
    } 
}
