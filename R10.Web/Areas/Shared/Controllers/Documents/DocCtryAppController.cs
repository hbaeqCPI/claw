using R10.Web.Helpers;
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

namespace R10.Web.Areas.Shared.Controllers.Documents
{
    [Area("Shared"), Authorize(Policy = PatentAuthorizationPolicy.CanAccessSystem)]
    public class DocCtryAppController : Microsoft.AspNetCore.Mvc.Controller
    {
        private readonly ICountryApplicationService _applicationService;
        private readonly ICountryApplicationViewModelService _applicationViewModelService;

        public DocCtryAppController(
            ICountryApplicationService applicationService,
            ICountryApplicationViewModelService applicationViewModelService
            )
        {
            _applicationService = applicationService;
            _applicationViewModelService = applicationViewModelService;
        }

        private IQueryable<CountryApplication> CountryApplications => _applicationService.CountryApplications;

        public IActionResult PageRead([DataSourceRequest] DataSourceRequest request, List<QueryFilterViewModel> mainSearchFilters, int? keyId)
        {
            if (!ModelState.IsValid) return new JsonBadRequest(new { errors = ModelState.Errors() });

            if (keyId == null)
            {
                var applications = _applicationViewModelService.AddCriteria(this.CountryApplications, mainSearchFilters);
                var result = applications.ProjectTo<DocumentCtryAppResultsViewModel>();
                //var result = await _applicationViewModelService.CreateViewModelForGrid(request, applications);

                return Json(result.ToDataSourceResult(request));
            }
            else
            {
                var application = CountryApplications.Where(t => t.AppId == keyId);
                var result = application.ProjectTo<DocumentCtryAppResultsViewModel>();                
                return Json(result.ToDataSourceResult(request));
            }
            
        }

    }
}
