using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using R10.Core;
using R10.Core.Entities;
using R10.Core.Exceptions;
using R10.Core.Helpers;
using R10.Core.Identity;
using R10.Core.Interfaces;
using R10.Web.Areas.Admin.ViewModels;
using R10.Web.Areas.Admin.Views;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;
using R10.Web.Extensions.ActionResults;
using R10.Web.Filters;
using R10.Web.Interfaces;
using R10.Web.Models;
using R10.Web.Models.PageViewModels;
using R10.Web.Security;
using R10.Web.Services;
using System.Data.SqlClient;
using System.Security.Claims;
using AutoMapper.QueryableExtensions;
using R10.Web.Helpers;
using R10.Web.Areas.Admin.Helpers;
using Microsoft.Extensions.Configuration;
using System.IO.Pipelines;
using R10.Core.Entities.Shared;

using R10.Web.Areas;

namespace R10.Web.Areas.Admin.Controllers
{
    [Area("Admin"), Authorize(Policy = CPiAuthorizationPolicy.Administrator)]
    public class UserController : BaseController
    {
        private readonly CPiUserManager _userManager;
        private readonly IEmailSender _emailSender;
        private readonly ICPiUserPermissionManager _permissionManager;
        private readonly CPiIdentitySettings _cpiSettings;
        private readonly ICPiUserSettingManager _settingManager;
        private readonly IUserAccountService _userAccountService;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        private readonly ISystemSettings<DefaultSetting> _settings;

        private int _loginInactiveDays;

        public UserController(
            CPiUserManager userManager,
            IEmailSender emailSender,
            ICPiUserPermissionManager permissionManager,
            IOptions<CPiIdentitySettings> cpiSettings,
            ICPiUserSettingManager settingManager,
            IUserAccountService userAccountService,
            IStringLocalizer<SharedResource> localizer,
            ILogger<UserController> logger,
            IConfiguration configuration,
            ISystemSettings<DefaultSetting> settings
            )
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _permissionManager = permissionManager;
            _cpiSettings = cpiSettings.Value;
            _loginInactiveDays = _cpiSettings.SignIn.InactiveDays;
            _settingManager = settingManager;
            _userAccountService = userAccountService;
            _localizer = localizer;
            _logger = logger;
            _configuration = configuration;
            _settings = settings;
        }

        private string DataContainer => "adminUserDetail";
        private string SidebarTitle => _localizer[AdminNavPages.SidebarTitle].ToString();
        private string SidebarPartialView => "_AdminNav";

        private IQueryable<CPiUser> UserList
        {
            get
            {
                var users = _userManager.Users;

                //Hide CPi admins from client admins
                if (!User.IsSuper())
                    users = users.Where(u => u.UserType != CPiUserType.SuperAdministrator);

                //Hide ContactPerson if systems that use ContactPerson user type are disabled
                if (!User.IsSystemWithContactPersonEnabled())
                    users = users.Where(u => u.UserType != CPiUserType.ContactPerson);

                //Hide Attorney if systems that use Attorney user type are disabled
                if (!User.IsSystemWithAttorneyEnabled())
                    users = users.Where(u => u.UserType != CPiUserType.Attorney);

                //Hide Inventor if DMS is disabled
                if (!User.IsSystemEnabled(SystemType.DMS))
                    users = users.Where(u => u.UserType != CPiUserType.Inventor);

                return users;
            }
        }

