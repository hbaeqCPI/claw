using R10.Web.Helpers;
using AutoMapper;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using R10.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using R10.Core;
using R10.Core.Entities;
using R10.Core.Exceptions;
using R10.Core.Helpers;
using R10.Core.Interfaces;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;
using R10.Web.Extensions.ActionResults;
using R10.Web.Interfaces;
using R10.Web.Models.PageViewModels;
using R10.Web.Security;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using R10.Core.Entities.Patent;
using R10.Web.Areas.Shared.Services;
using R10.Core.Entities.Shared;
using R10.Core.Entities.GlobalSearch;

using R10.Web.Areas;

namespace R10.Web.Areas.Shared.Controllers
{
    [Area("Shared"), Authorize(Policy = SharedAuthorizationPolicy.CanAccessSystem)]
    public class QuickEmailDataSourceSetupController : BaseController
    {
        private readonly IAuthorizationService _authService;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly IMapper _mapper;
        private readonly ISystemSettings<PatSetting> _patSettings;
        private readonly ISystemSettings<DefaultSetting> _settings;
        private readonly IQuickEmailService _qeService;
        private readonly ExportHelper _exportHelper;
        private readonly IQEDataSourceViewModelService _qeDataSourceViewModelService;

        private readonly string _searchContainer = "qeDataSourceSearch";
        private readonly string _detailContainer = "qeDataSourceDetail";

        public QuickEmailDataSourceSetupController(
                    IAuthorizationService authService,
                    IStringLocalizer<SharedResource> localizer,
                    IQuickEmailService qeService,
                    IQEDataSourceViewModelService qeDataSourceViewModelService,
                    IMapper mapper,
                    ExportHelper exportHelper,
                    ISystemSettings<PatSetting> patSettings,
                    ISystemSettings<DefaultSetting> settings
            )
        {
            _authService = authService;
            _localizer = localizer;
            _qeService = qeService;
            _qeDataSourceViewModelService = qeDataSourceViewModelService;
            _mapper = mapper;
            _exportHelper = exportHelper;
            _patSettings = patSettings;
            _settings = settings;
        }

        [Authorize(Policy = PatentAuthorizationPolicy.FullRead)]
        public IActionResult Patent()
        {
            return Index("P");
        }

        [Authorize(Policy = TrademarkAuthorizationPolicy.FullRead)]
        public IActionResult Trademark()
        {
            return Index("T");
        }

        [Authorize(Policy = GeneralMatterAuthorizationPolicy.FullRead)]
        public IActionResult GeneralMatter()
        {
            return Index("G");
        }

        [Authorize(Policy = DMSAuthorizationPolicy.CanAccessSystem)]
        public IActionResult Disclosure()
        {
            return Index(SystemTypeCode.DMS);
        }

        [Authorize(Policy = AMSAuthorizationPolicy.CanAccessSystem)]
        public IActionResult AMS()
        {
            return Index(SystemTypeCode.AMS);
        }

        [Authorize(Policy = SearchRequestAuthorizationPolicy.CanAccessSystem)]
        public IActionResult Clearance()
        {
            return Index(SystemTypeCode.Clearance);
        }

        [Authorize(Policy = PatentClearanceAuthorizationPolicy.CanAccessSystem)]
        public IActionResult PatClearance()
        {
            return Index(SystemTypeCode.PatClearance);
        }

