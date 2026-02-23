using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities
{
    public class DataQueryMain : BaseEntity
    {
        [Key]
        public int QueryId { get; set; }

        [Required(ErrorMessage = "Query Name is required.")]
        [MaxLength(50)]
        [Display(Name = "Query Name")]
        public string?  QueryName { get; set; }

        [Required]
        [StringLength(256)]
        [Display(Name = "Owner Name")]
        public string?  OwnedBy { get; set; }

        [Display(Name = "Share Query")]
        public bool IsShared { get; set; }

        [Display(Name = "Allow Others to Edit")]
        public bool IsEditable { get; set; }
        
        [Display(Name = "Used In Widget")]
        public bool UsedInWidget { get; set; }
        
        public string?  SQLExpr { get; set; }

        public string?  Remarks { get; set; }

        [Display(Name = "Category")]
        public int? DQCatId { get; set; }

        public List<DataQueryTag>? DataQueryTags { get; set; }
        public DataQueryCategory? DataQueryCategory { get; set; }
    }
}
