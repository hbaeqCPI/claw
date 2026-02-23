using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.GeneralMatter
{
    public class GMOtherPartyType : BaseEntity
    {
        public int TypeID { get; set; }

        [Key]
        [Required]
        [StringLength(20)]
        [Display(Name = "Other Party Type")]
        public string? OtherPartyType { get; set; }

        [StringLength(255)]
        [Display(Name = "Description")]
        public string? Description { get; set; }
        
        public List<GMMatterOtherParty>? GMMatterOtherParties { get; set; }
    }
}
