namespace R10.Web.Areas.Shared.ViewModels
{
    public static class SharePointDocLibrary
    {
        public const string Patent = "Patent";
        public const string Trademark = "Trademark";
        public const string GeneralMatter = "General Matter";
        public const string PatClearance = "Patent Clearance";
        public const string DMS = "DMS";
        public const string AMS = "AMS";
        public const string IDS = "IDS";
        public const string TmkRequest = "Trademark Request";
        public const string Shared = "Shared";
        public const string LetterTemplates = "Letter Templates";
        public const string LetterLog = "Letters Log";
        public const string QELog = "Quick Emails Log";
        public const string IPFormsLog = "IP Forms Log";
        public const string Orphanage = "Orphanage";
        public const string Calendar = "Calendar Files";
    }

    public static class SharePointDocLibraryFolder
    {
        public const string Invention = "Invention";
        public const string Application = "Application";
        public const string Trademark = "Trademark";
        public const string GeneralMatter = "General Matter";
        public const string Action = "Action";
        public const string Cost = "Cost";
        public const string PatClearance = "Clearance";
        public const string DMS = "DMS";
        public const string AMS = "AMS";
        public const string TmkRequest = "Trademark Request";
        public const string DeDocket = "DeDocket";
        public const string TmkLinks = "Trademark Links";
        public const string Product = "Product";
        public const string InventionCostTracking = "Invention Cost";
        public const string InventionAction = "Invention Action";
        public const string Conflict = "Conflict";
        public const string GMOPTmk = "Other Party Trademark";
    }

    public static class SharePointSeparator
    {
        public const string Folder = "`"; //set to - when using a single folder for RecKey (ex. ABC-US instead of ABC parent then US child)
        //public const string Folder = "-";
        public const string Field = "~"; //set to - when using a single folder for RecKey (ex. ABC-US instead of ABC parent then US child)
        //public const string Field = "-";
    }

}
