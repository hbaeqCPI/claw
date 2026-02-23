using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using R10.Core.Interfaces;
using R10.Web.Helpers;
using R10.Web.Extensions;
using R10.Web.Security;
using Kendo.Mvc.UI;
using R10.Web.Areas.Shared.ViewModels;
using R10.Core.Services.Shared;
using R10.Web.Extensions.ActionResults;
using R10.Core.Entities;
using R10.Web.Interfaces;
using Microsoft.EntityFrameworkCore;   
using AutoMapper.QueryableExtensions;
using Kendo.Mvc.Extensions;
using R10.Core.Identity;
using System.Linq.Expressions;
using R10.Web.Models;
using Microsoft.Extensions.Localization;
using R10.Web.Models.PageViewModels;
using R10.Core.Helpers;
using R10.Core;
using R10.Core.Exceptions;
using R10.Web.Services;
using R10.Core.Entities.Shared;

using R10.Web.Areas;

namespace R10.Web.Areas.Shared.Controllers
{
    [Area("Shared"), Authorize(Policy = SharedAuthorizationPolicy.CanAccessSystem)]
    public class AttorneyController : BaseController
    {
        private readonly IAuthorizationService _authService;
        private readonly IViewModelService<Attorney> _attorneyViewModelService;
        private readonly IAttorneyService _attorneyService;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly ICountryLookupViewModelService _countryLookupService;
        private readonly IReportService _reportService;
        private readonly ISystemSettings<DefaultSetting> _settings;

        private readonly string _dataContainer = "attorneyDetail";
        
        public AttorneyController(
            IAuthorizationService authService, 
            IViewModelService<Attorney> attorneyViewModelService,
            IAttorneyService attorneyService,
            IStringLocalizer<SharedResource> localizer,
            ICountryLookupViewModelService countryLookupService,
            IReportService reportService,
            ISystemSettings<DefaultSetting> settings
            )
        {
            _authService = authService;
            _attorneyViewModelService = attorneyViewModelService;
            _attorneyService = attorneyService;
            _localizer = localizer;
            _countryLookupService = countryLookupService;
            _reportService = reportService;
            _settings = settings;
        }

