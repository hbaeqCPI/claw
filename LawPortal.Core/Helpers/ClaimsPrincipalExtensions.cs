using Newtonsoft.Json;
using LawPortal.Core.Entities;
using LawPortal.Core.Entities.Shared;
using LawPortal.Core.Identity;
using System.Security.Claims;

namespace LawPortal.Core.Helpers
{
    public static class ClaimsPrincipalExtensions
    {
        public static SystemStatusType? SystemStatus { get; private set; }

        public static string GetEncryptionKey(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.NameIdentifier)?.Value.Replace("-", "");
        }
        public static string GetUserName(this ClaimsPrincipal user)
        {
            return user.Identity.Name != null ? user.Identity.Name.Split("@")[0].Left(20) : user.FindFirst(ClaimTypes.Email)?.Value.Split("@")[0].Left(20);
        }

        public static string GetEmail(this ClaimsPrincipal user)
        {
            return (user.Identity?.Name ?? user.FindFirst(ClaimTypes.Email)?.Value) ?? "";
        }

        public static string GetFullName(this ClaimsPrincipal user)
        {
            return $"{user.FindFirst(CPiClaimTypes.FirstName).Value} {user.FindFirst(CPiClaimTypes.LastName).Value}";
        }

        public static bool IsCpiUser(this ClaimsPrincipal user)
        {
            var domain = user.Identity?.Name != null ? user.Identity.Name.Split("@")[1] : user.FindFirst(ClaimTypes.Email)?.Value.Split("@")[1];
            return CPiPermissions.CpiDomains.Contains(domain ?? "");
        }

        public static bool IsAdmin(this ClaimsPrincipal user)
        {
            try
            {
                return user.GetUserType() == CPiUserType.Administrator || user.IsSuper();
            }
            catch
            {
                return false;
            }
        }

