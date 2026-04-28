using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace LawPortal.Core.DTOs
{
    [Keyless]
    public class WebLinksDTO
    {
        public int ModuleId { get; set; }
        public int MainId { get; set; }
        public bool NavigateFamily { get; set; }

        [Display(Name="Main Code")]
        public string? MainCode { get; set; }
        public string? Country { get; set; }

        [Display(Name = "Description")]
        public string? Description { get; set; }
        public string? ClientDescription { get; set; }
        public string? CaseTypeIncludeExclude { get; set; }
        public string? CaseTypeFilter { get; set; }
        public string? Remarks { get; set; }
        public bool RecordLink { get; set; }
        public string? SubSystem { get; set; }

    }
   
}
