using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Identity
{
    public class CPiSettings
    {
        public const string DefaultPage = "DefaultPage";
        public const string QuickDocket = "QuickDocket";
        public const string UserPreferences = "UserPreferences";
        public const string UserAccountSettings = "UserAccountSettings";
        public const string UserNotificationSettings = "UserNotificationSettings";
        public const string SystemStatus = "SystemStatus";
        public const string SystemNotification = "SystemNotification";
        public const string CookieConsent = "CookieConsent";
        public const string ActionIndicator = "ActionIndicator";
        public const string DeDocketFields = "DeDocketFields";
        public const string DocVerificationReviewFilters = "DocVerificationReviewFilters";
        public const string InventionDisclosureStatus = "InventionDisclosureStatus";
    }
    public class DefaultPage
    {
        public int DefaultPageId { get; set; }
    }
    public class UserPreferences
    {
        public string Theme { get; set; } = "default";
        public string LanguageSource { get; set; } = "User";
        public DateTime SystemStatusMessageSeenDate { get; set; }
        public DateTime CookieConsentDate { get; set; }
        public int SearchResultsPageSize { get; set; }
    }
    public class UserAccountSettings
    {
        public bool RestrictPrivateDocuments { get; set; }
        public bool RestrictExportControl { get; set; } = true;
        public bool RestrictInventorInfo { get; set; }
        public bool RestrictAdhocActions { get;set; }
        public bool RestrictPatTradeSecret { get; set; } = true;
        public bool RestrictPatTradeSecretReports { get; set; } = true;
        public List<string> DashboardAccess { get; set; } = new List<string>();
        public List<string> Mailboxes { get; set; } = new List<string>();
        public DocumentStorageAccountType DocumentStorageAccountType { get; set; } = DocumentStorageAccountType.User;
        public bool AllowHandleMyEPOCommunications { get; set; } = false;
        public bool RestrictDMSTradeSecret { get; set; } = true;
        public bool RestrictDMSTradeSecretReports { get; set; } = true;
    }
    public class UserNotificationSettings
    {
        public bool ReceiveAMSInstructionNotification { get; set; }
        public bool ReceiveAMSSendToCPIReminder { get; set; } = true;

        public bool ReceiveRMSInstructionNotification { get; set; }
        public bool ReceiveFFInstructionNotification { get; set; }
        public bool ReceiveDeDocketInstructionNotification { get; set; }
        public bool ReceivePendingRegistrationNotification { get; set; }
        public bool ReceiveTaskSchedulerNotification { get; set; }
        public bool ReceiveQuickEmail { get; set; } = true;
        public bool QuickEmailCopyToSelf { get; set; } = true;
        public string QuickEmailSystems { get; set; } = "PEADTCG";
        public bool ReceiveWorkflowEmail { get; set; } = true;
        public string WorkflowSystems { get; set; } = "PEDTCG";
        public bool ReceivePatentWatchEmail { get;set; } = true;

        //DMS
        public bool ReceiveDMSNewDisclosureNotification { get; set; } = true;
        public bool ReceiveDMSNewDiscussionNotification { get; set; } = true;
        public bool ReceiveDMSDiscussionReplyNotification { get; set; } = true;
        public bool ReceiveDMSInventorChangeNotification { get; set; } = true;        
        //DMS Action Reminder
        public bool ReceiveDMSActionReminder { get; set;} = true;
        public int DMSActionReminderRepeatInterval { get; set; } = 0;       //int value from 0
        public int DMSActionReminderRepeatRecurrence { get; set; } = 0;     //int value 0 none; 1 days; 2 weeks; 3 months
        public int DMSActionReminderRepeatOnDay { get; set; } = 1;          //int value (available only if RepeatRecurrence above is 2(weeks)) 1 monday; 2 tue; 3 wed; 4 thu; 5 fri; 6 sat; 7 sun;


        //Delegate Action Management
        public bool ReceiveActionDelegatedNotification { get; set; } = true;
        public bool ReceiveActionCompletedNotification { get; set; } = true;
        public bool ReceiveActionDeletedNotification { get; set; } = true;
        public bool ReceiveActionReassignedNotification { get; set; } = true;
        public bool ReceiveActionDueDateChangedNotification { get; set; } = true;
        public int DaysAfterActionDueToKeepReceivingNotifications { get; set; } 
    }

    public enum SystemStatusType
    {
        Active = 0,
        Maintenance = 1,
        [Display(Name = "Read Only")]
        ReadOnly = 9
    }
    public class SystemStatus
    {
        public SystemStatusType StatusType { get; set; }
        public SystemNotification? Notification { get; set; }

        public static SystemStatus ReadOnly { get; } = new SystemStatus()
        {
            StatusType = SystemStatusType.ReadOnly,
            Notification = SystemNotification.ReadOnly
        };
    }

    public class SystemNotification
    {
        [MaxLength(280)]
        public string? Message { get; set; }
        public bool Active { get; set; }
        public DateTime? ActiveFrom { get; set; }
        public DateTime? ActiveTo { get; set; }
        public int Severity { get; set; }

        public static SystemNotification ReadOnly { get; } = new SystemNotification()
        {
            Message = "Database is in read-only mode until further notice.",
            Active = true,
            Severity = 1
        };

        public static SystemNotification CookieConsent { get; } = new SystemNotification()
        {
            Message = "This website uses cookies to give you the best browsing experience. By choosing 'I Agree' you accept the use of cookies in accordance with our Privacy Policy.",
            Active = true,
            Severity = 0
        };
    }


    public class CPiIdentitySettings
    {
        public Password Password { get; set; }
        public SignIn SignIn { get; set; }
        public Lockout Lockout { get; set; }
        public Registration Registration { get; set; }
        public ExternalLogin ExternalLogin { get; set; }
        public Cookie Cookie { get; set; }
    }
    public class Password
    {
        public int ExpireDays { get; set; }
        public int LastUnique { get; set; }
        public bool CanHavePartsOfName { get; set; }
        public int MinimumCharsFromName { get; set; }
        public string NamePartsSeparator { get; set; }
    }
    public class SignIn
    {
        public int InactiveDays { get; set; }
        public bool AllowSaveEmail { get; set; }
        public bool AllowStaySignedIn { get; set; }
        public bool RequireTwoFactorAuthentication { get; set; }
        public bool RequireExternalLoginTwoFactor { get; set; }
        public List<string>? RequireSSODomains { get; set; }
        public int ActivationPeriodInDays { get; set; }
    }
    public class Lockout
    {
        public bool DisableAccount { get; set; }
    }
    public class Registration
    {
        public bool Allowed { get; set; }
        public bool AllowedForExernalLogin { get; set; }
        public List<string>? ValidEmailDomains { get; set; }
    }
    public class ExternalLogin
    {
        public bool AutoRegister { get; set; }
        public bool AutoLink { get; set; }
        public string RoleAttributeName { get; set; } = "CpiRole";
        public bool RequireRoleAttribute { get; set; }
        public string? EmailAttributeName { get; set; }
        public string? FirstNameAttributeName { get; set; }
        public string? LastNameAttributeName { get; set; }
        public bool NeedsApproval { get; set; }
        public bool DisablePassword { get; set; }
    }
    public class Cookie
    {
        public TimeSpan StaySignedInTimeSpan { get; set; }
    }
    public class ActionIndicator
    {
        public bool Enabled { get; set; }
        public Dictionary<string, string> Colors { get; set; } = new Dictionary<string, string>();
    }

    public class Theme
    {
        public Dictionary<string, string> Colors { get; set; } = new Dictionary<string, string>();
    }

    public class DeDocketFields
    {
        [Display(GroupName = "Patent", Name = "Invention")]
        public DeDocketInventionFields? Invention { get; set; }

        [Display(GroupName = "Patent", Name = "Country Application")]
        public DeDocketCountryApplicationFields? CountryApplication { get; set; }

        [Display(GroupName = "Patent", Name = "Patent Action Due")]
        public DeDocketActionDueFields? PatentActionDue { get; set; }

        [Display(GroupName = "Patent", Name = "Patent Due Date")]
        public DeDocketDueDateFields? PatentDueDate { get; set; }

        [Display(GroupName = "Trademark", Name = "Trademark")]
        public DeDocketTrademarkFields? Trademark { get; set; }

        [Display(GroupName = "Trademark", Name = "Trademark Action Due")]
        public DeDocketActionDueFields? TrademarkActionDue { get; set; }

        [Display(GroupName = "Trademark", Name = "Trademark Due Date")]
        public DeDocketDueDateFields? TrademarkDueDate { get; set; }

        [Display(GroupName = "GeneralMatter", Name = "General Matter")]
        public DeDocketGeneralMatterFields? GeneralMatter { get; set; }

        [Display(GroupName = "GeneralMatter", Name = "General Matter Action Due")]
        public DeDocketActionDueFields? GeneralMatterActionDue { get; set; }

        [Display(GroupName = "GeneralMatter", Name = "General Matter Due Date")]
        public DeDocketDueDateFields? GeneralMatterDueDate { get; set; }
    }

    public class DeDocketInventionFields
    {
        [Display(Name = "Title")]
        public bool Title { get; set; }

        [Display(Name = "Attorney1")]
        public bool Attorney1 { get; set; }

        [Display(Name = "Attorney2")]
        public bool Attorney2 { get; set; }

        [Display(Name = "Attorney3")]
        public bool Attorney3 { get; set; }

        [Display(Name = "Attorney4")]
        public bool Attorney4 { get; set; }

        [Display(Name = "Attorney5")]
        public bool Attorney5 { get; set; }

        [Display(Name = "LabelClientRef")]
        public bool ClientReference { get; set; }

        [Display(Name = "Remarks")]
        public bool Remarks { get; set; }

        [Display(Name = "Upload Documents")]
        public bool UploadDocuments { get; set; }
    }

    public class DeDocketCountryApplicationFields
    {
        [Display(Name = "Application Title")]
        public bool Title { get; set; }

        [Display(Name = "Tax Schedule")]
        public bool TaxSchedule { get; set; }

        [Display(Name = "LabelClientRef")]
        public bool ClientReference { get; set; }

        [Display(Name = "Other Reference Number")]
        public bool OtherReferenceNumber { get; set; }

        [Display(Name = "LabelAgentRef")]
        public bool AgentReference { get; set; }

        [Display(Name = "Remarks")]
        public bool Remarks { get; set; }

        [Display(Name = "Upload Documents")]
        public bool UploadDocuments { get; set; }
    }

    public class DeDocketActionDueFields
    {
        [Display(Name = "Remarks")]
        public bool Remarks { get; set; }

        [Display(Name = "Upload Documents")]
        public bool UploadDocuments { get; set; }
    }

    public class DeDocketDueDateFields
    {
        [Display(Name = "Remarks")]
        public bool Remarks { get; set; }
    }

    public class DeDocketTrademarkFields
    {
        [Display(Name = "Trademark Name")]
        public bool TrademarkName { get; set; }

        [Display(Name = "Attorney1")]
        public bool Attorney1 { get; set; }

        [Display(Name = "Attorney2")]
        public bool Attorney2 { get; set; }

        [Display(Name = "Attorney3")]
        public bool Attorney3 { get; set; }

        [Display(Name = "Attorney4")]
        public bool Attorney4 { get; set; }

        [Display(Name = "Attorney5")]
        public bool Attorney5 { get; set; }

        [Display(Name = "LabelClientRef")]
        public bool ClientReference { get; set; }

        [Display(Name = "Other Reference Number")]
        public bool OtherReferenceNumber { get; set; }

        [Display(Name = "LabelAgentRef")]
        public bool AgentReference { get; set; }

        [Display(Name = "Goods")]
        public bool Goods { get; set; }

        [Display(Name = "Remarks")]
        public bool Remarks { get; set; }

        [Display(Name = "Upload Documents")]
        public bool UploadDocuments { get; set; }
    }

    public class DeDocketGeneralMatterFields
    {
        [Display(Name = "Matter Title")]
        public bool MatterTitle { get; set; }

        [Display(Name = "Attorneys")]
        public bool Attorneys { get; set; }

        [Display(Name = "LabelClientRef")]
        public bool ClientReference { get; set; }

        [Display(Name = "Other Reference Number")]
        public bool OtherReferenceNumber { get; set; }

        [Display(Name = "Reference Number")]
        public bool AgentReference { get; set; }

        [Display(Name = "Remarks")]
        public bool Remarks { get; set; }

        [Display(Name = "Upload Documents")]
        public bool UploadDocuments { get; set; }
    }

    public class ServiceAccount
    {
        private string? _userName;
        private string? _password;

        public string UserName 
        { 
            get { return string.IsNullOrEmpty(_userName) ? "Scheduler@cpiip.com" : _userName; }
            set { _userName = value; }
        }
        public string Password
        {
            get { return string.IsNullOrEmpty(_password) ? "CpiSched123$1" : _password; }
            set { _password = value; }
        }
    }

    public enum DocumentStorageAccountType
    {
        User,    //use delegated permission (authorization code flow) 
        Service  //for background services. use application permission.
                 //SharePoint: client credentials flow. must have Sites.Manage.All permission to upload docs
                 //iManage: password flow.
        //todo: ,ReadOnly //for users with no sp account. use application permission or delegated ropc using shared account without mfa. needs at least Site.Read.All 
    }

    public class DocVerificationReviewFilters
    {
        public string? CountryFilter { get; set; }
        public string? CaseTypeFilter { get; set; }
        public string? ClientFilter { get; set; }
        public bool IsActiveOnly { get; set; }
        public DateTime? UploadedDate { get; set; }        
        public string? DocNameFilter { get; set; }        
    }

    public class InventionDisclosureStatus
    {
        public bool Enabled { get; set; }
        public Dictionary<string, string> Colors { get; set; } = new Dictionary<string, string>();
    }
}
