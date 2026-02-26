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

namespace R10.Web.Areas.Shared.Controllers.Reports
{
    [Area("Shared"), Authorize(Policy = SharedAuthorizationPolicy.CanAccessSystem)]
    public class CostTrackingLookUpController : SharedReportBaseLookUpController
    {
        protected readonly ICostTrackingService<PatCostTrack> _patCostTrackingService;
        protected readonly ICostTrackingService<TmkCostTrack> _tmkCostTrackingService;

        public CostTrackingLookUpController(IInventionService inventionService
            , ICountryApplicationService applicationService
            , ISharedReportViewModelService sharedReportViewModelService
            , ITmkTrademarkService trademarkService
            , ICostTrackingService<PatCostTrack> patCostTrackingService
            , ICostTrackingService<TmkCostTrack> tmkCostTrackingService
            , ISystemSettings<PatSetting> patSettings
            , IMultipleEntityService<Invention, PatOwnerInv> patOwnerInvService
            , IMultipleEntityService<PatOwnerApp> patOwnerAppService
            , IEntityService<TmkOwner> tmkOwnerService
            , IDueDateService<PatActionDue, PatDueDate> patDueDateService
            , IDueDateService<TmkActionDue, TmkDueDate> tmkDueDateService
            ) : base(inventionService, applicationService, sharedReportViewModelService, trademarkService, patSettings, patOwnerInvService, patOwnerAppService, tmkOwnerService
            , patDueDateService, tmkDueDateService)
        {
            _patCostTrackingService = patCostTrackingService;
            _tmkCostTrackingService = tmkCostTrackingService;

            CountryApplications = applicationService.CountryApplications.Where(c => patCostTrackingService.QueryableList.FirstOrDefault(d => d.AppId == c.AppId) != null);
            TmkTrademarks = trademarkService.TmkTrademarks.Where(c => tmkCostTrackingService.QueryableList.FirstOrDefault(d => d.TmkId == c.TmkId) != null);
            Inventions = inventionService.QueryableList.Where(i => CountryApplications.Any(ca => ca.InvId == i.InvId));
        }

        public async Task<IActionResult> GetTitleList([DataSourceRequest] DataSourceRequest request, string property, string text, string systemType, FilterType filterType, string requiredRelation = "")
        {
            if (systemType == null||systemType=="") systemType = "P,T";
            var appTitles = systemType.Contains("P") ? CountryApplications.Select(c => new SharedEntity { Code = c.AppTitle }).Distinct().Where(c => c.Code != null).ToList() : new List<SharedEntity>();
            var tmkTitles = systemType.Contains("T") ? TmkTrademarks.Select(c => new SharedEntity { Code = c.TrademarkName }).Distinct().Where(c => c.Code != null).ToList() : new List<SharedEntity>();

            var result2 = new List<SharedEntity>();
            if (systemType.Contains("P"))
                result2.AddRange(appTitles);
            if (systemType.Contains("T"))
                result2.AddRange(tmkTitles);


            var result = result2.Where(c => c.Code.StartsWith(text == null ? "" : text)).Select(c=> new { Title = c.Code });

            if (request.PageSize > 0)
            {
                request.Filters.Clear();
                return Json(await result.ToDataSourceResultAsync(request));
            }

            var list = result.ToList();
            return Json(list);
        }

        public async Task<IActionResult> GetCostTypeList(string property, string text, string systemType, FilterType filterType, string requiredRelation = "")
        {
            var CostTypes = _sharedReportViewModelService.GetCombinedCostTypes;
            return Json(await QueryHelper.GetPicklistDataAsync(CostTypes, property, text, filterType, requiredRelation));
        }
    }
}
