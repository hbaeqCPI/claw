using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using R10.Web.Models;
using R10.Web.Models.AccountViewModels;
using R10.Web.Services;
using R10.Core.Identity;
using R10.Web.Extensions;
using R10.Infrastructure.Identity;
using Microsoft.AspNetCore.Http;
using R10.Web.Interfaces;
using R10.Core.Interfaces;
using R10.Core.Helpers;
using System.Globalization;
using R10.Core.Entities;
using R10.Web.Areas.Shared.ViewModels;
using Microsoft.Extensions.Configuration;
using System.Text.Encodings.Web;
using R10.Core.Entities.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using Sustainsys.Saml2.AspNetCore2;

namespace R10.Web.Controllers
{
    [Authorize]
    [Route("[action]")]
    public class AccountController : Microsoft.AspNetCore.Mvc.Controller
    {
        const string Cookie_SaveEmail = "CPI_User";
        const string Cookie_ConsentDate = "CPI_CookieConsent";

        private readonly CPiUserManager _userManager;
        private readonly SignInManager<CPiUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;
        private readonly CPiIdentitySettings _cpiSettings;
        private readonly ICPiSystemSettingManager _systemSettingManager;
        //private readonly IEmailTemplateService _emailTemplateService;
        private readonly IConfiguration _configuration;
        private readonly JavaScriptEncoder _jsEncoder;
        private readonly ICPiExternalLoginManager _ssoManager;
        private readonly ISystemSettings<DefaultSetting> _defaultSettings;
        private readonly IUserAccountService _userAccountService;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public AccountController(
            CPiUserManager userManager,
            SignInManager<CPiUser> signInManager,
            IEmailSender emailSender,
            ILogger<AccountController> logger,
            IOptions<CPiIdentitySettings> cpiSettings,
            ICPiSystemSettingManager systemSettingManager,
            //IEmailTemplateService emailTemplateService,
            IConfiguration configuration,
            JavaScriptEncoder jsEncoder,
            ICPiExternalLoginManager ssoManager,
            ISystemSettings<DefaultSetting> defaultSettings,
            IUserAccountService userAccountService,
            IStringLocalizer<SharedResource> localizer)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _logger = logger;
            _cpiSettings = cpiSettings.Value;
            _systemSettingManager = systemSettingManager;
            //_emailTemplateService = emailTemplateService;
            _configuration = configuration;
            _jsEncoder = jsEncoder;
            _ssoManager = ssoManager;
            _defaultSettings = defaultSettings;
            _userAccountService = userAccountService;
            _localizer = localizer;
        }

