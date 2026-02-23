using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Http;
using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Entities.Shared;
using R10.Core.Queries.Shared;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;
using R10.Web.Interfaces;
using R10.Web.Models;

namespace R10.Web.Services
{
    /// <summary>
    /// Stub for debloated app; returns empty/default for all Quick Email operations.
    /// </summary>
    public class QuickEmailViewModelServiceStub : IOuickEmailViewModelService
    {
        public IQueryable<QEDetailView> GetQuickEmails(string systemType) => Enumerable.Empty<QEDetailView>().AsQueryable();
        public Task<QuickEmailSetupDetailViewModel> GetQuickEmailSetupById(int id) => Task.FromResult<QuickEmailSetupDetailViewModel>(null);
        public Task<QEDetailView> GetQuickEmailByName(string systemType, string templateName) => Task.FromResult<QEDetailView>(null);
        public IQueryable<QERecipient> GetRecipients(int id) => Enumerable.Empty<QERecipient>().AsQueryable();
        public Task<QEDataSource> GetDataSource(string systemType, string dataSourceName) => Task.FromResult<QEDataSource>(null);

        public Task<ModuleMain> GetModule(int id) => Task.FromResult<ModuleMain>(null);
        public Task<ModuleMain> GetModule(string moduleCode, string subModule) => Task.FromResult<ModuleMain>(null);
        public Task<string> GetScreenName(int screenId) => Task.FromResult<string>(null);
        public Task<string> GetScreenName(string systemType, string screenCode) => Task.FromResult<string>(null);
        public Task<SystemScreen> GetSystemScreen(int screenId) => Task.FromResult<SystemScreen>(null);
        public Task<SystemScreen> GetSystemScreen(string systemType, string screenCode) => Task.FromResult<SystemScreen>(null);
        public Task<QuickEmailDetailViewModel> GetDefaultOrFirstQuickEmail(string systemType, string screenCode) => Task.FromResult<QuickEmailDetailViewModel>(null);
        public Task<QuickEmailDetailViewModel> GetQuickEmailById(int id) => Task.FromResult(new QuickEmailDetailViewModel { QESetupID = id });

