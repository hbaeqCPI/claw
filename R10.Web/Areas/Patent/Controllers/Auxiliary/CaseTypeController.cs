using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using R10.Core.Entities.Patent;
using R10.Core.Helpers;
using R10.Core.Interfaces;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;
using R10.Web.Extensions.ActionResults;
using R10.Web.Helpers;
using R10.Web.Interfaces;
using R10.Web.Models;
using R10.Web.Models.PageViewModels;
using R10.Web.Security;
using R10.Web.Services;
using Newtonsoft.Json;

using R10.Web.Areas;

namespace R10.Web.Areas.Patent.Controllers
{
    [Area("Patent"), Authorize(Policy = PatentAuthorizationPolicy.CanAccessAuxiliary)]
    public class CaseTypeController : BaseController
    {
        private readonly IAuthorizationService _authService;
        private readonly IViewModelService<PatCaseType> _viewModelService;
        private readonly IEntityService<PatCaseType> _auxService;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly IApplicationDbContext _repository;

        private readonly string _dataContainer = "patCaseTypeDetail";

        public CaseTypeController(
            IAuthorizationService authService,
            IViewModelService<PatCaseType> viewModelService,
            IEntityService<PatCaseType> auxService,
            IStringLocalizer<SharedResource> localizer,
            IApplicationDbContext repository)
        {
            _authService = authService;
            _viewModelService = viewModelService;
            _auxService = auxService;
            _localizer = localizer;
            _repository = repository;
        }

        public async Task<IActionResult> Index()
        {
            var model = new PageViewModel()
            {
                Page = PageType.Search,
                PageId = "patCaseTypeSearch",
                Title = _localizer["Case Type Search"].ToString(),
                CanAddRecord = (await _authService.AuthorizeAsync(User, PatentAuthorizationPolicy.AuxiliaryModify)).Succeeded
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
                PageId = "patCaseTypeSearchResults",
                Title = _localizer["Case Type Search Results"].ToString(),
                CanAddRecord = (await _authService.AuthorizeAsync(User, PatentAuthorizationPolicy.AuxiliaryModify)).Succeeded
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
                var caseTypes = _auxService.QueryableList;
                if (mainSearchFilters != null && mainSearchFilters.Count > 0)
                {
                    {
                    caseTypes = Helpers.QueryHelper.ApplySystemsFilter(caseTypes, mainSearchFilters, a => a.Systems);
                    }
                }
                caseTypes = _viewModelService.AddCriteria(caseTypes, mainSearchFilters);

                var result = await _viewModelService.CreateViewModelForGrid(request, caseTypes, "CaseType", "CaseType");
                return Json(result);
            }
            return new JsonBadRequest(new { errors = ModelState.Errors() });
        }

        private async Task<DetailPageViewModel<PatCaseType>> PrepareEditScreen(string id, string systems = "")
        {
            var viewModel = new DetailPageViewModel<PatCaseType>
            {
                Detail = await _auxService.QueryableList.FirstOrDefaultAsync(c => c.CaseType == id && c.Systems == systems)
            };

            if (viewModel.Detail != null)
            {
                viewModel.AddPatentAuxiliarySecurityPolicies();
                await viewModel.ApplyDetailPagePermission(User, _authService);

                viewModel.CanEmail = false;

                this.AddDefaultNavigationUrls(viewModel);

                viewModel.Container = _dataContainer;
                viewModel.AddScreenUrl = viewModel.CanAddRecord ? Url.Action("Add", new { fromSearch = true }) : "";

                viewModel.EditScreenUrl = this.Url.Action("Detail", new { id = id, systems = systems });
                viewModel.DeleteScreenUrl = viewModel.CanDeleteRecord ? Url.Action("Delete", new { caseTypeCode = id, systems = systems }) : "";
                viewModel.CopyScreenUrl = $"{viewModel.CopyScreenUrl}?id={id}&systems={Uri.EscapeDataString(systems)}";
                viewModel.SearchScreenUrl = this.Url.Action("Index");
            }
            return viewModel;
        }

