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
using LawPortal.Core.Helpers;
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
    public class AreaDeleteController : BaseController
    {
        private readonly IAuthorizationService _authService;
        private readonly IApplicationDbContext _repository;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly string _dataContainer = "patAreaDeleteDetail";

        public AreaDeleteController(IAuthorizationService authService, IApplicationDbContext repository, IStringLocalizer<SharedResource> localizer)
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
            var model = new PageViewModel { Page = PageType.Search, PageId = "patAreaDeleteSearch", Title = _localizer["Area Delete Search"].ToString(), CanAddRecord = (await _authService.AuthorizeAsync(User, PatentAuthorizationPolicy.AuxiliaryModify)).Succeeded };
            return Request.IsAjax() ? PartialView("Index", model) : View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Search([FromBody] List<QueryFilterViewModel> mainSearchFilters)
        {
            var model = new PageViewModel { Page = PageType.SearchResults, PageId = "patAreaDeleteSearchResults", Title = _localizer["Area Delete Search Results"].ToString(), CanAddRecord = (await _authService.AuthorizeAsync(User, PatentAuthorizationPolicy.AuxiliaryModify)).Succeeded };
            return PartialView("Index", model);
        }

        [HttpGet]
        public IActionResult Search() => RedirectToAction("Index");

        public async Task<IActionResult> PageRead([DataSourceRequest] DataSourceRequest request, List<QueryFilterViewModel> mainSearchFilters)
        {
            var entities = _repository.PatAreaDeletes.AsNoTracking().AsQueryable();

            if (mainSearchFilters != null && mainSearchFilters.Count > 0)
            {
                {
                    entities = Helpers.QueryHelper.ApplySystemsFilter(entities, mainSearchFilters, a => a.Systems);
                }
            }
            entities = entities.BuildCriteria(mainSearchFilters);
            var data = await entities.ToListAsync();
            return Json(data.ToDataSourceResult(request));
        }

        public async Task<IActionResult> Detail(string areaCode, string areaNewCode = "", string systems = "", bool singleRecord = false, bool fromSearch = false)
        {
            var detail = await _repository.PatAreaDeletes.AsNoTracking()
                .FirstOrDefaultAsync(c => c.Area == areaCode && c.AreaNew == areaNewCode && c.Systems == systems);
            if (detail == null)
            {
                if (Request.IsAjax()) return new RecordDoesNotExistResult();
                return RedirectToAction("Index");
            }
            var perm = await GetPermission();
            perm.AddScreenUrl = perm.CanAddRecord ? Url.Action("Add", new { fromSearch = true }) : "";
            perm.SearchScreenUrl = Url.Action("Index");
            perm.DeleteScreenUrl = perm.CanDeleteRecord ? Url.Action("Delete", new { areaCode = areaCode, areaNewCode = areaNewCode, systems = systems }) : "";
            perm.CopyScreenUrl = perm.CanCopyRecord ? Url.Action("Add", new { fromSearch = true, copyArea = areaCode, copyDescription = detail.Description, copyAreaNew = areaNewCode, copySystems = detail.Systems }) : "";
            perm.IsCopyScreenPopup = false;
            var model = new PageViewModel
            {
                Page = PageType.Detail,
                PageId = _dataContainer,
                Title = _localizer["Area Delete Detail"].ToString(),
                RecordId = 1,
                SingleRecord = singleRecord || !Request.IsAjax(),
                Data = detail,
                PagePermission = perm
            };
            if (Request.IsAjax() && !singleRecord && !fromSearch) model.Page = PageType.DetailContent;
            return Request.IsAjax() ? PartialView("Index", model) : View("Index", model);
        }

        [Authorize(Policy = PatentAuthorizationPolicy.AuxiliaryModify)]
        public async Task<IActionResult> Add(bool fromSearch = false, string copyArea = "", string copyDescription = "", string copyAreaNew = "", string copySystems = "")
        {
            var data = new PatAreaDelete { IsNewRecord = true };

            if (!string.IsNullOrEmpty(copyArea))
            {
                data.Area = copyArea;
                data.Description = copyDescription;
                data.AreaNew = copyAreaNew;
                data.Systems = copySystems ?? "";
            }

            var model = new PageViewModel
            {
                Page = fromSearch ? PageType.Detail : PageType.DetailContent,
                PageId = _dataContainer,
                Title = _localizer["New Area Delete"].ToString(),
                Data = data,
                PagePermission = await GetPermission(),
                AfterCancelledInsert = $"function() {{ window.location.href = '{Url.Action("Index")}'; }}"
            };
            ModelState.Clear();
            return Request.IsAjax() ? PartialView("Index", model) : View("Index", model);
        }

        [HttpPost, Authorize(Policy = PatentAuthorizationPolicy.AuxiliaryModify), ValidateAntiForgeryToken]
        public async Task<IActionResult> Save([FromBody] PatAreaDelete entity)
        {
            ModelState.Clear(); // Clear binding errors for auto-filled New fields

            var userName = User.GetUserName();
            var now = DateTime.Now;
            entity.UserID = userName;
            entity.LastUpdate = now;
            entity.Description ??= "";
            entity.Systems ??= "";
            entity.AreaNew = entity.Area ?? "";

            // Validate required fields
            if (string.IsNullOrWhiteSpace(entity.Area))
                ModelState.AddModelError("Area", "Area is required.");
            if (!ModelState.IsValid) return new JsonBadRequest(new { errors = ModelState.Errors() });

            // Require at least one system
            if (string.IsNullOrWhiteSpace(entity.Systems))
                return new JsonBadRequest("At least one system must be selected.");

            // Deduplicate and sort systems
            var newSystems = entity.Systems.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(s => s, StringComparer.OrdinalIgnoreCase).ToList();
            entity.Systems = string.Join(",", newSystems);

            var isNewRecord = entity.IsNewRecord || entity.OriginalSystems == "__NEW__" || entity.OriginalSystems == null;
            var originalSystemsValue = entity.OriginalSystems == "__EMPTY__" ? "" : (entity.OriginalSystems ?? "");

            if (!isNewRecord)
            {
                // Update existing record
                var existing = await _repository.PatAreaDeletes.AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Area == entity.Area && c.Systems == originalSystemsValue);

                if (existing != null)
                {
                    entity.DateCreated = existing.DateCreated ?? now;

                    // Check for duplicate systems across records with the same (Area, AreaNew)
                    var allRecords = await _repository.PatAreaDeletes.AsNoTracking()
                        .Where(c => c.Area == entity.Area
                            && c.Systems != originalSystemsValue
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
                        return new JsonBadRequest($"The following systems are already assigned to another Area Delete record: {string.Join(", ", duplicates)}");

                    await _repository.Database.ExecuteSqlRawAsync(
                        @"UPDATE tblPatAreaDelete SET Area=@p0, Description=@p1, AreaNew=@p2, Systems=@p3, UserID=@p4, DateCreated=@p5, LastUpdate=@p6
                          WHERE Area=@p7 AND Systems=@p9",
                        entity.Area ?? "", entity.Description ?? "", entity.AreaNew ?? "", entity.Systems,
                        entity.UserID ?? "", entity.DateCreated, entity.LastUpdate,
                        existing.Area ?? "", existing.AreaNew ?? "", existing.Systems ?? "");
                }
                else
                {
                    return new RecordDoesNotExistResult();
                }
            }
            else
            {
                entity.DateCreated = now;

                // Check for duplicate systems across records with the same (Area, AreaNew)
                var allRecords = await _repository.PatAreaDeletes.AsNoTracking()
                    .Where(c => c.Area == entity.Area
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
                    return new JsonBadRequest($"The following systems are already assigned to another Area Delete record: {string.Join(", ", duplicates)}");

                await _repository.Database.ExecuteSqlRawAsync(
                    @"INSERT INTO tblPatAreaDelete (Area, Description, AreaNew, Systems, UserID, DateCreated, LastUpdate)
                      VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6)",
                    entity.Area ?? "", entity.Description ?? "", entity.AreaNew ?? "", entity.Systems,
                    entity.UserID ?? "", entity.DateCreated, entity.LastUpdate);
            }

            return Json(new { id = 0, redirectUrl = Url.Action("Detail", new { areaCode = entity.Area, areaNewCode = entity.AreaNew, systems = entity.Systems, singleRecord = true }) });
        }

        [HttpPost, Authorize(Policy = PatentAuthorizationPolicy.AuxiliaryCanDelete)]
        public async Task<IActionResult> Delete(string areaCode = "", string areaNewCode = "", string systems = "")
        {
            var count = await _repository.Database.ExecuteSqlRawAsync(
                "DELETE FROM tblPatAreaDelete WHERE Area=@p0 AND Systems=@p2",
                new object[] {
                    new Microsoft.Data.SqlClient.SqlParameter("@p0", areaCode ?? ""),
                    new Microsoft.Data.SqlClient.SqlParameter("@p1", areaNewCode ?? ""),
                    new Microsoft.Data.SqlClient.SqlParameter("@p2", systems ?? "")
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
            return await GetPicklistData(_repository.PatAreaDeletes.AsNoTracking(), request, property, text, filterType, requiredRelation);
        }

        public IActionResult GetRecordStamps(string areaCode = "", string areaNewCode = "", string systems = "")
        {
            return ViewComponent("RecordStamps", new { createdBy = "", dateCreated = (DateTime?)null, updatedBy = "", lastUpdate = (DateTime?)null });
        }

        [HttpGet]
        public IActionResult DetailLink(string areaCode = "", string areaNewCode = "", string systems = "")
        {
            if (!string.IsNullOrEmpty(areaCode))
                return RedirectToAction(nameof(Detail), new { areaCode = areaCode, areaNewCode = areaNewCode, systems = systems, singleRecord = true, fromSearch = true });
            return RedirectToAction(nameof(Add), new { fromSearch = true });
        }

        [HttpPost]
        public IActionResult ExcelExportSave(string contentType, string base64, string fileName)
        {
            return File(Convert.FromBase64String(base64), contentType, fileName);
        }
    }
}
