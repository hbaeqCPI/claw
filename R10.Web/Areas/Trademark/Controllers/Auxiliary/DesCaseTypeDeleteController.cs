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
    public class DesCaseTypeDeleteController : BaseController
    {
        private readonly IAuthorizationService _authService;
        private readonly IApplicationDbContext _repository;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly string _dataContainer = "tmkDesCaseTypeDeleteDetail";

        public DesCaseTypeDeleteController(IAuthorizationService authService, IApplicationDbContext repository, IStringLocalizer<SharedResource> localizer)
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
            var model = new PageViewModel { Page = PageType.Search, PageId = "tmkDesCaseTypeDeleteSearch", Title = _localizer["Des Case Type Delete Search"].ToString(), CanAddRecord = (await _authService.AuthorizeAsync(User, TrademarkAuthorizationPolicy.AuxiliaryModify)).Succeeded };
            return Request.IsAjax() ? PartialView("Index", model) : View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Search([FromBody] List<QueryFilterViewModel> mainSearchFilters)
        {
            var model = new PageViewModel { Page = PageType.SearchResults, PageId = "tmkDesCaseTypeDeleteSearchResults", Title = _localizer["Des Case Type Delete Search Results"].ToString(), CanAddRecord = (await _authService.AuthorizeAsync(User, TrademarkAuthorizationPolicy.AuxiliaryModify)).Succeeded };
            return PartialView("Index", model);
        }

        [HttpGet]
        public IActionResult Search() => RedirectToAction("Index");

        public async Task<IActionResult> PageRead([DataSourceRequest] DataSourceRequest request, List<QueryFilterViewModel> mainSearchFilters)
        {
            var entities = _repository.TmkDesCaseTypeDeletes.AsNoTracking().AsQueryable();

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
                    if (filter.Property == "IntlCode" && !string.IsNullOrEmpty(filter.Value))
                        entities = entities.Where(a => a.IntlCode == filter.Value);
                    else if (filter.Property == "CaseType" && !string.IsNullOrEmpty(filter.Value))
                        entities = entities.Where(a => a.CaseType == filter.Value);
                    else if (filter.Property == "DesCountry" && !string.IsNullOrEmpty(filter.Value))
                        entities = entities.Where(a => a.DesCountry == filter.Value);
                    else if (filter.Property == "DesCaseType" && !string.IsNullOrEmpty(filter.Value))
                        entities = entities.Where(a => a.DesCaseType == filter.Value);
                }
            }

            var data = await entities.ToListAsync();
            return Json(data.ToDataSourceResult(request));
        }

        public async Task<IActionResult> Detail(string intlCode, string caseType, string desCountry, string desCaseType, string intlCodeNew = "", string caseTypeNew = "", string desCountryNew = "", string desCaseTypeNew = "", string systems = "", bool singleRecord = false, bool fromSearch = false)
        {
            var detail = await _repository.TmkDesCaseTypeDeletes.AsNoTracking()
                .FirstOrDefaultAsync(c => c.IntlCode == intlCode && c.CaseType == caseType && c.DesCountry == desCountry && c.DesCaseType == desCaseType
                    && c.IntlCodeNew == intlCodeNew && c.CaseTypeNew == caseTypeNew && c.DesCountryNew == desCountryNew && c.DesCaseTypeNew == desCaseTypeNew && c.Systems == systems);
            if (detail == null)
            {
                if (Request.IsAjax()) return new RecordDoesNotExistResult();
                return RedirectToAction("Index");
            }
            var perm = await GetPermission();
            perm.AddScreenUrl = perm.CanAddRecord ? Url.Action("Add", new { fromSearch = true }) : "";
            perm.SearchScreenUrl = Url.Action("Index");
            perm.DeleteScreenUrl = perm.CanDeleteRecord ? Url.Action("Delete", new { intlCode, caseType, desCountry, desCaseType, intlCodeNew, caseTypeNew, desCountryNew, desCaseTypeNew, systems }) : "";
            perm.CopyScreenUrl = perm.CanCopyRecord ? Url.Action("Add", new { fromSearch = true, copyIntlCode = detail.IntlCode, copyCaseType = detail.CaseType, copyDesCountry = detail.DesCountry, copyDesCaseType = detail.DesCaseType, copyDefault = detail.Default, copyIntlCodeNew = detail.IntlCodeNew, copyCaseTypeNew = detail.CaseTypeNew, copyDesCountryNew = detail.DesCountryNew, copyDesCaseTypeNew = detail.DesCaseTypeNew, copySystems = detail.Systems }) : "";
            perm.IsCopyScreenPopup = false;
            var model = new PageViewModel
            {
                Page = PageType.Detail,
                PageId = _dataContainer,
                Title = _localizer["Des Case Type Delete Detail"].ToString(),
                RecordId = 1,
                SingleRecord = singleRecord || !Request.IsAjax(),
                Data = detail,
                PagePermission = perm
            };
            if (Request.IsAjax() && !singleRecord && !fromSearch) model.Page = PageType.DetailContent;
            return Request.IsAjax() ? PartialView("Index", model) : View("Index", model);
        }

        [Authorize(Policy = TrademarkAuthorizationPolicy.AuxiliaryModify)]
        public async Task<IActionResult> Add(bool fromSearch = false, string copyIntlCode = "", string copyCaseType = "", string copyDesCountry = "", string copyDesCaseType = "", bool copyDefault = false, string copyIntlCodeNew = "", string copyCaseTypeNew = "", string copyDesCountryNew = "", string copyDesCaseTypeNew = "", string copySystems = "")
        {
            if (!Request.IsAjax()) return RedirectToAction("Index");
            var data = new TmkDesCaseTypeDelete { IsNewRecord = true };

            if (!string.IsNullOrEmpty(copyIntlCode) || !string.IsNullOrEmpty(copyCaseType) || !string.IsNullOrEmpty(copyDesCountry) || !string.IsNullOrEmpty(copyDesCaseType))
            {
                data.IntlCode = copyIntlCode;
                data.CaseType = copyCaseType;
                data.DesCountry = copyDesCountry;
                data.DesCaseType = copyDesCaseType;
                data.Default = copyDefault;
                data.IntlCodeNew = copyIntlCodeNew;
                data.CaseTypeNew = copyCaseTypeNew;
                data.DesCountryNew = copyDesCountryNew;
                data.DesCaseTypeNew = copyDesCaseTypeNew;
                data.Systems = copySystems ?? "";
            }

            var model = new PageViewModel
            {
                Page = fromSearch ? PageType.Detail : PageType.DetailContent,
                PageId = _dataContainer,
                Title = _localizer["New Des Case Type Delete"].ToString(),
                Data = data,
                PagePermission = await GetPermission(),
                AfterCancelledInsert = $"function() {{ window.location.href = '{Url.Action("Index")}'; }}"
            };
            ModelState.Clear();
            return PartialView("Index", model);
        }

        [HttpPost, Authorize(Policy = TrademarkAuthorizationPolicy.AuxiliaryModify), ValidateAntiForgeryToken]
        public async Task<IActionResult> Save([FromBody] TmkDesCaseTypeDelete entity)
        {
            ModelState.Clear(); // Clear binding errors for auto-filled New fields

            entity.Systems ??= "";
            entity.IntlCodeNew = entity.IntlCode ?? "";
            entity.CaseTypeNew = entity.CaseType ?? "";
            entity.DesCountryNew = entity.DesCountry ?? "";
            entity.DesCaseTypeNew = entity.DesCaseType ?? "";

            // Validate required fields
            if (string.IsNullOrWhiteSpace(entity.IntlCode))
                ModelState.AddModelError("IntlCode", "Intl Code is required.");
            if (string.IsNullOrWhiteSpace(entity.CaseType))
                ModelState.AddModelError("CaseType", "Case Type is required.");
            if (string.IsNullOrWhiteSpace(entity.DesCountry))
                ModelState.AddModelError("DesCountry", "Des Country is required.");
            if (string.IsNullOrWhiteSpace(entity.DesCaseType))
                ModelState.AddModelError("DesCaseType", "Des Case Type is required.");
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
                var existing = await _repository.TmkDesCaseTypeDeletes.AsNoTracking()
                    .FirstOrDefaultAsync(c => c.IntlCode == entity.IntlCode && c.CaseType == entity.CaseType && c.DesCountry == entity.DesCountry && c.DesCaseType == entity.DesCaseType
                        && c.Systems == originalSystemsValue);

                if (existing != null)
                {
                    await _repository.Database.ExecuteSqlRawAsync(
                        @"UPDATE tblTmkDesCaseTypeDelete SET IntlCode=@p0, CaseType=@p1, DesCountry=@p2, DesCaseType=@p3,
                          [Default]=@p4, IntlCodeNew=@p5, CaseTypeNew=@p6, DesCountryNew=@p7, DesCaseTypeNew=@p8, Systems=@p9
                          WHERE IntlCode=@p10 AND CaseType=@p11 AND DesCountry=@p12 AND DesCaseType=@p13
                          AND Systems=@p18",
                        new object[] {
                            new Microsoft.Data.SqlClient.SqlParameter("@p0", entity.IntlCode ?? ""),
                            new Microsoft.Data.SqlClient.SqlParameter("@p1", entity.CaseType ?? ""),
                            new Microsoft.Data.SqlClient.SqlParameter("@p2", entity.DesCountry ?? ""),
                            new Microsoft.Data.SqlClient.SqlParameter("@p3", entity.DesCaseType ?? ""),
                            new Microsoft.Data.SqlClient.SqlParameter("@p4", entity.Default),
                            new Microsoft.Data.SqlClient.SqlParameter("@p5", entity.IntlCodeNew ?? ""),
                            new Microsoft.Data.SqlClient.SqlParameter("@p6", entity.CaseTypeNew ?? ""),
                            new Microsoft.Data.SqlClient.SqlParameter("@p7", entity.DesCountryNew ?? ""),
                            new Microsoft.Data.SqlClient.SqlParameter("@p8", entity.DesCaseTypeNew ?? ""),
                            new Microsoft.Data.SqlClient.SqlParameter("@p9", entity.Systems),
                            new Microsoft.Data.SqlClient.SqlParameter("@p10", existing.IntlCode ?? ""),
                            new Microsoft.Data.SqlClient.SqlParameter("@p11", existing.CaseType ?? ""),
                            new Microsoft.Data.SqlClient.SqlParameter("@p12", existing.DesCountry ?? ""),
                            new Microsoft.Data.SqlClient.SqlParameter("@p13", existing.DesCaseType ?? ""),
                            new Microsoft.Data.SqlClient.SqlParameter("@p14", existing.IntlCodeNew ?? ""),
                            new Microsoft.Data.SqlClient.SqlParameter("@p15", existing.CaseTypeNew ?? ""),
                            new Microsoft.Data.SqlClient.SqlParameter("@p16", existing.DesCountryNew ?? ""),
                            new Microsoft.Data.SqlClient.SqlParameter("@p17", existing.DesCaseTypeNew ?? ""),
                            new Microsoft.Data.SqlClient.SqlParameter("@p18", existing.Systems ?? "")
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
                    @"INSERT INTO tblTmkDesCaseTypeDelete (IntlCode, CaseType, DesCountry, DesCaseType, [Default], IntlCodeNew, CaseTypeNew, DesCountryNew, DesCaseTypeNew, Systems)
                      VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9)",
                    new object[] {
                        new Microsoft.Data.SqlClient.SqlParameter("@p0", entity.IntlCode ?? ""),
                        new Microsoft.Data.SqlClient.SqlParameter("@p1", entity.CaseType ?? ""),
                        new Microsoft.Data.SqlClient.SqlParameter("@p2", entity.DesCountry ?? ""),
                        new Microsoft.Data.SqlClient.SqlParameter("@p3", entity.DesCaseType ?? ""),
                        new Microsoft.Data.SqlClient.SqlParameter("@p4", entity.Default),
                        new Microsoft.Data.SqlClient.SqlParameter("@p5", entity.IntlCodeNew ?? ""),
                        new Microsoft.Data.SqlClient.SqlParameter("@p6", entity.CaseTypeNew ?? ""),
                        new Microsoft.Data.SqlClient.SqlParameter("@p7", entity.DesCountryNew ?? ""),
                        new Microsoft.Data.SqlClient.SqlParameter("@p8", entity.DesCaseTypeNew ?? ""),
                        new Microsoft.Data.SqlClient.SqlParameter("@p9", entity.Systems)
                    });
            }

            return Json(new { id = 0, redirectUrl = Url.Action("Detail", new { intlCode = entity.IntlCode, caseType = entity.CaseType, desCountry = entity.DesCountry, desCaseType = entity.DesCaseType, intlCodeNew = entity.IntlCodeNew, caseTypeNew = entity.CaseTypeNew, desCountryNew = entity.DesCountryNew, desCaseTypeNew = entity.DesCaseTypeNew, systems = entity.Systems, singleRecord = true }) });
        }

        [HttpPost, Authorize(Policy = TrademarkAuthorizationPolicy.AuxiliaryCanDelete)]
        public async Task<IActionResult> Delete(string intlCode = "", string caseType = "", string desCountry = "", string desCaseType = "", string intlCodeNew = "", string caseTypeNew = "", string desCountryNew = "", string desCaseTypeNew = "", string systems = "")
        {
            var count = await _repository.Database.ExecuteSqlRawAsync(
                "DELETE FROM tblTmkDesCaseTypeDelete WHERE IntlCode=@p0 AND CaseType=@p1 AND DesCountry=@p2 AND DesCaseType=@p3 AND Systems=@p8",
                new object[] {
                    new Microsoft.Data.SqlClient.SqlParameter("@p0", intlCode ?? ""),
                    new Microsoft.Data.SqlClient.SqlParameter("@p1", caseType ?? ""),
                    new Microsoft.Data.SqlClient.SqlParameter("@p2", desCountry ?? ""),
                    new Microsoft.Data.SqlClient.SqlParameter("@p3", desCaseType ?? ""),
                    new Microsoft.Data.SqlClient.SqlParameter("@p4", intlCodeNew ?? ""),
                    new Microsoft.Data.SqlClient.SqlParameter("@p5", caseTypeNew ?? ""),
                    new Microsoft.Data.SqlClient.SqlParameter("@p6", desCountryNew ?? ""),
                    new Microsoft.Data.SqlClient.SqlParameter("@p7", desCaseTypeNew ?? ""),
                    new Microsoft.Data.SqlClient.SqlParameter("@p8", systems ?? "")
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

        public IActionResult GetRecordStamps(string intlCode = "", string caseType = "", string desCountry = "", string desCaseType = "", string intlCodeNew = "", string caseTypeNew = "", string desCountryNew = "", string desCaseTypeNew = "", string systems = "")
        {
            return ViewComponent("RecordStamps", new { createdBy = "", dateCreated = (DateTime?)null, updatedBy = "", lastUpdate = (DateTime?)null });
        }

        [HttpGet]
        public IActionResult DetailLink(string intlCode = "", string caseType = "", string desCountry = "", string desCaseType = "", string intlCodeNew = "", string caseTypeNew = "", string desCountryNew = "", string desCaseTypeNew = "", string systems = "")
        {
            if (!string.IsNullOrEmpty(intlCode) || !string.IsNullOrEmpty(caseType) || !string.IsNullOrEmpty(desCountry) || !string.IsNullOrEmpty(desCaseType))
                return RedirectToAction(nameof(Detail), new { intlCode, caseType, desCountry, desCaseType, intlCodeNew, caseTypeNew, desCountryNew, desCaseTypeNew, systems, singleRecord = true, fromSearch = true });
            return RedirectToAction(nameof(Add), new { fromSearch = true });
        }

        [HttpPost]
        public IActionResult ExcelExportSave(string contentType, string base64, string fileName)
        {
            return File(Convert.FromBase64String(base64), contentType, fileName);
        }
    }
}
