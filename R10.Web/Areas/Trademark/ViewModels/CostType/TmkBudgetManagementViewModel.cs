using R10.Core.Entities.Trademark;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Trademark.ViewModels
{
    public class TmkBudgetManagementViewModel : TmkBudgetManagement
    {
        [Display(Name = "Real Cost")]
        public decimal RealCost { get; set; }
        [Display(Name = "Remaining Amount")]
        public decimal RemainingAmount { get; set; }
    }

    public class TmkBudgetManagementGenerateViewModel : TmkBudgetManagementViewModel
    {
        public TmkBudgetManagementGenerateViewModel()
        {

        }
        public TmkBudgetManagementGenerateViewModel(TmkBudgetManagementViewModel data)
        {
            this.BMId = data.BMId;
            this.CostType = data.CostType;
            this.CaseType = data.CaseType;
            this.Country = data.Country;
            this.CreatedBy = data.CreatedBy;
            this.CurrencyType = data.CurrencyType;
            this.DateCreated = data.DateCreated;
            this.ForecastCost = data.ForecastCost;
            this.FromDate = data.FromDate;
            this.LastUpdate = data.LastUpdate;
            this.TmkCostType = data.TmkCostType;
            this.TmkCountry = data.TmkCountry;
            this.RealCost = data.RealCost;
            this.RemainingAmount = data.RemainingAmount;
            this.ToDate = data.ToDate;
            this.tStamp = data.tStamp;
            this.UpdatedBy = data.UpdatedBy;
        }
        [Display(Name = "Increase Percentage")]
        public decimal IncreasePercentage { get; set; }
        [Display(Name = "This Forecast Cost")]
        public decimal ThisForecastCost { get; set; }
    }
}
