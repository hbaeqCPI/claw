using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Core.Entities
{
    public partial class LetterEntitySetting:BaseEntity
    {
        [Key]
        public int SettingId { get; set; }

        [Required]
        [StringLength(1)]
        public string?  EntityType { get; set; }

        public int EntityId { get; set; }

        [Required]
        public int ContactId { get; set; }

        [StringLength(1)]
        public string?  SendAs { get; set; }

        [Required]
        [Display(Name = "Letter Category")]
        public int LetCatId { get; set; }

        public LetterCategory? LetterCategory { get; set; }

    }
}
