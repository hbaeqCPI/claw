using LawPortal.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace LawPortal.Web.Areas.Shared.ViewModels
{
    public class ActionTypeCopyViewModel
    {
        public int ActionTypeId { get; set; }

        [Display(Name = "New Action Type")]
        [StringLength(60)]
        [Required]
        public string? ActionType { get; set; }

        [Display(Name="Action Parameters")]
        public bool CopyParameters { get; set; }

        [Display(Name = "Remarks")]
        public bool CopyRemarks { get; set; }

        
    }
}
