using R10.Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace R10.Core.Entities
{
    public class EFS : BaseEntity
    {
        [Key]
        public int EfsDocId { get; set; }

        [StringLength(1)]
        [Display(Name = "System Type")]
        public string? SystemType { get; set; }

        public int? ModuleId { get; set; }

        [StringLength(100)]
        [Display(Name = "Group")]
        public string? GroupDesc { get; set; }

        [StringLength(100)]
        [Display(Name = "Description")]
        public string? DocDesc { get; set; }

        [StringLength(10)]
        public string? DocType { get; set; }

        [StringLength(10)]
        public string? SubType { get; set; }

        [StringLength(255)]
        public string? SourceTables { get; set; }

        [StringLength(50)]
        public string? RecLimit { get; set; }

        [StringLength(500)]
        public string? DocPath { get; set; }

        [StringLength(500)]
        public string? MapFile { get; set; }

        [StringLength(5)]
        [Display(Name = "Country")]
        public string? Country { get; set; }

        public int? DisplayOrder { get; set; }

        public string? Remarks { get; set; }

        [Display(Name = "For eSignature?")]
        public bool ForSignature { get; set; }

        [Display(Name = "Email Body Template")]
        public int? SignatureQESetupId { get; set; }

        [NotMapped]
        public int SignatureQESetupId2 { get; set; }

        [Display(Name = "Anchor")]
        public string? AnchorCode { get; set; }
      
        public QEMain? QEMain { get; set; }
    }

}
