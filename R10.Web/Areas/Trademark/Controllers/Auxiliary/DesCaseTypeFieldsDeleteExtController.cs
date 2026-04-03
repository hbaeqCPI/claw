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
using R10.Core.Entities.Trademark;
using R10.Core.Interfaces;
using R10.Web.Extensions;
using R10.Web.Extensions.ActionResults;
using R10.Web.Helpers;
using R10.Web.Models;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Models.PageViewModels;
using R10.Web.Security;

namespace R10.Web.Areas.Trademark.Controllers
{
    [Area("Trademark"), Authorize(Policy = TrademarkAuthorizationPolicy.CanAccessAuxiliary)]
    public class DesCaseTypeFieldsDeleteExtController : BaseController
    {
        private readonly IAuthorizationService _authService;
        private readonly IApplicationDbContext _repository;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly string _dataContainer = "tmkDesCaseTypeFieldsDeleteExtDetail";

        public DesCaseTypeFieldsDeleteExtController(IAuthorizationService authService, IApplicationDbContext repository, IStringLocalizer<SharedResource> localizer)
        {
            _authService = authService;
            _repository = repository;
            _localizer = localizer;
        }

        private async Task<DetailPagePermission> GetPermission()
        {
            var p = new DetailPagePermission();
            p.CanEditRecord = (await _authService.AuthorizeAsync(User, TrademarkAuthorizationPolicy.AuxiliaryModify)).Succeeded;
            p.CanDeleteRecord = (await _authService.AuthorizeAsync(User, TrademarkAuthorizationPolicy.AuxiliaryCanDelete)).Succeeded;
            p.CanAddRecord = p.CanEditRecord;
            p.CanCopyRecord = p.CanEditRecord;
            return p;
        }