        public Task<QEPatInventionView> GetPatInvention(ParentDataStrategyParam param) => Task.FromResult<QEPatInventionView>(null);
        public Task<QEPatInventionAttyChangedView> GetPatInventionAttyChanged(int invId) => Task.FromResult<QEPatInventionAttyChangedView>(null);
        public Task<QEPatCountryApplicationView> GetPatCountryApplication(ParentDataStrategyParam param) => Task.FromResult<QEPatCountryApplicationView>(null);
        public Task<QEPatInventorAppAwardView> GetPatInventorAppAward(int awardId) => Task.FromResult<QEPatInventorAppAwardView>(null);
        public Task<QEPatIRLumpSumAwardView> GetPatIRLumpSumAward(int inventorInvId) => Task.FromResult<QEPatIRLumpSumAwardView>(null);
        public Task<QEPatIRYearlyAwardView> GetPatIRYearlyAward(int inventorInvId) => Task.FromResult<QEPatIRYearlyAwardView>(null);
        public Task<QEPatIRDistributionAwardView> GetPatIRDistributionAward(int distributionId) => Task.FromResult<QEPatIRDistributionAwardView>(null);
        public Task<QEPatIRFRRemunerationAwardView> GetPatIRFRRemunerationAward(int inventorInvId) => Task.FromResult<QEPatIRFRRemunerationAwardView>(null);
        public Task<QEPatCostTrackingView> GetPatCostTracking(int costTrackId) => Task.FromResult<QEPatCostTrackingView>(null);
        public Task<QEPatCostTrackingInvView> GetPatCostTrackingInv(int costTrackInvId) => Task.FromResult<QEPatCostTrackingInvView>(null);
        public Task<QEPatActionDueView> GetPatActionDue(int actId) => Task.FromResult<QEPatActionDueView>(null);
        public Task<QEPatActionDueInvView> GetPatActionDueInv(int actId) => Task.FromResult<QEPatActionDueInvView>(null);
        public Task<QEPatActionDueDateView> GetPatActionDueDate(int ddId) => Task.FromResult<QEPatActionDueDateView>(null);
        public Task<QEPatActionDueDateInvView> GetPatActionDueDateInv(int ddId) => Task.FromResult<QEPatActionDueDateInvView>(null);
        public Task<QEPatActionDueDateDedocketView> GetPatActionDueDateDedocket(int deDocketId) => Task.FromResult<QEPatActionDueDateDedocketView>(null);
        public Task<QEPatActionDueDateInvDedocketView> GetPatActionDueDateInvDedocket(int deDocketInvId) => Task.FromResult<QEPatActionDueDateInvDedocketView>(null);
        public Task<QEPatActionDueDateDelegationView> QEPatActionDueDateDelegation(int delegationId) => Task.FromResult<QEPatActionDueDateDelegationView>(null);
        public Task<QEPatActionDueDateInvDelegationView> QEPatActionDueDateInvDelegation(int delegationId) => Task.FromResult<QEPatActionDueDateInvDelegationView>(null);
        public Task<QEPatActionDueDateDelegationView> QEPatActionDueDateDeletedDelegation(int delegationId) => Task.FromResult<QEPatActionDueDateDelegationView>(null);
        public Task<QEPatActionDueDateInvDelegationView> QEPatActionDueDateInvDeletedDelegation(int delegationId) => Task.FromResult<QEPatActionDueDateInvDelegationView>(null);
        public Task<QEPatActionDueDateDelegationView> QEPatActionDueDateReassignedDelegation(int delegationId) => Task.FromResult<QEPatActionDueDateDelegationView>(null);
        public Task<QEPatActionDueDateInvDelegationView> QEPatActionDueDateInvReassignedDelegation(int delegationId) => Task.FromResult<QEPatActionDueDateInvDelegationView>(null);
        public Task<QEPatSearchView> GetPatSearchNotifyLog(int searchId) => Task.FromResult<QEPatSearchView>(null);
        public Task<QEPatCountryApplicationImageView> GetPatCountryApplicationImage(ParentDataStrategyParam param) => Task.FromResult<QEPatCountryApplicationImageView>(null);
        public Task<QEPatActionImageView> GetPatActionImage(ParentDataStrategyParam param) => Task.FromResult<QEPatActionImageView>(null);
        public Task<QEPatActionInvImageView> GetPatActionInvImage(ParentDataStrategyParam param) => Task.FromResult<QEPatActionInvImageView>(null);
        public Task<QEPatActionDueDeletedView> GetPatActionDueDeleted(int actId) => Task.FromResult<QEPatActionDueDeletedView>(null);
        public Task<QEPatActionDueInvDeletedView> GetPatActionDueInvDeleted(int actId) => Task.FromResult<QEPatActionDueInvDeletedView>(null);
        public Task<QEPatCountryAppDeletedView> GetPatCountryAppDeleted(int appId) => Task.FromResult<QEPatCountryAppDeletedView>(null);
        public Task<QEPacClearanceView> GetPatClearance(int pacId) => Task.FromResult<QEPacClearanceView>(null);

