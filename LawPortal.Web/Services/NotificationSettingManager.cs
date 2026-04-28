using Newtonsoft.Json.Linq;
using Microsoft.EntityFrameworkCore;
using LawPortal.Core.Identity;
using LawPortal.Core.Interfaces;
using LawPortal.Web.Areas.Shared.ViewModels;
using LawPortal.Web.Interfaces;
using LawPortal.Core.Entities;
using LawPortal.Core.Helpers;
using LawPortal.Infrastructure.Data;
using System.Reflection;
using System.ComponentModel;
using LawPortal.Core.Entities.Shared;

namespace LawPortal.Web.Services
{
    public class NotificationSettingManager : INotificationSettingManager
    {
        private readonly ICPiDbContext _cpiDbContext;
        private readonly ICPiUserSettingManager _userSettingsManager;

        public NotificationSettingManager(ICPiDbContext cpiDbContext, ICPiUserSettingManager userSettingsManager)
        {
            _cpiDbContext = cpiDbContext;
            _userSettingsManager = userSettingsManager;
        }

        private IQueryable<CPiUserSetting> CPiUserSettings => _cpiDbContext.GetRepository<CPiUserSetting>().QueryableList;

        private UserNotificationSettings DefaultSettings => new UserNotificationSettings();

        public async Task<UserNotificationSettings> GetUserSetting(string userId)
        {
            return await _userSettingsManager.GetUserSetting<UserNotificationSettings>(userId);
        }

        public async Task SaveUserSetting(string userId, UserNotificationSettings settings)
        {
            await _userSettingsManager.SaveUserSetting(userId, CPiSettings.UserNotificationSettings, JObject.FromObject(settings));
        }

        public async Task<List<LocalizedMailAddress>> GetFFInstructionNotificationRecipients()
        {
            return await CPiUserSettings.Where(s => s.CPiSetting.Name == CPiSettings.UserNotificationSettings &&
                                                    s.CPiUser.Status == CPiUserStatus.Approved &&
                                                    Convert.ToBoolean(SqlHelper.JsonValue(s.Settings, $"$.{nameof(UserNotificationSettings.ReceiveFFInstructionNotification)}") ?? $"{DefaultSettings.ReceiveFFInstructionNotification}") &&
                                                    (s.CPiUser.UserType == CPiUserType.Administrator || s.CPiUser.UserType == CPiUserType.SuperAdministrator ||
                                                        s.CPiUser.CPiUserSystemRoles.Any(r => r.SystemId == SystemType.ForeignFiling && CPiPermissions.CanReceiveInstructionsNotifications.Contains(r.RoleId))))
                                        .Select(r => new LocalizedMailAddress(r.CPiUser.Email, r.CPiUser.FullName, r.CPiUser.Locale))
                                        .ToListAsync();
        }

        public async Task<List<LocalizedMailAddress>> GetRMSInstructionNotificationRecipients()
        {
            return await CPiUserSettings.Where(s => s.CPiSetting.Name == CPiSettings.UserNotificationSettings &&
                                                    s.CPiUser.Status == CPiUserStatus.Approved &&
                                                    Convert.ToBoolean(SqlHelper.JsonValue(s.Settings, $"$.{nameof(UserNotificationSettings.ReceiveRMSInstructionNotification)}") ?? $"{DefaultSettings.ReceiveRMSInstructionNotification}") &&
                                                    (s.CPiUser.UserType == CPiUserType.Administrator || s.CPiUser.UserType == CPiUserType.SuperAdministrator ||
                                                        s.CPiUser.CPiUserSystemRoles.Any(r => r.SystemId == SystemType.RMS && CPiPermissions.CanReceiveInstructionsNotifications.Contains(r.RoleId))))
                                        .Select(r => new LocalizedMailAddress(r.CPiUser.Email, r.CPiUser.FullName, r.CPiUser.Locale))
                                        .ToListAsync();
        }

