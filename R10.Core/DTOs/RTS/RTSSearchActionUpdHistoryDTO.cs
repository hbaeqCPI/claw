using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace R10.Core.DTOs
{
    [Keyless]
    public class RTSSearchActionUpdHistoryDTO
    {
        public int PLAppId { get; set; }

        [Display(Name="Changed On")]
        public DateTime ChangeDate { get; set; }
        
        public string? UpdateSource { get; set; }

        [Display(Name = "Patent Office Action")]
        public string? PLAction { get; set; }

        [Display(Name = "Your Action")]
        public string? PMSAction { get; set; }

        [Display(Name = "Reverted On")]
        public DateTime? UndoDate { get; set; }

        [Display(Name = "Reverted By")]
        public string? UndoUserId { get; set; }

    }
}
