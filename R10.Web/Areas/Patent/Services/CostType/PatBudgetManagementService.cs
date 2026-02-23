using R10.Core.Entities.Patent;
using R10.Core.Interfaces;
using R10.Core.Interfaces.Patent;
using R10.Web.Areas.Patent.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Patent.Services
{
    public class PatBudgetManagementService
    {
        private readonly ICostTrackingService<PatCostTrack> _costTrackingService;

        public PatBudgetManagementService(ICostTrackingService<PatCostTrack> costTrackingService)
        {
            _costTrackingService = costTrackingService;
        }

        public decimal GetRealCost(PatCostType costType, PatBudgetManagementViewModel viewModel)
        {
            decimal result = 0;
            var costTrackings = _costTrackingService.QueryableList.Where(c=> c.CostType.Equals(costType.CostType)
            && (String.IsNullOrEmpty(viewModel.Country) || c.Country.Equals(viewModel.Country))
            && (String.IsNullOrEmpty(viewModel.CurrencyType) || c.CurrencyType.Equals(viewModel.CurrencyType))
            && (String.IsNullOrEmpty(viewModel.CaseType) || c.CountryApplication.CaseType.Equals(viewModel.CaseType))
            && (c.InvoiceDate>=viewModel.FromDate) && (c.InvoiceDate<=viewModel.ToDate)
            );
            foreach (PatCostTrack costTracking in costTrackings)
            {
                result += costTracking.InvoiceAmount;
            }

            return result;
        }
    }
}
