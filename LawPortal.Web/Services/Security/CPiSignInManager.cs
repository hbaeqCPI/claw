using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using LawPortal.Core.Identity;
using System;
using DocumentFormat.OpenXml.Drawing.Charts;

namespace LawPortal.Web.Services
{
    public class CPiSignInManager : SignInManager<CPiUser>
    {
        private readonly CPiUserManager _userManager;
        private readonly CPiIdentitySettings _cpiSettings;

        public CPiSignInManager(CPiUserManager userManager,
            IHttpContextAccessor contextAccessor,
            IUserClaimsPrincipalFactory<CPiUser> claimsFactory,
            IOptions<IdentityOptions> optionsAccessor,
            ILogger<CPiSignInManager> logger,
            IAuthenticationSchemeProvider schemes,
            IOptions<CPiIdentitySettings> cpiSettings
            ) : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, null)
        {
            _userManager = userManager;
            _cpiSettings = cpiSettings.Value;
        }

        private async Task<CPiSignInResult> CheckCPiUserRequirements(string loginProvider, string providerKey)
        {
            var user = await _userManager.FindByLoginAsync(loginProvider, providerKey);
            if (user != null)
                return await CheckCPiUserRequirements(user);

            return CPiSignInResult.Success;
        }

        private async Task<CPiSignInResult> CheckCPiUserRequirements(CPiUser user, string? password = null)
        {
            int passwordExpireDays = _cpiSettings.Password.ExpireDays;
            int loginInactiveDays = _cpiSettings.SignIn.InactiveDays;
            var today = DateTime.Now.Date;

            if (!(today >= (user.ValidDateFrom ?? DateTime.MinValue) && today <= (user.ValidDateTo ?? DateTime.MaxValue)))
                return CPiSignInResult.NotAllowed;
            if (user.IsPending)
                return CPiSignInResult.Pending;
            if (!user.IsEnabled)
                return CPiSignInResult.Disabled;
            if (user.IsEnabled && user.LastLoginDate != null && loginInactiveDays > 0 && (DateTime.Now - user.LastLoginDate).Value.Days >= loginInactiveDays)
                return CPiSignInResult.Inactive;

            // disable new account if not used within required activation period
            if (user.IsEnabled && user.LastLoginDate == null && _cpiSettings.SignIn.ActivationPeriodInDays > 0 && user.LastUpdate != null && ((DateTime.Now - user.LastUpdate).Value.Days >= _cpiSettings.SignIn.ActivationPeriodInDays))
            {
                await UpdateUserStatus(user, CPiUserStatus.Disabled);
                return CPiSignInResult.Disabled;
            }

            if (password != null && !(user.CannotChangePassword ?? false) && (user.LastPasswordChangeDate == null || (!(user.PasswordNeverExpires ?? false) && passwordExpireDays > 0 && (DateTime.Now - user.LastPasswordChangeDate).Value.Days >= passwordExpireDays)))
                return CPiSignInResult.PasswordChangeRequired;
            if (user.RequiresConfirmedEmail)
                return CPiSignInResult.ConfirmedEmailRequired;
            if (user.WebApiAccessOnly ?? false)
                return CPiSignInResult.NotAllowed;

            return CPiSignInResult.Success;
        }

        public async Task<CPiSignInResult> CheckWebApiUserRequirements(CPiUser user, string? password = null, bool lockoutOnFailure = false)
        {
            var result = password == null ? null : await CheckPasswordSignInAsync(user, password, lockoutOnFailure);
            if (password == null || (result?.Succeeded ?? false))
            {
                int passwordExpireDays = _cpiSettings.Password.ExpireDays;
                //int loginInactiveDays = _cpiSettings.SignIn.InactiveDays;
                var today = DateTime.Now.Date;

                if (!(today >= (user.ValidDateFrom ?? DateTime.MinValue) && today <= (user.ValidDateTo ?? DateTime.MaxValue)))
                    return CPiSignInResult.NotAllowed;
                if (user.IsPending)
                    return CPiSignInResult.Pending;
                if (!user.IsEnabled)
                    return CPiSignInResult.Disabled;
                //if (user.IsEnabled && user.LastLoginDate != null && loginInactiveDays > 0 && (DateTime.Now - user.LastLoginDate).Value.Days >= loginInactiveDays)
                //    return CPiSignInResult.Inactive;
                if (password != null && !(user.CannotChangePassword ?? false) && (user.LastPasswordChangeDate == null || (!(user.PasswordNeverExpires ?? false) && passwordExpireDays > 0 && (DateTime.Now - user.LastPasswordChangeDate).Value.Days >= passwordExpireDays)))
                    return CPiSignInResult.PasswordChangeRequired;
                //if (user.RequiresConfirmedEmail)
                //    return CPiSignInResult.ConfirmedEmailRequired;

                // Forbid ExternalLoginOnly account from using password grant type
                if (!string.IsNullOrEmpty(password) && _userManager.IsSSORequired(user))
                    return CPiSignInResult.NotAllowed;

                return CPiSignInResult.Success;
            }
            return CPiSignInResult.NotAllowed;
        }

        private async Task UpdateLastLoginDate(CPiUser user)
        {
            user.LastLoginDate = DateTime.Now;
            await _userManager.UpdateAsync(user);
        }

        private async Task UpdateUserStatus(CPiUser user, CPiUserStatus status)
        {
            user = await _userManager.FindByIdAsync(user.Id);
            if (user != null)
            {
                user.Status = status;
                await _userManager.UpdateAsync(user);
            }
        }

        public override async Task<SignInResult> PasswordSignInAsync(CPiUser user, string password, bool isPersistent, bool lockoutOnFailure)
        {
            // check password without signing in
            var result = await CheckPasswordSignInAsync(user, password, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                var cpiRequirement = await CheckCPiUserRequirements(user, password);

                if (!cpiRequirement.Succeeded)
                    return cpiRequirement;
                //else if ((await GetExternalAuthenticationSchemesAsync()).Count() > 0 && (user.ExternalLoginOnly ?? false))                
                //    return CPiSignInResult.ExternalLoginOnly;

                //.net 6.0 no longer calls SignInAsync
                //check result
                result = await base.PasswordSignInAsync(user, password, isPersistent, lockoutOnFailure);
                //then update last login date if succeeded
                if (result.Succeeded)
                    await UpdateLastLoginDate(user);

            }

            return result;
        }

        public override async Task<SignInResult> ExternalLoginSignInAsync(string loginProvider, string providerKey, bool isPersistent, bool bypassTwoFactor)
        {
            var cpiRequirement = await CheckCPiUserRequirements(loginProvider, providerKey);
            if (!cpiRequirement.Succeeded)
                return cpiRequirement;

            return await base.ExternalLoginSignInAsync(loginProvider, providerKey, isPersistent, bypassTwoFactor);
        }

        public async override Task SignInAsync(CPiUser user, AuthenticationProperties authenticationProperties, string authenticationMethod = null)
        {
            await UpdateLastLoginDate(user);
            await base.SignInAsync(user, authenticationProperties, authenticationMethod);
        }

        protected async override Task<SignInResult> LockedOut(CPiUser user)
        {
            var result = await base.LockedOut(user);

            if (result.IsLockedOut && _cpiSettings.Lockout.DisableAccount && user.Status != CPiUserStatus.Disabled)
            {
                await UpdateUserStatus(user, CPiUserStatus.Disabled);
            }
            return result;
        }

        public async override Task<IEnumerable<AuthenticationScheme>> GetExternalAuthenticationSchemesAsync()
        {
            return await base.GetExternalAuthenticationSchemesAsync();
        }
    }
}
