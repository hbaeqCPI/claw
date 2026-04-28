using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using LawPortal.Core.Entities.Patent;
using LawPortal.Core.Interfaces;
using LawPortal.Web.Extensions;
using LawPortal.Web.Extensions.ActionResults;
using LawPortal.Web.Helpers;
using LawPortal.Web.Models;
using LawPortal.Web.Areas.Shared.ViewModels;
using LawPortal.Web.Models.PageViewModels;
using LawPortal.Web.Security;

namespace LawPortal.Web.Areas.Patent.Controllers
{
    [Area("Patent"), Authorize(Policy = PatentAuthorizationPolicy.CanAccessAuxiliary)]
    public class CountryExpDeleteController : BaseController
    {
        private readonly IAuthorizationService _authService;
        private readonly IApplicationDbContext _repository;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly string _dataContainer = "patCountryExpDeleteDetail";

        public CountryExpDeleteController(IAuthorizationService authService, IApplicationDbContext repository, IStringLocalizer<SharedResource> localizer)
        {
            _authService = authService;
            _repository = repository;
            _localizer = localizer;
        }

        private async Task<DetailPagePermission> GetPermission()
        {
            var p = new DetailPagePermission();
            p.CanEditRecord = (await _authService.AuthorizeAsync(User, PatentAuthorizationPolicy.AuxiliaryModify)).Succeeded;
            p.CanDeleteRecord = (await _authService.AuthorizeAsync(User, PatentAuthorizationPolicy.AuxiliaryCanDelete)).Succeeded;
            p.CanAddRecord = p.CanEditRecord;
            p.CanCopyRecord = p.CanEditRecord;
            return p;
        }

