using R10.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Patent.ViewModels
{
    public class PatInventorDetailViewModel : R10.Core.Entities.Patent.PatInventor
    {
        public string? CountryName { get; set; }
        public string? POCountryName { get; set; }
        public string? CitizenshipCountryName { get; set; }
        public string? UserId { get; set; }
        public bool IsDMSReviewer { get; set; }
        public bool IsPatentUser { get; set; }
        public bool IsPatClearanceUser { get; set; }
        public List<SysCustomFieldSetting>? SysCustomFieldSettings { get; set; }

    }
}
