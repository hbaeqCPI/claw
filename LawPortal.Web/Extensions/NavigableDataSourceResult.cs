using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LawPortal.Web.Extensions
{
    /// <summary>
    /// Turns an in-memory grid result into one the detail-page record navigator can walk.
    /// </summary>
    public static class NavigableDataSourceResult
    {
        /// <summary>
        /// Like Kendo's ToDataSourceResult, but also emits a
        /// <see cref="RecordNavigationKey"/> per row so the record navigator has
        /// something to navigate by on screens whose records are not keyed on a single
        /// int. For the aux screens that page in memory (Area Country, Des Case Type and
        /// friends), which return Kendo's plain result and therefore carry neither Ids
        /// nor Keys.
        /// </summary>
        /// <param name="keySpecs">
        /// "queryName:Property" pairs naming the record — take them from the screen's own
        /// search-grid row link. See <see cref="RecordNavigationKey"/>.
        /// </param>
        public static CPiDataSourceResult ToNavigableDataSourceResult<T>(
            this IEnumerable<T> data, DataSourceRequest request, params string[] keySpecs)
        {
            var paged = data.ToDataSourceResult(request);

            // The keys must be in the order the grid shows, across ALL pages: the
            // navigator steps through them by position. Sort and filter the same way,
            // but without paging, so key[n] is the nth row of the result set.
            var ordered = data.ToDataSourceResult(new DataSourceRequest
            {
                Sorts = request.Sorts,
                Filters = request.Filters
            });

            var keys = RecordNavigationKey.BuildAll(ordered.Data.Cast<T>(), keySpecs);

            return new CPiDataSourceResult
            {
                Data = paged.Data,
                Total = paged.Total,
                Ids = Array.Empty<int>(),
                Keys = keys
            };
        }

        /// <summary>
        /// IQueryable counterpart, for the screens that read with
        /// ToDataSourceResultAsync.
        /// </summary>
        public static async Task<CPiDataSourceResult> ToNavigableDataSourceResultAsync<T>(
            this IQueryable<T> data, DataSourceRequest request, params string[] keySpecs)
        {
            var paged = await data.ToDataSourceResultAsync(request);

            var ordered = await data.ToDataSourceResultAsync(new DataSourceRequest
            {
                Sorts = request.Sorts,
                Filters = request.Filters
            });

            var keys = RecordNavigationKey.BuildAll(ordered.Data.Cast<T>(), keySpecs);

            return new CPiDataSourceResult
            {
                Data = paged.Data,
                Total = paged.Total,
                Ids = Array.Empty<int>(),
                Keys = keys
            };
        }
    }
}
