using LawPortal.Core.Entities;
using LawPortal.Core.Identity;
using LawPortal.Web.Helpers;
using LawPortal.Web.Areas.Shared.ViewModels;
using LawPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
//using LawPortal.Core.Entities.MailDownload;
using LawPortal.Core.Helpers;
using LawPortal.Core.Entities.Shared;

namespace LawPortal.Web.Areas.Admin.Helpers
{
    public static class AdminQueryableExtensions
    {
        public static IQueryable<CPiUser> AddCriteria(this IQueryable<CPiUser> cpiUser, List<QueryFilterViewModel> mainSearchFilters)
        {
            var fullName = mainSearchFilters.FirstOrDefault(f => f.Property == "FullName");
            if (fullName != null)
            {
                cpiUser = cpiUser.Where(u => EF.Functions.Like(string.Concat(u.FirstName, " ", u.LastName), fullName.Value));
                mainSearchFilters.Remove(fullName);
            }

            var initial = mainSearchFilters.FirstOrDefault(f => f.Property == "Initial");
            if (initial != null)
            {
                cpiUser = cpiUser.Where(u => !string.IsNullOrEmpty(u.LastName) && u.LastName.StartsWith(initial.Value));
                mainSearchFilters.Remove(initial);
            }

            //default to true (restricted)
            var patTradeSecret = mainSearchFilters.FirstOrDefault(f => f.Property == "PatTradeSecret");
            if (patTradeSecret != null && !string.IsNullOrEmpty(patTradeSecret.Value))
            {
                bool.TryParse(patTradeSecret.Value, out var showRestricted);                
                cpiUser = cpiUser.Where(u => u.CPiUserSettings.Any(s => s.CPiSetting.Name == "UserAccountSettings" && s.Settings.Contains($"\"RestrictPatTradeSecret\":false")) == !showRestricted);
                mainSearchFilters.Remove(patTradeSecret);
            }

            //only pat users and admins have this option
            //default to true (restricted)
            var exportControl = mainSearchFilters.FirstOrDefault(f => f.Property == "ExportControl");
            if (exportControl != null && !string.IsNullOrEmpty(exportControl.Value))
            {
                bool.TryParse(exportControl.Value, out var showRestricted);
                if (showRestricted)
                    cpiUser = cpiUser.Where(u => (u.UserType != CPiUserType.Administrator && u.UserType != CPiUserType.SuperAdministrator &&
                        !u.CPiUserSystemRoles.Any(r => r.SystemId == SystemType.Patent && CPiPermissions.RegularUser.Contains(r.RoleId))) ||
                        !u.CPiUserSettings.Any(s => s.CPiSetting.Name == "UserAccountSettings" && s.Settings.Contains($"\"RestrictExportControl\":false"))); 
                else
                    cpiUser = cpiUser.Where(u => (u.UserType == CPiUserType.Administrator || u.UserType == CPiUserType.SuperAdministrator ||
                        u.CPiUserSystemRoles.Any(r => r.SystemId == SystemType.Patent && CPiPermissions.RegularUser.Contains(r.RoleId))) &&
                        u.CPiUserSettings.Any(s => s.CPiSetting.Name == "UserAccountSettings" && s.Settings.Contains($"\"RestrictExportControl\":false")));

                mainSearchFilters.Remove(exportControl);
            }

            var privateDocuments = mainSearchFilters.FirstOrDefault(f => f.Property == "PrivateDocuments");
            if (privateDocuments != null && !string.IsNullOrEmpty(privateDocuments.Value))
            {
                bool.TryParse(privateDocuments.Value, out var showRestricted);
                cpiUser = cpiUser.Where(u => u.CPiUserSettings.Any(s => s.CPiSetting.Name == "UserAccountSettings" && s.Settings.Contains($"\"RestrictPrivateDocuments\":true")) == showRestricted);
                mainSearchFilters.Remove(privateDocuments);
            }

            //only pat users and admins have this option
            var inventorInfo = mainSearchFilters.FirstOrDefault(f => f.Property == "InventorInfo");
            if (inventorInfo != null && !string.IsNullOrEmpty(inventorInfo.Value))
            {
                bool.TryParse(inventorInfo.Value, out var showRestricted);
                if (showRestricted)
                    cpiUser = cpiUser.Where(u => (u.UserType != CPiUserType.Administrator && u.UserType != CPiUserType.SuperAdministrator) &&
                        !u.CPiUserSystemRoles.Any(r => r.SystemId == SystemType.Patent && CPiPermissions.RegularUser.Contains(r.RoleId)) ||
                        u.CPiUserSettings.Any(s => s.CPiSetting.Name == "UserAccountSettings" && s.Settings.Contains($"\"RestrictInventorInfo\":true")));
                else
                    cpiUser = cpiUser.Where(u => (u.UserType == CPiUserType.Administrator || u.UserType == CPiUserType.SuperAdministrator) ||
                        u.CPiUserSystemRoles.Any(r => r.SystemId == SystemType.Patent && CPiPermissions.RegularUser.Contains(r.RoleId)) && 
                        !u.CPiUserSettings.Any(s => s.CPiSetting.Name == "UserAccountSettings" && s.Settings.Contains($"\"RestrictInventorInfo\":true")));

                mainSearchFilters.Remove(inventorInfo);
            }

            //only pat/tmk/gm modify have this option
            //admins have no restrictions
            var adhocActions = mainSearchFilters.FirstOrDefault(f => f.Property == "AdhocActions");
            if (adhocActions != null && !string.IsNullOrEmpty(adhocActions.Value))
            {
                bool.TryParse(adhocActions.Value, out var showRestricted);
                if (showRestricted)
                    cpiUser = cpiUser.Where(u => (u.UserType != CPiUserType.Administrator && u.UserType != CPiUserType.SuperAdministrator) && 
                        !u.CPiUserSystemRoles.Any(r => 
                                r.SystemId == SystemType.Patent && CPiPermissions.FullModify.Contains(r.RoleId) ||
                                r.SystemId == SystemType.Trademark && CPiPermissions.FullModify.Contains(r.RoleId) ||
                                r.SystemId == SystemType.GeneralMatter && CPiPermissions.FullModify.Contains(r.RoleId)) || 
                        u.CPiUserSettings.Any(s => s.CPiSetting.Name == "UserAccountSettings" && s.Settings.Contains($"\"RestrictAdhocActions\":true")));
                else
                    cpiUser = cpiUser.Where(u => (u.UserType == CPiUserType.Administrator || u.UserType == CPiUserType.SuperAdministrator) || 
                        u.CPiUserSystemRoles.Any(r => 
                                r.SystemId == SystemType.Patent && CPiPermissions.FullModify.Contains(r.RoleId) ||
                                r.SystemId == SystemType.Trademark && CPiPermissions.FullModify.Contains(r.RoleId) ||
                                r.SystemId == SystemType.GeneralMatter && CPiPermissions.FullModify.Contains(r.RoleId)) &&
                        !u.CPiUserSettings.Any(s => s.CPiSetting.Name == "UserAccountSettings" && s.Settings.Contains($"\"RestrictAdhocActions\":true")));

                mainSearchFilters.Remove(adhocActions);
            }

            if (mainSearchFilters.Any())
                cpiUser = QueryHelper.BuildCriteria<CPiUser>(cpiUser, mainSearchFilters);

            return cpiUser;
        }

