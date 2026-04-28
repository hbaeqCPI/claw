using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using LawPortal.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using LawPortal.Core.Entities;
using LawPortal.Core.Helpers;
using LawPortal.Core.Interfaces;
using LawPortal.Web.Extensions;
using LawPortal.Web.Extensions.ActionResults;
using LawPortal.Web.Helpers;
using LawPortal.Web.Interfaces;
using LawPortal.Web.Models.PageViewModels;
using LawPortal.Web.Areas.Shared.ViewModels;
using LawPortal.Web.Security;
using LawPortal.Web.Services;
using LawPortal.Web.Areas;

namespace LawPortal.Web.Areas.Shared.Controllers
{
    [Area("Shared"), Authorize(Policy = SharedAuthorizationPolicy.CanAccessSystem)]
    public class SystemController : BaseController
    {
        private readonly IAuthorizationService _authService;
        private readonly IViewModelService<AppSystem> _viewModelService;
        private readonly IEntityService<AppSystem> _systemService;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly IApplicationDbContext _repository;

        private readonly string _dataContainer = "cpiSystemDetail";

        public SystemController(
            IAuthorizationService authService,
            IViewModelService<AppSystem> viewModelService,
            IEntityService<AppSystem> systemService,
            IStringLocalizer<SharedResource> localizer,
            IApplicationDbContext repository)
        {
            _authService = authService;
            _viewModelService = viewModelService;
            _systemService = systemService;
            _localizer = localizer;
            _repository = repository;
        }

