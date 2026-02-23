using R10.Web.Helpers;
using System.Collections.Generic;
using System.Linq;
using AutoMapper.QueryableExtensions;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using R10.Core.Entities.GeneralMatter;
using R10.Core.Interfaces;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;
using R10.Web.Extensions.ActionResults;
using R10.Web.Interfaces;
using R10.Web.Security;

namespace R10.Web.Areas.Shared.Controllers.Documents
{
    [Area("Shared"), Authorize(Policy = GeneralMatterAuthorizationPolicy.CanAccessSystem)]
    public class DocGeneralMatterController : Microsoft.AspNetCore.Mvc.Controller
    {
        private readonly IGMMatterService _gmMatterService;

        public DocGeneralMatterController(IGMMatterService gmMatterService)
        {
            _gmMatterService = gmMatterService;
        }

        private IQueryable<GMMatter> Matters => _gmMatterService.QueryableList;

        public IActionResult PageRead([DataSourceRequest] DataSourceRequest request, List<QueryFilterViewModel> mainSearchFilters, int? keyId)
        {
            if (!ModelState.IsValid) return new JsonBadRequest(new { errors = ModelState.Errors() });

            if (keyId == null)
            {
                var result = Matters.ProjectTo<DocumentGeneralMatterResultsViewModel>();
                return Json(result.ToDataSourceResult(request));
            }
            else
            {
                var matters = Matters.Where(t => t.MatId == keyId);
                var result = matters.ProjectTo<DocumentGeneralMatterResultsViewModel>();
                return Json(result.ToDataSourceResult(request));
            }
        }
    }
}
