using R10.Core.Identity;
using R10.Web.Areas.Shared.ViewModels;

namespace R10.Web.Interfaces
{
    public interface INotificationSettingManager
    {
        Task<UserNotificationSettings> GetUserSetting(string userId);
        Task SaveUserSetting(string userId, UserNotificationSettings settings);
        Task<List<LocalizedMailAddress>> GetRMSInstructionNotificationRecipients();
        Task<List<LocalizedMailAddress>> GetFFInstructionNotificationRecipients();
        Task<List<LocalizedMailAddress>> GetDeDocketInstructionNotificationRecipients();
        Task<List<LocalizedMailAddress>> GetRegistrationApprovalNotificationRecipients();
        Task<List<LocalizedMailAddress>> GetTaskSchedulerNotificationRecipients();
        Task<List<string>> GetDoNotSendQuickEmailList(QuickEmailOptOutSetting option, char systemType);
    }
}
