using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using R10.Core.DTOs;
using R10.Core.Entities;
// using R10.Core.Entities.Clearance; // Removed during deep clean
// using R10.Core.Entities.ForeignFiling; // Removed during deep clean
// using R10.Core.Entities.GeneralMatter; // Removed during deep clean
// using R10.Core.Entities.PatClearance; // Removed during deep clean
using R10.Core.Entities.Patent;
// using R10.Core.Entities.RMS; // Removed during deep clean
using R10.Core.Entities.Shared;
using R10.Core.Entities.Trademark;
using R10.Core.Helpers;
using R10.Core.Interfaces;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;
using R10.Web.Extensions.ActionResults;
using R10.Web.Helpers;
using R10.Web.Models;
using R10.Web.Models.PageViewModels;
using R10.Web.Security;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using R10.Web.Areas;

namespace R10.Web.Areas.Shared.Controllers
{
    [Area("Shared"), Authorize(Policy = SharedAuthorizationPolicy.CanAccessAudit)]
    public class AuditTrailController : BaseController
    {
        private readonly IAuditService _auditService;
        private readonly ISystemSettings<PatSetting> _patSettings;
        private readonly ISystemSettings<TmkSetting> _tmkSettings;
        // private readonly ISystemSettings<GMSetting> _gmSettings; // Removed during deep clean
        private readonly ISystemSettings<DefaultSetting> _defaultSettings;
        // private readonly ISystemSettings<FFSetting> _ffSettings; // Removed during deep clean
        // private readonly ISystemSettings<PacSetting> _pacSettings; // Removed during deep clean
        // private readonly ISystemSettings<RMSSetting> _rmsSettings; // Removed during deep clean
        // private readonly ISystemSettings<TmcSetting> _tmcSettings; // Removed during deep clean
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly ExportHelper _exportHelper;
        private readonly IAuthorizationService _authService;

        public AuditTrailController(
            IAuditService auditService,
            ISystemSettings<PatSetting> patSettings,
            ISystemSettings<TmkSetting> tmkSettings,
            // ISystemSettings<GMSetting> gmSettings, // Removed during deep clean
            ISystemSettings<DefaultSetting> defaultSettings,
            // ISystemSettings<FFSetting> ffSettings, // Removed during deep clean
            // ISystemSettings<PacSetting> pacSettings, // Removed during deep clean
            // ISystemSettings<RMSSetting> rmsSettings, // Removed during deep clean
            // ISystemSettings<TmcSetting> tmcSettings, // Removed during deep clean
            IStringLocalizer<SharedResource> localizer,
            ExportHelper exportHelper,
            IAuthorizationService authService
            )
        {
            _auditService = auditService;
            _patSettings = patSettings;
            _tmkSettings = tmkSettings;
            // _gmSettings = gmSettings; // Removed during deep clean
            _defaultSettings = defaultSettings;
            // _ffSettings = ffSettings; // Removed during deep clean
            // _pacSettings = pacSettings; // Removed during deep clean
            // _rmsSettings = rmsSettings; // Removed during deep clean
            // _tmcSettings = tmcSettings; // Removed during deep clean
            _localizer = localizer;
            _exportHelper = exportHelper;
            _authService = authService;
        }

