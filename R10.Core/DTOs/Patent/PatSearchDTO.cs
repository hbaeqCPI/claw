
using System.ComponentModel.DataAnnotations;

namespace R10.Core.DTOs
{
    public class PatSearchDTO
    {
        public int SearchId { get; set; }
        public string? CriteriaName { get; set; }
        public string? AppId { get; set; }
        public string? LinkUrl { get; set; }
        public string? TitleHighlighted { get; set; }
        public string? AbstractHighlighted { get; set; }
        public string? Country { get; set; }
        public string? AppnoOrigHighlighted { get; set; }
        public string? AppDateHighlighted { get; set; }
        public string? OwnersHighlighted { get; set; }
        public string? InventorsHighlighted { get; set; }
        public string? AppnoOrig { get; set; }
    }

    public class PatSearchExportDTO
    {
        public int SearchId { get; set; }
        public string? CriteriaName { get; set; }

        public string? AppId { get; set; }

        [Display(Name = "Country")]
        public string? Country { get; set; }

        [Display(Name = "Application No.")]
        public string? AppnoOrig { get; set; }

        [Display(Name = "Filing Date")]
        public string? AppDate { get; set; }

        [Display(Name = "Abstract")]
        public string? Abstract { get; set; }

        [Display(Name = "Title")]
        public string? Title { get; set; }

        [Display(Name = "Owners")]
        public string? Owners { get; set; }

        [Display(Name = "Inventors")]
        public string? Inventors { get; set; }

    }

    public class PatSearchEmailDTO
    {
        public int SearchId { get; set; }
        public string? Title { get; set; }
        public string? Country { get; set; }
        public string? AppnoOrig { get; set; }
        public string? AppDate { get; set; }
        public string? Owners { get; set; }
        public string? Inventors { get; set; }
        public string? LinkUrl { get; set; }
    }
}
