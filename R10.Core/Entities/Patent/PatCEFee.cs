using R10.Core.Entities.AMS;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Core.Entities.Patent
{
    public class PatCEFee : BaseEntity
    {
        [Key]
        public int FeeSetupId { get; set; }
        
        [StringLength(10)]
        [Display(Name = "Setup Name")]
        public string CEFeeSetupName { get; set; }

        [StringLength(255)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        public List<PatCEFeeDetail>? PatCEFeeDetail { get; set; }
        public List<Client>? Client { get; set; }
    }

}
