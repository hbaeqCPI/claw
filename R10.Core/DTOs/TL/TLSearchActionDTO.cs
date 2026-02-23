using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace R10.Core.DTOs
{
    [Keyless]
    public class TLSearchActionAsDownloadedDTO
    {
        public int TLTmkId { get; set; }

        [Display(Name = "Action")]
        public string? SearchAction { get; set; }

        [Display(Name = "Base Date")]
        public DateTime BaseDate { get; set; }

        [Display(Name = "Due Date")]
        public DateTime? DueDate { get; set; }

        [Display(Name = "Completed Date")]
        public DateTime? DateCompleted { get; set; }

        public string? TMSAction { get; set; }
    }
    
}
