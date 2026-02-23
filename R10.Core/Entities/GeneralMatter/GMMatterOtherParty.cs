using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.GeneralMatter
{
    public class GMMatterOtherParty : BaseEntity
    {
        [Key]
        public int OPID { get; set; }

        [Required]
        public int MatId { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Other Party")]
        [UIHint("GMOtherParty")]
        public string? OtherParty{ get; set; }

        [StringLength(20)]
        [Display(Name = "Type")]
        [UIHint("GMOtherPartyType")]
        public string? OtherPartyType { get; set; }

        public GMMatter? GMMatter { get; set; }
        public GMOtherParty? GMOtherParty { get; set; }
        public GMOtherPartyType? GMOtherPartyType { get; set; }
    }
}
