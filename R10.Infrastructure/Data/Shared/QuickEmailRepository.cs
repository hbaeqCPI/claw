using Microsoft.EntityFrameworkCore;
using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Interfaces;
using R10.Core.Queries.Shared;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Security.Claims;
using R10.Core.Entities.Shared;
using R10.Core.Entities.Documents;
using R10.Core.Services.Shared;

namespace R10.Infrastructure.Data
{

    public class QuickEmailRepository : IQuickEmailRepository
    {

        protected readonly ApplicationDbContext _dbContext;
        private readonly IServiceProvider _serviceProvider;

        public QuickEmailRepository(ApplicationDbContext dbContext,
            IServiceProvider serviceProvider)
        {
            _dbContext = dbContext;
            _serviceProvider = serviceProvider;
        }

        public IQueryable<QEDetailView> GetQuickEmails(string systemType)
        {
            var quickEmails = _dbContext.QEDetailView.Where(e => e.SystemType == systemType).AsNoTracking();
            return quickEmails;
        }

        public IQueryable<QEMain> GetQuickEmailById(int id)
        {
            var quickEmail = _dbContext.QEMains.Where(e => e.QESetupID == id).AsNoTracking();
            return quickEmail;
        }

        public IQueryable<QEMain> GetQuickEmails(string systemType, string screenCode, string? templateName, int? qeCatId, List<string>? tags = null)

        {
            var tagsList = "";
            if (tags != null && tags.Count > 1)
            {
                foreach (var val in tags)
                {
                    tagsList = tagsList + val + "~";
                }
            }
            var quickEmail = _dbContext.QEMains.Where(qe => qe.SystemScreen.SystemType == systemType
                                                            && qe.SystemScreen.ScreenCode == screenCode
                                                            && (string.IsNullOrEmpty(templateName) || qe.TemplateName.Contains(templateName) || EF.Functions.Like(qe.TemplateName, templateName.Replace("*", "%")))
                                                            && (qeCatId == null || qe.QECatId == qeCatId)
                                                            && (tags == null || tags.Count() == 0
                                                                             || (string.IsNullOrEmpty(tagsList) && qe.QETags.Any(t => EF.Functions.Like(t.Tag, tags.FirstOrDefault())))
                                                                             || (!string.IsNullOrEmpty(tagsList) && qe.QETags.Any(t => EF.Functions.Like(tagsList, '%' + t.Tag + '%')))
                                                               )
                                                            && qe.InUse == true
                                                            ).AsNoTracking();
            return quickEmail;
        }

        public async Task<QEDetailView> GetQuickEmailByName(string systemType, string templateName)
        {
            var quickEmail = await _dbContext.QEDetailView.Where(e => e.SystemType == systemType && e.TemplateName == templateName).AsNoTracking().FirstOrDefaultAsync();
            return quickEmail;
        }

        public async Task<QEMain> GetDefaultQuickEmail(string systemType, string screenCode)
        {
            var systemScreen = await GetSystemScreen(systemType, screenCode);
            var quickEmail = await _dbContext.QEMains.Where(e => e.ScreenId == systemScreen.ScreenId && e.IsDefault == true && e.InUse).AsNoTracking().FirstOrDefaultAsync();

            // no default template so just get the first applicable template
            if (quickEmail == null)
                quickEmail = await _dbContext.QEMains.Where(e => e.ScreenId == systemScreen.ScreenId && e.InUse).AsNoTracking().FirstOrDefaultAsync();

            return quickEmail;
        }

        public async Task<ModuleMain> GetModule(int id)
        {
            var module = await _dbContext.ModulesMain.FindAsync(id);
            return module;
        }
        public async Task<ModuleMain> GetModule(string moduleCode, string subModule)
        {
            var module = await _dbContext.ModulesMain.Where(e => e.ModuleCode == moduleCode && e.SubModule == subModule).AsNoTracking().FirstOrDefaultAsync();
            return module;
        }

        public async Task<SystemScreen> GetSystemScreen(int id)
        {
            var systemScreen = await _dbContext.SystemScreens.Where(e => e.ScreenId == id).AsNoTracking().FirstOrDefaultAsync();
            return systemScreen;
        }

        public async Task<SystemScreen> GetSystemScreen(string systemType, string screenCode)
        {
            var systemScreen = await _dbContext.SystemScreens.Where(e => e.SystemType == systemType && e.ScreenCode == screenCode).AsNoTracking().FirstOrDefaultAsync();
            return systemScreen;
        }

        public IQueryable<QERecipient> GetRecipients(int id)
        {
            var quickEmail = _dbContext.QERecipients.Where(e => e.QESetupID == id).OrderBy(e => e.OrderOfEntry).Include(r => r.QERoleSource).AsNoTracking();
            return quickEmail;
        }

        public async Task<QEDataSource> GetDataSource(string systemType, string dataSourceName)
        {
            var dataSource = await _dbContext.QEDataSources.Where(e => e.SystemType == systemType && e.DataSourceName == dataSourceName).AsNoTracking().FirstOrDefaultAsync();
            return dataSource;
        }

