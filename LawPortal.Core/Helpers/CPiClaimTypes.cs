using System;
using System.Collections.Generic;
using System.Text;

namespace LawPortal.Core.Helpers
{
    public static class CPiClaimTypes
    {
        public const string FirstName = "CPi.FirstName";
        public const string LastName = "CPi.LastName";
        public const string Locale = "CPi.Locale";
        public const string UserType = "CPi.UserType";
        public const string System = "CPi.System";
        public const string RespOffice = "CPi.RespOffice";
        public const string EntityFilterType = "CPi.EntityFilterType";
        public const string DefaultPage = "CPi.DefaultPage";
        public const string Theme = "CPi.Theme";
        public const string SystemStatus = "CPi.SystemStatus";
        public const string StatusMessage = "CPi.StatusMessage";
        public const string Modules = "CPi.Modules";
        public const string SearchResultsPageSize = "CPi.SearchResultsPageSize";
        public const string RestrictExportControl = "CPi.RestrictExportControl";
        public const string ClientType = "CPi.ClientType";
        public const string TwoFactorRequired = "CPi.TwoFactorRequired";
        public const string SSORequired = "CPi.SSORequired";
        public const string UseOutlookAddIn = "CPi.UseOutlookAddIn";
        public const string DashboardAccess = "CPi.DashboardAccess";
        public const string Mailbox = "CPi.Mailbox";
        public const string DocumentStorageAccountType = "CPi.DocumentStorageAccountType";
        public const string DocumentStorage = "CPi.DocumentStorage";
        public const string TradeSecret = "CPi.TradeSecret";
        public const string TradeSecretReports = "CPi.TradeSecretReports";
        public const string EntityId = "CPi.EntityId";
    }

    public enum CPiModule
    {
        PatAudit,
        TmkAudit,
        GMAudit,
        PatDeDocket,
        TmkDeDocket,
        GMDeDocket,
        PatPortfolioOnboarding,
        TmkPortfolioOnboarding,
        GMPortfolioOnboarding,
        InventorAward,
        TrademarkLinks,
        RTS,
        PatCustomReport,
        TmkCustomReport,
        GMCustomReport,
        PatProducts,
        TmkProducts,
        GMProducts,
        AMSProducts,
        DMSAudit,
        CustomField,
        PatCostEstimator,
        PatDocumentVerification,
        TmkDocumentVerification,
        GMDocumentVerification,
        TmkCostEstimator,
        DMSPreview,
        AMSDecisionManagement,
        IDSImport,
        PowerBIConnector,
        GermanRemuneration,
        FrenchRemuneration,
        FFAudit,
        PacAudit,
        AMSAudit,
        RMSAudit,
        TmcAudit
    }
}