        public Task<QETmkTrademarkView> GetTmkTrademark(ParentDataStrategyParam param) => Task.FromResult<QETmkTrademarkView>(null);
        public Task<QETmkCostTrackingView> GetTmkCostTracking(int costTrackId) => Task.FromResult<QETmkCostTrackingView>(null);
        public Task<QETmkActionDueView> GetTmkActionDue(int actId) => Task.FromResult<QETmkActionDueView>(null);
        public Task<QETmkActionDueDateView> GetTmkActionDueDate(int ddId) => Task.FromResult<QETmkActionDueDateView>(null);
        public Task<QETmkActionDueDateDedocketView> GetTmkActionDueDateDedocket(int deDocketId) => Task.FromResult<QETmkActionDueDateDedocketView>(null);
        public Task<QETmkConflictView> GetTmkConflict(int conflictId) => Task.FromResult<QETmkConflictView>(null);
        public Task<QETmkImageView> GetTmkImage(ParentDataStrategyParam param) => Task.FromResult<QETmkImageView>(null);
        public Task<QETmkActionImageView> GetTmkActionImage(ParentDataStrategyParam param) => Task.FromResult<QETmkActionImageView>(null);
        public Task<QETmkActionDueDeletedView> GetTmkActionDueDeleted(int actId) => Task.FromResult<QETmkActionDueDeletedView>(null);
        public Task<QETmkTrademarkDeletedView> GetTmkTrademarkDeleted(int tmkId) => Task.FromResult<QETmkTrademarkDeletedView>(null);
        public Task<QETmkTrademarkAttyChangedView> GetTmkTrademarkAttyChanged(int tmkId) => Task.FromResult<QETmkTrademarkAttyChangedView>(null);
        public Task<QETmcClearanceView> GetTmcClearance(int tmcId) => Task.FromResult<QETmcClearanceView>(null);
        public Task<QETmkActionDueDateDelegationView> QETmkActionDueDateDelegation(int delegationId) => Task.FromResult<QETmkActionDueDateDelegationView>(null);
        public Task<QETmkActionDueDateDelegationView> QETmkActionDueDateDeletedDelegation(int delegationId) => Task.FromResult<QETmkActionDueDateDelegationView>(null);
        public Task<QETmkActionDueDateDelegationView> QETmkActionDueDateReassignedDelegation(int delegationId) => Task.FromResult<QETmkActionDueDateDelegationView>(null);

        public Task<QEGmMatterView> GetGmMatter(ParentDataStrategyParam param) => Task.FromResult<QEGmMatterView>(null);
        public Task<QEGmCostTrackingView> GetGmCostTracking(int costTrackId) => Task.FromResult<QEGmCostTrackingView>(null);
        public Task<QEGmActionDueView> GetGmActionDue(int actId) => Task.FromResult<QEGmActionDueView>(null);
        public Task<QEGmActionDueDateView> GetGmActionDueDate(int ddId) => Task.FromResult<QEGmActionDueDateView>(null);
        public Task<QEGmActionDueDateDedocketView> GetGmActionDueDateDedocket(int deDocketId) => Task.FromResult<QEGmActionDueDateDedocketView>(null);
        public Task<QEGmImageView> GetGmImage(ParentDataStrategyParam param) => Task.FromResult<QEGmImageView>(null);
        public Task<QEGmActionImageView> GetGmActionImage(ParentDataStrategyParam param) => Task.FromResult<QEGmActionImageView>(null);
        public Task<QEGmActionDueDeletedView> GetGmActionDueDeleted(int actId) => Task.FromResult<QEGmActionDueDeletedView>(null);
        public Task<QEGmMatterDeletedView> GetGmMatterDeleted(int matId) => Task.FromResult<QEGmMatterDeletedView>(null);
        public Task<QEGmActionDueDateDelegationView> QEGmActionDueDateDelegation(int delegationId) => Task.FromResult<QEGmActionDueDateDelegationView>(null);
        public Task<QEGmActionDueDateDelegationView> QEGmActionDueDateDeletedDelegation(int delegationId) => Task.FromResult<QEGmActionDueDateDelegationView>(null);
        public Task<QEGmActionDueDateDelegationView> QEGmActionDueDateReassignedDelegation(int delegationId) => Task.FromResult<QEGmActionDueDateDelegationView>(null);

        public Task<QEPatRequestDocketView> GetPatRequestDocket(int reqId) => Task.FromResult<QEPatRequestDocketView>(null);
        public Task<QETmkRequestDocketView> GetTmkRequestDocket(int reqId) => Task.FromResult<QETmkRequestDocketView>(null);
        public Task<QEGmRequestDocketView> GetGmRequestDocket(int reqId) => Task.FromResult<QEGmRequestDocketView>(null);