        public async Task<QEPatInventionView> GetPatInvention(int invId)
        {
            var invention = await _dbContext.QEPatInventionView.Where(e => e.InvId == invId).AsNoTracking().FirstOrDefaultAsync();
            return invention;
        }

        public async Task<QEPatInventionAttyChangedView> GetPatInventionAttyChanged(int invId)
        {
            var invention = await _dbContext.QEPatInventionAttyChangedView.Where(e => e.InvId == invId).AsNoTracking().FirstOrDefaultAsync();
            return invention;
        }

        public async Task<QEPatCountryApplicationView> GetPatCountryApplication(int appId)
        {
            var application = await _dbContext.QEPatCountryApplicationView.Where(e => e.AppId == appId).AsNoTracking().FirstOrDefaultAsync();
            return application;
        }

        public async Task<QEPatInventorAppAwardView> GetPatInventorAppAward(int awardId)
        {
            var award = await _dbContext.QEPatInventorAppAwardView.Where(e => e.AwardId == awardId).AsNoTracking().FirstOrDefaultAsync();
            return award;
        }

        public async Task<QEPatInventorDMSAwardView> GetPatInventorDMSAward(int awardId)
        {
            var award = await _dbContext.QEPatInventorDMSAwardView.Where(e => e.AwardId == awardId).AsNoTracking().FirstOrDefaultAsync();
            return award;
        }

        public async Task<QEPatIRLumpSumAwardView> GetPatIRLumpSumAward(int inventorInvId)
        {
            var award = await _dbContext.QEPatIRLumpSumAwardView.Where(e => e.InventorInvID == inventorInvId).AsNoTracking().FirstOrDefaultAsync();
            return award;
        }

        public async Task<QEPatIRYearlyAwardView> GetPatIRYearlyAward(int inventorInvId)
        {
            var award = await _dbContext.QEPatIRYearlyAwardView.Where(e => e.InventorInvID == inventorInvId).AsNoTracking().FirstOrDefaultAsync();
            return award;
        }

        public async Task<QEPatIRDistributionAwardView> GetPatIRDistributionAward(int distributionId)
        {
            var award = await _dbContext.QEPatIRDistributionAwardView.Where(e => e.DistributionId == distributionId).AsNoTracking().FirstOrDefaultAsync();
            return award;
        }

        public async Task<QEPatIRFRRemunerationAwardView> GetPatIRFRRemunerationAward(int inventorInvId)
        {
            var award = await _dbContext.QEPatIRFRRemunerationAwardView.Where(e => e.InventorInvID == inventorInvId).AsNoTracking().FirstOrDefaultAsync();
            return award;
        }

        public async Task<QEPatCostTrackingView> GetPatCostTracking(int costTrackId)
        {
            var costTrack = await _dbContext.QEPatCostTrackingView.Where(e => e.CostTrackID == costTrackId).AsNoTracking().FirstOrDefaultAsync();
            return costTrack;
        }

        public async Task<QEPatCostTrackingInvView> GetPatCostTrackingInv(int costTrackInvId)
        {
            var costTrack = await _dbContext.QEPatCostTrackingInvView.Where(e => e.CostTrackInvId == costTrackInvId).AsNoTracking().FirstOrDefaultAsync();
            return costTrack;
        }

        public async Task<QEPatActionDueView> GetPatActionDue(int actId)
        {
            var action = await _dbContext.QEPatActionDueView.Where(e => e.ActId == actId).AsNoTracking().FirstOrDefaultAsync();
            return action;
        }

        public async Task<QEPatActionDueInvView> GetPatActionDueInv(int actId)
        {
            var action = await _dbContext.QEPatActionDueInvView.Where(e => e.ActId == actId).AsNoTracking().FirstOrDefaultAsync();
            return action;
        }

        public async Task<QEPatActionDueDateView> GetPatActionDueDate(int ddId)
        {
            var action = await _dbContext.QEPatActionDueDateView.Where(e => e.DDId == ddId).AsNoTracking().FirstOrDefaultAsync();
            return action;
        }

        public async Task<QEPatActionDueDateInvView> GetPatActionDueDateInv(int ddId)
        {
            var action = await _dbContext.QEPatActionDueDateInvView.Where(e => e.DDId == ddId).AsNoTracking().FirstOrDefaultAsync();
            return action;
        }

        public async Task<QEPatActionDueDateDedocketView> GetPatActionDueDateDedocket(int deDocketId)
        {
            var action = await _dbContext.QEPatActionDueDateDedocketView.Where(e => e.DeDocketId == deDocketId).AsNoTracking().FirstOrDefaultAsync();
            return action;
        }

        public async Task<QEPatActionDueDateInvDedocketView> GetPatActionDueDateInvDedocket(int deDocketId)
        {
            var action = await _dbContext.QEPatActionDueDateInvDedocketView.Where(e => e.DeDocketId == deDocketId).AsNoTracking().FirstOrDefaultAsync();
            return action;
        }

