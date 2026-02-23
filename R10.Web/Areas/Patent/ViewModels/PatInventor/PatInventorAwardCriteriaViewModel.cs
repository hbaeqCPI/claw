using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Patent.ViewModels
{
    public class PatInventorAwardCriteriaViewModel : PatInventorAwardCriteria
    {
        [StringLength(20)]
        [Display(Name = "Award Type")]
        public string? AwardType { get; set; }
    }
}
