using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace R10.Core.DTOs
{
    [Keyless]
    public class RTSSearchContinuityParentDTO
    {
        public int PLAppId { get; set; }

        [Display(Name= "Description")]
        public string? Description { get; set; }

        [Display(Name = "Parent No.")]
        public string? ParentNo { get; set; }

        [Display(Name = "Parent Status")]
        public string? ParentStatus { get; set; }

        [Display(Name = "Parent Filing Date")]
        public DateTime? FilingDate { get; set; }

        [Display(Name = "Patent No.")]
        public string? PatentNo { get; set; }
    }
}
