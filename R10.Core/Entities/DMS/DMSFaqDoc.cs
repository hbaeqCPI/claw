using R10.Core.DTOs;
using R10.Core.Entities.Documents;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Core.Entities.DMS
{
    public class DMSFaqDoc: DMSFaqDocDetail
    {
        public DocType? DocType { get; set; }
        public DocFile? DocFile { get; set; }
    }

    public class DMSFaqDocDetail : BaseEntity
    {
        [Key]
        public int FaqId { get; set; }        

        [Display(Name = "Name")]
        [Required]
        public string? DocName { get; set; }

        [Display(Name = "Document Type")]
        public int? DocTypeId { get; set; }

        [Display(Name = "URL")]
        public string? DocUrl { get; set; }

        public int? FileId { get; set; }

        [StringLength(2000)]
        [Display(Name = "Message")]
        public string? Message { get; set; }

        [Display(Name = "Active?")]
        public bool IsActive { get; set; }
    }
}
