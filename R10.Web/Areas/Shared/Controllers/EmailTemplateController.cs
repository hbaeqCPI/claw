using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using R10.Core;
using R10.Core.Entities;
using R10.Core.Exceptions;
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
    public class EmailTemplateController : BaseController
    {
        private readonly IAuthorizationService _authService;
        private readonly IViewModelService<EmailTemplate> _viewModelService;
        private readonly IEmailTemplateService _emailTemplateService;
        private readonly IStringLocalizer<SharedResource> _localizer;

        private readonly string _dataContainer = "emailTemplateDetail";

        public EmailTemplateController(
            IAuthorizationService authService, 
            IViewModelService<EmailTemplate> viewModelService, 
            IEmailTemplateService emailTemplateService, 
            IStringLocalizer<SharedResource> localizer)
        {
            _authService = authService;
            _viewModelService = viewModelService;
            _emailTemplateService = emailTemplateService;
            _localizer = localizer;
        }

        public async Task<IActionResult> Index()
        {
            var model = new PageViewModel()
            {
                Page = PageType.Search,
                PageId = "emailTemplateSearch",
                Title = _localizer["Notification Template Search"].ToString(),
                CanAddRecord = (await _authService.AuthorizeAsync(User, SharedAuthorizationPolicy.FullModify)).Succeeded
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
                PageId = "emailTemplateSearchResults",
                Title = _localizer["Notification Template Search Results"].ToString(),
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
                var templates = _viewModelService.AddCriteria(mainSearchFilters);
                var result = await _viewModelService.CreateViewModelForGrid(request, templates, "Name", "EmailTemplateId");
                return Json(result);
            }
            return new JsonBadRequest(new { errors = ModelState.Errors() });
        }

        private async Task<DetailPageViewModel<EmailTemplate>> PrepareEditScreen(int id)
        {
            var viewModel = new DetailPageViewModel<EmailTemplate>();
            viewModel.Detail = await _emailTemplateService.GetByIdAsync(id);

            if (viewModel.Detail != null)
            {
                viewModel.AddSharedSecurityPolicies();
                await viewModel.ApplyDetailPagePermission(User, _authService);

                viewModel.CanEmail = false;

                //hide print button
                viewModel.CanPrintRecord = false;

                this.AddDefaultNavigationUrls(viewModel);

                viewModel.EditScreenUrl = $"{viewModel.EditScreenUrl}/{id}";
                viewModel.SearchScreenUrl = this.Url.Action("Index");

                viewModel.CopyScreenUrl = $"{viewModel.CopyScreenUrl}/{id}";
                viewModel.IsCopyScreenPopup = false;

                viewModel.Container = _dataContainer;
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
            PageViewModel model = new PageViewModel()
            {
                Page = PageType.Detail,
                PageId = page.Container,
                Title = _localizer["Notification Template Detail"].ToString(),
                RecordId = detail.EmailTemplateId,
                SingleRecord = singleRecord || !Request.IsAjax(),
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

        [HttpGet()]
        public async Task<IActionResult> DetailLink(int? id, string name)
        {
            if (id == null && !string.IsNullOrEmpty(name))
                id = await _emailTemplateService.QueryableList.Where(t => t.Name == name).Select(t => t.EmailTemplateId).FirstOrDefaultAsync();

            if (id > 0)
                return RedirectToAction(nameof(Detail), new { id = id, singleRecord = true, fromSearch = false });
            else if ((await _authService.AuthorizeAsync(User, SharedAuthorizationPolicy.FullModify)).Succeeded)
                return RedirectToAction(nameof(Add), new { fromSearch = true, name = name });
            else
                return new RecordDoesNotExistResult();
        }

        private async Task<DetailPageViewModel<EmailTemplate>> PrepareAddScreen(int id = 0)
        {
            var viewModel = new DetailPageViewModel<EmailTemplate>();
            viewModel.Detail = new EmailTemplate();

            if (id > 0)
            {
                //copy
                var detail = await _emailTemplateService.QueryableList
                                        .Where(t => t.EmailTemplateId == id)
                                        .Select(t => new EmailTemplate() { Name = t.Name + " - Copy", Description = t.Description, Template = t.Template })
                                        .FirstOrDefaultAsync();
                Guard.Against.NoRecordPermission(detail != null);
                viewModel.Detail = detail;
            }

            viewModel.AddSharedSecurityPolicies();
            await viewModel.ApplyDetailPagePermission(User, _authService);

            this.AddDefaultNavigationUrls(viewModel);
            viewModel.Container = _dataContainer;
            return viewModel;
        }

        [Authorize(Policy = SharedAuthorizationPolicy.FullModify)]
        public async Task<IActionResult> Add(bool fromSearch = false, string name = "")
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
                Title = _localizer["New Notification Template"].ToString(),
                RecordId = detail.EmailTemplateId,
                PagePermission = page,
                Data = detail,
                FromSearch = fromSearch
            };

            return PartialView("Index", model);
        }

        [Authorize(Policy = SharedAuthorizationPolicy.FullModify)]
        public async Task<IActionResult> Copy(int id)
        {
            if (!Request.IsAjax() || id == 0)
                return RedirectToAction("Index");

            var page = await PrepareAddScreen(id);
            if (page.Detail == null)
                return RedirectToAction("Index");

            var detail = page.Detail;
            PageViewModel model = new PageViewModel()
            {
                Page = PageType.DetailContent,
                PageId = page.Container,
                Title = _localizer["New Email Template"].ToString(),
                RecordId = detail.EmailTemplateId,
                PagePermission = page,
                Data = detail,
                FromSearch = false
            };

            return PartialView("Index", model);
        }

        [HttpPost, Authorize(Policy = SharedAuthorizationPolicy.FullModify)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save([FromBody] EmailTemplate emailTemplate)
        {
            if (ModelState.IsValid)
            {
                UpdateEntityStamps(emailTemplate, emailTemplate.EmailTemplateId);

                emailTemplate.Template = WebUtility.HtmlDecode(emailTemplate.Template);

                if (emailTemplate.EmailTemplateId > 0)
                    await _emailTemplateService.Update(emailTemplate);
                else
                    await _emailTemplateService.Add(emailTemplate);

                return Json(emailTemplate.EmailTemplateId);
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
            var entity = await _emailTemplateService.GetByIdAsync(id);

            if (entity == null)
                return new RecordDoesNotExistResult();

            entity.tStamp = Convert.FromBase64String(tStamp);
            await _emailTemplateService.Delete(entity);

            return Ok();
        }

        public async Task<IActionResult> GetPicklistData([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType, string requiredRelation = "")
        {
            return await GetPicklistData(_emailTemplateService.QueryableList, request, property, text, filterType, requiredRelation);
        }

        //tags lookup
        public IActionResult GetDataFieldsList(string contentType)
        {
            var templateTags = new List<string>() { "Subject", "Body", "LogoUrl" };
            return Json(templateTags.Select(t => new { Text = t, Value = $"{{{{{t}}}}}" }).ToList());
        }

        //email setup detail lookup
        public async Task<IActionResult> GetTemplateList()
        {
            var data = await _emailTemplateService.QueryableList.Select(t => new { Text = t.Name, Value = t.EmailTemplateId }).ToListAsync();
            return Json(data);
        }
    }
}