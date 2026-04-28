using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace LawPortal.Web.Models
{
    public enum ScreenName
    {
        [Display(Name = "Invention")]
        PatInvention,

        [Display(Name = "Invention Attorney Changed")]
        PatInventionAttyChanged,

        [Display(Name = "CountryApplication")]
        PatCountryApplication,

        [Display(Name = "Cost Tracking")]
        PatCostTracking,

        [Display(Name = "Action Due")]
        PatActionDue,
                
        [Display(Name = "Country App Deleted")]
        PatCountryAppDeleted,

        [Display(Name = "Action Due Deleted")]
        PatActionDueDeleted,              

        [Display(Name = "Inventor App Award")]
        PatInventorAppAward,

        [Display(Name = "Inventor DMS Award")]
        PatInventorDMSAward,

        [Display(Name = "Lump Sum Award")]
        PatRemunerationLumpSumAward,

        [Display(Name = "Yearly Award")]
        PatRemunerationYearlyAward,

        [Display(Name = "Distribution Award")]
        PatRemunerationDistributionAward,

        [Display(Name = "French Remuneration")]
        PatIRFRRemuneration,

        [Display(Name = "Trademark")]
        TmkTrademark,

        [Display(Name = "Trademark Deleted")]
        TmkTrademarkDeleted,

        [Display(Name = "Cost Tracking")]
        TmkCostTracking,

        [Display(Name = "Action Due")]
        TmkActionDue,

        [Display(Name = "Action Due Deleted")]
        TmkActionDueDeleted,

        [Display(Name = "Conflict")]
        TmkConflict,

        [Display(Name = "General Matter")]
        GmMatter,

        [Display(Name = "General Matter Deleted")]
        GmMatterDeleted,

        [Display(Name = "Cost Tracking")]
        GmCostTracking,

        [Display(Name = "Action Due")]
        GmActionDue,

        [Display(Name = "Action Due Deleted")]
        GmActionDueDeleted,

        [Display(Name = "Invention Disclosure")]
        DmsDisclosure,

        [Display(Name = "Disclosure Review")]
        DmsReview,

        [Display(Name = "Disclosure Submission")]
        DmsSubmit,

        [Display(Name = "Action Due")]
        DmsActionDue,

        [Display(Name = "AMS Main")]
        AmsMain,

        [Display(Name = "AMS Due")]
        AmsDue,

        [Display(Name = "Clearance")]
        TmcClearance,

        [Display(Name = "Action Due Date")]
        PatActionDueDate,

        [Display(Name = "Due Date Dedocket")]
        PatActionDueDateDedocket,

        [Display(Name = "Action Delegation")]
        PatActionDelegation,

        [Display(Name = "Deleted Action Delegation")]
        PatActionDeletedDelegation,

        [Display(Name = "Reassigned Action Delegation")]
        PatActionReassignedDelegation,

        [Display(Name = "Action Due Date")]
        TmkActionDueDate,

        [Display(Name = "Due Date Dedocket")]
        TmkActionDueDateDedocket,

        [Display(Name = "Action Due Date")]
        GmActionDueDate,
        
        [Display(Name = "Due Date Dedocket")]
        GmActionDueDateDedocket,

        [Display(Name = "Action Delegation")]
        GmActionDelegation,

        [Display(Name = "Deleted Action Delegation")]
        GmActionDeletedDelegation,

        [Display(Name = "Reassigned Action Delegation")]
        GmActionReassignedDelegation,

        [Display(Name = "Action Due Date")]
        DmsActionDueDate,
        
        [Display(Name = "Action Delegation")]
        DmsActionDelegation,

        [Display(Name = "Patent Clearance Search")]
        PatClearance,

        [Display(Name = "Patent Search")]
        PatSearch,

        [Display(Name = "Application Images")]
        PatImagesApp,
        [Display(Name = "Patent Action Images")]
        PatImagesAct,
        
        [Display(Name = "Trademark Images")]
        TmkImages,
        [Display(Name = "Trademark Action Images")]
        TmkImagesAct,

        [Display(Name = "Trademark Attorney Changed")]
        TmkTrademarktAttyChanged,

        [Display(Name = "Action Delegation")]
        TmkActionDelegation,

        [Display(Name = "Deleted Action Delegation")]
        TmkActionDeletedDelegation,

        [Display(Name = "Reassigned Action Delegation")]
        TmkActionReassignedDelegation,

        [Display(Name = "GM Images")]
        GMImages,
        [Display(Name = "GM Action Images")]
        GMImagesAct,
        
        [Display(Name = "PatInventor")]
        PatInventor,

        [Display(Name = "Invention Cost Tracking")]
        PatCostTrackingInv,

        [Display(Name = "Patent Document Responsible Docketing Assigned")]
        PatDocRespDocketingAssigned,

        [Display(Name = "Patent Reassigned Document Responsible Docketing")]
        PatDocRespDocketingReassigned,

        [Display(Name = "Trademark Document Responsible Docketing Assigned")]
        TmkDocRespDocketingAssigned,

        [Display(Name = "Trademark Reassigned Document Responsible Docketing")]
        TmkDocRespDocketingReassigned,

        [Display(Name = "GM Document Responsible Docketing Assigned")]
        GmDocRespDocketingAssigned,

        [Display(Name = "GM Reassigned Document Responsible Docketing")]
        GmDocRespDocketingReassigned,

        [Display(Name = "Invention Action Due")]
        PatActionDueInv,

        [Display(Name = "Invention Action Due Deleted")]
        PatActionDueInvDeleted,

        [Display(Name = "Invention Action Due Date")]
        PatActionDueDateInv,

        [Display(Name = "Invention Due Date Dedocket")]
        PatActionDueDateInvDedocket,

        [Display(Name = "Invention Action Delegation")]
        PatActionInvDelegation,

        [Display(Name = "Patent Invention Action Images")]
        PatImagesActInv,

        [Display(Name = "Reassigned Invention Action Delegation")]
        PatActionInvReassignedDelegation,

        [Display(Name = "Deleted Invention Action Delegation")]
        PatActionInvDeletedDelegation,

        [Display(Name = "Disclosure Meeting Agenda")]
        DmsAgenda,

        [Display(Name = "Document Verification")]
        SharedDocVerification,

        [Display(Name = "Patent Document Responsible Reporting Assigned")]
        PatDocRespReportingAssigned,

        [Display(Name = "Patent Reassigned Document Responsible Reporting")]
        PatDocRespReportingReassigned,

        [Display(Name = "Trademark Document Responsible Reporting Assigned")]
        TmkDocRespReportingAssigned,

        [Display(Name = "Trademark Reassigned Document Responsible Reporting")]
        TmkDocRespReportingReassigned,

        [Display(Name = "GM Document Responsible Reporting Assigned")]
        GmDocRespReportingAssigned,

        [Display(Name = "GM Reassigned Document Responsible Reporting")]
        GmDocRespReportingReassigned,

        [Display(Name = "Request Docket")]
        PatRequestDocket,

        [Display(Name = "Request Docket")]
        TmkRequestDocket,

        [Display(Name = "Request Docket")]
        GmRequestDocket,

        NotSet
    }
}
