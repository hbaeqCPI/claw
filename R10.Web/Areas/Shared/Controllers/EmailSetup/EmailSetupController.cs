using R10.Web.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper.QueryableExtensions;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using R10.Core.Entities;
using R10.Core.Interfaces;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;
using R10.Web.Extensions.ActionResults;
using R10.Web.Interfaces;
using R10.Web.Models;
using R10.Web.Models.PageViewModels;
using R10.Web.Security;

using R10.Web.Areas;

namespace R10.Web.Areas.Shared.Controllers
{
    [Area("Shared"), Authorize(Policy = SharedAuthorizationPolicy.CanAccessSystem)]
    public class EmailSetupController : BaseController
    {
        private readonly IAuthorizationService _authService;
        private readonly IViewModelService<EmailType> _viewModelService;
        private readonly IParentEntityService<EmailType, EmailSetup> _emailTypeService;
        private readonly IEmailTemplateService _emailTemplateService;
        private readonly IStringLocalizer<SharedResource> _localizer;

        private readonly string _dataContainer = "emailSetupDetail";

        public EmailSetupController(
            IAuthorizationService authService,
            IViewModelService<EmailType> viewModelService,
            IParentEntityService<EmailType, EmailSetup> emailTypeService,
            IEmailTemplateService emailTemplateService,
            IStringLocalizer<SharedResource> localizer)
        {
            _authService = authService;
            _viewModelService = viewModelService;
            _emailTypeService = emailTypeService;
            _emailTemplateService = emailTemplateService;
            _localizer = localizer;
        }

        [Authorize(Policy = AMSAuthorizationPolicy.CanAccessSystem)]
        public async Task<IActionResult> AMS()
        {
            return await Index(SystemType.AMS);
        }

        [Authorize(Policy = ForeignFilingAuthorizationPolicy.CanAccessSystem)]
        public async Task<IActionResult> ForeignFiling()
        {
            return await Index(SystemType.ForeignFiling);
        }

        [Authorize(Policy = RMSAuthorizationPolicy.CanAccessSystem)]
        public async Task<IActionResult> RMS()
        {
            return await Index(SystemType.RMS);
        }

        [Authorize(Policy = CPiAuthorizationPolicy.Administrator)]
        public async Task<IActionResult> Admin()
        {
            return await Index("Admin");
        }

