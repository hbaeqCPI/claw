namespace R10.Web.Models.NetDocumentsModels
{
    public class SetupViewModel
    {
        public string? DocRootContainerId { get; set; }
        public string? DocRootContainerName { get; set; }
        public string? DocDefaultFolderId { get; set; }
        public DocumentViewerPermission? Permission { get; set; }
    }
}