        public async Task<IActionResult> Detail(string id, string systems = "", bool singleRecord = false, bool fromSearch = false)
        {
            var page = await PrepareEditScreen(id, systems);
            if (page.Detail == null)
            {
                if (Request.IsAjax())
                    return new RecordDoesNotExistResult();
                else
                    return RedirectToAction("Index");
            }

            PatCaseType detail = page.Detail;
            PageViewModel model = new PageViewModel()
            {
                Page = PageType.Detail,
                PageId = page.Container,
                Title = _localizer["Case Type Detail"].ToString(),
                SingleRecord = singleRecord || !Request.IsAjax(),
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

        [HttpPost()]
        public IActionResult ExcelExportSave(string contentType, string base64, string fileName)
        {
            var fileContents = Convert.FromBase64String(base64);
            return File(fileContents, contentType, fileName);
        }

        private async Task<DetailPageViewModel<PatCaseType>> PrepareAddScreen()
        {
            var viewModel = new DetailPageViewModel<PatCaseType>
            {
                Detail = new PatCaseType()
            };

            viewModel.AddPatentAuxiliarySecurityPolicies();
            await viewModel.ApplyDetailPagePermission(User, _authService);

            this.AddDefaultNavigationUrls(viewModel);
            viewModel.Container = _dataContainer;
            return viewModel;
        }

        [Authorize(Policy = PatentAuthorizationPolicy.AuxiliaryModify)]
        public async Task<IActionResult> Add(bool fromSearch = false)
        {
            if (!Request.IsAjax() && TempData.Peek("CopyOptions") == null)
                return RedirectToAction("Index");

            var page = await PrepareAddScreen();
            if (page.Detail == null)
                return RedirectToAction("Index");

            if (TempData["CopyOptions"] != null)
                await ExtractCopyParams(page);

            PatCaseType detail = page.Detail;
            PageViewModel model = new PageViewModel()
            {
                Page = fromSearch ? PageType.Detail : PageType.DetailContent,
                PageId = page.Container,
                Title = _localizer["New Case Type"].ToString(),
                PagePermission = page,
                Data = detail,
                FromSearch = fromSearch
            };
            ModelState.Clear();

            if (Request.IsAjax())
                return PartialView("Index", model);
            return View("Index", model);
        }

        [HttpPost, Authorize(Policy = PatentAuthorizationPolicy.AuxiliaryModify)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save([FromBody] PatCaseType caseType)
        {
            if (ModelState.IsValid)
            {
                var userName = User.GetUserName();
                var now = DateTime.Now;
                caseType.UserID = userName;
                caseType.LastUpdate = now;
                caseType.Description ??= "";
                caseType.Systems ??= "";

                // Require at least one system
                if (string.IsNullOrWhiteSpace(caseType.Systems))
                    return new JsonBadRequest("At least one system must be selected.");

                // Deduplicate and sort systems within this record
                var newSystems = caseType.Systems.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(s => s, StringComparer.OrdinalIgnoreCase).ToList();
                caseType.Systems = string.Join(",", newSystems);

                var isNewRecord = caseType.IsNewRecord || caseType.OriginalSystems == "__NEW__" || caseType.OriginalSystems == null;
                var originalSystemsValue = caseType.OriginalSystems == "__EMPTY__" ? "" : (caseType.OriginalSystems ?? "");

                // Find existing record on update (match by CaseType + original systems)
                PatCaseType existing = null;
                if (!isNewRecord)
                {
                    existing = await _auxService.QueryableList.AsNoTracking()
                        .FirstOrDefaultAsync(c => c.CaseType == caseType.CaseType && c.Systems == originalSystemsValue);
                }

                // Check for duplicate systems across other records with the same CaseType name
                var allRecords = await _auxService.QueryableList.AsNoTracking()
                    .Where(c => c.CaseType == caseType.CaseType && c.Systems != null && c.Systems != "")
                    .Select(c => c.Systems)
                    .ToListAsync();

                // Exclude existing record's systems from the check
                if (existing != null && !string.IsNullOrEmpty(existing.Systems))
                    allRecords.Remove(existing.Systems);

                var usedSystems = allRecords
                    .Where(s => !string.IsNullOrEmpty(s))
                    .SelectMany(s => s.Split(',', StringSplitOptions.RemoveEmptyEntries))
                    .Select(s => s.Trim())
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                var duplicates = newSystems.Where(s => usedSystems.Contains(s)).ToList();
                if (duplicates.Any())
                    return new JsonBadRequest($"The following systems are already assigned to {caseType.CaseType}: {string.Join(", ", duplicates)}");

                if (existing != null)
                {
                    caseType.DateCreated = existing.DateCreated ?? now;

                    // Use raw SQL to avoid EF composite key tracking issues
                    await _repository.Database.ExecuteSqlRawAsync(
                        @"UPDATE tblPatCaseType SET CaseType=@p0, Description=@p1, LockPatRecord=@p2, Systems=@p3, UserID=@p4, DateCreated=@p5, LastUpdate=@p6
                          WHERE CaseType=@p7 AND Systems=@p8",
                        caseType.CaseType, caseType.Description ?? "", caseType.LockPatRecord ?? false, caseType.Systems, caseType.UserID ?? "",
                        caseType.DateCreated, caseType.LastUpdate,
                        existing.CaseType, existing.Systems ?? "");
                }
                else
                {
                    caseType.DateCreated = now;
                    await _repository.Database.ExecuteSqlRawAsync(
                        @"INSERT INTO tblPatCaseType (CaseType, Description, LockPatRecord, Systems, UserID, DateCreated, LastUpdate)
                          VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6)",
                        caseType.CaseType, caseType.Description ?? "", caseType.LockPatRecord ?? false, caseType.Systems, caseType.UserID ?? "",
                        caseType.DateCreated, caseType.LastUpdate);
                }

                return Json(new { id = caseType.CaseType, redirectUrl = Url.Action("Detail", new { id = caseType.CaseType, systems = caseType.Systems, singleRecord = true }) });
            }
            else
                return new JsonBadRequest(new { errors = ModelState.Errors() });
        }

        [HttpPost, Authorize(Policy = PatentAuthorizationPolicy.AuxiliaryCanDelete)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string caseTypeCode, string systems = "")
        {
            var count = await _repository.Database.ExecuteSqlRawAsync(
                "DELETE FROM tblPatCaseType WHERE CaseType=@p0 AND Systems=@p1", caseTypeCode ?? "", systems ?? "");

            if (count == 0)
                return new RecordDoesNotExistResult();

            return Ok();
        }

        [HttpGet()]
        public async Task<IActionResult> Copy(string id, string systems = "")
        {
            var entity = await _auxService.QueryableList.FirstOrDefaultAsync(c => c.CaseType == id && c.Systems == systems);
            if (entity == null) return new RecordDoesNotExistResult();

            var viewModel = new CaseTypeCopyViewModel
            {
                OriginalCaseType = entity.CaseType,
                CaseType = entity.CaseType
            };
            return PartialView("_Copy", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditCopied([FromBody] CaseTypeCopyViewModel copy)
        {
            if (!ModelState.IsValid) return new JsonBadRequest(new { errors = ModelState.Errors() });
            TempData["CopyOptions"] = JsonConvert.SerializeObject(copy);
            return RedirectToAction("Add");
        }

        private async Task ExtractCopyParams(DetailPageViewModel<PatCaseType> page)
        {
            var copyOptionsString = TempData["CopyOptions"].ToString();
            ViewBag.CopyOptions = copyOptionsString;
            var copyOptions = JsonConvert.DeserializeObject<CaseTypeCopyViewModel>(copyOptionsString);
            if (copyOptions != null)
            {
                var source = await _auxService.QueryableList.AsNoTracking().FirstOrDefaultAsync(c => c.CaseType == copyOptions.OriginalCaseType);
                if (source != null)
                {
                    page.Detail.CaseType = copyOptions.CaseType;
                    page.Detail.Description = source.Description;
                    page.Detail.Systems = source.Systems;
                }
            }
        }

        [HttpGet]
        public IActionResult Print()
        {
            ViewBag.Url = Url.Action("Print");
            ViewBag.DownloadName = "Case Type Print Screen";
            return View();
        }

        [HttpPost]
        public IActionResult Print([FromBody] PrintViewModel patCaseTypePrintModel)
        {
            // ReportService removed during debloat
            return BadRequest("Report service is not available.");
        }

        public async Task<IActionResult> GetRecordStamps(string id, string systems = "")
        {
            var caseType = await _auxService.QueryableList.FirstOrDefaultAsync(c => c.CaseType == id && c.Systems == systems);
            if (caseType == null)
                return new NoRecordFoundResult();

            return ViewComponent("RecordStamps", new { createdBy = caseType.UserID, dateCreated = caseType.DateCreated, updatedBy = caseType.UserID, lastUpdate = caseType.LastUpdate });
        }

        public IActionResult GetSystemList()
        {
            return Json(Helpers.SystemsHelper.SystemNames);
        }

        public async Task<IActionResult> GetPicklistData([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType, string requiredRelation = "")
        {
            return await GetPicklistData(_auxService.QueryableList, request, property, text, filterType, requiredRelation);
        }

        public async Task<IActionResult> GetCaseTypeList([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType, string requiredRelation = "")
        {
            return await GetPicklistData(_auxService.QueryableList, request, property, text, filterType, new string[] { "CaseType", "Description" }, requiredRelation);
        }

        public async Task<IActionResult> GetCaseTypeByCountry(string country)
        {
            var caseTypes = _auxService.QueryableList;
            var list = await caseTypes.Select(c => new { CaseType = c.CaseType, Description = c.Description }).OrderBy(c => c.CaseType).ToListAsync();
            return Json(list);
        }

        [HttpGet()]
        public async Task<IActionResult> DetailLink(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                var entity = await _viewModelService.GetEntityByCode("CaseType", id);
                if (entity == null)
                    return new RecordDoesNotExistResult();
                else
                    return RedirectToAction(nameof(Detail), new { id = entity.CaseType, singleRecord = true, fromSearch = true });
            }
            else
                return RedirectToAction(nameof(Add), new { fromSearch = true });
        }

        public async Task<IActionResult> GetActiveCaseTypeList()
        {
            var list = await _auxService.QueryableList.Select(c => new { CaseType = c.CaseType, Description = c.Description }).OrderBy(c => c.CaseType).ToListAsync();

            return Json(list);
        }
    }
}