        public async Task<QEPatActionDueDeletedView> GetPatActionDueDeleted(int actId)
        {
            var action = await _dbContext.QEPatActionDueDeletedView.Where(e => e.ActId == actId).AsNoTracking().FirstOrDefaultAsync();
            return action;
        }

        public async Task<QEPatActionDueInvDeletedView> GetPatActionDueInvDeleted(int actId)
        {
            var action = await _dbContext.QEPatActionDueInvDeletedView.Where(e => e.ActId == actId).AsNoTracking().FirstOrDefaultAsync();
            return action;
        }

        public async Task<QEPatActionDueDateDelegationView> QEPatActionDueDateDelegation(int delegationId)
        {
            var action = await _dbContext.QEPatActionDueDateDelegationView.Where(e => e.DelegationId == delegationId).AsNoTracking().FirstOrDefaultAsync();
            return action;
        }

        public async Task<QEPatActionDueDateInvDelegationView> QEPatActionDueDateInvDelegation(int delegationId)
        {
            var action = await _dbContext.QEPatActionDueDateInvDelegationView.Where(e => e.DelegationId == delegationId).AsNoTracking().FirstOrDefaultAsync();
            return action;
        }

        public async Task<QEPatActionDueDateDelegationView> QEPatActionDueDateReassignedDelegation(int delegationId)
        {
            var action = await _dbContext.QEPatActionDueDateDelegationView.FromSqlInterpolated($"Select * From vwQE_Pat_ActionDueDateReassignedDelegation Where DelegationId={delegationId}").AsNoTracking().FirstOrDefaultAsync();
            return action;
        }

        public async Task<QEPatActionDueDateInvDelegationView> QEPatActionDueDateInvReassignedDelegation(int delegationId)
        {
            var action = await _dbContext.QEPatActionDueDateInvDelegationView.FromSqlInterpolated($"Select * From vwQE_Pat_ActionDueDateInvReassignedDelegation Where DelegationId={delegationId}").AsNoTracking().FirstOrDefaultAsync();
            return action;
        }

        public async Task<QEPatActionDueDateDelegationView> QEPatActionDueDateDeletedDelegation(int delegationId)
        {
            var action = await _dbContext.QEPatActionDueDateDelegationView.FromSqlInterpolated($"Select * From vwQE_Pat_ActionDueDateDeletedDelegation Where DelegationId={delegationId}").AsNoTracking().FirstOrDefaultAsync();
            return action;
        }

        public async Task<QEPatActionDueDateInvDelegationView> QEPatActionDueDateInvDeletedDelegation(int delegationId)
        {
            var action = await _dbContext.QEPatActionDueDateInvDelegationView.FromSqlInterpolated($"Select * From vwQE_Pat_ActionDueDateInvDeletedDelegation Where DelegationId={delegationId}").AsNoTracking().FirstOrDefaultAsync();
            return action;
        }

        public async Task<QEPatCountryAppDeletedView> GetPatCountryAppDeleted(int appId)
        {
            var record = await _dbContext.QEPatCountryAppDeletedView.Where(e => e.AppId == appId).AsNoTracking().FirstOrDefaultAsync();
            return record;
        }

        public async Task<QEPatSearchView> GetPatSearchNotifyLog(int searchId)
        {
            var result = await _dbContext.QEPatSearchView.Where(e => e.SearchId == searchId).AsNoTracking().FirstOrDefaultAsync();
            return result;
        }

        public async Task<QEPatCountryApplicationImageView> GetPatCountryApplicationImage(int docId)
        {
            var result = await _dbContext.QEPatCountryApplicationImageView.Where(e => e.DocId == docId).AsNoTracking().FirstOrDefaultAsync();
            return result;
        }
        public async Task<QEPatActionImageView> GetPatActionImage(int docId)
        {
            var result = await _dbContext.QEPatActionImageView.Where(e => e.DocId == docId).AsNoTracking().FirstOrDefaultAsync();
            return result;
        }

        public async Task<QEPatActionInvImageView> GetPatActionInvImage(int docId)
        {
            var result = await _dbContext.QEPatActionInvImageView.Where(e => e.DocId == docId).AsNoTracking().FirstOrDefaultAsync();
            return result;
        }


        public async Task<QETmkTrademarkView> GetTmkTrademark(int tmkId)
        {
            var trademark = await _dbContext.QETmkTrademarkView.Where(e => e.TmkId == tmkId).AsNoTracking().FirstOrDefaultAsync();
            return trademark;
        }

        public async Task<QETmkCostTrackingView> GetTmkCostTracking(int costTrackId)
        {
            var costTrack = await _dbContext.QETmkCostTrackingView.Where(e => e.CostTrackId == costTrackId).AsNoTracking().FirstOrDefaultAsync();
            return costTrack;
        }

        public async Task<QETmkActionDueView> GetTmkActionDue(int actId)
        {
            var action = await _dbContext.QETmkActionDueView.Where(e => e.ActId == actId).AsNoTracking().FirstOrDefaultAsync();
            return action;
        }

