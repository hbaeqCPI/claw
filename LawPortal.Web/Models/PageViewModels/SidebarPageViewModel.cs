namespace LawPortal.Web.Models.PageViewModels
{
    public class SidebarPageViewModel
    {
        public string PageTitle { get; set; }
        public string Title { get; set; }
        public string PageId { get; set; }
        public string MainPartialView { get; set; }
        public string SideBarPartialView { get; set; }
        public Object MainViewModel { get; set; }
        public Object SideBarViewModel { get; set; }
    }
}