        public async Task<IActionResult> Index(string sys = SystemTypeCode.Patent)
        {
            var authorized = false;
            if (sys == SystemTypeCode.Patent)
                authorized = (await _authService.AuthorizeAsync(User, PatentAuthorizationPolicy.CanAccessAudit)).Succeeded;
            else if (sys == SystemTypeCode.Trademark)
                authorized = (await _authService.AuthorizeAsync(User, TrademarkAuthorizationPolicy.CanAccessAudit)).Succeeded;
            // Removed during deep clean - GeneralMatter, DMS, ForeignFiling, PatClearance, AMS, RMS, Clearance
            // else if (sys == SystemTypeCode.GeneralMatter)
            //     authorized = (await _authService.AuthorizeAsync(User, GeneralMatterAuthorizationPolicy.CanAccessAudit)).Succeeded;
            // else if (sys == SystemTypeCode.DMS)
            //     authorized = (await _authService.AuthorizeAsync(User, DMSAuthorizationPolicy.CanAccessAudit)).Succeeded;
            // else if (sys == SystemTypeCode.ForeignFiling)
            //     authorized = (await _authService.AuthorizeAsync(User, ForeignFilingAuthorizationPolicy.CanAccessAudit)).Succeeded;
            // else if (sys == SystemTypeCode.PatClearance)
            //     authorized = (await _authService.AuthorizeAsync(User, PatentClearanceAuthorizationPolicy.CanAccessAudit)).Succeeded;
            // else if (sys == SystemTypeCode.AMS)
            //     authorized = (await _authService.AuthorizeAsync(User, AMSAuthorizationPolicy.CanAccessAudit)).Succeeded;
            // else if (sys == SystemTypeCode.RMS)
            //     authorized = (await _authService.AuthorizeAsync(User, RMSAuthorizationPolicy.CanAccessAudit)).Succeeded;
            // else if (sys == SystemTypeCode.Clearance)
            //     authorized = (await _authService.AuthorizeAsync(User, SearchRequestAuthorizationPolicy.CanAccessAudit)).Succeeded;
            else
                authorized = (await _authService.AuthorizeAsync(User, PatentAuthorizationPolicy.CanAccessAudit)).Succeeded;

            if (!authorized)
                return Forbid();

            var model = new PageViewModel()
            {
                Page = PageType.Search,
                PageId = "auditTrailSearch",
                Title = _localizer["Audit Trail Search"].ToString(),
                SystemType = sys
            };

            var availableSystemTypes = await _auditService.GetAvailableSystemTypes();
            ViewBag.AvailableSystemTypes = availableSystemTypes;

            if (Request.IsAjax())
                return PartialView("Index", model);

            return View("Index", model);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Search([FromBody] List<QueryFilterViewModel> mainSearchFilters)
        {
            //var fromSystem = mainSearchFilters.FirstOrDefault(c => c.Property == "FromSystem");
            var model = new PageViewModel()
            {
                Page = PageType.SearchResults,
                PageId = "auditTrailSearchResults",
                Title = _localizer["Audit Trail Search Results"].ToString(),
                GridPageSize = 8
                //SystemType = fromSystem.Value
            };
            //mainSearchFilters.Remove(fromSystem);

            return PartialView("Index", model);
        }

        public async Task<IActionResult> GetPicklistData([DataSourceRequest] DataSourceRequest request, string systemType, string dataType)
        {
            var result = await _auditService.GetAuditLookup(systemType, dataType);
            return Json(result);
        }
        
        public async Task<IActionResult> GetSystemCriteria (string systemType)
        {
            AuditTrailSearchSubCriteriaViewModel systemCriteria = new AuditTrailSearchSubCriteriaViewModel();
            switch (systemType)
            {
                case SystemTypeCode.Patent:
                    var patSettings = await _patSettings.GetSetting();
                    systemCriteria.SystemName = SystemType.Patent;
                    systemCriteria.AreaName = SystemType.Patent;
                    systemCriteria.ControllerName = "CountryApplicationLookup";
                    systemCriteria.ValueMapper = "patCountryAppPage.caseNumberSearchValueMapper";

                    systemCriteria.LabelCaseNumber = patSettings.LabelCaseNumber;
                    systemCriteria.EnableComboPaging = patSettings.EnableComboBoxPaging;
                    systemCriteria.ComboPagingSize = patSettings.ComboBoxPagingSize;
                    break;                
                case SystemTypeCode.Trademark:
                    var tmkSettings = await _tmkSettings.GetSetting();
                    systemCriteria.SystemName = SystemType.Trademark;
                    systemCriteria.AreaName = SystemType.Trademark;
                    systemCriteria.ControllerName = "TmkTrademarkLookup";
                    systemCriteria.ValueMapper = "tmkTrademarkPage.caseNumberSearchValueMapper";

                    systemCriteria.LabelCaseNumber = tmkSettings.LabelCaseNumber;
                    systemCriteria.EnableComboPaging = tmkSettings.EnableComboBoxPaging;
                    systemCriteria.ComboPagingSize = tmkSettings.ComboBoxPagingSize;
                    break;
                // Removed during deep clean - GeneralMatter, DMS, ForeignFiling, PatClearance, AMS, RMS, Clearance case blocks
                // case SystemTypeCode.GeneralMatter:
                // case SystemTypeCode.DMS:
                // case SystemTypeCode.ForeignFiling:
                // case SystemTypeCode.PatClearance:
                // case SystemTypeCode.AMS:
                // case SystemTypeCode.RMS:
                // case SystemTypeCode.Clearance:
                case SystemTypeCode.Shared:
                    systemCriteria.SystemName = SystemType.Shared;
                    systemCriteria.AreaName = SystemType.Shared;
                    break;
                default:
                    var pSettings = await _patSettings.GetSetting();
                    systemCriteria.SystemName = SystemType.Patent;
                    systemCriteria.AreaName = SystemType.Patent;
                    systemCriteria.ControllerName = "CountryApplicationLookup";
                    systemCriteria.ValueMapper = "patCountryAppPage.caseNumberSearchValueMapper";

                    systemCriteria.LabelCaseNumber = pSettings.LabelCaseNumber;
                    systemCriteria.EnableComboPaging = pSettings.EnableComboBoxPaging;
                    systemCriteria.ComboPagingSize = pSettings.ComboBoxPagingSize;
                    break;
            }

            systemCriteria.SystemType = systemType;

            return PartialView("_SearchSubCriteria", systemCriteria);

        }

        private string GetSystemTypes()
        {
            var systemTypes = "";

            if (User.IsInSystem(SystemType.Patent)) systemTypes = systemTypes + "P|";
            if (User.IsInSystem(SystemType.Trademark)) systemTypes = systemTypes + "T|";
            // Removed during deep clean - GeneralMatter, DMS, ForeignFiling, PatClearance, AMS, RMS, SearchRequest
            // if (User.IsInSystem(SystemType.GeneralMatter)) systemTypes = systemTypes + "G|";
            // if (User.IsInSystem(SystemType.DMS)) systemTypes = systemTypes + "D|";
            // if (User.IsInSystem(SystemType.ForeignFiling)) systemTypes = systemTypes + "F|";
            // if (User.IsInSystem(SystemType.PatClearance)) systemTypes = systemTypes + "E|";
            // if (User.IsInSystem(SystemType.AMS)) systemTypes = systemTypes + "A|";
            // if (User.IsInSystem(SystemType.RMS)) systemTypes = systemTypes + "R|";
            // if (User.IsInSystem(SystemType.SearchRequest)) systemTypes = systemTypes + "C|";
            if (User.IsInSystem(SystemType.Shared)) systemTypes = systemTypes + "S|";

            return systemTypes;
        }


        public async Task<Microsoft.AspNetCore.Mvc.ActionResult> GetAuditLogHeader([DataSourceRequest] DataSourceRequest request, AuditSearchDTO searchCriteria)
        {
            if (ModelState.IsValid)
            {
                if (request != null)
                {
                    searchCriteria.Page = request.Page;
                    searchCriteria.PageSize = request.PageSize;
                }

                //var result = (await _auditService.GetAuditLogHeader(searchCriteria)).ToDataSourceResult(request); 
                var pagedResult = await _auditService.GetAuditLogHeader(searchCriteria); 
                var ids = pagedResult.Data?.Select(d => d.AudTrailId).ToArray() ?? new int[0];

                //return Json(result);
                return Json(new CPiDataSourceResult()
                {
                    Data = pagedResult.Data,
                    Total = pagedResult.TotalCount,
                    Ids = ids
                });
            }
            return new JsonBadRequest(new { errors = ModelState.Errors() });
        }

        public async Task<Microsoft.AspNetCore.Mvc.ActionResult> GetAuditKeys([DataSourceRequest] DataSourceRequest request, string systemType, int audTrailId)
        {
            if (ModelState.IsValid)
            {
                var result = (await _auditService.GetAuditKey(systemType, audTrailId)).ToDataSourceResult(request);
                return Json(result);
            }
            return new JsonBadRequest(new { errors = ModelState.Errors() });
        }

        public async Task<Microsoft.AspNetCore.Mvc.ActionResult> GetAuditLogDetail([DataSourceRequest] DataSourceRequest request, string systemType, int audTrailId)
        {
            if (ModelState.IsValid)
            {
                var result = (await _auditService.GetAuditLogDetail(systemType, audTrailId)).ToDataSourceResult(request);
                return Json(result);
            }
            return new JsonBadRequest(new { errors = ModelState.Errors() });
        }

        public async Task<Microsoft.AspNetCore.Mvc.ActionResult> ExportFile(AuditSearchDTO searchCriteria)
        {
            if (ModelState.IsValid)
            {
                try
                {                    
                    var result = await _auditService.GetAuditReport(searchCriteria);
                    var fileStream = await _exportHelper.ListToExcelMemoryStream(result, "AuditTrail", _localizer, showTime: true);
                    return File(fileStream.ToArray(), ImageHelper.GetContentType(".xlsx"), "Audit Trail Report.xlsx");
                }
                catch (Exception e)
                {
                    var error = e.Message;
                }                
            }
            return Ok();
        }
    }
}