        private IActionResult Index(string sys)
        {
            var viewModel = new QEDataSourcePageViewModel()
            {
                PageId = _searchContainer,                  // container name
                DetailPageId = _detailContainer,            // for view data passed to _SearchIndex partial inside _SearchIndex
                Title = _localizer["Quick Email Data Sources"].ToString(),
                SystemType = sys
            };

            return View("Index", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PageRead([DataSourceRequest] DataSourceRequest request, List<QueryFilterViewModel> mainSearchFilters, string sys)
        {
            //var canAccess = await QEHelper.CanAccessQE(sys, User, _authService);
            //Guard.Against.NoRecordPermission(canAccess);

            // search grid result
            if (ModelState.IsValid)
            {
                var dataSources = _qeDataSourceViewModelService.AddCriteria(_qeService.QEDataSourcesFiltered, mainSearchFilters);                

                var result = await _qeDataSourceViewModelService.CreateViewModelForSearchGrid(request, dataSources);
                return Json(result);
            }
            return new JsonBadRequest(new { errors = ModelState.Errors() });
        }

        public async Task<IActionResult> Detail(int id, string sys)
        {
            //var canAccess = await QEHelper.CanAccessQE(sys, User, _authService);
            //Guard.Against.NoRecordPermission(canAccess);

            var page = await PrepareEditScreen(id, sys);
            if (page.Detail == null)
            {
                Guard.Against.NoRecordPermission(!Request.IsAjax());
                return RedirectToAction(GetEntryAction(sys));
            }

            SetDetailViewData(page);

            return PartialView("_DataSourceDetailContent", page.Detail);
        }

        public async Task<IActionResult> EmptyDetail(string sys)
        {
            var page = await PrepareEmptyScreen(sys);
            SetDetailViewData(page);
            return PartialView("_DataSourceDetailContent", page.Detail);
        }

        private async Task<DetailPageViewModel<QEDataSourceDetailViewModel>> PrepareEditScreen(int id, string sys)
        {
            var viewModel = new DetailPageViewModel<QEDataSourceDetailViewModel>();
            viewModel.Detail = await _qeDataSourceViewModelService.CreateViewModelForDetailScreen(id);

            if (viewModel.Detail != null)
            {
                if (!(await HasSystemPermission(viewModel.Detail.SystemType)))
                    throw new NoRecordPermissionException();

                AddSecurityPolicies(viewModel.Detail.SystemType, viewModel);
                await viewModel.ApplyDetailPagePermission(User, _authService);

                var customFieldPermission = new DetailPagePermission();
                customFieldPermission.CanAddRecord = viewModel.CanAddRecord;
                customFieldPermission.CanEditRecord = viewModel.CanEditRecord;
                customFieldPermission.CanDeleteRecord = viewModel.CanDeleteRecord;
                ViewBag.CustomFieldPermission = customFieldPermission;

                viewModel.CanSearch = false;
                viewModel.CanPrintRecord = false;
                viewModel.CanEmail = false;
                viewModel.CanDeleteRecord = false;
                viewModel.CanCopyRecord = false;
                viewModel.CanAddRecord = false;

                this.AddDefaultNavigationUrls(viewModel);

                viewModel.DeleteConfirmationUrl = Url.DeleteConfirmWithCodeLink();

                viewModel.RefreshRecordUrl = Url.Action("detail") + "?sys=" + sys;
                if (viewModel.CanCopyRecord)
                    viewModel.CopyScreenUrl += "/" + id.ToString() + "?sys=" + sys;
                if (viewModel.CanAddRecord)
                    viewModel.AddScreenUrl += "?sys=" + sys;
                if (viewModel.CanDeleteRecord)
                    viewModel.DeleteScreenUrl += "?sys=" + sys;

                viewModel.Container = _detailContainer;
                viewModel.Detail.SystemType = sys;


            }
            return viewModel;
        }

        private async Task<DetailPageViewModel<QEDataSourceDetailViewModel>> PrepareEmptyScreen(string sys)
        {
            var viewModel = new DetailPageViewModel<QEDataSourceDetailViewModel>();
            var detail = await _qeDataSourceViewModelService.CreateViewModelForDetailScreen(0);
            detail.DataSourceID = -1;
            detail.SystemType = sys;
            viewModel.Detail = detail;

            if (!(await HasSystemPermission(viewModel.Detail.SystemType)))
                throw new NoRecordPermissionException();

            AddSecurityPolicies(viewModel.Detail.SystemType, viewModel);
            await viewModel.ApplyDetailPagePermission(User, _authService);

            var customFieldPermission = new DetailPagePermission();
            customFieldPermission.CanAddRecord = viewModel.CanAddRecord;
            customFieldPermission.CanEditRecord = viewModel.CanEditRecord;
            customFieldPermission.CanDeleteRecord = viewModel.CanDeleteRecord;
            ViewBag.CustomFieldPermission = customFieldPermission;

            viewModel.CanSearch = false;
            viewModel.CanPrintRecord = false;
            viewModel.CanEmail = false;
            viewModel.CanRefreshRecord = false;
            viewModel.CanCopyRecord = false;
            viewModel.CanAddRecord = false;
            viewModel.CanDeleteRecord = false;

            this.AddDefaultNavigationUrls(viewModel);

            viewModel.AddScreenUrl += $"?sys={sys}";
            viewModel.Container = _detailContainer;

            return viewModel;
        }

        private string GetEntryAction(string sys)
        {
            switch (sys)
            {
                case "P": return "Patent";
                case "T": return "Trademark";
                case "G": return "GenMatter";
                default: return "";
            }
        }

        private void SetDetailViewData(DetailPageViewModel<QEDataSourceDetailViewModel> page)
        {
            ViewData["PagePermission"] = page;      // (DetailPagePermission)page;
            ViewData["PageId"] = page.Container;
        }

        public async Task<IActionResult> GetDataSourceFieldList([DataSourceRequest] DataSourceRequest request, int dataSourceId)
        {
            string sortField = "";
            string sortDir = "";

            if (request.Sorts != null && request.Sorts.Any())
            {
                sortField = request.Sorts[0].Member;
                sortDir = request.Sorts[0].SortDirection.ToString();
            }
            var result = await _qeService.GetDataSourceFieldList(dataSourceId, sortField, sortDir);
            return Json(result.ToDataSourceResult(request));
        }

        public async Task<IActionResult> ExportToExcel(int dataSourceId, string sortField, string sortDir)
        {
            var result = await _qeService.GetDataSourceFieldList(dataSourceId, sortField, sortDir);

            var excludeColumns = new List<string>();
            excludeColumns.Add("FieldSource");
            excludeColumns.Add("CustomFieldSettingId");

            var fileStream = await _exportHelper.ListToExcelMemoryStream(result, "FieldList", _localizer, excludeColumns: excludeColumns);

            return File(fileStream.ToArray(), ImageHelper.GetContentType(".xlsx"), "DataSourceFieldList.xlsx");

        }

        public async Task<IActionResult> GetPicklistData([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType, string requiredRelation = "", string systemType = "")
        {
            var dataSources = _qeService.QEDataSourcesFiltered.Where(ds => systemType == "" || ds.SystemType == systemType); ;

            return await GetPicklistData(dataSources, request, property, text, filterType, requiredRelation);
        }

        public async Task<IActionResult> GetRecordStamps(int id)
        {
            var dataSource = await _qeService.GetQeDataSourceByIdAsync(id);
            if (dataSource == null)
                return new NoRecordFoundResult();

            return ViewComponent("RecordStamps", new { createdBy = dataSource.CreatedBy, dateCreated = dataSource.DateCreated, updatedBy = dataSource.UpdatedBy, lastUpdate = dataSource.LastUpdate, tStamp = dataSource.tStamp });
        }

        private async Task<bool> HasSystemPermission(string systemType)
        {
            switch (systemType)
            {
                case SystemTypeCode.Patent:
                    return (await _authService.AuthorizeAsync(User, PatentAuthorizationPolicy.FullRead)).Succeeded;

                case SystemTypeCode.Trademark:
                    return (await _authService.AuthorizeAsync(User, TrademarkAuthorizationPolicy.FullRead)).Succeeded;

                case SystemTypeCode.GeneralMatter:
                    return (await _authService.AuthorizeAsync(User, GeneralMatterAuthorizationPolicy.FullRead)).Succeeded;

                case SystemTypeCode.DMS:
                    return (await _authService.AuthorizeAsync(User, DMSAuthorizationPolicy.CanAccessSystem)).Succeeded;

                case SystemTypeCode.AMS:
                    return (await _authService.AuthorizeAsync(User, AMSAuthorizationPolicy.CanAccessSystem)).Succeeded;

                case SystemTypeCode.Clearance:
                    return (await _authService.AuthorizeAsync(User, SearchRequestAuthorizationPolicy.CanAccessSystem)).Succeeded;

                case SystemTypeCode.PatClearance:
                    return (await _authService.AuthorizeAsync(User, PatentClearanceAuthorizationPolicy.CanAccessSystem)).Succeeded;

                default:
                    return false;
            }
        }
        private async Task<bool> HasDeletePermission(string systemType)
        {
            switch (systemType)
            {
                case SystemTypeCode.Patent:
                    return (await _authService.AuthorizeAsync(User, PatentAuthorizationPolicy.CanDelete)).Succeeded;

                case SystemTypeCode.Trademark:
                    return (await _authService.AuthorizeAsync(User, TrademarkAuthorizationPolicy.CanDelete)).Succeeded;

                case SystemTypeCode.GeneralMatter:
                    return (await _authService.AuthorizeAsync(User, GeneralMatterAuthorizationPolicy.CanDelete)).Succeeded;

                case SystemTypeCode.DMS:
                    return (await _authService.AuthorizeAsync(User, DMSAuthorizationPolicy.CanDelete)).Succeeded;

                case SystemTypeCode.AMS:
                    return (await _authService.AuthorizeAsync(User, AMSAuthorizationPolicy.CanDelete)).Succeeded;

                case SystemTypeCode.Clearance:
                    return (await _authService.AuthorizeAsync(User, SearchRequestAuthorizationPolicy.CanDelete)).Succeeded;

                case SystemTypeCode.PatClearance:
                    return (await _authService.AuthorizeAsync(User, PatentClearanceAuthorizationPolicy.CanDelete)).Succeeded;

                default:
                    return false;
            }
        }

        private async Task<bool> HasFullModifyPermission(string systemType)
        {
            switch (systemType)
            {
                case SystemTypeCode.Patent:
                    return (await _authService.AuthorizeAsync(User, PatentAuthorizationPolicy.FullModify)).Succeeded;

                case SystemTypeCode.Trademark:
                    return (await _authService.AuthorizeAsync(User, TrademarkAuthorizationPolicy.FullModify)).Succeeded;

                case SystemTypeCode.GeneralMatter:
                    return (await _authService.AuthorizeAsync(User, GeneralMatterAuthorizationPolicy.FullModify)).Succeeded;

                case SystemTypeCode.DMS:
                    return (await _authService.AuthorizeAsync(User, DMSAuthorizationPolicy.FullModify)).Succeeded;

                case SystemTypeCode.AMS:
                    return (await _authService.AuthorizeAsync(User, AMSAuthorizationPolicy.FullModify)).Succeeded;

                case SystemTypeCode.Clearance:
                    return (await _authService.AuthorizeAsync(User, SearchRequestAuthorizationPolicy.FullModify)).Succeeded;

                case SystemTypeCode.PatClearance:
                    return (await _authService.AuthorizeAsync(User, PatentClearanceAuthorizationPolicy.FullModify)).Succeeded;

                default:
                    return false;
            }
        }

        private async Task<bool> HasRemarksOnlyPermission(string systemType)
        {
            switch (systemType)
            {
                case SystemTypeCode.Patent:
                    return (await _authService.AuthorizeAsync(User, PatentAuthorizationPolicy.RemarksOnlyModify)).Succeeded;

                case SystemTypeCode.Trademark:
                    return (await _authService.AuthorizeAsync(User, TrademarkAuthorizationPolicy.RemarksOnlyModify)).Succeeded;

                case SystemTypeCode.GeneralMatter:
                    return (await _authService.AuthorizeAsync(User, GeneralMatterAuthorizationPolicy.RemarksOnlyModify)).Succeeded;

                case SystemTypeCode.DMS:
                    return (await _authService.AuthorizeAsync(User, DMSAuthorizationPolicy.RemarksOnlyModify)).Succeeded;

                case SystemTypeCode.AMS:
                    return (await _authService.AuthorizeAsync(User, AMSAuthorizationPolicy.RemarksOnlyModify)).Succeeded;

                case SystemTypeCode.Clearance:
                    return (await _authService.AuthorizeAsync(User, SearchRequestAuthorizationPolicy.RemarksOnlyModify)).Succeeded;

                case SystemTypeCode.PatClearance:
                    return (await _authService.AuthorizeAsync(User, PatentClearanceAuthorizationPolicy.RemarksOnlyModify)).Succeeded;

                default:
                    return false;
            }
        }

        private void AddSecurityPolicies(string systemType, DetailPageViewModel<QEDataSourceDetailViewModel> viewModel)
        {
            switch (systemType)
            {
                case SystemTypeCode.Patent:
                    viewModel.AddPatentSecurityPolicies();
                    break;

                case SystemTypeCode.Trademark:
                    viewModel.AddTrademarkSecurityPolicies();
                    break;

                case SystemTypeCode.GeneralMatter:
                    viewModel.AddGeneralMatterSecurityPolicies();
                    break;

                case SystemTypeCode.DMS:
                    viewModel.AddDMSSecurityPolicies();
                    break;

                case SystemTypeCode.Clearance:
                    viewModel.AddClearanceSecurityPolicies();
                    break;

                case SystemTypeCode.PatClearance:
                    viewModel.AddPatentClearanceSecurityPolicies();
                    break;

                default: //AMS
                    viewModel.AddAMSSecurityPolicies();
                    break;

            }
        }
    }
}