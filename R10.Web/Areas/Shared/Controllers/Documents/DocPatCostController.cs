using R10.Web.Helpers;
using AutoMapper.QueryableExtensions;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using R10.Core.Entities.Patent;
using R10.Core.Interfaces;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;
using R10.Web.Extensions.ActionResults;
using R10.Web.Interfaces;
using R10.Web.Security;
using System.Collections.Generic;
using System.Linq;

namespace R10.Web.Areas.Shared.Controllers.Documents
{
    [Area("Shared"), Authorize(Policy = PatentAuthorizationPolicy.CanAccessSystem)]
    public class DocPatCostController : Microsoft.AspNetCore.Mvc.Controller
    {
        protected readonly ICostTrackingService<PatCostTrack> _costTrackingService;
        private readonly IPatCostTrackingViewModelService _costTrackingViewModelService;

        public DocPatCostController(
                ICostTrackingService<PatCostTrack> costTrackingService,
                IPatCostTrackingViewModelService costTrackingViewModelService

            )
        {
            _costTrackingService = costTrackingService;
            _costTrackingViewModelService = costTrackingViewModelService;
        }

        protected IQueryable<PatCostTrack> CostTrackings => _costTrackingService.QueryableList;

        public IActionResult PageRead([DataSourceRequest] DataSourceRequest request, List<QueryFilterViewModel> mainSearchFilters, int? keyId)
        {
            if (!ModelState.IsValid) return new JsonBadRequest(new { errors = ModelState.Errors() });

            if (keyId == null)
            {
                var costTrack = _costTrackingViewModelService.AddCriteria(mainSearchFilters, CostTrackings);
                var result = costTrack.ProjectTo<DocumentCostResultsViewModel>();
                return Json(result.ToDataSourceResult(request));
            }
            else
            {
                var costTrack = CostTrackings.Where(t => t.CostTrackId == keyId);
                var result = costTrack.ProjectTo<DocumentCostResultsViewModel>();
                return Json(result.ToDataSourceResult(request));
            }
        }
    }
}
