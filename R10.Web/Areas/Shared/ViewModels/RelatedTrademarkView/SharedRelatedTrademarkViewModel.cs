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
    public class SharedRelatedTrademarkViewModel : BaseEntity
    {
        public int KeyId { get; set; }
        public int ParentId { get; set; }
        public int? TmkId { get; set; }

        //[Required]
        [Display(Name = "Case Number")]
        public string? RelatedCaseNumber { get; set; }

        [Display(Name = "Country")]
        public string? Country { get; set; }

        [Display(Name = "Sub Case")]
        public string? SubCase { get; set; }

        [Display(Name = "Case Type")]
        public string? CaseType { get; set; }

        [Display(Name = "Trademark Name")]
        public string? Trademark { get; set; }

        [Display(Name = "Application No.")]
        public string? AppNumber { get; set; }

        [Display(Name = "Filing Date")]
        public DateTime? FilDate { get; set; }

        [Display(Name = "Registration No.")]
        public string? RegNumber { get; set; }

        [Display(Name = "Registration Date")]
        public DateTime? RegDate { get; set; }

        [Display(Name = "Publication No.")]
        public string? PubNumber { get; set; }

        [Display(Name = "Class")]
        public string? TrademarkClasses { get; set; }

        [Display(Name = "Status")]
        public string? TrademarkStatus { get; set; }

        [Display(Name = "Image")]
        public string? ImageFile { get; set; }

        [Display(Name = "Image")]
        public string? ThumbnailFile { get; set; }
        public string? SharePointRecKey { get; set; }
        public string? ThumbnailUrl { get; set; }
    }
}
