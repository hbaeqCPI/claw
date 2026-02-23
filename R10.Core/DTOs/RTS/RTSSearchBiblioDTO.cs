using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace R10.Core.DTOs
{
    [Keyless]
    public class RTSSearchBiblioDTO
    {
        public int PLAppId { get; set; }

        [Display(Name = "Application No.")]
        public string? AppNo { get; set; }
        [Display(Name = "Publication No.")]
        public string? PubNo { get; set; }
        [Display(Name = "Patent No.")]
        public string? PatNo { get; set; }
        [Display(Name = "Filing Date")]
        public DateTime? FilDate { get; set; }
        [Display(Name = "Publication Date")]
        public DateTime? PubDate { get; set; }
        [Display(Name = "Issue Date")]
        public DateTime? IssDate { get; set; }

        [Display(Name = "Parent/PCT No.")]
        public string? PctNo { get; set; }
        [Display(Name = "Parent/PCT Date")]
        public DateTime? PctDate { get; set; }

        [Display(Name = "Confirmation No.")]
        public string? ConfirmNo { get; set; }

        [Display(Name = "Entity Status")]
        public string? EntityStatus { get; set; }

        [Display(Name = "Assignee")]
        public string? Applicant { get; set; }

        [Display(Name = "Assignee Address")]
        public string? ApplicantAddress { get; set; }
        public string? ApplicantCountry { get; set; }

        [NotMapped]
        public List<RTSSearchTitleDTO> Titles { get; set; }

        [NotMapped]
        public List<RTSSearchInventorDTO> Inventors { get; set; }
        
    }
}
