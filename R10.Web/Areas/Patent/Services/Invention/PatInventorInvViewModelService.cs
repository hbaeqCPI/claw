using AutoMapper;
using AutoMapper.QueryableExtensions;
using Kendo.Mvc.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using R9.Core.Entities.Patent;
using R9.Core.Interfaces.Patent;
using R9.Web.Areas.Patent.ViewModels;
using R9.Web.Areas.Shared.ViewModels;
using R9.Web.Extensions;
using R9.Web.Helpers;
using R9.Web.Interfaces;
using R9.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R9.Web.Areas.Patent.Services
{
    public class PatInventorInvViewModelService : IPatInventorInvViewModelService
    {
        private readonly IPatInventorInvRepository _repository;
        private readonly IMapper _mapper;
        private readonly IStringLocalizer<SharedResource> _sharedLocalizer;

        public PatInventorInvViewModelService(IPatInventorInvRepository repository, IMapper mapper, IStringLocalizer<SharedResource> sharedLocalizer)

        {
            _repository = repository;
            _mapper = mapper;
            _sharedLocalizer = sharedLocalizer;
        }

        public IQueryable<PatInventorInv> AddCriteria(List<QueryFilterViewModel> mainSearchFilters)
        {
            var inventionInventors = _repository.PatInventorsInv;
            if (mainSearchFilters != null)
            {
                var moreFilter = mainSearchFilters.ToList();
                inventionInventors = AddCriteria(inventionInventors, moreFilter);
            }
            return inventionInventors;
        }

        public CPiDataSourceResult CreateViewModelForGrid(DataSourceRequest request, IQueryable<PatInventorInv> inventionInventors)
        {
            if (request.Sorts != null && request.Sorts.Any())
            {
                inventionInventors = inventionInventors.ApplySorting(request.Sorts);
            }
            else
            {
                inventionInventors = inventionInventors.OrderBy(ii => ii.InventorInvID);
            }
            var ids = inventionInventors.Select(ii => ii.InventorInvID).ToArray();
            var total = ids.Length;

            var inventorInvVM = inventionInventors.ProjectTo<InventionInventorViewModel>();
            inventorInvVM = inventorInvVM.ApplyPaging(request.Page, request.PageSize);

            var result = new CPiDataSourceResult()
            {
                Data = inventorInvVM,
                Total = total,
                Ids = ids
            };
            return result;
        }

        private IQueryable<PatInventorInv> AddCriteria(IQueryable<PatInventorInv> priorities, List<QueryFilterViewModel> mainSearchFilters)
        {
            var filteredPriorities = QueryHelper.BuildCriteria<PatInventorInv>(priorities, mainSearchFilters);
            return filteredPriorities;
        }

        public async Task<List<InventionInventorViewModel>> GetInventionInventors(int invId)
        {
            var vm = await _repository.PatInventorsInv.Where(p => p.InvId == invId).OrderBy(p => p.OrderOfEntry).ProjectTo<InventionInventorViewModel>().ToListAsync();
                       
            return vm;
        }
    }
}