        public async Task<QETmkActionDueDateView> GetTmkActionDueDate(int ddId)
        {
            var action = await _dbContext.QETmkActionDueDateView.Where(e => e.DDId == ddId).AsNoTracking().FirstOrDefaultAsync();
            return action;
        }

        public async Task<QETmkActionDueDateDedocketView> GetTmkActionDueDateDedocket(int deDocketId)
        {
            var action = await _dbContext.QETmkActionDueDateDedocketView.Where(e => e.DeDocketId == deDocketId).AsNoTracking().FirstOrDefaultAsync();
            return action;
        }

        public async Task<QETmkConflictView> GetTmkConflict(int conflictId)
        {
            var action = await _dbContext.QETmkConflictView.Where(e => e.ConflictID == conflictId).AsNoTracking().FirstOrDefaultAsync();
            return action;
        }

        public async Task<QETmkActionImageView> GetTmkActionImage(int docId)
        {
            var result = await _dbContext.QETmkActionImageView.Where(e => e.DocId == docId).AsNoTracking().FirstOrDefaultAsync();
            return result;
        }

        public async Task<QETmkImageView> GetTmkImage(int docId)
        {
            var result = await _dbContext.QETmkImageView.Where(e => e.DocId == docId).AsNoTracking().FirstOrDefaultAsync();
            return result;
        }

        public async Task<QETmkActionDueDeletedView> GetTmkActionDueDeleted(int actId)
        {
            var action = await _dbContext.QETmkActionDueDeletedView.Where(e => e.ActId == actId).AsNoTracking().FirstOrDefaultAsync();
            return action;
        }

        public async Task<QETmkTrademarkDeletedView> GetTmkTrademarkDeleted(int tmkId)
        {
            var record = await _dbContext.QETmkTrademarkDeletedView.Where(e => e.TmkId == tmkId).AsNoTracking().FirstOrDefaultAsync();
            return record;
        }

        public async Task<QETmkTrademarkAttyChangedView> GetTmkTrademarkAttyChanged(int tmkId)
        {
            var trademark = await _dbContext.QETmkTrademarkAttyChangedView.Where(e => e.TmkId == tmkId).AsNoTracking().FirstOrDefaultAsync();
            return trademark;
        }

        public async Task<QETmkActionDueDateDelegationView> QETmkActionDueDateDelegation(int delegationId)
        {
            var action = await _dbContext.QETmkActionDueDateDelegationView.Where(e => e.DelegationId == delegationId).AsNoTracking().FirstOrDefaultAsync();
            return action;
        }

        public async Task<QETmkActionDueDateDelegationView> QETmkActionDueDateReassignedDelegation(int delegationId)
        {
            var action = await _dbContext.QETmkActionDueDateDelegationView.FromSqlInterpolated($"Select * From vwQE_Tmk_ActionDueDateReassignedDelegation Where DelegationId={delegationId}").AsNoTracking().FirstOrDefaultAsync();
            return action;
        }

        public async Task<QETmkActionDueDateDelegationView> QETmkActionDueDateDeletedDelegation(int delegationId)
        {
            var action = await _dbContext.QETmkActionDueDateDelegationView.FromSqlInterpolated($"Select * From vwQE_Tmk_ActionDueDateDeletedDelegation Where DelegationId={delegationId}").AsNoTracking().FirstOrDefaultAsync();
            return action;
        }

        public async Task<QEGmMatterView> GetGmMatter(int matId)
        {
            var matter = await _dbContext.QEGmMatterView.Where(e => e.MatId == matId).AsNoTracking().FirstOrDefaultAsync();
            return matter;
        }

        public async Task<QEGmMatterDeletedView> GetGmMatterDeleted(int matId)
        {
            var matter = await _dbContext.QEGmMatterDeletedView.Where(e => e.MatId == matId).AsNoTracking().FirstOrDefaultAsync();
            return matter;
        }

        public async Task<QEGmCostTrackingView> GetGmCostTracking(int costTrackId)
        {
            var costTrack = await _dbContext.QEGmCostTrackingView.Where(e => e.CostTrackID == costTrackId).AsNoTracking().FirstOrDefaultAsync();
            return costTrack;
        }

        public async Task<QEGmActionDueView> GetGmActionDue(int actId)
        {
            var action = await _dbContext.QEGMActionDueView.Where(e => e.ActId == actId).AsNoTracking().FirstOrDefaultAsync();
            return action;
        }

        public async Task<QEGmActionDueDateView> GetGmActionDueDate(int ddId)
        {
            var action = await _dbContext.QEGmActionDueDateView.Where(e => e.DDId == ddId).AsNoTracking().FirstOrDefaultAsync();
            return action;
        }

        public async Task<QEGmActionDueDateDedocketView> GetGmActionDueDateDedocket(int deDocketId)
        {
            var action = await _dbContext.QEGmActionDueDateDedocketView.Where(e => e.DeDocketId == deDocketId).AsNoTracking().FirstOrDefaultAsync();
            return action;
        }

