using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Entities.Clearance;
using R10.Core.Entities.ForeignFiling;
using R10.Core.Entities.GeneralMatter;
using R10.Core.Entities.PatClearance;
using R10.Core.Entities.Patent;
using R10.Core.Entities.RMS;
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
        private readonly ISystemSettings<GMSetting> _gmSettings;
        private readonly ISystemSettings<DefaultSetting> _defaultSettings;
        private readonly ISystemSettings<FFSetting> _ffSettings;
        private readonly ISystemSettings<PacSetting> _pacSettings;
        private readonly ISystemSettings<RMSSetting> _rmsSettings;
        private readonly ISystemSettings<TmcSetting> _tmcSettings;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly ExportHelper _exportHelper;
        private readonly IAuthorizationService _authService;

        public AuditTrailController(
            IAuditService auditService,
            ISystemSettings<PatSetting> patSettings,
            ISystemSettings<TmkSetting> tmkSettings,
            ISystemSettings<GMSetting> gmSettings,
            ISystemSettings<DefaultSetting> defaultSettings,
            ISystemSettings<FFSetting> ffSettings,
            ISystemSettings<PacSetting> pacSettings,
            ISystemSettings<RMSSetting> rmsSettings,
            ISystemSettings<TmcSetting> tmcSettings,
            IStringLocalizer<SharedResource> localizer,
            ExportHelper exportHelper,
            IAuthorizationService authService
            )
        {
            _auditService = auditService;
            _patSettings = patSettings;
            _tmkSettings = tmkSettings;
            _gmSettings = gmSettings;
            _defaultSettings = defaultSettings;
            _ffSettings = ffSettings;
            _pacSettings = pacSettings;
            _rmsSettings = rmsSettings;
            _tmcSettings = tmcSettings;
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
            else if (sys == SystemTypeCode.GeneralMatter)
                authorized = (await _authService.AuthorizeAsync(User, GeneralMatterAuthorizationPolicy.CanAccessAudit)).Succeeded;
            else if (sys == SystemTypeCode.DMS)
                authorized = (await _authService.AuthorizeAsync(User, DMSAuthorizationPolicy.CanAccessAudit)).Succeeded;
            else if (sys == SystemTypeCode.ForeignFiling)
                authorized = (await _authService.AuthorizeAsync(User, ForeignFilingAuthorizationPolicy.CanAccessAudit)).Succeeded;
            else if (sys == SystemTypeCode.PatClearance)
                authorized = (await _authService.AuthorizeAsync(User, PatentClearanceAuthorizationPolicy.CanAccessAudit)).Succeeded;
            else if (sys == SystemTypeCode.AMS)
                authorized = (await _authService.AuthorizeAsync(User, AMSAuthorizationPolicy.CanAccessAudit)).Succeeded;
            else if (sys == SystemTypeCode.RMS)
                authorized = (await _authService.AuthorizeAsync(User, RMSAuthorizationPolicy.CanAccessAudit)).Succeeded;
            else if (sys == SystemTypeCode.Clearance)
                authorized = (await _authService.AuthorizeAsync(User, SearchRequestAuthorizationPolicy.CanAccessAudit)).Succeeded;
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
                case SystemTypeCode.GeneralMatter:
                    var gmSettings = await _gmSettings.GetSetting();
                    systemCriteria.SystemName = SystemType.GeneralMatter;
                    systemCriteria.AreaName = SystemType.GeneralMatter;
                    systemCriteria.ControllerName = "Matter";
                    systemCriteria.ValueMapper = "";

                    systemCriteria.LabelCaseNumber = gmSettings.LabelCaseNumber;
                    systemCriteria.EnableComboPaging = gmSettings.EnableComboBoxPaging;
                    systemCriteria.ComboPagingSize = gmSettings.ComboBoxPagingSize;
                    break;
                case SystemTypeCode.DMS:
                    var dmsDefaultSettings = await _defaultSettings.GetSetting();
                    systemCriteria.SystemName = SystemType.DMS;
                    systemCriteria.AreaName = SystemType.DMS;
                    systemCriteria.ControllerName = "DisclosureLookup";
                    systemCriteria.ValueMapper = "";
                    systemCriteria.LabelCaseNumber = dmsDefaultSettings.LabelCaseNumber ?? "Case Number";
                    systemCriteria.EnableComboPaging = dmsDefaultSettings.EnableComboBoxPaging;
                    systemCriteria.ComboPagingSize = dmsDefaultSettings.ComboBoxPagingSize;
                    break;
                case SystemTypeCode.ForeignFiling:
                    var ffSettings = await _ffSettings.GetSetting();
                    systemCriteria.SystemName = SystemType.ForeignFiling;
                    systemCriteria.AreaName = SystemType.ForeignFiling;
                    systemCriteria.ControllerName = "CountryApplicationLookup";
                    systemCriteria.ValueMapper = "patCountryAppPage.caseNumberSearchValueMapper";

                    systemCriteria.LabelCaseNumber = ffSettings.LabelCaseNumber;
                    systemCriteria.EnableComboPaging = ffSettings.EnableComboBoxPaging;
                    systemCriteria.ComboPagingSize = ffSettings.ComboBoxPagingSize;
                    break;
                case SystemTypeCode.PatClearance:
                    var pacSettings = await _pacSettings.GetSetting();
                    systemCriteria.SystemName = SystemType.PatClearance;
                    systemCriteria.AreaName = SystemType.PatClearance;
                    systemCriteria.ControllerName = "PacClearanceLookup";
                    systemCriteria.ValueMapper = "";

                    systemCriteria.LabelCaseNumber = pacSettings.LabelCaseNumber;
                    systemCriteria.EnableComboPaging = pacSettings.EnableComboBoxPaging;
                    systemCriteria.ComboPagingSize = pacSettings.ComboBoxPagingSize;
                    break;
                case SystemTypeCode.AMS:
                    var amsDefaultSettings = await _defaultSettings.GetSetting();
                    systemCriteria.SystemName = SystemType.AMS;
                    systemCriteria.AreaName = SystemType.AMS;
                    systemCriteria.ControllerName = "Main";
                    systemCriteria.ValueMapper = "";
                    systemCriteria.LabelCaseNumber = amsDefaultSettings.LabelCaseNumber ?? "Case Number";
                    systemCriteria.EnableComboPaging = amsDefaultSettings.EnableComboBoxPaging;
                    systemCriteria.ComboPagingSize = amsDefaultSettings.ComboBoxPagingSize;
                    break;
                case SystemTypeCode.RMS:
                    var rmsSettings = await _rmsSettings.GetSetting();
                    systemCriteria.SystemName = SystemType.RMS;
                    systemCriteria.AreaName = SystemType.RMS;
                    systemCriteria.ControllerName = "TmkTrademarkLookup";
                    systemCriteria.ValueMapper = "tmkTrademarkPage.caseNumberSearchValueMapper";

                    systemCriteria.LabelCaseNumber = rmsSettings.LabelCaseNumber;
                    systemCriteria.EnableComboPaging = rmsSettings.EnableComboBoxPaging;
                    systemCriteria.ComboPagingSize = rmsSettings.ComboBoxPagingSize;
                    break;
                case SystemTypeCode.Clearance:
                    var tmcSettings = await _tmcSettings.GetSetting();
                    systemCriteria.SystemName = SystemType.SearchRequest;
                    systemCriteria.AreaName = SystemType.SearchRequest;
                    systemCriteria.ControllerName = "TmcClearanceLookup";
                    systemCriteria.ValueMapper = "";

                    systemCriteria.LabelCaseNumber = tmcSettings.LabelCaseNumber;
                    systemCriteria.EnableComboPaging = tmcSettings.EnableComboBoxPaging;
                    systemCriteria.ComboPagingSize = tmcSettings.ComboBoxPagingSize;
                    break;
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
            if (User.IsInSystem(SystemType.GeneralMatter)) systemTypes = systemTypes + "G|";
            if (User.IsInSystem(SystemType.DMS)) systemTypes = systemTypes + "D|";
            if (User.IsInSystem(SystemType.ForeignFiling)) systemTypes = systemTypes + "F|";
            if (User.IsInSystem(SystemType.PatClearance)) systemTypes = systemTypes + "E|";
            if (User.IsInSystem(SystemType.AMS)) systemTypes = systemTypes + "A|";
            if (User.IsInSystem(SystemType.RMS)) systemTypes = systemTypes + "R|";
            if (User.IsInSystem(SystemType.SearchRequest)) systemTypes = systemTypes + "C|";
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