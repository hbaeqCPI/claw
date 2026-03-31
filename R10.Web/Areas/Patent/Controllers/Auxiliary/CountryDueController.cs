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
using R10.Core.Interfaces;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;
using R10.Web.Extensions.ActionResults;
using R10.Core.Helpers;
using R10.Web.Helpers;
using R10.Web.Interfaces;
using R10.Web.Models;
using R10.Web.Models.PageViewModels;
using R10.Web.Security;

using Newtonsoft.Json;
using R10.Web.Areas;

namespace R10.Web.Areas.Patent.Controllers
{
    [Area("Patent"), Authorize(Policy = PatentAuthorizationPolicy.CanAccessAuxiliary)]
    public class CountryDueController : BaseController
    {
        private readonly IAuthorizationService _authService;
        private readonly IViewModelService<PatCountryDue> _viewModelService;
        private readonly IEntityService<PatCountryDue> _auxService;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly IApplicationDbContext _repository;

        private readonly string _dataContainer = "patCountryDueDetail";

        public CountryDueController(
            IAuthorizationService authService,
            IViewModelService<PatCountryDue> viewModelService,
            IEntityService<PatCountryDue> auxService,
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
                PageId = "patCountryDueSearch",
                Title = _localizer["Country Due Search"].ToString(),
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
                PageId = "patCountryDueSearchResults",
                Title = _localizer["Country Due Search Results"].ToString(),
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
                var entities = _auxService.QueryableList;
                if (mainSearchFilters.Count > 0)
                {
                    var systemName = mainSearchFilters.FirstOrDefault(f => f.Property == "SystemName");
                    if (systemName != null)
                    {
                        entities = entities.Where(a => a.Systems != null && EF.Functions.Like(a.Systems, "%" + systemName.Value.Replace("%", "") + "%"));
                        mainSearchFilters.Remove(systemName);
                    }
                }
                entities = _viewModelService.AddCriteria(entities, mainSearchFilters);
                var result = await _viewModelService.CreateViewModelForGrid(request, entities, "ActionType", "CDueId");
                return Json(result);
            }
            return new JsonBadRequest(new { errors = ModelState.Errors() });
        }

        private async Task<DetailPageViewModel<PatCountryDue>> PrepareEditScreen(int id)
        {
            var viewModel = new DetailPageViewModel<PatCountryDue>
            {
                Detail = await GetById(id)
            };

            if (viewModel.Detail != null)
            {
                viewModel.AddPatentAuxiliarySecurityPolicies();
                await viewModel.ApplyDetailPagePermission(User, _authService);

                viewModel.CanEmail = false;

                this.AddDefaultNavigationUrls(viewModel);

                viewModel.Container = _dataContainer;

                viewModel.EditScreenUrl = this.Url.Action("Detail", new { id = id });
                viewModel.CopyScreenUrl = $"{viewModel.CopyScreenUrl}/{id}";
                viewModel.SearchScreenUrl = this.Url.Action("Index");
            }
            return viewModel;
        }

