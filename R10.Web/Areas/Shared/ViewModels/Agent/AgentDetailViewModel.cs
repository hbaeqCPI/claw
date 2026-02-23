using R10.Core.Entities;
using R10.Web.Filters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class AgentDetailViewModel: AgentDetail
    {
        public string? CountryName { get; set; }
        public string? POCountryName { get; set; }
        public List<SysCustomFieldSetting>? SysCustomFieldSettings { get; set; }
    }

}
