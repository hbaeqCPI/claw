using R10.Core.Entities.Trademark;
using R10.Core.Interfaces;
using R10.Web.Areas.Trademark.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Trademark.Services
{
    public class TmkBudgetManagementService
    {
        private readonly ICostTrackingService<TmkCostTrack> _costTrackingService;

        public TmkBudgetManagementService(ICostTrackingService<TmkCostTrack> costTrackingService)
        {
            _costTrackingService = costTrackingService;
        }

        public decimal GetRealCost(TmkCostType costType, TmkBudgetManagementViewModel viewModel)
        {
            decimal result = 0;
            var costTrackings = _costTrackingService.QueryableList.Where(c => c.CostType.Equals(costType.CostType)
            && (String.IsNullOrEmpty(viewModel.Country) || c.Country.Equals(viewModel.Country))
            && (String.IsNullOrEmpty(viewModel.CurrencyType) || c.CurrencyType.Equals(viewModel.CurrencyType))
            && (String.IsNullOrEmpty(viewModel.CaseType) || c.TmkTrademark.CaseType.Equals(viewModel.CaseType))
            && (c.InvoiceDate >= viewModel.FromDate) && (c.InvoiceDate <= viewModel.ToDate)
            );
            foreach (TmkCostTrack costTracking in costTrackings)
            {
                result += costTracking.InvoiceAmount;
            }

            return result;
        }
    }
}
