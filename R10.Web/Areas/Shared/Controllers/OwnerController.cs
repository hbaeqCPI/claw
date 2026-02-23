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
using R10.Core.Entities;
using R10.Core.Interfaces;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Helpers;
using R10.Web.Extensions;
using R10.Web.Extensions.ActionResults;
using R10.Web.Interfaces;
using R10.Web.Security;
using AutoMapper;
using System.Reflection;
using System.Text;
using R10.Web.Filters;
using System.Linq.Expressions;
using R10.Core.Services.Shared;
using R10.Core.Identity;
using R10.Web.Models;
using R10.Web.Models.PageViewModels;
using R10.Core.Helpers;
using R10.Core;
using R10.Core.Exceptions;
using AutoMapper.QueryableExtensions;
using R10.Core.Entities.Shared;
using R10.Web.Services;
using Kendo.Mvc;

namespace R10.Web.Areas.Shared.Controllers
{
    [Area("Shared"), Authorize(Policy = SharedAuthorizationPolicy.CanAccessSystem)]
    public class OwnerController :BaseController
    {
        private readonly IAuthorizationService _authService;
        private readonly IOwnerViewModelService _ownerViewModelService;
        private readonly IOwnerService _ownerService;
        //private readonly IEntitySyncService _entitySyncService;
        private readonly IMapper _mapper;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly ICountryLookupViewModelService _countryLookupService;
        private readonly ISystemSettings<DefaultSetting> _settings;
        private readonly IClientService _clientService;
        private readonly IReportService _reportService;

        private readonly string _dataContainer = "ownerDetail";

        public OwnerController(IAuthorizationService authService,IOwnerViewModelService ownerViewModelService, 
                               IOwnerService ownerService,
                              // IEntitySyncService entitySyncService,
                               IMapper mapper,
                               IStringLocalizer<SharedResource> localizer,
                               ISystemSettings<DefaultSetting> settings,
                               ICountryLookupViewModelService countryLookupService,
                               IClientService clientService,
                                IReportService reportService)
        {
            _authService = authService;
            _ownerViewModelService = ownerViewModelService;
            _ownerService = ownerService;
            //_entitySyncService = entitySyncService;
            _mapper = mapper;
            _localizer = localizer;
            _countryLookupService = countryLookupService;
            _settings = settings;
            _clientService = clientService;
            _reportService = reportService;
        }

