using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Patent
{
    public class PatEPODocumentMergeGuide : BaseEntity
    {
        [Key]
        public int GuideId { get; set; }

        [Required]
        public int MergeId { get; set; }
                
        [Required, Display(Name = "Communication Name")]
        public string? GuideFileName { get; set; }
        
        [Required, Display(Name = "Order Of Entry")]
        public int OrderOfEntry { get; set; }


        public PatEPODocumentMerge? Map { get; set; }
        public List<PatEPODocumentMergeGuideSub>? PatEPODocumentMergeGuideSubs { get; set; }
    }
}
