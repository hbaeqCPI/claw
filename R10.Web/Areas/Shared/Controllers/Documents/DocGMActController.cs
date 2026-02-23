using R10.Web.Helpers;
using AutoMapper.QueryableExtensions;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using R10.Core.Entities.GeneralMatter;
using R10.Core.Entities.Trademark;
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
    [Area("Shared"), Authorize(Policy = GeneralMatterAuthorizationPolicy.CanAccessSystem)]
    public class DocGMActController : Microsoft.AspNetCore.Mvc.Controller
    {
        private readonly IActionDueService<GMActionDue, GMDueDate> _actionDueService;

        public DocGMActController(IActionDueService<GMActionDue, GMDueDate> actionDueService)
        {
            _actionDueService = actionDueService;
        }

        protected IQueryable<GMActionDue> ActionsDue => _actionDueService.QueryableList;

        public IActionResult PageRead([DataSourceRequest] DataSourceRequest request, List<QueryFilterViewModel> mainSearchFilters, int? keyId)
        {
            if (!ModelState.IsValid) return new JsonBadRequest(new { errors = ModelState.Errors() });

            if (keyId == null)
            {
                var result = ActionsDue.ProjectTo<DocumentActionResultsViewModel>();
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