        public async Task<IActionResult> Index()
        {
            var model = new PageViewModel { Page = PageType.Search, PageId = "tmkDesCaseTypeFieldsDeleteExtSearch", Title = _localizer["Des Case Type Fields Delete Ext Search"].ToString(), CanAddRecord = (await _authService.AuthorizeAsync(User, TrademarkAuthorizationPolicy.AuxiliaryModify)).Succeeded };
            return Request.IsAjax() ? PartialView("Index", model) : View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Search([FromBody] List<QueryFilterViewModel> mainSearchFilters)
        {
            var model = new PageViewModel { Page = PageType.SearchResults, PageId = "tmkDesCaseTypeFieldsDeleteExtSearchResults", Title = _localizer["Des Case Type Fields Delete Ext Search Results"].ToString(), CanAddRecord = (await _authService.AuthorizeAsync(User, TrademarkAuthorizationPolicy.AuxiliaryModify)).Succeeded };
            return PartialView("Index", model);
        }

        [HttpGet]
        public IActionResult Search() => RedirectToAction("Index");

        public async Task<IActionResult> PageRead([DataSourceRequest] DataSourceRequest request, List<QueryFilterViewModel> mainSearchFilters)
        {
            var entities = _repository.TmkDesCaseTypeFieldsDeleteExts.AsNoTracking().AsQueryable();

            if (mainSearchFilters != null && mainSearchFilters.Count > 0)
            {
                var systemName = mainSearchFilters.FirstOrDefault(f => f.Property == "SystemName");
                if (systemName != null)
                {
                    entities = entities.Where(a => a.Systems != null && EF.Functions.Like(a.Systems, "%" + systemName.Value.Replace("%", "") + "%"));
                    mainSearchFilters.Remove(systemName);
                }

                foreach (var filter in mainSearchFilters)
                {
                    if (filter.Property == "DesCaseType" && !string.IsNullOrEmpty(filter.Value))
                        entities = entities.Where(a => EF.Functions.Like(a.DesCaseType, filter.Value));
                    else if (filter.Property == "FromField" && !string.IsNullOrEmpty(filter.Value))
                        entities = entities.Where(a => EF.Functions.Like(a.FromField, filter.Value));
                    else if (filter.Property == "ToField" && !string.IsNullOrEmpty(filter.Value))
                        entities = entities.Where(a => EF.Functions.Like(a.ToField, filter.Value));
                }
            }

            var data = await entities.ToListAsync();
            return Json(data.ToDataSourceResult(request));
        }

        public async Task<IActionResult> Detail(string desCaseType, string fromField, string toField, string desCaseTypeNew = "", string fromFieldNew = "", string toFieldNew = "", string systems = "", bool singleRecord = false, bool fromSearch = false)
        {
            var detail = await _repository.TmkDesCaseTypeFieldsDeleteExts.AsNoTracking()
                .FirstOrDefaultAsync(c => c.DesCaseType == desCaseType && c.FromField == fromField && c.ToField == toField
                    && c.DesCaseTypeNew == desCaseTypeNew && c.FromFieldNew == fromFieldNew && c.ToFieldNew == toFieldNew && c.Systems == systems);
            if (detail == null)
            {
                if (Request.IsAjax()) return new RecordDoesNotExistResult();
                return RedirectToAction("Index");
            }
            var perm = await GetPermission();
            perm.AddScreenUrl = perm.CanAddRecord ? Url.Action("Add", new { fromSearch = true }) : "";
            perm.SearchScreenUrl = Url.Action("Index");
            perm.DeleteScreenUrl = perm.CanDeleteRecord ? Url.Action("Delete", new { desCaseType, fromField, toField, desCaseTypeNew, fromFieldNew, toFieldNew, systems }) : "";
            perm.CopyScreenUrl = perm.CanCopyRecord ? Url.Action("Add", new { fromSearch = true, copyDesCaseType = detail.DesCaseType, copyFromField = detail.FromField, copyToField = detail.ToField, copyDesCaseTypeNew = detail.DesCaseTypeNew, copyFromFieldNew = detail.FromFieldNew, copyToFieldNew = detail.ToFieldNew, copySystems = detail.Systems }) : "";
            perm.IsCopyScreenPopup = false;
            var model = new PageViewModel
            {
                Page = PageType.Detail,
                PageId = _dataContainer,
                Title = _localizer["Des Case Type Fields Delete Ext Detail"].ToString(),
                RecordId = 1,
                SingleRecord = singleRecord || !Request.IsAjax(),
                Data = detail,
                PagePermission = perm
            };
            if (Request.IsAjax() && !singleRecord && !fromSearch) model.Page = PageType.DetailContent;
            return Request.IsAjax() ? PartialView("Index", model) : View("Index", model);
        }

        [Authorize(Policy = TrademarkAuthorizationPolicy.AuxiliaryModify)]
        public async Task<IActionResult> Add(bool fromSearch = false, string copyDesCaseType = "", string copyFromField = "", string copyToField = "", string copyDesCaseTypeNew = "", string copyFromFieldNew = "", string copyToFieldNew = "", string copySystems = "")
        {
            var data = new TmkDesCaseTypeFieldsDeleteExt { IsNewRecord = true };

            if (!string.IsNullOrEmpty(copyDesCaseType) || !string.IsNullOrEmpty(copyFromField) || !string.IsNullOrEmpty(copyToField))
            {
                data.DesCaseType = copyDesCaseType;
                data.FromField = copyFromField;
                data.ToField = copyToField;
                data.DesCaseTypeNew = copyDesCaseTypeNew;
                data.FromFieldNew = copyFromFieldNew;
                data.ToFieldNew = copyToFieldNew;
                data.Systems = copySystems ?? "";
            }

            var model = new PageViewModel
            {
                Page = fromSearch ? PageType.Detail : PageType.DetailContent,
                PageId = _dataContainer,
                Title = _localizer["New Des Case Type Fields Delete Ext"].ToString(),
                Data = data,
                PagePermission = await GetPermission(),
                AfterCancelledInsert = $"function() {{ window.location.href = '{Url.Action("Index")}'; }}"
            };
            ModelState.Clear();
            return Request.IsAjax() ? PartialView("Index", model) : View("Index", model);
        }

        [HttpPost, Authorize(Policy = TrademarkAuthorizationPolicy.AuxiliaryModify), ValidateAntiForgeryToken]
        public async Task<IActionResult> Save([FromBody] TmkDesCaseTypeFieldsDeleteExt entity)
        {
            ModelState.Clear(); // Clear binding errors for auto-filled New fields

            entity.Systems ??= "";
            entity.DesCaseTypeNew = entity.DesCaseType ?? "";
            entity.FromFieldNew = entity.FromField ?? "";
            entity.ToFieldNew = entity.ToField ?? "";

            // Validate required fields
            if (string.IsNullOrWhiteSpace(entity.DesCaseType))
                ModelState.AddModelError("DesCaseType", "Des Case Type is required.");
            if (string.IsNullOrWhiteSpace(entity.FromField))
                ModelState.AddModelError("FromField", "From Field is required.");
            if (string.IsNullOrWhiteSpace(entity.ToField))
                ModelState.AddModelError("ToField", "To Field is required.");
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
                var existing = await _repository.TmkDesCaseTypeFieldsDeleteExts.AsNoTracking()
                    .FirstOrDefaultAsync(c => c.DesCaseType == entity.DesCaseType && c.FromField == entity.FromField && c.ToField == entity.ToField
                        && c.Systems == originalSystemsValue);

                if (existing != null)
                {
                    await _repository.Database.ExecuteSqlRawAsync(
                        @"UPDATE tblTmkDesCaseTypeFieldsDelete_Ext SET DesCaseType=@p0, FromField=@p1, ToField=@p2,
                          DesCaseTypeNew=@p3, FromFieldNew=@p4, ToFieldNew=@p5, Systems=@p6
                          WHERE DesCaseType=@p7 AND FromField=@p8 AND ToField=@p9
                          AND Systems=@p13",
                        new object[] {
                            new Microsoft.Data.SqlClient.SqlParameter("@p0", entity.DesCaseType ?? ""),
                            new Microsoft.Data.SqlClient.SqlParameter("@p1", entity.FromField ?? ""),
                            new Microsoft.Data.SqlClient.SqlParameter("@p2", entity.ToField ?? ""),
                            new Microsoft.Data.SqlClient.SqlParameter("@p3", entity.DesCaseTypeNew ?? ""),
                            new Microsoft.Data.SqlClient.SqlParameter("@p4", entity.FromFieldNew ?? ""),
                            new Microsoft.Data.SqlClient.SqlParameter("@p5", entity.ToFieldNew ?? ""),
                            new Microsoft.Data.SqlClient.SqlParameter("@p6", entity.Systems),
                            new Microsoft.Data.SqlClient.SqlParameter("@p7", existing.DesCaseType ?? ""),
                            new Microsoft.Data.SqlClient.SqlParameter("@p8", existing.FromField ?? ""),
                            new Microsoft.Data.SqlClient.SqlParameter("@p9", existing.ToField ?? ""),
                            new Microsoft.Data.SqlClient.SqlParameter("@p10", existing.DesCaseTypeNew ?? ""),
                            new Microsoft.Data.SqlClient.SqlParameter("@p11", existing.FromFieldNew ?? ""),
                            new Microsoft.Data.SqlClient.SqlParameter("@p12", existing.ToFieldNew ?? ""),
                            new Microsoft.Data.SqlClient.SqlParameter("@p13", existing.Systems ?? "")
                        });
                }
                else
                {
                    return new RecordDoesNotExistResult();
                }
            }
            else
            {
                await _repository.Database.ExecuteSqlRawAsync(
                    @"INSERT INTO tblTmkDesCaseTypeFieldsDelete_Ext (DesCaseType, FromField, ToField, DesCaseTypeNew, FromFieldNew, ToFieldNew, Systems)
                      VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6)",
                    new object[] {
                        new Microsoft.Data.SqlClient.SqlParameter("@p0", entity.DesCaseType ?? ""),
                        new Microsoft.Data.SqlClient.SqlParameter("@p1", entity.FromField ?? ""),
                        new Microsoft.Data.SqlClient.SqlParameter("@p2", entity.ToField ?? ""),
                        new Microsoft.Data.SqlClient.SqlParameter("@p3", entity.DesCaseTypeNew ?? ""),
                        new Microsoft.Data.SqlClient.SqlParameter("@p4", entity.FromFieldNew ?? ""),
                        new Microsoft.Data.SqlClient.SqlParameter("@p5", entity.ToFieldNew ?? ""),
                        new Microsoft.Data.SqlClient.SqlParameter("@p6", entity.Systems)
                    });
            }

