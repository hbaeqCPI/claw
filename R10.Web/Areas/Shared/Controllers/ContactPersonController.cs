using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kendo.Mvc.UI;
using R10.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using R10.Core.Entities;
using R10.Core.Interfaces;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;
using R10.Web.Extensions.ActionResults;
using R10.Web.Helpers;
using R10.Web.Interfaces;
using R10.Web.Models.PageViewModels;
using R10.Web.Security;
using R10.Web.Services;
using R10.Core.Identity;
using R10.Core.Helpers;
using R10.Core.Entities.Shared;
using R10.Core.Services.Shared;
using R10.Web.Areas;

namespace R10.Web.Areas.Shared.Controllers
{
    [Area("Shared"), Authorize(Policy = SharedAuthorizationPolicy.CanAccessSystem)]
    public class ContactPersonController : BaseController
    {
        private readonly IAuthorizationService _authService;
        private readonly IViewModelService<ContactPerson> _viewModelService;
        private readonly IContactPersonService _entityService;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly ICountryLookupViewModelService _countryLookupService;
        private readonly IReportService _reportService;
        private readonly ISystemSettings<DefaultSetting> _defaultSettings;

        private readonly string _dataContainer = "contactPersonDetail";

        public ContactPersonController(IAuthorizationService authService, IViewModelService<ContactPerson> contactPersonViewModelService, IContactPersonService entityService,
            IStringLocalizer<SharedResource> localizer,
            ICountryLookupViewModelService countryLookupService,
            IReportService reportService,
            ISystemSettings<DefaultSetting> defaultSettings)
        {
            _authService = authService;
            _viewModelService = contactPersonViewModelService;
            _entityService = entityService;
            _localizer = localizer;
            _countryLookupService = countryLookupService;
            _reportService = reportService;
            _defaultSettings = defaultSettings;
        }

