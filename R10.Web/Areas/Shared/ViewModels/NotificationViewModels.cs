using System.Net.Mail;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class LocalizedMailAddress
    {
        public LocalizedMailAddress(string address, string displayName, string locale)
        {
            Address = address;
            DisplayName = displayName;
            Locale = locale ?? "en";
            MailAddress = new MailAddress(address, displayName);
        }
        public string Address { get; set; }
        public string DisplayName { get; set; }
        public string Locale { get; set; }
        public MailAddress MailAddress { get; set; }
    }

    public enum QuickEmailOptOutSetting
    {
        QuickEmail, Workflow, PatentWatch, DMSNewDisclosure, DMSNewDiscussion, DMSDiscussionReply,
        DMSInventorChange, ActionDelegated, ActionCompleted, ActionDeleted, ActionReassigned,
        ActionDueDateChanged, DMSActionReminder
    }
}
