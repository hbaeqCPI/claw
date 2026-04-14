using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using R10.Core.DTOs;
using R10.Core.Interfaces;
using R10.Core.Interfaces.Shared;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Helpers;
using R10.Web.Extensions;
using R10.Web.Extensions.ActionResults;
using R10.Web.Interfaces;
using R10.Web.Security;
using R10.Web.Models;
using R10.Web.Models.PageViewModels;
using AutoMapper.QueryableExtensions;
using R10.Core;
using R10.Core.Entities.Patent;
using R10.Web.Areas.Patent.ViewModels;
using R10.Web.Areas.Patent.ViewModels.CountryLaw;
using R10.Core.Helpers;
using ActiveQueryBuilder.Core;

using Newtonsoft.Json;
using R10.Web.Areas;

namespace R10.Web.Areas.Patent.Controllers
{
    [Area("Patent"), Authorize(Policy = PatentAuthorizationPolicy.CanAccessCountryLaw)]
    public class CountryLawController : BaseController
    {
        private readonly IAuthorizationService _authService;
        private readonly IViewModelService<PatCountryLaw> _countryLawViewModelService;
        private readonly IPatCountryLawService _countryLawService;
        private readonly IEntityService<PatCountry> _patCountryService;
        private readonly IStringLocalizer<CountryLawResource> _localizer;
        private readonly ISystemSettings<PatSetting> _settings;
        private readonly IReportService _reportService;
        private readonly IWebLinksService _webLinksService;
        private readonly IApplicationDbContext _repository;

        private readonly string _dataContainer = "countryLawDetailsView";

        public CountryLawController(
            IAuthorizationService authService,
            IPatCountryLawService countryLawService,
            IViewModelService<PatCountryLaw> countryLawViewModelService,
            IEntityService<PatCountry> patCountryService,
            IStringLocalizer<CountryLawResource> localizer,
            IReportService reportService,
            ISystemSettings<PatSetting> settings,
            IWebLinksService webLinksService,
            IApplicationDbContext repository
            )
        {
            _authService = authService;
            _countryLawService = countryLawService;
            _countryLawViewModelService = countryLawViewModelService;
            _patCountryService = patCountryService;
            _localizer = localizer;
            _settings = settings;
            _reportService = reportService;
            _webLinksService = webLinksService;
            _repository = repository;
        }

