using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using R10.Core.Identity;
using R10.Core.Interfaces;
using R10.Infrastructure.Identity;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using R10.Web.Helpers;
using R10.Core;
using R10.Core.Helpers;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using R10.Web.Extensions;
using R10.Core.Entities.Trademark;
using R10.Core.Entities.Patent;
using R10.Core.Entities;
using R10.Core.Entities.Shared;

namespace R10.Web.Services
{
    public class CPiUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<CPiUser, CPiRole>
    {
        private readonly CPiUserManager _userManager;
        private readonly ICPiUserDefaultPageManager _userSettingManager;
        private readonly ICPiSystemSettingManager _systemSettingManager;
        private readonly IHttpContextAccessor _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ISystemSettings<PatSetting> _patSettings;
        private readonly ISystemSettings<TmkSetting> _tmkSettings;
        private readonly ISystemSettings<DefaultSetting> _defaultSettings;
        private readonly CPiIdentitySettings _cpiSettings;

        public CPiUserClaimsPrincipalFactory(
            CPiUserManager userManager,
            RoleManager<CPiRole> roleManager,
            IOptions<IdentityOptions> options,
            ICPiUserDefaultPageManager userSettingManager,
            ICPiSystemSettingManager systemSettingManager,
            IHttpContextAccessor context,
            IWebHostEnvironment environment,
            ISystemSettings<PatSetting> patSettings,
            ISystemSettings<TmkSetting> tmkSettings,
            ISystemSettings<DefaultSetting> defaultSettings,
            IOptions<CPiIdentitySettings> cpiSettings
            ) : base(userManager, roleManager, options)
        {
            _userManager = userManager;
            _userSettingManager = userSettingManager;
            _systemSettingManager = systemSettingManager;
            _context = context;
            _environment = environment;
            _patSettings = patSettings;
            _tmkSettings = tmkSettings;
            _defaultSettings = defaultSettings;
            _cpiSettings = cpiSettings.Value;
        }
        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(CPiUser user)
        {
            var identity = await base.GenerateClaimsAsync(user);
            var encryptionKey = user.Id.Replace("-", "");
            var require2FA = false;
            var requireSSO = false;
            var referrerPath = _context?.HttpContext?.Request.Path.ToString().ToLower() ?? "";

            var current = _context?.HttpContext?.User.Identities.FirstOrDefault()?.Claims;
            if (current == null || current.Count() == 0)
            {
                //only check 2FA and SSO requirements during interactive login
                //pwd
                if (referrerPath.EndsWith("/login"))
                {
                    require2FA = _cpiSettings.SignIn.RequireTwoFactorAuthentication;
                    requireSSO = _userManager.IsSSORequired(user);
                }
                //sso
                else if (referrerPath.EndsWith("/externallogincallback"))
                    require2FA = _cpiSettings.SignIn.RequireExternalLoginTwoFactor;
            }
            else
            {
                require2FA = current.Any(c => c.Type == CPiClaimTypes.TwoFactorRequired && bool.Parse(c.Value));
                requireSSO = current.Any(c => c.Type == CPiClaimTypes.SSORequired && bool.Parse(c.Value));
            }

            if ((require2FA && !user.TwoFactorEnabled) || requireSSO)
            {
                if (require2FA && !user.TwoFactorEnabled)
                    identity.AddClaim(new Claim(CPiClaimTypes.TwoFactorRequired, "true"));

                if (requireSSO)
                    identity.AddClaim(new Claim(CPiClaimTypes.SSORequired, "true"));

                foreach (var claim in identity.Claims.Where(c => c.Type == ClaimTypes.Role || c.Type == ClaimTypes.System).ToList())
                {
                    identity.RemoveClaim(claim);
                }
            }

            //enabled systems
            var systems = await _userManager.GetSystems();
            foreach (var system in systems)
            {
                identity.AddClaim(new Claim(CPiClaimTypes.System, system.Id));

                //enabled systems with respoffice
                if (system.IsRespOfficeOn)
                    identity.AddClaim(new Claim(CPiClaimTypes.RespOffice, system.Id));
            }

            identity.AddClaim(new Claim(CPiClaimTypes.FirstName, user.FirstName));
            identity.AddClaim(new Claim(CPiClaimTypes.LastName, user.LastName));
            //identity.AddClaim(new Claim(CPiClaimTypes.UserType, user.UserType.ToString()));

            //do not add admin user type if 2FA is required but disabled by user
            if (!require2FA || user.TwoFactorEnabled || (user.UserType != CPiUserType.Administrator && user.UserType != CPiUserType.SuperAdministrator))
                identity.AddClaim(new Claim(CPiClaimTypes.UserType, JsonConvert.SerializeObject(user.UserType).Encrypt(encryptionKey)));

            identity.AddClaim(new Claim(CPiClaimTypes.EntityFilterType, ((int)user.EntityFilterType).ToString()));

            var defaultPage = await _userSettingManager.GetDefaultPage(user.Id);
            if (defaultPage != null)
            {
                identity.AddClaim(new Claim(CPiClaimTypes.DefaultPage, JsonConvert.SerializeObject(defaultPage).Encrypt(encryptionKey)));
            }

            var userPreferences = await _userSettingManager.GetUserSetting<UserPreferences>(user.Id);
            if (userPreferences == null)
                userPreferences = new UserPreferences();

            var userAccountSettings = await _userSettingManager.GetUserSetting<UserAccountSettings>(user.Id);

            identity.AddClaim(new Claim(CPiClaimTypes.Theme, userPreferences.Theme));
            identity.AddClaim(new Claim(CPiClaimTypes.SearchResultsPageSize, userPreferences.SearchResultsPageSize.ToString()));

            var locale = user.Locale;
            //Language source setting was removed from user account settings page
            //if (userPreferences.LanguageSource.ToLower() == "browser")
            //    locale = _context.HttpContext.Request.GetBrowserLocale();

            //check if locale is supported
            var localePattern = "lib\\cldr-data\\main\\{0}";
            var currentCulture = System.Globalization.CultureInfo.GetCultureInfo(locale);
            var cultureToUse = "en";

            if (System.IO.Directory.Exists(System.IO.Path.Combine(_environment.WebRootPath, string.Format(localePattern, currentCulture.Name))))
                cultureToUse = currentCulture.Name;
            else if (System.IO.Directory.Exists(System.IO.Path.Combine(_environment.WebRootPath, string.Format(localePattern, currentCulture.TwoLetterISOLanguageName))))
                cultureToUse = currentCulture.TwoLetterISOLanguageName;

            identity.AddClaim(new Claim(CPiClaimTypes.Locale, cultureToUse));

            //system status
            var systemStatus = await _systemSettingManager.GetSystemSetting<SystemStatus>("");
            identity.AddClaim(new Claim(CPiClaimTypes.SystemStatus, ((int)(systemStatus?.StatusType ?? SystemStatusType.Active)).ToString()));

            var defaultSettings = await _defaultSettings.GetSetting();

            //enabled modules
            var modules = new List<CPiModule>();

            if (systems.Any(s => s.Id == SystemType.Patent))
            {
                var patSettings = await _patSettings.GetSetting();

                if (patSettings.IsAuditOn)
                    modules.Add(CPiModule.PatAudit);
                if (patSettings.IsDeDocketOn)
                    modules.Add(CPiModule.PatDeDocket);
                if (patSettings.IsPortfolioOnboardingOn)
                    modules.Add(CPiModule.PatPortfolioOnboarding);
                if (patSettings.IsInventorAwardOn)
                    modules.Add(CPiModule.InventorAward);
                if (patSettings.IsRTSOn)
                    modules.Add(CPiModule.RTS);
                if (patSettings.IsCustomReportON)
                    modules.Add(CPiModule.PatCustomReport);
                if (patSettings.IsProductsOn)
                    modules.Add(CPiModule.PatProducts);
                if (patSettings.IsShowCustomFieldOn)
                    modules.Add(CPiModule.CustomField);
                if (patSettings.IsCostEstimatorOn)
                    modules.Add(CPiModule.PatCostEstimator);
                if (patSettings.IsDocumentVerificationOn)
                    modules.Add(CPiModule.PatDocumentVerification);
                if (patSettings.IsInventorRemunerationOn)
                    modules.Add(CPiModule.GermanRemuneration);
                if (patSettings.IsInventorFRRemunerationOn)
                    modules.Add(CPiModule.FrenchRemuneration);

                if (patSettings.IsExportControlOn)
                    identity.AddClaim(new Claim(CPiClaimTypes.RestrictExportControl, userAccountSettings.RestrictExportControl.ToString()));

                if (patSettings.IsTradeSecretOn)
                {
                    if (CPiPermissions.CanHaveTSClearance.Contains(user.UserType) && !userAccountSettings.RestrictPatTradeSecret)
                        identity.AddClaim(new Claim(CPiClaimTypes.TradeSecret, SystemType.Patent.Encrypt(encryptionKey)));

                    if (CPiPermissions.CanAccessTSReports.Contains(user.UserType) && !userAccountSettings.RestrictPatTradeSecretReports)
                        identity.AddClaim(new Claim(CPiClaimTypes.TradeSecretReports, SystemType.Patent.Encrypt(encryptionKey)));
                }
            }

            if (systems.Any(s => s.Id == SystemType.Trademark))
            {
                var tmkSettings = await _tmkSettings.GetSetting();

                if (tmkSettings.IsAuditOn)
                    modules.Add(CPiModule.TmkAudit);
                if (tmkSettings.IsDeDocketOn)
                    modules.Add(CPiModule.TmkDeDocket);
                if (tmkSettings.IsPortfolioOnboardingOn)
                    modules.Add(CPiModule.TmkPortfolioOnboarding);
                if (tmkSettings.IsTLOn)
                    modules.Add(CPiModule.TrademarkLinks);
                if (tmkSettings.IsCustomReportON)
                    modules.Add(CPiModule.TmkCustomReport);
                if (tmkSettings.IsProductsOn)
                    modules.Add(CPiModule.TmkProducts);
                if (tmkSettings.IsShowCustomFieldOn && !modules.Any(m => m == CPiModule.CustomField))
                    modules.Add(CPiModule.CustomField);
                if (tmkSettings.IsDocumentVerificationOn)
                    modules.Add(CPiModule.TmkDocumentVerification);
                if (tmkSettings.IsCostEstimatorOn)
                    modules.Add(CPiModule.TmkCostEstimator);
            }

            if (systems.Any(s => s.Id == SystemType.IDS))
            {
                var idsSettings = await _patSettings.GetSetting();

                if (idsSettings.IsIDSImportOn)
                    modules.Add(CPiModule.IDSImport);
            }

            if (defaultSettings.GetType().GetProperty("IsPowerBIConnectorOn")?.GetValue(defaultSettings) is bool powerBIConnectorOn && powerBIConnectorOn)
                modules.Add(CPiModule.PowerBIConnector);

            if (modules.Count > 0)
                identity.AddClaim(new Claim(CPiClaimTypes.Modules, JsonConvert.SerializeObject(modules).Encrypt(encryptionKey)));

            //remove duplicate ClaimTypes.System claims
            //remove disabled systems
            var userSystemRoles = new List<Claim>();
            foreach (var claim in identity.Claims.Where(c => c.Type == ClaimTypes.System).ToList())
            {
                if (!userSystemRoles.Any(c => c.Value == claim.Value) && systems.Any(c => c.Id == claim.Value))
                    userSystemRoles.Add(claim);

                identity.RemoveClaim(claim);
            }
            foreach (var claim in identity.Claims.Where(c => c.Type == ClaimTypes.Role).ToList())
            {
                if (!userSystemRoles.Any(c => c.Value == claim.Value) && systems.Any(c => claim.Value.StartsWith(c.Id)))
                {
                    userSystemRoles.Add(claim);

                    //remove disabled modules
                    var role = claim.Value.ToLower();
                    if ((role.StartsWith("patentcostestimator") && !modules.Contains(CPiModule.PatCostEstimator)) ||
                        (role.StartsWith("trademarkcostestimator") && !modules.Contains(CPiModule.TmkCostEstimator)) ||
                        (role.StartsWith("patentdedocketer") && !modules.Contains(CPiModule.PatDeDocket)) ||
                        (role.StartsWith("trademarkdedocketer") && !modules.Contains(CPiModule.TmkDeDocket)))
                        userSystemRoles.Remove(claim);
                }

                identity.RemoveClaim(claim);
            }
            identity.AddClaims(userSystemRoles);

            if ((await _userManager.HasDashboardAccessOptions(user)) && userAccountSettings.DashboardAccess != null && userAccountSettings.DashboardAccess.Any())
                identity.AddClaim(new Claim(CPiClaimTypes.DashboardAccess, JsonConvert.SerializeObject(userAccountSettings.DashboardAccess).Encrypt(encryptionKey)));

            identity.AddClaim(new Claim(CPiClaimTypes.ClientType, defaultSettings.IsCorporation ? "Corporation" : "LawFirm"));
            identity.AddClaim(new Claim(CPiClaimTypes.UseOutlookAddIn, user.UseOutlookAddIn.ToString()));

            if (userAccountSettings.Mailboxes != null)
                foreach (var mailbox in userAccountSettings.Mailboxes)
                {
                    identity.AddClaim(new Claim(CPiClaimTypes.Mailbox, mailbox));
                }

            identity.AddClaim(new Claim(CPiClaimTypes.DocumentStorage, ((int)(defaultSettings.DocumentStorage)).ToString()));
            if (defaultSettings.DocumentStorage != DocumentStorageOptions.BlobOrFileSystem)
                identity.AddClaim(new Claim(CPiClaimTypes.DocumentStorageAccountType, ((int)(userAccountSettings.DocumentStorageAccountType)).ToString()));

            if (user.UserType == CPiUserType.Attorney || user.UserType == CPiUserType.Inventor || user.UserType == CPiUserType.ContactPerson)
            {
                var entityFilters = await _userManager.GetEntityFilters(user.Id);
                var entityId = 0;

                if (entityFilters != null)
                    entityId = entityFilters.First().EntityId;

                identity.AddClaim(new Claim(CPiClaimTypes.EntityId, entityId.ToString()));
            }

            return identity;
        }
    }
}
