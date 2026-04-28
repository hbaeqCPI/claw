using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace LawPortal.Core.Entities.Shared
{
    public class DefaultSetting
    {
        public bool IsCorporation { get; set; }     //Client type is CORPORATION
        public string? ClientName { get; set; }

        public bool EnableComboBoxPaging { get; set; }
        public int ComboBoxPagingSize { get; set; }

        public string? LetterTemplateFolder { get; set; }

        public bool IsRestrictPrivateDocAccessOn { get; set; }

        public string? ImageFolder { get; set; }
        public string[] ValidImageFileExtensions { get; set; } = "txt|jpg|jpeg|doc|docx|rtf|xls|xlsx|pdf|gif|png|bmp|msg|eml|emlx|vsd|vsdx|dxf|dwg|ppt|pptx|mpp|mpt".Split('|');

        public string? LabelCaseNumber { get; set; }
        public string? LabelOldCaseNumber { get; set; }
        public string? LabelClient { get; set; }
        public string? LabelClientName { get; set; }
        public string? LabelClientRef { get; set; }
        public string? LabelAgent { get; set; }
        public string? LabelAgentName { get; set; }
        public string? LabelAgentRef { get; set; }
        public string? LabelOwner { get; set; }
        public string? LabelOwnerName { get; set; }
        public string? LabelDisclosureNumber { get; set; }
        public string? LabelKeyword { get; set; }
        public string? LabelClientMatter { get; set; }
        public string? LabelAttorney { get; set; }
        public string? LabelAttorney1 { get; set; }
        public string? LabelAttorney2 { get; set; }
        public string? LabelAttorney3 { get; set; }
        public string? LabelAttorney4 { get; set; }
        public string? LabelAttorney5 { get; set; }

        [Display(Description = "Map", GroupName = "Modules")]
        public bool IsMapOn { get; set; }

        public bool IsClientMatterOn { get; set; }

        [Display(Description = "DeDocket", GroupName = "Modules")]
        public bool IsDeDocketOn { get; set; }

        [Display(Description = "Portfolio Onboarding", GroupName = "Modules")]
        public bool IsPortfolioOnboardingOn { get; set; }

        [Display(Description = "Notifications", GroupName = "Modules")]
        public bool IsNotificationOn { get; set; }

        public bool CanResponseDateTriggerRecurring { get; set; }

        public int DQWhereColumnCount { get; set; }
        public string? DQExportImageExtensions { get; set; }
        public int DQExportImageSize { get; set; }
        public string? DQExportableImages { get; set; }

        public int SearchResultsPageSize { get; set; } = 25;
        public int SubFormPageSize { get; set; } = 10;
        public int MultipleEntitiesPageSize { get; set; } = 5;

        public int[] SearchResultsPageSizeOptions { get; set; } = new[] { 10, 25, 50, 100 };
        public int[] SubFormPageSizeOptions { get; set; } = new[] { 5, 10, 20, 50 };
        public int[] MultipleEntitiesPageSizeOptions { get; set; } = new[] { 5, 10, 20 };

        public int FileUploadSizeLimit { get; set; }

        [Display(Description = "Report Scheduler", GroupName = "Modules")]
        public bool IsReportSchedulerON { get; set; }
        public string? ReportDateFormat { get; set; }
        public string? ReportDateTimeFormat { get; set; }
        public string? ReportCurrencyFormat { get; set; }
        public string? ReportExcelRecordDelimiter { get; set; }
        public string? ReportExcelFieldDelimiter { get; set; }
        public string? ReportDetailFontSize { get; set; }
        public string? ReportCriteriaFontSize { get; set; }
        public bool ReportHideSubFormLabel { get; set; }
        public string? ReportHeaderShadingColor { get; set; }
        public string? ReportMainInfoShadingColor { get; set; }
        public string? ReportSubReportsHeaderShadingColor { get; set; }
        public string? ReportCriteriaHeaderShadingColor { get; set; }
        public bool ReportMultiCurrencyOn { get; set; }
        public bool ReportPatentWatchOn { get; set; }


        public int MaxApiPageSize { get; set; } = 100;

        public int DocViewerHeight { get; set; } = 1000;
        public int DocViewerWidth { get; set; } = 1000;
        public string? ViewableDocs { get; set; }


        // Email add-in
        public string? EmailDocumentFolder { get; set; }
        //public string? EmailSavePath { get; set; }
        public string? OutlookApiBaseUrl { get; set; }
        public bool IsOutlookToCPiOn { get; set; }
        public string? AttorneyRole { get; set; } = "RemarksOnly";
        public string? InventorRole { get; set; } = "ReadOnly";

        //storage  (now in appsettings.config)       
        //public string? StorageAccountName { get; set; }
        //public string? StorageUrl { get; set; }
        //public string? StorageContainerName { get; set; }
        //public string? StorageConnectionString { get; set; }             // data source/connstring expr used in cognitive search indexer creation

        public string? WidgetCurrencyFormat { get; set; }

        public bool IsGenFollowUpOn { get; set; }
        public int FollowUpActionTermMon { get; set; }
        public int FollowUpActionTermDay { get; set; }
        public string FollowUpActionIndicator { get; set; }


        // RS Options
        public string? RSCTMTaskCode { get; set; }
        public string? DECTMTaskCode { get; set; }
        public string? RSCTMClientName { get; set; }
        public string? RSIncludedSystems { get; set; }
        public int RSMaxRecordCount { get; set; } //Max record count for Report Scheduler
        public int RSMaxScheduleCount { get; set; } //Max schedule count for Report Scheduler

        //Custom Report Options
        [Display(Description = "Custom Report", GroupName = "Modules")]
        public bool IsCustomReportON { get; set; }
        public string? CustomReportAPIKey { get; set; }
        public string? CRReportBuilderDownloadUrl { get; set; }

        // Azure Global Search  (now in appsettings.config)       
        //public string? GlobalSearchUrl {get; set;}
        //public string? GlobalSearchServiceName {get; set; }
        //public string? GlobalSearchIndexName {get; set; }
        //public string? GlobalSearchIndexerName {get; set; }
        //public string? GlobalSearchSkillsetName { get; set; }
        //public int GlobalSearchIndexerExecDays { get; set; }
        //public int GlobalSearchIndexerExecHours { get; set; }
        //public int GlobalSearchIndexerExecMinutes { get; set; }
        

        // Azure Form Recognizer
        public string? FormRecognizerUrl { get; set; }
        public string? FormRecognizerServiceName { get; set; }
        public int FormRecognizerActionDayRange { get; set; }
        public bool FormRecognizerShowActionTab { get; set; }
        public bool FormRecognizerShowOtherInfoTab { get; set; }

        [Display(Description = "Custom Fields", GroupName = "Modules")]
        public bool IsShowCustomFieldOn { get; set; }

        [Display(Description = "Audit Trail", GroupName = "Modules")]
        public bool IsAuditOn { get; set; }

        public string? FiscalCalendarYearStart { get; set; }
        public string? FiscalCalendarYearEnd { get; set; }

        //NOTIFICATION EMAIL SETUP NAMES
        public string? NewPasswordNotification { get; set; } = "New User Account";
        public string? TemporaryPasswordNotification { get; set; } = "New User Account with Temporary Password";
        public string? ResetPasswordLinkNotification { get; set; } = "Reset Password Link";
        public string? NeedsConfirmationNotification { get; set; } = "Reset Password with Unconfirmed Email";
        public string? ConfirmEmailLinkNotification { get; set; } = "Email Confirmation Link";
        public string? PendingRegistrationNotification { get; set; } = "Pending User Registration";
        public string? AccountApprovalNotification { get; set; } = "User Account Approval";
        public string? TradeSecretAccessCodeNotification { get; set; } = "Trade Secret Access Code Notification";
        public string? TradeSecretRequestNotification { get; set; } = "Trade Secret Request Notification";
        public string? TaskSchedulerNotification { get; set; } = "Task Scheduler Notification";

        public bool DisableVideoBackground { get; set; }

        [Display(Description = "Time Tracker", GroupName = "Modules")]
        public bool IsTimeTrackerOn { get; set; }
        public decimal TimeTrackMinHours { get; set; }
        public decimal TimeTrackMaxHours { get; set; }

        [Display(Description = "Export Control", GroupName = "Modules")]
        public bool IsExportControlOn { get; set; }

        //login message api
        public string? LoginMessageApiUrl { get; set; }
        public string? LoginMessage { get; set; }

        //system notifications api
        public string? NotificationsApiUrl { get; set; }

        //Quick Email
        public string? FirmContact { get; set; }

        public string? ExchangeRateUrl { get; set; }
        public string? GoogleApiURL { get; set; }

        //Delegation
        [Display(Description = "Delegation", GroupName = "Modules")]
        public bool IsDelegationOn { get; set; }
        public bool IsDelegationReportOn { get; set; }
        public string? DelegationReportSendingTime { get; set; }
        public int DelegationReportFormat { get; set; }
        public bool DelegationReportPrintActionDueRemarks { get; set; }
        public bool DelegationReportPrintDueDateRemarks { get; set; }
        public bool DelegationReportPrintRemarks { get; set; }
        public string? DelegationReportPrintGoods { get; set; }
        public bool DelegationReportPrintImage { get; set; }
        public bool DelegationReportPrintImageDetail { get; set; }
        public bool DelegationReportPrintInventors { get; set; }
        public string? DelegationReportActiveSwitch { get; set; }
        public bool DelegationReportPrintDeDocketInstruction { get; set; }
        public string? DelegationReportPrintSystems { get; set; }
        public int DelegationStartDateAdjustment { get; set; }
        public int DelegationReminderDateAdjustment { get; set; }

        //Shared settings
        [Display(Description = "Form Generation", GroupName = "Modules")]
        public bool IsEFSOn { get; set; } //General

        [Display(Description = "Workflow", GroupName = "Modules")]
        public bool IsWorkflowOn { get; set; } //PMS/TMS/GMS

        [Display(Description = "Products", GroupName = "Modules")]
        public bool IsProductsOn { get; set; } //AMS/PMS/TMS/GMS

        [Display(Description = "Licenses", GroupName = "Modules")]
        public bool IsLicenseesOn { get; set; } //AMS/PMS/TMS

        //[Display(Description = "Storage", GroupName = "Modules")]
        public bool IsStorageOn { get; set; } //PMS/TMS

        [Display(Description = "Budget Management", GroupName = "Modules")]
        public bool IsBudgetManagementOn { get; set; } //PMS/TMS/GMS
        public decimal BudgetManagementIncreaseRate { get; set; } //PMS/TMS/GMS

        public bool ShowAttorneyNotificationsManager { get; set; }

        public bool ShowReminderCorrespondence { get; set; }

        public bool ShowWeblinksInExportToExcel { get; set; }
        
        public bool ShowSearchButtonInMainScreens { get; set; }
        public bool IsCognitiveSearchOn { get; set; }
        
        public string? ColorScheme { get; set; }

        [Display(Description = "Document Verification", GroupName = "Modules")]
        public bool IsDocumentVerificationOn { get; set; } //PMS/TMS/GMS        

        public string? DocVerificationDefaultFolderName { get; set; }

        public bool IncludeDeDocketInVerification { get; set; }
        
        public bool IsMyFavoriteOn { get; set; }

        //Use DocumentStorage setting to enable SharePoint
        //[Display(Description = "SharePoint Integration", GroupName = "Modules")]
        //public bool IsSharePointIntegrationOn { get; set; }
        public bool IsSharePointIntegrationOn => DocumentStorage == DocumentStorageOptions.SharePoint;
        //public bool IsAdvancedSharePointIntegrationOn { get; set; }
        //public bool IsSharePointDirectLogin { get; set; }
        public bool IsSharePointIntegrationByMetadataOn { get; set; }
        public string? SharePointInvalidCharacters { get; set; }
        public bool IsSharePointLoggingOn { get; set; } //for QE log, Letters log, IP Forms log
        public bool IsSharePointListRealTime { get; set; }
        public bool IsSharePointIntegrationKeyFieldsOnly { get; set; }
        public bool IsSharePointCopyDocsOnDesignation { get; set; }
        public bool IsSharePointCascadeKeyChanges { get; set; }

        //Allow each country application record to have own root doc folder instead of using parent invention root folder
        public bool IsCountryApplicationDocumentRoot { get; set; }


        [Display(Description = "eSignature", GroupName = "Modules")]
        public bool IsESignatureOn {get; set; }

        public string? ReleaseDate { get; set; }
        public string? ReleaseVersion { get; set; }

        [Display(Description = "Power BI Custom Connector", GroupName = "Modules")]
        public bool IsPowerBIConnectorOn { get; set; }
        public bool IsESignatureReviewOn { get; set; }
        public bool IsDocumentUploadAIOn { get; set; }
        
        //API ENDPOINTS FOR PUSHING DATA
        public bool IsPushAPIOn { get; set; }

        public string? AuxCustomFieldsTabLabel { get; set; }

        public string? LabelTaxAgent { get; set; }
        public string? LabelTaxAgentName { get; set; }
        public string? LabelLegalRepresentative { get; set; }
        public string? LabelLegalRepresentativeName { get; set; }

        [Display(Description = "Document Storage", GroupName = "Modules")]
        public DocumentStorageOptions DocumentStorage { get; set; }

        public string? DocuSignWebhookUrl { get; set; }

        public string? QDRemarksSource { get; set; }

        public string? AdminEmail { get; set; }

        private bool _isSoftDocketOn = false;
        public bool IsSoftDocketOn 
        {
            get => _isSoftDocketOn && !IsCorporation;
            set => _isSoftDocketOn = value;
        }
        
        private bool _isDocketRequestOn = false;
        public bool IsDocketRequestOn
        {
            get => _isDocketRequestOn && !IsCorporation;
            set => _isDocketRequestOn = value;
        }

        public bool IsTrademarkWatchOn { get; set; }

        public string? SoftDocketDefaultResponsibleAtty { get; set; }
        public string? DocketRequestDefaultResponsibleAtty { get; set; }

        public string? DefaultCurrency { get; set; }
        
    }

    public enum DocumentStorageOptions
    {
        [Display(Name = "Azure Blob Storage or File System")]
        BlobOrFileSystem,
        [Display(Name = "SharePoint Integration")]
        SharePoint,
        [Display(Name = "iManage Integration")]
        iManage,
        [Display(Name = "NetDocuments Integration")]
        NetDocuments
    }
}
