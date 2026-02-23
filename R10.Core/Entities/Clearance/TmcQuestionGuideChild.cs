using R10.Core.Entities.PatClearance;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace R10.Core.Entities.Clearance
{
    public class TmcQuestionGuideChild : BaseEntity
    {
        [Key]
        public int ChildId { get; set; }

        public int QuestionId { get; set; }

        [Required]
        [StringLength(510)]
        public string? Description { get; set; }
        public int OrderOfEntry { get; set; }

        public TmcQuestionGuide? TmcQuestionGuide { get; set; }

        [NotMapped]
        public bool CanEdit { get; set; }
    }
}