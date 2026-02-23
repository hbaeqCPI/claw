using R10.Core.DTOs;
using R10.Core.Queries.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using R10.Core.Entities.Documents;

namespace R10.Core.Interfaces
{
    public interface IQuickEmailRepository
    {
        IQueryable<QEDetailView> GetQuickEmails(string systemType);

        IQueryable<QEMain> GetQuickEmailById(int id);
        IQueryable<QEMain> GetQuickEmails(string systemType, string screenCode, string? templateName, int? qeCatId, List<string>? tags = null);
        //Task<QEDetailView> GetQuickEmailById(int id);

        Task<QEMain> GetDefaultQuickEmail(string systemType, string screenCode);

        Task<ModuleMain> GetModule(int id);
        Task<ModuleMain> GetModule(string moduleCode, string subModule);
        Task<SystemScreen> GetSystemScreen(int id);
        Task<SystemScreen> GetSystemScreen(string systemType, string screenCode);
        Task<QEDetailView> GetQuickEmailByName(string systemType, string templateName);

        IQueryable<QERecipient> GetRecipients(int id);

        Task<QEDataSource> GetDataSource(string systemType, string dataSourceName);

        Task<QEPatInventionView> GetPatInvention(int invId);
        Task<QEPatInventionAttyChangedView> GetPatInventionAttyChanged(int invId);
        Task<QEPatCountryApplicationView> GetPatCountryApplication(int appId);
        Task<QEPatInventorAppAwardView> GetPatInventorAppAward(int awardId);
        Task<QEPatInventorDMSAwardView> GetPatInventorDMSAward(int awardId);
        Task<QEPatIRLumpSumAwardView> GetPatIRLumpSumAward(int inventorInvId);
        Task<QEPatIRYearlyAwardView> GetPatIRYearlyAward(int inventorInvId);
        Task<QEPatIRDistributionAwardView> GetPatIRDistributionAward(int distributionId);
        Task<QEPatIRFRRemunerationAwardView> GetPatIRFRRemunerationAward(int inventorInvId);
        Task<QEPatCostTrackingView> GetPatCostTracking(int costTrackId);
        Task<QEPatCostTrackingInvView> GetPatCostTrackingInv(int costTrackInvId);
        Task<QEPatActionDueView> GetPatActionDue(int actId);
        Task<QEPatActionDueInvView> GetPatActionDueInv(int actId);
        Task<QEPatActionDueDateView> GetPatActionDueDate(int ddId);
        Task<QEPatActionDueDateInvView> GetPatActionDueDateInv(int ddId);
        Task<QEPatActionDueDateDedocketView> GetPatActionDueDateDedocket(int deDocketId);
        Task<QEPatActionDueDateInvDedocketView> GetPatActionDueDateInvDedocket(int deDocketInvId);
        Task<QEPatActionDueDateDelegationView> QEPatActionDueDateDelegation(int delegationId);
        Task<QEPatActionDueDateInvDelegationView> QEPatActionDueDateInvDelegation(int delegationId);
        Task<QEPatActionDueDateDelegationView> QEPatActionDueDateDeletedDelegation(int delegationId);
        Task<QEPatActionDueDateInvDelegationView> QEPatActionDueDateInvDeletedDelegation(int delegationId);
        Task<QEPatActionDueDateDelegationView> QEPatActionDueDateReassignedDelegation(int delegationId);
        Task<QEPatActionDueDateInvDelegationView> QEPatActionDueDateInvReassignedDelegation(int delegationId);

        Task<QEPatSearchView> GetPatSearchNotifyLog(int searchId);
        Task<QEPatCountryApplicationImageView> GetPatCountryApplicationImage(int docId);
        Task<QEPatActionImageView> GetPatActionImage(int docId);
        Task<QEPatActionInvImageView> GetPatActionInvImage(int docId);
        Task<QEPatActionDueDeletedView> GetPatActionDueDeleted(int actId);
        Task<QEPatActionDueInvDeletedView> GetPatActionDueInvDeleted(int actId);
        Task<QEPatCountryAppDeletedView> GetPatCountryAppDeleted(int appId);

        Task<QETmkTrademarkView> GetTmkTrademark(int tmkId);
        Task<QETmkCostTrackingView> GetTmkCostTracking(int costTrackId);
        Task<QETmkActionDueView> GetTmkActionDue(int actId);
        Task<QETmkActionDueDateView> GetTmkActionDueDate(int ddId);
        Task<QETmkActionDueDateDedocketView> GetTmkActionDueDateDedocket(int deDocketId);
        Task<QETmkConflictView> GetTmkConflict(int conflictId);
        Task<QETmkImageView> GetTmkImage(int docId);
        Task<QETmkActionImageView> GetTmkActionImage(int docId);
        Task<QETmkActionDueDeletedView> GetTmkActionDueDeleted(int actId);
        Task<QETmkTrademarkDeletedView> GetTmkTrademarkDeleted(int tmkId);
        Task<QETmkTrademarkAttyChangedView> GetTmkTrademarkAttyChanged(int tmkId);
        Task<QETmkActionDueDateDelegationView> QETmkActionDueDateDelegation(int delegationId);
        Task<QETmkActionDueDateDelegationView> QETmkActionDueDateDeletedDelegation(int delegationId);
        Task<QETmkActionDueDateDelegationView> QETmkActionDueDateReassignedDelegation(int delegationId);