        public async Task<IActionResult> Index()
        {
            var model = new PageViewModel()
            {
                Page = PageType.Search,
                PageId = "attorneySearch",
                Title = _localizer["Attorney Search"].ToString(),
                CanAddRecord = await CanAddRecord()
            };

            var settings = await _settings.GetSetting();
            if (settings.IsShowCustomFieldOn)
            {
                ViewBag.SysCustomFieldSettings = await _attorneyService.GetCustomFields();
            }

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
                PageId = "attorneySearchResults",
                Title = _localizer["Attorney Search Results"].ToString(),
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
                var attorneys = this.Attorneys;

                var hasUserAccount = mainSearchFilters.FirstOrDefault(f => f.Property == "HasUserAccount");
                if (hasUserAccount != null)
                {
                    attorneys = attorneys.Where(a => a.EntityFilters.Any(ef => ef.EntityId == a.AttorneyID && ef.CPiUser.UserType == CPiUserType.Attorney));
                    mainSearchFilters.Remove(hasUserAccount);
                }

                var isPatentUser = mainSearchFilters.FirstOrDefault(f => f.Property == "IsPatentUser");
                if (isPatentUser != null)
                {
                    attorneys = attorneys.Where(i => i.EntityFilters.Any(ef => ef.EntityId == i.AttorneyID && ef.CPiUser.UserType == CPiUserType.Attorney && ef.CPiUser.CPiUserSystemRoles.Any(usr => usr.CPiSystem.IsEnabled && usr.SystemId == SystemType.Patent)));
                    mainSearchFilters.Remove(isPatentUser);
                }

                var isTrademarkUser = mainSearchFilters.FirstOrDefault(f => f.Property == "IsTrademarkUser");
                if (isTrademarkUser != null)
                {
                    attorneys = attorneys.Where(i => i.EntityFilters.Any(ef => ef.EntityId == i.AttorneyID && ef.CPiUser.UserType == CPiUserType.Attorney && ef.CPiUser.CPiUserSystemRoles.Any(usr => usr.CPiSystem.IsEnabled && usr.SystemId == SystemType.Trademark)));
                    mainSearchFilters.Remove(isTrademarkUser);
                }

                var isGeneralMatterUser = mainSearchFilters.FirstOrDefault(f => f.Property == "IsGeneralMatterUser");
                if (isGeneralMatterUser != null)
                {
                    attorneys = attorneys.Where(i => i.EntityFilters.Any(ef => ef.EntityId == i.AttorneyID && ef.CPiUser.UserType == CPiUserType.Attorney && ef.CPiUser.CPiUserSystemRoles.Any(usr => usr.CPiSystem.IsEnabled && usr.SystemId == SystemType.GeneralMatter)));
                    mainSearchFilters.Remove(isGeneralMatterUser);
                }

                attorneys = _attorneyViewModelService.AddCriteria(attorneys, mainSearchFilters);
                var result = await _attorneyViewModelService.CreateViewModelForGrid<AttorneySearchResultViewModel>(request, attorneys, "AttorneyCode", "AttorneyID");
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
        public async Task<IActionResult> DetailLink(int? id, string code)
        {
            if (id == null && !string.IsNullOrEmpty(code))
                id = await Attorneys.Where(a => a.AttorneyCode == code).Select(a => a.AttorneyID).FirstOrDefaultAsync();

            if (id > 0)
                return RedirectToAction(nameof(Detail), new { id = id, singleRecord = true, fromSearch = true });
            else if ((await _authService.AuthorizeAsync(User, SharedAuthorizationPolicy.FullModify)).Succeeded)
                return RedirectToAction(nameof(Add), new { fromSearch = true, code = code });
            else
                return new RecordDoesNotExistResult();
        }

        public async Task<IActionResult> Detail(int id, bool singleRecord = false, bool fromSearch = false, string tab = "")
        {
            var page = await PrepareEditScreen(this.Attorneys, id);
            if (page.Detail == null)
            {
                Guard.Against.NoRecordPermission(!Request.IsAjax());
                return RedirectToAction("Index");
            }

            var detail = page.Detail;
            PageViewModel model = new PageViewModel()
            {
                Page = PageType.Detail,
                PageId = page.Container,
                Title = _localizer["Attorney Detail"].ToString(),
                RecordId = detail.AttorneyID,
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
            ViewBag.DownloadName = "Attorney Print Screen";
            return View();
        }

        [HttpPost]
        public IActionResult Print([FromBody] PrintViewModel attorneyPrintModel)
        {
            if (ModelState.IsValid)
            {
                return _reportService.GetReport(attorneyPrintModel, ReportType.SharedAttorneyPrintScreen).Result;
            }

            return BadRequest("Unhandled error.");
        }

        [Authorize(Policy = SharedAuthorizationPolicy.FullModify)]
        public async Task<IActionResult> Add(bool fromSearch = false, string code = "")
        {
            if (!Request.IsAjax())
                return RedirectToAction("Index");

            //do not allow add if user entity filter type is attorney
            //user won't be able to access new record
            Guard.Against.UnAuthorizedAccess(await CanAddRecord());

            var page = await PrepareAddScreen();
            if (page.Detail == null)
                return RedirectToAction("Index");
            else if (!string.IsNullOrEmpty(code))
                page.Detail.AttorneyCode = code;

            var detail = page.Detail;
            PageViewModel model = new PageViewModel()
            {
                Page = fromSearch ? PageType.Detail : PageType.DetailContent,
                PageId = page.Container,
                Title = _localizer["New Attorney"].ToString(),
                RecordId = detail.AttorneyID,
                PagePermission = page,
                Data = detail,
                FromSearch = fromSearch
            };

            return PartialView("Index", model);
        }

        [HttpPost, Authorize(Policy = SharedAuthorizationPolicy.CanDelete)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, string tStamp)
        {
            var atty = await _attorneyService.GetByIdAsync(id); //we need the attycode as alternate key
            atty.tStamp = Convert.FromBase64String(tStamp);
            await _attorneyService.Delete(atty);
            return Ok();
        }

        [HttpPost, Authorize(Policy = SharedAuthorizationPolicy.FullModify)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save([FromBody] Attorney attorney)
        {
            if (ModelState.IsValid)
            {
                UpdateEntityStamps(attorney, attorney.AttorneyID);

                if (attorney.AttorneyID > 0)
                    await _attorneyService.Update(attorney);
                else
                    await _attorneyService.Add(attorney);

                return Json(attorney.AttorneyID);
            }
            else
                return new JsonBadRequest(new { errors = ModelState.Errors() });
        }

        [HttpPost, Authorize(Policy = SharedAuthorizationPolicy.RemarksOnlyModify)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveRemarks([FromBody] Attorney attorney)
        {
            UpdateEntityStamps(attorney, attorney.AttorneyID);
            await _attorneyService.UpdateRemarks(attorney);
            return Json(attorney.AttorneyID);
        }

        public async Task<IActionResult> GetRecordStamps(int id)
        {
            var attorney = await _attorneyService.GetByIdAsync(id);
            if (attorney == null)
                return new NoRecordFoundResult();

            return ViewComponent("RecordStamps", new { createdBy = attorney.CreatedBy, dateCreated = attorney.DateCreated, updatedBy = attorney.UpdatedBy, lastUpdate = attorney.LastUpdate, tStamp = attorney.tStamp });
        }

        protected IQueryable<Attorney> Attorneys => _attorneyService.QueryableList;

        private async Task<DetailPageViewModel<AttorneyDetailViewModel>> PrepareEditScreen(IQueryable<Attorney> attorneys, int id)
        {
            var viewModel = new DetailPageViewModel<AttorneyDetailViewModel>();
            viewModel.Detail = await Attorneys.ProjectTo<AttorneyDetailViewModel>().FirstOrDefaultAsync(a => a.AttorneyID == id);
    
            if (viewModel.Detail != null)
            {
                viewModel.AddSharedSecurityPolicies();
                await viewModel.ApplyDetailPagePermission(User, _authService);

                //do not allow add if user entity filter type is attorney
                //user won't be able to access new record
                viewModel.CanAddRecord = await CanAddRecord();

                //hide copy and email buttons
                viewModel.CanCopyRecord = false;
                viewModel.CanEmail = false;

                this.AddDefaultNavigationUrls(viewModel);

                viewModel.EditScreenUrl = $"{viewModel.EditScreenUrl}/{id}";
                viewModel.SearchScreenUrl = this.Url.Action("Index");
                viewModel.Container = _dataContainer;

                if (User.IsAdmin())
                {
                    var pageActions = new List<DetailPageAction>();

                    if (string.IsNullOrEmpty(viewModel.Detail.UserId))
                    {
                        pageActions.Add(new DetailPageAction()
                        {
                            Label = _localizer["Create User Account"].ToString(),
                            Class = "cpiButtonLink",
                            IconClass = "fal fa-user-plus",
                            Url = Url.Action("AddAttorney", "User", new { area = "Admin", id = viewModel.Detail.AttorneyID }),
                        });
                    }
                    else
                    {
                        pageActions.Add(new DetailPageAction()
                        {
                            Label = _localizer["View User Account"].ToString(),
                            Class = "cpiButtonLink",
                            IconClass = "fal fa-user",
                            Url = Url.Action("DetailLink", "User", new { area = "Admin", id = viewModel.Detail.UserId, show = 3 }),
                        });

                    }

                    viewModel.PageActions = pageActions;                    
                }
                var setting = await _settings.GetSetting();
                if (setting.IsShowCustomFieldOn)
                {
                    viewModel.Detail.SysCustomFieldSettings = await _attorneyService.GetCustomFields();
                }
            }
            return viewModel;
        }

        private async Task<DetailPageViewModel<AttorneyDetailViewModel>> PrepareAddScreen()
        {
            var viewModel = new DetailPageViewModel<AttorneyDetailViewModel>();
            viewModel.Detail = new AttorneyDetailViewModel();

            viewModel.AddSharedSecurityPolicies();
            await viewModel.ApplyDetailPagePermission(User, _authService);

            this.AddDefaultNavigationUrls(viewModel);
            viewModel.Container = _dataContainer;
            var setting = await _settings.GetSetting();
            if (setting.IsShowCustomFieldOn)
            {
                viewModel.Detail.SysCustomFieldSettings = await _attorneyService.GetCustomFields();
            }
            return viewModel;
        }

        private async Task<bool> CanAddRecord()
        {
            //do not allow add if user entity filter type is attorney
            //user won't be able to access new record
            return (await _authService.AuthorizeAsync(User, SharedAuthorizationPolicy.FullModify)).Succeeded &&
                User.GetEntityFilterType() != CPiEntityType.Attorney;
        }

        public async Task<IActionResult> GetPicklistData([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType, string requiredRelation = "")
        {
            return await GetPicklistData(Attorneys, request, property, text, filterType, requiredRelation);
        }

        //TODO: USE GetAttorneyList
        public async Task<IActionResult> GetAttorneysList(string property, string text, FilterType filterType, string requiredRelation = "")
        {
            var attorneys = this.Attorneys;

            attorneys = QueryHelper.BuildCriteria(attorneys, property, text, filterType, requiredRelation).OrderBy(property);
            var list = attorneys.Select(a => new { AttorneyID = a.AttorneyID, AttorneyCode = a.AttorneyCode, AttorneyName = a.AttorneyName });
            return Json(await list.ToListAsync());
        }

        public async Task<IActionResult> GetAttorneyList([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType, string requiredRelation = "")
        {
            var attorneys = Attorneys.Where(c => (bool)c.IsActive);
            return await GetPicklistData(attorneys, request, property, text, filterType, new string[] { "AttorneyID", "AttorneyCode", "AttorneyName" }, requiredRelation);
            //requiredRelation won't work if already projected to viewmodel
            //return await GetPicklistData(Attorneys.ProjectTo<AttorneyListViewModel>(), request, property, text, filterType, requiredRelation, false);
        }

        public async Task<IActionResult> GetAllActiveAttorneyList([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType, string requiredRelation = "")
        {
            var attorneys = Attorneys.Where(c => (bool)c.IsActive);
            return Json(await attorneys.Select(c=> new { AttorneyID = c.AttorneyID, DueDateAttorneyCode = c.AttorneyCode, DueDateAttorneyName = c.AttorneyName}).OrderBy(c=>c.DueDateAttorneyName).ToListAsync());
        }

        //todo: move to dmsdisclosure controller
        public async Task<IActionResult> GetDMSDisclosureAttorneysList(string textProperty, string text, FilterType filterType, string requiredRelation = "")
        {
            var attorneys = this.Attorneys;
            attorneys = attorneys.Where(a => a.AttorneyDisclosures.Any());
            var list = attorneys.Select(a => new { AttorneyID = a.AttorneyID, AttorneyCode = a.AttorneyCode, AttorneyName = a.AttorneyName });
            return Json(await list.ToListAsync());
        }

        //public IActionResult GetApplicationAttorneysList(string textProperty, string text, FilterType filterType, string requiredRelation = "")
        //{
        //    var attorneys = _repository.QueryableList;
        //    attorneys = attorneys.Where(a => a.Attorney1Inventions.Any(i=> i.CountryApplications.Any()) || a.Attorney2Inventions.Any(i => i.CountryApplications.Any()) || a.Attorney3Inventions.Any(i => i.CountryApplications.Any()));
        //    attorneys = QueryHelper.BuildCriteria(attorneys, textProperty, text, filterType, requiredRelation);
        //    var list = attorneys.Select(a => new { AttorneyID = a.AttorneyID, AttorneyCode = a.AttorneyCode, AttorneyName = a.AttorneyName }).ToList();
        //    return Json(list);
        //}

        //todo: move to trademark controller
        public async Task<IActionResult> GetTrademarkAttorneysList(string textProperty, string text, FilterType filterType, string requiredRelation = "")
        {
            var attorneys = this.Attorneys;
            attorneys = attorneys.Where(a => a.Attorney1Trademarks.Any() || a.Attorney2Trademarks.Any() || a.Attorney3Trademarks.Any());
            attorneys = QueryHelper.BuildCriteria(attorneys, textProperty, text, filterType, requiredRelation);
            var list = attorneys.Select(a => new { AttorneyID = a.AttorneyID, AttorneyCode = a.AttorneyCode, AttorneyName = a.AttorneyName });
            return Json(await list.ToListAsync());
        }

        public async Task<IActionResult> GetCountryList([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType)
        {
            var countries = _countryLookupService.Countries
                                    .Where(w => Attorneys.Any(a => a.Country == w.Country))
                                    .Distinct().OrderBy(property)
                                    .BuildCriteria(property, text, filterType);
            return await GetPicklistData(countries, request);
        }

        public async Task<IActionResult> GetPOCountryList([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType)
        {
            var countries = _countryLookupService.Countries
                                    .Where(w => Attorneys.Any(a => a.POCountry == w.Country))
                                    .Distinct().OrderBy(property)
                                    .BuildCriteria(property, text, filterType);
            return await GetPicklistData(countries, request);
        }

        public async Task<IActionResult> GetClearanceAttorneyList([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType, string requiredRelation = "")
        {
            var attorneys = _attorneyService.ClearanceQueryableList.Where(c => (bool)c.IsActive);
            return await GetPicklistData(attorneys, request, property, text, filterType, new string[] { "AttorneyID", "AttorneyCode", "AttorneyName" }, requiredRelation);
            //requiredRelation won't work if already projected to viewmodel
            //return await GetPicklistData(Attorneys.ProjectTo<AttorneyListViewModel>(), request, property, text, filterType, requiredRelation, false);
        }
    } 
}