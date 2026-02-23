using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Core.Entities.FormExtract
{
    public class FormIFWActMapPat : BaseEntity
    {
        [Key]
        public int MapId { get; set; }

        public int MapHdrId { get; set; }

        [Display(Name = "Extracted Term")]
        public int Term { get; set; }

        [StringLength(60)]
        [Display(Name = "Action Type")]
        [Required]
        public string? PMSActionType { get; set; }

        public FormIFWActMap? FormIFWActMap { get; set; }

        [Display(Name = "Document Description")]
        public string? DocumentDescription { get; set; }
    }
}