        Task<QEGmMatterView> GetGmMatter(int matId);
        Task<QEGmCostTrackingView> GetGmCostTracking(int costTrackId);
        Task<QEGmActionDueView> GetGmActionDue(int actId);
        Task<QEGmActionDueDateView> GetGmActionDueDate(int ddId);
        Task<QEGmActionDueDateDedocketView> GetGmActionDueDateDedocket(int deDocketId);
        Task<QEGmActionDueDateDelegationView> QEGmActionDueDateDelegation(int delegationId);
        Task<QEGmImageView> GetGmImage(int docId);
        Task<QEGmActionImageView> GetGmActionImage(int docId);
        Task<QEGmActionDueDeletedView> GetGmActionDueDeleted(int actId);
        Task<QEGmMatterDeletedView> GetGmMatterDeleted(int matId);

        Task<QEDmsDisclosureView> GetDmsDisclosure(int dmsId);
        Task<QEDmsDisclosureReviewView> GetDmsDisclosureReview(int dmsId);
        Task<QEDmsAgendaView> GetDmsAgenda(int agendaId);
        Task<QEDmsActionDueView> GetDmsActionDue(int actId);
        Task<QEDmsActionDueDateView> GetDmsActionDueDate(int ddId);
        Task<QEDmsActionDueDateDelegationView> QEDmsActionDueDateDelegation(int delegationId);
        Task<QEGmActionDueDateDelegationView> QEGmActionDueDateDeletedDelegation(int delegationId);
        Task<QEGmActionDueDateDelegationView> QEGmActionDueDateReassignedDelegation(int delegationId);

        Task<QEDocVerificationImageView> GetDocVerificationImage(int docId);

        Task<QEPatRequestDocketView> GetPatRequestDocket(int reqId);
        Task<QETmkRequestDocketView> GetTmkRequestDocket(int reqId);
        Task<QEGmRequestDocketView> GetGmRequestDocket(int reqId);

        Task<string> GetRecipientEmail(string roleLink, QERoleSource roleSource);
        Task<Dictionary<string, string>> GetRecipientNameAndEmail(string roleLink, QERoleSource roleSource);

        Task<List<QERecipientRoleDTO>> GetRecipientRole(string roleLink, string sourceSql);

        IQueryable<QEImagesLinksDTO> GetImages(string imageTable, int parentId);
        IQueryable<DocDocument> GetDocDocuments();

        //IQueryable<QEImagesLinksDTO> GetPatInventionImages(int invId);
        //IQueryable<QEImagesLinksDTO> GetPatCountryApplicationImages(int appId);
        //IQueryable<QEImagesLinksDTO> GetPatCostTrackingImages(int costTrackId);
        //IQueryable<QEImagesLinksDTO> GetPatActionDueImages(int actId);

        //IQueryable<QEImagesLinksDTO> GetTmkTrademarkImages(int tmkId);
        //IQueryable<QEImagesLinksDTO> GetTmkCostTrackingImages(int costTrackId);
        //IQueryable<QEImagesLinksDTO> GetTmkActionDueImages(int actId);

        //IQueryable<QEImagesLinksDTO> GetGmMatterImages(int matId);
        //IQueryable<QEImagesLinksDTO> GetGmCostTrackingImages(int costTrackId);
        //IQueryable<QEImagesLinksDTO> GetGmActionDueImages(int actId);

        //IQueryable<QEImagesLinksDTO> GetDmsDisclosureImages(int dmsId);

        Task<QETmcClearanceView> GetTmcClearance(int tmcId);

        Task<QEPacClearanceView> GetPatClearance(int pacId);

        Task<List<QEFieldListDTO>> GetCustomFieldData(int dataSourceID, int parentId);
        
        Task<QEPatCountryApplicationDocRespDocketingView> GetPatCountryApplicationDocRespDocketing(int docId = 0, string driveItemId = "");
        Task<QEPatCountryApplicationDocRespDocketingView> GetPatCountryApplicationDocRespDocketingReassigned(int docId = 0, string driveItemId = "");                
        Task<QETmkTrademarkDocRespDocketingView> GetTmkTrademarkDocRespDocketing(int docId = 0, string driveItemId = "");
        Task<QETmkTrademarkDocRespDocketingView> GetTmkTrademarkDocRespDocketingReassigned(int docId = 0, string driveItemId = "");        
        Task<QEGmMatterDocRespDocketingView> GetGmMatterDocRespDocketing(int docId = 0, string driveItemId = "");
        Task<QEGmMatterDocRespDocketingView> GetGmMatterDocRespDocketingReassigned(int docId = 0, string driveItemId = "");     
        
        Task<QEPatCountryApplicationDocRespReportingView> GetPatCountryApplicationDocRespReporting(int docId = 0, string driveItemId = "");
        Task<QEPatCountryApplicationDocRespReportingView> GetPatCountryApplicationDocRespReportingReassigned(int docId = 0, string driveItemId = "");                
        Task<QETmkTrademarkDocRespReportingView> GetTmkTrademarkDocRespReporting(int docId = 0, string driveItemId = "");
        Task<QETmkTrademarkDocRespReportingView> GetTmkTrademarkDocRespReportingReassigned(int docId = 0, string driveItemId = "");        
        Task<QEGmMatterDocRespReportingView> GetGmMatterDocRespReporting(int docId = 0, string driveItemId = "");
        Task<QEGmMatterDocRespReportingView> GetGmMatterDocRespReportingReassigned(int docId = 0, string driveItemId = "");
    }
}
