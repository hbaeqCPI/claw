using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Patent.ViewModels
{
    public class CountryApplicationPriorityViewModel
    {

        [Display(Name = "Priority Country")]
        public string? PriorityCountry { get; set; }
        [Display(Name = "Priority Number")]
        public string? PriorityNumber { get; set; }
        [Display(Name = "Priority Date")]
        public DateTime? PriorityDate { get; set; }
    }
}
