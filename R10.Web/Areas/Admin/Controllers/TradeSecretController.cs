using IdentityModel.Client;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using R10.Core.Entities;
using R10.Core.Entities.Shared;
using R10.Core.Helpers;
using R10.Core.Identity;
using R10.Core.Interfaces;
using R10.Core.Interfaces.Shared;
using R10.Core.Services;
using R10.Web.Areas.Admin.Helpers;
using R10.Web.Areas.Admin.Services;
using R10.Web.Areas.Admin.ViewModels;
using R10.Web.Areas.Admin.Views;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Areas.Shared.ViewModels.EmailTemplate;
using R10.Web.Extensions;
using R10.Web.Extensions.ActionResults;
using R10.Web.Helpers;
using R10.Web.Interfaces;
using R10.Web.Models;
using R10.Web.Models.PageViewModels;
using R10.Web.Security;
using R10.Web.Services;

using R10.Web.Areas;

namespace R10.Web.Areas.Admin.Controllers
{
    [Area("Admin"), Authorize(Policy = SharedAuthorizationPolicy.CanAccessTradeSecret)]
    public class TradeSecretController : BaseController
    {
        private readonly ITradeSecretService _tradeSecretService;
        private readonly IEmailSender _emailSender;
        private readonly IEmailTemplateService _emailTemplateService;
        private readonly INotificationService _notificationService;
        private readonly CPiUserManager _userManager;
        private readonly ISystemSettings<DefaultSetting> _defaultSettings;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public TradeSecretController(ITradeSecretService tradeSecretService,
            IEmailSender emailSender,
            IEmailTemplateService emailTemplateService,
            INotificationService notificationService,
            CPiUserManager userManager,
            ISystemSettings<DefaultSetting> defaultSettings,
            IStringLocalizer<SharedResource> localizer)
        {
            _tradeSecretService = tradeSecretService;
            _emailSender = emailSender;
            _emailTemplateService = emailTemplateService;
            _notificationService = notificationService;
            _userManager = userManager;
            _defaultSettings = defaultSettings;
            _localizer = localizer;
        }

        private string DataContainer => "adminTradeSecretDetail";
        private string SidebarTitle => _localizer[AdminNavPages.SidebarTitle].ToString();
        private string SidebarPartialView => "_SidebarNav";

        private IQueryable<TradeSecretRequest> TradeSecretRequests
        {
            get
            {
                var queryableList = _tradeSecretService.QueryableList;

                if (!User.IsSuper())
                    queryableList = queryableList.Where(ts => ts.CPiUser != null && ts.CPiUser.UserType != CPiUserType.SuperAdministrator);

                return queryableList;
            }
        }

        [Authorize(Policy = SharedAuthorizationPolicy.TradeSecretAdmin)]
        public IActionResult Index(string? p)
        {
            if (!string.IsNullOrEmpty(p))
                ViewData["Status"] = "Pending";

            var model = new PageViewModel()
            {
                PageId = "adminTradeSecretSearch",
                Title = _localizer["Trade Secret Requests"].ToString(),
                CanAddRecord = false
            };

            var sidebarModel = new SidebarPageViewModel()
            {
                Title = SidebarTitle,
                PageTitle = model.Title,
                PageId = model.PageId,
                MainPartialView = "_SidebarSearchResultsPage",
                MainViewModel = model,
                SideBarPartialView = SidebarPartialView,
                SideBarViewModel = AdminNavPages.TradeSecret
            };

            if (Request.IsAjax())
                return PartialView("Index", sidebarModel);

            return View(sidebarModel);
        }

        [Authorize(Policy = SharedAuthorizationPolicy.TradeSecretAdmin)]
        [HttpGet]
        public IActionResult Search()
        {
            return RedirectToAction("Index");
        }

        [Authorize(Policy = SharedAuthorizationPolicy.TradeSecretAdmin)]
        public async Task<IActionResult> PageRead([DataSourceRequest] DataSourceRequest request, List<QueryFilterViewModel> mainSearchFilters)
        {
            if (ModelState.IsValid)
            {
                var requests = TradeSecretRequests.AddCriteria(mainSearchFilters).ProjectTo<TradeSecretRequestListViewModel>();

                if (request.Sorts != null && request.Sorts.Any())
                    requests = requests.ApplySorting(request.Sorts);
                else
                    requests = requests.OrderByDescending(r => r.RequestDate);

                var ids = await requests.Select(r => r.RequestId).ToArrayAsync();

                return Json(new CPiDataSourceResult()
                {
                    Data = await requests.ApplyPaging(request.Page, request.PageSize).ToListAsync(),
                    Total = ids.Length,
                    Ids = ids
                });
            }
            return new JsonBadRequest(new { errors = ModelState.Errors() });
        }

