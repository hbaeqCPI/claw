namespace R10.Web.Areas.Shared.ViewModels 
{
    public class SharePointReportImage
    {
        public string? System { get; set; }
        public string? Module { get; set; }
        public int Id { get; set; }
        public bool IsDefault { get; set; }
        public bool IsPrintOnReport { get; set; }
        public string? FileName { get; set; }
        //public string? ImageUrl { get; set; }
        public string? ItemId { get; set; }
        public int OrderOfEntry { get; set; }
    }

    public class SharePointImageList
    {
        public string? System { get; set; }
        public string? Module { get; set; }
        public int Id { get; set; }
        public string? ImageNames { get; set; }
    }
}
