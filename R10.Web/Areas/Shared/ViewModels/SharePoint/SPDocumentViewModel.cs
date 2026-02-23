namespace R10.Web.Areas.Shared.ViewModels
{
    public class SPDocumentViewModel
    {
        public string CaseFolder { get; set; }
        public int Id { get; set; }
        public IEnumerable<IFormFile>? UploadedFiles { get; set; }
        public string? Title { get; set; }
        public bool PrintOnReports { get; set; }
        public bool Default { get; set; }
        public string? Tags { get; set; }
        public string? Remarks { get; set; }
    }
}