        public async Task<IActionResult> Index(string? systemType)
        {
            var model = new PageViewModel()
            {
                Page = PageType.Search,
                PageId = "emailSetupSearch",
                Title = string.IsNullOrEmpty(systemType) ?
                            _localizer["Notification Setup Search"].ToString() :
                            _localizer[$"{GetSystemName(systemType)} Notification Setup Search"].ToString(),
                CanAddRecord = (await _authService.AuthorizeAsync(User, SharedAuthorizationPolicy.FullModify)).Succeeded
            };

            ViewData["SystemType"] = systemType;

            if (Request.IsAjax())
                return PartialView("Index", model);

            return View("Index", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Search([FromBody] List<QueryFilterViewModel> mainSearchFilters)
        {
            var systemType = mainSearchFilters.FirstOrDefault(f => f.Property == "SystemType");
            if (systemType != null)
                mainSearchFilters.Remove(systemType);

            var model = new PageViewModel()
            {
                Page = PageType.SearchResults,
                PageId = "emailSetupSearchResults",
                Title = systemType == null || string.IsNullOrEmpty(systemType.Value) ?
                            _localizer["Notification Setup Search Results"].ToString() :
                            _localizer[$"{GetSystemName(systemType.Value)} Notification Setup Search Results"].ToString(),
                CanAddRecord = (await _authService.AuthorizeAsync(User, SharedAuthorizationPolicy.FullModify)).Succeeded
            };

            ViewData["SystemType"] = systemType?.Value;

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
                var systemType = mainSearchFilters.FirstOrDefault(f => f.Property == "SystemType");
                if (systemType != null)
                    mainSearchFilters.Remove(systemType);

                var emailTypes = _viewModelService.AddCriteria(await GetUserEmailTypes(systemType?.Value), mainSearchFilters);
                var result = await _viewModelService.CreateViewModelForGrid<EmailTypeSearchResultViewModel>(request, emailTypes, "Name", "EmailTypeId");
                return Json(result);
            }
            return new JsonBadRequest(new { errors = ModelState.Errors() });
        }

        private async Task<DetailPageViewModel<EmailTypeDetailViewModel>> PrepareEditScreen(int id)
        {
            var viewModel = new DetailPageViewModel<EmailTypeDetailViewModel>
            {
                Detail = await (await GetUserEmailTypes("")).ProjectTo<EmailTypeDetailViewModel>().SingleOrDefaultAsync(t => t.EmailTypeId == id)
            };

            if (viewModel.Detail != null)
            {
                viewModel.AddSharedSecurityPolicies();
                await viewModel.ApplyDetailPagePermission(User, _authService);

                //hide copy and email buttons
                viewModel.CanCopyRecord = false;
                viewModel.CanEmail = false;

                //hide print button
                viewModel.CanPrintRecord = false;

                this.AddDefaultNavigationUrls(viewModel);

                viewModel.Container = _dataContainer;

                viewModel.EditScreenUrl = Url.Action("Detail", new { id = id });
                viewModel.SearchScreenUrl = Url.Action("Index");

                var systemType = GetSystemTypeByPolicy(viewModel.Detail.Policy ?? "");
                viewModel.AddScreenUrl = Url.Action("Add", new { systemType = systemType });
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

            var detail = page.Detail;
            var systemType = GetSystemTypeByPolicy(detail.Policy ?? "");
            PageViewModel model = new PageViewModel()
            {
                Page = PageType.Detail,
                PageId = page.Container,
                Title = string.IsNullOrEmpty(systemType) ?
                            _localizer["Notification Setup Detail"].ToString() :
                            _localizer[$"{GetSystemName(systemType)} Notification Setup Detail"].ToString(),
                RecordId = detail.EmailTypeId,
                SingleRecord = singleRecord || !Request.IsAjax(),
                PagePermission = page,
                Data = detail
            };

            ViewData["SystemType"] = systemType;

            if (Request.IsAjax())
            {
                if (!singleRecord && !fromSearch)
                    model.Page = PageType.DetailContent;

                return PartialView("Index", model);
            }

            return View("Index", model);
        }

        private async Task<DetailPageViewModel<EmailTypeDetailViewModel>> PrepareAddScreen()
        {
            var viewModel = new DetailPageViewModel<EmailTypeDetailViewModel>
            {
                Detail = new EmailTypeDetailViewModel() { IsEnabled = true }
            };

            viewModel.AddSharedSecurityPolicies();
            await viewModel.ApplyDetailPagePermission(User, _authService);

            this.AddDefaultNavigationUrls(viewModel);
            viewModel.Container = _dataContainer;
            return viewModel;
        }

        [Authorize(Policy = SharedAuthorizationPolicy.FullModify)]
        public async Task<IActionResult> Add(bool fromSearch = false, string name = "", string systemType = "")
        {
            if (!Request.IsAjax())
                return RedirectToAction("Index");

            var page = await PrepareAddScreen();
            if (page.Detail == null)
                return RedirectToAction("Index");

            if (!string.IsNullOrEmpty(name))
                page.Detail.Name = name;

            var detail = page.Detail;
            PageViewModel model = new PageViewModel()
            {
                Page = fromSearch ? PageType.Detail : PageType.DetailContent,
                PageId = page.Container,
                Title = string.IsNullOrEmpty(systemType) ? 
                            _localizer["New Notification Setup"].ToString() : 
                            _localizer[$"New {GetSystemName(systemType)} Notification Setup"].ToString(),
                RecordId = detail.EmailTypeId,
                PagePermission = page,
                Data = detail,
                FromSearch = fromSearch
            };

            ViewData["SystemType"] = systemType;

            return PartialView("Index", model);
        }

        [HttpPost, Authorize(Policy = SharedAuthorizationPolicy.FullModify)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save([FromBody] EmailType emailType)
        {
            if (ModelState.IsValid)
            {
                UpdateEntityStamps(emailType, emailType.EmailTypeId);

                if (emailType.EmailTypeId > 0)
                    await _emailTypeService.Update(emailType);
                else
                    await _emailTypeService.Add(emailType);

                return Json(emailType.EmailTypeId);
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        [HttpPost, Authorize(Policy = SharedAuthorizationPolicy.CanDelete)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, string tStamp)
        {
            var entity = await _emailTypeService.GetByIdAsync(id);

            if (entity == null)
                return new RecordDoesNotExistResult();

            entity.tStamp = Convert.FromBase64String(tStamp);
            await _emailTypeService.Delete(entity);

            return Ok();
        }

        public async Task<IActionResult> GetRecordStamps(int id)
        {
            var actionType = await _emailTypeService.GetByIdAsync(id);
            if (actionType == null)
                return new NoRecordFoundResult();

            return ViewComponent("RecordStamps", new { createdBy = actionType.CreatedBy, dateCreated = actionType.DateCreated, updatedBy = actionType.UpdatedBy, lastUpdate = actionType.LastUpdate, tStamp = actionType.tStamp });
        }

        [HttpGet()]
        public async Task<IActionResult> DetailLink(string id)
        {
            if (!String.IsNullOrEmpty(id))
            {
                var entity = await _viewModelService.GetEntityByCode("Name", id);
                if (entity == null)
                    //return new RecordDoesNotExistResult();
                    return RedirectToAction(nameof(Add), new { fromSearch = true, name = id });
                else
                    return RedirectToAction(nameof(Detail), new { id = entity.EmailTypeId, singleRecord = true, fromSearch = true });
            }
            else
            {
                return RedirectToAction(nameof(Add), new { fromSearch = true });
            }
        }

        public async Task<IActionResult> GetPicklistData([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType, string requiredRelation = "")
        {
            return await GetPicklistData(await GetUserEmailTypes(requiredRelation), request, property, text, filterType);
        }

        //search screen lookup
        public async Task<IActionResult> GetContentTypeList([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType, string requiredRelation = "")
        {
            var data = (await GetUserEmailTypes(requiredRelation)).Select(e => new { Description = e.EmailContentType.Description });
            return await GetPicklistData(data, request, property, text, filterType, "", false);
        }

        //search screen lookup
        public async Task<IActionResult> GetEmailTemplateList([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType, string requiredRelation = "")
        {
            var data =  _emailTypeService.QueryableList.Select(e => new { Name = e.EmailTemplate.Name });
            return await GetPicklistData(data, request, property, text, filterType, requiredRelation, false);
        }

        //detail screen lookup
        public async Task<IActionResult> GetUserContentTypeList(string? requiredRelation = "") 
        {
            var data = (await GetUserContentTypes(requiredRelation)).Select(l => new { Text = l.Description, Value = l.Name }).ToList();
            return Json(data);
        }

        //cover letter lookup
        public async Task<IActionResult> GetEmailTypeList(string contentType)
        {
            var data = await _emailTypeService.GetEmailTypes(contentType);
            return Json(data);
        }

        private async Task<List<EmailContentType>> GetUserContentTypes(string? systemType)
        {
            var userContentTypes = new List<EmailContentType>();

            foreach(var contentType in await _emailTemplateService.ContentTypes.Where(t => string.IsNullOrEmpty(systemType) || GetPoliciesBySystemType(systemType).Contains(t.Policy)).ToListAsync())
            {
                if ((await _authService.AuthorizeAsync(User, contentType.Policy)).Succeeded)
                    userContentTypes.Add(contentType);
            }

            return userContentTypes;
        }

        private async Task<IQueryable<EmailType>> GetUserEmailTypes(string? systemType)
        {
            var contentTypes = (await GetUserContentTypes(systemType)).Where(t => string.IsNullOrEmpty(systemType) || GetPoliciesBySystemType(systemType).Contains(t.Policy ?? "")).Select(t => t.Name).ToList();

            return _emailTypeService.QueryableList.Where(t => contentTypes.Contains(t.ContentType));
        }

        private List<string> AMSPolicies => new List<string> { "RegularUserAMS" };
        private List<string> RMSPolicies => new List<string> { "RegularUserRMS" };
        private List<string> FFPolicies => new List<string> { "RegularUserForeignFiling" };
        private List<string> AdminPolicies => new List<string> { "Administrator", "TradeSecretAdmin" };

        private List<string> GetPoliciesBySystemType(string? systemType)
        {
            switch (systemType)
            {
                case SystemType.AMS:
                    return AMSPolicies;

                case SystemType.RMS:
                    return RMSPolicies;

                case SystemType.ForeignFiling:
                    return FFPolicies;

                default:
                    return AdminPolicies;
            }
        }

        private string GetSystemTypeByPolicy(string policy)
        {
            if (AMSPolicies.Contains(policy))
                return SystemType.AMS;

            if (RMSPolicies.Contains(policy))
                return SystemType.RMS;

            if (FFPolicies.Contains(policy))
                return SystemType.ForeignFiling;

            return "Admin";

        }

        private string GetSystemName(string systemType)
        {
            switch(systemType)
            {
                case SystemType.AMS:
                    return "AMS";

                case SystemType.RMS:
                    return "RMS";

                case SystemType.ForeignFiling:
                    return "Foreign Filing";

                default:
                    return "Admin";
            }
        }
    }
}