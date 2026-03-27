using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities
{
    public class ClawBaseEntity
    {
        [StringLength(20)]
        [Display(Name = "User")]
        public string? UserID { get; set; }

        [Display(Name = "Date Created")]
        public DateTime? DateCreated { get; set; }

        [Display(Name = "Last Update")]
        public DateTime? LastUpdate { get; set; }
    }
}
