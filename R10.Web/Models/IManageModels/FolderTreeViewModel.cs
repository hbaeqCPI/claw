namespace R10.Web.Models.IManageModels
{
    public class FolderTreeViewModel
    {
        public string? PageId { get; set; }
        public string? ContainerId { get; set; }
        public Container? RootContainer { get; set; }
        public List<Folder>? Folders {  get; set; }
        public DocumentViewerPermission? Permission { get; set; }
    }
}
