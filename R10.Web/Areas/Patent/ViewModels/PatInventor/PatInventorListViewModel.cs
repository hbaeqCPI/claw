using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Patent.ViewModels
{
    public class PatInventorListViewModel
    {
        public int InventorID { get; set; }

        [Display(Name = "Last Name")]
        public string? LastName { get; set; }

        [Display(Name = "First Name")]
        public string? FirstName { get; set; }

        [Display(Name = "Middle")]
        public string? MiddleInitial { get; set; }

        [Display(Name = "Inventor")]
        public string? Inventor { get; set; }
    }
}
