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
    public class QEDataSourceViewModelService : IQEDataSourceViewModelService
    {
        private readonly IQuickEmailService _qeService;
        private readonly IMapper _mapper;

        public QEDataSourceViewModelService(
                IQuickEmailService qeService,
                IMapper mapper
                )
        {
            _qeService = qeService;
            _mapper = mapper;

        }

        public IQueryable<QEDataSource> AddCriteria(IQueryable<QEDataSource> dataSources, List<QueryFilterViewModel> mainSearchFilters)
        {
            if (mainSearchFilters.Count > 0)
            {
                var systemType = mainSearchFilters.FirstOrDefault(f => f.Property == "SystemType");
                if (systemType != null)
                {
                    dataSources = dataSources.Where(ds => ds.SystemType == systemType.Value);
                    mainSearchFilters.Remove(systemType);
                }
            }
            if (mainSearchFilters.Any())
                dataSources = QueryHelper.BuildCriteria(dataSources, mainSearchFilters);

            return dataSources;
        }

        public async Task<CPiDataSourceResult> CreateViewModelForSearchGrid(DataSourceRequest request, IQueryable<QEDataSource> dataSources)
        {
            var model = dataSources.ProjectTo<QEDataSourceSearchResultViewModel>();
            if (request.Sorts != null && request.Sorts.Any())
                model = model.ApplySorting(request.Sorts);
            else
                model = model.OrderBy(l => l.DataSourceName);

            //var ids = await model.Select(l => l.LetId).ToArrayAsync();
            var recCount = await model.Select(l => l.DataSourceID).CountAsync();

            return new CPiDataSourceResult()
            {
                Data = await model.ApplyPaging(request.Page, request.PageSize).ToListAsync(),
                Total = recCount
                //Ids = ids
            };

        }

        public async Task<QEDataSourceDetailViewModel> CreateViewModelForDetailScreen(int id)
        {
            var viewModel = new QEDataSourceDetailViewModel();

            if (id > 0)
            {
                viewModel = await _qeService.QEDataSources.ProjectTo<QEDataSourceDetailViewModel>()
                                .SingleOrDefaultAsync(ds => ds.DataSourceID == id);
            }
            return viewModel;
        }
    }
}
