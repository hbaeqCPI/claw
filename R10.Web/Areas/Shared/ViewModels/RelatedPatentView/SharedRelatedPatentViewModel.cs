using R10.Core.Entities;
using R10.Core.Entities.GeneralMatter;
using R10.Web.Areas.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;


namespace R10.Web.Areas.Shared.ViewModels
{
    public class SharedRelatedPatentViewModel : BaseEntity
    {
        public int KeyId { get; set; }
        public int ParentId { get; set; }
        public int? InvId { get; set; }
        public int? AppId { get; set; }

        //[Required]
        [Display(Name = "Case Number")]
        public string? RelatedCaseNumber { get; set; }

        [Display(Name = "Country")]
        public string? Country { get; set; }

        [Display(Name = "Sub Case")]
        public string? SubCase { get; set; }

        [Display(Name = "Case Type")]
        public string? CaseType { get; set; }

        [Display(Name = "Title")]
        public string? Title { get; set; }

        [Display(Name = "Application No.")]
        public string? AppNumber { get; set; }

        [Display(Name = "Filing Date")]
        public DateTime? FilDate { get; set; }

        [StringLength(20)]
        [Display(Name = "Publication No.")]
        public string? PubNumber { get; set; }

        [Display(Name = "Publication Date")]
        public DateTime? PubDate { get; set; }

        [StringLength(20)]
        [Display(Name = "Patent No.")]
        public string? PatNumber { get; set; }

        [Display(Name = "Issue Date")]
        public DateTime? IssDate { get; set; }

        [Display(Name = "Expiration Date")]
        public DateTime? ExpDate { get; set; }

        [Display(Name = "Status")]
        public string? ApplicationStatus { get; set; }

        [Display(Name = "Image")]
        public string? ImageFile { get; set; }

        [Display(Name = "Image")]
        public string? ThumbnailFile { get; set; }
        public string? SharePointRecKey { get; set; }
        public string? ThumbnailUrl { get; set; }
    }
}
