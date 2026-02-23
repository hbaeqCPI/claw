using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.AMS
{
    public class AMSVATRate : AMSVATRateDetail
    {
        public PatCountry? PatCountry { get; set; }
    }
    public class AMSVATRateDetail : BaseEntity
    {
        public int VATRateId { get; set; }

        [Required]
        [StringLength(5)]
        [Display(Name = "Country")]
        public string Country { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "VAT Name")]
        public string VATName { get; set; }

        [Display(Name = "Local")]
        public bool LocalVAT { get; set; }

        [Display(Name = "Local VAT Rate (%)")]
        public decimal LocalVATRate { get; set; }

        [Display(Name = "Apply to Official Fee")]
        public bool LocalVATonPTOFee { get; set; }

        [Display(Name = "Apply to Service Fee")]
        public bool LocalVATonServiceFee { get; set; }

        [Display(Name = "Foreign")]
        public bool ForeignVAT { get; set; }

        [Display(Name = "Foreign VAT Rate (%)")]
        public decimal ForeignVATRate { get; set; }

        [Display(Name = "Apply to Official Fee")]
        public bool ForeignVATonPTOFee { get; set; }

        [Display(Name = "Apply to Service Fee")]
        public bool ForeignVATonServiceFee { get; set; }

    }
}
