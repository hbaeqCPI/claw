using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.Trademark
{
    public class TmkBudgetManagement : TmkBudgetManagementDetail
    {
        public TmkCountry? TmkCountry { get; set; }
        public TmkCostType? TmkCostType { get; set; }
    }
    public class TmkBudgetManagementDetail : BaseEntity
    {
        [Key]
        [Display(Name = "ID")]
        public int BMId { get; set; }

        public string? CostType { get; set; }

        [StringLength(5)]
        [Display(Name = "Country")]
        public string? Country { get; set; }

        [StringLength(3)]
        [Display(Name = "Case Type")]
        public string? CaseType { get; set; }

        [Display(Name = "From Date")]
        [DisplayFormat(DataFormatString = "{0:dd-MMM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime FromDate { get; set; }

        [Display(Name = "To Date")]
        [DisplayFormat(DataFormatString = "{0:dd-MMM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime ToDate { get; set; }

        [Display(Name = "Forecast Cost")]
        public decimal ForecastCost { get; set; }
        [StringLength(3)]
        [Display(Name = "Currency Type")]
        public string? CurrencyType { get; set; }
    }
}
