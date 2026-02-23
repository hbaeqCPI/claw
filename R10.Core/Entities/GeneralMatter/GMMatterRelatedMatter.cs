using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.GeneralMatter
{
    public class GMMatterRelatedMatter : BaseEntity
    {
        [Key]
        public int GMMId { get; set; }

        public int MatId { get; set; }

        [Required]
        public int RelatedId { get; set; }

        [StringLength(25)]
        public string? CaseNumber { get; set; }

        [StringLength(255)]
        public string? MatterTitle { get; set; }

        [StringLength(20)]
        public string? MatterStatus { get; set; }        

        public GMMatter? GMMatter { get; set; }
        public GMMatter? RelatedGMMatter { get; set; }

    }
}
