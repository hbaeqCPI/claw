using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace R10.Core.DTOs
{
    [Keyless]
    public class RTSSearchPFSDocDTO
    {
        public int AppId { get; set; }

        [Display(Name="KD")]
        public string? KD { get; set; }

        [Display(Name = "App. No")]
        public string? AppNo { get; set; }

        [Display(Name = "Filing/Parent Date")]
        public DateTime? AppDate { get; set; }

        [Display(Name = "Doc No")]
        public string? DocumentNo { get; set; }

        [Display(Name = "Doc Date")]
        public DateTime? PubDate { get; set; }

        [Display(Name = "Assignee")]
        public string? ApplicantName { get; set; }
        
    }
}
