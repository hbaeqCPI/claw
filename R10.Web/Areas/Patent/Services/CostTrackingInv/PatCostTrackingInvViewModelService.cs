using Kendo.Mvc.UI;
using R10.Core.DTOs;
using R10.Core.Entities.Patent;
using R10.Core.Interfaces.Patent;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;
using R10.Web.Helpers;
using R10.Web.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using R10.Web.Areas.Patent.ViewModels;
using Microsoft.EntityFrameworkCore;
using R10.Core.Interfaces;

namespace R10.Web.Areas.Patent.Services
{
    public class PatCostTrackingInvViewModelService : IPatCostTrackingInvViewModelService
    {
        private readonly ICostTrackingService<PatCostTrackInv> _costTrackingService;
        private readonly IInventionService _inventionService;
        private readonly IMapper _mapper;

        public PatCostTrackingInvViewModelService(ICostTrackingService<PatCostTrackInv> costTrackingService, IInventionService inventionService, IMapper mapper)
        {
            _costTrackingService = costTrackingService;
            _inventionService = inventionService;
            _mapper = mapper;
        }

        public IQueryable<PatCostTrackInv> AddCriteria(List<QueryFilterViewModel> mainSearchFilters, IQueryable<PatCostTrackInv> costTracks)
        {
            if (mainSearchFilters.Count > 0)
            {
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
                    costTracks = QueryHelper.BuildCriteria<PatCostTrackInv>(costTracks, mainSearchFilters);
            }

            return costTracks;
        }

        public async Task<CPiDataSourceResult> CreateViewModelForGrid(DataSourceRequest request, IQueryable<PatCostTrackInv> costTracks)
        {
            var model = costTracks.ProjectTo<PatCostTrackingInvSearchResultViewModel>();

            if (request.Sorts != null && request.Sorts.Any())
                model = model.ApplySorting(request.Sorts);
            else
                model = model.OrderBy(app => app.CaseNumber);

            var ids = await model.Select(c => c.CostTrackInvId).ToArrayAsync();

            return new CPiDataSourceResult()
            {
                Data = await model.ApplyPaging(request.Page, request.PageSize).ToListAsync(),
                Total = ids.Length,
                Ids = ids
            };
        }

        public async Task<PatCostTrackingInvDetailViewModel> CreateViewModelForDetailScreen(int id)
        {
            var viewModel = new PatCostTrackingInvDetailViewModel();

            if (id > 0)
            {
                viewModel = await _costTrackingService.QueryableList.ProjectTo<PatCostTrackingInvDetailViewModel>()
                    .SingleOrDefaultAsync(i => i.CostTrackInvId == id);

                if (viewModel != null)
                    viewModel.CanModifyAgent = await _costTrackingService.CanModifyAgent(viewModel.AgentID ?? 0);
            }

            return viewModel;
        }

        public PatCostTrackInv ConvertViewModelToCostTracking(PatCostTrackingInvDetailViewModel viewModel)
        {
            return _mapper.Map<PatCostTrackInv>(viewModel);
        }

        public async Task<CaseNumberLookupViewModel> CaseNumberSearchValueMapper(IQueryable<PatCostTrackInv> costTracks, string value)
        {
            var result = await _costTrackingService.QueryableList.Where(i => i.CaseNumber == value)
                .Select(ct => new CaseNumberLookupViewModel { Id = ct.CostTrackInvId, CaseNumber = ct.CaseNumber }).FirstOrDefaultAsync();
            return result;
        }

        public async Task<List<PatCostTrackingInvInvInfoViewModel>> GetInvInfoList(string caseNumber)
        {
            var invInfo = await _inventionService.Inventions
                .Where(c => c.CaseNumber == caseNumber)
                .ProjectTo<PatCostTrackingInvInvInfoViewModel>()
                .ToListAsync();

            return invInfo;
        }

        public IQueryable<PatCostTrackingInvInvInfoViewModel> InvInfo => _inventionService.Inventions.ProjectTo<PatCostTrackingInvInvInfoViewModel>();
    }
}