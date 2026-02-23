using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Http;
using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Entities.Shared;
using R10.Core.Identity;
using R10.Core.Queries.Shared;
using R10.Web.Areas;
using R10.Web.Areas.Shared.Controllers;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;
using R10.Web.Models;

namespace R10.Web.Interfaces
{
    public interface IOuickEmailViewModelService
    {
        #region Quick Email Setup

        IQueryable<QEDetailView> GetQuickEmails(string systemType);
        Task<QuickEmailSetupDetailViewModel> GetQuickEmailSetupById(int id);
        Task<QEDetailView> GetQuickEmailByName(string systemType, string templateName);
        //Task<List<QuickEmailSetupRecipientViewModel>> GetRecipients(int id);
        IQueryable<QERecipient> GetRecipients(int id);
        Task<QEDataSource> GetDataSource(string systemType, string dataSourceName);

        #endregion

        #region Quick Email

        Task<ModuleMain> GetModule(int id);
        Task<ModuleMain> GetModule(string moduleCode, string subModule);

        Task<string> GetScreenName(int screenId);
        Task<string> GetScreenName(string systemType, string screenCode);
        Task<SystemScreen> GetSystemScreen(int screenId);
        Task<SystemScreen> GetSystemScreen(string systemType, string screenCode);
        Task<QuickEmailDetailViewModel> GetDefaultOrFirstQuickEmail(string systemType, string screenCode);
        Task<QuickEmailDetailViewModel> GetQuickEmailById(int id);

        Task<QEPatInventionView> GetPatInvention(ParentDataStrategyParam param);
        Task<QEPatInventionAttyChangedView> GetPatInventionAttyChanged(int invId);
        Task<QEPatCountryApplicationView> GetPatCountryApplication(ParentDataStrategyParam param);
        Task<QEPatInventorAppAwardView> GetPatInventorAppAward(int awardId);
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
        Task<QEPatCountryApplicationImageView> GetPatCountryApplicationImage(ParentDataStrategyParam param);
        Task<QEPatActionImageView> GetPatActionImage(ParentDataStrategyParam param);
        Task<QEPatActionInvImageView> GetPatActionInvImage(ParentDataStrategyParam param);
        Task<QEPatActionDueDeletedView> GetPatActionDueDeleted(int actId);
        Task<QEPatActionDueInvDeletedView> GetPatActionDueInvDeleted(int actId);
        Task<QEPatCountryAppDeletedView> GetPatCountryAppDeleted(int appId);

        Task<QEPacClearanceView> GetPatClearance(int pacId);

        Task<QETmkTrademarkView> GetTmkTrademark(ParentDataStrategyParam param);
        Task<QETmkCostTrackingView> GetTmkCostTracking(int costTrackId);
        Task<QETmkActionDueView> GetTmkActionDue(int actId);
        Task<QETmkActionDueDateView> GetTmkActionDueDate(int ddId);
        Task<QETmkActionDueDateDedocketView> GetTmkActionDueDateDedocket(int deDocketId);
        Task<QETmkConflictView> GetTmkConflict(int conflictId);
        Task<QETmkImageView> GetTmkImage(ParentDataStrategyParam param);
        Task<QETmkActionImageView> GetTmkActionImage(ParentDataStrategyParam param);
        Task<QETmkActionDueDeletedView> GetTmkActionDueDeleted(int actId);
        Task<QETmkTrademarkDeletedView> GetTmkTrademarkDeleted(int tmkId);
        Task<QETmkTrademarkAttyChangedView> GetTmkTrademarkAttyChanged(int tmkId);
        Task<QETmcClearanceView> GetTmcClearance(int tmcId);
        Task<QETmkActionDueDateDelegationView> QETmkActionDueDateDelegation(int delegationId);
        Task<QETmkActionDueDateDelegationView> QETmkActionDueDateDeletedDelegation(int delegationId);
        Task<QETmkActionDueDateDelegationView> QETmkActionDueDateReassignedDelegation(int delegationId);