        public async Task<List<LocalizedMailAddress>> GetDeDocketInstructionNotificationRecipients()
        {
            var deDocketSystems = new List<string>() { SystemType.Patent, SystemType.Trademark, SystemType.GeneralMatter };
            return await CPiUserSettings.Where(s => s.CPiSetting.Name == CPiSettings.UserNotificationSettings &&
                                                    s.CPiUser.Status == CPiUserStatus.Approved &&
                                                    Convert.ToBoolean(SqlHelper.JsonValue(s.Settings, $"$.{nameof(UserNotificationSettings.ReceiveDeDocketInstructionNotification)}") ?? $"{DefaultSettings.ReceiveDeDocketInstructionNotification}") &&
                                                    (s.CPiUser.UserType == CPiUserType.Administrator || s.CPiUser.UserType == CPiUserType.SuperAdministrator ||
                                                        s.CPiUser.CPiUserSystemRoles.Any(r => deDocketSystems.Contains(r.SystemId) && CPiPermissions.CanReceiveInstructionsNotifications.Contains(r.RoleId))))
                                        .Select(r => new LocalizedMailAddress(r.CPiUser.Email, r.CPiUser.FullName, r.CPiUser.Locale))
                                        .ToListAsync();
        }

        public async Task<List<LocalizedMailAddress>> GetRegistrationApprovalNotificationRecipients()
        {
            return await CPiUserSettings.Where(s => s.CPiSetting.Name == CPiSettings.UserNotificationSettings &&
                                                    s.CPiUser.Status == CPiUserStatus.Approved &&
                                                    Convert.ToBoolean(SqlHelper.JsonValue(s.Settings, $"$.{nameof(UserNotificationSettings.ReceivePendingRegistrationNotification)}") ?? $"{DefaultSettings.ReceivePendingRegistrationNotification}") &&
                                                    (s.CPiUser.UserType == CPiUserType.Administrator || s.CPiUser.UserType == CPiUserType.SuperAdministrator))
                                        .Select(r => new LocalizedMailAddress(r.CPiUser.Email, r.CPiUser.FullName, r.CPiUser.Locale))
                                        .ToListAsync();
        }

        /// <summary>
        /// Task scheduler error notification recipients
        /// </summary>
        /// <returns></returns>
        public async Task<List<LocalizedMailAddress>> GetTaskSchedulerNotificationRecipients()
        {
            var recipients =  await CPiUserSettings.Where(s => s.CPiSetting.Name == CPiSettings.UserNotificationSettings &&
                                                    s.CPiUser.Status == CPiUserStatus.Approved &&
                                                    Convert.ToBoolean(SqlHelper.JsonValue(s.Settings, $"$.{nameof(UserNotificationSettings.ReceiveTaskSchedulerNotification)}") ?? $"{DefaultSettings.ReceiveTaskSchedulerNotification}") &&
                                                    (s.CPiUser.UserType == CPiUserType.Administrator || s.CPiUser.UserType == CPiUserType.SuperAdministrator))
                                        .Select(r => new LocalizedMailAddress(r.CPiUser.Email, r.CPiUser.FullName, r.CPiUser.Locale))
                                        .ToListAsync();

            var adminEmail = await _cpiDbContext.GetRepository<Option>().QueryableList
                                        .Where(o => o.OptionSubKey == "AdminEmail")
                                        .Select(o => o.OptionValue).FirstOrDefaultAsync();

            // Include admin email if not found in recipients
            if (!string.IsNullOrEmpty(adminEmail) && !recipients.Any(r => r.MailAddress?.Address == adminEmail))
                recipients.Add(new LocalizedMailAddress(adminEmail, adminEmail, "en"));

            return recipients;
        }

