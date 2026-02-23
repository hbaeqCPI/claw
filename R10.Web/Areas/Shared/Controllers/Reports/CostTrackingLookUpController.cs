using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Mvc;
using R10.Core.Interfaces.Patent;
using R10.Web.Interfaces;
using R10.Core.Entities.Patent;
using System.Linq;
using R10.Core.Interfaces.Patent;
using Kendo.Mvc.Extensions;
using R10.Web.Helpers;
using Microsoft.EntityFrameworkCore;
using R10.Web.Security;
using Microsoft.AspNetCore.Authorization;
using R10.Core.Interfaces;
using R10.Web.Interfaces.Shared;
using R10.Core.Entities.Trademark;
using R10.Core.Entities.GeneralMatter;
using R10.Core.Interfaces.DMS;
using R10.Core.Interfaces.AMS;
using R10.Core.Entities.DMS;

namespace R10.Web.Areas.Shared.Controllers.Reports
{
    [Area("Shared"), Authorize(Policy = SharedAuthorizationPolicy.CanAccessSystem)]
    public class CostTrackingLookUpController : SharedReportBaseLookUpController
    {
        protected readonly ICostTrackingService<PatCostTrack> _patCostTrackingService;
        protected readonly ICostTrackingService<TmkCostTrack> _tmkCostTrackingService;
        protected readonly ICostTrackingService<GMCostTrack> _gmCostTrackingService;

        public CostTrackingLookUpController(IInventionService inventionService
            , ICountryApplicationService applicationService
            , ISharedReportViewModelService sharedReportViewModelService
            , ITmkTrademarkService trademarkService
            , IGMMatterService gmMatterService
            , ICostTrackingService<PatCostTrack> patCostTrackingService
            , ICostTrackingService<TmkCostTrack> tmkCostTrackingService
            , ICostTrackingService<GMCostTrack> gmCostTrackingService
            , IDisclosureService disclosureService
            , IAMSDueService amsDueService
            , ISystemSettings<PatSetting> patSettings
            , IMultipleEntityService<Invention, PatOwnerInv> patOwnerInvService
            , IMultipleEntityService<PatOwnerApp> patOwnerAppService
            , IEntityService<TmkOwner> tmkOwnerService
            , IMultipleEntityService<GMMatter, GMMatterAttorney> matterAttorneyService
            , IGMMatterCountryService matterCountryService

            , IDueDateService<PatActionDue, PatDueDate> patDueDateService
            , IDueDateService<TmkActionDue, TmkDueDate> tmkDueDateService
            , IDueDateService<GMActionDue, GMDueDate> gmDueDateService
            , IDueDateService<DMSActionDue, DMSDueDate> dmsDueDateService
            ) : base(inventionService, applicationService, sharedReportViewModelService, trademarkService, gmMatterService, disclosureService, amsDueService, patSettings, patOwnerInvService, patOwnerAppService, tmkOwnerService, matterAttorneyService, matterCountryService
            , patDueDateService, tmkDueDateService, gmDueDateService, dmsDueDateService)
        {
            _patCostTrackingService = patCostTrackingService;
            _tmkCostTrackingService = tmkCostTrackingService;
            _gmCostTrackingService = gmCostTrackingService;

            CountryApplications = applicationService.CountryApplications.Where(c => patCostTrackingService.QueryableList.FirstOrDefault(d => d.AppId == c.AppId) != null);
            TmkTrademarks = trademarkService.TmkTrademarks.Where(c => tmkCostTrackingService.QueryableList.FirstOrDefault(d => d.TmkId == c.TmkId) != null);
            GMMatters = gmMatterService.QueryableList.Where(c => gmCostTrackingService.QueryableList.FirstOrDefault(d => d.MatId == c.MatId) != null);
            Inventions = inventionService.QueryableList.Where(i => CountryApplications.Any(ca => ca.InvId == i.InvId));
        }

        public async Task<IActionResult> GetTitleList([DataSourceRequest] DataSourceRequest request, string property, string text, string systemType, FilterType filterType, string requiredRelation = "")
        {
            if (systemType == null||systemType=="") systemType = "P,T,G";
            var appTitles = systemType.Contains("P") ? CountryApplications.Select(c => new SharedEntity { Code = c.AppTitle }).Distinct().Where(c => c.Code != null).ToList() : new List<SharedEntity>();//Enumerable.Empty<CountryApplication>().AsQueryable().Select(c => new SharedEntity { Code = c.AppTitle });
            var tmkTitles = systemType.Contains("T") ? TmkTrademarks.Select(c => new SharedEntity { Code = c.TrademarkName }).Distinct().Where(c => c.Code != null).ToList() : new List<SharedEntity>();//Enumerable.Empty<TmkTrademark>().AsQueryable().Select(c => new SharedEntity  { Code = c.TrademarkName });
            var gmTitles = systemType.Contains("G") ? GMMatters.Select(c => new SharedEntity { Code = c.MatterTitle }).Distinct().Where(c => c.Code != null).ToList() : new List<SharedEntity>();//Enumerable.Empty<GMMatter>().AsQueryable().Select(c => new SharedEntity  { Code = c.MatterTitle });

            //var result2 = systemType.Contains("P") ? { appTitles.AddRange(tmkTitles); appTitles.AddRange(gmTitles); appTitles.Distinct(); }
            //: systemType.Contains("T") ? tmkTitles.AddRange(gmTitles).AddRange(appTitles).Distinct()
            //: systemType.Contains("G") ? gmTitles.AddRange(appTitles).AddRange(tmkTitles).Distinct()
            //: Enumerable.Empty<CountryApplication>().AsQueryable().Select(c => new SharedEntity { Code = c.AppTitle }).Distinct();

            var result2 = new List<SharedEntity>();
            if (systemType.Contains("P"))
                result2.AddRange(appTitles);
            if (systemType.Contains("T"))
                result2.AddRange(tmkTitles);
            if (systemType.Contains("G"))
                result2.AddRange(gmTitles);


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