        [TempData]
        public string? ErrorMessage { get; set; }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string? returnUrl = null)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }

            var systemStatus = await GetSystemStatus();
            var cookieConsent = await _systemSettingManager.GetSystemSetting<SystemNotification>("", "CookieConsent");

            if (!string.IsNullOrEmpty(Request.Cookies[Cookie_ConsentDate]))
            {
                var consentDate = DateTime.Parse(Request.Cookies[Cookie_ConsentDate]);
                cookieConsent.Active = cookieConsent.ActiveFrom > consentDate;
            }

            if (!string.IsNullOrEmpty(TempData.Peek("ErrorMessage")?.ToString()))
                ModelState.AddModelError(string.Empty, TempData["ErrorMessage"].ToString());

            var model = new LoginViewModel()
            {
                SystemStatusType = systemStatus.StatusType,
                CookieConsent = cookieConsent
            };

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ViewData["ReturnUrl"] = returnUrl;

            if (_cpiSettings.SignIn.AllowSaveEmail)
            {
                var email = Request.Cookies[Cookie_SaveEmail];
                if (email != null)
                {
                    model.Email = email;
                    model.SaveEmail = true;
                }
            }

            ViewData["ShowSiteHeader"] = false;
            ViewData["PageSelector"] = "login-page";
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                if (model.SaveEmail)
                {
                    Response.Cookies.Append(Cookie_SaveEmail, model.Email, new CookieOptions()
                    {
                        Path = HttpContext.Request.PathBase,
                        Expires = DateTime.Now.AddDays(14),
                        HttpOnly=true,
                        Secure=true
                    });
                }
                else if (Request.Cookies.ContainsKey(Cookie_SaveEmail))
                {
                    Response.Cookies.Delete(Cookie_SaveEmail, new CookieOptions()
                    {
                        Path = HttpContext.Request.PathBase
                    });
                }

                var user = await _userManager.FindByEmailAsync(model.Email);
                var result = CPiSignInResult.Failed;

                //user is required to use their registered external login
                if (user != null && _userManager.IsSSORequired(user) && (await _userManager.GetLoginsAsync(user)).Count > 0)
                    result = CPiSignInResult.ExternalLoginOnly;
                else
                    result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.StaySignedIn, lockoutOnFailure: true);

                if (result.Succeeded)
                {
                    if (await IsMaintenanceMode())
                    {
                        if (user == null || user.UserType != CPiUserType.SuperAdministrator)
                        {
                            await _signInManager.SignOutAsync();
                            //todo: redirect to maintenance page
                            return RedirectToAction(nameof(Login));
                        }
                    }

                    _logger.LogInformation("User {user} logged in.", model.Email);

                    return RedirectToLocal(returnUrl);
                }
                if (result.RequiresTwoFactor)
                {
                    return RedirectToAction(nameof(LoginWith2fa), new { returnUrl, model.StaySignedIn });
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User {user} is locked out.", model.Email);
                    if (_cpiSettings.Lockout.DisableAccount)
                    {
                        ModelState.AddModelError(string.Empty, "Account has been locked out.");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Account has been locked out due to too many failed login attempts. Please try again later.");
                    }
                }
                else if (result.IsNotAllowed)
                {
                    //check CPiSignInResult
                    var cpiResult = result as CPiSignInResult;
                    if (cpiResult?.RequiresConfirmedEmail ?? false)
                    {
                        if (user != null)
                        {
                            return RedirectToAction(nameof(ConfirmEmail), new { user.Id });
                        }
                    }

                    //redirect to reset password link if user is required to change password
                    if ((cpiResult?.RequiresPasswordChange ?? false) && !string.IsNullOrEmpty(model.Email))
                    {
                        return Redirect(await ResetPasswordLink(model.Email));
                    }

                    var notAllowedError = "";

                    //user is required to use their registered external login
                    if ((cpiResult?.RequiresExternalLogin ?? false) && user != null)
                    {
                        var logins = await _userManager.GetLoginsAsync(user);
                        if (logins.Count > 0)
                            notAllowedError = $"Please use the &quot;Log in with {logins.FirstOrDefault()?.ProviderDisplayName}&quot; button to login.";
                        else
                            notAllowedError = "Please use Single Sign-On to login.";
                    }
                    else
                        notAllowedError = await GetCPiSignResultError(cpiResult, model.Email);

                    _logger.LogWarning("User {user} is not allowed to login. Error: {error}", model.Email, notAllowedError);
                    ModelState.AddModelError(string.Empty, notAllowedError);
                }
                else
                {
                    _logger.LogWarning("User {user} failed login attempt.", model.Email);
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                }
            }

            ViewData["ShowSiteHeader"] = false;
            ViewData["PageSelector"] = "login-page";
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> LoginWith2fa(bool rememberMe, string? returnUrl = null)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }

            // Ensure the user has gone through the username & password screen first
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();

            if (user == null)
            {
                _logger.LogWarning("Unable to load two-factor authentication user.");
                return RedirectToAction(nameof(Login));
            }


            var model = new LoginWith2faViewModel { RememberMe = rememberMe };
            ViewData["ReturnUrl"] = returnUrl;

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginWith2fa(LoginWith2faViewModel model, bool rememberMe, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                _logger.LogWarning("Unable to load user with ID '{userId}'.", _userManager.GetUserId(User));
                return RedirectToAction(nameof(Login));
            }

            //add claims needed by sustainsys to invoke saml2 slo
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info?.LoginProvider == Saml2Defaults.Scheme)
                _signInManager.ClaimsFactory = new Saml2ClaimsFactory(_signInManager.ClaimsFactory, info);

            var authenticatorCode = model.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);

            var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(authenticatorCode, rememberMe, model.RememberMachine);

            if (result.Succeeded)
            {
                _logger.LogInformation("User {user} logged in with 2fa.", user.UserName);
                return RedirectToLocal(returnUrl);
            }
            else if (result.IsLockedOut)
            {
                _logger.LogWarning("User {user} is locked out.", user.UserName);

                if (_cpiSettings.Lockout.DisableAccount)
                {
                    ModelState.AddModelError(string.Empty, "Account has been locked out.");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Account has been locked out due to too many failed login attempts. Please try again later.");
                }
            }
            else
            {
                _logger.LogWarning("Invalid authenticator code entered for user {user}.", user.UserName);
                ModelState.AddModelError(string.Empty, "Invalid authenticator code.");
            }
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> LoginWithRecoveryCode(string? returnUrl = null)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }

            // Ensure the user has gone through the username & password screen first
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                _logger.LogWarning("Unable to load two-factor authentication user.");
                return RedirectToAction(nameof(Login));
            }

            ViewData["ReturnUrl"] = returnUrl;

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginWithRecoveryCode(LoginWithRecoveryCodeViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                _logger.LogWarning("Unable to load two-factor authentication user.");
                return RedirectToAction(nameof(Login));
            }

            var recoveryCode = model.RecoveryCode.Replace(" ", string.Empty);

            var result = await _signInManager.TwoFactorRecoveryCodeSignInAsync(recoveryCode);

            if (result.Succeeded)
            {
                _logger.LogInformation("User {user} logged in with a recovery code.", user.UserName);
                return RedirectToLocal(returnUrl);
            }
            if (result.IsLockedOut)
            {
                _logger.LogWarning("User {user} is locked out.", user.UserName);
                ModelState.AddModelError(string.Empty, "This account has been locked out. Please try again later.");
            }
            else
            {
                _logger.LogWarning("Invalid recovery code entered for user {user}", user.UserName);
                ModelState.AddModelError(string.Empty, "Invalid recovery code entered.");
            }
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Register(string? returnUrl = null)
        {
            if (!await IsSystemActive())
                return RedirectToAction(nameof(Login));

            if (User.Identity.IsAuthenticated)
                return RedirectToAction(nameof(ManageController.Index), "Manage");

            if (!_cpiSettings.Registration.Allowed)
                return RedirectToAction(nameof(Login));

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
        {
            if (!await IsSystemActive())
                return RedirectToAction(nameof(Login));

            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                //email domain validation
                if (_cpiSettings.Registration.ValidEmailDomains != null && !_cpiSettings.Registration.ValidEmailDomains.Any(d => string.Equals(d, model.Email.Split("@")[1], StringComparison.OrdinalIgnoreCase)))
                {
                    ModelState.AddModelError("Email", "Please use your company email address.");
                    return View(model);
                }

                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user == null)
                {
                    user = CPiUser.NewRegistration;

                    user.UserName = model.Email;
                    user.Email = model.Email;
                    user.FirstName = model.FirstName;
                    user.LastName = model.LastName;

                    var result = await _userManager.CreateAsync(user, model.Password);
                    if (result.Succeeded)
                    {
                        _logger.LogInformation("User {user} created a new account with password.", model.Email);

                        //send new account confirmation to user
                        //set sendNewAccountConfirmation = user.IsEnabled to not send when user is still pending
                        var sendNewAccountConfirmation = false; // user.IsEnabled;
                        if (sendNewAccountConfirmation)
                        {
                            var emailType = await _userAccountService.GetDefaultNewPasswordNotification(false);
                            var sendResult = await _userAccountService.SendNewPassword(user.Locale, emailType, new UserAccountEmail()
                            {
                                FirstName = user.FirstName,
                                LastName = user.LastName,
                                Email = user.Email,
                                Password = model.Password,
                                CallToAction = _localizer.GetStringWithCulture("Login", user.Locale),
                                CallToActionUrl = Url.LoginLink(Request.Scheme),
                                LogoUrl = Url.CPiLogoLink(Request.Scheme)
                            });

                            if (sendResult.Success)
                                _logger.LogInformation("User new account notification sent to {userEmail}.", user.Email);
                            else
                                _logger.LogError(sendResult.ErrorMessage);
                        }

                        ViewData["SuccessMessage"] = _localizer["Thank you for signing up!"];

                        //needs moderator to activate the account
                        if (!user.IsEnabled)
                        {
                            //send pending account notification to admin
                            await SendUserRegistrationNotification(user);

                            ViewData["SuccessMessage"] = $"<p>{ViewData["SuccessMessage"]}</p><p>{_localizer["Registration has been submitted for approval. You will receive an email notification once approved."]}</p>";
                            _logger.LogInformation("Registration for user {user}' needs approval.", user.UserName);
                        }

                        //email confirmation is required
                        if (_signInManager.Options.SignIn.RequireConfirmedEmail || user.RequiresConfirmedEmail)
                        {
                            //todo: use email template
                            await SendEmailConfirmationAsync(user);

                            ViewData["SuccessMessage"] = $"<p>{ViewData["SuccessMessage"]}</p><p>{_localizer["We have sent an email to verify your email adress. Please check your email and follow the instructions to complete the sign up process."]}</p>";
                            _logger.LogInformation("Email confirmation link sent to {userEmail}.", user.Email);
                        }
                        //successful registration
                        else if (user.IsEnabled)
                        {
                            //link contact/atty/inventor
                            await LinkUserAccount(user);

                            //sign in user automatically
                            await _signInManager.SignInAsync(user, isPersistent: false);
                            _logger.LogInformation("User {user} logged in.", user.UserName);

                            return RedirectToLocal(returnUrl);
                        }
                    }
                    else
                    {
                        AddErrors(result);
                    }
                }
                else
                {
                    ModelState.AddModelError("Email", UserRegistrationEmailExistsError(user.Email));
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User {user} logged out.", User.GetEmail());

            //saml2 slo
            if (User.HasClaim(ClaimTypes.AuthenticationMethod, Saml2Defaults.Scheme))
                return SignOut(new AuthenticationProperties()
                {
                    RedirectUri = Url.Action(nameof(HomeController.Index), "Home")
                }, Saml2Defaults.Scheme);
            else
                return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        //Use IdpRedirect?provider=Okta
        //Okta Initiate login URI
        //[HttpGet]
        //[AllowAnonymous]
        //public async Task<IActionResult> Okta()
        //{
        //    var provider = _configuration["Authentication:Okta:Name"];
        //    var loginProviders = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        //    if (loginProviders.Any(l => l.Name == provider))
        //        return await ExternalLogin(provider);

        //    return RedirectToAction(nameof(Login));
        //}

        //Idp Initiate login URI
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> IdpRedirect(string provider)
        {
            var loginProviders = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (loginProviders.Any(l => l.Name == provider))
                return await ExternalLogin(provider);

            return RedirectToAction(nameof(Login));
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExternalLogin(string provider, string? returnUrl = null)
        {
            if (await IsMaintenanceMode())
                return RedirectToAction(nameof(Login));

            // Request a redirect to the external login provider.
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
        {
            if (await IsMaintenanceMode())
                return RedirectToAction(nameof(Login));

            if (!string.IsNullOrEmpty(remoteError))
            {
                ErrorMessage = remoteError;
                return RedirectToAction(nameof(Login));
            }
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToAction(nameof(Login));
            }

            //get claim types
            var emailAttributeName = string.IsNullOrEmpty(_cpiSettings.ExternalLogin.EmailAttributeName) ? ClaimTypes.Email : _cpiSettings.ExternalLogin.EmailAttributeName;
            var firstNameAttributeName = string.IsNullOrEmpty(_cpiSettings.ExternalLogin.FirstNameAttributeName) ? ClaimTypes.GivenName : _cpiSettings.ExternalLogin.FirstNameAttributeName;
            var lastNameAttributeName = string.IsNullOrEmpty(_cpiSettings.ExternalLogin.LastNameAttributeName) ? ClaimTypes.Surname : _cpiSettings.ExternalLogin.LastNameAttributeName;

            //add claims needed by sustainsys to invoke saml2 slo
            if (info.LoginProvider == Saml2Defaults.Scheme)
                _signInManager.ClaimsFactory = new Saml2ClaimsFactory(_signInManager.ClaimsFactory, info);

            // Sign in the user with this external login provider if the user already has a login.
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: false);

            if (result.Succeeded)
            {
                _logger.LogInformation("User {user} logged in with {loginProvider} provider.", info.Principal.FindFirstValue(emailAttributeName), info.LoginProvider);
                return RedirectToLocal(returnUrl);
            }
            if (result.IsLockedOut)
            {
                _logger.LogWarning("User {user} is locked out.", info.Principal.Identity.Name);
                ViewData["ErrorMessage"] = $"This account has been locked out. Please try again later.";
                return View(nameof(ExternalLogin));
            }
            if (result.IsNotAllowed)
            {
                //check CPiSignInResult
                var cpiResult = result as CPiSignInResult;
                string email = info.Principal.FindFirstValue(emailAttributeName);

                if (cpiResult.RequiresConfirmedEmail && email != null)
                {
                    var userConfirmEmail = await _userManager.FindByEmailAsync(email);
                    if (userConfirmEmail != null)
                    {
                        return RedirectToAction(nameof(ConfirmEmail), new { userConfirmEmail.Id });
                    }
                }

                var notAllowedError = await GetCPiSignResultError(cpiResult, email);
                _logger.LogWarning("User {user} is not allowed to login. Error: {error}", email, notAllowedError);

                ViewData["ErrorMessage"] = notAllowedError;
                return View(nameof(ExternalLogin));
            }
            if (result.RequiresTwoFactor)
            {
                return RedirectToAction(nameof(LoginWith2fa), new { returnUrl, StaySignedIn = false });
            }

            //user provisioning workflow
            ViewData["ReturnUrl"] = returnUrl;
            var model = new ExternalLoginViewModel
            {
                FirstName = info.Principal.FindFirstValue(firstNameAttributeName),
                LastName = info.Principal.FindFirstValue(lastNameAttributeName),
                Email = info.Principal.FindFirstValue(emailAttributeName),
                LoginProvider = info.LoginProvider
            };

            //validate required attributes
            if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.FirstName) || string.IsNullOrEmpty(model.LastName))
            {
                _logger.LogError("Invalid {loginProvider} login attempt: {providerKey}. Missing required attributes. Email: {email}. FirstName: {firstName}. LastName: {lastName}.", info.LoginProvider, info.ProviderKey, model.Email, model.FirstName, model.LastName);

                ViewData["ErrorMessage"] = "Invalid login attempt.";
                return View(nameof(ExternalLogin));
            }

            //validate role claim
            var cpiClaim = info.Principal.FindFirstValue(_cpiSettings.ExternalLogin.RoleAttributeName);
            if (_cpiSettings.ExternalLogin.RequireRoleAttribute && string.IsNullOrEmpty(cpiClaim))
            {
                _logger.LogError("Invalid {loginProvider} login attempt: {providerKey}. Attribute: {attributeName} is required from Idp.", info.LoginProvider, info.ProviderKey, _cpiSettings.ExternalLogin.RoleAttributeName);

                ViewData["ErrorMessage"] = "Invalid login attempt.";
                return View(nameof(ExternalLogin));
            }

            //validate if email exists
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                //auto-registration
                if (_cpiSettings.ExternalLogin.AutoRegister)
                {
                    //create user
                    user = await _ssoManager.GetSSOUserAsync(cpiClaim);

                    user.UserName = model.Email;
                    user.Email = model.Email;
                    user.FirstName = model.FirstName;
                    user.LastName = model.LastName;
                    user.CannotChangePassword = _cpiSettings.ExternalLogin.DisablePassword;
                    user.ExternalLoginOnly = _cpiSettings.ExternalLogin.DisablePassword;

                    var now = DateTime.Now;

                    user.UpdatedBy = info.LoginProvider;
                    user.LastUpdate = now;
                    user.CreatedBy = info.LoginProvider;
                    user.DateCreated = now;

                    if (_cpiSettings.ExternalLogin.NeedsApproval)
                        user.Status = CPiUserStatus.Pending;

                    var identityResult = await _userManager.CreateAsync(user);
                    if (identityResult.Succeeded)
                    {
                        //add roles using cpiClaim
                        identityResult = await _ssoManager.AddRolesAsync(user, cpiClaim);
                        if (identityResult.Succeeded)
                        {
                            identityResult = await _userManager.AddLoginAsync(user, info);
                            if (identityResult.Succeeded)
                            {
                                _logger.LogInformation("User {user} automatically created using {loginProvider} provider.", model.Email, info.LoginProvider);

                                if (user.IsEnabled)
                                {
                                    //link contact/atty/inventor
                                    await LinkUserAccount(user);

                                    //sign in user automatically
                                    await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
                                    return RedirectToLocal(returnUrl);
                                }
                            }
                        }
                    }

                    if (!identityResult.Succeeded)
                        //error creating user account
                        _logger.LogError("Error creating {loginProvider} account with {email}: {errors}", info.LoginProvider, model.Email, string.Join("\r\n", identityResult.Errors));

                    if (user.IsPending)
                    {
                        //send pending account notification to admin
                        await SendUserRegistrationNotification(user);

                        ViewData["SuccessMessage"] = "Registration has been submitted for approval. You will receive an email notification once approved.";
                        _logger.LogInformation("Registration for user {user} needs approval.", user.UserName);
                    }
                    else
                    {
                        _logger.LogWarning("User {user} failed login attempt.", model.Email);
                        ViewData["ErrorMessage"] = "Invalid login attempt.";
                    }

                    return View(nameof(ExternalLogin));
                }

                //self-registration
                if (_cpiSettings.Registration.AllowedForExernalLogin)
                {
                    return View(nameof(ExternalLogin), model);
                }

                //registration not allowed
                _logger.LogError("Invalid {loginProvider} login attempt: User registration is disabled.", info.LoginProvider);

                ViewData["ErrorMessage"] = "User registration is disabled.";
                return View(nameof(ExternalLogin));
            }
            else
            {
                if (_cpiSettings.ExternalLogin.AutoLink)
                {
                    //only link enabled accounts
                    if (user.IsEnabled)
                    {
                        var addLoginResult = await _userManager.AddLoginAsync(user, info);
                        if (addLoginResult.Succeeded)
                        {
                            _logger.LogInformation("User {user} automatically linked with {loginProvider} provider.", model.Email, info.LoginProvider);

                            result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: false);

                            if (result.RequiresTwoFactor)
                            {
                                return RedirectToAction(nameof(LoginWith2fa), new { returnUrl, StaySignedIn = false });
                            }
                            if (result.Succeeded)
                            {
                                _logger.LogInformation("User {user} logged in with {loginProvider} provider.", model.Email, info.LoginProvider);
                                return RedirectToLocal(returnUrl);
                            }
                            else
                            {
                                _logger.LogWarning("User {user} failed login attempt.", model.Email);
                                ViewData["ErrorMessage"] = "Invalid login attempt.";
                            }
                        }
                        else
                        {
                            //error linking user account
                            _logger.LogError("Error linking {loginProvider} account with {user}: {errors}", info.LoginProvider, model.Email, string.Join("\r\n", addLoginResult.Errors));

                            ViewData["ErrorMessage"] = "Invalid login attempt.";
                        }
                    }
                    else
                    {
                        _logger.LogError("Error linking {loginProvider} account with {user}: {errors}", info.LoginProvider, model.Email, "Account is disabled.");

                        ViewData["ErrorMessage"] = "Account is disabled.";
                    }

                    return View(nameof(ExternalLogin));
                }
                else
                {
                    _logger.LogError("Invalid {loginProvider} login attempt: {providerKey}. Unable to create user. Email {email} already exists.", info.LoginProvider, info.ProviderKey, model.Email);

                    ViewData["ErrorMessage"] = $"" +
                        $"<p>User account with email: '{model.Email}' already exists.</p>" +
                        $"<p>If this is your account, please log in using your CPI password then register your {info.LoginProvider} account from your My Account page.</p>" +
                        $"<p>If you forgot your password, please click <a href='{Url.Action(nameof(ForgotPassword), "Account")}' class='alert-link'>here</a> to reset it.</p>";
                    return View(nameof(ExternalLogin));
                }
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExternalLoginConfirmation(ExternalLoginViewModel model, string? returnUrl = null)
        {
            if (await IsMaintenanceMode())
                return RedirectToAction(nameof(Login));

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await _signInManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    _logger.LogWarning("Error loading external login information during confirmation.");
                    return RedirectToAction(nameof(Login));
                }

                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user == null)
                {
                    var cpiClaim = info.Principal.FindFirstValue(_cpiSettings.ExternalLogin.RoleAttributeName);

                    user = await _ssoManager.GetSSOUserAsync(cpiClaim);

                    user.UserName = model.Email;
                    user.Email = model.Email;
                    user.FirstName = model.FirstName;
                    user.LastName = model.LastName;
                    user.CannotChangePassword = _cpiSettings.ExternalLogin.DisablePassword;
                    user.ExternalLoginOnly = _cpiSettings.ExternalLogin.DisablePassword;

                    var now = DateTime.Now;

                    user.UpdatedBy = info.LoginProvider;
                    user.LastUpdate = now;
                    user.CreatedBy = info.LoginProvider;
                    user.DateCreated = now;

                    if (_cpiSettings.ExternalLogin.NeedsApproval)
                        user.Status = CPiUserStatus.Pending;

                    var result = await _userManager.CreateAsync(user);
                    if (result.Succeeded)
                    {
                        result = await _ssoManager.AddRolesAsync(user, cpiClaim);
                        if (result.Succeeded)
                        {
                            result = await _userManager.AddLoginAsync(user, info);
                            if (result.Succeeded)
                            {
                                _logger.LogInformation("User {user} created an account using {loginProvider} provider.", user.UserName, info.LoginProvider);

                                if (user.IsEnabled)
                                {
                                    //link contact/atty/inventor
                                    await LinkUserAccount(user);

                                    //sign in user automatically
                                    await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
                                    return RedirectToLocal(returnUrl);
                                }
                                else
                                {
                                    //send pending account notification to admin
                                    await SendUserRegistrationNotification(user);

                                    //needs moderator to activate the account
                                    ViewData["SuccessMessage"] = "Registration has been submitted for approval. You will receive an email notification once approved.";
                                    _logger.LogInformation("Registration for user with ID '{id}' needs approval.", user.Id);

                                    return View(nameof(ExternalLogin), model);
                                }
                            }
                        }
                    }
                    AddErrors(result);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, UserRegistrationEmailExistsError(user.Email));
                }
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(nameof(ExternalLogin), model);
        }

        private string UserRegistrationEmailExistsError(string email)
        {
            //TODO:
            //notify user of registration attempt?

            //return $"<p>Email '{email}' is already taken.</p><p>If this is your account and you forgot your password, please click <a href='{Url.Action(nameof(ForgotPassword), "Account")}' class='alert-link'>here</a> to reset it.";
            return $"Email '{email}' is already taken.";
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string id, string token)
        {
            if (await IsMaintenanceMode())
                return RedirectToAction(nameof(Login));

            if (id == null)
                return RedirectToAction(nameof(HomeController.Index), "Home");

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                _logger.LogWarning("Unable to load user with ID '{id}'.", id);
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }

            if (user.EmailConfirmed)
            {
                ViewData["SuccessMessage"] = "<p>Thank you for confirming your email.</p>";
            }
            else if (token == null)
            {
                ModelState.AddModelError(string.Empty, "Your email has not been confirmed");
            }
            else
            {
                var result = await _userManager.ConfirmEmailAsync(user, token);
                if (result.Succeeded)
                {
                    if (user.Status == CPiUserStatus.Approved)
                        ViewData["SuccessMessage"] = "<p>Thank you for confirming your email.</p>";
                    else
                        ViewData["SuccessMessage"] = "<p>Thank you for confirming your email.</p><p>Your registration is pending approval. You will receive an email notification once approved.</p>";
                }
                else
                {
                    //AddErrors(result);
                    ModelState.AddModelError(string.Empty, "Unable to verify email");
                }
            }

            return View("ConfirmEmail");
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmEmail(string id)
        {
            if (await IsMaintenanceMode())
                return RedirectToAction(nameof(Login));

            if (id == null)
                return RedirectToAction(nameof(HomeController.Index), "Home");

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                _logger.LogWarning("Unable to load user with ID '{id}'.", id);
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }

            var result = await SendEmailConfirmationAsync(user);

            if (result.Success)
            {
                ViewData["SuccessMessage"] = "Verification email sent. Please check your email.";
            }
            else
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage);
            }                       

            return View("ConfirmEmail");
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword()
        {
            if (await IsMaintenanceMode())
                return RedirectToAction(nameof(Login));

            if (User.Identity.IsAuthenticated)
                return RedirectToAction(nameof(ManageController.ChangePassword), "Manage");

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (await IsMaintenanceMode())
                return RedirectToAction(nameof(Login));

            model.Email = _jsEncoder.Encode(model.Email); //mitigate acunetix's xss report
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                //todo: allow unconfirmed email??
                if (user == null)
                {
                    // Don't reveal that the user does not exist
                    ViewData["SuccessMessage"] = $"A link to reset your password was sent to {model.Email}.";
                }
                else
                {
                    // For more information on how to enable account confirmation and password reset please
                    // visit https://go.microsoft.com/fwlink/?LinkID=532713

                    var result = new EmailSenderResult() { ErrorMessage = "Unable to send email." };

                    if (user.CannotChangePassword ?? false)
                    {
                        _logger.LogWarning("User {user} cannot change password.", user.UserName);
                        ModelState.AddModelError(string.Empty, "Unable to send email.");
                        return View(model);
                    }

                    if (user.RequiresConfirmedEmail)
                    {
                        // User needs to confirm email
                        // Send email confirmation link
                        result = await SendNeedsToConfirmEmailAsync(user);
                    }
                    else
                    {
                        // Send reset password link
                        //result = await SendResetPasswordLinkAsync(user);

                        // EmailTemplateService removed during debloat
                        result = new EmailSenderResult() { ErrorMessage = "Email template service not available." };

                    }

                    if (result.Success)
                        ViewData["SuccessMessage"] = $"A link to reset your password was sent to {model.Email}.";
                    else
                        ModelState.AddModelError(string.Empty, result.ErrorMessage);
                }
            }

            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(string id = null, string token = null)
        {
            if (await IsMaintenanceMode())
                return RedirectToAction(nameof(Login));

            if (User.Identity.IsAuthenticated)
                return RedirectToAction(nameof(ManageController.ChangePassword), "Manage");

            if (id == null && token == null)
            {
                _logger.LogWarning("A code must be supplied for password reset.");
                return RedirectToAction(nameof(ForgotPassword));
            }
            var model = new ResetPasswordViewModel { UserId = id, Code = token };
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (await IsMaintenanceMode())
                return RedirectToAction(nameof(Login));

            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByIdAsync(model.UserId);
            //remove email input?
            //var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                AddErrors(IdentityResult.Failed(new IdentityError() { Description = "Invalid token." }));
                //todo: log event
            }
            else if (user.CannotChangePassword ?? false)
            {
                // Don't reveal that the user cannot change password
                _logger.LogWarning("User {user} cannot change password.", user.UserName);
                AddErrors(IdentityResult.Failed(new IdentityError() { Description = "Invalid token." }));
            }
            else
            {
                var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
                if (result.Succeeded)
                {
                    //todo: send confirmation?
                    //todo: log event?
                    ViewData["SuccessMessage"] = "Your password has been reset.";
                }
                else
                {
                    //todo: log errors
                    AddErrors(result);
                }
            }

            return View();
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult CookieConsent()
        {
            Response.Cookies.Append(Cookie_ConsentDate, DateTime.Now.ToString("o", CultureInfo.InvariantCulture), new CookieOptions()
            {
                Path = HttpContext.Request.PathBase,
                Expires = DateTime.Now.AddDays(14),
                HttpOnly=true,
                Secure=true
            });
            return Ok();
        }

        #region Helpers
        private async Task<EmailSenderResult> SendEmailConfirmationAsync(string email)
        {
            CPiUser user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return new EmailSenderResult { Success = false };
            }
            return await SendEmailConfirmationAsync(user);
        }

        private async Task<EmailSenderResult> SendEmailConfirmationAsync(CPiUser user)
        {
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var callbackUrl = Url.EmailConfirmationLink(user.Id, code, Request.Scheme);
            return await _emailSender.SendEmailConfirmationAsync(user, callbackUrl);
        }

        private async Task<EmailSenderResult> SendNeedsToConfirmEmailAsync(CPiUser user)
        {
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var callbackUrl = Url.EmailConfirmationLink(user.Id, code, Request.Scheme);
            return await _emailSender.SendNeedsToConfirmEmailAsync(user, callbackUrl, Url.ForgotPasswordLink(Request.Scheme));
        }
        //private async Task<EmailSenderResult> SendResetPasswordLinkAsync(CPiUser user)
        //{
        //    return await _emailSender.SendResetPasswordLinkAsync(user, await ResetPasswordLink(user));
        //}
        private async Task<string> ResetPasswordLink(CPiUser user)
        {
            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            return  Url.ResetPasswordCallbackLink(user.Id, code, Request.Scheme);
        }
        private async Task<string> ResetPasswordLink(string email)
        {
            CPiUser user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return null;
            }
            return await ResetPasswordLink(user);
        }

        private async Task<string> GetCPiSignResultError(CPiSignInResult result, string email)
        {
            string errorMessage;

            if (result?.RequiresConfirmedEmail ?? false)
            {
                if (email != null && (await SendEmailConfirmationAsync(email)).Success)
                {
                    errorMessage = $"<p>Your email has not been confirmed.</p><p>We have sent an email to {email} to verify your email adress.</p><p>Please check you email and follow the instructions before signing in.</p>";
                }
                else
                {
                    errorMessage = $"<p>Your email has not been confirmed.</p>";
                }
            }
            else if (result?.IsInactive ?? false)
            {
                errorMessage = "Account has been disabled due to inactivity.";
            }
            else if (result?.RequiresPasswordChange ?? false)
            {
                if (email == null)
                {
                    errorMessage = $"<p>Your password has expired.</p>";
                }
                else
                {
                    errorMessage = $"<p>Your password has expired.</p><p>Please click <a href=\"{await ResetPasswordLink(email)}\" class=\"alert-link\">here</a> to reset your password.</p>";
                }                
            }
            else if (result?.IsPending ?? false)
            {
                errorMessage = "Account is pending approval.";
            }
            else if (result?.IsDisabled ?? false)
            {
                errorMessage = "Account has been disabled.";
            }
            else
            {
                errorMessage = "Invalid login attempt.";
            }

            return errorMessage;
        }

        private void AddErrors(IdentityResult result)
        {
            var errors = result.Errors.ToList();

            if (errors.Any(e => e.Code == "DuplicateUserName") && errors.Any(e => e.Code == "DuplicateEmail"))
            {
                var duplicateUserNameError = errors.FirstOrDefault(e => e.Code == "DuplicateUserName");
                errors.Remove(duplicateUserNameError);
            }

            string errInvalidLink = $"<p>Please click <a href='{Url.Action(nameof(ForgotPassword), "Account")}' class='alert-link'>here</a> to request a new password reset link.";

            foreach (var error in errors)
            {
                string errMessage = error.Description.ToLower().Contains("invalid token") ? $"<p>Link has expired.</p>{errInvalidLink}" : error.Description;

                ModelState.AddModelError(string.Empty, errMessage);
            }
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }

        private async Task<bool> IsMaintenanceMode()
        {
            var systemStatus = await GetSystemStatus();
            return systemStatus.StatusType == SystemStatusType.Maintenance;
        }

        private async Task<bool> IsSystemActive()
        {
            var systemStatus = await GetSystemStatus();
            return systemStatus.StatusType == SystemStatusType.Active;
        }

        private async Task<SystemStatus> GetSystemStatus()
        {
            return await _systemSettingManager.GetSystemSetting<SystemStatus>("");
        }

        private async Task SendUserRegistrationNotification(CPiUser user)
        {

            var sendResult = await _userAccountService.SendUserRegistrationNotification(new UserRegistrationNotification()
            {
                UserFirstName = user.FirstName,
                UserLastName = user.LastName,
                UserEmail = user.Email,
                UserStatus = user.Status,
                UserType = user.UserType,
                CallToAction = "View Pending Users", //localized in SendUserRegistrationNotification
                CallToActionUrl = $"{Url.UserSetupLink(Request.Scheme)}?status={CPiUserStatus.Pending}",
                LogoUrl = Url.CPiLogoLink(Request.Scheme)
            });

            if (sendResult.Success)
                _logger.LogInformation("{user} user registration approval notification sent.", user.UserName);
            else
                _logger.LogError(sendResult.ErrorMessage);
        }

        private async Task LinkUserAccount(CPiUser user)
        {
            try
            {
                await _userAccountService.LinkUserAccount(user);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
            }
        }

        #endregion
    }
}