        private async Task<DetailPageViewModel<TradeSecretRequestDetailViewModel>> PrepareEditScreen(int id)
        {
            var detail = await TradeSecretRequests.Where(r => r.RequestId == id).ProjectTo<TradeSecretRequestDetailViewModel>().SingleOrDefaultAsync();
            var viewModel = new DetailPageViewModel<TradeSecretRequestDetailViewModel>();

            if (detail != null)
            {
                detail.DetailUrl = Url.GetTradeSecretDetailLink(detail.ScreenId, detail.RecId);

                viewModel.Detail = detail;
                viewModel.CanAddRecord = false;
                viewModel.CanEditRecord = !detail.IsExpired;
                viewModel.CanDeleteRecord = false;
                viewModel.CanPrintRecord = false;

                this.AddDefaultNavigationUrls(viewModel);

                viewModel.Container = DataContainer;

                viewModel.SearchScreenUrl = Url.Action("Index");
                viewModel.EditScreenUrl = Url.Action("Detail", new { id = id });

                if (!detail.IsExpired)
                {
                    viewModel.PageActions.Add(new DetailPageAction()
                    {
                        Label = _localizer["Revoke"].ToString(),
                        Class = "open-confirm revoke-request", //call generic confirm dialog
                        IconClass = "fal fa-ban",
                        Url = Url.Action("RevokeConfirm", new { id = detail.RequestId }),
                        Data = new Dictionary<string, string>() { { "confirm-buttons", "okButtons" } }
                    });
                }

            }
            return viewModel;
        }

        [Authorize(Policy = SharedAuthorizationPolicy.TradeSecretAdmin)]
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
            var model = new PageViewModel()
            {
                Page = PageType.Detail,
                PageId = page.Container,
                Title = _localizer["Request Detail"].ToString(),
                RecordId = detail.RequestId,
                SingleRecord = singleRecord || !Request.IsAjax(),
                PagePermission = page,
                Data = detail
            };

            var sidebarModel = new SidebarPageViewModel()
            {
                Title = SidebarTitle,
                PageTitle = model.Title,
                PageId = model.PageId,
                MainPartialView = "_SidebarDetailPage",
                MainViewModel = model,
                SideBarPartialView = SidebarPartialView,
                SideBarViewModel = AdminNavPages.TradeSecret
            };

            if (Request.IsAjax())
            {
                if (!singleRecord && !fromSearch)
                {
                    model.Page = PageType.DetailContent;
                    return PartialView("_Index", model);
                }

                return PartialView("Index", sidebarModel);
            }

            return View("Index", sidebarModel);
        }

