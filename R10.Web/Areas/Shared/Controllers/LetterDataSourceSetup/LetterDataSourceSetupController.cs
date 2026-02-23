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

using R10.Web.Areas;

namespace R10.Web.Areas.Shared.Controllers
{
    [Area("Shared"), Authorize(Policy = SharedAuthorizationPolicy.CanAccessLetters)]
    public class LetterDataSourceSetupController : BaseController
    {
        private readonly IAuthorizationService _authService;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly IMapper _mapper;
        private readonly ISystemSettings<PatSetting> _patSettings;
        private readonly ILetterService _letterService;
        private readonly ExportHelper _exportHelper;
        private readonly ILetterDataSourceViewModelService _letterDataSourceViewModelService;

        private readonly string _searchContainer = "letterDataSourceSearch";
        private readonly string _detailContainer = "letterDataSourceDetail";

        public LetterDataSourceSetupController(
                    IAuthorizationService authService,
                    IStringLocalizer<SharedResource> localizer,
                    ILetterService letterService,
                    ILetterDataSourceViewModelService letterDataSourceViewModelService,
                    IMapper mapper,
                    ExportHelper exportHelper,
                    ISystemSettings<PatSetting> patSettings
                    )
        {
            _authService = authService;
            _localizer = localizer;
            _letterService = letterService;
            _letterDataSourceViewModelService = letterDataSourceViewModelService;
            _mapper = mapper;
            _exportHelper = exportHelper;
            _patSettings = patSettings;
        }

        [Authorize(Policy = PatentAuthorizationPolicy.CanAccessLettersSetup)]
        public IActionResult Patent()
        {
            return Index("P");
        }

        [Authorize(Policy = TrademarkAuthorizationPolicy.CanAccessLettersSetup)]
        public IActionResult Trademark()
        {
            return Index("T");
        }

        [Authorize(Policy = GeneralMatterAuthorizationPolicy.CanAccessLettersSetup)]
        public IActionResult GenMatter()
        {
            return Index("G");
        }

        private IActionResult Index(string sys)
        {
            var viewModel = new LetterDataSourcePageViewModel()
            {
                PageId = _searchContainer,                  // container name
                DetailPageId = _detailContainer,            // for view data passed to _SearchIndex partial inside _SearchIndex
                Title = _localizer["Letter Data Sources"].ToString(),
                SystemType = sys
            };

            return View("Index", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PageRead([DataSourceRequest] DataSourceRequest request, List<QueryFilterViewModel> mainSearchFilters, string sys)
        {
            var canAccess = await LetterHelper.CanAccessLetter(sys, User, _authService);
            Guard.Against.NoRecordPermission(canAccess);

            // search grid result
            if (ModelState.IsValid)
            {
                var dataSources = _letterDataSourceViewModelService.AddCriteria(_letterService.FilteredLetterDataSources.Where(ds => ds.SystemType == sys), mainSearchFilters);

                var result = await _letterDataSourceViewModelService.CreateViewModelForSearchGrid(request, dataSources);
                return Json(result);
            }
            return new JsonBadRequest(new { errors = ModelState.Errors() });
        }

        public async Task<IActionResult> Detail(int id, string sys)
        {
            var canAccess = await LetterHelper.CanAccessLetter(sys, User, _authService);
            Guard.Against.NoRecordPermission(canAccess);

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

        private async Task<DetailPageViewModel<LetterDataSourceDetailViewModel>> PrepareEditScreen(int id, string sys)
        {
            var viewModel = new DetailPageViewModel<LetterDataSourceDetailViewModel>();
            viewModel.Detail = await _letterDataSourceViewModelService.CreateViewModelForDetailScreen(id);

            if (viewModel.Detail != null)
            {
                viewModel.AddLettersSecurityPolicies(sys);
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

        private async Task<DetailPageViewModel<LetterDataSourceDetailViewModel>> PrepareEmptyScreen(string sys)
        {
            var viewModel = new DetailPageViewModel<LetterDataSourceDetailViewModel>();
            var detail = await _letterDataSourceViewModelService.CreateViewModelForDetailScreen(0);
            detail.DataSourceId = -1;
            detail.SystemType = sys;
            viewModel.Detail = detail;

            viewModel.AddLettersSecurityPolicies(sys);
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

        private void SetDetailViewData(DetailPageViewModel<LetterDataSourceDetailViewModel> page)
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
            var result = await _letterService.GetDataSourceFieldList(dataSourceId, sortField, sortDir);
            return Json(result.ToDataSourceResult(request));
        }

        public async Task<IActionResult> ExportToExcel(int dataSourceId, string sortField, string sortDir)
        {
            var result = await _letterService.GetDataSourceFieldList(dataSourceId, sortField, sortDir);

            var excludeColumns = new List<string>();
            excludeColumns.Add("FieldSource");
            excludeColumns.Add("CustomFieldSettingId");

            var fileStream = await _exportHelper.ListToExcelMemoryStream(result, "FieldList", _localizer, excludeColumns: excludeColumns);

            return File(fileStream.ToArray(), ImageHelper.GetContentType(".xlsx"), "DataSourceFieldList.xlsx");

        }

        public async Task<IActionResult> GetPicklistData([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType, string requiredRelation = "", string system = "")
        {
            var dataSources = _letterService.FilteredLetterDataSources.Where(ds => system == "" || ds.SystemType == system);

            return await GetPicklistData(dataSources, request, property, text, filterType, requiredRelation);
        }

        public async Task<IActionResult> GetRecordStamps(int id)
        {
            var dataSource = await _letterService.GetDataSourceById(id);
            if (dataSource == null)
                return new NoRecordFoundResult();

            return ViewComponent("RecordStamps", new { createdBy = dataSource.CreatedBy, dateCreated = dataSource.DateCreated, updatedBy = dataSource.UpdatedBy, lastUpdate = dataSource.LastUpdate, tStamp = dataSource.tStamp });
        }
    }
}