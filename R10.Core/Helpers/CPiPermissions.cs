using R10.Core.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Core.Helpers
{
    public static class CPiPermissions
    {
        public static List<string> CpiDomains => new List<string>() { "computerpackages.com", "cpiip.com", "cpi.email" };
        public static List<string> FullModify => new List<string>() { "modify", "nodelete" };
        public static List<string> CanDelete => new List<string>() { "modify"};
        public static List<string> RemarksOnly => new List<string>() { "remarksonly" };
        public static List<string> LimitedRead => new List<string>() { "limited" };
        public static List<string> FullRead => new List<string>() { "modify", "readonly", "nodelete", "remarksonly", "dedocketer" };

        public static List<string> Reviewer => new List<string>() { "reviewer" };
        public static List<string> Previewer => new List<string>() { "previewer" };
        public static List<string> Inventor => new List<string>() { "inventor" };
        public static List<string> DecisionMaker => new List<string>() { "modify", "decisionmaker" };
        public static List<string> RegularUser => new List<string>() { "modify", "readonly", "nodelete", "remarksonly", "limited", "dedocketer", "costtrackingmodify" };
        public static List<string> CanAccessSystem => new List<string>() { "modify", "readonly", "nodelete", "remarksonly", "limited", "dedocketer", "costtrackingmodify" };
        public static List<string> CanReceiveInstructionsNotifications => new List<string>() { "modify" };

        public static List<string> Letters => new List<string>() { "lettermodify",  "letterreadonly" };
        public static List<string> LetterModify => new List<string>() { "lettermodify" };

        public static List<string> CustomQuery => new List<string>() { "customquerymodify", "customqueryreadonly" };
        public static List<string> CustomQueryModify => new List<string>() { "customquerymodify" };

        public static List<string> Auxiliary => new List<string>() { "auxiliarymodify", "auxiliaryreadonly", "auxiliaryremarksonly", "auxiliarylimited", "auxiliarynodelete" };
        public static List<string> AuxiliaryModify => new List<string>() { "auxiliarymodify", "auxiliarynodelete" };
        public static List<string> AuxiliaryRemarksOnly => new List<string>() { "auxiliaryremarksonly" };
        public static List<string> AuxiliaryLimited => new List<string>() { "auxiliarylimited" };
        public static List<string> AuxiliaryCanDelete => new List<string>() { "auxiliarymodify" };

        public static List<string> CanAccessReview => new List<string>() { "modify", "reviewer" };
        public static List<string> CanAccessPreview => new List<string>() { "modify", "previewer" };
        public static List<string> CanUploadDocuments => new List<string>() { "modify", "nodelete","upload", "inventor" };

        public static List<string> DMSDueDateList => new List<string>() { "modify", "readonly", "reviewer", "inventor" };

        public static List<string> CountryLaw => new List<string>() { "countrylawmodify", "countrylawreadonly", "countrylawremarksonly", "countrylawnodelete" };
        public static List<string> CountryLawModify => new List<string>() { "countrylawmodify", "countrylawnodelete" };
        public static List<string> CountryLawRemarksOnly => new List<string>() { "countrylawremarksonly" };
        public static List<string> CountryLawCanDelete => new List<string>() { "countrylawmodify" };

        public static List<string> ActionType => new List<string>() { "actiontypemodify", "actiontypereadonly", "actiontyperemarksonly", "actiontypenodelete" };
        public static List<string> ActionTypeModify => new List<string>() { "actiontypemodify", "actiontypenodelete" };
        public static List<string> ActionTypeRemarksOnly => new List<string>() { "actiontyperemarksonly" };
        public static List<string> ActionTypeCanDelete => new List<string>() { "actiontypemodify" };

        public static List<string> Products => new List<string>() { "productsmodify", "productsreadonly", "productsnodelete" }; //, "productsremarksonly" };
        public static List<string> ProductsModify => new List<string>() { "productsmodify", "productsnodelete" };
        public static List<string> ProductsRemarksOnly => new List<string>() { "productsremarksonly" };
        public static List<string> ProductsCanDelete => new List<string>() { "productsmodify" };

        public static List<string> CostEstimator => new List<string>() { "costestimatormodify", "costestimatorreadonly", "costestimatorremarksonly", "costestimatornodelete" };
        public static List<string> CostEstimatorModify => new List<string>() { "costestimatormodify", "costestimatornodelete" };
        public static List<string> CostEstimatorRemarksOnly => new List<string>() { "costestimatorremarksonly" };
        public static List<string> CostEstimatorCanDelete => new List<string>() { "costestimatormodify" };

        public static List<string> GermanRemuneration => new List<string>() { "remunerationmodify", "remunerationreadonly", "remunerationremarksonly", "remunerationnodelete" };
        public static List<string> GermanRemunerationModify => new List<string>() { "remunerationmodify", "remunerationnodelete" };
        public static List<string> GermanRemunerationRemarksOnly => new List<string>() { "remunerationremarksonly" };
        public static List<string> GermanRemunerationCanDelete => new List<string>() { "remunerationmodify" };

        public static List<string> FrenchRemuneration => new List<string>() { "frremunerationmodify", "frremunerationreadonly", "frremunerationremarksonly", "frremunerationnodelete" };
        public static List<string> FrenchRemunerationModify => new List<string>() { "frremunerationmodify", "frremunerationnodelete" };
        public static List<string> FrenchRemunerationRemarksOnly => new List<string>() { "frremunerationremarksonly" };
        public static List<string> FrenchRemunerationCanDelete => new List<string>() { "frremunerationmodify" };

        public static List<string> CanAddClearance => new List<string>() { "reviewer", "modify" };

        public static List<string> CanAddDisclosure => new List<string>() { "inventor", "modify" };

        public static List<string> CostTracking => new List<string>() { "modify", "readonly", "nodelete", "remarksonly", "costtrackingmodify", "dedocketer" };
        public static List<string> CostTrackingModify => new List<string>() { "modify", "nodelete", "costtrackingmodify" };
        public static List<string> CostTrackingDelete => new List<string>() { "modify", "costtrackingmodify" };
        public static List<string> CostTrackingUpload => new List<string>() { "modify", "nodelete", "upload", "costtrackingmodify" };
        public static List<string> CostTrackingAuxiliaryModify => new List<string>() { "costtrackingmodify" };

        public static List<string> DeDocketer => new List<string>() { "dedocketer" };

        public static List<string> PatentScore => new List<string>() { "patentscoremodify" };
        public static List<string> PatentScoreModify => new List<string>() { "modify", "nodelete", "patentscoremodify" };

        public static List<string> DocumentVerification => new List<string>() { "documentverificationmodify", "documentverificationreadonly" };
        public static List<string> DocumentVerificationModify => new List<string>() { "documentverificationmodify" };

        public static List<string> Workflow => new List<string>() { "workflowmodify", "workflowreadonly" };
        public static List<string> WorkflowModify => new List<string>() { "workflowmodify" };

        public static List<string> CanReceiveActionDelegationRoles => new List<string>() { "modify", "nodelete", "readonly" };
        public static List<CPiUserType> CanReceiveActionDelegationUsers => new List<CPiUserType> { CPiUserType.User, CPiUserType.DocketService };

        public static List<CPiUserType> CanHavePatentScoreRole => new List<CPiUserType> { CPiUserType.Attorney };

        // Attorney user type can add and modify their own soft docket without the need to have SoftDocket role
        public static List<CPiUserType> SoftDocketUsers => new List<CPiUserType> { CPiUserType.Attorney };
        public static List<CPiUserType> CanHaveSoftDocketRole => new List<CPiUserType> { CPiUserType.User };
        public static List<string> SoftDocket => new List<string>() { "softdocket" };

        public static List<CPiUserType> RequestDocketUsers => new List<CPiUserType> { CPiUserType.Attorney };
        public static List<CPiUserType> CanHaveRequestDocketRole => new List<CPiUserType> { CPiUserType.User };
        public static List<string> RequestDocket => new List<string>() { "requestdocket" };

        public static List<string> Upload => new List<string>() { "upload" };
        public static List<string> CanHaveUploadRole => new List<string>() { "readonly", "remarksonly" };

        public static List<CPiUserType> CanHaveDMSTSClearance => new List<CPiUserType> { CPiUserType.Administrator, CPiUserType.SuperAdministrator, CPiUserType.User, CPiUserType.Inventor, CPiUserType.ContactPerson };
        public static List<CPiUserType> CanHaveTSClearance => new List<CPiUserType> { CPiUserType.Administrator, CPiUserType.SuperAdministrator, CPiUserType.User };
        public static List<CPiUserType> CanAccessTSReports => new List<CPiUserType> { CPiUserType.Administrator, CPiUserType.SuperAdministrator };

        public static List<CPiUserType> InternalUsers => new List<CPiUserType> { CPiUserType.User };

        public static bool IsInternalUser(this CPiUserType userType)
        {
            return InternalUsers.Contains(userType);
        }
        public static bool IsRegularUser(this CPiUserType userType)
        {
            return (new List<CPiUserType>() { CPiUserType.User, CPiUserType.DocketService }).Contains(userType);
        }
        public static bool HasLinkedEntity(this CPiUserType userType)
        {
            return (new List<CPiUserType>() { CPiUserType.Inventor, CPiUserType.Attorney, CPiUserType.ContactPerson }).Contains(userType);
        }
        public static bool IsEndUser(this CPiUserType userType)
        {
            return (new List<CPiUserType>() { CPiUserType.ContactPerson, CPiUserType.Attorney }).Contains(userType);
        }
    }
}
