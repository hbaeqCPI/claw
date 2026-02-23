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
    //not used
    public class PatPriorityViewModelService: IPatPriorityViewModelService
    {
        private readonly IPatPriorityRepository _repository;
        private readonly IMapper _mapper;
        private readonly IStringLocalizer<SharedResource> _sharedLocalizer;

        public PatPriorityViewModelService(IPatPriorityRepository repository, IMapper mapper, IStringLocalizer<SharedResource> sharedLocalizer)

        {
            _repository = repository;
            _mapper = mapper;
            _sharedLocalizer = sharedLocalizer;
        }

        public IQueryable<PatPriority> AddCriteria(List<QueryFilterViewModel> mainSearchFilters)
        {
            var priorities = _repository.PatPriorities;
            if (mainSearchFilters != null)
            {
                var moreFilter = mainSearchFilters.ToList();
                priorities = AddCriteria(priorities, moreFilter);
            }
            return priorities;
        }

        public CPiDataSourceResult CreateViewModelForGrid(DataSourceRequest request, IQueryable<PatPriority> priorities)
        {
            if (request.Sorts != null && request.Sorts.Any())
            {
                priorities = priorities.ApplySorting(request.Sorts);
            }
            else
            {
                priorities = priorities.OrderBy(priority => priority.PriId);
            }
            var ids = priorities.Select(i => i.PriId).ToArray();
            var total = ids.Length;

            var priorityVM = priorities.ProjectTo<InventionPriorityViewModel>();
            priorityVM = priorityVM.ApplyPaging(request.Page, request.PageSize);

            var result = new CPiDataSourceResult()
            {
                Data = priorityVM,
                Total = total,
                Ids = ids
            };
            return result;
        }

        private IQueryable<PatPriority> AddCriteria(IQueryable<PatPriority> priorities, List<QueryFilterViewModel> mainSearchFilters)
        {
            var filteredPriorities = QueryHelper.BuildCriteria<PatPriority>(priorities, mainSearchFilters);
            return filteredPriorities;
        }

        public async Task<List<InventionPriorityViewModel>> GetInventionPriorities(int invId)
        {
            var vm = await _repository.PatPriorities.Where(p => p.InvId == invId).ProjectTo<InventionPriorityViewModel>().ToListAsync();
                       
            return vm;
        }
    }
}