        [Authorize(Policy = SharedAuthorizationPolicy.TradeSecretAdmin)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save([FromBody] TradeSecretRequestDetailViewModel? requestDetail)
        {
            if (requestDetail == null ||  requestDetail.RequestId == 0 || string.IsNullOrEmpty(requestDetail.Status))
                return BadRequest();

            var req = await _tradeSecretService.UpdateApprovalStatus(requestDetail.RequestId, requestDetail.Status);

            if (req == null)
                return BadRequest(_localizer["Request not found or has expired"].ToString());

            if (req.IsGranted && !string.IsNullOrEmpty(req.Token))
                await SendAccessCodeNotification(req);

            return Json(req.RequestId);
        }

        [Authorize(Policy = SharedAuthorizationPolicy.TradeSecretAdmin)]
        [HttpGet]
        public IActionResult RevokeConfirm(int id)
        {
            return PartialView("_Revoke", id);
        }

        [Authorize(Policy = SharedAuthorizationPolicy.TradeSecretAdmin)]
        [HttpPost]
        public async Task<IActionResult> Revoke(int id)
        {
            var req = await _tradeSecretService.UpdateApprovalStatus(id, TradeSecretRequestStatus.Revoked);

            if (req == null)
                return BadRequest(_localizer["Request not found or has expired"].Value);


            return Ok(new { id = id, message = _localizer["Request has been revoked"].Value });
        }

        public async Task<IActionResult> GetRequestStatus(string locator)
        {
            var req = await _tradeSecretService.GetUserRequest(locator);
            var status = 0;
            var prompt = _localizer["Do you want to submit a request to view trade secret information?"].ToString();
            var url = Url.Action("GetApproval");

            if (req != null && !req.IsExpired)
            {
                prompt = _localizer["There is a pending request to view trade secret information. Do you want to submit a new request?"].ToString();

                if (req.IsGranted || req.IsCleared)
                {
                    status = 1;
                    prompt = _localizer["Request is granted."].ToString();
                    url = Url.Action("Validate");
                }
            }

            return Ok(new { status, prompt, url });
        }

        public async Task<IActionResult> GetApproval(string locator)
        {
            var req = await _tradeSecretService.CreateUserRequest(locator);
            if (req == null)
                return BadRequest(_localizer["Error creating request."].ToString());

            if (req.IsGranted)
            {
                //send access token to user's email
                if (!string.IsNullOrEmpty(req.Token))
                    await SendAccessCodeNotification(req);

                return Ok(new { granted = true, message = _localizer["Request is granted."].ToString(), url = Url.Action("Validate") });
            }

            //send notification to approvers
            await SendRequestNotification(req);

            return Ok(new { granted = false, message = _localizer["Request has been submitted."].ToString(), url = "" });
        }

        [HttpGet]
        public IActionResult Validate(string locator)
        {
            return PartialView("_Validate", locator);
        }

        [HttpPost]
        public async Task<IActionResult> Validate(string locator, string token)
        {
            if (await _tradeSecretService.ValidateAccessToken(locator, token))
                return Ok();

            return BadRequest(_localizer["Invalid access code."].ToString());
        }

        public async Task<IActionResult> Monitor(string locator)
        {
            var (ScreenId, RecId) = _tradeSecretService.GetLocator(locator);
            var req = await _tradeSecretService.GetUserRequest(locator);
            if (req == null || !req.IsCleared)
            {
                await _tradeSecretService.LogActivity(TradeSecretScreen.Invention, TradeSecretScreen.Invention, RecId, TradeSecretActivityCode.TimeOut, req?.RequestId ?? 0);
                return Forbid();
            }
            
            return Ok();
        }

        private async Task<EmailSenderResult> SendRequestNotification(TradeSecretRequest req)
        {
            var approvers = await _tradeSecretService.GetApproverMailAddresses(req.ScreenId);
            if (!approvers.Any())
                return new EmailSenderResult() { ErrorMessage = "Trade secret approvers not found. Unable to send email." };

            var callToActionUrl = Url.Action("Index", "TradeSecret", new { area = "Admin", p = "1" }, Request.Scheme);
            var defaultSettings = await _defaultSettings.GetSetting();
            EmailMessage? emailMessage = null;

            if (!string.IsNullOrEmpty(defaultSettings.TradeSecretRequestNotification))
                emailMessage = await _emailTemplateService.GetEmailMessage(defaultSettings.TradeSecretRequestNotification, User.GetLocale(), new TradeSecretRequestNotification()
                {
                    CallToAction = _localizer["View Requests"].ToString(),
                    CallToActionUrl = callToActionUrl,
                    LogoUrl = Url.CPiLogoLink(Request.Scheme)
                });

            await _notificationService.SendAlert(User.GetUserName(), approvers, _localizer["Trade secret approval request"].ToString(), _localizer["There is a new trade secret access request waiting for approval."].ToString(), callToActionUrl, TradeSecretHelper.RequestExpiration);

            if (emailMessage == null)
                return await _emailSender.SendEmailAsync(approvers, _localizer["New Trade Secret Access Request"].ToString(), _localizer["There is a new request for trade secret access"].ToString());
            else
                return await _emailSender.SendEmailAsync(approvers, emailMessage.Subject, emailMessage.Body);
        }

        private async Task<EmailSenderResult> SendAccessCodeNotification(TradeSecretRequest req)
        {
            if (req.Token == null)
                return new EmailSenderResult() { ErrorMessage = "Trade secret access code not found. Unable to send email." };

            if (string.IsNullOrEmpty(req.UserId))
                return new EmailSenderResult() { ErrorMessage = "User not found. Unable to send email." };

            var user = await _userManager.FindByIdAsync(req.UserId);
            if (user == null || string.IsNullOrEmpty(user.Email))
                return new EmailSenderResult() { ErrorMessage = "User not found. Unable to send email." };

            var callToActionUrl = GetCallToActionUrl(req);
            var defaultSettings = await _defaultSettings.GetSetting();
            EmailMessage? emailMessage = null;

            if (!string.IsNullOrEmpty(defaultSettings.TradeSecretAccessCodeNotification))
                emailMessage = await _emailTemplateService.GetEmailMessage(defaultSettings.TradeSecretAccessCodeNotification, User.GetLocale(), new TradeSecretAccessCodeNofication()
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email =  user.Email,
                    AccessCode = req.Token,
                    CallToAction = _localizer["View Trade Secret"].ToString(),
                    CallToActionUrl = callToActionUrl,
                    LogoUrl = Url.CPiLogoLink(Request.Scheme)
                });

            await _notificationService.SendAlert(User.GetUserName(), user.Email, _localizer["Trade secret request is granted"].ToString(), _localizer["An access code was sent to your email address."].ToString(), callToActionUrl, TradeSecretHelper.RequestExpiration);

            if (emailMessage == null)
                return await _emailSender.SendEmailAsync(user.Email, _localizer["Trade Secret Access Code"].ToString(), _localizer["Your trade secret access code is: {0}", req.Token].ToString());
            else
                return await _emailSender.SendEmailAsync(user.Email, emailMessage.Subject, emailMessage.Body);
        }

        private string GetCallToActionUrl(TradeSecretRequest req)
        {
            var detailLink = Url.GetTradeSecretDetailLink(req.ScreenId, req.RecId, Request.Scheme);
            var ts = req.RequestId.ToString().Encrypt(User.GetEncryptionKey());

            return $"{detailLink}?ts={ts}";
        }

        public async Task<IActionResult> GetPicklistData([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType, string requiredRelation = "")
        {
            var result = await GetPicklistData(TradeSecretRequests, request, property, text, filterType, requiredRelation);
            return result;
        }

        public async Task<IActionResult> GetEmails([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType, string requiredRelation = "")
        {
            var result = await TradeSecretRequests.Select(r => new { Email = r.CPiUser.Email, UserId = r.UserId }).Distinct().OrderBy(r => r.Email).ToListAsync();    
            return Json(result);
        }
    }
}