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
    public class PatInventorAppViewModelService : IPatInventorAppViewModelService
    {
        private readonly IPatInventorAppRepository _repository;
        private readonly IMapper _mapper;
        private readonly IStringLocalizer<SharedResource> _sharedLocalizer;

        public PatInventorAppViewModelService(IPatInventorAppRepository repository, IMapper mapper, IStringLocalizer<SharedResource> sharedLocalizer)

        {
            _repository = repository;
            _mapper = mapper;
            _sharedLocalizer = sharedLocalizer;
        }

        public IQueryable<PatInventorApp> AddCriteria(List<QueryFilterViewModel> mainSearchFilters)
        {
            var countryApplicationInventors = _repository.PatInventorsApp;
            if (mainSearchFilters != null)
            {
                var moreFilter = mainSearchFilters.ToList();
                countryApplicationInventors = AddCriteria(countryApplicationInventors, moreFilter);
            }
            return countryApplicationInventors;
        }

        public CPiDataSourceResult CreateViewModelForGrid(DataSourceRequest request, IQueryable<PatInventorApp> countryApplicationInventors)
        {
            if (request.Sorts != null && request.Sorts.Any())
            {
                countryApplicationInventors = countryApplicationInventors.ApplySorting(request.Sorts);
            }
            else
            {
                countryApplicationInventors = countryApplicationInventors.OrderBy(ii => ii.InventorIDApp);
            }
            var ids = countryApplicationInventors.Select(ii => ii.InventorIDApp).ToArray();
            var total = ids.Length;

            var InventorAppVM = countryApplicationInventors.ProjectTo<CountryApplicationInventorViewModel>();
            InventorAppVM = InventorAppVM.ApplyPaging(request.Page, request.PageSize);

            var result = new CPiDataSourceResult()
            {
                Data = InventorAppVM,
                Total = total,
                Ids = ids
            };
            return result;
        }

        private IQueryable<PatInventorApp> AddCriteria(IQueryable<PatInventorApp> inventorApp, List<QueryFilterViewModel> mainSearchFilters)
        {
            var filteredInventorApp = QueryHelper.BuildCriteria<PatInventorApp>(inventorApp, mainSearchFilters);
            return filteredInventorApp;
        }

        public async Task<List<CountryApplicationInventorViewModel>> GetCountryApplicationInventors(int appId)
        {
            var vm = await _repository.PatInventorsApp.Where(p => p.AppId == appId).OrderBy(p => p.OrderOfEntry).ProjectTo<CountryApplicationInventorViewModel>().ToListAsync();
                       
            return vm;
        }
    }
}
