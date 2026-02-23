using Kendo.Mvc.UI;
using R10.Core.Entities.Clearance;
using R10.Core.Identity;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Interfaces
{
    public interface ITmcReviewViewModelService
    {
        IQueryable<TmcClearance> AddCriteria(IQueryable<TmcClearance> clearances, List<QueryFilterViewModel> mainSearchFilters, CPiEntityType userReviewerType, int userReviewerId);
        Task<CPiDataSourceResult> CreateViewModelForGrid(DataSourceRequest request, IQueryable<TmcClearance> clearances, CPiEntityType userReviewerType, int userReviewerId);
    }
}
