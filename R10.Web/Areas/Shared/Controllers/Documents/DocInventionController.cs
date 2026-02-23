using R10.Web.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper.QueryableExtensions;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using R10.Core.Entities.Patent;
using R10.Core.Interfaces.Patent;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;
using R10.Web.Extensions.ActionResults;
using R10.Web.Interfaces;
using R10.Web.Security;

namespace R10.Web.Areas.Shared.Controllers
{
    [Area("Shared"),Authorize(Policy = PatentAuthorizationPolicy.CanAccessSystem)]
    public class DocInventionController : Microsoft.AspNetCore.Mvc.Controller
    {
        private readonly IInventionViewModelService _inventionViewModelService;
        private readonly IInventionService _inventionService;

        public DocInventionController(
            IInventionViewModelService inventionViewModelService,
            IInventionService inventionService
            )
        {
            _inventionViewModelService = inventionViewModelService;
            _inventionService = inventionService;
        }

        private IQueryable<Invention> Inventions => _inventionService.QueryableList;

        public IActionResult PageRead([DataSourceRequest] DataSourceRequest request, List<QueryFilterViewModel> mainSearchFilters, int? keyId)
        {
            if (!ModelState.IsValid) return new JsonBadRequest(new { errors = ModelState.Errors() });

            
            if (keyId == null)
            {
                var inventions = _inventionViewModelService.AddCriteria(mainSearchFilters, Inventions);
                var result = inventions.ProjectTo<DocumentInventionResultsViewModel>();
                //var result = await _inventionViewModelService.CreateViewModelForGrid(request, inventions);

                return Json(result.ToDataSourceResult(request));
            }
            else
            {
                var inventions = Inventions.Where(t => t.InvId == keyId);
                var result = inventions.ProjectTo<DocumentInventionResultsViewModel>();
                return Json(result.ToDataSourceResult(request));
            }
        }
    }
}
