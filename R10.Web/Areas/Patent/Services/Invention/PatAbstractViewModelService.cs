using AutoMapper;
using AutoMapper.QueryableExtensions;
using Kendo.Mvc.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using R9.Core.Entities.Patent;
using R9.Core.Interfaces;
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
    public class PatAbstractViewModelService: IPatAbstractViewModelService
    {
        private readonly IChildEntityService<Invention, PatAbstract> _abstractService;
        private readonly IMapper _mapper;
        private readonly IStringLocalizer<SharedResource> _sharedLocalizer;

        public PatAbstractViewModelService(IChildEntityService<Invention, PatAbstract> abstractService, IMapper mapper, IStringLocalizer<SharedResource> sharedLocalizer)

        {
            _abstractService = abstractService;
            _mapper = mapper;
            _sharedLocalizer = sharedLocalizer;
        }

        public IQueryable<PatAbstract> AddCriteria(List<QueryFilterViewModel> mainSearchFilters)
        {
            var abstracts = _abstractService.QueryableList;
            if (mainSearchFilters != null)
            {
                var moreFilter = mainSearchFilters.ToList();
                abstracts = AddCriteria(abstracts, moreFilter);
            }
            return abstracts;
        }

        public CPiDataSourceResult CreateViewModelForGrid(DataSourceRequest request, IQueryable<PatAbstract> abstracts)
        {
            if (request.Sorts != null && request.Sorts.Any())
            {
                abstracts = abstracts.ApplySorting(request.Sorts);
            }
            else
            {
                abstracts = abstracts.OrderBy(a => a.AbstractId);
            }
            var ids = abstracts.Select(a => a.AbstractId).ToArray();
            var total = ids.Length;

            var abstractVM = abstracts.ProjectTo<InventionAbstractViewModel>();
            abstractVM = abstractVM.ApplyPaging(request.Page, request.PageSize);

            var result = new CPiDataSourceResult()
            {
                Data = abstractVM,
                Total = total,
                Ids = ids
            };
            return result;
        }
     

        public async Task<List<InventionAbstractViewModel>> GetInventionAbstracts(int invId)
        {
            var vm = await _abstractService.QueryableList.Where(p => p.InvId == invId).ProjectTo<InventionAbstractViewModel>().ToListAsync();

            return vm;
        }

        private IQueryable<PatAbstract> AddCriteria(IQueryable<PatAbstract> abstracts, List<QueryFilterViewModel> mainSearchFilters)
        {
            var filteredPriorities = QueryHelper.BuildCriteria<PatAbstract>(abstracts, mainSearchFilters);
            return filteredPriorities;
        }
    }
}
