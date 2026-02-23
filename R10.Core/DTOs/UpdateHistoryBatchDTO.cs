using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace R10.Core.DTOs
{
    [Keyless]
    public class UpdateHistoryBatchDTO
    {
        public int JobId { get; set; }

        [Display(Name="Changed On")]
        public DateTime ChangeDate { get; set; }

    }
}
