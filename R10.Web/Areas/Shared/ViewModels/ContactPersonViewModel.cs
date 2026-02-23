using R10.Core.Entities;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class ContactPersonViewModel : ContactPerson
    {
        public string? CountryName { get; set; }
        public string? UserId { get; set; }
        public bool IsDMSReviewer { get; set; }
        public bool IsAMSDecisionMaker { get; set; }
        public bool IsTmkSearchReviewer { get; set; }
        public bool IsPatClearanceReviewer { get; set; }
        public bool IsRMSDecisionMaker { get; set; }
        public bool IsFFDecisionMaker { get; set; }
        public List<SysCustomFieldSetting>? SysCustomFieldSettings { get; set; }
    }
}
