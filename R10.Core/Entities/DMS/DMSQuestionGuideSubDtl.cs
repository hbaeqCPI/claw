using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Core.Entities.DMS
{
    public class DMSQuestionGuideSubDtl : BaseEntity
    {
        [Key]
        public int SubDtlId { get; set; }

        public int SubId { get; set; }

        [Required]
        [StringLength(510)]
        public string? Description { get; set; }
        public int OrderOfEntry { get; set; }

        public DMSQuestionGuideSub? DMSQuestionGuideSub { get; set; }

        [NotMapped]
        public bool CanEdit { get; set; }
    }
}