        public async Task<IActionResult> Index()
        {
            var ownerLabel = (await _settings.GetSetting()).LabelOwner;

            var model = new PageViewModel()
            {
                Page = PageType.Search,
                PageId = "ownerSearch",
                Title = _localizer[$"{ownerLabel} Search"].ToString(),
                CanAddRecord = await CanAddRecord()
            };

            var settings = await _settings.GetSetting();
            if (settings.IsShowCustomFieldOn)
            {
                ViewBag.SysCustomFieldSettings = await _ownerService.GetCustomFields();
            }

            if (Request.IsAjax())
                return PartialView("Index", model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Search([FromBody] List<QueryFilterViewModel> mainSearchFilters)
        {
            var ownerLabel = (await _settings.GetSetting()).LabelOwner;

            var model = new PageViewModel()
            {
                Page = PageType.SearchResults,
                PageId = "ownerSearchResults",
                Title = _localizer[$"{ownerLabel} Search Results"].ToString(),
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
                var owners = _ownerViewModelService.AddCriteria(this.Owners, mainSearchFilters);
                var result = await _ownerViewModelService.CreateViewModelForGrid(request, owners);
                return Json(result);
            }
            return new JsonBadRequest(new { errors = ModelState.Errors() });
        }

        [HttpPost()]
        public IActionResult ExcelExportSave(string contentType, string base64, string fileName) {
            var fileContents = Convert.FromBase64String(base64);
            return File(fileContents, contentType, fileName);
        }

        [HttpGet()]
        public async Task<IActionResult> DetailLink(int? id,string code)
        {
            if (id == null && !string.IsNullOrEmpty(code))
                id = await Owners.Where(o => o.OwnerCode == code).Select(c => c.OwnerID).FirstOrDefaultAsync();

            if (id > 0)
                return RedirectToAction(nameof(Detail), new { id = id, singleRecord = true, fromSearch = true });
            else if ((await _authService.AuthorizeAsync(User, SharedAuthorizationPolicy.FullModify)).Succeeded)
                return RedirectToAction(nameof(Add), new { fromSearch = true });
            else
                return new RecordDoesNotExistResult();
        }

        public async Task<IActionResult> Detail(int id, bool singleRecord = false, bool fromSearch = false, string tab = "")
        {
            var page = await PrepareEditScreen(this.Owners, id);
            if (page.Detail == null)
            {
                Guard.Against.NoRecordPermission(!Request.IsAjax());
                return RedirectToAction("Index");
            }

            var ownerLabel = (await _settings.GetSetting()).LabelOwner;
            var detail = page.Detail;
            PageViewModel model = new PageViewModel()
            {
                Page = PageType.Detail,
                PageId = page.Container,
                Title = _localizer[$"{ownerLabel} Detail"].ToString(),
                RecordId = detail.OwnerID,
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
        public async Task<IActionResult> Print()
        {
            ViewBag.Url = Url.Action("Print");
            ViewBag.DownloadName = _localizer[(await _settings.GetSetting()).LabelOwner].ToString() + " Print Screen";
            return View();
        }

        [HttpPost]
        public IActionResult Print([FromBody] PrintViewModel ownerPrintModel)
        {
            if (ModelState.IsValid)
            {
                return _reportService.GetReport(ownerPrintModel, ReportType.SharedOwnerPrintScreen).Result;
            }

            return BadRequest("Unhandled error.");
        }

        [Authorize(Policy = SharedAuthorizationPolicy.FullModify)]
        public async Task<IActionResult> Add(bool fromSearch = false)
        {
            if (!Request.IsAjax())
                return RedirectToAction("Index");

            //do not allow add if user entity filter type is owner
            //user won't be able to access new record
            Guard.Against.UnAuthorizedAccess(await CanAddRecord());

            var page = await PrepareAddScreen();
            if (page.Detail == null)
                return RedirectToAction("Index");

            var ownerLabel = (await _settings.GetSetting()).LabelOwner;
            var detail = page.Detail;
            PageViewModel model = new PageViewModel()
            {
                Page = fromSearch ? PageType.Detail : PageType.DetailContent,
                PageId = page.Container,
                Title = _localizer[$"New {ownerLabel}"].ToString(),
                RecordId = detail.OwnerID,
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
            await _ownerService.Delete(new Owner { OwnerID = id, tStamp = Convert.FromBase64String(tStamp) });
            return Ok();
        }

        [HttpPost, Authorize(Policy = SharedAuthorizationPolicy.FullModify)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save([FromBody] Owner owner)
        {
            if (ModelState.IsValid)
            {
                UpdateEntityStamps(owner, owner.OwnerID);

                if (owner.OwnerID > 0)
                    await _ownerService.Update(owner);
                else
                    await _ownerService.Add(owner);

                return Json(owner.OwnerID);
            }
            else
            {
                return new JsonBadRequest(new { errors = ModelState.Errors() });
            }
        }

        [HttpPost, Authorize(Policy = SharedAuthorizationPolicy.RemarksOnlyModify)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveRemarks([FromBody] Owner owner)
        {
            UpdateEntityStamps(owner, owner.OwnerID);
            await _ownerService.UpdateRemarks(owner);
            return Json(owner.OwnerID);
        }

        [Authorize(Policy = SharedAuthorizationPolicy.FullModify)]
        public IActionResult SyncToClientPrompt()
        {
            if (!Request.IsAjax())
                return RedirectToAction(nameof(Search));

            ViewBag.Url = Request.Path;
            return PartialView("_EntityDataSync", 4);
        }

        public async Task<IActionResult> OwnerWithClientSyncRead([DataSourceRequest] DataSourceRequest request, string searchText)
        {
            if (ModelState.IsValid)
            {
                var clients = _clientService.QueryableList.Where(c => this.Owners.Any(o => o.OwnerCode == c.ClientCode) && (string.IsNullOrEmpty(searchText) || c.ClientCode.Contains(searchText) || c.ClientName.Contains(searchText)));
                var model = clients.ProjectTo<ClientSearchResultViewModel>();
                var result = await model.ToDataSourceResultAsync(request);
                return Json(result);
            }
            return new JsonBadRequest(new { errors = ModelState.Errors() });
        }

        public async Task<IActionResult> AddOwnerToClientRead([DataSourceRequest] DataSourceRequest request)
        {
            if (ModelState.IsValid)
            {
                var owners = this.Owners.Where(o => !_clientService.QueryableList.Any(c => c.ClientCode == o.OwnerCode));
                var result = await _ownerViewModelService.CreateViewModelForGrid(request, owners);
                return Json(result);
            }
            return new JsonBadRequest(new { errors = ModelState.Errors() });
        }

        public async Task<IActionResult> AddOwnerToClient(int id)
        {
            Guard.Against.UnAuthorizedAccess(User.GetEntityFilterType() != CPiEntityType.Client);

            await _ownerService.AddOwnerToClient(new int[] {id });
            return Ok();
        }

        public async Task<IActionResult> SyncOwnerWithClient(int[] ids)
        {
            Guard.Against.UnAuthorizedAccess(User.GetEntityFilterType() != CPiEntityType.Client);

            if (ids.Length > 0)
            {
                await _ownerService.SyncOwnerWithClient(ids);
            }
            return Ok();
        }

        private async Task<Client> CreateClient(string ownerCode)
        {
            var client = await _ownerService.QueryableList.Where(o => o.OwnerCode == ownerCode).Include(o => o.OwnerContacts).ProjectTo<Client>().SingleOrDefaultAsync();

            if (client == null)
                return null;

            var clientId = await _clientService.QueryableList.Where(c => c.ClientCode == ownerCode).Select(c => c.ClientID).FirstOrDefaultAsync();

            client.ClientID = clientId;
            UpdateEntityStamps(client, clientId);

            foreach (var contact in client.ClientContacts)
            {
                contact.ClientContactID = 0;
                contact.CreatedBy = client.CreatedBy;
                contact.DateCreated = client.DateCreated;
                contact.UpdatedBy = client.UpdatedBy;
                contact.LastUpdate = client.LastUpdate;
            }
            return client;
        }

        public async Task<IActionResult> GetRecordStamps(int id) {
            var owner = await _ownerService.GetByIdAsync(id);
            if (owner==null)
                return new NoRecordFoundResult();

            return ViewComponent("RecordStamps", new {createdBy=owner.CreatedBy, dateCreated = owner.DateCreated, updatedBy = owner.UpdatedBy, lastUpdate=owner.LastUpdate, tStamp=owner.tStamp});
        }

        public async Task<Microsoft.AspNetCore.Mvc.ActionResult> ValueMapper(string value)
        {
            var owner = await this.Owners.FirstOrDefaultAsync(c => c.OwnerCode == value);
            return Json(owner);
        }

        public async Task<Microsoft.AspNetCore.Mvc.ActionResult> OwnerCodeNameValueMapper(string value)
        {
            var owner = await this.Owners.Where(c => c.OwnerCode == value).Select(o => new { Owner = o.OwnerCode, OwnerName = o.OwnerName }).FirstOrDefaultAsync(); 
            return Json(owner);
        }

        public async Task<int?> GetOwnerId(string ownerCode)
        {
            var owner = await _ownerService.QueryableList.Where(o => o.OwnerCode  == ownerCode).FirstOrDefaultAsync();
            return owner?.OwnerID;
        }

        protected IQueryable<Owner> Owners => _ownerService.QueryableList;

        private async Task<DetailPageViewModel<OwnerDetailViewModel>> PrepareEditScreen(IQueryable<Owner> owners, int id)
        {
            var viewModel = new DetailPageViewModel<OwnerDetailViewModel>();
            viewModel.Detail = await _ownerViewModelService.CreateViewModelForDetailScreen(id);

            if (viewModel.Detail != null) {
                viewModel.AddSharedSecurityPolicies();
                await viewModel.ApplyDetailPagePermission(User, _authService);

                viewModel.PageActions = await GetMorePageActions(viewModel);

                //do not allow add if user entity filter type is owner
                //user won't be able to access new record
                viewModel.CanAddRecord = await CanAddRecord();

                //hide copy and email buttons
                viewModel.CanCopyRecord = false;
                viewModel.CanEmail = false;

                this.AddDefaultNavigationUrls(viewModel);

                viewModel.EditScreenUrl = $"{viewModel.EditScreenUrl}/{id}";
                viewModel.SearchScreenUrl = this.Url.Action("Index");
                viewModel.Container = _dataContainer;

                var setting = await _settings.GetSetting();
                if (setting.IsShowCustomFieldOn)
                {
                    viewModel.Detail.SysCustomFieldSettings = await _ownerService.GetCustomFields();
                }
            }
            return viewModel;
        }

        /// <summary>
        /// Additional page nav actions
        /// </summary>
        /// <param name="pagePermission"></param>
        /// <returns></returns>
        private async Task<List<DetailPageAction>> GetMorePageActions(DetailPageViewModel<OwnerDetailViewModel> pagePermission)
        {
            var pageActions = new List<DetailPageAction>();
            var clientLabel = (await _settings.GetSetting()).LabelClient;

            //do not allow copy if user entity filter type is client
            if (pagePermission.CanEditRecord && !pagePermission.CanEditRemarksOnly && User.GetEntityFilterType() != CPiEntityType.Client)
            {
                pageActions.Add(new DetailPageAction { Url = Url.Action(nameof(SyncToClientPrompt)), Label = $"Synchronize with {clientLabel}", IsPopup = true, IconClass = "fa-sync-alt", ControlId = "ownerSyncToClient", IsPageNav = true });

                var hasClient = await _clientService.QueryableList.AnyAsync(c => c.ClientCode == pagePermission.Detail.OwnerCode);
                
                if (!hasClient && pagePermission.CanAddRecord)
                    pageActions.Add(new DetailPageAction { Label = $"Copy To {clientLabel}", IconClass = "fa-copy", ControlId = "ownerAddToClient" });
            }
            return pageActions;
        }

        private async Task<DetailPageViewModel<OwnerDetailViewModel>> PrepareAddScreen()
        {
            var viewModel = new DetailPageViewModel<OwnerDetailViewModel>();
            viewModel.Detail = new OwnerDetailViewModel();

            viewModel.AddSharedSecurityPolicies();
            await viewModel.ApplyDetailPagePermission(User, _authService);

            this.AddDefaultNavigationUrls(viewModel);
            viewModel.Container = _dataContainer;

            var setting = await _settings.GetSetting();
            if (setting.IsShowCustomFieldOn)
            {
                viewModel.Detail.SysCustomFieldSettings = await _ownerService.GetCustomFields();
            }

            return viewModel;
        }

        private async Task<bool> CanAddRecord()
        {
            //do not allow add if user entity filter type is owner
            //user won't be able to access new record
            return (await _authService.AuthorizeAsync(User, SharedAuthorizationPolicy.FullModify)).Succeeded &&
                User.GetEntityFilterType() != CPiEntityType.Owner;
        }

        public async Task<IActionResult> GetPicklistData([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType, string requiredRelation = "")
        {
            return await GetPicklistData(Owners, request, property, text, filterType, requiredRelation);
        }

        //TODO: USE GetOwnerList
        public async Task<IActionResult> GetOwnersList(string property, string text, FilterType filterType, string requiredRelation = "")
        {
            var owners = this.Owners;

            owners = QueryHelper.BuildCriteria(owners, property, text, filterType, requiredRelation).OrderBy(property);
            var list = await owners.Select(o => new { OwnerID = o.OwnerID, OwnerCode = o.OwnerCode, OwnerName = o.OwnerName }).ToListAsync();
            return Json(list);
        }

        public async Task<IActionResult> GetOwnerList([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType, string requiredRelation = "")
        {
            var owners = Owners.Where(c => (bool)c.IsActive);
            return await GetPicklistData(owners, request, property, text, filterType, new string[] { "OwnerID", "OwnerCode", "OwnerName" }, requiredRelation);
            //requiredRelation won't work if already projected to viewmodel
            //return await GetPicklistData(Owners.ProjectTo<OwnerListViewModel>(), request, property, text, filterType, requiredRelation, false);
        }

        public async Task<IActionResult> GetContactList([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType, string requiredRelation = "")
        {
            var ownerContacts = _ownerService.ChildService.QueryableList.Where(w => Owners.Any(a => a.OwnerID == w.OwnerID));
            IQueryable<ContactListViewModel> contacts;

            if (property.ToUpper() == "CONTACTNAME")
                contacts = ownerContacts.Select(s => new ContactListViewModel { ContactName = s.Contact.ContactName });
            else
                contacts = ownerContacts.Select(s => new ContactListViewModel { Contact = s.Contact.Contact, ContactName = s.Contact.ContactName });

            contacts = contacts.Distinct().OrderBy(property).BuildCriteria(property, text, filterType, requiredRelation);
            return await GetPicklistData(contacts, request);
        }

        public async Task<IActionResult> GetCountryList([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType)
        {
            var countries = _countryLookupService.Countries
                                    .Where(w => Owners.Any(a => a.Country == w.Country))
                                    .Distinct().OrderBy(property)
                                    .BuildCriteria(property, text, filterType);
            return await GetPicklistData(countries, request);
        }

        public async Task<IActionResult> GetPOCountryList([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType)
        {
            var countries = _countryLookupService.Countries
                                    .Where(w => Owners.Any(a => a.POCountry == w.Country))
                                    .Distinct().OrderBy(property)
                                    .BuildCriteria(property, text, filterType);
            return await GetPicklistData(countries, request);
        }

        public async Task<IActionResult> ContactsRead([DataSourceRequest] DataSourceRequest request, int ownerId)
        {
            var result = (await _ownerViewModelService.GetOwnerContacts(ownerId)).ToDataSourceResult(request);
            return Json(result);
        }


        [Authorize(Policy = SharedAuthorizationPolicy.FullModify)]
        public async Task<IActionResult> ContactsUpdate(int ownerId, 
            [Bind(Prefix = "updated")]IEnumerable<OwnerContactViewModel> updated,
            [Bind(Prefix = "new")]IEnumerable<OwnerContactViewModel> added, 
            [Bind(Prefix = "deleted")]IEnumerable<OwnerContactViewModel> deleted)
        {
            if (updated.Any() || added.Any() || deleted.Any())
            {
                if (!ModelState.IsValid)
                    return new JsonBadRequest(new { errors = ModelState.Errors() });

                await _ownerService.ChildService.Update(ownerId, User.GetUserName(),
                    _mapper.Map<List<OwnerContact>>(updated),
                    _mapper.Map<List<OwnerContact>>(added),
                    _mapper.Map<List<OwnerContact>>(deleted)
                    );
                var success = deleted.Count() + updated.Count() + added.Count() == 1 ?
                    _localizer["Contact has been saved successfully."].ToString() :
                    _localizer["Contacts have been saved successfully"].ToString();
                return Ok(new { success = success });
            }
            return Ok();
        }

        [Authorize(Policy = SharedAuthorizationPolicy.FullModify)]
        public async Task<IActionResult> ContactsDelete([Bind(Prefix = "deleted")] OwnerContactViewModel deleted)
        {
            if (deleted.OwnerContactID >= 0)
            {
                await _ownerService.ChildService.Update(deleted.OwnerID, User.GetUserName(), new List<OwnerContact>(), new List<OwnerContact>(), new List<OwnerContact>() { _mapper.Map<OwnerContact>(deleted) });
                return Ok(new { success = _localizer["Contact has been deleted successfully."].ToString() });
            }
            return Ok();
        }
    }
}