            return Json(new { id = 0, redirectUrl = Url.Action("Detail", new { desCaseType = entity.DesCaseType, fromField = entity.FromField, toField = entity.ToField, desCaseTypeNew = entity.DesCaseTypeNew, fromFieldNew = entity.FromFieldNew, toFieldNew = entity.ToFieldNew, systems = entity.Systems, singleRecord = true }) });
        }

        [HttpPost, Authorize(Policy = TrademarkAuthorizationPolicy.AuxiliaryCanDelete)]
        public async Task<IActionResult> Delete(string desCaseType = "", string fromField = "", string toField = "", string desCaseTypeNew = "", string fromFieldNew = "", string toFieldNew = "", string systems = "")
        {
            var count = await _repository.Database.ExecuteSqlRawAsync(
                "DELETE FROM tblTmkDesCaseTypeFieldsDelete_Ext WHERE DesCaseType=@p0 AND FromField=@p1 AND ToField=@p2 AND Systems=@p6",
                new object[] {
                    new Microsoft.Data.SqlClient.SqlParameter("@p0", desCaseType ?? ""),
                    new Microsoft.Data.SqlClient.SqlParameter("@p1", fromField ?? ""),
                    new Microsoft.Data.SqlClient.SqlParameter("@p2", toField ?? ""),
                    new Microsoft.Data.SqlClient.SqlParameter("@p3", desCaseTypeNew ?? ""),
                    new Microsoft.Data.SqlClient.SqlParameter("@p4", fromFieldNew ?? ""),
                    new Microsoft.Data.SqlClient.SqlParameter("@p5", toFieldNew ?? ""),
                    new Microsoft.Data.SqlClient.SqlParameter("@p6", systems ?? "")
                });

            if (count == 0)
                return new RecordDoesNotExistResult();

            return Ok();
        }

