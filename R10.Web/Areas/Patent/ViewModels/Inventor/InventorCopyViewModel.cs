using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Patent.ViewModels
{
    public class InventorCopyViewModel
    {
        public int InventorId { get; set; }

        [Display(Name = "Last Name")]
        [StringLength(25)]
        [Required]
        public string? LastName { get; set; }

        [Display(Name = "First Name")]
        [StringLength(25)]
        public string? FirstName { get; set; }

        [Display(Name = "Middle Name")]
        [StringLength(25)]
        public string? MiddleName { get; set; }

        [Display(Name = "Inventor Info")]
        public bool CopyInventorInfo { get; set; }

        [Display(Name="Address")]
        public bool CopyAddress { get; set; }

        [Display(Name = "PO Box")]
        public bool CopyPOBox { get; set; }

        [Display(Name = "Remarks")]
        public bool CopyRemarks { get; set; }

        
    }
}
