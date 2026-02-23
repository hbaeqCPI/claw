namespace R10.Web.Areas.Shared.ViewModels
{
    public class ChildQuickEmailSetupViewModel 
    {
        public int Id { get; set; }
        public string? SystemType { get; set; }
        public DetailPagePermission? Permission { get; set; }
        public QuickEmailSetupDetailViewModel? ParentViewModel { get; set; }
    }
}
