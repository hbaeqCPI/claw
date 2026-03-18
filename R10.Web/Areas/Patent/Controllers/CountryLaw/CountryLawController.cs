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
        private readonly IParentEntityService<PatCountry, PatAreaCountry> _patCountryService;
        private readonly IStringLocalizer<CountryLawResource> _localizer;
        private readonly ISystemSettings<PatSetting> _settings;
        private readonly IReportService _reportService;
        private readonly IWebLinksService _webLinksService;

        private readonly string _dataContainer = "countryLawDetailsView";

        public CountryLawController(
            IAuthorizationService authService,
            IPatCountryLawService countryLawService,
            IViewModelService<PatCountryLaw> countryLawViewModelService,
            IParentEntityService<PatCountry, PatAreaCountry> patCountryService,
            IStringLocalizer<CountryLawResource> localizer,
            IReportService reportService,
            ISystemSettings<PatSetting> settings,
            IWebLinksService webLinksService
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
                if (mainSearchFilters.Count > 0)
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
        public IActionResult DetailLink(int? id)
        {
            if (id > 0)
            {
                return RedirectToAction(nameof(Detail), new { id = id, singleRecord = true, fromSearch = true });
            }
            else
            {
                return RedirectToAction(nameof(Add), new { fromSearch = true });
            }
        }

        public async Task<IActionResult> DetailLinkCountryCaseType(string country, string caseType) {
            var id = 0;
            var countryLaw = await _countryLawService.PatCountryLaws.Where(c => c.Country == country && c.CaseType == caseType).FirstOrDefaultAsync();
            if (countryLaw != null)
                id = countryLaw.CountryLawID;
            return RedirectToAction(nameof(DetailLink), new { id = id});
        }

        public async Task<IActionResult> Detail(int id, bool singleRecord = false, bool fromSearch = false, string tab = "")
        {
            var page = await PrepareEditScreen(id);
            if (page.Detail == null)
            {
                return RedirectToAction("Index");
            }

            var detail = page.Detail;
            PageViewModel model = new PageViewModel()
            {
                Page = PageType.Detail,
                PageId = page.Container,
                Title = _localizer["Country Law Detail"].ToString(),
                RecordId = detail.CountryLawID,
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
            if (!Request.IsAjax())
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
                RecordId = detail.CountryLawID,
                PagePermission = page,
                Data = detail,
                FromSearch = fromSearch
            };
            ModelState.Clear();

            return PartialView("Index", model);
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
        public async Task<IActionResult> Delete(int id, string tStamp)
        {
            var countryLaw = _countryLawService.PatCountryLaws.FirstOrDefault(c => c.CountryLawID == id && c.tStamp == Convert.FromBase64String(tStamp));
            if (countryLaw != null)
            {
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
                var overlapError = await CheckSystemsOverlap(countryLaw);
                if (overlapError != null)
                    return BadRequest(overlapError);

                UpdateEntityStamps(countryLaw, countryLaw.CountryLawID);

                if (countryLaw.CountryLawID > 0)
                    await _countryLawService.UpdateCountryLaw(countryLaw);
                else
                {
                    await _countryLawService.AddCountryLaw(countryLaw);
                    if (!string.IsNullOrEmpty(countryLaw.CopyOptions))
                        await CopyChildData(countryLaw);
                }

                return Json(countryLaw.CountryLawID);
            }
            else
            {
                return new JsonBadRequest(new { errors = ModelState.Errors() });
            }
        }

        private async Task<string> CheckSystemsOverlap(PatCountryLaw countryLaw)
        {
            var existing = await _countryLawService.PatCountryLaws.AsNoTracking()
                .Where(c => c.Country == countryLaw.Country && c.CaseType == countryLaw.CaseType && c.CountryLawID != countryLaw.CountryLawID)
                .Select(c => new { c.Systems })
                .ToListAsync();

            if (!existing.Any()) return null;

            var newSystems = ParseSystems(countryLaw.Systems);

            foreach (var ex in existing)
            {
                var existingSystems = ParseSystems(ex.Systems);
                var overlap = newSystems.Intersect(existingSystems).ToList();
                if (overlap.Any())
                    return $"System(s) '{string.Join(", ", overlap)}' already assigned to another Country Law record for {countryLaw.Country}/{countryLaw.CaseType}.";
            }

            return null;
        }

        private static HashSet<string> ParseSystems(string systems)
        {
            if (string.IsNullOrWhiteSpace(systems))
                return new HashSet<string>();
            return new HashSet<string>(systems.Split("; ", StringSplitOptions.RemoveEmptyEntries));
        }

        private async Task CopyChildData(PatCountryLaw newCountryLaw)
        {
            var copyOptions = JsonConvert.DeserializeObject<CountryLawCopyViewModel>(newCountryLaw.CopyOptions);
            if (copyOptions == null) return;

            var userName = User.GetUserName();
            var now = DateTime.Now;

            if (copyOptions.CopyLawActions)
            {
                var sourceDues = await _countryLawService.GetCountryDues(copyOptions.CountryLawID);
                foreach (var due in sourceDues)
                {
                    due.CDueId = 0;
                    due.CountryLawID = newCountryLaw.CountryLawID;
                    due.Country = newCountryLaw.Country;
                    due.CaseType = newCountryLaw.CaseType;
                    due.CPIAction = false;
                    due.CPIPermanentID = 0;
                    due.CreatedBy = userName;
                    due.UpdatedBy = userName;
                    due.DateCreated = now;
                    due.LastUpdate = now;
                }
                await _countryLawService.AddChildren(sourceDues);
            }

            if (copyOptions.CopyExpirationTerms)
            {
                var sourceExps = await _countryLawService.GetCountryExps(copyOptions.CountryLawID);
                foreach (var exp in sourceExps)
                {
                    exp.CExpId = 0;
                    exp.CountryLawID = newCountryLaw.CountryLawID;
                    exp.Country = newCountryLaw.Country;
                    exp.CaseType = newCountryLaw.CaseType;
                    exp.CreatedBy = userName;
                    exp.UpdatedBy = userName;
                    exp.DateCreated = now;
                    exp.LastUpdate = now;
                }
                await _countryLawService.AddChildren(sourceExps);
            }

            if (copyOptions.CopyDesignatedCountries)
            {
                var sourceDesCaseTypes = await _countryLawService.PatDesCaseTypes.AsNoTracking()
                    .Where(d => d.IntlCode == copyOptions.Country && d.CaseType == copyOptions.CaseType).ToListAsync();
                foreach (var des in sourceDesCaseTypes)
                {
                    des.DesCaseTypeID = 0;
                    des.IntlCode = newCountryLaw.Country;
                    des.CaseType = newCountryLaw.CaseType;
                    des.CreatedBy = userName;
                    des.UpdatedBy = userName;
                    des.DateCreated = now;
                    des.LastUpdate = now;
                }
                await _countryLawService.AddChildren(sourceDesCaseTypes);
            }
        }

        [HttpPost, Authorize(Policy = PatentAuthorizationPolicy.CountryLawRemarksOnly)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveRemarks([FromBody] PatCountryLaw countryLaw)
        {
            if (ModelState.IsValid)
            {
                UpdateEntityStamps(countryLaw, countryLaw.CountryLawID);
                await _countryLawService.UpdateCountryLawRemarks(countryLaw);
                return Json(countryLaw.CountryLawID);
            }
            else
            {
                return new JsonBadRequest(new { errors = ModelState.Errors() });
            }
        }


        public async Task<IActionResult> GetRecordStamps(int id)
        {
            var countryLaw = await GetById(id);
            if (countryLaw == null)
                return new NoRecordFoundResult();

            return ViewComponent("RecordStamps", new { createdBy = countryLaw.CreatedBy, dateCreated = countryLaw.DateCreated, updatedBy = countryLaw.UpdatedBy, lastUpdate = countryLaw.LastUpdate, tStamp = countryLaw.tStamp });
        }

        public async Task<IActionResult> GetPicklistData([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType, string requiredRelation = "")
        {
            return await GetPicklistData(_countryLawService.PatCountryLaws, request, property, text, filterType, requiredRelation);
        }

        public async Task<IActionResult> GetCountryList(string property, string text, FilterType filterType)
        {
            var countries = _patCountryService.QueryableList;
            countries = countries.Where(c => c.PatCountryLaws.Any());
            countries = QueryHelper.BuildCriteria(countries, property, text, filterType);
            var list = await countries.Select(c => new { Country = c.Country, CountryName = c.CountryName }).OrderBy(c => c.Country).ToListAsync();
            return Json(list);
        }

        public async Task<IActionResult> GetCaseTypeList(string property, string text, FilterType filterType)
        {
            var caseTypes = _countryLawService.PatCaseTypes.Where(c => c.CaseTypeCountryLaws.Any());
            caseTypes = QueryHelper.BuildCriteria(caseTypes, property, text, filterType);
            var list = await caseTypes.Select(c => new { CaseType = c.CaseType, Description = c.Description }).OrderBy(c => c.CaseType).ToListAsync();
            return Json(list);
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

        public async Task<IActionResult> CountryDueRead([DataSourceRequest] DataSourceRequest request, int parentId)
        {
            var result = (await _countryLawService.GetCountryDues(parentId)).ToDataSourceResult(request);
            return Json(result);
        }

        public IActionResult CountryDueAdd(int countryLawId, string country, string caseType)
        {
            return PartialView("_CountryDueEntry", new PatCountryDue { CountryLawID = countryLawId, Country = country, CaseType = caseType, Calculate = true});
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

        public IActionResult GetFollowupList(string country)
        {
            var result = _countryLawService.GetFollowupList(country);
            return Json(result);
        }

        [Authorize(Policy = PatentAuthorizationPolicy.CountryLawModify)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CountryDueSave([FromBody] PatCountryDue countryDue)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(countryDue.ActionType))
                    countryDue.ActionType = countryDue.ActionDue;

                if (countryDue.CPIAction)
                {
                    var origCountryDue = await _countryLawService.PatCountryDues.AsNoTracking().FirstOrDefaultAsync(c => c.CDueId == countryDue.CDueId);
                    origCountryDue.Calculate = countryDue.Calculate;
                    origCountryDue.FollowupAction = countryDue.FollowupAction;
                    origCountryDue.OldFollowupAction = countryDue.OldFollowupAction;
                    origCountryDue.ParentTStamp = countryDue.ParentTStamp;
                    origCountryDue.MultipleBasedOn = countryDue.MultipleBasedOn;
                    UpdateEntityStamps(origCountryDue, origCountryDue.CDueId);
                    await _countryLawService.CountryDueUpdate(origCountryDue);
                }
                else {
                    UpdateEntityStamps(countryDue, countryDue.CDueId);
                    await _countryLawService.CountryDueUpdate(countryDue);
                }
                return Ok();
            }
            return BadRequest(ModelState);
        }


        [Authorize(Policy = PatentAuthorizationPolicy.CountryLawModify)]
        public async Task<IActionResult> CountryDuesUpdate([DataSourceRequest] DataSourceRequest request, int parentId, string country,string caseType, byte[] tStamp, [Bind(Prefix = "updated")]IList<PatCountryDue> updated,
                   [Bind(Prefix = "new")]IList<PatCountryDue> added, [Bind(Prefix = "deleted")]IList<PatCountryDue> deleted)
        {
            //no delete validation
            var canDelete = (await _authService.AuthorizeAsync(User, PatentAuthorizationPolicy.CountryLawCanDelete)).Succeeded;
            if (deleted.Any() && !canDelete)
                return Forbid("Not authorized.");

            if (!ModelState.IsValid)
                return new JsonBadRequest(new { errors = ModelState.Errors() });

            foreach (var item in updated)
            {
                item.PatCountryLaw = null;
                UpdateEntityStamps(item, item.CDueId);
            }
            foreach (var item in added)
            {
                item.PatCountryLaw = null;
                UpdateEntityStamps(item, item.CDueId);
            }
            await _countryLawService.UpdateChild(parentId,country,caseType, User.GetUserName(), tStamp, updated, added, deleted);
            return Ok();
        }

        [Authorize(Policy = PatentAuthorizationPolicy.CountryLawCanDelete)]
        public async Task<IActionResult> CountryDueDelete([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "deleted")] PatCountryDue deleted)
        {
            if (!ModelState.IsValid)
                return new JsonBadRequest(new { errors = ModelState.Errors() });

            if (deleted.CDueId > 0)
            {
                UpdateEntityStamps(deleted, deleted.CDueId);
                await _countryLawService.DeleteCountryDue(deleted.CountryLawID, deleted.Country,deleted.CaseType,User.GetUserName(), deleted.ParentTStamp, new List<PatCountryDue>() { deleted });
            }
            return Ok();
        }
        #endregion

        #region CountryExp

        public async Task<IActionResult> CountryExpRead([DataSourceRequest] DataSourceRequest request, int parentId)
        {
            var result = (await _countryLawService.GetCountryExps(parentId)).ToDataSourceResult(request);
            return Json(result);
        }

        [Authorize(Policy = PatentAuthorizationPolicy.CountryLawModify)]
        public async Task<IActionResult> CountryExpUpdate([DataSourceRequest] DataSourceRequest request, int parentId, string country, string caseType, byte[] tStamp, [Bind(Prefix = "updated")]IList<PatCountryExp> updated,
                   [Bind(Prefix = "new")]IList<PatCountryExp> added, [Bind(Prefix = "deleted")]IList<PatCountryExp> deleted)
        {
            //no delete validation
            var canDelete = (await _authService.AuthorizeAsync(User, PatentAuthorizationPolicy.CountryLawCanDelete)).Succeeded;
            if (deleted.Any() && !canDelete)
                return Forbid("Not authorized.");

            if (!ModelState.IsValid)
                return new JsonBadRequest(new { errors = ModelState.Errors() });

            foreach (var item in updated)
            {
                UpdateEntityStamps(item, item.CExpId);
            }
            foreach (var item in added)
            {
                UpdateEntityStamps(item, item.CExpId);
            }
            await _countryLawService.UpdateChild(parentId, country,caseType, User.GetUserName(), tStamp, updated, added, deleted);
            return Ok();
        }

        [Authorize(Policy = PatentAuthorizationPolicy.CountryLawCanDelete)]
        public async Task<IActionResult> CountryExpDelete([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "deleted")] PatCountryExp deleted)
        {
            if (!ModelState.IsValid)
                return new JsonBadRequest(new { errors = ModelState.Errors() });

            if (deleted.CExpId > 0)
            {
                UpdateEntityStamps(deleted, deleted.CExpId);
                await _countryLawService.UpdateChild(deleted.CountryLawID, deleted.Country, deleted.CaseType, User.GetUserName(), deleted.ParentTStamp, new List<PatCountryExp>(), new List<PatCountryExp>(), new List<PatCountryExp>() { deleted });
            }
            return Ok();
        }
        #endregion

        #region DesCaseType

        public IActionResult DesCaseTypeRead([DataSourceRequest] DataSourceRequest request, string country, string caseType)
        {
            var result = _countryLawService.PatDesCaseTypes.ProjectTo<PatDesCaseTypeViewModel>().Where(d => d.IntlCode == country && d.CaseType == caseType).ToDataSourceResult(request);
            return Json(result);
        }

        [Authorize(Policy = PatentAuthorizationPolicy.CountryLawModify)]
        public async Task<IActionResult> DesCaseTypeUpdate([DataSourceRequest] DataSourceRequest request, int parentId, string country, string caseType, byte[] tStamp, [Bind(Prefix = "updated")]IList<PatDesCaseType> updated,
                   [Bind(Prefix = "new")]IList<PatDesCaseType> added, [Bind(Prefix = "deleted")]IList<PatDesCaseType> deleted)
        {
            //no delete validation
            var canDelete = (await _authService.AuthorizeAsync(User, PatentAuthorizationPolicy.CountryLawCanDelete)).Succeeded;
            if (deleted.Any() && !canDelete)
                return Forbid("Not authorized.");

            if (!ModelState.IsValid)
                return new JsonBadRequest(new { errors = ModelState.Errors() });

            foreach (var item in updated)
            {
                item.ChildCountry = null;
                item.ChildCaseType = null;
                item.ParentCaseType = null;
                item.ParentCountry = null;
                UpdateEntityStamps(item, item.DesCaseTypeID);
            }
            foreach (var item in added)
            {
                item.IntlCode = country;
                item.CaseType = caseType;
                item.GenApp = item.GenApp ?? false;
                item.DesCtryFieldID = 0;
                item.ChildCountry = null;
                UpdateEntityStamps(item, item.DesCaseTypeID);
            }
            await _countryLawService.UpdateChild(parentId,country,caseType, User.GetUserName(), tStamp, updated, added, deleted);
            return Ok();
        }

        [Authorize(Policy = PatentAuthorizationPolicy.CountryLawCanDelete)]
        public async Task<IActionResult> DesCaseTypeDelete([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "deleted")] PatDesCaseType deleted)
        {
            if (!ModelState.IsValid)
                return new JsonBadRequest(new { errors = ModelState.Errors() });

            if (deleted.DesCaseTypeID > 0)
            {
                UpdateEntityStamps(deleted, deleted.DesCaseTypeID); //for stamping of the header record
                await _countryLawService.UpdateChild(deleted.CountryLawID, deleted.IntlCode, deleted.CaseType, User.GetUserName(), deleted.ParentTStamp, new List<PatDesCaseType>(), new List<PatDesCaseType>(), new List<PatDesCaseType>() { deleted });
            }
            return Ok();
        }
        #endregion

        protected IQueryable<PatCountryLaw> PatCountryLaws => _countryLawService.PatCountryLaws;

        private async Task<DetailPageViewModel<PatCountryLawDetailViewModel>> PrepareEditScreen(int id)
        {
            var viewModel = new DetailPageViewModel<PatCountryLawDetailViewModel>
            {
                Detail = await GetById(id)
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

                viewModel.EditScreenUrl = Url.Action("Detail", new { id = id });
                viewModel.CopyScreenUrl = $"{viewModel.CopyScreenUrl}/{id}";
                viewModel.SearchScreenUrl = Url.Action("Index");
            }
            return viewModel;
        }

        private async Task<PatCountryLawDetailViewModel> GetById(int id)
        {
            var detail = await _countryLawService.PatCountryLaws.ProjectTo<PatCountryLawDetailViewModel>().FirstOrDefaultAsync(c => c.CountryLawID == id);
            detail.HasDesignatedCountries = await _countryLawService.HasDesignatedCountries(detail.Country, detail.CaseType);
            return detail;
        }

        public async Task<IActionResult> WebLinksRead([DataSourceRequest] DataSourceRequest request, int id, int displayChoice = 2)
        {
            var webLinks = await _webLinksService.GetWebLinks(id, "patcountrylaw", "FormLink", "");
            if (webLinks != null && displayChoice == 0)
                webLinks = webLinks.Where(w => w.RecordLink).ToList();

            var result = webLinks != null ? webLinks.ToDataSourceResult(request) : new Kendo.Mvc.UI.DataSourceResult();
            return Json(result);
        }

        public async Task<IActionResult> WebLinksUrl(int mainId, int id)
        {
            try
            {
                var url = await _webLinksService.GetWebLinksUrl(mainId, id, "patcountrylaw", "FormLink", "");
                return Json(new { url });
            }
            catch (ArgumentException ex)
            {
                return Json(new { url = "", error = ex.Message });
            }
        }

        [HttpGet()]
        public async Task<IActionResult> Copy(int id)
        {
            var entity = await _countryLawService.PatCountryLaws.FirstOrDefaultAsync(c => c.CountryLawID == id);
            if (entity == null) return new RecordDoesNotExistResult();
            var viewModel = new CountryLawCopyViewModel
            {
                CountryLawID = entity.CountryLawID,
                Country = entity.Country,
                CaseType = entity.CaseType,
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
                var source = await _countryLawService.PatCountryLaws.ProjectTo<PatCountryLawDetailViewModel>().FirstOrDefaultAsync(c => c.CountryLawID == copyOptions.CountryLawID);
                if (source != null)
                {
                    page.Detail = source;
                    page.Detail.CountryLawID = 0;
                    page.Detail.Country = copyOptions.Country;
                    page.Detail.CaseType = copyOptions.CaseType;
                    page.Detail.Remarks = copyOptions.CopyRemarks ? source.Remarks : "";
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
            if (countryName != null)
            {
                countryLaws = countryLaws.Where(c => EF.Functions.Like(c.PatCountry.CountryName, countryName.Value));
                mainSearchFilters.Remove(countryName);
            }

            var systemName = mainSearchFilters.FirstOrDefault(f => f.Property == "SystemName");
            if (systemName != null)
            {
                countryLaws = countryLaws.Where(c => c.Systems != null && EF.Functions.Like(c.Systems, "%" + systemName.Value + "%"));
                mainSearchFilters.Remove(systemName);
            }

            return countryLaws;
        }

        private async Task<CPiDataSourceResult> CreateViewModelForGrid(DataSourceRequest request, IQueryable<PatCountryLaw> countryLaws)
        {
            var model = countryLaws.ProjectTo<PatCountryLawSearchViewModel>();

            if (request.Sorts != null && request.Sorts.Any())
                model = model.ApplySorting(request.Sorts);
            else
                model = model.OrderBy(c => c.CountryName).ThenBy(c => c.CaseType);

            var ids = await model.Select(c => c.CountryLawID).ToArrayAsync();

            return new CPiDataSourceResult()
            {
                Data = await model.ApplyPaging(request.Page, request.PageSize).ToListAsync(),
                Total = ids.Length,
                Ids = ids
            };
        }
    }
}