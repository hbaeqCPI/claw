using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Helpers;
using R10.Core.Identity;
using R10.Core.Interfaces;
using R10.Web.Extensions;
using R10.Web.Interfaces;
using R10.Web.Models;
using R10.Web.Models.ManageViewModels;
using R10.Web.Views.Manage;
using R10.Web.Models.PageViewModels;
using R10.Web.Services;
using R10.Core.Entities.Shared;
using R10.Core.Services;

namespace R10.Web.Controllers
{
    [Authorize]
    //[Route("[controller]/[action]")]
    public class ManageController : Microsoft.AspNetCore.Mvc.Controller
    {
        private readonly CPiUserManager _userManager;
        private readonly SignInManager<CPiUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;
        private readonly UrlEncoder _urlEncoder;
        private readonly ICPiUserDefaultPageManager _settingManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly IOptions<RequestLocalizationOptions> _locOptions;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly ISystemSettings<DefaultSetting> _settings;
        private readonly ICPiSystemSettingManager _systemSettingManager;

        private const string AuthenicatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";        

        public ManageController(
          CPiUserManager userManager,
          SignInManager<CPiUser> signInManager,
          IEmailSender emailSender,
          ILogger<ManageController> logger,
          UrlEncoder urlEncoder,
          ICPiUserDefaultPageManager settingManager,
          IAuthorizationService authorizationService,
          IOptions<RequestLocalizationOptions> locOptions,
          IStringLocalizer<SharedResource> localizer,
          ISystemSettings<DefaultSetting> settings,
          ICPiSystemSettingManager systemSettingManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _logger = logger;
            _urlEncoder = urlEncoder;
            _settingManager = settingManager;
            _authorizationService = authorizationService;
            _locOptions = locOptions;
            _localizer = localizer;
            _settings = settings;
            _systemSettingManager = systemSettingManager;
        }

        private string SidebarTitle => _localizer["My Account"];
        private string SidebarPartialView => "_SidebarNav";

        [TempData]
        public string? StatusMessage { get; set; }

        [HttpGet]
        public IActionResult Index()
        {
            return RedirectToAction("Profile");
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                //throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
                return await UnloadUser();
            }

            var model = new ProfileViewModel
            {
                Username = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                //Email = user.Email,
                //PhoneNumber = user.PhoneNumber,
                Locale = user.Locale,
                IsEmailConfirmed = user.EmailConfirmed,
                StatusMessage = StatusMessage
            };

            return View("Index", new SidebarPageViewModel()
            {
                Title = SidebarTitle,
                PageTitle = _localizer["Profile"],
                PageId = "Profile",
                MainPartialView = "Profile",
                MainViewModel = model,
                SideBarPartialView = SidebarPartialView,
                SideBarViewModel = ManageNavPages.Profile
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", new SidebarPageViewModel()
                {
                    Title = SidebarTitle,
                    PageTitle = _localizer["Profile"],
                    PageId = "Profile",
                    MainPartialView = "Profile",
                    MainViewModel = model,
                    SideBarPartialView = SidebarPartialView,
                    SideBarViewModel = ManageNavPages.Profile
                });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                //throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
                return await UnloadUser();
            }

            //todo: allow user to change email??
            //var email = user.Email;
            //if (model.Email != email)
            //{
            //    var setEmailResult = await _userManager.SetEmailAsync(user, model.Email);
            //    if (!setEmailResult.Succeeded)
            //    {
            //        throw new ApplicationException($"Unexpected error occurred setting email for user with ID '{user.Id}'.");
            //    }
            //}

            //var phoneNumber = user.PhoneNumber;
            //if (model.PhoneNumber != phoneNumber)
            //{
            //    var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, model.PhoneNumber);
            //    if (!setPhoneResult.Succeeded)
            //    {
            //        throw new ApplicationException($"Unexpected error occurred setting phone number for user with ID '{user.Id}'.");
            //    }
            //}