        public async Task<IActionResult> GetSystemList()
        {
            var systems = (await _repository.AppSystems.AsNoTracking()
                .Select(s => s.SystemName)
                .ToListAsync())
                .OrderBy(s => s, StringComparer.OrdinalIgnoreCase).ThenBy(s => s.Length).ToList();
            return Json(systems);
        }

        public IActionResult GetRecordStamps(string desCaseType = "", string fromField = "", string toField = "", string desCaseTypeNew = "", string fromFieldNew = "", string toFieldNew = "", string systems = "")
        {
            return ViewComponent("RecordStamps", new { createdBy = "", dateCreated = (DateTime?)null, updatedBy = "", lastUpdate = (DateTime?)null });
        }

        [HttpGet]
        public IActionResult DetailLink(string desCaseType = "", string fromField = "", string toField = "", string desCaseTypeNew = "", string fromFieldNew = "", string toFieldNew = "", string systems = "")
        {
            if (!string.IsNullOrEmpty(desCaseType) || !string.IsNullOrEmpty(fromField) || !string.IsNullOrEmpty(toField))
                return RedirectToAction(nameof(Detail), new { desCaseType, fromField, toField, desCaseTypeNew, fromFieldNew, toFieldNew, systems, singleRecord = true, fromSearch = true });
            return RedirectToAction(nameof(Add), new { fromSearch = true });
        }

        [HttpPost]
        public IActionResult ExcelExportSave(string contentType, string base64, string fileName)
        {
            return File(Convert.FromBase64String(base64), contentType, fileName);
        }
    }
}
