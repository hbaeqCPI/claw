using R10.Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class RelatedMatterViewModel:BaseEntity
    {
       
        public int GMPId { get; set; }

        public int MatId { get; set; }
        public int TmkId { get; set; }
        public int InvId { get; set; }
        public int AppId { get; set; }

        [Display(Name = "Matter")]
        public string? RelatedCaseNumber { get; set; }

        [Display(Name = "Sub Case")]
        public string? RelatedSubCase { get; set; }

        [Display(Name = "Matter Type")]
        public string? MatterType { get; set; }

        [Display(Name = "Matter Title")]
        public string? MatterTitle { get; set; }

    }
}
