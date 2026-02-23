namespace R10.Web.Models.DocumentViewModels
{
    public class DocumentFileHistoryViewModel
    {
        public int ParentValue { get; set; }
        public string? SystemType { get; set; }

        public string? FileHistoryScreenCode { get; set; }
        public string? FileHistoryViewerAction { get; set; }
        public string? FileHistoryViewerController { get; set; }
        public string? FileHistoryViewerArea { get; set; }
        public string? FileHistoryAction { get; set; }
        public string? FileHistoryController { get; set; }
        public string? FileHistoryArea { get; set; }
    }
}
