using R10.Core.Entities.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.Trademark
{
    public class TmkSetting : DefaultSetting
    {
        public bool IsMultipleOwnerOn { get; set; }
        public bool IsFamilyNumberOn { get; set; }
        public string? ClientMatterDivider { get; set; }
        public string? CountryCopyRanges { get; set; }
        public string? CountriesThatAllowMultipleDesignation { get; set; }

        [Display(Description = "Trademark Links", GroupName = "Modules")]
        public bool IsTLOn { get; set; }

        public string? ExcelExportTemplateFolder { get; set; }
        public string? CountryLawUpdateURL { get; set; }
        public string? HomeCountry { get; set; }
        public string? TmkCustomFieldsTabLabel { get; set; }

        public bool IsTLUpdateWorkflowOn { get; set; }
        public bool IsTLUpdateWorkflowEmailOn { get; set; }
        public int TLUpdateWorkflowCutOff { get; set; }
        public bool IsTLUpdateWorkflowDateCheckOn { get; set; }
        public int DefaultBillingAttorney { get; set; }

        public string? CountryLawDocTemplate { get; set; }

        [Display(Description = "Cost Estimator", GroupName = "Modules")]
        public bool IsCostEstimatorOn { get; set; }

        public string? CostEstimatorCurrencyFormat { get; set; }
        public string? TrademarkWatchRecipients { get; set; }
    }
}
