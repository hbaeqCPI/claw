using R10.Web.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper.QueryableExtensions;
using Kendo.Mvc.Extensions;
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
    public class EmailSetupDetailController : BaseController
    {
        private readonly IAuthorizationService _authService;
        private readonly IViewModelService<EmailSetup> _viewModelService;
        private readonly IParentEntityService<EmailType, EmailSetup> _emailTypeService;
        private readonly IEmailTemplateService _emailTemplateService;
        private readonly IStringLocalizer<SharedResource> _localizer;

        private readonly string _dataContainer = "emailContentDetail";

        public EmailSetupDetailController(
            IAuthorizationService authService,
            IViewModelService<EmailSetup> viewModelService,
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

        private IQueryable<EmailSetup> EmailSetups => _emailTypeService.ChildService.QueryableList;

        public IActionResult Index()
        {
            return RedirectToAction("Index", "EmailSetup", new { area = "Shared" });
        }

        [HttpGet]
        public IActionResult Search()
        {
            return RedirectToAction("Search", "EmailSetup", new { area = "Shared" });
        }

        public async Task<IActionResult> GridRead([DataSourceRequest] DataSourceRequest request, int id)
        {
            var result = (await EmailSetups
                            .Where(e => e.EmailTypeId == id)
                            .ProjectTo<EmailSetupListViewModel>().ToListAsync()).ToDataSourceResult(request);
            return Json(result);
        }

        [Authorize(Policy = SharedAuthorizationPolicy.FullModify)]
        public async Task<IActionResult> GridDelete([Bind(Prefix = "deleted")] EmailSetup deleted)
        {
            if (deleted.EmailSetupId > 0)
            {
                await _emailTypeService.ChildService.Delete(deleted);
                return Ok(new { success = _localizer["Content has been deleted successfully."].ToString() });
            }
            return Ok();
        }

        private async Task<DetailPageViewModel<EmailSetupDetailViewModel>> PrepareEditScreen(int id)
        {
            var viewModel = new DetailPageViewModel<EmailSetupDetailViewModel>
            {
                Detail = await EmailSetups.ProjectTo<EmailSetupDetailViewModel>().SingleOrDefaultAsync(e => e.EmailSetupId == id)
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

                //hide search
                viewModel.CanSearch = false;

                this.AddDefaultNavigationUrls(viewModel);

                viewModel.Container = _dataContainer;

                viewModel.EditScreenUrl = Url.Action("Detail", new { id = id });
                viewModel.SearchScreenUrl = Url.Action("Index");

                //add url with parent id
                viewModel.AddScreenUrl = viewModel.CanAddRecord ? Url.Action("Add", new { id = viewModel.Detail.EmailTypeId }) : "";

                //preview
                viewModel.PageActions = new List<DetailPageAction>()
                {
                    new DetailPageAction()
                    {
                        ControlId = "Preview",
                        Label = _localizer["Preview"],
                        IconClass = "fal fa-search",
                        Url = Url.Action("Preview")
                    }
                };
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
                Title = _localizer["Email Content Detail"].ToString(),
                RecordId = detail.EmailSetupId,
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

        private async Task<DetailPageViewModel<EmailSetupDetailViewModel>> PrepareAddScreen(int emailTypeId)
        {
            var detail = await _emailTypeService.QueryableList
                                    .Where(e => e.EmailTypeId == emailTypeId)
                                    .Select(e => new EmailSetupDetailViewModel() 
                                    {
                                        EmailTypeId = e.EmailTypeId,
                                        Name = e.Name,
                                        Description = e.Description,
                                        ContentType = e.ContentType,
                                        ContentTypeDescription = e.EmailContentType.Description
                                    })
                                    .FirstOrDefaultAsync();
            Guard.Against.NoRecordPermission(detail != null);

            var viewModel = new DetailPageViewModel<EmailSetupDetailViewModel>
            {
                Detail = detail
            };

            viewModel.AddSharedSecurityPolicies();
            await viewModel.ApplyDetailPagePermission(User, _authService);

            this.AddDefaultNavigationUrls(viewModel);

            viewModel.Container = _dataContainer;
            return viewModel;
        }

        [Authorize(Policy = SharedAuthorizationPolicy.FullModify)]
        public async Task<IActionResult> Add(int id, bool fromSearch = false)
        {
            if (!Request.IsAjax())
                return RedirectToAction("Index");

            if (id == 0)
                return BadRequest();

            var page = await PrepareAddScreen(id);
            if (page.Detail == null)
                return RedirectToAction("Index");

            var detail = page.Detail;
            PageViewModel model = new PageViewModel()
            {
                Page = fromSearch ? PageType.Detail : PageType.DetailContent,
                PageId = page.Container,
                Title = _localizer["New Email Content"].ToString(),
                RecordId = detail.EmailSetupId,
                PagePermission = page,
                Data = detail,
                FromSearch = fromSearch,
                //not needed. default was updated to use showPreviousNode
                //AfterCancelledInsert = fromSearch ? "function() { window.cpiBreadCrumbs.showPreviousNode(); }" : ""
            };

            return PartialView("Index", model);
        }

        [HttpPost, Authorize(Policy = SharedAuthorizationPolicy.FullModify)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save([FromBody] EmailSetup emailSetup)
        {
            if (ModelState.IsValid)
            {
                UpdateEntityStamps(emailSetup, emailSetup.EmailSetupId);
                
                emailSetup.Body = WebUtility.HtmlDecode(emailSetup.Body);

                if (emailSetup.EmailSetupId > 0)
                    await _emailTypeService.ChildService.Update(emailSetup);
                else
                    await _emailTypeService.ChildService.Add(emailSetup);

                return Json(emailSetup.EmailSetupId);
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
            var entity = await _emailTypeService.ChildService.GetByIdAsync(id);

            if (entity == null)
                return new RecordDoesNotExistResult();

            entity.tStamp = Convert.FromBase64String(tStamp);
            await _emailTypeService.ChildService.Delete(entity);

            return Ok();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Preview(string name, string language)
        {
            if (string.IsNullOrEmpty(name))
                return BadRequest();

            var emailMessage = await _emailTemplateService.GetEmailMessage(name, language, (new EmailContent()
            {
                //CallToAction = "{{CallToAction}}",
                //CallToActionUrl = "{{CallToActionUrl}}",
                LogoUrl = Url.CPiLogoLink(Request.Scheme)
            }));

            return Json(emailMessage.Body);
        }

        //tags lookup
        public IActionResult GetDataFieldsList(string contentType)
        {
            return Json(GetTags(contentType).Select(t => new { Text = t, Value = $"{{{{{t}}}}}" }).ToList());
        }

        private List<string> GetTags(string contentType)
        {
            var templateDataType = Type.GetType(contentType);

            if (templateDataType == null)
            {
                string assemblyQualifiedName = AppDomain.CurrentDomain.GetAssemblies()
                                    .ToList()
                                    .SelectMany(x => x.GetTypes())
                                    .Where(x => x.Name == contentType)
                                    .Select(x => x.AssemblyQualifiedName)
                                    .FirstOrDefault();
                templateDataType = Type.GetType(assemblyQualifiedName);
            }

            if (templateDataType == null)
                return new List<string>();

            return templateDataType.GetProperties().Select(p => p.Name).ToList();
        }

        public async Task<IActionResult> GetPicklistData([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType, string requiredRelation = "")
        {
            return await GetPicklistData(EmailSetups, request, property, text, filterType, requiredRelation);
        }
    }
}