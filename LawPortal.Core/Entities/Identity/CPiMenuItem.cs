using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace LawPortal.Core.Entities
{
    public class CPiMenuItem : BaseEntity
    {
        [Key]
        [StringLength(450)]
        public String Id { get; set; }

        [StringLength(450)]
        public string ParentId { get; set; }

        [StringLength(256)]
        [Required]
        public string Title { get; set; }

        [Display(Name = "Page")]
        public int? PageId { get; set; }

        [StringLength(256)]
        public string? Url { get; set; }

        public int SortOrder { get; set; }

        public bool IsEnabled { get; set; }

        [Display(Name = "Open In New Window")]
        public bool? OpenInNewWindow { get; set; }

        [StringLength(450)]
        public string? Policy { get; set; }

        public CPiMenuPage? Page { get; set; }
    }
}