        public async Task<IActionResult> Index()
        {
            var model = new PageViewModel()
            {
                //Page = PageType.Search, //not used
                PageId = "adminUserSearch",
                Title = _localizer["Users"].ToString(),
                CanAddRecord = true // (await _authService.AuthorizeAsync(User, AMSAuthorizationPolicy.AuxiliaryModify)).Succeeded
            };

            var sidebarModel = new SidebarPageViewModel()
            {
                Title = SidebarTitle,
                PageTitle = model.Title,
                PageId = model.PageId,
                //MainPartialView = "_SidebarSearchPage", //search page is not used
                MainPartialView = "_SidebarSearchResultsPage",
                MainViewModel = model,
                SideBarPartialView = SidebarPartialView,
                SideBarViewModel = AdminNavPages.Users
            };

            if (Request.IsAjax())
                return PartialView("Index", sidebarModel);

            return View(sidebarModel);
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
                var users = UserList.AddCriteria(mainSearchFilters)
                                    .ProjectTo<UserListViewModel>(new { userId = User.GetUserIdentifier(), loginInactiveDays = _loginInactiveDays });

                if (request.Sorts != null && request.Sorts.Any())
                    users = users.ApplySorting(request.Sorts);
                else
                    users = users.OrderBy(u => u.LastName);

                var ids = await users.Select(u => u.PkId).ToArrayAsync();

                return Json(new CPiDataSourceResult()
                {
                    Data = await users.ApplyPaging(request.Page, request.PageSize).ToListAsync(),
                    Total = ids.Length,
                    Ids = ids
                });
            }
            return new JsonBadRequest(new { errors = ModelState.Errors() });
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
            var model = new PageViewModel()
            {
                Page = PageType.Detail,
                PageId = page.Container,
                Title = _localizer["User Detail"].ToString(),
                RecordId = detail.PkId,
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
                SideBarViewModel = AdminNavPages.Users
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

        public IActionResult Add(bool fromSearch = false)
        {
            if (!Request.IsAjax())
                return RedirectToAction("Index");

            var page = PrepareAddScreen();
            if (page.Detail == null)
                return RedirectToAction("Index");

            var detail = page.Detail;
            var model = new PageViewModel()
            {
                Page = PageType.Detail,
                PageId = page.Container,
                Title = _localizer["New User"].ToString(),
                RecordId = detail.PkId,
                PagePermission = page,
                Data = detail,
                FromSearch = fromSearch
            };

            var sidebarModel = new SidebarPageViewModel()
            {
                Title = SidebarTitle,
                PageTitle = model.Title,
                PageId = model.PageId,
                MainPartialView = "_SidebarDetailPage",
                MainViewModel = model,
                SideBarPartialView = SidebarPartialView,
                SideBarViewModel = AdminNavPages.Users
            };

            if (!fromSearch)
            {
                model.Page = PageType.DetailContent;
                return PartialView("_Index", model);
            }

            return PartialView("Index", sidebarModel);
        }

        public async Task<IActionResult> Copy(int id)
        {
            if (!Request.IsAjax())
                return RedirectToAction("Index");

            var user = await UserList.FirstOrDefaultAsync(u => u.PkId == id);
            if (user == null)
                return BadRequest(_localizer["User not found."].ToString());

            var page = PrepareAddScreen();
            if (page.Detail == null)
                return RedirectToAction("Index");

            var detail = page.Detail;
            //copy user details
            detail.CopyId = user.Id;
            detail.UserType = (int)user.UserType;
            detail.Status = (int)user.Status;
            detail.EntityFilterType = (int)user.EntityFilterType;
            detail.PasswordNeverExpires = user.PasswordNeverExpires ?? false;
            detail.CannotChangePassword = user.CannotChangePassword ?? false;
            detail.WebApiAccessOnly = user.WebApiAccessOnly ?? false;
            detail.ExternalLoginOnly = user.ExternalLoginOnly ?? false;


            var model = new PageViewModel()
            {
                Page = PageType.Detail,
                PageId = page.Container,
                Title = _localizer["Copy User"].ToString(),
                RecordId = detail.PkId,
                PagePermission = page,
                Data = detail,
                FromSearch = false
            };

            model.Page = PageType.DetailContent;
            return PartialView("_Index", model);
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save([FromBody] UserDetailViewModel userDetail)
        {
            if (ModelState.IsValid)
            {
                //invalid date range
                if ((userDetail.ValidDateFrom ?? DateTime.MinValue) > (userDetail.ValidDateTo ?? DateTime.MaxValue))
                    return BadRequest(_localizer["Effective date range is incorrect."].ToString());

                //prevent current user lock out
                var isLoggedInUser = userDetail.Id == User.GetUserIdentifier();
                var today = DateTime.Now.Date;
                if (isLoggedInUser && !(today >= (userDetail.ValidDateFrom ?? DateTime.MinValue) && today <= (userDetail.ValidDateTo ?? DateTime.MaxValue)))
                    return BadRequest(_localizer["Effective dates will lock out current user."].ToString());

                //cpi admin validation
                if ((CPiUserType)userDetail.UserType == CPiUserType.SuperAdministrator && !User.IsSuper())
                    return BadRequest(_localizer["Invalid request."].ToString());

                if (userDetail.PkId > 0)
                    return await Update(userDetail);
                else
                    return await Create(userDetail);
            }
            else
                return new JsonBadRequest(new { errors = ModelState.Errors() });
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await UserList.FirstOrDefaultAsync(u => u.PkId == id);
            if (user == null)
                return BadRequest(_localizer["User not found."].ToString());

            //prevent current user lock out
            if (user.Id == User.GetUserIdentifier())
                return BadRequest(_localizer["Cannot delete logged in account."].ToString());

            //cpi admin validation
            if (user.UserType == CPiUserType.SuperAdministrator && !User.IsSuper())
                return BadRequest(_localizer["Invalid request."].ToString());

            var result = await _userManager.DeleteAsync(user);

            if (result.Succeeded)
                return Ok();

            return new JsonBadRequest(new { errors = LogErrors(result) });
        }

        private async Task<UserDetailViewModel> GetUserDetailViewModel(int pkId)
        {
            return await UserList.Where(u => u.PkId == pkId)
                                       .ProjectTo<UserDetailViewModel>(new { loginInactiveDays = _loginInactiveDays })
                                       .SingleOrDefaultAsync();
        }

        private async Task<DetailPageViewModel<UserDetailViewModel>> PrepareEditScreen(int id)
        {
            var detail = await GetUserDetailViewModel(id);
            var viewModel = new DetailPageViewModel<UserDetailViewModel>{ Detail = detail };
            var isLoggedInUser = detail.Id == User.GetUserIdentifier();

            if (detail != null)
            {
                viewModel.CanAddRecord = true;
                viewModel.CanEditRecord = true;
                viewModel.CanDeleteRecord = (detail.Id != User.GetUserIdentifier());
                viewModel.CanCopyRecord = true;
                viewModel.CanPrintRecord = false;

                this.AddDefaultNavigationUrls(viewModel);

                viewModel.Container = DataContainer;

                viewModel.SearchScreenUrl = Url.Action("Index");
                viewModel.EditScreenUrl = Url.Action("Detail", new { id = id });
                viewModel.CopyScreenUrl = Url.Action("Copy", new { id = id });
                viewModel.IsCopyScreenPopup = false;

                var showPopUp = 0;

                if (TempData.ContainsKey("ShowPopUp"))
                    showPopUp = (TempData["ShowPopUp"] as int?) ?? 0;

                var pageActions = new List<DetailPageAction>();

                // Entity Filter and Account Settings buttons removed — all users have full r/w access

                if (!isLoggedInUser && !string.IsNullOrEmpty(detail.PasswordHash))
                    pageActions.Add(new DetailPageAction()
                    {
                        Label = _localizer["Reset Password"].ToString(),
                        Class = "cpi-confirm reset-password", //call generic confirm dialog
                        IconClass = "fal fa-key",
                        Url = Url.Action("ResetPassword"),
                        Data = new Dictionary<string, string>() { { "get-id", detail.PkId.ToString() }, { "confirm-buttons", "saveButtons" } }
                    });

                if (detail.RequiresConfirmedEmail)
                    pageActions.Add(new DetailPageAction()
                    {
                        Label = _localizer["Resend Email Confirmation"].ToString(),
                        Class = "cpi-confirm resend-email-confirmation", //call generic confirm dialog
                        IconClass = "fal fa-paper-plane",
                        Url = Url.Action("ResendEmailConfirmation"),
                        Data = new Dictionary<string, string>() { { "get-id", detail.PkId.ToString() }, { "confirm-buttons", "sendEmailButtons" } }
                    });

                if (detail.Inactive)
                    pageActions.Add(new DetailPageAction()
                    {
                        Label = _localizer["Reactivate Account"].ToString(),
                        Class = "cpi-confirm reactivate-account", //call generic confirm dialog
                        IconClass = "fal fa-user-unlock",
                        Url = Url.Action("ReactivateAccount"),
                        Data = new Dictionary<string, string>() { { "get-id", detail.PkId.ToString() }, { "confirm-buttons", "reactivateButtons" } }
                    });

                if (detail.IsLockedOut)
                    pageActions.Add(new DetailPageAction()
                    {
                        Label = _localizer["Unlock Account"].ToString(),
                        Class = "cpi-confirm unlock-account", //call generic confirm dialog
                        IconClass = "fal fa-unlock",
                        Url = Url.Action("UnlockAccount"),
                        Data = new Dictionary<string, string>() { { "get-id", detail.PkId.ToString() }, { "confirm-buttons", "unlockButtons" } }
                    });

                viewModel.PageActions = pageActions;
            }
            return viewModel;
        }

        private DetailPageViewModel<UserDetailViewModel> PrepareAddScreen()
        {
            var viewModel = new DetailPageViewModel<UserDetailViewModel>
            {
                Detail = new UserDetailViewModel()
            };

            this.AddDefaultNavigationUrls(viewModel);
            viewModel.Container = DataContainer;
            return viewModel;
        }

        private async Task<IActionResult> Update(UserDetailViewModel userDetail)
        {
            var user = await _userManager.FindByIdAsync(userDetail.Id);
            if (user == null)
                return BadRequest(_localizer["User not found."].ToString());
            else
            {
                //check if new email is in use
                var newEmail = user.Email != userDetail.Email;
                if (newEmail)
                {
                    var userCheck = await _userManager.FindByEmailAsync(userDetail.Email);
                    if (userCheck != null)
                        return BadRequest(_localizer["Another account is using {0}.", userDetail.Email].ToString());
                }
                //register/unregister Outlook Add-in Client
                if(user.UseOutlookAddIn != userDetail.UseOutlookAddIn)
                {
                    if (userDetail.UseOutlookAddIn)
                    {
                        var tuple = await AddOutlookClientRegistration(user);
                        if (tuple != null)
                        {
                            user.ClientId = tuple.Item1;
                            await SendOutlookAddInRegistrationEmail(user, tuple);
                        }
                    }
                    else
                    {
                        bool res = await DeleteOutlookClientRegistration(user);
                        if(res) user.ClientId = null;
                    }
                }

                bool newAccessLevel = (user.UserType != (CPiUserType)userDetail.UserType);
                bool newStatus = (user.Status != (CPiUserStatus)userDetail.Status);

                user.UserName = userDetail.Email;
                user.Email = userDetail.Email;
                user.FirstName = userDetail.FirstName;
                user.LastName = userDetail.LastName;
                user.UserType = (CPiUserType)userDetail.UserType;
                user.Status = (CPiUserStatus)userDetail.Status;
                user.PasswordNeverExpires = userDetail.PasswordNeverExpires;
                user.ValidDateFrom = userDetail.ValidDateFrom;
                user.ValidDateTo = userDetail.ValidDateTo;
                user.CannotChangePassword = userDetail.CannotChangePassword;
                user.UseOutlookAddIn = userDetail.UseOutlookAddIn;
                user.HourlyRate = userDetail.HourlyRate;
                user.WebApiAccessOnly = userDetail.WebApiAccessOnly;
                user.ExternalLoginOnly = userDetail.ExternalLoginOnly;

                if (newAccessLevel)
                    user.EntityFilterType = user.DefaultEntityFilterType;

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    //reset default widgets
                    await _userManager.ResetDefaultWidgets(user);

                    if (newAccessLevel)
                    {
                        result = await _userManager.AddDefaultRolesAsync(user);

                        if (result.Succeeded)
                            await _permissionManager.ResetSettings(user);
                        else
                            LogErrors(result);

                        //link contact/inventor/attorney
                        await LinkUserAccount(user);
                    }

                    //send approval status
                    if (newStatus && (CPiUserStatus)userDetail.Status == CPiUserStatus.Approved)
                    {
                        //link contact/inventor/attorney
                        await LinkUserAccount(user);

                        if (user.LastLoginDate == null)
                            await SendApprovalEmail(user);
                    }

                    var popup = user.UserType.HasLinkedEntity() && (newEmail || newAccessLevel || newStatus) ? "account-settings" : "";

                    return Json(new { id = userDetail.PkId, openPopup = popup});
                }
                else
                    return new JsonBadRequest(new { errors = LogErrors(result) });
            }
        }

        private async Task<IActionResult> Create(UserDetailViewModel userDetail)
        {
            var user = await _userManager.FindByEmailAsync(userDetail.Email);
            if (user == null)
            {
                user = new CPiUser();

                user.UserName = userDetail.Email;
                user.Email = userDetail.Email;
                user.FirstName = userDetail.FirstName;
                user.LastName = userDetail.LastName;
                user.UserType = (CPiUserType)userDetail.UserType;
                user.Status = (CPiUserStatus)userDetail.Status;
                user.EntityFilterType = userDetail.EntityFilterType == 0 ? user.DefaultEntityFilterType : (CPiEntityType)userDetail.EntityFilterType;
                user.PasswordNeverExpires = userDetail.PasswordNeverExpires;
                user.ValidDateFrom = userDetail.ValidDateFrom;
                user.ValidDateTo = userDetail.ValidDateTo;
                user.CannotChangePassword = userDetail.CannotChangePassword;
                user.UseOutlookAddIn = userDetail.UseOutlookAddIn;
                user.HourlyRate = userDetail.HourlyRate;
                user.WebApiAccessOnly = userDetail.WebApiAccessOnly;
                user.ExternalLoginOnly = userDetail.ExternalLoginOnly;

                if (!userDetail.RequireChangePassword)
                {
                    user.LastPasswordChangeDate = DateTime.Now;
                }

                if (userDetail.UseOutlookAddIn)
                {
                    var tuple = await AddOutlookClientRegistration(user);
                    if (tuple != null)
                    {
                        user.ClientId = tuple.Item1;
                        await SendOutlookAddInRegistrationEmail(user, tuple);
                    }
                }
                string newPassword = userDetail.Password ?? "";
                var result = await _userManager.CreateAsync(user, newPassword);

                //copy user
                if (result.Succeeded && !string.IsNullOrEmpty(userDetail.CopyId))
                {
                    var userFrom = await _userManager.FindByIdAsync(userDetail.CopyId);
                    result = await _permissionManager.CopyUserRoles(userFrom, user);

                    //rollback if something went wrong
                    if (!result.Succeeded)
                        await _userManager.DeleteAsync(user);
                }

                if (result.Succeeded)
                {
                    if (userDetail.EmailNewPassword)
                    {
                        var emailType = await _userAccountService.GetDefaultNewPasswordNotification(userDetail.RequireChangePassword);
                        var sendResult = await _userAccountService.SendNewPassword(user.Locale, emailType, new UserAccountEmail()
                        {
                            FirstName = user.FirstName,
                            LastName = user.LastName,
                            Email = user.Email,
                            Password = newPassword,
                            CallToAction = _localizer.GetStringWithCulture("Login", user.Locale),
                            CallToActionUrl = Url.LoginLink(Request.Scheme),
                            LogoUrl = Url.CPiLogoLink(Request.Scheme)
                        });

                        if (!sendResult.Success)
                            _logger.LogError(sendResult.ErrorMessage);
                    }

                    //link contact/inventor/attorney
                    var entityId = (userDetail.EntityId ?? 0);
                    if (entityId > 0)
                        await _permissionManager.LinkEntity(user, entityId, user.EntityFilterType);
                    else
                        await LinkUserAccount(user);

                    var popup = user.UserType.HasLinkedEntity() ? "account-settings" : "";

                    return Json(new { id = user.PkId, openPopup = popup });
                }
                else
                    return new JsonBadRequest(new { errors = LogErrors(result) });
            }
            else
                return BadRequest(_localizer["Another account is using {0}.", userDetail.Email].ToString());
        }

        private async Task SendApprovalEmail(CPiUser user)
        {
            //send approval email
            var sendResult = await _userAccountService.SendApprovalNotification(user.Locale, new UserAccountApprovalNotification()
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                CallToAction = _localizer.GetStringWithCulture("Login", user.Locale),
                CallToActionUrl = Url.LoginLink(Request.Scheme),
                LogoUrl = Url.CPiLogoLink(Request.Scheme)
            });

            if (!sendResult.Success)
                _logger.LogError(sendResult.ErrorMessage);
        }

