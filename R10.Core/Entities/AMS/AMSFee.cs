using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.AMS
{
    public class AMSFee : BaseEntity
    {
        public int FeeSetupId { get; set; }

        [Key]
        [StringLength(10)]
        [Display(Name = "Setup Name")]
        public string? FeeSetupName { get; set; }

        [StringLength(30)]
        public string? Description { get; set; }

        public List<AMSFeeDetail>? AMSFeeDetail { get; set; }
        public List<Client>? Client { get; set; }
    }
}
