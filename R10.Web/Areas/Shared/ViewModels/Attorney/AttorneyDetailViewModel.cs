using R10.Core.Entities;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class AttorneyDetailViewModel:AttorneyDetail
    {
        public string? CountryName { get; set; }
        public string? POCountryName { get; set; }
        public string? UserId { get; set; }
        public bool IsPatentUser { get; set; }
        public bool IsTrademarkUser { get; set; }
        public bool IsGeneralMatterUser { get; set; }
        public List<SysCustomFieldSetting>? SysCustomFieldSettings { get; set; }

    }
}
