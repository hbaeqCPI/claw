using AutoMapper;
using AutoMapper.QueryableExtensions;
using Kendo.Mvc.UI;
using Microsoft.EntityFrameworkCore;
using R10.Core.Entities;
using R10.Core.Interfaces;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;
using R10.Web.Helpers;
using R10.Web.Interfaces.Shared;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Shared.Services
{
    public class DataQueryViewModelService : IDataQueryViewModelService
    {
        private readonly IDataQueryService _dataQueryService;
        private readonly IMapper _mapper;

        public DataQueryViewModelService(
                IDataQueryService dataQueryService,
                IMapper mapper
                )
        {
            _dataQueryService = dataQueryService;
            _mapper = mapper;
        }

        public IQueryable<DataQueryMain> AddCriteria(List<QueryFilterViewModel> mainSearchFilters, IQueryable<DataQueryMain> dataQueriesMain, string userEmail, bool isAdmin = false)
        {
            dataQueriesMain = dataQueriesMain.Where(q => isAdmin || q.IsShared || q.OwnedBy == userEmail);
            if (mainSearchFilters.Count > 0)
            {
                var dqCatIdFilter = mainSearchFilters.FirstOrDefault(f => f.Property == "DQCatId_Search");
                if (dqCatIdFilter != null)
                { 
                    dataQueriesMain = dataQueriesMain.Where(q => q.DQCatId == Int32.Parse(dqCatIdFilter.Value));
                    mainSearchFilters.Remove(dqCatIdFilter);
                }

                var tag = mainSearchFilters.FirstOrDefault(f => f.Property == "Tag");
                if (tag != null)
                {
                    var tagsList = "";
                    if (tag != null)
                    {
                        var tags = tag.GetValueListForLoop();
                        if (tags.Count > 1)
                        {
                            foreach (var val in tags)
                            {
                                tagsList = tagsList + val + "~";
                            }
                        }
                    }

                    dataQueriesMain = dataQueriesMain.Where(dq => tag == null
                                                    || (string.IsNullOrEmpty(tagsList)
                                                        && dq.DataQueryTags.Any(t => EF.Functions.Like(t.Tag, tag.Value)))
                                                    || (!string.IsNullOrEmpty(tagsList)
                                                        && dq.DataQueryTags.Any(t => EF.Functions.Like(tagsList, '%' + t.Tag + '%')))
                                            );

                    mainSearchFilters.Remove(tag);
                }

                var showOnlyMyQueriesFilter = mainSearchFilters.FirstOrDefault(f => f.Property == "ShowOnlyMyQueries");
                var showOnlyMyQueries = showOnlyMyQueriesFilter != null;
                if (showOnlyMyQueries)
                {
                    dataQueriesMain = dataQueriesMain.Where(q => q.OwnedBy == userEmail);
                    mainSearchFilters.Remove(showOnlyMyQueriesFilter);
                }
                if (mainSearchFilters.Any())
                    dataQueriesMain = QueryHelper.BuildCriteria<DataQueryMain>(dataQueriesMain, mainSearchFilters);
            }
            return dataQueriesMain;
        }

        public async Task<CPiDataSourceResult> CreateViewModelForGrid(DataSourceRequest request, IQueryable<DataQueryMain> dataQueriesMain)
        {
            var model = dataQueriesMain.ProjectTo<DataQuerySearchResultViewModel>();
            if (request.Sorts != null && request.Sorts.Any())
                model = model.ApplySorting(request.Sorts);
            else
                model = model.OrderBy(q => q.QueryName);

            var ids = await model.Select(q => q.QueryId).ToArrayAsync();

            return new CPiDataSourceResult()
            {
                Data = await model.ApplyPaging(request.Page, request.PageSize).ToListAsync(),
                Total = ids.Length,
                Ids = ids
            };

        }

        public async Task<DataQueryDetailViewModel> CreateViewModelForDetailScreen(int id)
        {
            var viewModel = new DataQueryDetailViewModel();

            if (id > 0)
            {
                viewModel = await _dataQueryService.DataQueriesMain.ProjectTo<DataQueryDetailViewModel>()
                                .SingleOrDefaultAsync(q => q.QueryId == id);
            }
            
            return viewModel;
        }

        public string GetSQLExpr(int id)
        {
            var sql = "";
            if (id > 0)
            {
                sql = _dataQueryService.DataQueriesMain.SingleOrDefault(q => q.QueryId == id).SQLExpr;
            }
            return sql;
        }

    }
}
