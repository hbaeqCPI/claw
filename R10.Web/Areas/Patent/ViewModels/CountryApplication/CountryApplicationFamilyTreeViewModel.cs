using R10.Core.Entities;
using R10.Core.Entities.Patent;
using R10.Web.Areas.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Patent.ViewModels
{
    public class CountryApplicationFamilyTreeViewModel : CountryApplicationDetail
    {
        public string? CaseTypeDescription { get; set; }
        public string? ClientName { get; set; }
        public bool IsActive { get; set; }
        public CountryApplicationPriorityViewModel? Priority { get; set; }

    }
}
