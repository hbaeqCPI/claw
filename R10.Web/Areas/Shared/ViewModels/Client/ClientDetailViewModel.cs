using R10.Core.Entities;
using R10.Web.Filters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class ClientDetailViewModel: ClientDetail
    {
        public string? CountryName { get; set; }
        public string? POCountryName { get; set; }
        public string? PatDefaultAtty1Code { get; set; }
        public string? PatDefaultAtty2Code { get; set; }
        public string? PatDefaultAtty3Code { get; set; }
        public string? PatDefaultAtty4Code { get; set; }
        public string? PatDefaultAtty5Code { get; set; }
        public string? TmkDefaultAtty1Code { get; set; }
        public string? TmkDefaultAtty2Code { get; set; }
        public string? TmkDefaultAtty3Code { get; set; }
        public string? TmkDefaultAtty4Code { get; set; }
        public string? TmkDefaultAtty5Code { get; set; }

        public string? Attorney1Label { get; set; }
        public string? Attorney2Label { get; set; }
        public string? Attorney3Label { get; set; }
        public string? Attorney4Label { get; set; }
        public string? Attorney5Label { get; set; }

        [Display(Name = "Cost Estimator General Setup")]
        public string? PatCEGeneralSetupName { get; set; }
        [Display(Name = "Cost Estimator General Setup")]
        public string? TmkCEGeneralSetupName { get; set; }
        public string? RemunerationSettingName { get; set; }
        public List<SysCustomFieldSetting>? SysCustomFieldSettings { get; set; }
    }



}
