namespace R10.Web.Areas.Patent.ViewModels
{ 
    public class ShowAwardViewModel
    {
        public int ParentId { get; set; }

        public int InventorID { get; set; }
        public string? Module { get; set; }
        public bool CanEdit { get; set; }
        public string? Inventor { get; set; }
    }
}
