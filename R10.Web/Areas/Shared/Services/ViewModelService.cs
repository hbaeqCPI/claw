using AutoMapper;
using AutoMapper.QueryableExtensions;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using R10.Core.Interfaces;
using R10.Core.Services.Shared;
using R10.Web.Areas;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;
using R10.Web.Helpers;
using R10.Web.Interfaces;
using R10.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Services
{
    public class ViewModelService<T> : IViewModelService<T> where T: class
    {
        private readonly ICPiDbContext _cpiDbContext;
        protected readonly IMapper _mapper;

        public ViewModelService(ICPiDbContext cpiDbContext, IMapper mapper)
        {
            _cpiDbContext = cpiDbContext;
            _mapper = mapper;
        }

        public IQueryable<T> AddCriteria(IQueryable<T> list, List<QueryFilterViewModel> mainSearchFilters)
        {
           
            if (mainSearchFilters != null && mainSearchFilters.Count > 0)
            {
                list = QueryHelper.BuildCriteria(list, mainSearchFilters);
            }
            return list;
        }

        public IQueryable<T> AddCriteria(List<QueryFilterViewModel> mainSearchFilters)
        {
            var list = _cpiDbContext.GetRepository<T>().QueryableList;
            return AddCriteria(list, mainSearchFilters);
        }

        public virtual async Task<CPiDataSourceResult> CreateViewModelForGrid(DataSourceRequest request, IQueryable<T> list, string? defaultSortOrder, string? idProperty)
         {
            if (request.Sorts != null && request.Sorts.Any())
                list = list.ApplySorting(request.Sorts);
            else
                list = list.OrderBy(ExpressionHelper.GetPropertyExpression<T>(defaultSortOrder));

            int[] ids;
            int total;
            try
            {
                ids = await list.Select(ExpressionHelper.GetIntPropertyExpression<T>(idProperty)).ToArrayAsync();
                total = ids.Length;
            }
            catch
            {
                // Fallback for non-int key properties (e.g., string keys)
                total = await list.CountAsync();
                ids = Array.Empty<int>();
            }

            return  new CPiDataSourceResult()
            {
                Data = await list.ApplyPaging(request.Page, request.PageSize).ToListAsync(),
                Total = total,
                Ids = ids
            };
        }

        public virtual async Task<CPiDataSourceResult> CreateViewModelForGrid<T2>(DataSourceRequest request, IQueryable<T> list, string? defaultSortOrder, string? idProperty)
        {
            var listVM = list.ProjectTo<T2>();

            if (request.Sorts != null && request.Sorts.Any())
                listVM = listVM.ApplySorting(request.Sorts);
            else
                listVM = listVM.OrderBy(ExpressionHelper.GetPropertyExpression<T2>(defaultSortOrder));

            var ids = await listVM.Select(ExpressionHelper.GetIntPropertyExpression<T2>(idProperty)).ToArrayAsync();

            return new CPiDataSourceResult()
            {
                Data = await listVM.ApplyPaging(request.Page, request.PageSize).ToListAsync(),
                Total = ids.Length,
                Ids = ids
            };
        }


        public virtual async Task<T> GetEntityByCode(string? property, string? value)
        {
            var predicate = ExpressionHelper.BuildPredicate<T>(property, value, false);
            return await _cpiDbContext.GetRepository<T>().QueryableList.Where(predicate).FirstOrDefaultAsync();
        }

        public T2 MapToDomainModel<T2, T3>(T3 vm)
        {
            var model = _mapper.Map<T2>(vm);
            return model;
        }


        //can't use ref parameter (list) on async method
        //protected virtual async Task<int[]> GetIds<T2>(DataSourceRequest request, IQueryable<T2> list, string? defaultSortOrder, string? idProperty)
        //{
        //    if (request.Sorts != null && request.Sorts.Any())
        //        list = list.ApplySorting(request.Sorts);
        //    else
        //        list = list.OrderBy(ExpressionHelper.GetPropertyExpression<T2>(defaultSortOrder));

        //    var ids = await list.Select(ExpressionHelper.GetIntPropertyExpression<T2>(idProperty)).ToArrayAsync();
        //    return ids;
        //}

       

    }
}
