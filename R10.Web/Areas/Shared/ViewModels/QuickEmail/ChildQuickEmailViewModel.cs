
namespace R10.Web.Areas.Shared.ViewModels
{
    public class ChildQuickEmailViewModel 
    {
        public int Id { get; set; }
        public string? SystemType { get; set; }
        public string? RoleLink { get; set; }
        public DetailPagePermission? Permission { get; set; }
        public QuickEmailDetailViewModel? ParentViewModel { get; set; }
    }
}