        public async Task<QEGmActionImageView> GetGmActionImage(int docId)
        {
            var result = await _dbContext.QEGmActionImageView.Where(e => e.DocId == docId).AsNoTracking().FirstOrDefaultAsync();
            return result;
        }

        public async Task<QEGmImageView> GetGmImage(int docId)
        {
            var result = await _dbContext.QEGmImageView.Where(e => e.DocId == docId).AsNoTracking().FirstOrDefaultAsync();
            return result;
        }

        public async Task<QEGmActionDueDeletedView> GetGmActionDueDeleted(int actId)
        {
            var action = await _dbContext.QEGmActionDueDeletedView.Where(e => e.ActId == actId).AsNoTracking().FirstOrDefaultAsync();
            return action;
        }

        public async Task<QEGmActionDueDateDelegationView> QEGmActionDueDateDelegation(int delegationId)
        {
            var action = await _dbContext.QEGmActionDueDateDelegationView.Where(e => e.DelegationId == delegationId).AsNoTracking().FirstOrDefaultAsync();
            return action;
        }

        public async Task<QEGmActionDueDateDelegationView> QEGmActionDueDateReassignedDelegation(int delegationId)
        {
            var action = await _dbContext.QEGmActionDueDateDelegationView.FromSqlInterpolated($"Select * From vwQE_Gm_ActionDueDateReassignedDelegation Where DelegationId={delegationId}").AsNoTracking().FirstOrDefaultAsync();
            return action;
        }

        public async Task<QEGmActionDueDateDelegationView> QEGmActionDueDateDeletedDelegation(int delegationId)
        {
            var action = await _dbContext.QEGmActionDueDateDelegationView.FromSqlInterpolated($"Select * From vwQE_GM_ActionDueDateDeletedDelegation Where DelegationId={delegationId}").AsNoTracking().FirstOrDefaultAsync();
            return action;
        }

        public async Task<QEDmsDisclosureView> GetDmsDisclosure(int dmsId)
        {
            var disclosure = await _dbContext.QEDmsDisclosureView.Where(e => e.DMSId == dmsId).AsNoTracking().FirstOrDefaultAsync();
            return disclosure;
        }

        public async Task<QEDmsDisclosureReviewView> GetDmsDisclosureReview(int dmsId)
        {
            var disclosure = await _dbContext.QEDmsDisclosureReviewView.Where(e => e.DMSId == dmsId).AsNoTracking().FirstOrDefaultAsync();
            return disclosure;
        }

        public async Task<QEDmsAgendaView> GetDmsAgenda(int agendaId)
        {
            var agenda = await _dbContext.QEDmsAgendaView.Where(e => e.AgendaId == agendaId).AsNoTracking().FirstOrDefaultAsync();
            return agenda;
        }

        public async Task<QEDmsActionDueView> GetDmsActionDue(int actId)
        {
            var action = await _dbContext.QEDmsActionDueView.Where(e => e.ActId == actId).AsNoTracking().FirstOrDefaultAsync();
            return action;
        }

        public async Task<QEDmsActionDueDateView> GetDmsActionDueDate(int ddId)
        {
            var action = await _dbContext.QEDmsActionDueDateView.Where(e => e.DDId == ddId).AsNoTracking().FirstOrDefaultAsync();
            return action;
        }

        public async Task<QEDmsActionDueDateDelegationView> QEDmsActionDueDateDelegation(int delegationId)
        {
            var action = await _dbContext.QEDmsActionDueDateDelegationView.Where(e => e.DelegationId == delegationId).AsNoTracking().FirstOrDefaultAsync();
            return action;
        }

        public async Task<QETmcClearanceView> GetTmcClearance(int tmcId)
        {
            var clearance = await _dbContext.QETmcClearanceView.Where(e => e.TmcId == tmcId).AsNoTracking().FirstOrDefaultAsync();
            return clearance;
        }

        public async Task<QEPacClearanceView> GetPatClearance(int pacId)
        {
            var patClearance = await _dbContext.QEPacClearanceView.Where(e => e.PacId == pacId).AsNoTracking().FirstOrDefaultAsync();
            return patClearance;
        }

