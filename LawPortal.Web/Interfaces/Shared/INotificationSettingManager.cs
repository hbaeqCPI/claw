using LawPortal.Core.Identity;
using LawPortal.Web.Areas.Shared.ViewModels;

namespace LawPortal.Web.Interfaces
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