        public static bool IsSuper(this ClaimsPrincipal user)
        {
            try
            {
                return user.GetUserType() == CPiUserType.SuperAdministrator;
            }
            catch
            {
                return false;
            }

        }
        public static string GetUserIdentifier(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        //Use to check if entity filter is needed
        public static bool HasEntityFilter(this ClaimsPrincipal user)
        {
            return user.IsAdmin() ? false : user.GetEntityFilterType() != CPiEntityType.None;
        }

        //Use to check if at least one System has IsRespOffice on
        public static bool HasRespOfficeFilter(this ClaimsPrincipal user)
        {
            //return user.IsAdmin() ? false : user.HasClaim(c => c.Type == CPiClaimTypes.RespOffice);
            //only regular users have resp office filter
            return user.GetUserType().IsRegularUser() ? user.IsRespOfficeOn() : false;
        }

        //Use to check if resp office filter or validation is needed
        public static bool HasRespOfficeFilter(this ClaimsPrincipal user, string systemId)
        {
            //return user.IsAdmin() ? false : user.HasClaim(c => c.Type == CPiClaimTypes.RespOffice && c.Value.ToLower() == systemId.ToLower());
            //only regular users have resp office filter
            return user.GetUserType().IsRegularUser() ? user.HasClaim(c => c.Type == CPiClaimTypes.RespOffice && c.Value.ToLower() == systemId.ToLower()) && user.IsInSystem(systemId) : false;
        }

        //Use to toggle resp office field display
        public static bool IsRespOfficeOn(this ClaimsPrincipal user, string systemId = "")
        {
            if (string.IsNullOrEmpty(systemId))
                return user.HasClaim(c => c.Type == CPiClaimTypes.RespOffice);
            else
                return user.HasClaim(c => c.Type == CPiClaimTypes.RespOffice && c.Value.ToLower() == systemId.ToLower());
        }

        public static CPiEntityType GetEntityFilterType(this ClaimsPrincipal user)
        {
            if (user.HasClaim(c => c.Type == CPiClaimTypes.EntityFilterType))
                return (CPiEntityType)int.Parse(user.FindFirst(CPiClaimTypes.EntityFilterType)?.Value);

            return CPiEntityType.None;
        }

        public static EntityFilterParam GetEntityFilterParam(this ClaimsPrincipal user)
        {
            return new EntityFilterParam(user.GetUserIdentifier(), user.HasRespOfficeFilter(), user.HasEntityFilter(), user.GetEntityFilterType());
        }

        public static IEnumerable<Claim> GetRoles(this ClaimsPrincipal user)
        {
            return user.Claims.Where(c => c.Type == ClaimTypes.Role).ToList();
        }

        public static bool IsSystemEnabled(this ClaimsPrincipal user, string systemId)
        {
            return (user.HasClaim(c => c.Type == CPiClaimTypes.System && c.Value.ToLower() == systemId.ToLower()));
        }

        public static bool IsSystemWithContactPersonEnabled(this ClaimsPrincipal user)
        {
            return (user.HasClaim(c => c.Type == CPiClaimTypes.System && SystemType.HasContactPersonUser.Any(s => s.ToLower() ==  c.Value.ToLower())));
        }

        public static bool IsSystemWithAttorneyEnabled(this ClaimsPrincipal user)
        {
            return (user.HasClaim(c => c.Type == CPiClaimTypes.System && SystemType.HasAttorneyUser.Any(s => s.ToLower() == c.Value.ToLower())));
        }

        public static bool IsInSystem(this ClaimsPrincipal user, string systemId)
        {
            return (user.IsSystemEnabled(systemId) && (user.IsAdmin() || user.HasClaim(c => c.Type == ClaimTypes.System && c.Value?.ToLower() == systemId?.ToLower())));
        }

        public static bool IsInSystems(this ClaimsPrincipal user, IEnumerable<string> systems)
        {
            return user.HasClaim(c => c.Type == CPiClaimTypes.System && systems.Any(s => s.ToLower() == c.Value?.ToLower())) &&
                (user.IsAdmin() || user.HasClaim(c => c.Type == ClaimTypes.System && systems.Any(s => s.ToLower() == c.Value?.ToLower())));
        }

        public static bool IsInRole(this ClaimsPrincipal user, string systemId, string roleId)
        {
            //return ((string.IsNullOrEmpty(systemId) || user.IsSystemEnabled(systemId)) && (user.IsAdmin() || user.HasClaim(c => c.Type == ClaimTypes.Role && c.Value.ToLower().StartsWith(systemId?.ToLower() + roleId?.ToLower()))));
            return ((string.IsNullOrEmpty(systemId) || user.IsSystemEnabled(systemId)) && (user.IsAdmin() || user.HasClaim(c => c.Type == ClaimTypes.Role && 
            (
              c.Value.ToLower().StartsWith(systemId?.ToLower() + roleId?.ToLower()) && !c.Value.ToLower().StartsWith("patentinventorremuneration") ||
              c.Value.ToLower() == roleId?.ToLower()
            )
            )));
        }

        public static bool IsInRoles(this ClaimsPrincipal user, string systemId, List<string> roles)
        {
            foreach (string role in roles)
            {
                if (user.IsInRole(systemId, role))
                {
                    return true;
                }
            }

            return false;
        }
        public static bool IsInRoles(this ClaimsPrincipal user, List<string> roles)
        {
            foreach (string system in user.GetSystems())
            {
                if (user.IsInRoles(system, roles))
                    return true;
            }

            return false;
        }

        public static bool IsInUserType(this ClaimsPrincipal user, CPiUserType userType)
        {
            return user.IsAdmin() || user.GetUserType() == userType;
        }

        public static bool IsInUserTypes(this ClaimsPrincipal user, List<CPiUserType> userTypes)
        {
            foreach (var userType in userTypes)
            {
                if (user.IsInUserType(userType))
                {
                    return true;
                }
            }

            return false;
        }

        public static CPiUserType GetUserType(this ClaimsPrincipal user)
        {
            if (!user.HasClaim(c => c.Type == CPiClaimTypes.UserType))
                return CPiUserType.User;

            //return (CPiUserType)Enum.Parse(typeof(CPiUserType), user.FindFirstValue(CPiClaimTypes.UserType));

            var userType = user.FindFirst(CPiClaimTypes.UserType)?.Value;
            try
            {
                return JsonConvert.DeserializeObject<CPiUserType>(userType.Decrypt(user.GetEncryptionKey()));
            }
            catch
            {
                return (CPiUserType)Enum.Parse(typeof(CPiUserType), userType);
            }
        }

        public static DefaultPageAction? GetDefaultPage(this ClaimsPrincipal user)
        {
            var defaultPage = user.FindFirst(CPiClaimTypes.DefaultPage)?.Value;
            if (!string.IsNullOrEmpty(defaultPage))
            {
                return JsonConvert.DeserializeObject<DefaultPageAction>(defaultPage.Decrypt(user.GetEncryptionKey()));
            }

            return null;
        }

        public static List<string> GetEnabledSystems(this ClaimsPrincipal user)
        {
            return user.Claims.Where(c => c.Type == CPiClaimTypes.System).Select(c => c.Value).ToList();
        }

        public static List<string> GetSystems(this ClaimsPrincipal user)
        {
            var enabledSystems = user.GetEnabledSystems();
            if (user.IsAdmin())
                return enabledSystems;
            else
                return user.Claims.Where(c => c.Type == ClaimTypes.System && enabledSystems.Contains(c.Value)).Select(c => c.Value).ToList();
        }

        public static List<SystemType> GetSystemTypes(this ClaimsPrincipal user)
        {
            var systems = user.GetSystems();
            return systems.Select(s => new SystemType() { TypeId = s.Substring(0, 1), SystemId = s }).ToList();
        }

        public static string GetTheme(this ClaimsPrincipal user)
        {
            return user.FindFirst(CPiClaimTypes.Theme)?.Value;
        }

        public static string GetLocale(this ClaimsPrincipal user)
        {
            return user.FindFirst(CPiClaimTypes.Locale)?.Value ?? "en";
        }

        public static SystemStatusType GetSystemStatus(this ClaimsPrincipal user)
        {
            if (SystemStatus != null)
                return (SystemStatusType)SystemStatus;

            return (SystemStatusType)Enum.Parse(typeof(SystemStatusType), user.FindFirstValue(CPiClaimTypes.SystemStatus) ?? ((int)SystemStatusType.Active).ToString());
        }

        public static void SetSystemStatus(this ClaimsPrincipal user, SystemStatusType systemStatus)
        {
            SystemStatus = systemStatus;
        }

        public static bool IsAMSIntegrated(this ClaimsPrincipal user)
        {
            return user.IsSystemEnabled(SystemType.Patent) && user.IsSystemEnabled(SystemType.AMS);
        }

        public static bool IsDeDocketer(this ClaimsPrincipal user, string systemId, string respOffice)
        {
            var role = "DeDocketer";

            var hasRespOfficeFilter = user.HasRespOfficeFilter(systemId);
            if (hasRespOfficeFilter)
                role = $"{role}|{respOffice}";

            if (user.IsAdmin())
                return false;

            if (hasRespOfficeFilter && string.IsNullOrEmpty(respOffice))
                return false;

            return user.IsInRoles(systemId, new List<string> { role });
        }

        public static List<CPiModule> GetModules(this ClaimsPrincipal user)
        {
            var modules = user.FindFirst(CPiClaimTypes.Modules)?.Value;
            if (!string.IsNullOrEmpty(modules))
            {
                return JsonConvert.DeserializeObject<List<CPiModule>>(modules.Decrypt(user.GetEncryptionKey()));
            }

            return new List<CPiModule>();
        }

        public static bool IsModuleEnabled(this ClaimsPrincipal user, params CPiModule[] module)
        {
            var modules = user.GetModules();
            return modules.Any(m => module.Contains(m));
        }

        public static bool IsSharedLimited(this ClaimsPrincipal user)
        {
            return user.IsInRoles(SystemType.Shared, CPiPermissions.LimitedRead) && !user.IsAdmin();
        }

        public static bool IsAuxiliaryLimited(this ClaimsPrincipal user, string systemId)
        {
            return user.IsInRoles(systemId, CPiPermissions.AuxiliaryLimited) && !user.IsAdmin();
        }

        public static int GetSearchResultsPageSize(this ClaimsPrincipal user, int defaultValue)
        {
            var searchResultsPageSize =  int.Parse(user.FindFirst(CPiClaimTypes.SearchResultsPageSize)?.Value ?? "0");
            return searchResultsPageSize > 0 ? searchResultsPageSize : defaultValue;
        }

        public static bool RestrictExportControl(this ClaimsPrincipal user)
        {
            //claim is null if IsExportControlOn setting is off
            return bool.Parse(user.FindFirst(CPiClaimTypes.RestrictExportControl)?.Value ?? "false");
        }

        public static string GetClientType(this ClaimsPrincipal user)
        {
            return user.FindFirst(CPiClaimTypes.ClientType)?.Value ?? string.Empty;
        }

        //2FA is required but disabled by user
        public static bool TwoFactorRequired(this ClaimsPrincipal user)
        {
            return user.HasClaim(c => c.Type == CPiClaimTypes.TwoFactorRequired && bool.Parse(c.Value));
        }

        //user is required to use SSO to login
        public static bool SSORequired(this ClaimsPrincipal user)
        {
            return user.HasClaim(c => c.Type == CPiClaimTypes.SSORequired && bool.Parse(c.Value));
        }

        public static bool ShowIPDecisionCentral(this ClaimsPrincipal user)
        {
            return user.GetUserType().IsEndUser() && (user.GetDefaultPage()?.Controller.Equals("IPDecisionCentral", StringComparison.InvariantCultureIgnoreCase) ?? false);
        }

        public static string GetHelpFolder(this ClaimsPrincipal user)
        {
            var baseFolder = user.FindFirst(CPiClaimTypes.ClientType)?.Value;

            if (user.GetUserType() == CPiUserType.DocketService)
                return $"{baseFolder}Limited";

            return baseFolder;
        }

        /// <summary>
        /// Determines the authentication type to use when getting Graph client
        /// </summary>
        /// <param name="user"></param>
        /// <returns>SharePointAccountType</returns>
        public static DocumentStorageAccountType GetDocumentStorageAccountType(this ClaimsPrincipal user)
        {
            return (DocumentStorageAccountType)Enum.Parse(typeof(DocumentStorageAccountType), user.FindFirstValue(CPiClaimTypes.DocumentStorageAccountType) ?? ((int)DocumentStorageAccountType.User).ToString());
        }

        public static DocumentStorageOptions GetDocumentStorageOption(this ClaimsPrincipal user)
        {
            return (DocumentStorageOptions)Enum.Parse(typeof(DocumentStorageOptions), user.FindFirstValue(CPiClaimTypes.DocumentStorage) ?? ((int)DocumentStorageOptions.BlobOrFileSystem).ToString());
        }

        public static bool HasDashboardAccess(this ClaimsPrincipal user, string systemId)
        {
            var dashboardAccess = user.GetDashboardAccess();
            return (user.IsSystemEnabled(systemId) && (user.IsAdmin() ||
                user.HasClaim(c => c.Type == ClaimTypes.System && c.Value?.ToLower() == systemId?.ToLower()) ||
                dashboardAccess.Exists(s => s.ToLower() == systemId.ToLower())
                ));
        }

        public static List<string> GetDashboardAccess(this ClaimsPrincipal user)
        {
            var dashboardAccessClaim = user.FindFirst(CPiClaimTypes.DashboardAccess)?.Value;
            if (!string.IsNullOrEmpty(dashboardAccessClaim))
            {
                var dashboardAccess = JsonConvert.DeserializeObject<List<string>>(dashboardAccessClaim.Decrypt(user.GetEncryptionKey()));
                if (dashboardAccess != null)
                {
                    return dashboardAccess.Where(s => SystemType.HasDashboardWidgets.Contains(s)).ToList();
                }
            }

            return new List<string>();
        }

        #region Trade Secret

        #region Patent
        public static bool CanAccessPatTradeSecret(this ClaimsPrincipal user)
        {
            return user.IsInUserTypes(CPiPermissions.CanHaveTSClearance) && user.Claims.Any(c => c.Type == CPiClaimTypes.TradeSecret && c.Value.Decrypt(user.GetEncryptionKey()) == SystemType.Patent);
        }

        public static bool CanEditPatTradeSecretFields(this ClaimsPrincipal user)
        {
            return user.CanAccessPatTradeSecret() && user.IsInRoles(SystemType.Patent, CPiPermissions.FullModify);
        }

        public static bool CanDeletePatTradeSecretFields(this ClaimsPrincipal user)
        {
            return user.CanAccessPatTradeSecret() && user.IsInRoles(SystemType.Patent, CPiPermissions.CanDelete);
        }

        public static bool IsPatTradeSecretAdmin(this ClaimsPrincipal user)
        {
            return user.CanAccessPatTradeSecret() && user.IsAdmin();
        }

        public static bool CanAccessPatTradeSecretReports(this ClaimsPrincipal user)
        {
            return user.IsInUserTypes(CPiPermissions.CanAccessTSReports) && user.Claims.Any(c => c.Type == CPiClaimTypes.TradeSecretReports && c.Value.Decrypt(user.GetEncryptionKey()) == SystemType.Patent);
        }
        #endregion

        #region DMS
        public static bool CanAccessDMSTradeSecret(this ClaimsPrincipal user)
        {
            return user.IsInUserTypes(CPiPermissions.CanHaveDMSTSClearance) && user.Claims.Any(c => c.Type == CPiClaimTypes.TradeSecret && c.Value.Decrypt(user.GetEncryptionKey()) == SystemType.DMS);
        }

        public static bool CanEditDMSTradeSecretFields(this ClaimsPrincipal user)
        {
            var dmsPermissions = CPiPermissions.FullModify;
            dmsPermissions.AddRange(CPiPermissions.Inventor);
            return user.CanAccessDMSTradeSecret() && user.IsInRoles(SystemType.DMS, dmsPermissions);
        }

        public static bool CanDeleteDMSTradeSecretFields(this ClaimsPrincipal user)
        {
            var dmsPermissions = CPiPermissions.CanDelete;
            dmsPermissions.AddRange(CPiPermissions.Inventor);
            return user.CanAccessDMSTradeSecret() && user.IsInRoles(SystemType.DMS, dmsPermissions);
        }

        public static bool IsDMSTradeSecretAdmin(this ClaimsPrincipal user)
        {
            return user.CanAccessDMSTradeSecret() && user.IsAdmin();
        }

        public static bool IsDMSTradeSecretModify(this ClaimsPrincipal user)
        {
            return user.CanAccessDMSTradeSecret() && user.IsInRoles(SystemType.DMS, CPiPermissions.FullModify);
        }

        public static bool CanAccessDMSTradeSecretReports(this ClaimsPrincipal user)
        {
            return user.IsInUserTypes(CPiPermissions.CanAccessTSReports) && user.Claims.Any(c => c.Type == CPiClaimTypes.TradeSecretReports && c.Value.Decrypt(user.GetEncryptionKey()) == SystemType.DMS);
        }
        #endregion

        #endregion

        public static int? GetEntityId(this ClaimsPrincipal user)
        {
            int? entityId = null;

            if (user.Claims.Any(c => c.Type == CPiClaimTypes.EntityId))
            {
                try
                {
                    entityId = Int32.Parse(user.Claims.First(c => c.Type == CPiClaimTypes.EntityId).Value);
                }
                catch
                {
                    entityId = 0;
                }
            }

            return entityId;
        }

        public static bool IsSoftDocketUser(this ClaimsPrincipal user)
        {
            return CPiPermissions.SoftDocketUsers.Contains(user.GetUserType());
        }

        public static bool IsRequestDocketUser(this ClaimsPrincipal user)
        {
            return CPiPermissions.RequestDocketUsers.Contains(user.GetUserType());
        }
    }
}
