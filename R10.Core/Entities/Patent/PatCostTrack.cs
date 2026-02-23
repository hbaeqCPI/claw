using R10.Core.Entities.Documents;
using R10.Core.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace R10.Core.Entities.Patent
{

    public class PatCostTrack : PatCostTrackDetail
    {
        public CountryApplication? CountryApplication { get; set; }

        public Agent? Agent { get; set; }
        public Attorney? BillingAttorney { get; set; }
        public PatCostType? PatCostType { get; set; }
        public CurrencyType? PatCurrencyType { get; set; }
        public PatCountry? PatCountry { get; set; }
        //public CPiUser? CPiUser { get; set; }

    //public List<PatImageCost>? Images { get; set; }

    [NotMapped]
        public List<DocFolder>? DocFolders { get; set; }
        
    }

    public class PatCostTrackDetail : BaseEntity
    {
        [Key]
        public int CostTrackId { get; set; }
        public int AppId { get; set; }

        [Required, StringLength(25)]
        public string CaseNumber { get; set; }

        [Required, StringLength(5)]
        public string Country { get; set; }

        [StringLength(8)]
        [Display(Name = "Sub Case")]
        public string? SubCase { get; set; }

        
        [Required, StringLength(30), Display(Name = "Cost Type")]
        public string? CostType { get; set; }

        [Required, Display(Name = "Invoice Date")]
        public DateTime InvoiceDate { get; set; }

        [Required, StringLength(50), Display(Name = "Invoice Number")]
        public string? InvoiceNumber { get; set; }

        [Display(Name = "Invoice Amount")]
        public decimal InvoiceAmount { get; set; }

        [Display(Name = "Payment Date")]
        public DateTime? PayDate { get; set; }

        [Display(Name = "Currency Type")]
        public string? CurrencyType { get; set; }

        [Display(Name = "Exchange Rate")]
        public double? ExchangeRate { get; set; }

        [Display(Name = "Allowance Rate")]
        public double? AllowanceRate { get; set; }

        [Display(Name = "Net Cost")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public double NetCost { get; set; }

        [Display(Name = "Agent")]
        public int? AgentID { get; set; }

        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }

        [Display(Name = "Billing Attorney")]
        public int? BillingAttorneyId { get; set; }

        [Display(Name = "Hourly Rate")]
        public decimal? HourlyRate { get; set; }

        [Display(Name = "Billing User")]
        public int? BillingUserPkId { get; set; }
    }

}
