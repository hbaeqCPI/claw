using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace R10.Core.DTOs
{
    [Keyless]
    public class TLSearchTTABDTO
    {
        public int TLTmkId { get; set; }
        public int TTABId { get; set; }

        [Display(Name = "Proceeding")]
        public string? ProceedingNo { get; set; }
        public string? ProceedingLink { get; set; }

        [Display(Name = "Filing Date")]
        public DateTime? ProceedingFilDate { get; set; }

        [Display(Name = "Status")]
        public string? Status { get; set; }

        [Display(Name = "Defendant")]
        public string? Defendant { get; set; }

        [Display(Name = "Plaintiff")]
        public string? Plaintiff { get; set; }
        
    }
}