        public async Task<IActionResult> Index()
        {
            var model = new PageViewModel { Page = PageType.Search, PageId = "patCountryExpDeleteSearch", Title = _localizer["Country Exp Delete Search"].ToString(), CanAddRecord = (await _authService.AuthorizeAsync(User, PatentAuthorizationPolicy.AuxiliaryModify)).Succeeded };
            return Request.IsAjax() ? PartialView("Index", model) : View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Search([FromBody] List<QueryFilterViewModel> mainSearchFilters)
        {
            var model = new PageViewModel { Page = PageType.SearchResults, PageId = "patCountryExpDeleteSearchResults", Title = _localizer["Country Exp Delete Search Results"].ToString(), CanAddRecord = (await _authService.AuthorizeAsync(User, PatentAuthorizationPolicy.AuxiliaryModify)).Succeeded };
            return PartialView("Index", model);
        }

        [HttpGet]
        public IActionResult Search() => RedirectToAction("Index");

        public async Task<IActionResult> PageRead([DataSourceRequest] DataSourceRequest request, List<QueryFilterViewModel> mainSearchFilters)
        {
            var entities = _repository.PatCountryExpDeletes.AsNoTracking().AsQueryable();

            if (mainSearchFilters != null && mainSearchFilters.Count > 0)
            {
                entities = Helpers.QueryHelper.ApplySystemsFilter(entities, mainSearchFilters, a => a.Systems);

                foreach (var dateFilter in mainSearchFilters.Where(f =>
                    (f.Property == "EffStartDateFrom" || f.Property == "EffStartDateTo" ||
                     f.Property == "EffEndDateFrom" || f.Property == "EffEndDateTo") &&
                    !string.IsNullOrEmpty(f.Value)).ToList())
                {
                    if (DateTime.TryParse(dateFilter.Value, out var dt))
                    {
                        if (dateFilter.Property == "EffStartDateFrom")
                            entities = entities.Where(e => e.EffStartDate >= dt);
                        else if (dateFilter.Property == "EffStartDateTo")
                            entities = entities.Where(e => e.EffStartDate <= dt.AddDays(1).AddSeconds(-1));
                        else if (dateFilter.Property == "EffEndDateFrom")
                            entities = entities.Where(e => e.EffEndDate >= dt);
                        else if (dateFilter.Property == "EffEndDateTo")
                            entities = entities.Where(e => e.EffEndDate <= dt.AddDays(1).AddSeconds(-1));
                    }
                    mainSearchFilters.Remove(dateFilter);
                }
            }
            entities = entities.BuildCriteria(mainSearchFilters);
            var data = await entities.ToListAsync();
            return Json(data.ToDataSourceResult(request));
        }

        public async Task<IActionResult> Detail(int cExpId, string systems = "", bool singleRecord = false, bool fromSearch = false)
        {
            var detail = await _repository.PatCountryExpDeletes.AsNoTracking()
                .FirstOrDefaultAsync(c => c.CExpId == cExpId && c.Systems == systems);
            if (detail == null)
            {
                if (Request.IsAjax()) return new RecordDoesNotExistResult();
                return RedirectToAction("Index");
            }
            var perm = await GetPermission();
            perm.AddScreenUrl = perm.CanAddRecord ? Url.Action("Add", new { fromSearch = true }) : "";
            perm.SearchScreenUrl = Url.Action("Index");
            perm.DeleteScreenUrl = perm.CanDeleteRecord ? Url.Action("Delete", new { cExpId = cExpId, systems = systems }) : "";
            perm.CopyScreenUrl = perm.CanCopyRecord ? Url.Action("Add", new { fromSearch = true, copyCountry = detail.Country, copyCaseType = detail.CaseType, copyType = detail.Type, copyBasedOn = detail.BasedOn, copyYr = detail.Yr, copyMo = detail.Mo, copyDy = detail.Dy, copyEffBasedOn = detail.EffBasedOn, copySystems = detail.Systems }) : "";
            perm.IsCopyScreenPopup = false;
            var model = new PageViewModel
            {
                Page = PageType.Detail,
                PageId = _dataContainer,
                Title = _localizer["Country Exp Delete Detail"].ToString(),
                RecordId = 1,
                SingleRecord = singleRecord || !Request.IsAjax(),
                Data = detail,
                PagePermission = perm
            };
            if (Request.IsAjax() && !singleRecord && !fromSearch) model.Page = PageType.DetailContent;
            return Request.IsAjax() ? PartialView("Index", model) : View("Index", model);
        }

        [Authorize(Policy = PatentAuthorizationPolicy.AuxiliaryModify)]
        public async Task<IActionResult> Add(bool fromSearch = false, string copyCountry = "", string copyCaseType = "", string copyType = "", string copyBasedOn = "", int copyYr = 0, int copyMo = 0, int copyDy = 0, string copyEffBasedOn = "", string copySystems = "")
        {
            var data = new PatCountryExpDelete { IsNewRecord = true };

            if (!string.IsNullOrEmpty(copyCountry))
            {
                data.Country = copyCountry;
                data.CaseType = copyCaseType;
                data.Type = copyType;
                data.BasedOn = copyBasedOn;
                data.Yr = copyYr;
                data.Mo = copyMo;
                data.Dy = copyDy;
                data.EffBasedOn = copyEffBasedOn;
                data.Systems = copySystems ?? "";
            }

            var model = new PageViewModel
            {
                Page = fromSearch ? PageType.Detail : PageType.DetailContent,
                PageId = _dataContainer,
                Title = _localizer["New Country Exp Delete"].ToString(),
                Data = data,
                PagePermission = await GetPermission(),
                AfterCancelledInsert = $"function() {{ window.location.href = '{Url.Action("Index")}'; }}"
            };
            ModelState.Clear();
            return Request.IsAjax() ? PartialView("Index", model) : View("Index", model);
        }

        [Authorize(Policy = PatentAuthorizationPolicy.AuxiliaryModify)]
        public async Task<IActionResult> Copy(int cExpId, string systems = "")
        {
            var entity = await _repository.PatCountryExpDeletes.AsNoTracking()
                .FirstOrDefaultAsync(c => c.CExpId == cExpId && c.Systems == systems);
            if (entity == null) return new RecordDoesNotExistResult();

            return RedirectToAction("Add", new { copyCountry = entity.Country, copyCaseType = entity.CaseType, copyType = entity.Type, copyBasedOn = entity.BasedOn, copyYr = entity.Yr, copyMo = entity.Mo, copyDy = entity.Dy, copyEffBasedOn = entity.EffBasedOn, copySystems = entity.Systems });
        }

        [HttpPost, Authorize(Policy = PatentAuthorizationPolicy.AuxiliaryModify), ValidateAntiForgeryToken]
        public async Task<IActionResult> Save([FromBody] PatCountryExpDelete entity)
        {
            ModelState.Clear(); // Clear binding errors for auto-filled New fields

            entity.Systems ??= "";

            // Validate required fields
            if (string.IsNullOrWhiteSpace(entity.Country))
                ModelState.AddModelError("Country", "Country is required.");
            if (string.IsNullOrWhiteSpace(entity.CaseType))
                ModelState.AddModelError("CaseType", "Case Type is required.");
            if (string.IsNullOrWhiteSpace(entity.Type))
                ModelState.AddModelError("Type", "Type is required.");
            if (string.IsNullOrWhiteSpace(entity.BasedOn))
                ModelState.AddModelError("BasedOn", "Based On is required.");
            if (!ModelState.IsValid) return new JsonBadRequest(new { errors = ModelState.Errors() });

            // Require at least one system
            if (string.IsNullOrWhiteSpace(entity.Systems))
                return new JsonBadRequest("At least one system must be selected.");

            // Deduplicate and sort systems
            var newSystems = entity.Systems.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(s => s, StringComparer.OrdinalIgnoreCase).ToList();
            entity.Systems = string.Join(",", newSystems);

            var isNewRecord = entity.IsNewRecord || entity.OriginalSystems == "__NEW__" || entity.OriginalSystems == null;

            if (!isNewRecord)
            {
                // Update existing record
                var originalSystems = entity.OriginalSystems == "__EMPTY__" ? "" : entity.OriginalSystems;

                var existing = await _repository.PatCountryExpDeletes.AsNoTracking()
                    .FirstOrDefaultAsync(c => c.CExpId == entity.CExpId && c.Systems == originalSystems);

                if (existing != null)
                {
                    // Check for duplicate systems across records with same (Country, CaseType, Type, BasedOn, Yr, Mo, Dy)
                    var allRecords = await _repository.PatCountryExpDeletes.AsNoTracking()
                        .Where(c => c.Country == entity.Country && c.CaseType == entity.CaseType
                            && c.Type == entity.Type && c.BasedOn == entity.BasedOn
                            && c.Yr == entity.Yr && c.Mo == entity.Mo && c.Dy == entity.Dy
                            && c.Systems != originalSystems
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
                        return new JsonBadRequest($"The following systems are already assigned to another Country Exp Delete record: {string.Join(", ", duplicates)}");

                    var effStart = new Microsoft.Data.SqlClient.SqlParameter("@p8", System.Data.SqlDbType.DateTime) { Value = entity.EffStartDate.HasValue ? entity.EffStartDate.Value : DBNull.Value };
                    var effEnd = new Microsoft.Data.SqlClient.SqlParameter("@p9", System.Data.SqlDbType.DateTime) { Value = entity.EffEndDate.HasValue ? entity.EffEndDate.Value : DBNull.Value };

                    await _repository.Database.ExecuteSqlRawAsync(
                        @"UPDATE tblPatCountryExpDelete SET Country=@p0, CaseType=@p1, Type=@p2, BasedOn=@p3,
                          Yr=@p4, Mo=@p5, Dy=@p6, EffBasedOn=@p7, EffStartDate=@p8, EffEndDate=@p9,
                          Systems=@p10
                          WHERE CExpId=@p11 AND Systems=@p12",
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
                            new Microsoft.Data.SqlClient.SqlParameter("@p11", entity.CExpId),
                            new Microsoft.Data.SqlClient.SqlParameter("@p12", originalSystems ?? "")
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
                // Check for duplicate systems across records with same (Country, CaseType, Type, BasedOn, Yr, Mo, Dy)
                var dupeRecords = await _repository.PatCountryExpDeletes.AsNoTracking()
                    .Where(c => c.Country == entity.Country && c.CaseType == entity.CaseType
                        && c.Type == entity.Type && c.BasedOn == entity.BasedOn
                        && c.Yr == entity.Yr && c.Mo == entity.Mo && c.Dy == entity.Dy
                        && c.Systems != null && c.Systems != "")
                    .Select(c => c.Systems)
                    .ToListAsync();
                var usedSys = dupeRecords
                    .SelectMany(s => s.Split(',', StringSplitOptions.RemoveEmptyEntries))
                    .Select(s => s.Trim())
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);
                var dupes = newSystems.Where(s => usedSys.Contains(s)).ToList();
                if (dupes.Any())
                    return new JsonBadRequest($"System(s) '{string.Join(", ", dupes)}' already assigned to this combination.");
                var effStart = new Microsoft.Data.SqlClient.SqlParameter("@p8", System.Data.SqlDbType.DateTime) { Value = entity.EffStartDate.HasValue ? entity.EffStartDate.Value : DBNull.Value };
                var effEnd = new Microsoft.Data.SqlClient.SqlParameter("@p9", System.Data.SqlDbType.DateTime) { Value = entity.EffEndDate.HasValue ? entity.EffEndDate.Value : DBNull.Value };

                await _repository.Database.ExecuteSqlRawAsync(
                    @"INSERT INTO tblPatCountryExpDelete (CExpId, Country, CaseType, Type, BasedOn,
                      Yr, Mo, Dy, EffBasedOn, EffStartDate, EffEndDate, Systems)
                      VALUES ((SELECT ISNULL(MAX(CExpId),0)+1 FROM tblPatCountryExpDelete), @p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9, @p10)",
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

                // Retrieve the newly inserted record
                var inserted = await _repository.PatCountryExpDeletes.AsNoTracking()
                    .Where(c => c.Country == entity.Country && c.CaseType == entity.CaseType
                        && c.Type == entity.Type && c.Systems == entity.Systems)
                    .OrderByDescending(c => c.CExpId)
                    .FirstOrDefaultAsync();
                if (inserted != null)
                    entity.CExpId = inserted.CExpId;
            }

            return Json(new { id = 0, redirectUrl = Url.Action("Detail", new { cExpId = entity.CExpId, systems = entity.Systems, singleRecord = true }) });
        }

        [HttpPost, Authorize(Policy = PatentAuthorizationPolicy.AuxiliaryCanDelete)]
        public async Task<IActionResult> Delete(int cExpId, string systems = "")
        {
            var count = await _repository.Database.ExecuteSqlRawAsync(
                "DELETE FROM tblPatCountryExpDelete WHERE CExpId=@p0 AND Systems=@p1",
                new object[] {
                    new Microsoft.Data.SqlClient.SqlParameter("@p0", cExpId),
                    new Microsoft.Data.SqlClient.SqlParameter("@p1", systems ?? "")
                });

            if (count == 0)
                return new RecordDoesNotExistResult();

            return Ok();
        }

        public IActionResult GetSystemList()
        {
            return Json(Helpers.SystemsHelper.SystemNames);
        }

        public async Task<IActionResult> GetPicklistData([DataSourceRequest] DataSourceRequest request, string property, string text = "", FilterType filterType = FilterType.StartsWith, string requiredRelation = "")
        {
            return await GetPicklistData(_repository.PatCountryExpDeletes.AsNoTracking(), request, property, text, filterType, requiredRelation);
        }

        public async Task<IActionResult> GetBasedOnList(string property = "BasedOn", string text = "", FilterType filterType = FilterType.Contains)
        {
            var query = _repository.PatCountryExpDeletes.AsNoTracking()
                .Where(c => c.BasedOn != null && c.BasedOn != "")
                .Select(c => new { BasedOn = c.BasedOn })
                .Distinct();
            if (!string.IsNullOrEmpty(text))
                query = query.Where(c => EF.Functions.Like(c.BasedOn, $"%{text}%"));
            return Json(await query.OrderBy(c => c.BasedOn).ToListAsync());
        }

        public async Task<IActionResult> GetTypeList(string property = "Type", string text = "", FilterType filterType = FilterType.Contains)
        {
            var query = _repository.PatCountryExpDeletes.AsNoTracking()
                .Where(c => c.Type != null && c.Type != "")
                .Select(c => new { Type = c.Type })
                .Distinct();
            if (!string.IsNullOrEmpty(text))
                query = query.Where(c => EF.Functions.Like(c.Type, $"%{text}%"));
            return Json(await query.OrderBy(c => c.Type).ToListAsync());
        }

        public IActionResult GetRecordStamps(int cExpId = 0, string systems = "")
        {
            return ViewComponent("RecordStamps", new { createdBy = "", dateCreated = (DateTime?)null, updatedBy = "", lastUpdate = (DateTime?)null });
        }

        [HttpGet]
        public IActionResult DetailLink(int cExpId = 0, string systems = "")
        {
            if (cExpId > 0)
                return RedirectToAction(nameof(Detail), new { cExpId = cExpId, systems = systems, singleRecord = true, fromSearch = true });
            return RedirectToAction(nameof(Add), new { fromSearch = true });
        }

        [HttpPost]
        public IActionResult ExcelExportSave(string contentType, string base64, string fileName)
        {
            return File(Convert.FromBase64String(base64), contentType, fileName);
        }
    }
}
