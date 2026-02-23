using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace R10.Core.Entities.Patent
{
    public class PatEPODocumentMergeGuideSub : BaseEntity
    {
        [Key]
        public int SubId { get; set; }

        public int GuideId { get; set; }

        [Required]
        [StringLength(510)]
        public string? SubFileName { get; set; }
        public int OrderOfEntry { get; set; }
        
        public PatEPODocumentMergeGuide? PatEPODocumentMergeGuide { get; set; }
    }
}