        private async Task LinkUserAccount(CPiUser user)
        {
            try
            {
                await _userAccountService.LinkUserAccount(user);
            }
            catch(Exception e)
            {
                _logger.LogError(e, e.Message);
            }
        }

        private List<string> LogErrors(IdentityResult result)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();

            foreach (var error in errors)
            {
                _logger.LogError(error);
            }
            return errors;
        }

        [HttpPost]
        public IActionResult GeneratePassword()
        {
            try
            {
                var password = _userManager.GenerateRandomPassword();
                return Json(new { password = password });
            }
            catch
            {
                return BadRequest(_localizer["Unable to generate random password."].ToString());
            }
        }

        [HttpGet]
        public async Task<IActionResult> ResetPassword(int id)
        {
            return PartialView("_ResetPassword", await GetUserDetailViewModel(id));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword([FromBody] UserDetailViewModel userDetail)
        {
            if (userDetail != null || userDetail.Password == userDetail.ConfirmPassword)
            {
                var user = await UserList.FirstOrDefaultAsync(u => u.PkId == userDetail.PkId);
                if (user == null)
                    return BadRequest(_localizer["User not found."].ToString());
                else if (string.IsNullOrEmpty(user.PasswordHash)) //user has no password
                    return BadRequest(_localizer["Invalid request."].ToString());
                else
                {
                    string newPassword = userDetail.Password;
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);

                    IdentityResult result = await _userManager.ResetPasswordAsync(user, token, newPassword);

                    if (result.Succeeded)
                    {
                        if (userDetail.RequireChangePassword)
                        {
                            user.LastPasswordChangeDate = null;

                            result = await _userManager.UpdateAsync(user);
                            if (!result.Succeeded)
                                LogErrors(result);
                        }

                        if (userDetail.EmailNewPassword)
                        {

                            var emailType = await _userAccountService.GetDefaultNewPasswordNotification(userDetail.RequireChangePassword);
                            var sendResult = await _userAccountService.SendNewPassword(user.Locale, emailType, new UserAccountEmail()
                            {
                                FirstName = user.FirstName,
                                LastName = user.LastName,
                                Email = user.Email,
                                Password = newPassword,
                                CallToAction = _localizer.GetStringWithCulture("Login", user.Locale),
                                CallToActionUrl = Url.LoginLink(Request.Scheme),
                                LogoUrl = Url.CPiLogoLink(Request.Scheme)
                            });

                            if (!sendResult.Success)
                                _logger.LogError(sendResult.ErrorMessage);
                        }

                        return Ok(new { message = _localizer["Password successfully updated."].ToString() });
                    }
                    else
                        return new JsonBadRequest(new { errors = LogErrors(result) });
                }
            }
            else
                return BadRequest(_localizer["Invalid request."].ToString());
        }

        [HttpGet]
        public async Task<IActionResult> ReactivateAccount(int id)
        {
            var user = await GetUserDetailViewModel(id);
            if (user == null)
                return BadRequest(_localizer["Invalid request."].ToString());

            return PartialView("_UserActionDialog", new UserActionViewModel() { 
                UserDetail = user, 
                Message = _localizer["Are you sure you want to reactivate {0}?", user.FullName],
                Action = (int)UpdateAction.Reactivate
            } );
        }

        [HttpGet]
        public async Task<IActionResult> UnlockAccount(int id)
        {
            var user = await GetUserDetailViewModel(id);
            if (user == null)
                return BadRequest(_localizer["Invalid request."].ToString());

            return PartialView("_UserActionDialog", new UserActionViewModel()
            {
                UserDetail = user,
                Message = _localizer["Are you sure you want to unlock {0}?", user.FullName],
                Action = (int)UpdateAction.Unlock
            });
        }

        [HttpGet]
        public async Task<IActionResult> ResendEmailConfirmation(int id)
        {
            var user = await GetUserDetailViewModel(id);
            if (user == null)
                return BadRequest(_localizer["Invalid request."].ToString());

            return PartialView("_UserActionDialog", new UserActionViewModel()
            {
                UserDetail = user,
                Message = _localizer["Are you sure you want to resend confirmation email to {0}?", user.FullName],
                Action = (int)UpdateAction.Resend
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update([FromBody] UserUpdateAction update)
        {
            var action = (UpdateAction)update.Action;
            var user = await _userManager.FindByIdAsync(update.UserId);

            if (user == null)
                return BadRequest(_localizer["Invalid request."].ToString());

            if (user.Id == User.GetUserIdentifier())
                return BadRequest(_localizer["Invalid request."].ToString());

            var message = "";
            switch (action)
            {
                case UpdateAction.Enable:
                    user.Status = CPiUserStatus.Approved;
                    message = _localizer["{0} successfully enabled.", user.FullName].ToString();
                    break;

                case UpdateAction.Approve:
                    user.Status = CPiUserStatus.Approved;
                    message = _localizer["{0} successfully approved.", user.FullName].ToString();
                    break;

                case UpdateAction.Reject:
                    user.Status = CPiUserStatus.Rejected;
                    message = _localizer["{0} successfully rejected.", user.FullName].ToString();
                    break;

                case UpdateAction.Disable:
                    user.Status = CPiUserStatus.Disabled;
                    message = _localizer["{0} successfully disabled.", user.FullName].ToString();
                    break;

                case UpdateAction.Reactivate:
                    user.LastLoginDate = null;
                    message = _localizer["{0} successfully reactivated.", user.FullName].ToString();
                    break;

                case UpdateAction.Unlock:
                    if (user.IsLockedOut)
                    {
                        user.LockoutEnd = null;
                        message = _localizer["{0} successfully unlocked.", user.FullName].ToString();
                    }
                    break;

                case UpdateAction.Resend:
                    if (user.RequiresConfirmedEmail)
                    {
                        //todo: use email template
                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        var callbackUrl = Url.EmailConfirmationLink(user.Id, code, Request.Scheme);
                        var sendResult = await _emailSender.SendEmailConfirmationAsync(user, callbackUrl);

                        if (sendResult.Success)
                            return Ok(new { id = user.PkId, message = _localizer["Successfully sent confirmation email to {0}.", user.FullName].ToString() });
                        else
                        {
                            _logger.LogError(sendResult.ErrorMessage);
                            return BadRequest(sendResult.ErrorMessage);
                        }
                    }
                    else
                        return BadRequest(_localizer["Invalid request."].ToString());

                default:
                    return BadRequest(_localizer["Invalid request."].ToString());
            }

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                //inventor approval
                if (action == UpdateAction.Approve)
                    await LinkUserAccount(user);

                return Ok(new { id = user.PkId, message = message });
            }
            else
                return new JsonBadRequest(new { errors = LogErrors(result) });
        }


        public async Task<IActionResult> SetEntityFilter(int id)
        {
            var user = await GetUserDetailViewModel(id);
            if (user == null || !((CPiUserType)user.UserType).IsRegularUser())
                return BadRequest(_localizer["Invalid request."].ToString());

            return PartialView("_SetEntityFilter", user);
        }

        public async Task<IActionResult> AccountSettings(int id)
        {
            ////check policy if only cpiadmin has access to settings
            //ViewData.Model = new UserAccountSettings();
            //CPiSetting setting = await _settingManager.GetCPiSetting(CPiSettings.UserAccountSettings);
            //if (setting != null && (setting.Policy == "*" || (await _authorizationService.AuthorizeAsync(User, setting.Policy)).Succeeded))
            //{
            //    ViewData.Model = await _settingManager.GetUserSetting<UserAccountSettings>(userId);
            //}
            var user = await UserList.FirstOrDefaultAsync(u => u.PkId == id);
            if (user == null)
                return BadRequest(_localizer["Invalid request."].ToString());

            var userId = user.Id;
            var entityId = 0;
            var entityName = "";
            //var isReviewer = false;
            //var isDecisionMaker = false;

            if (user.UserType == CPiUserType.Inventor)
            {
                var entityFilter = await _permissionManager.UserEntityFilter(CPiEntityType.Inventor, userId).FirstOrDefaultAsync();
                if (entityFilter != null)
                {
                    entityId = entityFilter.Id; // await _permissionManager.UserEntityFilter(CPiEntityType.Inventor, userId).Select(e => e.Id).FirstOrDefaultAsync();
                    entityName = entityFilter.Name; // await _inventorService.QueryableList.Where(i => i.InventorID == entityId).Select(i => i.Inventor).FirstOrDefaultAsync();
                }

                //isReviewer = await _permissionManager.UserHasSystemPermission(user.Id, SystemType.DMS, CPiPermissions.Reviewer);
                //var inventor = await _inventorService.QueryableList.Where(i => i.InventorID == entityId).Select(i => new { Inventor = i.Inventor, IsReviewer = i.IsReviewer }).FirstOrDefaultAsync();
                //if (inventor != null)
                //{
                //    entityName = inventor.Inventor;
                //    isReviewer = inventor.IsReviewer ?? false;
                //}
            }

            if (user.UserType == CPiUserType.Attorney)
            {
                var entityFilter = await _permissionManager.UserEntityFilter(CPiEntityType.Attorney, userId).FirstOrDefaultAsync();
                if (entityFilter != null)
                {
                    entityId = entityFilter.Id; // await _permissionManager.UserEntityFilter(CPiEntityType.Attorney, userId).Select(e => e.Id).FirstOrDefaultAsync();
                    entityName = entityFilter.Name; // await _attorneyService.QueryableList.Where(a => a.AttorneyID == entityId).Select(i => i.AttorneyName).FirstOrDefaultAsync();
                }
            }

            if (user.UserType == CPiUserType.ContactPerson)
            {
                var entityFilter = await _permissionManager.UserEntityFilter(CPiEntityType.ContactPerson, userId).FirstOrDefaultAsync();
                if (entityFilter != null)
                {
                    entityId = entityFilter.Id; // await _permissionManager.UserEntityFilter(CPiEntityType.ContactPerson, userId).Select(e => e.Id).FirstOrDefaultAsync();
                    entityName = entityFilter.Name; // await _contactPersonService.QueryableList.Where(c => c.ContactID == entityId).Select(c => c.ContactName).FirstOrDefaultAsync();
                }

                //isReviewer = await _permissionManager.UserHasSystemPermission(user.Id, SystemType.DMS, CPiPermissions.Reviewer);
                //isDecisionMaker = await _permissionManager.UserHasSystemPermission(user.Id, SystemType.AMS, CPiPermissions.DecisionMaker);
            }

            var userRoles = await _permissionManager.GetUserRoles(user);

            var model = new AccountSettingsViewModel()
            {
                UserId = user.Id,
                Settings = await _settingManager.GetUserSetting<UserAccountSettings>(userId),
                NotificationSettings = await _settingManager.GetUserSetting<UserNotificationSettings>(userId),
                UserType = user.UserType,
                EntityId = entityId,
                EntityName = entityName,
                //IsReviewer = isReviewer,
                //IsDecisionMaker = isDecisionMaker,
                IsAdmin = user.IsAdmin,
                IsReviewer = userRoles.Any(r => r.SystemId == SystemType.DMS && CPiPermissions.Reviewer.Contains(r.RoleId.ToLower())),
                IsAMSDecisionMaker = userRoles.Any(r => r.SystemId == SystemType.AMS && CPiPermissions.DecisionMaker.Contains(r.RoleId.ToLower())),
                IsRMSDecisionMaker = userRoles.Any(r => r.SystemId == SystemType.RMS && CPiPermissions.DecisionMaker.Contains(r.RoleId.ToLower())),
                IsForeignFilingDecisionMaker = userRoles.Any(r => r.SystemId == SystemType.ForeignFiling && CPiPermissions.DecisionMaker.Contains(r.RoleId.ToLower())),
                CanReceiveAMSNotifications = await _permissionManager.CanReceiveAMSNotifications(userId),
                CanReceiveRMSNotifications = await _permissionManager.CanReceiveRMSNotifications(userId),
                CanReceiveFFNotifications = await _permissionManager.CanReceiveFFNotifications(userId),
                CanReceiveDeDocketNotifications = await _permissionManager.CanReceiveDeDocketNotifications(userId),
                HasAMS = User.IsSystemEnabled(SystemType.AMS) && await _permissionManager.UserHasSystemPermission(userId, SystemType.AMS, CPiPermissions.RegularUser),
                HasRMS = User.IsSystemEnabled(SystemType.RMS) && await _permissionManager.UserHasSystemPermission(userId, SystemType.RMS, CPiPermissions.RegularUser),
                HasForeignFiling = User.IsSystemEnabled(SystemType.ForeignFiling) && await _permissionManager.UserHasSystemPermission(userId, SystemType.ForeignFiling, CPiPermissions.RegularUser),
                HasDMS = User.IsSystemEnabled(SystemType.DMS) && await _permissionManager.UserHasSystemPermission(userId, SystemType.DMS, CPiPermissions.RegularUser),
                HasPatent = User.IsSystemEnabled(SystemType.Patent) && await _permissionManager.UserHasSystemPermission(userId, SystemType.Patent, CPiPermissions.RegularUser),
                HasTrademark = User.IsSystemEnabled(SystemType.Trademark) && await _permissionManager.UserHasSystemPermission(userId, SystemType.Trademark, CPiPermissions.RegularUser),
                HasGeneralMatter = User.IsSystemEnabled(SystemType.GeneralMatter) && await _permissionManager.UserHasSystemPermission(userId, SystemType.GeneralMatter, CPiPermissions.RegularUser),
                AMSAuxiliaryRole = userRoles.Where(r => r.SystemId == SystemType.AMS && CPiPermissions.Auxiliary.Contains(r.RoleId.ToLower())).Select(r => r.RoleId).FirstOrDefault(),
                RMSAuxiliaryRole = userRoles.Where(r => r.SystemId == SystemType.RMS && CPiPermissions.Auxiliary.Contains(r.RoleId.ToLower())).Select(r => r.RoleId).FirstOrDefault(),
                ForeignFilingAuxiliaryRole = userRoles.Where(r => r.SystemId == SystemType.ForeignFiling && CPiPermissions.Auxiliary.Contains(r.RoleId.ToLower())).Select(r => r.RoleId).FirstOrDefault(),
                DMSAuxiliaryRole = userRoles.Where(r => r.SystemId == SystemType.DMS && CPiPermissions.Auxiliary.Contains(r.RoleId.ToLower())).Select(r => r.RoleId).FirstOrDefault(),
                PatentAuxiliaryRole = userRoles.Where(r => r.SystemId == SystemType.Patent && CPiPermissions.Auxiliary.Contains(r.RoleId.ToLower())).Select(r => r.RoleId).FirstOrDefault(),
                TrademarkAuxiliaryRole = userRoles.Where(r => r.SystemId == SystemType.Trademark && CPiPermissions.Auxiliary.Contains(r.RoleId.ToLower())).Select(r => r.RoleId).FirstOrDefault(),
                GeneralMatterAuxiliaryRole = userRoles.Where(r => r.SystemId == SystemType.GeneralMatter && CPiPermissions.Auxiliary.Contains(r.RoleId.ToLower())).Select(r => r.RoleId).FirstOrDefault(),
                PatentCountryLawRole = userRoles.Where(r => r.SystemId == SystemType.Patent && CPiPermissions.CountryLaw.Contains(r.RoleId.ToLower())).Select(r => r.RoleId).FirstOrDefault(),
                TrademarkCountryLawRole = userRoles.Where(r => r.SystemId == SystemType.Trademark && CPiPermissions.CountryLaw.Contains(r.RoleId.ToLower())).Select(r => r.RoleId).FirstOrDefault(),
                GeneralMatterCountryLawRole = userRoles.Where(r => r.SystemId == SystemType.GeneralMatter && CPiPermissions.CountryLaw.Contains(r.RoleId.ToLower())).Select(r => r.RoleId).FirstOrDefault(),
                PatentActionTypeRole = userRoles.Where(r => r.SystemId == SystemType.Patent && CPiPermissions.ActionType.Contains(r.RoleId.ToLower())).Select(r => r.RoleId).FirstOrDefault(),
                TrademarkActionTypeRole = userRoles.Where(r => r.SystemId == SystemType.Trademark && CPiPermissions.ActionType.Contains(r.RoleId.ToLower())).Select(r => r.RoleId).FirstOrDefault(),
                GeneralMatterActionTypeRole = userRoles.Where(r => r.SystemId == SystemType.GeneralMatter && CPiPermissions.ActionType.Contains(r.RoleId.ToLower())).Select(r => r.RoleId).FirstOrDefault(),
                PatentLetterRole = userRoles.Where(r => r.SystemId == SystemType.Patent && CPiPermissions.Letters.Contains(r.RoleId.ToLower())).Select(r => r.RoleId).FirstOrDefault(),
                TrademarkLetterRole = userRoles.Where(r => r.SystemId == SystemType.Trademark && CPiPermissions.Letters.Contains(r.RoleId.ToLower())).Select(r => r.RoleId).FirstOrDefault(),
                GeneralMatterLetterRole = userRoles.Where(r => r.SystemId == SystemType.GeneralMatter && CPiPermissions.Letters.Contains(r.RoleId.ToLower())).Select(r => r.RoleId).FirstOrDefault(),
                PatentCustomQueryRole = userRoles.Where(r => r.SystemId == SystemType.Patent && CPiPermissions.CustomQuery.Contains(r.RoleId.ToLower())).Select(r => r.RoleId).FirstOrDefault(),
                TrademarkCustomQueryRole = userRoles.Where(r => r.SystemId == SystemType.Trademark && CPiPermissions.CustomQuery.Contains(r.RoleId.ToLower())).Select(r => r.RoleId).FirstOrDefault(),
                GeneralMatterCustomQueryRole = userRoles.Where(r => r.SystemId == SystemType.GeneralMatter && CPiPermissions.CustomQuery.Contains(r.RoleId.ToLower())).Select(r => r.RoleId).FirstOrDefault(),
                AMSCustomQueryRole = userRoles.Where(r => r.SystemId == SystemType.AMS && CPiPermissions.CustomQuery.Contains(r.RoleId.ToLower())).Select(r => r.RoleId).FirstOrDefault(),
                PatentProductsRole = userRoles.Where(r => r.SystemId == SystemType.Patent && CPiPermissions.Products.Contains(r.RoleId.ToLower())).Select(r => r.RoleId).FirstOrDefault(),
                TrademarkProductsRole = userRoles.Where(r => r.SystemId == SystemType.Trademark && CPiPermissions.Products.Contains(r.RoleId.ToLower())).Select(r => r.RoleId).FirstOrDefault(),
                GeneralMatterProductsRole = userRoles.Where(r => r.SystemId == SystemType.GeneralMatter && CPiPermissions.Products.Contains(r.RoleId.ToLower())).Select(r => r.RoleId).FirstOrDefault(),
                AMSProductsRole = userRoles.Where(r => r.SystemId == SystemType.AMS && CPiPermissions.Products.Contains(r.RoleId.ToLower())).Select(r => r.RoleId).FirstOrDefault(),
                CanUploadPatent = userRoles.Any(r => r.SystemId == SystemType.Patent && CPiPermissions.CanUploadDocuments.Contains(r.RoleId.ToLower())),
                CanUploadTrademark = userRoles.Any(r => r.SystemId == SystemType.Trademark && CPiPermissions.CanUploadDocuments.Contains(r.RoleId.ToLower())),
                CanUploadGeneralMatter = userRoles.Any(r => r.SystemId == SystemType.GeneralMatter && CPiPermissions.CanUploadDocuments.Contains(r.RoleId.ToLower())),

                IsClearanceReviewer = userRoles.Any(r => r.SystemId == SystemType.SearchRequest && CPiPermissions.Reviewer.Contains(r.RoleId.ToLower())),
                HasClearance = User.IsSystemEnabled(SystemType.SearchRequest) && await _permissionManager.UserHasSystemPermission(userId, SystemType.SearchRequest, CPiPermissions.RegularUser),
                ClearanceAuxiliaryRole = userRoles.Where(r => r.SystemId == SystemType.SearchRequest && CPiPermissions.Auxiliary.Contains(r.RoleId.ToLower())).Select(r => r.RoleId).FirstOrDefault(),

                IsPatClearanceReviewer = userRoles.Any(r => r.SystemId == SystemType.PatClearance && CPiPermissions.Reviewer.Contains(r.RoleId.ToLower())),
                HasPatClearance = User.IsSystemEnabled(SystemType.PatClearance) && await _permissionManager.UserHasSystemPermission(userId, SystemType.PatClearance, CPiPermissions.RegularUser),
                PatClearanceAuxiliaryRole = userRoles.Where(r => r.SystemId == SystemType.PatClearance && CPiPermissions.Auxiliary.Contains(r.RoleId.ToLower())).Select(r => r.RoleId).FirstOrDefault(),

                PatentCostEstimatorRole = userRoles.Where(r => r.SystemId == SystemType.Patent && CPiPermissions.CostEstimator.Contains(r.RoleId.ToLower())).Select(r => r.RoleId).FirstOrDefault(),
                TrademarkCostEstimatorRole = userRoles.Where(r => r.SystemId == SystemType.Trademark && CPiPermissions.CostEstimator.Contains(r.RoleId.ToLower())).Select(r => r.RoleId).FirstOrDefault(),

                PatentGermanRemunerationRole = userRoles.Where(r => r.SystemId == SystemType.Patent && CPiPermissions.GermanRemuneration.Contains(r.RoleId.ToLower())).Select(r => r.RoleId).FirstOrDefault(),
                PatentFrenchRemunerationRole = userRoles.Where(r => r.SystemId == SystemType.Patent && CPiPermissions.FrenchRemuneration.Contains(r.RoleId.ToLower())).Select(r => r.RoleId).FirstOrDefault(),

                IsPatentModify = userRoles.Any(r => r.SystemId == SystemType.Patent && CPiPermissions.FullModify.Contains(r.RoleId.ToLower())),
                IsTrademarkModify = userRoles.Any(r => r.SystemId == SystemType.Trademark && CPiPermissions.FullModify.Contains(r.RoleId.ToLower())),
                IsGeneralMattersModify = userRoles.Any(r => r.SystemId == SystemType.GeneralMatter && CPiPermissions.FullModify.Contains(r.RoleId.ToLower())),

                CanHavePatentUploadRole = User.IsSystemEnabled(SystemType.Patent) && (user.UserType == CPiUserType.Attorney || userRoles.Any(r => r.SystemId == SystemType.Patent && CPiPermissions.CanHaveUploadRole.Contains(r.RoleId.ToLower()))),
                CanHaveTrademarkUploadRole = User.IsSystemEnabled(SystemType.Trademark) && (user.UserType == CPiUserType.Attorney || userRoles.Any(r => r.SystemId == SystemType.Trademark && CPiPermissions.CanHaveUploadRole.Contains(r.RoleId.ToLower()))),
                CanHaveGeneralMatterUploadRole = User.IsSystemEnabled(SystemType.GeneralMatter) && (user.UserType == CPiUserType.Attorney || userRoles.Any(r => r.SystemId == SystemType.GeneralMatter && CPiPermissions.CanHaveUploadRole.Contains(r.RoleId.ToLower()))),

                IsPatentScoreModify = userRoles.Any(r => r.SystemId == SystemType.Patent && CPiPermissions.PatentScoreModify.Contains(r.RoleId.ToLower())),

                IsPatentSoftDocket = userRoles.Any(r => r.SystemId == SystemType.Patent && CPiPermissions.SoftDocket.Contains(r.RoleId.ToLower())),
                IsTrademarkSoftDocket = userRoles.Any(r => r.SystemId == SystemType.Trademark && CPiPermissions.SoftDocket.Contains(r.RoleId.ToLower())),
                IsGeneralMatterSoftDocket = userRoles.Any(r => r.SystemId == SystemType.GeneralMatter && CPiPermissions.SoftDocket.Contains(r.RoleId.ToLower())),

                IsPatentRequestDocket = userRoles.Any(r => r.SystemId == SystemType.Patent && CPiPermissions.RequestDocket.Contains(r.RoleId.ToLower())),
                IsTrademarkRequestDocket = userRoles.Any(r => r.SystemId == SystemType.Trademark && CPiPermissions.RequestDocket.Contains(r.RoleId.ToLower())),
                IsGeneralMatterRequestDocket = userRoles.Any(r => r.SystemId == SystemType.GeneralMatter && CPiPermissions.RequestDocket.Contains(r.RoleId.ToLower())),

                PatentDocumentVerificationRole = userRoles.Where(r => r.SystemId == SystemType.Patent && CPiPermissions.DocumentVerification.Contains(r.RoleId.ToLower())).Select(r => r.RoleId).FirstOrDefault(),
                TrademarkDocumentVerificationRole = userRoles.Where(r => r.SystemId == SystemType.Trademark && CPiPermissions.DocumentVerification.Contains(r.RoleId.ToLower())).Select(r => r.RoleId).FirstOrDefault(),
                GeneralMatterDocumentVerificationRole = userRoles.Where(r => r.SystemId == SystemType.GeneralMatter && CPiPermissions.DocumentVerification.Contains(r.RoleId.ToLower())).Select(r => r.RoleId).FirstOrDefault(),

                PatentWorkflowRole = userRoles.Where(r => r.SystemId == SystemType.Patent && CPiPermissions.Workflow.Contains(r.RoleId.ToLower())).Select(r => r.RoleId).FirstOrDefault(),
                TrademarkWorkflowRole = userRoles.Where(r => r.SystemId == SystemType.Trademark && CPiPermissions.Workflow.Contains(r.RoleId.ToLower())).Select(r => r.RoleId).FirstOrDefault(),
                GeneralMatterWorkflowRole = userRoles.Where(r => r.SystemId == SystemType.GeneralMatter && CPiPermissions.Workflow.Contains(r.RoleId.ToLower())).Select(r => r.RoleId).FirstOrDefault(),
                DMSWorkflowRole = userRoles.Where(r => r.SystemId == SystemType.DMS && CPiPermissions.Workflow.Contains(r.RoleId.ToLower())).Select(r => r.RoleId).FirstOrDefault(),
                PatClearanceWorkflowRole = userRoles.Where(r => r.SystemId == SystemType.PatClearance && CPiPermissions.Workflow.Contains(r.RoleId.ToLower())).Select(r => r.RoleId).FirstOrDefault(),
                ClearanceWorkflowRole = userRoles.Where(r => r.SystemId == SystemType.SearchRequest && CPiPermissions.Workflow.Contains(r.RoleId.ToLower())).Select(r => r.RoleId).FirstOrDefault(),

                IsPreviewer = userRoles.Any(r => r.SystemId == SystemType.DMS && CPiPermissions.Previewer.Contains(r.RoleId.ToLower())),
            };

            return PartialView("_AccountSettings", model);
        }

        public async Task<IActionResult> GetPicklistData([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType, string requiredRelation = "")
        {
            var result =  await GetPicklistData(UserList, request, property, text, filterType, requiredRelation);
            return result;
        }

        public async Task<IActionResult> GetFullNameList([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType, string requiredRelation = "")
        {
            return await GetPicklistData(UserList.Select(u => new { FullName = string.Concat(u.FirstName, " ", u.LastName) }).Distinct(), request, property, text, filterType, requiredRelation, false);
        }

        //todo:not used
        //todo:use antiforgerytoken

        [HttpPost]
        public async Task<JsonResult> PendingCount()
        {
            //number of pending users
            return Json(await _userManager.Users.CountAsync(user => user.Status == CPiUserStatus.Pending));
        }

        public async Task<IActionResult> GetRecordStamps(int id)
        {
            var user = await UserList.FirstOrDefaultAsync(u => u.PkId == id);
            if (user == null)
                return new NoRecordFoundResult();

            return ViewComponent("RecordStamps", new { createdBy = user.CreatedBy, dateCreated = user.DateCreated, updatedBy = user.UpdatedBy, lastUpdate = user.LastUpdate, tStamp = user.tStamp });
        }

        [HttpGet]
        public async Task<IActionResult> DetailLink(string id, int? show)
        {
            if (!string.IsNullOrEmpty(id))
            {
                var user = await UserList.FirstOrDefaultAsync(u => u.Id == id);
                if (user == null)
                    return new RecordDoesNotExistResult();
                else
                {
                    TempData["ShowPopUp"] = (show ?? 0);
                    return RedirectToAction(nameof(Detail), new { id = user.PkId, singleRecord = true, fromSearch = true });
                }
            }
            else
                return RedirectToAction(nameof(Add), new { fromSearch = true });
        }

        [HttpGet]
        private IActionResult AddUser(UserDetailViewModel detail)
        {
            var page = PrepareAddScreen();
            if (page.Detail == null)
                return RedirectToAction("Index");

            var model = new PageViewModel()
            {
                Page = PageType.Detail,
                PageId = page.Container,
                Title = _localizer["New User"].ToString(),
                RecordId = detail.PkId,
                PagePermission = page,
                Data = detail,
                FromSearch = true
            };

            var sidebarModel = new SidebarPageViewModel()
            {
                Title = SidebarTitle,
                PageTitle = model.Title,
                PageId = model.PageId,
                MainPartialView = "_SidebarDetailPage",
                MainViewModel = model,
                SideBarPartialView = SidebarPartialView,
                SideBarViewModel = AdminNavPages.Users
            };

            return PartialView("Index", sidebarModel);
        }

        // ContactPersonService removed during debloat
        /*[HttpGet]
        public async Task<IActionResult> AddContact(int id)
        {
            if (!Request.IsAjax())
                return RedirectToAction("Index");

            var contactPerson = await _contactPersonService.GetByIdAsync(id);
            if (contactPerson == null)
                return new RecordDoesNotExistResult();

            var detail = new UserDetailViewModel();
            detail.UserType = (int)CPiUserType.ContactPerson;
            detail.Status = (int)CPiUserStatus.Approved;
            detail.EntityFilterType = (int)CPiEntityType.ContactPerson;
            detail.EntityId = contactPerson.ContactID;
            detail.FirstName = contactPerson.FirstName ?? "";
            detail.LastName = contactPerson.LastName ?? "";
            detail.Email = contactPerson.EMail ?? "";

            return AddUser(detail);
        }*/

        // PatInventorService removed during debloat
        [HttpGet]
        public IActionResult AddInventor(int id)
        {
            return BadRequest("Inventor service is not available.");
        }

        // AttorneyService removed during debloat
        /*[HttpGet]
        public async Task<IActionResult> AddAttorney(int id)
        {
            if (!Request.IsAjax())
                return RedirectToAction("Index");

            var attorney = await _attorneyService.GetByIdAsync(id);
            if (attorney == null)
                return new RecordDoesNotExistResult();

            var detail = new UserDetailViewModel();
            detail.UserType = (int)CPiUserType.Attorney;
            detail.Status = (int)CPiUserStatus.Approved;
            detail.EntityFilterType = (int)CPiEntityType.Attorney;
            detail.EntityId = attorney.AttorneyID;
            detail.FirstName = attorney.AttorneyName ?? "";
            //detail.FirstName = attorney.FirstName ?? "";
            //detail.LastName = attorney.LastName ?? "";
            detail.Email = attorney.EMail ?? "";

            return AddUser(detail);
        }*/

        private async Task<bool> DeleteOutlookClientRegistration(CPiUser user)
        {
            var res = await _userAccountService.DeleteOutlookAddInClient(user.ClientId);
            if (!res)
            {
                var message = "Unable to unregister the client from OpenId Applications.";
                _logger.LogError(message);
                return false;
            }
            return true;
        }

        private async Task<Tuple<string, string>> AddOutlookClientRegistration(CPiUser user)
        {
            var obj = await _userAccountService.RegisterOutlookAddInClient(user.Email);
            if (!obj.Success)
            {
                var message = "Failed to register the user as a client in Outlook Add-in";
                _logger.LogError(message);
                return null;
            }
            else { return obj.ClientIdSecret; }
        }


        private async Task SendOutlookAddInRegistrationEmail(CPiUser user, Tuple<string, string> clientIdSecret)
        {
            var emailType = "Outlook Add-in Registration";
            var outlookUrl = _configuration.GetValue<string>("EmailAddIn:OutlookOriginUrl") + "/manifest.prod.xml";
            var sendResult = await _userAccountService.SendOutlookAddInRegistration(user.Locale, emailType, new OutlookAddInRegistration()
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                ClientId = clientIdSecret.Item1,
                ClientSecret = clientIdSecret.Item2,
                CallToActionUrl = Url.ApplicationBaseUrl(Request.Scheme),
                LogoUrl = Url.CPiLogoLink(Request.Scheme),
                OutlookUrl = outlookUrl,
                ClientName = (await _settings.GetSetting()).ClientName??""
                //OutlookUrl = "https://outlooktocpiwesteurope.z6.web.core.windows.net/manifest.prod.xml"
            });

            if (!sendResult.Success)
                _logger.LogError(sendResult.ErrorMessage);
        }
    }
}