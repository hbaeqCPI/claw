namespace R10.Web.Models.IManageModels
{
    public class DocumentViewerPermission
    {
        public bool CanUpload { get; set; }
        public bool CanDelete { get; set; }
        public bool CanEdit { get; set; }
        public bool CanSetupFolder { get; set; }
        public bool CanDownload { get; set; }
    }
}
