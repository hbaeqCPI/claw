using R10.Core.Entities;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class OwnerDetailViewModel: OwnerDetail
    {
        public string? CountryName { get; set; }
        public string? POCountryName { get; set; }
        public string? ClientCode { get; set; }
        public List<SysCustomFieldSetting>? SysCustomFieldSettings { get; set; }
    }

}
