using AutoMapper;
using AutoMapper.QueryableExtensions;
using Kendo.Mvc.UI;
using Microsoft.EntityFrameworkCore;
using R10.Core.DTOs;
using R10.Core.Entities.Trademark;
using R10.Core.Interfaces;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Areas.Trademark.ViewModels;
using R10.Web.Extensions;
using R10.Web.Helpers;
using R10.Web.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Trademark.Services
{
    public class TmkCostTrackingViewModelService: ITmkCostTrackingViewModelService
    {
        private readonly ICostTrackingService<TmkCostTrack> _costTrackingService;
        private readonly ITmkTrademarkService _trademarkService;
        private readonly IMapper _mapper;

        public TmkCostTrackingViewModelService(ICostTrackingService<TmkCostTrack> costTrackingService, ITmkTrademarkService trademarkService, IMapper mapper)
        {
            _costTrackingService = costTrackingService;
            _trademarkService = trademarkService;
            _mapper = mapper;
        }

        public IQueryable<TmkCostTrack> AddCriteria(List<QueryFilterViewModel> mainSearchFilters, IQueryable<TmkCostTrack> costTracks)
        {
            if (mainSearchFilters.Count > 0)
            {
                var countryOp = mainSearchFilters.GetFilterOperator("CountryOp");
                var country = mainSearchFilters.FirstOrDefault(f => f.Property == "Country");
                if (country != null)
                {
                    country.Operator = countryOp;
                    var countries = country.GetValueList();

                    if (countries.Count > 0)
                    {
                        if (country.Operator == "eq")
                            costTracks = costTracks.Where(c => countries.Contains(c.Country));
                        else
                            costTracks = costTracks.Where(c => !countries.Contains(c.Country));

                        mainSearchFilters.Remove(country);
                    }
                }

                var caseTypeOp = mainSearchFilters.GetFilterOperator("CaseTypeOp");
                var caseType = mainSearchFilters.FirstOrDefault(f => f.Property == "TmkTrademark.CaseType");
                if (caseType != null)
                {
                    caseType.Operator = caseTypeOp;
                    var caseTypes = caseType.GetValueList();

                    if (caseTypes.Count > 0)
                    {
                        if (caseType.Operator == "eq")
                            costTracks = costTracks.Where(c => caseTypes.Contains(c.TmkTrademark.CaseType));
                        else
                            costTracks = costTracks.Where(c => !caseTypes.Contains(c.TmkTrademark.CaseType));

                        mainSearchFilters.Remove(caseType);
                    }
                }

                var costTypeOp = mainSearchFilters.GetFilterOperator("CostTypeOp");
                var costType = mainSearchFilters.FirstOrDefault(f => f.Property == "CostType");
                if (costType != null)
                {
                    costType.Operator = costTypeOp;
                    var costTypes = costType.GetValueList();

                    if (costTypes.Count > 0)
                    {
                        if (costType.Operator == "eq")
                            costTracks = costTracks.Where(c => costTypes.Contains(c.CostType));
                        else
                            costTracks = costTracks.Where(c => !costTypes.Contains(c.CostType));

                        mainSearchFilters.Remove(costType);
                    }
                }

                var invoiceAmountOp = mainSearchFilters.GetFilterOperator("InvoiceAmountOp");
                var invoiceAmount = mainSearchFilters.FirstOrDefault(f => f.Property == "InvoiceAmount");
                if (invoiceAmount != null)
                    invoiceAmount.Operator = invoiceAmountOp;

                var netCostOp = mainSearchFilters.GetFilterOperator("NetCostOp");
                var netCost = mainSearchFilters.FirstOrDefault(f => f.Property == "NetCost");
                if (netCost != null)
                    netCost.Operator = netCostOp;

                if (mainSearchFilters.Any())
                    costTracks = QueryHelper.BuildCriteria<TmkCostTrack>(costTracks, mainSearchFilters);
            }

            return costTracks;
        }

        public async Task<CPiDataSourceResult> CreateViewModelForGrid(DataSourceRequest request, IQueryable<TmkCostTrack> costTracks)
        {
            var model = costTracks.ProjectTo<TmkCostTrackingSearchResultViewModel>();

            if (request.Sorts != null && request.Sorts.Any())
                model = model.ApplySorting(request.Sorts);
            else
                model = model.OrderBy(app => app.CaseNumber).ThenBy(app => app.Country).ThenBy(app => app.SubCase);

            var ids = await model.Select(c => c.CostTrackId).ToArrayAsync();

            return new CPiDataSourceResult()
            {
                Data = await model.ApplyPaging(request.Page, request.PageSize).ToListAsync(),
                Total = ids.Length,
                Ids = ids
            };
        }

        public async Task<TmkCostTrackingDetailViewModel> CreateViewModelForDetailScreen(int id)
        {
            var viewModel = new TmkCostTrackingDetailViewModel();

            if (id > 0)
            {
                viewModel = await _costTrackingService.QueryableList.ProjectTo<TmkCostTrackingDetailViewModel>()
                    .SingleOrDefaultAsync(i => i.CostTrackId == id);

                if (viewModel != null)
                    viewModel.CanModifyAgent = await _costTrackingService.CanModifyAgent(viewModel.AgentID ?? 0);
            }

            return viewModel;
        }

        public TmkCostTrack ConvertViewModelToCostTracking(TmkCostTrackingDetailViewModel viewModel)
        {
            return _mapper.Map<TmkCostTrack>(viewModel);
        }

        public async Task<CaseNumberLookupViewModel> CaseNumberSearchValueMapper(IQueryable<TmkCostTrack> costTracks, string value)
        {
            var result = await _costTrackingService.QueryableList.Where(i => i.CaseNumber == value)
                .Select(ct => new CaseNumberLookupViewModel { Id = ct.CostTrackId, CaseNumber = ct.CaseNumber }).FirstOrDefaultAsync();
            return result;
        }

        public async Task<List<TmkCostTrackingTmkInfoViewModel>> GetTmkInfoList(string caseNumber, string country, string subCase)
        {
            var tmkInfo = await _trademarkService.TmkTrademarks
                .Where(c => c.CaseNumber == caseNumber)
                .ProjectTo<TmkCostTrackingTmkInfoViewModel>()
                .ToListAsync();

            return tmkInfo;
        }

        public IQueryable<TmkCostTrackingTmkInfoViewModel> TmkInfo => _trademarkService.TmkTrademarks.ProjectTo<TmkCostTrackingTmkInfoViewModel>();
    }
}