        public async Task<QEPatCountryApplicationDocRespDocketingView> GetPatCountryApplicationDocRespDocketing(int docId = 0, string driveItemId = "")
        {
            var qeView = new QEPatCountryApplicationDocRespDocketingView();
            if (docId > 0)
            {
                qeView = await _dbContext.QEPatCountryApplicationDocRespDocketingView.Where(e => e.DocId == docId).AsNoTracking().FirstOrDefaultAsync();
            }
            else if (!string.IsNullOrEmpty(driveItemId))
            {
                qeView = await _dbContext.QEPatCountryApplicationDocRespDocketingView.Where(e => e.DriveItemId == driveItemId).AsNoTracking().FirstOrDefaultAsync();
            }
            return qeView ?? new QEPatCountryApplicationDocRespDocketingView();
        }
        public async Task<QEPatCountryApplicationDocRespDocketingView> GetPatCountryApplicationDocRespDocketingReassigned(int docId = 0, string driveItemId = "")
        {
            var qeView = new QEPatCountryApplicationDocRespDocketingView();
            if (docId > 0)
            {
                qeView = await _dbContext.QEPatCountryApplicationDocRespDocketingView.FromSqlInterpolated($"Select * From vwQE_Pat_CountryAppDocRespDocketingReassigned Where DocId={docId}").AsNoTracking().FirstOrDefaultAsync();
            }
            else if (!string.IsNullOrEmpty(driveItemId))
            {
                qeView = await _dbContext.QEPatCountryApplicationDocRespDocketingView.FromSqlInterpolated($"Select * From vwQE_Pat_CountryAppDocRespDocketingReassigned Where DriveItemId={driveItemId}").AsNoTracking().FirstOrDefaultAsync();
            }
            return qeView ?? new QEPatCountryApplicationDocRespDocketingView();
        }

        public async Task<QETmkTrademarkDocRespDocketingView> GetTmkTrademarkDocRespDocketing(int docId = 0, string driveItemId = "")
        {
            var qeView = new QETmkTrademarkDocRespDocketingView();
            if (docId > 0)
            {
                qeView = await _dbContext.QETmkTrademarkDocRespDocketingView.Where(e => e.DocId == docId).AsNoTracking().FirstOrDefaultAsync();
            }
            else if (!string.IsNullOrEmpty(driveItemId))
            {
                qeView = await _dbContext.QETmkTrademarkDocRespDocketingView.Where(e => e.DriveItemId == driveItemId).AsNoTracking().FirstOrDefaultAsync();
            }
            return qeView ?? new QETmkTrademarkDocRespDocketingView();
        }
        public async Task<QETmkTrademarkDocRespDocketingView> GetTmkTrademarkDocRespDocketingReassigned(int docId = 0, string driveItemId = "")
        {
            var qeView = new QETmkTrademarkDocRespDocketingView();
            if (docId > 0)
            {
                qeView = await _dbContext.QETmkTrademarkDocRespDocketingView.FromSqlInterpolated($"Select * From vwQE_Tmk_TrademarkDocRespDocketingReassigned Where DocId={docId}").AsNoTracking().FirstOrDefaultAsync();
            }
            else if (!string.IsNullOrEmpty(driveItemId))
            {
                qeView = await _dbContext.QETmkTrademarkDocRespDocketingView.FromSqlInterpolated($"Select * From vwQE_Tmk_TrademarkDocRespDocketingReassigned Where DriveItemId={driveItemId}").AsNoTracking().FirstOrDefaultAsync();
            }
            return qeView ?? new QETmkTrademarkDocRespDocketingView();
        }

        public async Task<QEGmMatterDocRespDocketingView> GetGmMatterDocRespDocketing(int docId = 0, string driveItemId = "")
        {
            var qeView = new QEGmMatterDocRespDocketingView();
            if (docId > 0)
            {
                qeView = await _dbContext.QEGmMatterDocRespDocketingView.Where(e => e.DocId == docId).AsNoTracking().FirstOrDefaultAsync();
            }
            else if (!string.IsNullOrEmpty(driveItemId))
            {
                qeView = await _dbContext.QEGmMatterDocRespDocketingView.Where(e => e.DriveItemId == driveItemId).AsNoTracking().FirstOrDefaultAsync();
            }
            return qeView ?? new QEGmMatterDocRespDocketingView();
        }
        public async Task<QEGmMatterDocRespDocketingView> GetGmMatterDocRespDocketingReassigned(int docId = 0, string driveItemId = "")
        {
            var qeView = new QEGmMatterDocRespDocketingView();
            if (docId > 0)
            {
                qeView = await _dbContext.QEGmMatterDocRespDocketingView.FromSqlInterpolated($"Select * From vwQE_GM_GMMatterDocRespDocketingReassigned Where DocId={docId}").AsNoTracking().FirstOrDefaultAsync();
            }
            else if (!string.IsNullOrEmpty(driveItemId))
            {
                qeView = await _dbContext.QEGmMatterDocRespDocketingView.FromSqlInterpolated($"Select * From vwQE_GM_GMMatterDocRespDocketingReassigned Where DriveItemId={driveItemId}").AsNoTracking().FirstOrDefaultAsync();
            }
            return qeView ?? new QEGmMatterDocRespDocketingView();
        }

