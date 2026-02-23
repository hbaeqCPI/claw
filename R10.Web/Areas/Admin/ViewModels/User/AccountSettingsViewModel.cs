using R10.Core.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Admin.ViewModels
{
    public class AccountSettingsViewModel
    {
        public string UserId { get; set; }
        public UserAccountSettings Settings { get; set; }
        public UserNotificationSettings NotificationSettings { get; set; }
        public CPiUserType UserType { get; set; }

        public int EntityId { get; set; }
        public string EntityName { get; set; }

        public bool IsAdmin { get; set; }

        public bool IsReviewer { get; set; }
        public bool IsPreviewer { get; set; }

        public bool IsClearanceReviewer { get; set; }
        public bool HasClearance { get; set; }
        public string ClearanceAuxiliaryRole { get; set; }

        public bool IsPatClearanceReviewer { get; set; }
        public bool HasPatClearance { get; set; }
        public string PatClearanceAuxiliaryRole { get; set; }

        public bool IsAMSDecisionMaker { get; set; }
        public bool IsRMSDecisionMaker { get; set; }
        public bool IsForeignFilingDecisionMaker { get; set; }

        public bool CanReceiveAMSNotifications { get; set; }
        public bool CanReceiveRMSNotifications { get; set; }
        public bool CanReceiveFFNotifications { get; set; }
        public bool CanReceiveDeDocketNotifications { get; set; }

        public bool HasAMS { get; set; }
        public bool HasRMS { get; set; }
        public bool HasForeignFiling { get; set; }
        public bool HasDMS { get; set; }
        public bool HasPatent { get; set; }
        public bool HasTrademark { get; set; }
        public bool HasGeneralMatter { get; set; }

        public string PatentLetterRole { get; set; }
        public string TrademarkLetterRole { get; set; }
        public string GeneralMatterLetterRole { get; set; }

        public string PatentCustomQueryRole { get; set; }
        public string TrademarkCustomQueryRole { get; set; }
        public string GeneralMatterCustomQueryRole { get; set; }
        public string AMSCustomQueryRole { get; set; }

        public string PatentProductsRole { get; set; }
        public string TrademarkProductsRole { get; set; }
        public string GeneralMatterProductsRole { get; set; }
        public string AMSProductsRole { get; set; }

        public string AMSAuxiliaryRole { get; set; }
        public string RMSAuxiliaryRole { get; set; }
        public string ForeignFilingAuxiliaryRole { get; set; }
        public string DMSAuxiliaryRole { get; set; }
        public string PatentAuxiliaryRole { get; set; }
        public string TrademarkAuxiliaryRole { get; set; }
        public string GeneralMatterAuxiliaryRole { get; set; }

        public string PatentCountryLawRole { get; set; }
        public string TrademarkCountryLawRole { get; set; }
        public string GeneralMatterCountryLawRole { get; set; }

        public string PatentActionTypeRole { get; set; }
        public string TrademarkActionTypeRole { get; set; }
        public string GeneralMatterActionTypeRole { get; set; }

        public bool CanUploadPatent { get; set; }
        public bool CanUploadTrademark { get; set; }
        public bool CanUploadGeneralMatter { get; set; }

        public string PatentCostEstimatorRole { get; set; }
        public string TrademarkCostEstimatorRole { get; set; }

        public string PatentGermanRemunerationRole { get; set; }
        public string PatentFrenchRemunerationRole { get; set; }

        public bool IsPatentModify { get; set; }
        public bool IsTrademarkModify { get; set; }
        public bool IsGeneralMattersModify { get; set; }

        public bool IsPatentScoreModify { get; set; }

        public string PatentDocumentVerificationRole { get; set; }
        public string TrademarkDocumentVerificationRole { get; set; }
        public string GeneralMatterDocumentVerificationRole { get; set; }

        public string PatentWorkflowRole { get; set; }
        public string TrademarkWorkflowRole { get; set; }
        public string GeneralMatterWorkflowRole { get; set; }
        public string DMSWorkflowRole { get; set; }
        public string PatClearanceWorkflowRole { get; set; }
        public string ClearanceWorkflowRole { get; set; }

        public bool IsPatentSoftDocket { get; set; }
        public bool IsTrademarkSoftDocket { get; set; }
        public bool IsGeneralMatterSoftDocket { get; set; }

        public bool IsPatentRequestDocket { get; set; }
        public bool IsTrademarkRequestDocket { get; set; }
        public bool IsGeneralMatterRequestDocket { get; set; }

        public bool CanHavePatentUploadRole { get; set; }
        public bool CanHaveTrademarkUploadRole { get; set; }
        public bool CanHaveGeneralMatterUploadRole { get; set; }
    }
}
