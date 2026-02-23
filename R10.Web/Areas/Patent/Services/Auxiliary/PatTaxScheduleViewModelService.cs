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
    public class PatTaxScheduleViewModelService : IPatTaxScheduleViewModelService
    {
        private readonly IParentEntityService<PatTaxBase, PatTaxYear> _taxBaseService;

        public PatTaxScheduleViewModelService(IParentEntityService<PatTaxBase, PatTaxYear> taxBaseService)

        {
            _taxBaseService = taxBaseService;
        }

        public IQueryable<PatTaxBase> AddCriteria(List<QueryFilterViewModel> mainSearchFilters)
        {
            var patTaxBases = _taxBaseService.QueryableList;
            if (mainSearchFilters != null)
            {
                var moreFilter = mainSearchFilters.ToList();
                patTaxBases = AddCriteria(patTaxBases, moreFilter);
            }
            return patTaxBases;
        }

        public async Task<CPiDataSourceResult> CreateViewModelForGrid(DataSourceRequest request, IQueryable<PatTaxBase> patTaxBases)
        {
            var model = patTaxBases.ProjectTo<PatTaxScheduleSearchResultViewModel>();

            if (request.Sorts != null && request.Sorts.Any())
                model = model.ApplySorting(request.Sorts);
            else
                model = model.OrderBy(m => m.TaxSchedule);

            var ids = await model.Select(m => m.TaxBID).ToArrayAsync();

            return new CPiDataSourceResult()
            {
                Data = model.ApplyPaging(request.Page, request.PageSize),
                Total = ids.Length,
                Ids = ids
            };
        }

        public async Task<PatTaxScheduleDetailViewModel> CreateViewModelForDetailScreen(int id)
        {
            var patTaxBase = await _taxBaseService.QueryableList.ProjectTo<PatTaxScheduleDetailViewModel>().SingleOrDefaultAsync(m => m.TaxBID == id);
            return patTaxBase;
        }

        private IQueryable<PatTaxBase> AddCriteria(IQueryable<PatTaxBase> patTaxBases, List<QueryFilterViewModel> mainSearchFilters)
        {
            var filteredPatTaxBases = QueryHelper.BuildCriteria<PatTaxBase>(patTaxBases, mainSearchFilters);
            return filteredPatTaxBases;
        }
    }
}
