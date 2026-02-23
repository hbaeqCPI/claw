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
using R10.Core.Entities.Trademark;
using R10.Core.Interfaces;
using R10.Core.Interfaces.Patent;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;
using R10.Web.Extensions.ActionResults;
using R10.Web.Interfaces;
using R10.Web.Security;

namespace R10.Web.Areas.Shared.Controllers.Documents
{
    [Area("Shared"), Authorize(Policy = TrademarkAuthorizationPolicy.CanAccessSystem)]
    public class DocTrademarkController : Microsoft.AspNetCore.Mvc.Controller
    {
        private readonly ITmkTrademarkService _trademarkService;
        private readonly ITmkTrademarkViewModelService _trademarkViewModelService;

        public DocTrademarkController(
            ITmkTrademarkService trademarkService,
            ITmkTrademarkViewModelService trademarkViewModelService
            )
        {
            _trademarkService = trademarkService;
            _trademarkViewModelService = trademarkViewModelService;
        }

        private IQueryable<TmkTrademark> Trademarks => _trademarkService.TmkTrademarks;

        public IActionResult PageRead([DataSourceRequest] DataSourceRequest request, List<QueryFilterViewModel> mainSearchFilters, int? keyId)
        {
            if (!ModelState.IsValid) return new JsonBadRequest(new { errors = ModelState.Errors() }); 

            if (keyId == null)
            {
                var trademarks = _trademarkViewModelService.AddCriteria(mainSearchFilters, Trademarks);
                var result = trademarks.ProjectTo<DocumentTrademarkResultsViewModel>();

                return Json(result.ToDataSourceResult(request));
            }
            else
            {
                var trademark = Trademarks.Where(t => t.TmkId == keyId);
                var result = trademark.ProjectTo<DocumentTrademarkResultsViewModel>();
                return Json(result.ToDataSourceResult(request));
            }
        }

    }
}
