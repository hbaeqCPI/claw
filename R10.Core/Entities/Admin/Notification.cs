using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities
{
    public class Notification : BaseEntity
    {
        [Key]
        public int MessageId { get; set; }

        [StringLength(50)]
        [Display(Name = "Category")]
        public string? Category { get; set; }

        [StringLength(1)]
        [Display(Name = "Type")]
        [UIHint("NotificationType")]
        public string? Type { get; set; }

        [StringLength(50)]
        [Display(Name = "Title")]
        [Required]
        public string Title { get; set; }

        [StringLength(2000)]
        [Display(Name = "Message")]
        [Required]
        public string Message { get; set; }

        [Display(Name = "Recipient")]
        [Required]
        public string UserName { get; set; }

        [Display(Name = "Effective From")]
        public DateTime EffectiveFrom { get; set; }

        [Display(Name = "Effective To")]
        public DateTime? EffectiveTo { get; set; }

        [StringLength(255)]
        [Display(Name = "Navigate To Url")]
        public string? NavigateToUrl { get; set; }

        [Display(Name = "Viewed")]
        public bool Viewed { get; set; }

        
    }
}
