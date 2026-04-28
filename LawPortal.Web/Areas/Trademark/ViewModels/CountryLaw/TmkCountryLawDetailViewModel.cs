using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using LawPortal.Core.Entities.Trademark;

namespace LawPortal.Web.Areas.Trademark.ViewModels.CountryLaw
{
    public class TmkCountryLawDetailViewModel:TmkCountryLaw
    {
        public string? AgentCode { get; set; }

        [Display(Name = "Country Name")]
        public string? CountryName { get; set; }

        [Display(Name = "Agent Name")]
        public string? AgentName { get; set; }

        [Display(Name = "Description")]
        public string? CaseTypeDescription { get; set; }

        public bool HasDesignatedCountries { get; set; }
        public bool IsCPiAction { get; set; }
        public bool CanDeleteChild { get; set; }
    }
}
