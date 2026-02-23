namespace R10.Web.Models.PageViewModels
{
    public class QEDataSourcePageViewModel
    {
        public string? Title { get; set; }
        public string? PageId { get; set; }
        public string? DetailPageId { get; set; }
        public string? SystemType { get; set; }
        public int GridPageSize { get; set; } = 20;
    }
}