        public Task<object> GetPatCountryApplicationDocRespDocketing(ParentDataStrategyParam param) => Task.FromResult<object>(null);
        public Task<object> GetPatCountryApplicationDocRespDocketingReassigned(ParentDataStrategyParam param) => Task.FromResult<object>(null);
        public Task<object> GetTmkTrademarkDocRespDocketing(ParentDataStrategyParam param) => Task.FromResult<object>(null);
        public Task<object> GetTmkTrademarkDocRespDocketingReassigned(ParentDataStrategyParam param) => Task.FromResult<object>(null);
        public Task<object> GetGmMatterDocRespDocketing(ParentDataStrategyParam param) => Task.FromResult<object>(null);
        public Task<object> GetGmMatterDocRespDocketingReassigned(ParentDataStrategyParam param) => Task.FromResult<object>(null);
        public Task<object> GetPatCountryApplicationDocRespReporting(ParentDataStrategyParam param) => Task.FromResult<object>(null);
        public Task<object> GetPatCountryApplicationDocRespReportingReassigned(ParentDataStrategyParam param) => Task.FromResult<object>(null);
        public Task<object> GetTmkTrademarkDocRespReporting(ParentDataStrategyParam param) => Task.FromResult<object>(null);
        public Task<object> GetTmkTrademarkDocRespReportingReassigned(ParentDataStrategyParam param) => Task.FromResult<object>(null);
        public Task<object> GetGmMatterDocRespReporting(ParentDataStrategyParam param) => Task.FromResult<object>(null);
        public Task<object> GetGmMatterDocRespReportingReassigned(ParentDataStrategyParam param) => Task.FromResult<object>(null);

        public Task<QEDocVerificationImageView> GetDocVerificationImage(ParentDataStrategyParam param) => Task.FromResult<QEDocVerificationImageView>(null);
        public Task<List<QEFieldListDTO>> GetCustomFieldData(int dataSourceID, int parentId) => Task.FromResult(new List<QEFieldListDTO>());

        public Task<List<QERecipient>> GetDefaultRecipients(int id, string sendAs) => Task.FromResult(new List<QERecipient>());
        public Task<List<QERecipient>> GetDefaultToRecipients(int id) => Task.FromResult(new List<QERecipient>());
        public Task<List<QERecipient>> GetDefaultCopyToRecipients(int id) => Task.FromResult(new List<QERecipient>());
        public Task<List<QERecipient>> GetRecipients(int id, string sendAs) => Task.FromResult(new List<QERecipient>());
        public Task<List<QERecipientRoleDTO>> GetRecipientRole(string roleLink, string roleSource) => Task.FromResult(new List<QERecipientRoleDTO>());
        public Task<string> GetRecipientEmail(string roleLink, QERoleSource roleSource) => Task.FromResult<string>(null);
        public Task<Dictionary<string, string>> GetRecipientNameAndEmail(string roleLink, QERoleSource roleSource) => Task.FromResult(new Dictionary<string, string>());

        public Task<List<QuickEmailImageLinkViewModel>> GetImages(string tableName, int parentId) => Task.FromResult(new List<QuickEmailImageLinkViewModel>());
        public Task<List<QuickEmailImageLinkViewModel>> GetExistImages(string tableName, int parentId, string systemName) => Task.FromResult(new List<QuickEmailImageLinkViewModel>());
        public Task<List<QuickEmailImageLinkViewModel>> GetImagesByFolderId(int folderId) => Task.FromResult(new List<QuickEmailImageLinkViewModel>());
        public Task<MemoryStream> ImageToStream(string systemName, string imagePath) => Task.FromResult<MemoryStream>(null);
        public Task CopyAttachmentToTemporaryFolder(string systemName, string imagePath, string destinationPath, string? itemId, string? docLibrary) => Task.CompletedTask;
        public Task LogEmailImageAttachmentFromStream(QELog qeLog, MemoryStream sourceStream, string newFileName, string newThumbNail) => Task.CompletedTask;
        public Task LogEmailImageAttachment(QELog qeLog, string sourceFile, string newFileName, string newThumbNail) => Task.CompletedTask;
        public Task LogEmailUploadedAttachment(QELog qeLog, IFormFile file, string newFileName, string newThumbNail) => Task.CompletedTask;
        public Task<bool> IsUserRestrictedFromPrivateDocuments() => Task.FromResult(false);

        public Task<CPiDataSourceResult> CreateViewModelForGrid(DataSourceRequest request, string systemType, string screenCode, string? templateName, int? qeCatId, List<string>? tags = null) => Task.FromResult(new CPiDataSourceResult());
    }
}
