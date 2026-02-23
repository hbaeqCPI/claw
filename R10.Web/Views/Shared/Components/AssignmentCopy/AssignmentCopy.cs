using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.ViewComponents
{
    public class AssignmentCopy : ViewComponent
    {
        public IViewComponentResult Invoke(AssignmentCopyOptions model)
        {
            return View(model);
        }
    }

    public class AssignmentCopyOptions
    {
        public string? Area { get; set; }
        public string? Controller { get; set; }
        public int ParentId { get; set; }
        public string? CaseNumberLabel { get; set; }

        public string? CaseNumber { get; set; }

        [Display(Name = "Country")]
        public string? Country { get; set; }

        [Display(Name = "Sub Case")]
        public string? SubCase { get; set; }

        [Display(Name = "Application Number")]
        public string? AppNumber { get; set; }

        [Display(Name = "From")]
        public string? AssignmentFrom { get; set; }
        
        [Display(Name = "To")]
        public string? AssignmentTo { get; set; }

        [StringLength(8)]
        [Display(Name = "Reel")]
        public string? Reel { get; set; }

        [StringLength(8)]
        [Display(Name = "Frame")]
        public string? Frame { get; set; }


    }
}