        public static IQueryable<CPiMenuPage> AddCriteria(this IQueryable<CPiMenuPage> menuPage, List<QueryFilterViewModel> mainSearchFilters)
        {
            var area = mainSearchFilters.FirstOrDefault(f => f.Property == "Area");
            if (area != null)
            {
                menuPage = menuPage.Where(p => EF.Functions.Like(SqlHelper.JsonValue(p.RouteOptions, "$.area"), area.Value));
                mainSearchFilters.Remove(area);
            }

            if (mainSearchFilters.Any())
                menuPage = QueryHelper.BuildCriteria<CPiMenuPage>(menuPage, mainSearchFilters);

            return menuPage;
        }

        /*public static IQueryable<MailDownloadDataMap> AddCriteria(this IQueryable<MailDownloadDataMap> maps, List<QueryFilterViewModel> mainSearchFilters)
        {
            var mapName = mainSearchFilters.FirstOrDefault(f => f.Property == "AttributeName");
            if (mapName != null)
            {
                maps = maps.Where(m => EF.Functions.Like(m.Attribute.Name, mapName.Value));
                mainSearchFilters.Remove(mapName);
            }

            var pattern = mainSearchFilters.FirstOrDefault(f => f.Property == "Pattern");
            if (pattern != null)
            {
                maps = maps.Where(m => m.MapPatterns.Any(p => EF.Functions.Like(p.Pattern, pattern.Value)));
                mainSearchFilters.Remove(pattern);
            }

            if (mainSearchFilters.Any())
                maps = QueryHelper.BuildCriteria<MailDownloadDataMap>(maps, mainSearchFilters);

            return maps;
        }*/

        /*public static IQueryable<TradeSecretRequest> AddCriteria(this IQueryable<TradeSecretRequest> tradeSecretRequest, List<QueryFilterViewModel> mainSearchFilters)
        {
            //hide expired by default
            var showExpired = mainSearchFilters.FirstOrDefault(f => f.Property == "ShowExpired");
            if (showExpired == null) 
                tradeSecretRequest = tradeSecretRequest.Where(r => 
                    r.Status != TradeSecretRequestStatus.Denied && 
                    r.Status != TradeSecretRequestStatus.Revoked &&
                    r.ValidationFailedCount < TradeSecretHelper.MaxValidationFailedCount &&
                    EF.Functions.DateDiffMinute(r.StatusDate, DateTime.Now) <= TradeSecretHelper.RequestExpiration
                    );
            else
                mainSearchFilters.Remove(showExpired);

            if (mainSearchFilters.Any())
                tradeSecretRequest = QueryHelper.BuildCriteria<TradeSecretRequest>(tradeSecretRequest, mainSearchFilters);

            return tradeSecretRequest;
        }*/

        public static IQueryable<ScheduledTask> AddCriteria(this IQueryable<ScheduledTask> tasks, List<QueryFilterViewModel> mainSearchFilters)
        {
            if (mainSearchFilters.Any())
                tasks = QueryHelper.BuildCriteria<ScheduledTask>(tasks, mainSearchFilters);

            return tasks;
        }
    }
}