        public async Task<List<string>> GetDoNotSendQuickEmailList(QuickEmailOptOutSetting option, char systemType)
        {
            if (option == QuickEmailOptOutSetting.Workflow)
                return await CPiUserSettings.Where(s => s.CPiSetting.Name == CPiSettings.UserNotificationSettings && (
                                                        !Convert.ToBoolean(SqlHelper.JsonValue(s.Settings, $"$.{nameof(UserNotificationSettings.ReceiveWorkflowEmail)}") ?? $"{DefaultSettings.ReceiveWorkflowEmail}") ||
                                                        !EF.Functions.Like(SqlHelper.JsonValue(s.Settings, $"$.{nameof(UserNotificationSettings.WorkflowSystems)}") ?? DefaultSettings.WorkflowSystems, $"%{systemType}%")
                                                        ))
                                            .Select(r => r.CPiUser.Email.Trim())
                                            .ToListAsync();

            if (option == QuickEmailOptOutSetting.PatentWatch)
                return await CPiUserSettings.Where(s => s.CPiSetting.Name == CPiSettings.UserNotificationSettings && 
                                                        !Convert.ToBoolean(SqlHelper.JsonValue(s.Settings, $"$.{nameof(UserNotificationSettings.ReceivePatentWatchEmail)}") ?? $"{DefaultSettings.ReceivePatentWatchEmail}")
                                                        )
                                            .Select(r => r.CPiUser.Email.Trim())
                                            .ToListAsync();

            if (option == QuickEmailOptOutSetting.DMSNewDisclosure)
                return await CPiUserSettings.Where(s => s.CPiSetting.Name == CPiSettings.UserNotificationSettings &&
                                                        !Convert.ToBoolean(SqlHelper.JsonValue(s.Settings, $"$.{nameof(UserNotificationSettings.ReceiveDMSNewDisclosureNotification)}") ?? $"{DefaultSettings.ReceiveDMSNewDisclosureNotification}")
                                                        )
                                            .Select(r => r.CPiUser.Email.Trim())
                                            .ToListAsync();

            if (option == QuickEmailOptOutSetting.DMSNewDiscussion)
                return await CPiUserSettings.Where(s => s.CPiSetting.Name == CPiSettings.UserNotificationSettings &&
                                                        !Convert.ToBoolean(SqlHelper.JsonValue(s.Settings, $"$.{nameof(UserNotificationSettings.ReceiveDMSNewDiscussionNotification)}") ?? $"{DefaultSettings.ReceiveDMSNewDiscussionNotification}")
                                                        )
                                            .Select(r => r.CPiUser.Email.Trim())
                                            .ToListAsync();

            if (option == QuickEmailOptOutSetting.DMSDiscussionReply)
                return await CPiUserSettings.Where(s => s.CPiSetting.Name == CPiSettings.UserNotificationSettings &&
                                                        !Convert.ToBoolean(SqlHelper.JsonValue(s.Settings, $"$.{nameof(UserNotificationSettings.ReceiveDMSDiscussionReplyNotification)}") ?? $"{DefaultSettings.ReceiveDMSDiscussionReplyNotification}")
                                                        )
                                            .Select(r => r.CPiUser.Email.Trim())
                                            .ToListAsync();

            if (option == QuickEmailOptOutSetting.DMSInventorChange)
                return await CPiUserSettings.Where(s => s.CPiSetting.Name == CPiSettings.UserNotificationSettings &&
                                                        !Convert.ToBoolean(SqlHelper.JsonValue(s.Settings, $"$.{nameof(UserNotificationSettings.ReceiveDMSInventorChangeNotification)}") ?? $"{DefaultSettings.ReceiveDMSInventorChangeNotification}")
                                                        )
                                            .Select(r => r.CPiUser.Email.Trim())
                                            .ToListAsync();

            if (option == QuickEmailOptOutSetting.ActionDelegated)
                return await CPiUserSettings.Where(s => s.CPiSetting.Name == CPiSettings.UserNotificationSettings &&
                                                        !Convert.ToBoolean(SqlHelper.JsonValue(s.Settings, $"$.{nameof(UserNotificationSettings.ReceiveActionDelegatedNotification)}") ?? $"{DefaultSettings.ReceiveActionDelegatedNotification}")
                                                        )
                                            .Select(r => r.CPiUser.Email.Trim())
                                            .ToListAsync();

            if (option == QuickEmailOptOutSetting.ActionCompleted)
                return await CPiUserSettings.Where(s => s.CPiSetting.Name == CPiSettings.UserNotificationSettings &&
                                                        !Convert.ToBoolean(SqlHelper.JsonValue(s.Settings, $"$.{nameof(UserNotificationSettings.ReceiveActionCompletedNotification)}") ?? $"{DefaultSettings.ReceiveActionCompletedNotification}")
                                                        )
                                            .Select(r => r.CPiUser.Email.Trim())
                                            .ToListAsync();

            if (option == QuickEmailOptOutSetting.ActionDeleted)
                return await CPiUserSettings.Where(s => s.CPiSetting.Name == CPiSettings.UserNotificationSettings &&
                                                        !Convert.ToBoolean(SqlHelper.JsonValue(s.Settings, $"$.{nameof(UserNotificationSettings.ReceiveActionDeletedNotification)}") ?? $"{DefaultSettings.ReceiveActionDeletedNotification}")
                                                        )
                                            .Select(r => r.CPiUser.Email.Trim())
                                            .ToListAsync();

            if (option == QuickEmailOptOutSetting.ActionReassigned)
                return await CPiUserSettings.Where(s => s.CPiSetting.Name == CPiSettings.UserNotificationSettings &&
                                                        !Convert.ToBoolean(SqlHelper.JsonValue(s.Settings, $"$.{nameof(UserNotificationSettings.ReceiveActionDeletedNotification)}") ?? $"{DefaultSettings.ReceiveActionDeletedNotification}")
                                                        )
                                            .Select(r => r.CPiUser.Email.Trim())
                                            .ToListAsync();

            if (option == QuickEmailOptOutSetting.ActionDueDateChanged)
                return await CPiUserSettings.Where(s => s.CPiSetting.Name == CPiSettings.UserNotificationSettings &&
                                                        !Convert.ToBoolean(SqlHelper.JsonValue(s.Settings, $"$.{nameof(UserNotificationSettings.ReceiveActionDueDateChangedNotification)}") ?? $"{DefaultSettings.ReceiveActionDueDateChangedNotification}")
                                                        )
                                            .Select(r => r.CPiUser.Email.Trim())
                                            .ToListAsync();

            if (option == QuickEmailOptOutSetting.DMSActionReminder)
                return await CPiUserSettings.Where(s => s.CPiSetting.Name == CPiSettings.UserNotificationSettings &&
                                                        !Convert.ToBoolean(SqlHelper.JsonValue(s.Settings, $"$.{nameof(UserNotificationSettings.ReceiveDMSActionReminder)}") ?? $"{DefaultSettings.ReceiveDMSActionReminder}")
                                                        )
                                            .Select(r => r.CPiUser.Email.Trim())
                                            .ToListAsync();

            return await CPiUserSettings.Where(s => s.CPiSetting.Name == CPiSettings.UserNotificationSettings && (
                                                    !Convert.ToBoolean(SqlHelper.JsonValue(s.Settings, $"$.{nameof(UserNotificationSettings.ReceiveQuickEmail)}") ?? $"{DefaultSettings.ReceiveQuickEmail}") ||
                                                    !EF.Functions.Like(SqlHelper.JsonValue(s.Settings, $"$.{nameof(UserNotificationSettings.QuickEmailSystems)}") ?? DefaultSettings.QuickEmailSystems, $"%{systemType}%")
                                                    ))
                                        .Select(r => r.CPiUser.Email.Trim())
                                        .ToListAsync();
        }
    }
}
