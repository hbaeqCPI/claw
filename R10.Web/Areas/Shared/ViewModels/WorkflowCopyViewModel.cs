using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class WorkflowCopyViewModel
    {
        public int WrkId { get; set; }

        [Display(Name = "New Workflow")]
        [StringLength(100)]
        [Required]
        public string? Workflow { get; set; }

        [Display(Name="Action")]
        public bool CopyActions { get; set; }

        
    }
}
