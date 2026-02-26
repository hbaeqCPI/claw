using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Mvc;
using R10.Core.Interfaces.Patent;
using R10.Web.Interfaces;
using R10.Core.Entities.Patent;
using Kendo.Mvc.Extensions;
using R10.Web.Helpers;
using Microsoft.EntityFrameworkCore;
using R10.Web.Security;
using Microsoft.AspNetCore.Authorization;
using R10.Core.Interfaces;
using R10.Web.Interfaces.Shared;
using R10.Core.Entities.Trademark;
using R10.Core.Entities;
using R10.Core.Helpers;

namespace R10.Web.Areas.Shared.Controllers.Reports
{
    [Area("Shared"), Authorize(Policy = SharedAuthorizationPolicy.CanAccessSystem)]
    public class CustomReportLookUpController : SharedReportBaseLookUpController
    {
        private IReportDeployService _reportDeployService;
        private readonly IEntityService<CustomReport> _customReportentityService;

        public CustomReportLookUpController(
            IReportDeployService reportDeployService
            , IEntityService<CustomReport> customReportentityService
            , IInventionService inventionService
            , ICountryApplicationService applicationService
            , ISharedReportViewModelService sharedReportViewModelService
            , ITmkTrademarkService trademarkService
            , ISystemSettings<PatSetting> patSettings
            , IMultipleEntityService<Invention, PatOwnerInv> patOwnerInvService
            , IMultipleEntityService<PatOwnerApp> patOwnerAppService
            , IEntityService<TmkOwner> tmkOwnerService
            , IDueDateService<PatActionDue, PatDueDate> patDueDateService
            , IDueDateService<TmkActionDue, TmkDueDate> tmkDueDateService
            ) : base(inventionService, applicationService, sharedReportViewModelService, trademarkService, patSettings, patOwnerInvService, patOwnerAppService, tmkOwnerService
            , patDueDateService, tmkDueDateService)
        {
            _reportDeployService = reportDeployService;
            _customReportentityService = customReportentityService;
        }

        public async Task<IActionResult> GetCustomReportNameList(string property, string text, FilterType filterType)
        {
            return Json(await _customReportentityService.QueryableList.Where(c => c.IsShared || c.UserId == User.GetUserIdentifier()).Select(c=> new { ReportName = c.ReportName}).ToListAsync());
        }
    }
}