        public async Task<IActionResult> Detail(int id, bool singleRecord = false, bool fromSearch = false)
        {
            var page = await PrepareEditScreen(id);
            if (page.Detail == null)
            {
                if (Request.IsAjax())
                    return new RecordDoesNotExistResult();
                else
                    return RedirectToAction("Index");
            }

            PatCountryDue detail = page.Detail;
            PageViewModel model = new PageViewModel()
            {
                Page = PageType.Detail,
                PageId = page.Container,
                Title = _localizer["Country Due Detail"].ToString(),
                RecordId = detail.CDueId,
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

        private async Task<DetailPageViewModel<PatCountryDue>> PrepareAddScreen()
        {
            var viewModel = new DetailPageViewModel<PatCountryDue>
            {
                Detail = new PatCountryDue()
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
            if (!Request.IsAjax())
                return RedirectToAction("Index");

            var page = await PrepareAddScreen();
            if (page.Detail == null)
                return RedirectToAction("Index");

            if (TempData["CopyOptions"] != null)
                await ExtractCopyParams(page);

            PatCountryDue detail = page.Detail;
            PageViewModel model = new PageViewModel()
            {
                Page = fromSearch ? PageType.Detail : PageType.DetailContent,
                PageId = page.Container,
                Title = _localizer["New Country Due"].ToString(),
                RecordId = detail.CDueId,
                PagePermission = page,
                Data = detail,
                FromSearch = fromSearch
            };
            ModelState.Clear();

            return PartialView("Index", model);
        }

        [HttpPost, Authorize(Policy = PatentAuthorizationPolicy.AuxiliaryModify)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save([FromBody] PatCountryDue entity)
        {
            if (ModelState.IsValid)
            {
                var userName = User.GetUserName();
                var now = DateTime.Now;
                entity.UserID = userName;
                entity.LastUpdate = now;
                entity.Systems ??= "";

                // Require at least one system
                if (string.IsNullOrWhiteSpace(entity.Systems))
                    return new JsonBadRequest("At least one system must be selected.");

                // Deduplicate and sort systems
                var newSystems = entity.Systems.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(s => s, StringComparer.OrdinalIgnoreCase).ToList();
                entity.Systems = string.Join(",", newSystems);

                var isNewRecord = entity.IsNewRecord || entity.OriginalSystems == "__NEW__" || entity.OriginalSystems == null;

                if (entity.CDueId > 0 && !isNewRecord)
                {
                    // Update existing record by CDueId
                    var existing = await _auxService.QueryableList.AsNoTracking()
                        .FirstOrDefaultAsync(c => c.CDueId == entity.CDueId);

                    if (existing != null)
                    {
                        entity.DateCreated = existing.DateCreated ?? now;

                        // Check for duplicate systems across records with the same key fields
                        var allRecords = await _auxService.QueryableList.AsNoTracking()
                            .Where(c => c.CDueId != entity.CDueId
                                && c.Country == entity.Country && c.CaseType == entity.CaseType
                                && c.ActionType == entity.ActionType && c.ActionDue == entity.ActionDue
                                && c.BasedOn == entity.BasedOn && c.Yr == entity.Yr && c.Mo == entity.Mo
                                && c.Dy == entity.Dy && c.Indicator == entity.Indicator
                                && c.Systems != null && c.Systems != "")
                            .Select(c => c.Systems)
                            .ToListAsync();

                        var usedSystems = allRecords
                            .Where(s => !string.IsNullOrEmpty(s))
                            .SelectMany(s => s.Split(',', StringSplitOptions.RemoveEmptyEntries))
                            .Select(s => s.Trim())
                            .ToHashSet(StringComparer.OrdinalIgnoreCase);

                        var duplicates = newSystems.Where(s => usedSystems.Contains(s)).ToList();
                        if (duplicates.Any())
                            return new JsonBadRequest($"The following systems are already assigned to another Country Due record: {string.Join(", ", duplicates)}");

                        var effStart = new Microsoft.Data.SqlClient.SqlParameter("@p11", System.Data.SqlDbType.DateTime) { Value = entity.EffStartDate.HasValue ? entity.EffStartDate.Value : DBNull.Value };
                        var effEnd = new Microsoft.Data.SqlClient.SqlParameter("@p12", System.Data.SqlDbType.DateTime) { Value = entity.EffEndDate.HasValue ? entity.EffEndDate.Value : DBNull.Value };
                        var cpiPerm = new Microsoft.Data.SqlClient.SqlParameter("@p17", System.Data.SqlDbType.Int) { Value = entity.CPIPermanentID.HasValue ? entity.CPIPermanentID.Value : DBNull.Value };

                        await _repository.Database.ExecuteSqlRawAsync(
                            @"UPDATE tblPatCountryDue SET Country=@p0, CaseType=@p1, ActionType=@p2, ActionDue=@p3, BasedOn=@p4,
                              Yr=@p5, Mo=@p6, Dy=@p7, Indicator=@p8, Recurring=@p9,
                              EffBasedOn=@p10, EffStartDate=@p11, EffEndDate=@p12,
                              CPIAction=@p13, Calculate=@p14, CPIPermanentID=@p17,
                              Systems=@p15, UserID=@p18, DateCreated=@p19, LastUpdate=@p20
                              WHERE CDueId=@p16",
                            new object[] {
                                new Microsoft.Data.SqlClient.SqlParameter("@p0", entity.Country ?? ""),
                                new Microsoft.Data.SqlClient.SqlParameter("@p1", entity.CaseType ?? ""),
                                new Microsoft.Data.SqlClient.SqlParameter("@p2", entity.ActionType ?? ""),
                                new Microsoft.Data.SqlClient.SqlParameter("@p3", entity.ActionDue ?? ""),
                                new Microsoft.Data.SqlClient.SqlParameter("@p4", entity.BasedOn ?? ""),
                                new Microsoft.Data.SqlClient.SqlParameter("@p5", entity.Yr),
                                new Microsoft.Data.SqlClient.SqlParameter("@p6", entity.Mo),
                                new Microsoft.Data.SqlClient.SqlParameter("@p7", entity.Dy),
                                new Microsoft.Data.SqlClient.SqlParameter("@p8", entity.Indicator ?? ""),
                                new Microsoft.Data.SqlClient.SqlParameter("@p9", entity.Recurring),
                                new Microsoft.Data.SqlClient.SqlParameter("@p10", entity.EffBasedOn ?? ""),
                                effStart, effEnd,
                                new Microsoft.Data.SqlClient.SqlParameter("@p13", entity.CPIAction),
                                new Microsoft.Data.SqlClient.SqlParameter("@p14", entity.Calculate),
                                new Microsoft.Data.SqlClient.SqlParameter("@p15", entity.Systems),
                                new Microsoft.Data.SqlClient.SqlParameter("@p16", entity.CDueId),
                                cpiPerm,
                                new Microsoft.Data.SqlClient.SqlParameter("@p18", entity.UserID ?? ""),
                                new Microsoft.Data.SqlClient.SqlParameter("@p19", entity.DateCreated),
                                new Microsoft.Data.SqlClient.SqlParameter("@p20", entity.LastUpdate)
                            });
                    }
                    else
                    {
                        return new RecordDoesNotExistResult();
                    }
                }
                else
                {
                    // Insert new record
                    entity.DateCreated = now;

                    // Check for duplicate systems across records with the same key fields
                    var allRecords = await _auxService.QueryableList.AsNoTracking()
                        .Where(c => c.Country == entity.Country && c.CaseType == entity.CaseType
                            && c.ActionType == entity.ActionType && c.ActionDue == entity.ActionDue
                            && c.BasedOn == entity.BasedOn && c.Yr == entity.Yr && c.Mo == entity.Mo
                            && c.Dy == entity.Dy && c.Indicator == entity.Indicator
                            && c.Systems != null && c.Systems != "")
                        .Select(c => c.Systems)
                        .ToListAsync();

                    var usedSystems = allRecords
                        .Where(s => !string.IsNullOrEmpty(s))
                        .SelectMany(s => s.Split(',', StringSplitOptions.RemoveEmptyEntries))
                        .Select(s => s.Trim())
                        .ToHashSet(StringComparer.OrdinalIgnoreCase);

                    var duplicates = newSystems.Where(s => usedSystems.Contains(s)).ToList();
                    if (duplicates.Any())
                        return new JsonBadRequest($"The following systems are already assigned to another Country Due record: {string.Join(", ", duplicates)}");

                    await _repository.Database.ExecuteSqlRawAsync(
                        @"INSERT INTO tblPatCountryDue (CDueId, Country, CaseType, ActionType, ActionDue, BasedOn,
                          Yr, Mo, Dy, Indicator, Recurring,
                          EffBasedOn, EffStartDate, EffEndDate,
                          CPIAction, Calculate, CPIPermanentID,
                          Systems, UserID, DateCreated, LastUpdate)
                          VALUES ((SELECT ISNULL(MAX(CDueId),0)+1 FROM tblPatCountryDue), @p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9, @p10, @p11, @p12, @p13, @p14, @p17, @p15, @p18, @p19, @p20)",
                        new object[] {
                            new Microsoft.Data.SqlClient.SqlParameter("@p0", entity.Country ?? ""),
                            new Microsoft.Data.SqlClient.SqlParameter("@p1", entity.CaseType ?? ""),
                            new Microsoft.Data.SqlClient.SqlParameter("@p2", entity.ActionType ?? ""),
                            new Microsoft.Data.SqlClient.SqlParameter("@p3", entity.ActionDue ?? ""),
                            new Microsoft.Data.SqlClient.SqlParameter("@p4", entity.BasedOn ?? ""),
                            new Microsoft.Data.SqlClient.SqlParameter("@p5", entity.Yr),
                            new Microsoft.Data.SqlClient.SqlParameter("@p6", entity.Mo),
                            new Microsoft.Data.SqlClient.SqlParameter("@p7", entity.Dy),
                            new Microsoft.Data.SqlClient.SqlParameter("@p8", entity.Indicator ?? ""),
                            new Microsoft.Data.SqlClient.SqlParameter("@p9", entity.Recurring),
                            new Microsoft.Data.SqlClient.SqlParameter("@p10", entity.EffBasedOn ?? ""),
                            new Microsoft.Data.SqlClient.SqlParameter("@p11", System.Data.SqlDbType.DateTime) { Value = entity.EffStartDate.HasValue ? entity.EffStartDate.Value : DBNull.Value },
                            new Microsoft.Data.SqlClient.SqlParameter("@p12", System.Data.SqlDbType.DateTime) { Value = entity.EffEndDate.HasValue ? entity.EffEndDate.Value : DBNull.Value },
                            new Microsoft.Data.SqlClient.SqlParameter("@p13", entity.CPIAction),
                            new Microsoft.Data.SqlClient.SqlParameter("@p14", entity.Calculate),
                            new Microsoft.Data.SqlClient.SqlParameter("@p15", entity.Systems),
                            new Microsoft.Data.SqlClient.SqlParameter("@p17", System.Data.SqlDbType.Int) { Value = entity.CPIPermanentID.HasValue ? entity.CPIPermanentID.Value : DBNull.Value },
                            new Microsoft.Data.SqlClient.SqlParameter("@p18", entity.UserID ?? ""),
                            new Microsoft.Data.SqlClient.SqlParameter("@p19", entity.DateCreated),
                            new Microsoft.Data.SqlClient.SqlParameter("@p20", entity.LastUpdate)
                        });

                    // Retrieve the newly inserted CDueId
                    var inserted = await _auxService.QueryableList.AsNoTracking()
                        .Where(c => c.Country == entity.Country && c.CaseType == entity.CaseType
                            && c.ActionType == entity.ActionType && c.Systems == entity.Systems)
                        .OrderByDescending(c => c.CDueId)
                        .FirstOrDefaultAsync();
                    if (inserted != null)
                        entity.CDueId = inserted.CDueId;
                }

                return Json(new { id = entity.CDueId, redirectUrl = Url.Action("Detail", new { id = entity.CDueId, singleRecord = true }) });
            }
            else
                return new JsonBadRequest(new { errors = ModelState.Errors() });
        }

        [HttpPost, Authorize(Policy = PatentAuthorizationPolicy.AuxiliaryCanDelete)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var count = await _repository.Database.ExecuteSqlRawAsync(
                "DELETE FROM tblPatCountryDue WHERE CDueId=@p0", id);

            if (count == 0)
                return new RecordDoesNotExistResult();

            return Ok();
        }

        private async Task<PatCountryDue> GetById(int id)
        {
            return await _auxService.QueryableList.SingleOrDefaultAsync((c => c.CDueId == id));
        }

        public async Task<IActionResult> GetSystemList()
        {
            var systems = (await _repository.AppSystems.AsNoTracking()
                .Select(s => s.SystemName)
                .ToListAsync())
                .OrderBy(s => s, StringComparer.OrdinalIgnoreCase).ThenBy(s => s.Length).ToList();
            return Json(systems);
        }

        public async Task<IActionResult> GetPicklistData([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType, string requiredRelation = "")
        {
            return await GetPicklistData(_auxService.QueryableList, request, property, text, filterType, requiredRelation);
        }

        [HttpGet()]
        public async Task<IActionResult> Copy(int id)
        {
            var entity = await GetById(id);
            if (entity == null) return new RecordDoesNotExistResult();
            var viewModel = new CountryDueCopyViewModel
            {
                CDueId = entity.CDueId,
                Country = entity.Country,
                CaseType = entity.CaseType,
                ActionType = entity.ActionType,
                ActionDue = entity.ActionDue,
                BasedOn = entity.BasedOn,
                Yr = entity.Yr,
                Mo = entity.Mo,
                Dy = entity.Dy,
                Indicator = entity.Indicator,
                Recurring = entity.Recurring,
                EffBasedOn = entity.EffBasedOn,
                EffStartDate = entity.EffStartDate,
                EffEndDate = entity.EffEndDate
            };
            return PartialView("_Copy", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditCopied([FromBody] CountryDueCopyViewModel copy)
        {
            if (!ModelState.IsValid) return new JsonBadRequest(new { errors = ModelState.Errors() });
            TempData["CopyOptions"] = JsonConvert.SerializeObject(copy);
            return RedirectToAction("Add");
        }

        private async Task ExtractCopyParams(DetailPageViewModel<PatCountryDue> page)
        {
            var copyOptionsString = TempData["CopyOptions"].ToString();
            ViewBag.CopyOptions = copyOptionsString;
            var copyOptions = JsonConvert.DeserializeObject<CountryDueCopyViewModel>(copyOptionsString);
            if (copyOptions != null)
            {
                var source = await _auxService.QueryableList.AsNoTracking().FirstOrDefaultAsync(c => c.CDueId == copyOptions.CDueId);
                if (source != null)
                {
                    page.Detail = source;
                    page.Detail.CDueId = 0;
                    page.Detail.Country = copyOptions.Country;
                    page.Detail.CaseType = copyOptions.CaseType;
                    page.Detail.ActionType = copyOptions.ActionType;
                    page.Detail.ActionDue = copyOptions.ActionDue;
                    page.Detail.BasedOn = copyOptions.BasedOn;
                    page.Detail.Yr = copyOptions.Yr;
                    page.Detail.Mo = copyOptions.Mo;
                    page.Detail.Dy = copyOptions.Dy;
                    page.Detail.Indicator = copyOptions.Indicator;
                    page.Detail.Recurring = copyOptions.Recurring;
                    page.Detail.EffBasedOn = copyOptions.EffBasedOn;
                    page.Detail.EffStartDate = copyOptions.EffStartDate;
                    page.Detail.EffEndDate = copyOptions.EffEndDate;
                    page.Detail.CPIAction = false;
                    page.Detail.CPIPermanentID = 0;
                }
            }
        }

        [HttpGet()]
        public async Task<IActionResult> DetailLink(string id)
        {
            if (!string.IsNullOrEmpty(id) && id != "0")
            {
                var entity = await _viewModelService.GetEntityByCode("CDueId", id);
                if (entity == null)
                    return new RecordDoesNotExistResult();
                else
                    return RedirectToAction(nameof(Detail), new { id = entity.CDueId, singleRecord = true, fromSearch = true });
            }
            else
                return RedirectToAction(nameof(Add), new { fromSearch = true });
        }
    }
}
