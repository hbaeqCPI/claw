using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities
{
    public class DocuSignAnchor : BaseEntity
    {
        [Key]
        public int DocuSignAnchorId { get; set; }

        [StringLength(20)]
        [Display(Name = "Anchor Code")]
        public string? AnchorCode { get; set; }

        [StringLength(255)]
        [Display(Name = "Description")]
        public string? Description { get; set; }
        
        public bool IsCPIAnchor { get; set; }

        public List<DocuSignAnchorTab>? DocuSignAnchorTabs { get; set; }

    }

    public class DocuSignAnchorTab : BaseEntity
    {
        [Key]
        public int DocuSignTabId { get; set; }
        public int DocuSignAnchorId { get; set; }

        [StringLength(20)]
        [Display(Name ="Type")]
        public string? AnchorType { get; set; }

        [StringLength(255)]
        [Display(Name ="Anchor Tag")]
        public string? Anchor { get; set; }

        [Display(Name ="Y Offset (px)")]
        public int AnchorYOffSet { get; set; }
        
        [Display(Name ="X Offset (px)")]
        public int AnchorXOffSet { get; set; }
        
        public DocuSignAnchor? DocuSignAnchor { get; set; }
    }

}