        public async Task<QEPatCountryApplicationDocRespReportingView> GetPatCountryApplicationDocRespReporting(int docId = 0, string driveItemId = "")
        {
            var qeVM = new QEPatCountryApplicationDocRespReportingView();
            if (docId > 0)
            {
                qeVM = await _dbContext.QEPatCountryApplicationDocRespReportingView.Where(e => e.DocId == docId).AsNoTracking().FirstOrDefaultAsync();
            }
            else if (!string.IsNullOrEmpty(driveItemId))
            {
                qeVM = await _dbContext.QEPatCountryApplicationDocRespReportingView.Where(e => e.DriveItemId == driveItemId).AsNoTracking().FirstOrDefaultAsync();
            }
            return qeVM ?? new QEPatCountryApplicationDocRespReportingView();
        }
        public async Task<QEPatCountryApplicationDocRespReportingView> GetPatCountryApplicationDocRespReportingReassigned(int docId = 0, string driveItemId = "")
        {
            var qeVM = new QEPatCountryApplicationDocRespReportingView();
            if (docId > 0)
            {
                qeVM = await _dbContext.QEPatCountryApplicationDocRespReportingView.FromSqlInterpolated($"Select * From vwQE_Pat_CountryAppDocRespReportingReassigned Where DocId={docId}").AsNoTracking().FirstOrDefaultAsync();
            }
            else if (!string.IsNullOrEmpty(driveItemId))
            {
                qeVM = await _dbContext.QEPatCountryApplicationDocRespReportingView.FromSqlInterpolated($"Select * From vwQE_Pat_CountryAppDocRespReportingReassigned Where DriveItemId={driveItemId}").AsNoTracking().FirstOrDefaultAsync();
            }
            return qeVM ?? new QEPatCountryApplicationDocRespReportingView();
        }

        public async Task<QETmkTrademarkDocRespReportingView> GetTmkTrademarkDocRespReporting(int docId = 0, string driveItemId = "")
        {
            var qeVM = new QETmkTrademarkDocRespReportingView();
            if (docId > 0)
            {
                qeVM = await _dbContext.QETmkTrademarkDocRespReportingView.Where(e => e.DocId == docId).AsNoTracking().FirstOrDefaultAsync();
            }
            else if (!string.IsNullOrEmpty(driveItemId))
            {
                qeVM = await _dbContext.QETmkTrademarkDocRespReportingView.Where(e => e.DriveItemId == driveItemId).AsNoTracking().FirstOrDefaultAsync();
            }
            return qeVM ?? new QETmkTrademarkDocRespReportingView();
        }
        public async Task<QETmkTrademarkDocRespReportingView> GetTmkTrademarkDocRespReportingReassigned(int docId = 0, string driveItemId = "")
        {
            var qeVM = new QETmkTrademarkDocRespReportingView();
            if (docId > 0)
            {
                qeVM = await _dbContext.QETmkTrademarkDocRespReportingView.FromSqlInterpolated($"Select * From vwQE_Tmk_TrademarkDocRespReportingReassigned Where DocId={docId}").AsNoTracking().FirstOrDefaultAsync();
            }
            else if (!string.IsNullOrEmpty(driveItemId))
            {
                qeVM = await _dbContext.QETmkTrademarkDocRespReportingView.FromSqlInterpolated($"Select * From vwQE_Tmk_TrademarkDocRespReportingReassigned Where DriveItemId={driveItemId}").AsNoTracking().FirstOrDefaultAsync();
            }
            return qeVM ?? new QETmkTrademarkDocRespReportingView();
        }

        public async Task<QEGmMatterDocRespReportingView> GetGmMatterDocRespReporting(int docId = 0, string driveItemId = "")
        {
            var qeVM = new QEGmMatterDocRespReportingView();
            if (docId > 0)
            {
                qeVM = await _dbContext.QEGmMatterDocRespReportingView.Where(e => e.DocId == docId).AsNoTracking().FirstOrDefaultAsync();
            }
            else if (!string.IsNullOrEmpty(driveItemId))
            {
                qeVM = await _dbContext.QEGmMatterDocRespReportingView.Where(e => e.DriveItemId == driveItemId).AsNoTracking().FirstOrDefaultAsync();
            }
            return qeVM ?? new QEGmMatterDocRespReportingView();
        }
        public async Task<QEGmMatterDocRespReportingView> GetGmMatterDocRespReportingReassigned(int docId = 0, string driveItemId = "")
        {
            var qeVM = new QEGmMatterDocRespReportingView();
            if (docId > 0)
            {
                qeVM = await _dbContext.QEGmMatterDocRespReportingView.FromSqlInterpolated($"Select * From vwQE_GM_GMMatterDocRespReportingReassigned Where DocId={docId}").AsNoTracking().FirstOrDefaultAsync();
            }
            else if (!string.IsNullOrEmpty(driveItemId))
            {
                qeVM = await _dbContext.QEGmMatterDocRespReportingView.FromSqlInterpolated($"Select * From vwQE_GM_GMMatterDocRespReportingReassigned Where DriveItemId={driveItemId}").AsNoTracking().FirstOrDefaultAsync();
            }
            return qeVM ?? new QEGmMatterDocRespReportingView();
        }

        public async Task<QEDocVerificationImageView> GetDocVerificationImage(int docId)
        {
            var result = await _dbContext.QEDocVerificationImageView.Where(e => e.DocId == docId).AsNoTracking().FirstOrDefaultAsync();
            return result;
        }

