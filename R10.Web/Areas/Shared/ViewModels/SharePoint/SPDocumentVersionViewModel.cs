namespace R10.Web.Areas.Shared.ViewModels
{
    public class SPDocumentVersionViewModel
    {
        public string ServerRelativeUrl { get; set; }
        public string VersionLabel { get; set; }
        public string Url { get; set; }
        public DateTime Created { get; set; }
        public string CreatedBy { get; set; }
        public string CheckInComment { get; set; }
    }
}
