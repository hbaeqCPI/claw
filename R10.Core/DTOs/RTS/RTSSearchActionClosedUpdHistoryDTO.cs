using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace R10.Core.DTOs
{
    [Keyless]
    public class RTSSearchActionClosedUpdHistoryDTO
    {
        public int PLAppId { get; set; }

        [Display(Name="Changed On")]
        public DateTime ChangeDate { get; set; }

        [Display(Name = "Action Type")]
        public string? PMSActionType { get; set; }

        [Display(Name = "Action Due")]
        public string? PMSActionDue { get; set; }

        [Display(Name = "Indicator")]
        public string? PMSIndicator { get; set; }

        [Display(Name = "Date Taken")]
        public DateTime? DateTaken { get; set; }


    }
}