        public async Task<IActionResult> Index()
        {
            var model = new PageViewModel()
            {
                Page = PageType.Search,
                PageId = "contactPersonSearch",
                Title = _localizer["Contact Person Search"].ToString(),
                CanAddRecord = (await _authService.AuthorizeAsync(User, SharedAuthorizationPolicy.FullModify)).Succeeded
            };

            var settings = await _defaultSettings.GetSetting();
            if (settings.IsShowCustomFieldOn)
            {
                ViewBag.SysCustomFieldSettings = await _entityService.GetCustomFields();
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
                PageId = "contactPersonSearchResults",
                Title = _localizer["Contact Person Search Results"].ToString(),
                CanAddRecord = (await _authService.AuthorizeAsync(User, SharedAuthorizationPolicy.FullModify)).Succeeded
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
                var contactPersons = _entityService.QueryableList;

                var hasUserAccount = mainSearchFilters.FirstOrDefault(f => f.Property == "HasUserAccount");
                if (hasUserAccount != null)
                {
                    contactPersons = contactPersons.Where(cp => cp.EntityFilters.Any(ef => ef.EntityId == cp.ContactID && ef.CPiUser.UserType == CPiUserType.ContactPerson));
                    mainSearchFilters.Remove(hasUserAccount);
                }

                var isDMSReviewer = mainSearchFilters.FirstOrDefault(f => f.Property == "IsDMSReviewer");
                if (isDMSReviewer != null)
                {
                    contactPersons = contactPersons.Where(cp => cp.EntityFilters.Any(ef => ef.EntityId == cp.ContactID && ef.CPiUser.UserType == CPiUserType.ContactPerson && ef.CPiUser.CPiUserSystemRoles.Any(usr => usr.CPiSystem.IsEnabled && usr.SystemId == SystemType.DMS && usr.RoleId == "Reviewer")));
                    mainSearchFilters.Remove(isDMSReviewer);
                }

                var isAMSDecisionMaker = mainSearchFilters.FirstOrDefault(f => f.Property == "IsAMSDecisionMaker");
                if (isAMSDecisionMaker != null)
                {
                    contactPersons = contactPersons.Where(cp => cp.EntityFilters.Any(ef => ef.EntityId == cp.ContactID && ef.CPiUser.UserType == CPiUserType.ContactPerson && ef.CPiUser.CPiUserSystemRoles.Any(usr => usr.CPiSystem.IsEnabled && usr.SystemId == SystemType.AMS && usr.RoleId == "DecisionMaker")));
                    mainSearchFilters.Remove(isAMSDecisionMaker);
                }

                var isTmkSearchReviewer = mainSearchFilters.FirstOrDefault(f => f.Property == "IsTmkSearchReviewer");
                if (isTmkSearchReviewer != null)
                {
                    contactPersons = contactPersons.Where(cp => cp.EntityFilters.Any(ef => ef.EntityId == cp.ContactID && ef.CPiUser.UserType == CPiUserType.ContactPerson && ef.CPiUser.CPiUserSystemRoles.Any(usr => usr.CPiSystem.IsEnabled && usr.SystemId == SystemType.SearchRequest && usr.RoleId == "Reviewer")));
                    mainSearchFilters.Remove(isTmkSearchReviewer);
                }

                var isPatClearanceReviewer = mainSearchFilters.FirstOrDefault(f => f.Property == "IsPatClearanceReviewer");
                if (isPatClearanceReviewer != null)
                {
                    contactPersons = contactPersons.Where(cp => cp.EntityFilters.Any(ef => ef.EntityId == cp.ContactID && ef.CPiUser.UserType == CPiUserType.ContactPerson && ef.CPiUser.CPiUserSystemRoles.Any(usr => usr.CPiSystem.IsEnabled && usr.SystemId == SystemType.PatClearance && usr.RoleId == "Reviewer")));
                    mainSearchFilters.Remove(isPatClearanceReviewer);
                }

                var isRMSDecisionMaker = mainSearchFilters.FirstOrDefault(f => f.Property == "IsRMSDecisionMaker");
                if (isRMSDecisionMaker != null)
                {
                    contactPersons = contactPersons.Where(cp => cp.EntityFilters.Any(ef => ef.EntityId == cp.ContactID && ef.CPiUser.UserType == CPiUserType.ContactPerson && ef.CPiUser.CPiUserSystemRoles.Any(usr => usr.CPiSystem.IsEnabled && usr.SystemId == SystemType.RMS && usr.RoleId == "DecisionMaker")));
                    mainSearchFilters.Remove(isRMSDecisionMaker);
                }

                var isFFDecisionMaker = mainSearchFilters.FirstOrDefault(f => f.Property == "IsFFDecisionMaker");
                if (isFFDecisionMaker != null)
                {
                    contactPersons = contactPersons.Where(cp => cp.EntityFilters.Any(ef => ef.EntityId == cp.ContactID && ef.CPiUser.UserType == CPiUserType.ContactPerson && ef.CPiUser.CPiUserSystemRoles.Any(usr => usr.CPiSystem.IsEnabled && usr.SystemId == SystemType.ForeignFiling && usr.RoleId == "DecisionMaker")));
                    mainSearchFilters.Remove(isFFDecisionMaker);
                }

                var initial = mainSearchFilters.FirstOrDefault(f => f.Property == "Initial");
                if (initial != null)
                {
                    contactPersons = contactPersons.Where(cp => !string.IsNullOrEmpty(cp.LastName) && cp.LastName.StartsWith(initial.Value));
                    mainSearchFilters.Remove(initial);
                }

                contactPersons = _viewModelService.AddCriteria(contactPersons, mainSearchFilters);

                //var result = _viewModelService.CreateViewModelForGrid<ContactSearchResultViewModel>(request, contactPersons,"Contact","ContactID");
                var result = await _viewModelService.CreateViewModelForGrid(request, contactPersons, "Contact", "ContactID");
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
        public async Task<IActionResult> DetailLink(int id)
        {
            if (id > 0)
            {
                var entity = await _entityService.QueryableList.Where(c => c.ContactID == id).FirstOrDefaultAsync();
                if (entity == null)
                    return new RecordDoesNotExistResult();
                else
                    return RedirectToAction(nameof(Detail), new { id = entity.ContactID, singleRecord = true, fromSearch = true });
            }
            else
                return RedirectToAction(nameof(Add), new { fromSearch = true });
        }

        public async Task<IActionResult> Detail(int id, bool singleRecord = false, bool fromSearch = false, string tab = "")
        {
            var page = await PrepareEditScreen(id);
            if (page.Detail == null)
            {
                if (Request.IsAjax())
                    return new RecordDoesNotExistResult();
                else
                    return RedirectToAction("Index");
            }

            var detail = page.Detail;
            PageViewModel model = new PageViewModel()
            {
                Page = PageType.Detail,
                PageId = page.Container,
                Title = _localizer["Contact Person Detail"].ToString(),
                RecordId = detail.ContactID,
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
            ViewBag.DownloadName = "Contact Person Print Screen";
            return View();
        }

        [HttpPost]
        public IActionResult Print([FromBody] PrintViewModel contactPersonPrintModel)
        {
            if (ModelState.IsValid)
            {
                return _reportService.GetReport(contactPersonPrintModel, ReportType.SharedContactPersonPrintScreen).Result;
            }

            return BadRequest("Unhandled error.");
        }

        [Authorize(Policy = SharedAuthorizationPolicy.FullModify)]
        public async Task<IActionResult> Add(bool fromSearch = false)
        {
            if (!Request.IsAjax())
                return RedirectToAction("Index");

            var page = await PrepareAddScreen();
            if (page.Detail == null)
                return RedirectToAction("Index");

            var detail = page.Detail;
            PageViewModel model = new PageViewModel()
            {
                Page = fromSearch ? PageType.Detail : PageType.DetailContent,
                PageId = page.Container,
                Title = _localizer["New Contact Person"].ToString(),
                RecordId = detail.ContactID,
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
            var entity = await _entityService.GetByIdAsync(id);

            if (entity == null)
                return new RecordDoesNotExistResult();

            entity.tStamp = Convert.FromBase64String(tStamp);
            await _entityService.Delete(entity);

            return Ok();
        }

        [HttpPost, Authorize(Policy = SharedAuthorizationPolicy.FullModify)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save([FromBody] ContactPerson contactPerson)
        {
            if (ModelState.IsValid)
            {
                UpdateEntityStamps(contactPerson, contactPerson.ContactID);

                if (string.IsNullOrEmpty(contactPerson.LastName) && string.IsNullOrEmpty(contactPerson.FirstName))
                    contactPerson.LastName = contactPerson.Contact;

                if (contactPerson.ContactID > 0)
                    await _entityService.Update(contactPerson);
                else
                    await _entityService.Add(contactPerson);

                return Json(contactPerson.ContactID);
            }
            else
                return new JsonBadRequest(new { errors = ModelState.Errors() });
        }

        [HttpPost, Authorize(Policy = SharedAuthorizationPolicy.RemarksOnlyModify)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveRemarks([FromBody] ContactPerson contactPerson)
        {
            UpdateEntityStamps(contactPerson, contactPerson.ContactID);
            await _entityService.UpdateRemarks(contactPerson);
            return Json(contactPerson.ContactID);
        }

        [Authorize(Policy = SharedAuthorizationPolicy.FullModify)]
        public async Task<IActionResult> Copy(int id)
        {
            if (!Request.IsAjax())
                return RedirectToAction("Index");

            var page = await PrepareEditScreen(id);
            if (page.Detail == null)
                return RedirectToAction("Index");

            page.Detail.ContactID = 0;
            page.Detail.Contact = null;
            //page.Detail.LastName = null;
            //page.Detail.FirstName = null;
            //page.Detail.MiddleInitial = null;
            page.Detail.UserId = null;
            page.Detail.IsDMSReviewer = false;
            page.Detail.IsAMSDecisionMaker = false;
            page.Detail.IsTmkSearchReviewer = false;
            page.Detail.IsPatClearanceReviewer = false;
            page.Detail.IsRMSDecisionMaker = false;
            page.Detail.IsFFDecisionMaker = false;


            var detail = page.Detail;
            PageViewModel model = new PageViewModel()
            {
                Page = PageType.DetailContent,
                PageId = page.Container,
                Title = _localizer["New Contact Person"].ToString(),
                RecordId = detail.ContactID,
                PagePermission = page,
                Data = detail,
                FromSearch = false
            };

            return PartialView("Index", model);

        }

        public async Task<IActionResult> GetRecordStamps(int id)
        {
            var contactPerson = await _entityService.GetByIdAsync(id);
            if (contactPerson == null)
                return new NoRecordFoundResult();

            return ViewComponent("RecordStamps", new { createdBy = contactPerson.CreatedBy, dateCreated = contactPerson.DateCreated, updatedBy = contactPerson.UpdatedBy, lastUpdate = contactPerson.LastUpdate, tStamp = contactPerson.tStamp });
        }

        private async Task<DetailPageViewModel<ContactPersonViewModel>> PrepareEditScreen(int id)
        {
            var viewModel = new DetailPageViewModel<ContactPersonViewModel>();
            viewModel.Detail = await _entityService.QueryableList.ProjectTo<ContactPersonViewModel>().FirstOrDefaultAsync(c => c.ContactID == id);

            viewModel.Detail.IsReviewer = false;

            if (viewModel.Detail != null)
            {
                viewModel.AddSharedSecurityPolicies();
                await viewModel.ApplyDetailPagePermission(User, _authService);

                //hide email button
                viewModel.CanEmail = false;

                this.AddDefaultNavigationUrls(viewModel);

                viewModel.EditScreenUrl = $"{viewModel.EditScreenUrl}/{id}";
                viewModel.CopyScreenUrl = $"{viewModel.CopyScreenUrl}/{id}";
                viewModel.SearchScreenUrl = this.Url.Action("Index");
                viewModel.IsCopyScreenPopup = false;
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
                            Url = Url.Action("AddContact", "User", new { area = "Admin", id = viewModel.Detail.ContactID }),
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

                var setting = await _defaultSettings.GetSetting();
                if (setting.IsShowCustomFieldOn)
                {
                    viewModel.Detail.SysCustomFieldSettings = await _entityService.GetCustomFields();
                }
            }
            return viewModel;
        }

        private async Task<DetailPageViewModel<ContactPersonViewModel>> PrepareAddScreen()
        {
            var viewModel = new DetailPageViewModel<ContactPersonViewModel>();
            viewModel.Detail = new ContactPersonViewModel();

            viewModel.AddSharedSecurityPolicies();
            await viewModel.ApplyDetailPagePermission(User, _authService);

            this.AddDefaultNavigationUrls(viewModel);
            viewModel.Container = _dataContainer;

            var setting = await _defaultSettings.GetSetting();
            if (setting.IsShowCustomFieldOn)
            {
                viewModel.Detail.SysCustomFieldSettings = await _entityService.GetCustomFields();
            }
            return viewModel;
        }

        public async Task<IActionResult> GetPicklistData([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType, string requiredRelation = "")
        {
            return await GetPicklistData(_entityService.QueryableList, request, property, text, filterType, requiredRelation);
        }

        public async Task<IActionResult> GetContactList([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType, string requiredRelation = "")
        {
            var contacts = _entityService.QueryableList.Where(c => (bool)c.IsActive);
            return await GetPicklistData(contacts, request, property, text, filterType, new string[] { "ContactID", "Contact", "ContactName" }, requiredRelation);
        }

        public async Task<IActionResult> GetCountryList([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType)
        {
            var countries = _countryLookupService.Countries
                                    .Where(w => _entityService.QueryableList.Any(a => a.Country == w.Country))
                                    .Distinct().OrderBy(property)
                                    .BuildCriteria(property, text, filterType);
            return await GetPicklistData(countries, request);
        }
    }
}
