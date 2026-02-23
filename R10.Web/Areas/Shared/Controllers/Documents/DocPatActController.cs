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
    public class DocPatActController : Microsoft.AspNetCore.Mvc.Controller
    {
        private readonly IActionDueService<PatActionDue, PatDueDate> _actionDueService;
        private readonly IPatActionDueViewModelService _actionDueViewModelService;

        public DocPatActController(
                IActionDueService<PatActionDue, PatDueDate> actionDueService,
                IPatActionDueViewModelService actionDueViewModelService)
        {
            _actionDueService = actionDueService;
            _actionDueViewModelService = actionDueViewModelService;
        }

        protected IQueryable<PatActionDue> ActionsDue => _actionDueService.QueryableList;

        public IActionResult PageRead([DataSourceRequest] DataSourceRequest request, List<QueryFilterViewModel> mainSearchFilters, int? keyId)
        {
            if (!ModelState.IsValid) return new JsonBadRequest(new { errors = ModelState.Errors() });

            if (keyId == null)
            {
                var actionsDue = _actionDueViewModelService.AddCriteria(mainSearchFilters, ActionsDue);
                var result = actionsDue.ProjectTo<DocumentActionResultsViewModel>();
                return Json(result.ToDataSourceResult(request));
            }
            else
            {
                var actionsDue = ActionsDue.Where(t => t.ActId == keyId);
                var result = actionsDue.ProjectTo<DocumentActionResultsViewModel>();
                return Json(result.ToDataSourceResult(request));
            }
        }
    }
}