        public async Task<QEPatRequestDocketView> GetPatRequestDocket(int reqId) {
            var result = await _dbContext.QEPatRequestDocketView.Where(e => e.ReqId == reqId).AsNoTracking().FirstOrDefaultAsync();
            return result;
        }
        public async Task<QETmkRequestDocketView> GetTmkRequestDocket(int reqId) {
            var result = await _dbContext.QETmkRequestDocketView.Where(e => e.ReqId == reqId).AsNoTracking().FirstOrDefaultAsync();
            return result;
        }
        public async Task<QEGmRequestDocketView> GetGmRequestDocket(int reqId) {
            var result = await _dbContext.QEGmRequestDocketView.Where(e => e.ReqId == reqId).AsNoTracking().FirstOrDefaultAsync();
            return result;
        }

        public async Task<string> GetRecipientEmail(string roleLink, QERoleSource roleSource)
        {
            var recipients = await GetRecipientRole(roleLink, roleSource.SourceSQL);
            if (recipients != null)
                return string.Join(";", recipients.Select(r => r.EntityEmail));
            //return recipient.EntityEmail;
            else
                return string.Empty;
        }

        public async Task<Dictionary<string, string>> GetRecipientNameAndEmail(string roleLink, QERoleSource roleSource)
        {
            var recipients = await GetRecipientRole(roleLink, roleSource.SourceSQL);
            if (recipients != null)
                return recipients.ToDictionary(k => string.IsNullOrEmpty(k.EntityFullName) ? k.EntityCode : k.EntityFullName, v => v.EntityEmail);
            else
                return null;
        }

        public async Task<List<QERecipientRoleDTO>> GetRecipientRole(string roleLink, string sourceSql)
        {
            if (string.IsNullOrEmpty(roleLink))
                return new List<QERecipientRoleDTO>();

            int parentId;
            if (int.TryParse(roleLink, out parentId))
            {
                var repository = sourceSql.Split(".");
                if (repository.Count() == 2)
                    return await GetRecipientRole(repository[0], repository[1], parentId);

                return new List<QERecipientRoleDTO>();
            }

            List<string> criteria = new List<string>();
            var roleLinks = roleLink.Split("|");
            foreach (var item in roleLinks)
            {
                var link = item.Split(",");
                criteria.Add($"(ParentTable='{link[0]}' And  ParentId={link[1]})");
            }
            string filter = string.Join(" OR ", criteria);
            var recipient = await _dbContext.QERecipientRoleDTO.FromSqlRaw($"Select * From ( {sourceSql} ) as T1 Where {filter}", sourceSql, filter).ToListAsync();
            return recipient;
        }

        private async Task<List<QERecipientRoleDTO>> GetRecipientRole(string serviceName, string methodName, int parentId)
        {
            string assemblyName = AppDomain.CurrentDomain.GetAssemblies()
                                        .ToList()
                                        .SelectMany(x => x.GetTypes())
                                        .Where(x => x.Name == serviceName)
                                        .Select(x => x.AssemblyQualifiedName)
                                        .FirstOrDefault();

            Type classType = Type.GetType(assemblyName);

            object viewModelService = _serviceProvider.GetRequiredService(classType);
            MethodInfo methodInfo = classType.GetMethod(methodName);

            dynamic awaitable;

            awaitable = methodInfo.Invoke(viewModelService, new object[] { parentId });

            await awaitable;
            var data = awaitable.GetAwaiter().GetResult();

            awaitable.Dispose();

            return data;
        }

        public IQueryable<QEImagesLinksDTO> GetImages(string imageTable, int parentId)
        {
            var images = _dbContext.DocEntityDTO.FromSqlRaw($"Select * From {imageTable}", imageTable)
                .Where(i => i.ParentId == parentId && string.IsNullOrEmpty(i.DocUrl))
                .AsNoTracking()
                .Select(e => new QEImagesLinksDTO
                {
                    FileId = e.FileId,
                    ParentId = e.ParentId,
                    FilePath = e.DocFileName,
                    ThumbnailFile = e.ThumbFileName,
                    ImageTitle = e.DocName,
                    //ImageTitle = e.UserFileName,
                    Remarks = e.Remarks,
                    IsPrivate = e.IsPrivate,
                    ScreenCode = e.ScreenCode,
                    IncludeInWorkflow = e.IncludeInWorkflow,
                    Tags = e.Tags
                });
            return images;
        }

        public IQueryable<DocDocument> GetDocDocuments()
        {
            return _dbContext.DocDocuments.AsNoTracking();
        }

        public async Task<List<QEFieldListDTO>> GetCustomFieldData(int dataSourceID, int parentId)
        {
            var result = await _dbContext.QEFieldListDTO.FromSqlInterpolated($"procQE_GetCustomFieldData @DataSourceId={dataSourceID}, @ParentId={parentId}")
                            .AsNoTracking().ToListAsync();
            return result;
        }


    }
}