        Task<QEGmMatterView> GetGmMatter(ParentDataStrategyParam param);
        Task<QEGmCostTrackingView> GetGmCostTracking(int costTrackId);
        Task<QEGmActionDueView> GetGmActionDue(int actId);
        Task<QEGmActionDueDateView> GetGmActionDueDate(int ddId);
        Task<QEGmActionDueDateDedocketView> GetGmActionDueDateDedocket(int deDocketId);
        Task<QEGmImageView> GetGmImage(ParentDataStrategyParam param);
        Task<QEGmActionImageView> GetGmActionImage(ParentDataStrategyParam param);
        Task<QEGmActionDueDeletedView> GetGmActionDueDeleted(int actId);
        Task<QEGmMatterDeletedView> GetGmMatterDeleted(int matId);
        Task<QEGmActionDueDateDelegationView> QEGmActionDueDateDelegation(int delegationId);
        Task<QEGmActionDueDateDelegationView> QEGmActionDueDateDeletedDelegation(int delegationId);
        Task<QEGmActionDueDateDelegationView> QEGmActionDueDateReassignedDelegation(int delegationId);

        Task<QEPatRequestDocketView> GetPatRequestDocket(int reqId);
        Task<QETmkRequestDocketView> GetTmkRequestDocket(int reqId);
        Task<QEGmRequestDocketView> GetGmRequestDocket(int reqId);

        Task<object> GetPatCountryApplicationDocRespDocketing(ParentDataStrategyParam param);
        Task<object> GetPatCountryApplicationDocRespDocketingReassigned(ParentDataStrategyParam param);

        Task<object> GetTmkTrademarkDocRespDocketing(ParentDataStrategyParam param);
        Task<object> GetTmkTrademarkDocRespDocketingReassigned(ParentDataStrategyParam param);

        Task<object> GetGmMatterDocRespDocketing(ParentDataStrategyParam param);
        Task<object> GetGmMatterDocRespDocketingReassigned(ParentDataStrategyParam param);

        Task<object> GetPatCountryApplicationDocRespReporting(ParentDataStrategyParam param);
        Task<object> GetPatCountryApplicationDocRespReportingReassigned(ParentDataStrategyParam param);

        Task<object> GetTmkTrademarkDocRespReporting(ParentDataStrategyParam param);
        Task<object> GetTmkTrademarkDocRespReportingReassigned(ParentDataStrategyParam param);

        Task<object> GetGmMatterDocRespReporting(ParentDataStrategyParam param);
        Task<object> GetGmMatterDocRespReportingReassigned(ParentDataStrategyParam param);


        Task<QEDocVerificationImageView> GetDocVerificationImage(ParentDataStrategyParam param);


        Task<List<QEFieldListDTO>> GetCustomFieldData(int dataSourceID, int parentId);

        Task<List<QERecipient>> GetDefaultRecipients(int id, string sendAs);
        Task<List<QERecipient>> GetDefaultToRecipients(int id);
        Task<List<QERecipient>> GetDefaultCopyToRecipients(int id);
        Task<List<QERecipient>> GetRecipients(int id, string sendAs);
        Task<List<QERecipientRoleDTO>> GetRecipientRole(string roleLink, string roleSource);
        Task<string> GetRecipientEmail(string roleLink, QERoleSource roleSource);
        Task<Dictionary<string, string>> GetRecipientNameAndEmail(string roleLink, QERoleSource roleSource);

        Task<List<QuickEmailImageLinkViewModel>> GetImages(string tableName, int parentId);
        Task<List<QuickEmailImageLinkViewModel>> GetExistImages(string tableName, int parentId, string systemName);
        Task<List<QuickEmailImageLinkViewModel>> GetImagesByFolderId(int folderId);
        Task<MemoryStream?> ImageToStream(string systemName, string imagePath);
        Task CopyAttachmentToTemporaryFolder(string systemName, string imagePath, string destinationPath,string? itemId,string? docLibrary);
        Task LogEmailImageAttachmentFromStream(QELog qeLog, MemoryStream sourceStream, string newFileName, string newThumbNail);
        Task LogEmailImageAttachment(QELog qeLog, string sourceFile, string newFileName, string newThumbNail);
        Task LogEmailUploadedAttachment(QELog qeLog, IFormFile file, string newFileName, string newThumbNail);
        Task<bool> IsUserRestrictedFromPrivateDocuments();

        #endregion

        #region Quick Email Popup Screen
        Task<CPiDataSourceResult> CreateViewModelForGrid(DataSourceRequest request, string systemType, string screenCode, string? templateName, int? qeCatId, List<string>? tags = null);

        #endregion

    }
}

 