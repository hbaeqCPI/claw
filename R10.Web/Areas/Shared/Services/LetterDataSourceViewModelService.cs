using AutoMapper;
using AutoMapper.QueryableExtensions;
using Kendo.Mvc.UI;
using Microsoft.EntityFrameworkCore;
using R10.Core.Entities;
using R10.Core.Interfaces;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;
using R10.Web.Helpers;
using R10.Web.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Shared.Services
{
    public class LetterDataSourceViewModelService : ILetterDataSourceViewModelService
    {
        private readonly ILetterService _letterService;
        private readonly IMapper _mapper;

        public LetterDataSourceViewModelService(
                ILetterService letterService,
                IMapper mapper
                )
        {
            _letterService = letterService;
            _mapper = mapper;

        }

        public IQueryable<LetterDataSource> AddCriteria(IQueryable<LetterDataSource> dataSources, List<QueryFilterViewModel> mainSearchFilters)
        {
            if (mainSearchFilters.Count > 0)
            {
                var systemType = mainSearchFilters.FirstOrDefault(f => f.Property == "SystemType");
                if (systemType != null)
                {
                    dataSources = dataSources.Where(ds => ds.SystemType == systemType.Value);
                    mainSearchFilters.Remove(systemType);
                }
                var letCategory = mainSearchFilters.FirstOrDefault(f => f.Property == "LetterCategory.LetCatDesc");
                if (letCategory != null)
                {
                    dataSources = dataSources.Where(ds => ds.DataSourceDescMain.Contains(letCategory.Value.ToString().Replace(" Letters", "")));
                    mainSearchFilters.Remove(letCategory);
                }
            }
            if (mainSearchFilters.Any())
                dataSources = QueryHelper.BuildCriteria(dataSources, mainSearchFilters);

            return dataSources;
        }

        public async Task<CPiDataSourceResult> CreateViewModelForSearchGrid(DataSourceRequest request, IQueryable<LetterDataSource> dataSources)
        {
            var model = dataSources.ProjectTo<LetterDataSourceSearchResultViewModel>();
            if (request.Sorts != null && request.Sorts.Any())
                model = model.ApplySorting(request.Sorts);
            else
                model = model.OrderBy(l => l.DataSourceDescMain);

            //var ids = await model.Select(l => l.LetId).ToArrayAsync();
            var recCount = await model.Select(l => l.DataSourceId).CountAsync();

            return new CPiDataSourceResult()
            {
                Data = await model.ApplyPaging(request.Page, request.PageSize).ToListAsync(),
                Total = recCount
                //Ids = ids
            };

        }

        public async Task<LetterDataSourceDetailViewModel> CreateViewModelForDetailScreen(int id)
        {
            var viewModel = new LetterDataSourceDetailViewModel();

            if (id > 0)
            {
                viewModel = await _letterService.LetterDataSources.ProjectTo<LetterDataSourceDetailViewModel>()
                                .SingleOrDefaultAsync(ds => ds.DataSourceId == id);
            }
            return viewModel;
        }
    }
}