        public async Task<IActionResult> Index()
        {
            var model = new PageViewModel()
            {
                Page = PageType.Search,
                PageId = "cpiSystemSearch",
                Title = _localizer["System Search"].ToString(),
                CanAddRecord = false // Systems are preset and cannot be added
            };

            if (Request.IsAjax())
                return PartialView("Index", model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Search([FromBody] List<QueryFilterViewModel> mainSearchFilters)
        {
            var model = new PageViewModel()
            {
                Page = PageType.SearchResults,
                PageId = "cpiSystemSearchResults",
                Title = _localizer["System Search Results"].ToString(),
                CanAddRecord = false // Systems are preset and cannot be added
            };

            return PartialView("Index", model);
        }

        [HttpGet]
        public IActionResult Search()
        {
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> PageRead([DataSourceRequest] DataSourceRequest request, List<QueryFilterViewModel> mainSearchFilters)
        {
            if (ModelState.IsValid)
            {
                var systems = _viewModelService.AddCriteria(mainSearchFilters);
                var result = await _viewModelService.CreateViewModelForGrid(request, systems, "SystemName", "SystemId");
                return Json(result);
            }
            return new JsonBadRequest(new { errors = ModelState.Errors() });
        }

        [HttpPost()]
        public IActionResult ExcelExportSave(string contentType, string base64, string fileName)
        {
            var fileContents = Convert.FromBase64String(base64);
            return File(fileContents, contentType, fileName);
        }

        public async Task<IActionResult> Detail(int id, bool singleRecord = false, bool fromSearch = false, string tab = "")
        {
            var page = await PrepareEditScreen(id);
            if (page.Detail == null)
            {
                if (Request.IsAjax())
                    return new RecordDoesNotExistResult();
                else
                    return RedirectToAction("Index");
            }

            var detail = page.Detail;
            PageViewModel model = new PageViewModel()
            {
                Page = PageType.Detail,
                PageId = page.Container,
                Title = _localizer["System Detail"].ToString(),
                RecordId = detail.SystemId,
                SingleRecord = singleRecord || !Request.IsAjax(),
                ActiveTab = tab,
                PagePermission = page,
                Data = detail
            };

            if (Request.IsAjax())
            {
                if (!singleRecord && !fromSearch)
                    model.Page = PageType.DetailContent;

                return PartialView("Index", model);
            }

            return View("Index", model);
        }

        [Authorize(Policy = SharedAuthorizationPolicy.FullModify)]
        public async Task<IActionResult> Add(string id, bool fromSearch = false)
        {
            if (!Request.IsAjax())
                return RedirectToAction("Index");

            var page = await PrepareAddScreen();
            if (page.Detail == null)
                return RedirectToAction("Index");

            var detail = page.Detail;

            if (!string.IsNullOrEmpty(id))
                detail.SystemName = id;

            PageViewModel model = new PageViewModel()
            {
                Page = fromSearch ? PageType.Detail : PageType.DetailContent,
                PageId = page.Container,
                Title = _localizer["New System"].ToString(),
                RecordId = detail.SystemId,
                PagePermission = page,
                Data = detail,
                FromSearch = fromSearch
            };

            return PartialView("Index", model);
        }

        [HttpPost, Authorize(Policy = SharedAuthorizationPolicy.CanDelete)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _systemService.QueryableList.FirstOrDefaultAsync(c => c.SystemId == id);

            if (entity == null)
                return new RecordDoesNotExistResult();

            var systemName = entity.SystemName;

            // Delete the system
            await _systemService.Delete(entity);

            // Cascade: remove this system name from all Systems columns across all tables
            if (!string.IsNullOrEmpty(systemName))
            {
                await RemoveSystemFromAllTables(systemName);
            }

            return Ok();
        }

        /// <summary>
        /// Removes a system name from the comma-separated Systems column across all tables.
        /// For each table with a Systems column, parses out the deleted system name and updates the row.
        /// </summary>
        private async Task RemoveSystemFromAllTables(string systemName)
        {
            // Get all table names that have a Systems column
            var tables = await _repository.Database.SqlQueryRaw<string>(
                "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE COLUMN_NAME = 'Systems' AND TABLE_NAME LIKE 'tbl%'")
                .ToListAsync();

            foreach (var table in tables)
            {
                // Update each table: parse out the system name from comma-separated Systems
                // Use STRING_SPLIT to parse, filter out the deleted system, and STRING_AGG to rejoin
                // Delete records where this is the ONLY system (would become empty after removal)
                var deleteSql = $@"DELETE FROM [{table}]
                    WHERE Systems IS NOT NULL AND Systems <> '' AND Systems = @p0";
                await _repository.Database.ExecuteSqlRawAsync(deleteSql,
                    new Microsoft.Data.SqlClient.SqlParameter("@p0", systemName));

                // Remove the system from records that have multiple systems
                var updateSql = $@"UPDATE [{table}] SET Systems = (
                    SELECT ISNULL(STRING_AGG(LTRIM(RTRIM(s.value)), ','), '')
                    FROM STRING_SPLIT(Systems, ',') s
                    WHERE LTRIM(RTRIM(s.value)) <> @p0 AND LTRIM(RTRIM(s.value)) <> ''
                )
                WHERE Systems IS NOT NULL AND Systems <> ''
                AND (Systems LIKE @p0 + ',%' OR Systems LIKE '%,' + @p0 OR Systems LIKE '%,' + @p0 + ',%')";
                await _repository.Database.ExecuteSqlRawAsync(updateSql,
                    new Microsoft.Data.SqlClient.SqlParameter("@p0", systemName));
            }
        }

        [HttpPost, Authorize(Policy = SharedAuthorizationPolicy.FullModify)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save([FromBody] AppSystem appSystem)
        {
            if (ModelState.IsValid)
            {
                UpdateEntityStamps(appSystem, appSystem.SystemId);

                if (appSystem.SystemId > 0)
                    await _systemService.Update(appSystem);
                else
                    await _systemService.Add(appSystem);

                return Json(appSystem.SystemId);
            }
            else
            {
                return new JsonBadRequest(new { errors = ModelState.Errors() });
            }
        }

        public async Task<IActionResult> GetRecordStamps(int id)
        {
            var appSystem = await _systemService.QueryableList.FirstOrDefaultAsync(s => s.SystemId == id);
            if (appSystem == null)
                return new NoRecordFoundResult();

            return ViewComponent("RecordStamps", new { createdBy = appSystem.CreatedBy, dateCreated = appSystem.DateCreated, updatedBy = appSystem.UpdatedBy, lastUpdate = appSystem.LastUpdate });
        }

        public async Task<IActionResult> GetPicklistData([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType, string requiredRelation = "")
        {
            return await GetPicklistData(_systemService.QueryableList, request, property, text, filterType, requiredRelation);
        }

        [HttpGet()]
        public async Task<IActionResult> DetailLink(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                var entity = await _viewModelService.GetEntityByCode("SystemName", id);
                if (entity == null)
                    return RedirectToAction(nameof(Add), new { id = id, fromSearch = true });
                else
                    return RedirectToAction(nameof(Detail), new { id = entity.SystemId, singleRecord = true, fromSearch = true });
            }
            else
                return RedirectToAction(nameof(Add), new { fromSearch = true });
        }

        private async Task<DetailPageViewModel<AppSystem>> PrepareEditScreen(int id)
        {
            var viewModel = new DetailPageViewModel<AppSystem>();
            viewModel.Detail = await _systemService.QueryableList.FirstOrDefaultAsync(c => c.SystemId == id);

            if (viewModel.Detail != null)
            {
                viewModel.AddSharedSecurityPolicies();
                await viewModel.ApplyDetailPagePermission(User, _authService);

                viewModel.CanCopyRecord = false;
                viewModel.CanEmail = false;
                viewModel.CanDeleteRecord = false; // Systems are preset

                this.AddDefaultNavigationUrls(viewModel);

                viewModel.EditScreenUrl = $"{viewModel.EditScreenUrl}/{id}";
                viewModel.Container = _dataContainer;
                viewModel.SearchScreenUrl = this.Url.Action("Index");
            }
            return viewModel;
        }

        private async Task<DetailPageViewModel<AppSystem>> PrepareAddScreen()
        {
            var viewModel = new DetailPageViewModel<AppSystem>();
            viewModel.Detail = new AppSystem();

            viewModel.AddSharedSecurityPolicies();
            await viewModel.ApplyDetailPagePermission(User, _authService);

            this.AddDefaultNavigationUrls(viewModel);
            viewModel.Container = _dataContainer;
            return viewModel;
        }
    }
}