        public async Task<IActionResult> Index()
        {
            var model = new PageViewModel()
            {
                Page = PageType.Search,
                PageId = "countryLawSearch",
                Title = _localizer["Country Law Search"].ToString(),
                CanAddRecord = await CanAddRecord()
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
                PageId = "countryLawSearchResults",
                Title = _localizer["Country Law Search Results"].ToString(),
                CanAddRecord = await CanAddRecord()
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
                var countryLaws = this.PatCountryLaws;
                if (mainSearchFilters != null && mainSearchFilters.Count > 0)
                {
                    countryLaws = AddRelatedDataCriteria(countryLaws, mainSearchFilters);
                    countryLaws = _countryLawViewModelService.AddCriteria(countryLaws, mainSearchFilters);
                }
                 var result = await CreateViewModelForGrid(request, countryLaws);
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

        [HttpGet()]
        public IActionResult DetailLink(string country, string caseType)
        {
            if (!string.IsNullOrEmpty(country) && !string.IsNullOrEmpty(caseType))
            {
                return RedirectToAction(nameof(Detail), new { country = country, caseType = caseType, singleRecord = true, fromSearch = true });
            }
            else
            {
                return RedirectToAction(nameof(Add), new { fromSearch = true });
            }
        }

        public IActionResult DetailLinkCountryCaseType(string country, string caseType) {
            return RedirectToAction(nameof(DetailLink), new { country = country, caseType = caseType });
        }

        public async Task<IActionResult> Detail(string country, string caseType, string systems = "", bool singleRecord = false, bool fromSearch = false, string tab = "")
        {
            if (string.IsNullOrEmpty(country) || string.IsNullOrEmpty(caseType))
                return RedirectToAction("Index");

            var page = await PrepareEditScreen(country, caseType, systems);
            if (page.Detail == null)
            {
                if (Request.IsAjax())
                    return new RecordDoesNotExistResult();
                return RedirectToAction("Index");
            }

            var detail = page.Detail;
            PageViewModel model = new PageViewModel()
            {
                Page = PageType.Detail,
                PageId = page.Container,
                Title = _localizer["Country Law Detail"].ToString(),
                RecordId = 1,
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

        [HttpGet]
        public IActionResult Print()
        {
            ViewBag.Url = Url.Action("Print");
            ViewBag.DownloadName = "Country Law Print Screen";
            return View();
        }

        [HttpPost]
        public IActionResult Print([FromBody] PatCountryLawPrintViewModel patCountryLawPrintModel)
        {
            if (ModelState.IsValid)
            {
                return _reportService.GetReport(patCountryLawPrintModel, ReportType.PatCountryLawPrintScreen).Result;
            }

            return BadRequest("Unhandled error.");
        }

        [Authorize(Policy = PatentAuthorizationPolicy.CountryLawModify)]
        public async Task<IActionResult> Add(bool fromSearch = false)
        {
            if (!Request.IsAjax() && !TempData.ContainsKey("CopyOptions"))
                return RedirectToAction("Index");

            var page = await PrepareAddScreen();
            if (page.Detail == null)
                return RedirectToAction("Index");

            if (TempData["CopyOptions"] != null)
                await ExtractCopyParams(page);

            var detail = page.Detail;
            PageViewModel model = new PageViewModel()
            {
                Page = fromSearch ? PageType.Detail : PageType.DetailContent,
                PageId = page.Container,
                Title = _localizer[$"New Country Law"].ToString(),
                RecordId = 0,
                PagePermission = page,
                Data = detail,
                FromSearch = fromSearch,
                AfterCancelledInsert = $"function() {{ window.location.href = '{Url.Action("Index")}'; }}"
            };
            ModelState.Clear();

            return Request.IsAjax() ? PartialView("Index", model) : View("Index", model);
        }

        private async Task<DetailPageViewModel<PatCountryLawDetailViewModel>> PrepareAddScreen()
        {
            var viewModel = new DetailPageViewModel<PatCountryLawDetailViewModel>
            {
                Detail = new PatCountryLawDetailViewModel()
            };

            viewModel.AddPatentCountryLawSecurityPolicies();
            await viewModel.ApplyDetailPagePermission(User, _authService);

            this.AddDefaultNavigationUrls(viewModel);
            viewModel.Container = _dataContainer;
            return viewModel;
        }

        [HttpPost, Authorize(Policy = PatentAuthorizationPolicy.CountryLawCanDelete)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string country, string caseType, string systems = "")
        {
            systems ??= "";
            var countryLaw = _countryLawService.PatCountryLaws.FirstOrDefault(c => c.Country == country && c.CaseType == caseType && c.Systems == systems);
            if (countryLaw == null && !string.IsNullOrEmpty(systems))
                countryLaw = _countryLawService.PatCountryLaws.FirstOrDefault(c => c.Country == country && c.CaseType == caseType && c.Systems.StartsWith(systems));
            if (countryLaw != null)
            {
                // Cascade delete all child records
                await _repository.Database.ExecuteSqlRawAsync(
                    "DELETE FROM tblPatCountryDue WHERE Country=@p0 AND CaseType=@p1 AND Systems=@p2",
                    country ?? "", caseType ?? "", systems ?? "");
                await _repository.Database.ExecuteSqlRawAsync(
                    "DELETE FROM tblPatCountryExp WHERE Country=@p0 AND CaseType=@p1 AND Systems=@p2",
                    country ?? "", caseType ?? "", systems ?? "");
                await _repository.Database.ExecuteSqlRawAsync(
                    "DELETE FROM tblPatDesCaseType WHERE IntlCode=@p0 AND CaseType=@p1 AND Systems=@p2",
                    country ?? "", caseType ?? "", systems ?? "");

                await _countryLawService.DeleteCountryLaw(countryLaw);
                return Ok();
            }
            else
                return BadRequest(_localizer["Unable to perform operation because the record is no longer on file or has been modified by another user."]);

        }

        [HttpPost, Authorize(Policy = PatentAuthorizationPolicy.CountryLawModify)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save([FromBody] PatCountryLaw countryLaw)
        {
            if (ModelState.IsValid)
            {
                var userName = User.GetUserName();
                var now = DateTime.Now;

                countryLaw.UserID = userName;
                countryLaw.LastUpdate = now;
                countryLaw.DefaultAgent ??= "";
                countryLaw.Remarks ??= "";
                countryLaw.UserRemarks ??= "";
                countryLaw.Systems ??= "";

                // Require at least one system
                if (string.IsNullOrWhiteSpace(countryLaw.Systems))
                {
                    return new JsonBadRequest("At least one system must be selected.");
                }

                // Deduplicate systems within this record
                if (!string.IsNullOrEmpty(countryLaw.Systems))
                {
                    var systems = countryLaw.Systems.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => s.Trim()).Distinct(StringComparer.OrdinalIgnoreCase);
                    countryLaw.Systems = string.Join(",", systems);
                }

                var isNewRecord = countryLaw.OriginalSystems == "__NEW__" || countryLaw.OriginalSystems == null
                    || !string.IsNullOrEmpty(countryLaw.CopyOptions);
                var originalSystemsValue = countryLaw.OriginalSystems == "__EMPTY__" ? "" : (countryLaw.OriginalSystems ?? "");

                // Check no individual system is already used under another row with same Country+CaseType
                if (!string.IsNullOrEmpty(countryLaw.Systems))
                {
                    var selectedSystems = countryLaw.Systems.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => s.Trim()).ToList();

                    // Get all systems already assigned to this Country+CaseType across ALL rows
                    var allRows = await _countryLawService.PatCountryLaws.AsNoTracking()
                        .Where(c => c.Country == countryLaw.Country && c.CaseType == countryLaw.CaseType
                                    && c.Systems != null && c.Systems != "")
                        .Select(c => c.Systems)
                        .ToListAsync();

                    // If editing, exclude the original record's systems from "taken" pool
                    var takenSystems = allRows
                        .Where(s => isNewRecord || s != originalSystemsValue)
                        .SelectMany(s => s.Split(',', StringSplitOptions.RemoveEmptyEntries))
                        .Select(s => s.Trim())
                        .ToHashSet(StringComparer.OrdinalIgnoreCase);

                    var overlap = selectedSystems.Where(s => takenSystems.Contains(s)).ToList();
                    if (overlap.Any())
                    {
                        return new JsonBadRequest($"System(s) '{string.Join(", ", overlap)}' already assigned to {countryLaw.Country}/{countryLaw.CaseType}.");
                    }
                }

                PatCountryLaw existing = null;
                if (!isNewRecord)
                {
                    existing = await _countryLawService.PatCountryLaws
                        .FirstOrDefaultAsync(c => c.Country == countryLaw.Country && c.CaseType == countryLaw.CaseType && c.Systems == originalSystemsValue);
                }

                if (existing != null)
                {
                    countryLaw.DateCreated = existing.DateCreated ?? now;

                    // Use raw SQL to avoid EF concurrency token (tStamp) issues
                    if (existing.Systems != countryLaw.Systems)
                    {
                        // Systems changed — delete old, insert new
                        await _repository.Database.ExecuteSqlRawAsync(
                            "DELETE FROM tblPatCountryLaw WHERE Country=@p0 AND CaseType=@p1 AND Systems=@p2",
                            existing.Country ?? "", existing.CaseType ?? "", existing.Systems ?? "");
                        await _repository.Database.ExecuteSqlRawAsync(
                            @"INSERT INTO tblPatCountryLaw (Country, CaseType, Systems, DefaultAgent, LabelTaxSched, AutoGenDesCtry, AutoUpdtDesPatRecs, CalcExpirBeforeIssue, Remarks, UserRemarks, UserID, DateCreated, LastUpdate)
                              VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9, @p10, @p11, @p12)",
                            countryLaw.Country ?? "", countryLaw.CaseType ?? "", countryLaw.Systems ?? "",
                            countryLaw.DefaultAgent ?? "", countryLaw.LabelTaxSched ?? "", countryLaw.AutoGenDesCtry, countryLaw.AutoUpdtDesPatRecs, countryLaw.CalcExpirBeforeIssue,
                            countryLaw.Remarks ?? "", countryLaw.UserRemarks ?? "", countryLaw.UserID ?? "", countryLaw.DateCreated, countryLaw.LastUpdate);
                    }
                    else
                    {
                        // Same systems — update in place
                        await _repository.Database.ExecuteSqlRawAsync(
                            @"UPDATE tblPatCountryLaw SET DefaultAgent=@p0, LabelTaxSched=@p1, AutoGenDesCtry=@p2, AutoUpdtDesPatRecs=@p3, CalcExpirBeforeIssue=@p4,
                              Remarks=@p5, UserRemarks=@p6, UserID=@p7, LastUpdate=@p8
                              WHERE Country=@p9 AND CaseType=@p10 AND Systems=@p11",
                            countryLaw.DefaultAgent ?? "", countryLaw.LabelTaxSched ?? "", countryLaw.AutoGenDesCtry, countryLaw.AutoUpdtDesPatRecs, countryLaw.CalcExpirBeforeIssue,
                            countryLaw.Remarks ?? "", countryLaw.UserRemarks ?? "", countryLaw.UserID ?? "", countryLaw.LastUpdate,
                            countryLaw.Country ?? "", countryLaw.CaseType ?? "", existing.Systems ?? "");
                    }

                    // Cascade Systems change to child records
                    if (countryLaw.Systems != originalSystemsValue)
                    {
                        await _repository.Database.ExecuteSqlRawAsync(
                            "UPDATE tblPatCountryDue SET Systems=@p0 WHERE Country=@p1 AND CaseType=@p2 AND Systems=@p3",
                            countryLaw.Systems, countryLaw.Country ?? "", countryLaw.CaseType ?? "", originalSystemsValue ?? "");
                        await _repository.Database.ExecuteSqlRawAsync(
                            "UPDATE tblPatCountryExp SET Systems=@p0 WHERE Country=@p1 AND CaseType=@p2 AND Systems=@p3",
                            countryLaw.Systems, countryLaw.Country ?? "", countryLaw.CaseType ?? "", originalSystemsValue ?? "");
                        await _repository.Database.ExecuteSqlRawAsync(
                            "UPDATE tblPatDesCaseType SET Systems=@p0 WHERE IntlCode=@p1 AND CaseType=@p2 AND Systems=@p3",
                            countryLaw.Systems, countryLaw.Country ?? "", countryLaw.CaseType ?? "", originalSystemsValue ?? "");
                    }
                }
                else
                {
                    // Double-check: no system overlap at individual level
                    var allExistingSystems = await _countryLawService.PatCountryLaws.AsNoTracking()
                        .Where(c => c.Country == countryLaw.Country && c.CaseType == countryLaw.CaseType)
                        .Select(c => c.Systems).ToListAsync();
                    var allTaken = allExistingSystems
                        .SelectMany(s => (s ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries))
                        .Select(s => s.Trim()).ToHashSet(StringComparer.OrdinalIgnoreCase);
                    var newIndividual = countryLaw.Systems.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();
                    var sysOverlap = newIndividual.Where(s => allTaken.Contains(s)).ToList();
                    if (sysOverlap.Any())
                        return new JsonBadRequest($"System(s) '{string.Join(", ", sysOverlap)}' already assigned to {countryLaw.Country}/{countryLaw.CaseType}.");

                    countryLaw.DateCreated = now;
                    _repository.DetachAllEntities();
                    await _repository.Database.ExecuteSqlRawAsync(
                        @"INSERT INTO tblPatCountryLaw (Country, CaseType, Systems, DefaultAgent, LabelTaxSched, AutoGenDesCtry, AutoUpdtDesPatRecs, CalcExpirBeforeIssue, Remarks, UserRemarks, UserID, DateCreated, LastUpdate)
                          VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9, @p10, @p11, @p12)",
                        countryLaw.Country ?? "", countryLaw.CaseType ?? "", countryLaw.Systems ?? "",
                        countryLaw.DefaultAgent ?? "", countryLaw.LabelTaxSched ?? "", countryLaw.AutoGenDesCtry, countryLaw.AutoUpdtDesPatRecs, countryLaw.CalcExpirBeforeIssue,
                        countryLaw.Remarks ?? "", countryLaw.UserRemarks ?? "", countryLaw.UserID ?? "", countryLaw.DateCreated, countryLaw.LastUpdate);
                    if (!string.IsNullOrEmpty(countryLaw.CopyOptions))
                        await CopyChildData(countryLaw);
                }

                return Json(new { id = 0, country = countryLaw.Country, caseType = countryLaw.CaseType,
                    redirectUrl = Url.Action("Detail", new { country = countryLaw.Country, caseType = countryLaw.CaseType, systems = countryLaw.Systems, singleRecord = true }) });
            }
            else
            {
                return new JsonBadRequest(new { errors = ModelState.Errors() });
            }
        }

        private async Task CopyChildData(PatCountryLaw newCountryLaw)
        {
            var copyOptions = JsonConvert.DeserializeObject<CountryLawCopyViewModel>(newCountryLaw.CopyOptions);
            if (copyOptions == null) return;

            var userName = User.GetUserName();
            var srcCountry = copyOptions.SourceCountry ?? copyOptions.Country;
            var srcCaseType = copyOptions.SourceCaseType ?? copyOptions.CaseType;
            var now = DateTime.Now;

            if (copyOptions.CopyLawActions)
            {
                var sourceDues = await _countryLawService.GetCountryDues(srcCountry, srcCaseType);
                var maxDueId = await _countryLawService.PatCountryDues.MaxAsync(d => (int?)d.CDueId) ?? 0;
                foreach (var due in sourceDues)
                {
                    due.CDueId = ++maxDueId;
                    due.Country = newCountryLaw.Country;
                    due.CaseType = newCountryLaw.CaseType;
                    due.Systems = newCountryLaw.Systems;
                    due.CPIAction = false;
                    due.CPIPermanentID = 0;
                    due.UserID = userName;
                    due.DateCreated = now;
                    due.LastUpdate = now;
                }
                await _countryLawService.AddChildren(sourceDues);
            }

            if (copyOptions.CopyExpirationTerms)
            {
                var sourceExps = await _countryLawService.GetCountryExps(srcCountry, srcCaseType);
                var maxExpId = await _repository.PatCountryExpirations.MaxAsync(e => (int?)e.CExpId) ?? 0;
                foreach (var exp in sourceExps)
                {
                    exp.CExpId = ++maxExpId;
                    exp.Country = newCountryLaw.Country;
                    exp.CaseType = newCountryLaw.CaseType;
                    exp.Systems = newCountryLaw.Systems;
                }
                await _countryLawService.AddChildren(sourceExps);
            }

            if (copyOptions.CopyDesignatedCountries)
            {
                var sourceDesCaseTypes = await _countryLawService.PatDesCaseTypes.AsNoTracking()
                    .Where(d => d.IntlCode == srcCountry && d.CaseType == srcCaseType).ToListAsync();
                // Use raw SQL to avoid EF tracking conflicts
                foreach (var des in sourceDesCaseTypes)
                {
                    await _repository.Database.ExecuteSqlRawAsync(
                        "INSERT INTO tblPatDesCaseType (IntlCode, CaseType, DesCountry, DesCaseType, [Default], Systems) VALUES (@p0, @p1, @p2, @p3, @p4, @p5)",
                        newCountryLaw.Country ?? "", newCountryLaw.CaseType ?? "", des.DesCountry ?? "", des.DesCaseType ?? "", des.Default, newCountryLaw.Systems ?? "");
                }
            }
        }

        [HttpPost, Authorize(Policy = PatentAuthorizationPolicy.CountryLawRemarksOnly)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveRemarks([FromBody] PatCountryLaw countryLaw)
        {
            if (ModelState.IsValid)
            {
                var userName = User.GetUserName();
                countryLaw.UserID = userName;
                countryLaw.LastUpdate = DateTime.Now;
                await _countryLawService.UpdateCountryLawRemarks(countryLaw);
                return Json(new { country = countryLaw.Country, caseType = countryLaw.CaseType });
            }
            else
            {
                return new JsonBadRequest(new { errors = ModelState.Errors() });
            }
        }


        public async Task<IActionResult> GetRecordStamps(string country, string caseType)
        {
            if (string.IsNullOrEmpty(country) || string.IsNullOrEmpty(caseType))
                return ViewComponent("RecordStamps", new { createdBy = "", dateCreated = (DateTime?)null, updatedBy = "", lastUpdate = (DateTime?)null });

            var countryLaw = await GetByKey(country, caseType);
            if (countryLaw == null)
                return ViewComponent("RecordStamps", new { createdBy = "", dateCreated = (DateTime?)null, updatedBy = "", lastUpdate = (DateTime?)null });

            return ViewComponent("RecordStamps", new { createdBy = countryLaw.UserID, dateCreated = countryLaw.DateCreated, updatedBy = countryLaw.UserID, lastUpdate = countryLaw.LastUpdate });
        }

        public async Task<IActionResult> GetPicklistData([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType, string requiredRelation = "")
        {
            return await GetPicklistData(_countryLawService.PatCountryLaws, request, property, text, filterType, requiredRelation);
        }

        public async Task<IActionResult> GetCountryList(string property, string text, FilterType filterType)
        {
            var countryLawCountries = _countryLawService.PatCountryLaws.Select(cl => cl.Country).Distinct();
            var countries = _patCountryService.QueryableList.Where(c => countryLawCountries.Contains(c.Country));
            countries = QueryHelper.BuildCriteria(countries, property, text, filterType);
            var list = await countries.Select(c => new { Country = c.Country, CountryName = c.CountryName }).OrderBy(c => c.Country).ToListAsync();
            return Json(list);
        }

        public async Task<IActionResult> GetCaseTypeList(string property, string text, FilterType filterType)
        {
            var clCaseTypes = _countryLawService.PatCountryLaws.Select(cl => cl.CaseType).Distinct();
            var caseTypes = _countryLawService.PatCaseTypes.Where(c => clCaseTypes.Contains(c.CaseType));
            caseTypes = QueryHelper.BuildCriteria(caseTypes, property, text, filterType);
            var list = await caseTypes.Select(c => new { CaseType = c.CaseType, Description = c.Description }).OrderBy(c => c.CaseType).ToListAsync();
            return Json(list);
        }

        public IActionResult GetSystemList()
        {
            return Json(Helpers.SystemsHelper.SystemNames);
        }

        //not needed
        //use BasedOnOption and GetPublicConstantValues type extension
        //public IActionResult GetBasedOnList(string property, string text, FilterType filterType)
        //{
        //    return Json(_countryLawService.GetBasedOnList());
        //}

        public async Task<IActionResult> GetExpirationTypeList(string property, string text, FilterType filterType)
        {
            return Json(await _countryLawService.GetExpirationTypeList());
        }

        #region CountryDue

        public async Task<IActionResult> CountryDueRead([DataSourceRequest] DataSourceRequest request, string country, string caseType, string systems = "")
        {
            var allDues = await _countryLawService.GetCountryDues(country, caseType);
            // Filter by systems - show records where any system overlaps
            var parentSystems = (systems ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var filtered = parentSystems.Any()
                ? allDues.Where(d => {
                    var dueSystems = (d.Systems ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToHashSet(StringComparer.OrdinalIgnoreCase);
                    return parentSystems.Any(ps => dueSystems.Contains(ps));
                }).ToList()
                : allDues;
            var result = filtered.ToDataSourceResult(request);
            return Json(result);
        }

        public IActionResult CountryDueAdd(string country, string caseType, string systems = "")
        {
            return PartialView("_CountryDueEntry", new PatCountryDue { Country = country, CaseType = caseType, Systems = systems, Calculate = true});
        }

        public async Task<IActionResult> CountryDueEdit(int cDueId)
        {
            var countryDue = await _countryLawService.GetCountryDue(cDueId);
            return PartialView("_CountryDueEntry", countryDue);
        }

        public async Task<IActionResult> CountryDueCopy(int cDueId)
        {
            var countryDue = await _countryLawService.GetCountryDue(cDueId);
            countryDue.CDueId = 0;
            countryDue.CPIAction = false;
            countryDue.CPIPermanentID = 0;

            ViewBag.CopyLawAction = true;
            return PartialView("_CountryDueEntry", countryDue);
        }

        public async Task<IActionResult> CountryDueCompute(int cDueId)
        {
            var vm = await _countryLawService.PatCountryDues.ProjectTo<CountryLawRetroParam>()
                .FirstOrDefaultAsync(d => d.CDueId == cDueId);
            return PartialView("_CountryDueCompute", vm);
        }

        [HttpPost, Authorize(Policy = PatentAuthorizationPolicy.CountryLawModify)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateCountryLawActions([FromBody]CountryLawRetroParam criteria)
        {
            criteria.UserName = User.Identity.Name;
            criteria.HasEntityFilterOn = User.HasEntityFilter();
            criteria.HasRespOfficeOn = User.HasRespOfficeFilter();
            await _countryLawService.GenerateCountryLawActions(criteria);
            return Ok();
        }

        public async Task<IActionResult> GetActionDues()
        {
            var result = await _countryLawService.GetActionDues();
            return Json(result);
        }

        public async Task<IActionResult> GetActionTypes()
        {
            var result = await _countryLawService.GetActionTypes();
            return Json(result);
        }

        //not needed
        //use RecurringOption enum for Recurring values
        //public IActionResult GetRecurringOptions()
        //{
        //    var result = _countryLawService.GetRecurringOptions();
        //    return Json(result);
        //}

        public async Task<IActionResult> GetFollowupList(string country)
        {
            var result = await _countryLawService.GetFollowupList(country);
            return Json(result);
        }

        [Authorize(Policy = PatentAuthorizationPolicy.CountryLawModify)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CountryDueSave([FromBody] PatCountryDue countryDue)
        {
            // Inherit Systems from parent if not provided
            if (string.IsNullOrEmpty(countryDue.Systems))
            {
                var parentSystems = await _countryLawService.PatCountryLaws.AsNoTracking()
                    .Where(c => c.Country == countryDue.Country && c.CaseType == countryDue.CaseType)
                    .Select(c => c.Systems).FirstOrDefaultAsync();
                countryDue.Systems = parentSystems ?? "";
                ModelState.Remove("Systems");
            }

            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(countryDue.ActionType))
                    countryDue.ActionType = countryDue.ActionDue;

                var userName = User.GetUserName();
                var now = DateTime.Now;

                countryDue.UserID = userName;
                countryDue.LastUpdate = now;
                if (countryDue.CDueId <= 0)
                    countryDue.DateCreated = now;
                await _countryLawService.CountryDueUpdate(countryDue);
                return Ok();
            }
            return BadRequest(ModelState);
        }


        [Authorize(Policy = PatentAuthorizationPolicy.CountryLawModify)]
        public async Task<IActionResult> CountryDuesUpdate([DataSourceRequest] DataSourceRequest request, string country, string caseType, [Bind(Prefix = "updated")]IList<PatCountryDue> updated,
                   [Bind(Prefix = "new")]IList<PatCountryDue> added, [Bind(Prefix = "deleted")]IList<PatCountryDue> deleted)
        {
            //no delete validation
            var canDelete = (await _authService.AuthorizeAsync(User, PatentAuthorizationPolicy.CountryLawCanDelete)).Succeeded;
            if (deleted.Any() && !canDelete)
                return Forbid("Not authorized.");

            // Get parent's Systems to ensure child records inherit it
            var parentSystems = (await _countryLawService.PatCountryLaws.AsNoTracking()
                .Where(c => c.Country == country && c.CaseType == caseType)
                .Select(c => c.Systems).FirstOrDefaultAsync()) ?? "";

            var userName = User.GetUserName();
            var now = DateTime.Now;
            foreach (var item in updated)
            {
                item.UserID = userName;
                item.LastUpdate = now;
                if (string.IsNullOrEmpty(item.Systems)) item.Systems = parentSystems;
            }
            // Generate CDueId for new records (not an identity column)
            var maxDueId = await _countryLawService.PatCountryDues.MaxAsync(d => (int?)d.CDueId) ?? 0;
            foreach (var item in added)
            {
                item.CDueId = ++maxDueId;
                item.UserID = userName;
                item.DateCreated = now;
                item.LastUpdate = now;
                if (string.IsNullOrEmpty(item.Systems)) item.Systems = parentSystems;
            }
            foreach (var item in deleted)
            {
                if (string.IsNullOrEmpty(item.Systems)) item.Systems = parentSystems;
            }
            await _countryLawService.UpdateChild(country, caseType, userName, updated, added, deleted);
            return Ok();
        }

        [Authorize(Policy = PatentAuthorizationPolicy.CountryLawCanDelete)]
        public async Task<IActionResult> CountryDueDelete([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "deleted")] PatCountryDue deleted)
        {
            // Inherit Systems from parent if not provided
            if (string.IsNullOrEmpty(deleted.Systems))
            {
                var parentSystems = await _countryLawService.PatCountryLaws.AsNoTracking()
                    .Where(c => c.Country == deleted.Country && c.CaseType == deleted.CaseType)
                    .Select(c => c.Systems).FirstOrDefaultAsync();
                deleted.Systems = parentSystems ?? "";
                ModelState.Remove("Systems");
            }
            ModelState.Remove("ActionType");
            ModelState.Remove("ActionDue");
            ModelState.Remove("BasedOn");
            ModelState.Remove("Indicator");

            if (!ModelState.IsValid)
                return new JsonBadRequest(new { errors = ModelState.Errors() });

            if (deleted.CDueId > 0)
            {
                await _countryLawService.DeleteCountryDue(deleted.Country, deleted.CaseType, User.GetUserName(), new List<PatCountryDue>() { deleted });
            }
            return Ok();
        }
        #endregion

        #region CountryExp

        public async Task<IActionResult> CountryExpRead([DataSourceRequest] DataSourceRequest request, string country, string caseType, string systems = "")
        {
            var allExps = await _countryLawService.GetCountryExps(country, caseType);
            var parentSystems = (systems ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var filtered = parentSystems.Any()
                ? allExps.Where(e => {
                    var expSystems = (e.Systems ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToHashSet(StringComparer.OrdinalIgnoreCase);
                    return parentSystems.Any(ps => expSystems.Contains(ps));
                }).ToList()
                : allExps;
            var result = filtered.ToDataSourceResult(request);
            return Json(result);
        }

        [Authorize(Policy = PatentAuthorizationPolicy.CountryLawModify)]
        public async Task<IActionResult> CountryExpUpdate([DataSourceRequest] DataSourceRequest request, string country, string caseType, [Bind(Prefix = "updated")]IList<PatCountryExp> updated,
                   [Bind(Prefix = "new")]IList<PatCountryExp> added, [Bind(Prefix = "deleted")]IList<PatCountryExp> deleted)
        {
            //no delete validation
            var canDelete = (await _authService.AuthorizeAsync(User, PatentAuthorizationPolicy.CountryLawCanDelete)).Succeeded;
            if (deleted.Any() && !canDelete)
                return Forbid("Not authorized.");

            // Validate required fields on added/updated records
            var errors = new Dictionary<string, string[]>();
            foreach (var e in updated.Concat(added))
            {
                if (string.IsNullOrWhiteSpace(e.Type)) errors["Type"] = new[] { "The Type field is required." };
                if (string.IsNullOrWhiteSpace(e.BasedOn)) errors["BasedOn"] = new[] { "The Based On field is required." };
                if (string.IsNullOrWhiteSpace(e.EffBasedOn)) errors["EffBasedOn"] = new[] { "The Eff Based On field is required." };
            }
            if (errors.Any())
                return new JsonBadRequest(new { errors });

            // Get parent's Systems to ensure child records inherit it
            var parentSystems = (await _countryLawService.PatCountryLaws.AsNoTracking()
                .Where(c => c.Country == country && c.CaseType == caseType)
                .Select(c => c.Systems).FirstOrDefaultAsync()) ?? "";

            // Generate CExpId for new records (not an identity column)
            var maxExpId = await _repository.PatCountryExpirations.MaxAsync(e => (int?)e.CExpId) ?? 0;
            var userName = User.GetUserName();
            foreach (var item in added)
            {
                item.CExpId = ++maxExpId;
                if (string.IsNullOrEmpty(item.Systems)) item.Systems = parentSystems;
            }
            foreach (var item in updated)
            {
                if (string.IsNullOrEmpty(item.Systems)) item.Systems = parentSystems;
            }
            foreach (var item in deleted)
            {
                if (string.IsNullOrEmpty(item.Systems)) item.Systems = parentSystems;
            }
            await _countryLawService.UpdateChild(country, caseType, userName, updated, added, deleted);
            return Ok();
        }

        [Authorize(Policy = PatentAuthorizationPolicy.CountryLawCanDelete)]
        public async Task<IActionResult> CountryExpDelete([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "deleted")] PatCountryExp deleted)
        {
            ModelState.Clear();

            if (deleted.CExpId > 0)
            {
                if (string.IsNullOrEmpty(deleted.Systems))
                {
                    var parentSystems = await _countryLawService.PatCountryLaws.AsNoTracking()
                        .Where(c => c.Country == deleted.Country && c.CaseType == deleted.CaseType)
                        .Select(c => c.Systems).FirstOrDefaultAsync();
                    deleted.Systems = parentSystems ?? "";
                }
                await _countryLawService.UpdateChild(deleted.Country, deleted.CaseType, User.GetUserName(), new List<PatCountryExp>(), new List<PatCountryExp>(), new List<PatCountryExp>() { deleted });
            }
            return Ok();
        }
        #endregion

        #region DesCaseType

        public async Task<IActionResult> DesCaseTypeRead([DataSourceRequest] DataSourceRequest request, string country, string caseType, string systems = "")
        {
            var parentSystems = (systems ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var desCaseTypes = _countryLawService.PatDesCaseTypes.Where(d => d.IntlCode == country && d.CaseType == caseType);
            var countries = _patCountryService.QueryableList;
            var result = await desCaseTypes
                .GroupJoin(countries, d => d.DesCountry, c => c.Country, (d, cs) => new { d, cs })
                .SelectMany(x => x.cs.DefaultIfEmpty(), (x, c) => new PatDesCaseTypeViewModel
                {
                    IntlCode = x.d.IntlCode,
                    CaseType = x.d.CaseType,
                    DesCountry = x.d.DesCountry,
                    DesCaseType = x.d.DesCaseType,
                    Default = x.d.Default,
                    Systems = x.d.Systems,
                    DesCountryName = c != null ? c.CountryName : ""
                })
                .ToListAsync();
            // Filter by systems overlap
            if (parentSystems.Any())
            {
                result = result.Where(r => {
                    var rSystems = (r.Systems ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToHashSet(StringComparer.OrdinalIgnoreCase);
                    return parentSystems.Any(ps => rSystems.Contains(ps));
                }).ToList();
            }
            return Json(result.ToDataSourceResult(request));
        }

        [Authorize(Policy = PatentAuthorizationPolicy.CountryLawModify)]
        public async Task<IActionResult> DesCaseTypeUpdate([DataSourceRequest] DataSourceRequest request, string country, string caseType, [Bind(Prefix = "updated")]IList<PatDesCaseType> updated,
                   [Bind(Prefix = "new")]IList<PatDesCaseType> added, [Bind(Prefix = "deleted")]IList<PatDesCaseType> deleted)
        {
            //no delete validation
            var canDelete = (await _authService.AuthorizeAsync(User, PatentAuthorizationPolicy.CountryLawCanDelete)).Succeeded;
            if (deleted.Any() && !canDelete)
                return Forbid("Not authorized.");

            // Get parent's Systems to ensure child records inherit it
            var parentSystems = (await _countryLawService.PatCountryLaws.AsNoTracking()
                .Where(c => c.Country == country && c.CaseType == caseType)
                .Select(c => c.Systems).FirstOrDefaultAsync()) ?? "";

            foreach (var item in added)
            {
                item.IntlCode = country;
                item.CaseType = caseType;
                item.Systems = parentSystems;
            }
            foreach (var item in updated)
            {
                item.IntlCode = country;
                item.CaseType = caseType;
                item.Systems = parentSystems;
            }
            foreach (var item in deleted)
            {
                item.IntlCode = country;
                item.CaseType = caseType;
                item.Systems = parentSystems;
            }
            await _countryLawService.UpdateChild(country, caseType, User.GetUserName(), updated, added, deleted);
            return Ok();
        }

        [Authorize(Policy = PatentAuthorizationPolicy.CountryLawCanDelete)]
        public async Task<IActionResult> DesCaseTypeDelete([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "deleted")] PatDesCaseType deleted)
        {
            ModelState.Clear();

            if (string.IsNullOrEmpty(deleted.Systems))
            {
                var parentSystems = await _countryLawService.PatCountryLaws.AsNoTracking()
                    .Where(c => c.Country == deleted.IntlCode && c.CaseType == deleted.CaseType)
                    .Select(c => c.Systems).FirstOrDefaultAsync();
                deleted.Systems = parentSystems ?? "";
            }
            await _countryLawService.UpdateChild(deleted.IntlCode, deleted.CaseType, User.GetUserName(), new List<PatDesCaseType>(), new List<PatDesCaseType>(), new List<PatDesCaseType>() { deleted });
            return Ok();
        }
        #endregion

        protected IQueryable<PatCountryLaw> PatCountryLaws => _countryLawService.PatCountryLaws;

        private async Task<DetailPageViewModel<PatCountryLawDetailViewModel>> PrepareEditScreen(string country, string caseType, string systems = "")
        {
            var viewModel = new DetailPageViewModel<PatCountryLawDetailViewModel>
            {
                Detail = await GetByKey(country, caseType, systems)
            };

            if (viewModel.Detail != null)
            {
                viewModel.AddPatentCountryLawSecurityPolicies();
                await viewModel.ApplyDetailPagePermission(User, _authService);

                viewModel.CanEmail = false;
                viewModel.Detail.CanDeleteChild = viewModel.CanDeleteRecord;
                viewModel.CanDeleteRecord = viewModel.CanDeleteRecord && !viewModel.Detail.IsCPiAction;

                this.AddDefaultNavigationUrls(viewModel);

                viewModel.Container = _dataContainer;

                viewModel.AddScreenUrl = viewModel.CanAddRecord ? Url.Action("Add", new { fromSearch = true }) : "";
                viewModel.EditScreenUrl = Url.Action("Detail", new { country = country, caseType = caseType, systems = systems });
                viewModel.DeleteScreenUrl = viewModel.CanDeleteRecord ? Url.Action("Delete", new { country = country, caseType = caseType, systems = systems }) : "";
                viewModel.CopyScreenUrl = $"{viewModel.CopyScreenUrl}?country={country}&caseType={caseType}&systems={systems}";
                viewModel.SearchScreenUrl = Url.Action("Index");
            }
            return viewModel;
        }

        private async Task<PatCountryLawDetailViewModel> GetByKey(string country, string caseType, string systems = "")
        {
            systems ??= "";
            var detail = await _countryLawService.PatCountryLaws.ProjectTo<PatCountryLawDetailViewModel>()
                .FirstOrDefaultAsync(c => c.Country == country && c.CaseType == caseType && c.Systems == systems);
            // Fallback: if systems contains special chars that got truncated by URL parsing, try starts-with
            if (detail == null && !string.IsNullOrEmpty(systems))
            {
                detail = await _countryLawService.PatCountryLaws.ProjectTo<PatCountryLawDetailViewModel>()
                    .FirstOrDefaultAsync(c => c.Country == country && c.CaseType == caseType && c.Systems.StartsWith(systems));
            }
            // Fallback: if no systems provided, get the first record for this country+casetype
            if (detail == null && string.IsNullOrEmpty(systems))
            {
                detail = await _countryLawService.PatCountryLaws.ProjectTo<PatCountryLawDetailViewModel>()
                    .FirstOrDefaultAsync(c => c.Country == country && c.CaseType == caseType);
            }
            if (detail != null)
            {
                detail.HasDesignatedCountries = await _countryLawService.HasDesignatedCountries(detail.Country, detail.CaseType);

                // Populate CountryName and CaseTypeDescription
                var countryEntity = await _patCountryService.QueryableList.AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Country == country);
                if (countryEntity != null)
                    detail.CountryName = countryEntity.CountryName;

                var caseTypeEntity = await _countryLawService.PatCaseTypes.AsNoTracking()
                    .FirstOrDefaultAsync(c => c.CaseType == caseType);
                if (caseTypeEntity != null)
                    detail.CaseTypeDescription = caseTypeEntity.Description;
            }
            return detail;
        }

        public async Task<IActionResult> WebLinksRead([DataSourceRequest] DataSourceRequest request, string country, string caseType, int displayChoice = 2)
        {
            var webLinks = await _webLinksService.GetWebLinks(0, "patcountrylaw", "FormLink", "");
            if (webLinks != null && displayChoice == 0)
                webLinks = webLinks.Where(w => w.RecordLink).ToList();

            var result = webLinks != null ? webLinks.ToDataSourceResult(request) : new Kendo.Mvc.UI.DataSourceResult();
            return Json(result);
        }

        public async Task<IActionResult> WebLinksUrl(string country, string caseType, int id)
        {
            try
            {
                var url = await _webLinksService.GetWebLinksUrl(0, id, "patcountrylaw", "FormLink", "");
                return Json(new { url });
            }
            catch (ArgumentException ex)
            {
                return Json(new { url = "", error = ex.Message });
            }
        }

        [HttpGet()]
        public async Task<IActionResult> Copy(string country, string caseType, string systems = "")
        {
            systems ??= "";
            var entity = await _countryLawService.PatCountryLaws.FirstOrDefaultAsync(c => c.Country == country && c.CaseType == caseType && c.Systems == systems);
            if (entity == null && !string.IsNullOrEmpty(systems))
                entity = await _countryLawService.PatCountryLaws.FirstOrDefaultAsync(c => c.Country == country && c.CaseType == caseType && c.Systems.StartsWith(systems));
            if (entity == null && string.IsNullOrEmpty(systems))
                entity = await _countryLawService.PatCountryLaws.FirstOrDefaultAsync(c => c.Country == country && c.CaseType == caseType);
            if (entity == null) return new RecordDoesNotExistResult();
            var viewModel = new CountryLawCopyViewModel
            {
                SourceCountry = entity.Country,
                SourceCaseType = entity.CaseType,
                SourceSystems = entity.Systems,
                Country = entity.Country,
                CaseType = entity.CaseType,
                Systems = entity.Systems,
                CopyRemarks = true,
                CopyLawActions = true,
                CopyExpirationTerms = true,
                CopyDesignatedCountries = true
            };
            return PartialView("_Copy", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditCopied([FromBody] CountryLawCopyViewModel copy)
        {
            if (!ModelState.IsValid) return new JsonBadRequest(new { errors = ModelState.Errors() });
            TempData["CopyOptions"] = JsonConvert.SerializeObject(copy);
            return RedirectToAction("Add");
        }

        private async Task ExtractCopyParams(DetailPageViewModel<PatCountryLawDetailViewModel> page)
        {
            var copyOptionsString = TempData["CopyOptions"].ToString();
            ViewBag.CopyOptions = copyOptionsString;
            var copyOptions = JsonConvert.DeserializeObject<CountryLawCopyViewModel>(copyOptionsString);
            if (copyOptions != null)
            {
                // Find source by original identifiers
                var srcCountry = copyOptions.SourceCountry ?? copyOptions.Country;
                var srcCaseType = copyOptions.SourceCaseType ?? copyOptions.CaseType;
                var srcSystems = copyOptions.SourceSystems ?? copyOptions.Systems ?? "";
                var source = await _countryLawService.PatCountryLaws.AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Country == srcCountry && c.CaseType == srcCaseType && c.Systems == srcSystems);
                if (source != null)
                {
                    page.Detail.Country = copyOptions.Country;
                    page.Detail.CaseType = copyOptions.CaseType;
                    page.Detail.Systems = copyOptions.Systems ?? source.Systems ?? "";
                    page.Detail.DefaultAgent = source.DefaultAgent;
                    page.Detail.LabelTaxSched = source.LabelTaxSched;
                    page.Detail.AutoGenDesCtry = source.AutoGenDesCtry;
                    page.Detail.AutoUpdtDesPatRecs = source.AutoUpdtDesPatRecs;
                    page.Detail.CalcExpirBeforeIssue = source.CalcExpirBeforeIssue;
                    page.Detail.Remarks = copyOptions.CopyRemarks ? source.Remarks : "";
                    page.Detail.UserRemarks = copyOptions.CopyRemarks ? source.UserRemarks : "";
                }
            }
        }

        private async Task<bool> CanAddRecord()
        {
            return (await _authService.AuthorizeAsync(User, PatentAuthorizationPolicy.CountryLawModify)).Succeeded;
        }

        private IQueryable<PatCountryLaw> AddRelatedDataCriteria(IQueryable<PatCountryLaw> countryLaws, List<QueryFilterViewModel> mainSearchFilters)
        {
            var countryName = mainSearchFilters.FirstOrDefault(f => f.Property == "CountryName");
            if (countryName != null && !string.IsNullOrEmpty(countryName.Value))
            {
                var values = countryName.Value.StartsWith("[")
                    ? Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(countryName.Value) ?? new List<string> { countryName.Value }
                    : new List<string> { countryName.Value };
                var countries = _patCountryService.QueryableList.AsQueryable();
                countries = countries.Where(pc => values.Any(v => EF.Functions.Like(pc.CountryName, v)));
                var matchingCodes = countries.Select(pc => pc.Country);
                countryLaws = countryLaws.Where(c => matchingCodes.Contains(c.Country));
            }
            if (countryName != null) mainSearchFilters.Remove(countryName);

            countryLaws = Helpers.QueryHelper.ApplySystemsFilter(countryLaws, mainSearchFilters, a => a.Systems);

            // ----- Law Actions tab filters (prefix "LawAction.") -----
            var lawActionFilters = mainSearchFilters.Where(f => f.Property != null && f.Property.StartsWith("LawAction.") && !string.IsNullOrEmpty(f.Value)).ToList();
            if (lawActionFilters.Any())
            {
                var dueQuery = _repository.PatCountryDues.AsNoTracking().AsQueryable();
                foreach (var f in lawActionFilters)
                {
                    var propName = f.Property.Substring("LawAction.".Length);
                    dueQuery = ApplyRelatedFilter(dueQuery, propName, f.Value);
                }
                var matchingKeys = dueQuery.Select(d => new { d.Country, d.CaseType }).Distinct();
                countryLaws = countryLaws.Where(cl => matchingKeys.Any(k => k.Country == cl.Country && k.CaseType == cl.CaseType));
            }
            foreach (var f in lawActionFilters) mainSearchFilters.Remove(f);

            // ----- Expiration Terms tab filters (prefix "Expiration.") -----
            var expFilters = mainSearchFilters.Where(f => f.Property != null && f.Property.StartsWith("Expiration.") && !string.IsNullOrEmpty(f.Value)).ToList();
            if (expFilters.Any())
            {
                var expQuery = _repository.PatCountryExpirations.AsNoTracking().AsQueryable();
                foreach (var f in expFilters)
                {
                    var propName = f.Property.Substring("Expiration.".Length);
                    expQuery = ApplyRelatedFilter(expQuery, propName, f.Value);
                }
                var matchingExpKeys = expQuery.Select(e => new { e.Country, e.CaseType }).Distinct();
                countryLaws = countryLaws.Where(cl => matchingExpKeys.Any(k => k.Country == cl.Country && k.CaseType == cl.CaseType));
            }
            foreach (var f in expFilters) mainSearchFilters.Remove(f);

            // ----- Designated Countries tab filters (prefix "DesCaseType.") -----
            var desFilters = mainSearchFilters.Where(f => f.Property != null && f.Property.StartsWith("DesCaseType.") && !string.IsNullOrEmpty(f.Value)).ToList();
            if (desFilters.Any())
            {
                var desQuery = _repository.PatDesCaseTypes.AsNoTracking().AsQueryable();
                foreach (var f in desFilters)
                {
                    var propName = f.Property.Substring("DesCaseType.".Length);
                    desQuery = ApplyRelatedFilter(desQuery, propName, f.Value);
                }
                // DesCaseType.IntlCode = CountryLaw.Country, DesCaseType.CaseType = CountryLaw.CaseType
                var matchingKeys = desQuery.Select(d => new { IntlCode = d.IntlCode, d.CaseType }).Distinct();
                countryLaws = countryLaws.Where(cl => matchingKeys.Any(k => k.IntlCode == cl.Country && k.CaseType == cl.CaseType));
            }
            foreach (var f in desFilters) mainSearchFilters.Remove(f);

            return countryLaws;
        }

        /// <summary>
        /// Apply a single filter to a related-entity IQueryable. Handles From/To date suffixes,
        /// bool Yes/No strings, multi-value JSON arrays, and LIKE for strings.
        /// </summary>
        private IQueryable<T> ApplyRelatedFilter<T>(IQueryable<T> query, string propertyName, string value)
        {
            // Date range (From/To suffix)
            if (propertyName.EndsWith("From") || propertyName.EndsWith("To"))
            {
                if (DateTime.TryParse(value, out var dt))
                {
                    bool isFrom = propertyName.EndsWith("From");
                    var baseName = isFrom ? propertyName.Substring(0, propertyName.Length - 4) : propertyName.Substring(0, propertyName.Length - 2);
                    var param = System.Linq.Expressions.Expression.Parameter(typeof(T));
                    var prop = System.Linq.Expressions.Expression.Property(param, baseName);
                    var hasValue = System.Linq.Expressions.Expression.Property(prop, "HasValue");
                    var propValue = System.Linq.Expressions.Expression.Property(prop, "Value");
                    System.Linq.Expressions.Expression comparison;
                    if (isFrom)
                        comparison = System.Linq.Expressions.Expression.GreaterThanOrEqual(propValue, System.Linq.Expressions.Expression.Constant(dt));
                    else
                        comparison = System.Linq.Expressions.Expression.LessThanOrEqual(propValue, System.Linq.Expressions.Expression.Constant(dt.AddDays(1).AddSeconds(-1)));
                    var body = System.Linq.Expressions.Expression.AndAlso(hasValue, comparison);
                    var lambda = System.Linq.Expressions.Expression.Lambda<Func<T, bool>>(body, param);
                    return query.Where(lambda);
                }
                return query;
            }

            // Boolean
            if (bool.TryParse(value, out var boolVal))
            {
                var propInfo = typeof(T).GetProperty(propertyName);
                if (propInfo != null && (propInfo.PropertyType == typeof(bool) || propInfo.PropertyType == typeof(bool?)))
                {
                    var param = System.Linq.Expressions.Expression.Parameter(typeof(T));
                    var prop = System.Linq.Expressions.Expression.Property(param, propertyName);
                    var body = System.Linq.Expressions.Expression.Equal(prop, System.Linq.Expressions.Expression.Constant(boolVal, propInfo.PropertyType));
                    var lambda = System.Linq.Expressions.Expression.Lambda<Func<T, bool>>(body, param);
                    return query.Where(lambda);
                }
            }

            // Multi-value (JSON array) - build IN clause
            if (value.StartsWith("[") && value.EndsWith("]"))
            {
                try
                {
                    var values = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(value) ?? new List<string>();
                    if (values.Any())
                    {
                        var param = System.Linq.Expressions.Expression.Parameter(typeof(T));
                        var prop = System.Linq.Expressions.Expression.Property(param, propertyName);
                        var listExpr = System.Linq.Expressions.Expression.Constant(values);
                        var containsCall = System.Linq.Expressions.Expression.Call(listExpr, typeof(List<string>).GetMethod("Contains", new[] { typeof(string) })!, prop);
                        var lambda = System.Linq.Expressions.Expression.Lambda<Func<T, bool>>(containsCall, param);
                        return query.Where(lambda);
                    }
                }
                catch { }
            }

            // Single value LIKE for strings, Equal for other types
            var pi = typeof(T).GetProperty(propertyName);
            if (pi != null)
            {
                var param = System.Linq.Expressions.Expression.Parameter(typeof(T));
                var prop = System.Linq.Expressions.Expression.Property(param, propertyName);
                System.Linq.Expressions.Expression body;
                if (pi.PropertyType == typeof(string))
                {
                    var likeCall = System.Linq.Expressions.Expression.Call(
                        typeof(DbFunctionsExtensions), "Like", Type.EmptyTypes,
                        System.Linq.Expressions.Expression.Constant(EF.Functions), prop, System.Linq.Expressions.Expression.Constant(value));
                    body = likeCall;
                }
                else
                {
                    body = System.Linq.Expressions.Expression.Equal(prop, System.Linq.Expressions.Expression.Constant(Convert.ChangeType(value, pi.PropertyType)));
                }
                var lambda = System.Linq.Expressions.Expression.Lambda<Func<T, bool>>(body, param);
                return query.Where(lambda);
            }
            return query;
        }

        private async Task<CPiDataSourceResult> CreateViewModelForGrid(DataSourceRequest request, IQueryable<PatCountryLaw> countryLaws)
        {
            var countries = _patCountryService.QueryableList;
            var model = countryLaws.Join(countries, cl => cl.Country, c => c.Country, (cl, c) => new PatCountryLawSearchViewModel
            {
                Country = cl.Country,
                CountryName = c.CountryName,
                CaseType = cl.CaseType,
                Systems = cl.Systems,
                LabelTaxSched = cl.LabelTaxSched,
                UserID = cl.UserID,
                DateCreated = cl.DateCreated,
                LastUpdate = cl.LastUpdate
            });

            if (request.Sorts != null && request.Sorts.Any())
                model = model.ApplySorting(request.Sorts);
            else
                model = model.OrderBy(c => c.Country).ThenBy(c => c.CaseType);

            var total = await model.CountAsync();

            return new CPiDataSourceResult()
            {
                Data = await model.ApplyPaging(request.Page, request.PageSize).ToListAsync(),
                Total = total,
                Ids = new int[0]
            };
        }
    }
}
