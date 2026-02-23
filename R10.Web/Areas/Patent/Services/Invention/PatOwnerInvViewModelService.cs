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
    public class PatOwnerInvViewModelService : IPatOwnerInvViewModelService
    {
        private readonly IPatOwnerInvRepository _repository;
        private readonly IMapper _mapper;
        private readonly IStringLocalizer<SharedResource> _sharedLocalizer;

        public PatOwnerInvViewModelService(IPatOwnerInvRepository repository, IMapper mapper, IStringLocalizer<SharedResource> sharedLocalizer)

        {
            _repository = repository;
            _mapper = mapper;
            _sharedLocalizer = sharedLocalizer;
        }

        public IQueryable<PatOwnerInv> AddCriteria(List<QueryFilterViewModel> mainSearchFilters)
        {
            var inventionOwners = _repository.PatOwnersInv;
            if (mainSearchFilters != null)
            {
                var moreFilter = mainSearchFilters.ToList();
                inventionOwners = AddCriteria(inventionOwners, moreFilter);
            }
            return inventionOwners;
        }

        public CPiDataSourceResult CreateViewModelForGrid(DataSourceRequest request, IQueryable<PatOwnerInv> inventionOwners)
        {
            if (request.Sorts != null && request.Sorts.Any())
            {
                inventionOwners = inventionOwners.ApplySorting(request.Sorts);
            }
            else
            {
                inventionOwners = inventionOwners.OrderBy(ii => ii.OwnerInvID);
            }
            var ids = inventionOwners.Select(ii => ii.OwnerInvID).ToArray();
            var total = ids.Length;

            var OwnerInvVM = inventionOwners.ProjectTo<InventionOwnerViewModel>();
            OwnerInvVM = OwnerInvVM.ApplyPaging(request.Page, request.PageSize);

            var result = new CPiDataSourceResult()
            {
                Data = OwnerInvVM,
                Total = total,
                Ids = ids
            };
            return result;
        }

        private IQueryable<PatOwnerInv> AddCriteria(IQueryable<PatOwnerInv> priorities, List<QueryFilterViewModel> mainSearchFilters)
        {
            var filteredPriorities = QueryHelper.BuildCriteria<PatOwnerInv>(priorities, mainSearchFilters);
            return filteredPriorities;
        }

        public async Task<List<InventionOwnerViewModel>> GetInventionOwners(int invId)
        {
            var vm = await _repository.PatOwnersInv.Where(p => p.InvId == invId).OrderBy(p => p.OrderOfEntry).ProjectTo<InventionOwnerViewModel>().ToListAsync();
                       
            return vm;
        }
    }
}