            //update firstname and lastname
            bool updateProfile = false;

            if (model.FirstName != user.FirstName)
            {
                user.FirstName = model.FirstName;
                updateProfile = true;
            }

            if (model.LastName != user.LastName)
            {
                user.LastName = model.LastName;
                updateProfile = true;
            }

            if (model.Locale != user.Locale)
            {
                user.Locale = model.Locale;
                updateProfile = true;
            }

            if (updateProfile)
            {
                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    throw new ApplicationException($"Unexpected error occurred updating name for user with ID '{user.Id}'.");
                }

                //update claims
                await _signInManager.RefreshSignInAsync(user);
            }

            StatusMessage = _localizer["Your profile has been updated."];
            return RedirectToAction(nameof(Index));
        }

        //NOT USED
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> SendVerificationEmail(ProfileViewModel model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return View(model);
        //    }

        //    var user = await _userManager.GetUserAsync(User);
        //    if (user == null)
        //    {
        //        //throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        //        return await UnloadUser();
        //    }

        //    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        //    var callbackUrl = Url.EmailConfirmationLink(user.Id, code, Request.Scheme);
        //    var email = user.Email;
        //    await _emailSender.SendEmailConfirmationAsync(user, callbackUrl);

        //    StatusMessage = _localizer["Verification email sent. Please check your email."];
        //    return RedirectToAction(nameof(Index));
        //}

        [HttpGet]
        public async Task<IActionResult> ChangePassword()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                //throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
                return await UnloadUser();
            }

            if (user.CannotChangePassword ?? false)
                return RedirectToAction(nameof(Index));

            var hasPassword = await _userManager.HasPasswordAsync(user);
            if (!hasPassword)
            {
                return RedirectToAction(nameof(SetPassword));
            }

            var model = new ChangePasswordViewModel { StatusMessage = StatusMessage };
            return View("Index", new SidebarPageViewModel()
            {
                Title = SidebarTitle,
                PageTitle = _localizer["Change Password"],
                PageId = "ChangePassword",
                MainPartialView = "ChangePassword",
                MainViewModel = model,
                SideBarPartialView = SidebarPartialView,
                SideBarViewModel = ManageNavPages.ChangePassword
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", new SidebarPageViewModel()
                {
                    Title = SidebarTitle,
                    PageTitle = _localizer["Change Password"],
                    PageId = "ChangePassword",
                    MainPartialView = "ChangePassword",
                    MainViewModel = model,
                    SideBarPartialView = SidebarPartialView,
                    SideBarViewModel = ManageNavPages.ChangePassword
                });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                //throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
                return await UnloadUser();
            }

            if (user.CannotChangePassword ?? false)
                return RedirectToAction(nameof(Index));

            var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
            if (!changePasswordResult.Succeeded)
            {
                AddErrors(changePasswordResult);
                return View("Index", new SidebarPageViewModel()
                {
                    Title = SidebarTitle,
                    PageTitle = _localizer["Change Password"],
                    PageId = "ChangePassword",
                    MainPartialView = "ChangePassword",
                    MainViewModel = model,
                    SideBarPartialView = SidebarPartialView,
                    SideBarViewModel = ManageNavPages.ChangePassword
                });
            }

            await _signInManager.SignInAsync(user, isPersistent: false);
            _logger.LogInformation("User changed their password successfully.");
            StatusMessage = _localizer["Your password has been changed."];

            return RedirectToAction(nameof(ChangePassword));
        }

        [HttpGet]
        public async Task<IActionResult> SetPassword()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                //throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
                return await UnloadUser();
            }

            if (user.CannotChangePassword ?? false)
                return RedirectToAction(nameof(Index));

            var hasPassword = await _userManager.HasPasswordAsync(user);

            if (hasPassword)
            {
                return RedirectToAction(nameof(ChangePassword));
            }

            var model = new SetPasswordViewModel { StatusMessage = StatusMessage };
            return View("Index", new SidebarPageViewModel()
            {
                Title = SidebarTitle,
                PageTitle = _localizer["Set Password"],
                PageId = "SetPassword",
                MainPartialView = "SetPassword",
                MainViewModel = model,
                SideBarPartialView = SidebarPartialView,
                SideBarViewModel = ManageNavPages.ChangePassword
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetPassword(SetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", new SidebarPageViewModel()
                {
                    Title = SidebarTitle,
                    PageTitle = _localizer["Set Password"],
                    PageId = "SetPassword",
                    MainPartialView = "SetPassword",
                    MainViewModel = model,
                    SideBarPartialView = SidebarPartialView,
                    SideBarViewModel = ManageNavPages.ChangePassword
                });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                //throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
                return await UnloadUser();
            }

            if (user.CannotChangePassword ?? false)
                return RedirectToAction(nameof(Index));

            var addPasswordResult = await _userManager.AddPasswordAsync(user, model.NewPassword);
            if (!addPasswordResult.Succeeded)
            {
                AddErrors(addPasswordResult);
                return View("Index", new SidebarPageViewModel()
                {
                    Title = SidebarTitle,
                    PageTitle = _localizer["Set Password"],
                    PageId = "SetPassword",
                    MainPartialView = "SetPassword",
                    MainViewModel = model,
                    SideBarPartialView = SidebarPartialView,
                    SideBarViewModel = ManageNavPages.ChangePassword
                });
            }

            await _signInManager.SignInAsync(user, isPersistent: false);
            StatusMessage = _localizer["Your password has been set."];

            return RedirectToAction(nameof(SetPassword));
        }

        [HttpGet]
        public async Task<IActionResult> ExternalLogins()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var model = new ExternalLoginsViewModel { CurrentLogins = await _userManager.GetLoginsAsync(user) };
            model.OtherLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync())
                .Where(auth => model.CurrentLogins.All(ul => auth.Name != ul.LoginProvider))
                .ToList();
            model.ShowRemoveButton = await _userManager.HasPasswordAsync(user) || model.CurrentLogins.Count > 1;
            model.StatusMessage = StatusMessage;

            return View("Index", new SidebarPageViewModel()
            {
                Title = SidebarTitle,
                PageTitle = _localizer["External Logins"],
                PageId = "ExternalLogins",
                MainPartialView = "ExternalLogins",
                MainViewModel = model,
                SideBarPartialView = SidebarPartialView,
                SideBarViewModel = ManageNavPages.ExternalLogins
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LinkLogin(string provider)
        {
            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            // Request a redirect to the external login provider to link a login for the current user
            var redirectUrl = Url.Action(nameof(LinkLoginCallback));
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl, _userManager.GetUserId(User));
            return new ChallengeResult(provider, properties);
        }

        [HttpGet]
        public async Task<IActionResult> LinkLoginCallback(string remoteError = null)
        {
            if (!string.IsNullOrEmpty(remoteError))
            {
                StatusMessage = $"Error: {remoteError}";
                return RedirectToAction(nameof(ExternalLogins));
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var info = await _signInManager.GetExternalLoginInfoAsync(user.Id);
            if (info == null)
            {
                throw new ApplicationException($"Unexpected error occurred loading external login info for user with ID '{user.Id}'.");
            }

            var result = await _userManager.AddLoginAsync(user, info);
            if (!result.Succeeded)
            {
                //todo: 
                //login already associated LoginAlreadyAssociated
                var err = result.Errors.First();
                if (err.Code == "LoginAlreadyAssociated")
                {
                    StatusMessage = $"Error: {err.Description}";
                    return RedirectToAction(nameof(ExternalLogins));
                }
                throw new ApplicationException($"Unexpected error occurred adding external login for user with ID '{user.Id}'.");
            }

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            StatusMessage = _localizer["The external login was added."];
            return RedirectToAction(nameof(ExternalLogins));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveLogin(RemoveLoginViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var result = await _userManager.RemoveLoginAsync(user, model.LoginProvider, model.ProviderKey);
            if (!result.Succeeded)
            {
                throw new ApplicationException($"Unexpected error occurred removing external login for user with ID '{user.Id}'.");
            }

            await _signInManager.SignInAsync(user, isPersistent: false);
            StatusMessage = _localizer["The external login was removed."];
            return RedirectToAction(nameof(ExternalLogins));
        }

        [HttpGet]
        public async Task<IActionResult> TwoFactorAuthentication()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var model = new TwoFactorAuthenticationViewModel
            {
                HasAuthenticator = await _userManager.GetAuthenticatorKeyAsync(user) != null,
                Is2faEnabled = user.TwoFactorEnabled,
                RecoveryCodesLeft = await _userManager.CountRecoveryCodesAsync(user),
            };

            return View("Index", new SidebarPageViewModel()
            {
                Title = SidebarTitle,
                PageTitle = _localizer["Two-Factor Authentication"],
                PageId = "TwoFactorAuthentication",
                MainPartialView = "TwoFactorAuthentication",
                MainViewModel = model,
                SideBarPartialView = SidebarPartialView,
                SideBarViewModel = ManageNavPages.TwoFactorAuthentication
            });
        }

        [HttpGet]
        public async Task<IActionResult> Disable2faWarning()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!user.TwoFactorEnabled)
            {
                throw new ApplicationException($"Unexpected error occured disabling 2FA for user with ID '{user.Id}'.");
            }

            return View("Index", new SidebarPageViewModel()
            {
                Title = SidebarTitle,
                PageTitle = _localizer["Disable Two-Factor Authentication"],
                PageId = "Disable2fa",
                MainPartialView = "Disable2fa",
                //MainViewModel = model,
                SideBarPartialView = SidebarPartialView,
                SideBarViewModel = ManageNavPages.TwoFactorAuthentication
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Disable2fa()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var disable2faResult = await _userManager.SetTwoFactorEnabledAsync(user, false);
            if (!disable2faResult.Succeeded)
            {
                throw new ApplicationException($"Unexpected error occured disabling 2FA for user with ID '{user.Id}'.");
            }

            _logger.LogInformation("User with ID {UserId} has disabled 2fa.", user.Id);
            return RedirectToAction(nameof(TwoFactorAuthentication));
        }

        [HttpGet]
        public async Task<IActionResult> EnableAuthenticator()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
            if (string.IsNullOrEmpty(unformattedKey))
            {
                await _userManager.ResetAuthenticatorKeyAsync(user);
                unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
            }

            var model = new EnableAuthenticatorViewModel
            {
                SharedKey = FormatKey(unformattedKey),
                AuthenticatorUri = GenerateQrCodeUri(user.Email, unformattedKey)
            };


            return View("Index", new SidebarPageViewModel()
            {
                Title = SidebarTitle,
                PageTitle = _localizer["Enable Authenticator"],
                PageId = "EnableAuthenticator",
                MainPartialView = "EnableAuthenticator",
                MainViewModel = model,
                SideBarPartialView = SidebarPartialView,
                SideBarViewModel = ManageNavPages.TwoFactorAuthentication
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnableAuthenticator(EnableAuthenticatorViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", new SidebarPageViewModel()
                {
                    Title = SidebarTitle,
                    PageTitle = _localizer["Enable Authenticator"],
                    PageId = "EnableAuthenticator",
                    MainPartialView = "EnableAuthenticator",
                    MainViewModel = model,
                    SideBarPartialView = SidebarPartialView,
                    SideBarViewModel = ManageNavPages.TwoFactorAuthentication
                });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            // Strip spaces and hypens
            var verificationCode = model.Code.Replace(" ", string.Empty).Replace("-", string.Empty);

            var is2faTokenValid = await _userManager.VerifyTwoFactorTokenAsync(
                user, _userManager.Options.Tokens.AuthenticatorTokenProvider, verificationCode);

            if (!is2faTokenValid)
            {
                ModelState.AddModelError("model.Code", "Verification code is invalid.");
                return View("Index", new SidebarPageViewModel()
                {
                    Title = SidebarTitle,
                    PageTitle = _localizer["Enable Authenticator"],
                    PageId = "EnableAuthenticator",
                    MainPartialView = "EnableAuthenticator",
                    MainViewModel = model,
                    SideBarPartialView = SidebarPartialView,
                    SideBarViewModel = ManageNavPages.TwoFactorAuthentication
                });
            }

            await _userManager.SetTwoFactorEnabledAsync(user, true);
            _logger.LogInformation("User with ID {UserId} has enabled 2FA with an authenticator app.", user.Id);
            return RedirectToAction(nameof(GenerateRecoveryCodes));
        }

        [HttpGet]
        public IActionResult ResetAuthenticatorWarning()
        {
            return View("Index", new SidebarPageViewModel()
            {
                Title = SidebarTitle,
                PageTitle = _localizer["Reset Authenticator Key"],
                PageId = "ResetAuthenticator",
                MainPartialView = "ResetAuthenticator",
                //MainViewModel = model,
                SideBarPartialView = SidebarPartialView,
                SideBarViewModel = ManageNavPages.TwoFactorAuthentication
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetAuthenticator()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await _userManager.SetTwoFactorEnabledAsync(user, false);
            await _userManager.ResetAuthenticatorKeyAsync(user);
            _logger.LogInformation("User with id '{UserId}' has reset their authentication app key.", user.Id);

            return RedirectToAction(nameof(EnableAuthenticator));
        }

        [HttpGet]
        public async Task<IActionResult> GenerateRecoveryCodes()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!user.TwoFactorEnabled)
            {
                throw new ApplicationException($"Cannot generate recovery codes for user with ID '{user.Id}' as they do not have 2FA enabled.");
            }

            var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
            var model = new GenerateRecoveryCodesViewModel { RecoveryCodes = recoveryCodes.ToArray() };

            _logger.LogInformation("User with ID {UserId} has generated new 2FA recovery codes.", user.Id);

            return View("Index", new SidebarPageViewModel()
            {
                Title = SidebarTitle,
                PageTitle = _localizer["Recovery Codes"],
                PageId = "GenerateRecoveryCodes",
                MainPartialView = "GenerateRecoveryCodes",
                MainViewModel = model,
                SideBarPartialView = SidebarPartialView,
                SideBarViewModel = ManageNavPages.TwoFactorAuthentication
            });
        }

        [HttpGet]
        public async Task<IActionResult> Settings()
        {
            //forbid if 2FA is required but disabled by user
            //or if user is required to setup sso
            if (User.TwoFactorRequired() || User.SSORequired())
                return Forbid();

            var user = await _userManager.GetUserAsync(User);
            var model = new SettingsViewModel();
            CPiSetting setting;
            
            setting = await _settingManager.GetCPiSetting(CPiSettings.DefaultPage);
            if (setting != null && await Authorize(setting.Policy))
            {
                //var defaultPage = await _settingManager.GetUserSetting(user.Id, CPiSettings.DefaultPage);
                //model.DefaultPage = defaultPage == null ? new DefaultPage() : defaultPage.GetSettings<DefaultPage>();
                model.DefaultPage = await _settingManager.GetUserSetting<DefaultPage>(user.Id);
            }

            setting = await _settingManager.GetCPiSetting(CPiSettings.UserPreferences);
            if (setting != null && await Authorize(setting.Policy))
            {
                //var userPreferences = await _settingManager.GetUserSetting(user.Id, CPiSettings.UserPreferences);
                //model.UserPreferences = userPreferences == null ? new UserPreferences() : userPreferences.GetSettings<UserPreferences>();
                model.UserPreferences = await _settingManager.GetUserSetting<UserPreferences>(user.Id);
            }

            //setting = await _settingManager.GetCPiSetting(CPiSettings.UserNotificationSettings);
            //if (setting != null && await Authorize(setting.Policy))
            //    model.UserNotificationSettings = await _settingManager.GetUserSetting<UserNotificationSettings>(user.Id);

            model.Theme = await _systemSettingManager.GetSystemSetting<Theme>();

            return View("Index", new SidebarPageViewModel()
            {
                Title = SidebarTitle,
                PageTitle = _localizer["Settings"],
                PageId = "Settings",
                MainPartialView = "Settings",
                MainViewModel = model,
                SideBarPartialView = SidebarPartialView,
                SideBarViewModel = ManageNavPages.Settings
            });
        }


        [HttpPost]
        public async Task<IActionResult> SaveSetting(string settingName, string setting, bool refresh)
        {
            var errorMessage = _localizer["Unable to update setting."].ToString();

            try
            {
                JObject settings = JObject.Parse(setting);

                await _settingManager.SaveUserSetting(User.GetUserIdentifier(), settingName, settings);

                //refresh claims
                if (refresh)
                {
                    var user = await _userManager.GetUserAsync(User);
                    if (user == null)
                        return BadRequest(errorMessage);
                    else
                        await _signInManager.RefreshSignInAsync(user);
                }

                return Json(new { success = true, message = _localizer["Setting successfully updated."] });
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest(errorMessage);
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCulture(string locale)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                if (user.Locale !=locale)
                {
                    user.Locale = locale;
                    var result = await _userManager.UpdateAsync(user);
                    if (!result.Succeeded)
                    {
                        throw new ApplicationException($"Unexpected error occurred updating locale for user with ID '{user.Id}'.");
                    }

                    //update claims
                    await _signInManager.RefreshSignInAsync(user);
                }
            }
            return Ok();
        }
        
        public async Task<IActionResult> GetDefaultPageList()
        {
            var defaultPages = await _settingManager.GetDefaultPages();
            var authorizedPages = new List<CPiDefaultPage>();
            foreach (var page in defaultPages)
            {
                try
                {
                    if (await Authorize(page.Policy))
                    {
                        authorizedPages.Add(page);
                    }
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e.Message);
                }
            }
            return Json(authorizedPages.OrderBy(p => p.Name));
        }


        #region Helpers

        private async Task<bool> Authorize(string policy)
        {
            return (policy == "*" || (await _authorizationService.AuthorizeAsync(User, policy)).Succeeded);
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        private string FormatKey(string unformattedKey)
        {
            var result = new StringBuilder();
            int currentPosition = 0;
            while (currentPosition + 4 < unformattedKey.Length)
            {
                result.Append(unformattedKey.Substring(currentPosition, 4)).Append(" ");
                currentPosition += 4;
            }
            if (currentPosition < unformattedKey.Length)
            {
                result.Append(unformattedKey.Substring(currentPosition));
            }

            return result.ToString().ToLowerInvariant();
        }

        private string GenerateQrCodeUri(string email, string unformattedKey)
        {
            var name = Request.Host.Host;
            if (Request.Host.Port != null)
                name += $"-{Request.Host.Port}";

            return string.Format(
                AuthenicatorUriFormat,
                _urlEncoder.Encode(name),
                _urlEncoder.Encode(email),
                unformattedKey);
        }

        private async Task<Microsoft.AspNetCore.Mvc.ActionResult> UnloadUser()
        {
            //user not found
            //logout then redirect to login screen

            //log error
            _logger.LogError($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

            await _signInManager.SignOutAsync();

            return await Task.Run<Microsoft.AspNetCore.Mvc.ActionResult>(() =>
            {
                //if (true)
                //{
                //    return RedirectToAction(nameof(AccountController.Login), "Account");
                //}
                //else
                //{
                //    return View("Index");
                //}

                //todo: show message?
                //TempData["RedirectMessage"] = "Please log back in.";
                return RedirectToAction(nameof(AccountController.Login), "Account");
            });
        }

        #endregion
    }
}
