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
    public class CountryExpController : BaseController
    {
        private readonly IAuthorizationService _authService;
        private readonly IViewModelService<PatCountryExp> _viewModelService;
        private readonly IEntityService<PatCountryExp> _auxService;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly IApplicationDbContext _repository;

        private readonly string _dataContainer = "patCountryExpDetail";

        public CountryExpController(
            IAuthorizationService authService,
            IViewModelService<PatCountryExp> viewModelService,
            IEntityService<PatCountryExp> auxService,
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
                PageId = "patCountryExpSearch",
                Title = _localizer["Country Expiration Search"].ToString(),
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
                PageId = "patCountryExpSearchResults",
                Title = _localizer["Country Expiration Search Results"].ToString(),
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
                var result = await _viewModelService.CreateViewModelForGrid(request, entities, "Country", "CExpId");
                return Json(result);
            }
            return new JsonBadRequest(new { errors = ModelState.Errors() });
        }

        private async Task<DetailPageViewModel<PatCountryExp>> PrepareEditScreen(int id)
        {
            var viewModel = new DetailPageViewModel<PatCountryExp>
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

                viewModel.AddScreenUrl = viewModel.CanAddRecord ? Url.Action("Add", new { fromSearch = true }) : "";
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

            PatCountryExp detail = page.Detail;
            PageViewModel model = new PageViewModel()
            {
                Page = PageType.Detail,
                PageId = page.Container,
                Title = _localizer["Country Expiration Detail"].ToString(),
                RecordId = detail.CExpId,
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

        private async Task<DetailPageViewModel<PatCountryExp>> PrepareAddScreen()
        {
            var viewModel = new DetailPageViewModel<PatCountryExp>
            {
                Detail = new PatCountryExp()
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

            PatCountryExp detail = page.Detail;
            PageViewModel model = new PageViewModel()
            {
                Page = fromSearch ? PageType.Detail : PageType.DetailContent,
                PageId = page.Container,
                Title = _localizer["New Country Expiration"].ToString(),
                RecordId = detail.CExpId,
                PagePermission = page,
                Data = detail,
                FromSearch = fromSearch,
                AfterCancelledInsert = $"function() {{ window.location.href = '{Url.Action("Index")}'; }}"
            };
            ModelState.Clear();

            return Request.IsAjax() ? PartialView("Index", model) : View("Index", model);
        }

        [HttpPost, Authorize(Policy = PatentAuthorizationPolicy.AuxiliaryModify)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save([FromBody] PatCountryExp entity)
        {
            if (ModelState.IsValid)
            {
                entity.Systems ??= "";

                // Require at least one system
                if (string.IsNullOrWhiteSpace(entity.Systems))
                    return new JsonBadRequest("At least one system must be selected.");

                // Deduplicate and sort systems
                var newSystems = entity.Systems.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(s => s, StringComparer.OrdinalIgnoreCase).ToList();
                entity.Systems = string.Join(",", newSystems);

                var isNewRecord = entity.IsNewRecord || entity.OriginalSystems == "__NEW__" || entity.OriginalSystems == null;

                if (entity.CExpId > 0 && !isNewRecord)
                {
                    // Update existing record
                    var existing = await _auxService.QueryableList.AsNoTracking()
                        .FirstOrDefaultAsync(c => c.CExpId == entity.CExpId);

                    if (existing != null)
                    {
                        // Check for duplicate systems across records with the same key fields
                        var allRecords = await _auxService.QueryableList.AsNoTracking()
                            .Where(c => c.CExpId != entity.CExpId
                                && c.Country == entity.Country && c.CaseType == entity.CaseType
                                && c.Type == entity.Type && c.BasedOn == entity.BasedOn
                                && c.Yr == entity.Yr && c.Mo == entity.Mo && c.Dy == entity.Dy
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
                            return new JsonBadRequest($"The following systems are already assigned to another Country Expiration record: {string.Join(", ", duplicates)}");

                        var effStart = new Microsoft.Data.SqlClient.SqlParameter("@p8", System.Data.SqlDbType.DateTime) { Value = entity.EffStartDate.HasValue ? entity.EffStartDate.Value : DBNull.Value };
                        var effEnd = new Microsoft.Data.SqlClient.SqlParameter("@p9", System.Data.SqlDbType.DateTime) { Value = entity.EffEndDate.HasValue ? entity.EffEndDate.Value : DBNull.Value };

                        await _repository.Database.ExecuteSqlRawAsync(
                            @"UPDATE tblPatCountryExp SET Country=@p0, CaseType=@p1, Type=@p2, BasedOn=@p3,
                              Yr=@p4, Mo=@p5, Dy=@p6, EffBasedOn=@p7, EffStartDate=@p8, EffEndDate=@p9,
                              Systems=@p10
                              WHERE CExpId=@p11",
                            new object[] {
                                new Microsoft.Data.SqlClient.SqlParameter("@p0", entity.Country ?? ""),
                                new Microsoft.Data.SqlClient.SqlParameter("@p1", entity.CaseType ?? ""),
                                new Microsoft.Data.SqlClient.SqlParameter("@p2", entity.Type ?? ""),
                                new Microsoft.Data.SqlClient.SqlParameter("@p3", entity.BasedOn ?? ""),
                                new Microsoft.Data.SqlClient.SqlParameter("@p4", entity.Yr),
                                new Microsoft.Data.SqlClient.SqlParameter("@p5", entity.Mo),
                                new Microsoft.Data.SqlClient.SqlParameter("@p6", entity.Dy),
                                new Microsoft.Data.SqlClient.SqlParameter("@p7", entity.EffBasedOn ?? ""),
                                effStart, effEnd,
                                new Microsoft.Data.SqlClient.SqlParameter("@p10", entity.Systems),
                                new Microsoft.Data.SqlClient.SqlParameter("@p11", entity.CExpId)
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
                    // Check for duplicate systems across records with the same key fields
                    var allRecords = await _auxService.QueryableList.AsNoTracking()
                        .Where(c => c.Country == entity.Country && c.CaseType == entity.CaseType
                            && c.Type == entity.Type && c.BasedOn == entity.BasedOn
                            && c.Yr == entity.Yr && c.Mo == entity.Mo && c.Dy == entity.Dy
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
                        return new JsonBadRequest($"The following systems are already assigned to another Country Expiration record: {string.Join(", ", duplicates)}");

                    var effStart = new Microsoft.Data.SqlClient.SqlParameter("@p8", System.Data.SqlDbType.DateTime) { Value = entity.EffStartDate.HasValue ? entity.EffStartDate.Value : DBNull.Value };
                    var effEnd = new Microsoft.Data.SqlClient.SqlParameter("@p9", System.Data.SqlDbType.DateTime) { Value = entity.EffEndDate.HasValue ? entity.EffEndDate.Value : DBNull.Value };

                    await _repository.Database.ExecuteSqlRawAsync(
                        @"INSERT INTO tblPatCountryExp (CExpId, Country, CaseType, Type, BasedOn,
                          Yr, Mo, Dy, EffBasedOn, EffStartDate, EffEndDate, Systems)
                          VALUES ((SELECT ISNULL(MAX(CExpId),0)+1 FROM tblPatCountryExp), @p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9, @p10)",
                        new object[] {
                            new Microsoft.Data.SqlClient.SqlParameter("@p0", entity.Country ?? ""),
                            new Microsoft.Data.SqlClient.SqlParameter("@p1", entity.CaseType ?? ""),
                            new Microsoft.Data.SqlClient.SqlParameter("@p2", entity.Type ?? ""),
                            new Microsoft.Data.SqlClient.SqlParameter("@p3", entity.BasedOn ?? ""),
                            new Microsoft.Data.SqlClient.SqlParameter("@p4", entity.Yr),
                            new Microsoft.Data.SqlClient.SqlParameter("@p5", entity.Mo),
                            new Microsoft.Data.SqlClient.SqlParameter("@p6", entity.Dy),
                            new Microsoft.Data.SqlClient.SqlParameter("@p7", entity.EffBasedOn ?? ""),
                            effStart, effEnd,
                            new Microsoft.Data.SqlClient.SqlParameter("@p10", entity.Systems)
                        });

                    // Retrieve the newly inserted CExpId
                    var inserted = await _auxService.QueryableList.AsNoTracking()
                        .Where(c => c.Country == entity.Country && c.CaseType == entity.CaseType
                            && c.Type == entity.Type && c.Systems == entity.Systems)
                        .OrderByDescending(c => c.CExpId)
                        .FirstOrDefaultAsync();
                    if (inserted != null)
                        entity.CExpId = inserted.CExpId;
                }

                return Json(new { id = entity.CExpId, redirectUrl = Url.Action("Detail", new { id = entity.CExpId, singleRecord = true }) });
            }
            else
                return new JsonBadRequest(new { errors = ModelState.Errors() });
        }

        [HttpPost, Authorize(Policy = PatentAuthorizationPolicy.AuxiliaryCanDelete)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var count = await _repository.Database.ExecuteSqlRawAsync(
                "DELETE FROM tblPatCountryExp WHERE CExpId=@p0", id);

            if (count == 0)
                return new RecordDoesNotExistResult();

            return Ok();
        }

        private async Task<PatCountryExp> GetById(int id)
        {
            return await _auxService.QueryableList.SingleOrDefaultAsync((c => c.CExpId == id));
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
            var viewModel = new CountryExpCopyViewModel
            {
                CExpId = entity.CExpId,
                Country = entity.Country,
                CaseType = entity.CaseType,
                Type = entity.Type,
                BasedOn = entity.BasedOn,
                Yr = entity.Yr,
                Mo = entity.Mo,
                EffBasedOn = entity.EffBasedOn,
                EffStartDate = entity.EffStartDate,
                EffEndDate = entity.EffEndDate
            };
            return PartialView("_Copy", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditCopied([FromBody] CountryExpCopyViewModel copy)
        {
            if (!ModelState.IsValid) return new JsonBadRequest(new { errors = ModelState.Errors() });
            TempData["CopyOptions"] = JsonConvert.SerializeObject(copy);
            return RedirectToAction("Add");
        }

        private async Task ExtractCopyParams(DetailPageViewModel<PatCountryExp> page)
        {
            var copyOptionsString = TempData["CopyOptions"].ToString();
            ViewBag.CopyOptions = copyOptionsString;
            var copyOptions = JsonConvert.DeserializeObject<CountryExpCopyViewModel>(copyOptionsString);
            if (copyOptions != null)
            {
                var source = await _auxService.QueryableList.AsNoTracking().FirstOrDefaultAsync(c => c.CExpId == copyOptions.CExpId);
                if (source != null)
                {
                    page.Detail = source;
                    page.Detail.CExpId = 0;
                    page.Detail.Country = copyOptions.Country;
                    page.Detail.CaseType = copyOptions.CaseType;
                    page.Detail.Type = copyOptions.Type;
                    page.Detail.BasedOn = copyOptions.BasedOn;
                    page.Detail.Yr = copyOptions.Yr;
                    page.Detail.Mo = copyOptions.Mo;
                    page.Detail.EffBasedOn = copyOptions.EffBasedOn;
                    page.Detail.EffStartDate = copyOptions.EffStartDate;
                    page.Detail.EffEndDate = copyOptions.EffEndDate;
                }
            }
        }

        [HttpGet()]
        public async Task<IActionResult> DetailLink(string id)
        {
            if (!string.IsNullOrEmpty(id) && id != "0")
            {
                var entity = await _viewModelService.GetEntityByCode("CExpId", id);
                if (entity == null)
                    return new RecordDoesNotExistResult();
                else
                    return RedirectToAction(nameof(Detail), new { id = entity.CExpId, singleRecord = true, fromSearch = true });
            }
            else
                return RedirectToAction(nameof(Add), new { fromSearch = true });
        }
    }
}
