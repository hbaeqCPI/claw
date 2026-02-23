using R10.Core.Entities.Patent;
using R10.Web.Areas.Patent.Controllers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Patent.ViewModels
{
    public class InventionPriorityViewModel : PatPriority
    {
        [Display(Name = "Country Name")]
        public string? CountryName { get; set; }

        [StringLength(20)]
        [Display(Name = "Application No.")]
        public string? AppNumberPrio { get; set; } //for data entry, html id conflict with appnumber in ctry app (ctry app and inv are open)
    }
}
