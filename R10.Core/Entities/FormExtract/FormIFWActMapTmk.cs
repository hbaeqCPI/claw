using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Core.Entities.FormExtract
{
    public class FormIFWActMapTmk : BaseEntity
    {
        [Key]
        public int MapId { get; set; }

        public int MapHdrId { get; set; }

        [Display(Name = "Extracted Term")]
        public int Term { get; set; }

        [StringLength(60)]
        [Display(Name = "Action Type")]
        [Required]
        public string? TMSActionType { get; set; }

        public FormIFWActMap? FormIFWActMap { get; set; }

    }
}
