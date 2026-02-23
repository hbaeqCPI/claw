using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using R10.Core.Entities.ReportScheduler;
using R10.Core.Entities.Trademark;
using R10.Core.Exceptions;
using R10.Core.Helpers;
using R10.Core.Identity;
using R10.Core.Interfaces;
using R10.Core.Interfaces.Patent;
using R10.Core.Interfaces.Shared;
using R10.Core.Services.Shared;

namespace R10.Core.Services
{
    public class TLUpdateService : ITLUpdateService
    {
        private readonly IApplicationDbContext _repository;
        private readonly ITLUpdateRepository _tlUpdateRepository;
        private readonly INumberFormatService _numberFormatService;
        private readonly ISystemSettings<TLSetting> _settings;
        private readonly ClaimsPrincipal _user;
        private DateTime _actionCutOffDate;

        public TLUpdateService(IApplicationDbContext repository, ClaimsPrincipal user,
            ITLUpdateRepository tlUpdateRepository, INumberFormatService numberFormatService,
            ISystemSettings<TLSetting> settings)
        {
            _repository = repository;
            _tlUpdateRepository = tlUpdateRepository;
            _user = user;
            _numberFormatService = numberFormatService;
            _settings = settings;
        }

        public IQueryable<T> TLUpdates<T>() where T : TMSEntityFilter
        {
            var updates = _repository.Set<T>().AsNoTracking();

            if (_user.HasRespOfficeFilter(SystemType.Trademark))
                updates = updates.Where(RespOfficeFilter<T>());

            if (_user.HasEntityFilter())
                updates = updates.Where(EntityFilter<T>());
            return updates;
        }

        public async Task<IQueryable<TLActionComparePTO>> TLActionUpdates() 
        {
            var updates = TLUpdates<TLActionComparePTO>();

            var settings = await _settings.GetSetting();
            var cutOffMonths = settings.ActionCutOffDate;
            _actionCutOffDate = DateTime.Now.Date.AddMonths(cutOffMonths * -1);
            updates = updates.Where(ActionCutOffFilter());

            return updates;
        }

        #region Biblio
        public async Task InitializeUpdate(string updateType)
        {
            if (updateType == TLUpdateType.Biblio)
            {
                await StandardizeNumber();
                await _tlUpdateRepository.MarkBiblioDiscrepancies();
            }
        }

        public async Task StandardizeNumber()
        {
            var records = await _tlUpdateRepository.GetNumbersToStandardize();
            var country = "";
            var caseType = "";

            var appNoTemplates = new List<WebLinksNumberTemplateDTO>();
            var patNoTemplates = new List<WebLinksNumberTemplateDTO>();

            foreach (var item in records)
            {
                if (country != item.TMSCountry || caseType != item.TMSCaseType)
                {

                    // no Pubno in TL downloaded data
                    appNoTemplates = await _numberFormatService.GetNumberTemplates(WebLinksSystemType.Trademark, item.TMSCountry, item.TMSCaseType, WebLinksNumberType.AppNo, WebLinksTemplateType.User, "");
                    patNoTemplates = await _numberFormatService.GetNumberTemplates(WebLinksSystemType.Trademark, item.TMSCountry, item.TMSCaseType, WebLinksNumberType.PatRegNo, WebLinksTemplateType.User, "");
                    country = item.TMSCountry;
                    caseType = item.TMSCaseType;
                }

                try
                {
                    if (!string.IsNullOrEmpty(item.TMSAppNo))
                    {
                        var numberInfo = new WebLinksNumberInfoDTO
                        {
                            SystemType = WebLinksSystemType.Trademark,
                            Country = item.TMSCountry,
                            CaseType = item.TMSCaseType,
                            NumberType = WebLinksNumberType.AppNo,
                            Number = item.TMSAppNo,
                            NumberDate = item.TMSFilDate
                        };
                        var parsedInfo = _numberFormatService.ParseNumber(appNoTemplates, numberInfo);

                        if (parsedInfo != null && parsedInfo.Success)
                            item.TMSStdAppNo = parsedInfo.Number;
                        else
                        {
                            var number = Regex.Replace(item.TMSAppNo, @"[^\d]+", "");
                            if (number.Length > 7)
                                number = number.Right(7);

                            item.TMSStdAppNo = number.PadLeft(7, '0');
                        }

                    }
                    else
                        item.TMSStdAppNo = "";

                    if (!string.IsNullOrEmpty(item.TMSRegNo))
                    {
                        var numberInfo = new WebLinksNumberInfoDTO
                        {
                            SystemType = WebLinksSystemType.Trademark,
                            Country = item.TMSCountry,
                            CaseType = item.TMSCaseType,
                            NumberType = WebLinksNumberType.PatRegNo,
                            Number = item.TMSRegNo,
                            NumberDate = item.TMSRegDate
                        };
                        var parsedInfo = _numberFormatService.ParseNumber(patNoTemplates, numberInfo);

                        if (parsedInfo != null && parsedInfo.Success)
                            item.TMSStdRegNo = parsedInfo.Number;
                        else
                        {
                            var number = Regex.Replace(item.TMSRegNo, @"[^\d]+", "");
                            if (number.Length > 7)
                                number = number.Right(7);

                            item.TMSStdRegNo = number.PadLeft(7, '0');
                        }

                    }
                    else
                        item.TMSStdRegNo = "";


                }

                //for debugging
                catch (Exception ex)
                {
                    throw ex;
                }

            }
            if (records.Any())
                await _tlUpdateRepository.SaveStandardNumber(records);

        }


        public async Task<List<TLCompareGoodsDTO>> CompareGoods(int tlTmkId)
        {
            return await _tlUpdateRepository.CompareGoods(tlTmkId);
        }

        public async Task BiblioUpdateSetting(int tlTmkId, string fieldName, bool update, string tStamp)
        {
            var tlSearch = new TLSearch { TLTmkId = tlTmkId, tStamp = Convert.FromBase64String(tStamp) };
            var entity = _repository.TLSearchRecords.Attach(tlSearch);

            switch (fieldName)
            {
                case TLBiblioUpdateField.UpdateAppNo:
                    tlSearch.UpdateAppNo = update;
                    entity.Property(s => s.UpdateAppNo).IsModified = true;
                    break;
                case TLBiblioUpdateField.UpdatePubNo:
                    tlSearch.UpdatePubNo = update;
                    entity.Property(s => s.UpdatePubNo).IsModified = true;
                    break;
                case TLBiblioUpdateField.UpdateRegNo:
                    tlSearch.UpdateRegNo = update;
                    entity.Property(s => s.UpdateRegNo).IsModified = true;
                    break;
                case TLBiblioUpdateField.UpdateFilDate:
                    tlSearch.UpdateFilDate = update;
                    entity.Property(s => s.UpdateFilDate).IsModified = true;
                    break;
                case TLBiblioUpdateField.UpdatePubDate:
                    tlSearch.UpdatePubDate = update;
                    entity.Property(s => s.UpdatePubDate).IsModified = true;
                    break;
                case TLBiblioUpdateField.UpdateRegDate:
                    tlSearch.UpdateRegDate = update;
                    entity.Property(s => s.UpdateRegDate).IsModified = true;
                    break;
                case TLBiblioUpdateField.UpdateAllowanceDate:
                    tlSearch.UpdateAllowanceDate = update;
                    entity.Property(s => s.UpdateAllowanceDate).IsModified = true;
                    break;
                case TLBiblioUpdateField.UpdateNextRenewalDate:
                    tlSearch.UpdateNextRenewalDate = update;
                    entity.Property(s => s.UpdateNextRenewalDate).IsModified = true;
                    break;
                case TLBiblioUpdateField.UpdateGoods:
                    tlSearch.UpdateGoods = update;
                    entity.Property(s => s.UpdateGoods).IsModified = true;
                    break;
                case TLBiblioUpdateField.Exclude:
                    tlSearch.Exclude = update;
                    entity.Property(s => s.Exclude).IsModified = true;
                    break;
            }
            await _repository.SaveChangesAsync();

        }

        public async Task<int> UpdateBiblioRecord(int tlTmkId, string updatedBy)
        {
            return await _tlUpdateRepository.UpdateBiblioRecord(tlTmkId, updatedBy);
        }

        public async Task<int> UpdateBiblioRecords(TLUpdateCriteria criteria)
        {
            return await _tlUpdateRepository.UpdateBiblioRecords(criteria);
        }

        public async Task<List<TLUpdateWorkflow>> GetUpdateWorkflowRecs(int jobId, int tlTmkId) { 
            return await _tlUpdateRepository.GetUpdateWorkflowRecs(jobId, tlTmkId);
        }

        public async Task<List<UpdateHistoryBatchDTO>> GetBiblioUpdHistoryBatches(int tmkId,  int revertType)
        {
            return await _repository.TLBiblioUpdatesHistory.Where(h => (tmkId == 0 || h.TMSTmkId == tmkId) && (revertType == 2 || h.Reverted == revertType))
                 .Select(h => new UpdateHistoryBatchDTO { JobId = h.JobId, ChangeDate = h.ChangeDate }).Distinct().ToListAsync();
        }

        public async Task<List<TLBiblioUpdateHistory>> GetBiblioUpdHistory(int tmkId, int revertType, int jobId)
        {
            return await _repository.TLBiblioUpdatesHistory.Where(h => (tmkId == 0 || h.TMSTmkId == tmkId) && (revertType == 2 || h.Reverted == revertType) && (jobId == 0 || h.JobId == jobId)).ToListAsync();
        }

        public async Task<bool> UndoBiblio(int jobId, int tmkId, int logId, string updatedBy)
        {
            return await _tlUpdateRepository.UndoBiblio(jobId, tmkId, logId, updatedBy);
        }

        #endregion

        #region TrademarkName
        public async Task TrademarkNameUpdateSetting(int tlTmkId, string fieldName, bool update, string tStamp)
        {
            var tlSearch = new TLSearch { TLTmkId = tlTmkId, tStamp = Convert.FromBase64String(tStamp) };
            var entity = _repository.TLSearchRecords.Attach(tlSearch);

            switch (fieldName)
            {
                case TLTrademarkNameUpdateField.Update:
                    tlSearch.UpdateTrademarkName = update;
                    entity.Property(s => s.UpdateTrademarkName).IsModified = true;
                    break;

                case TLTrademarkNameUpdateField.Exclude:
                    tlSearch.ExcludeTrademarkName = update;
                    entity.Property(s => s.ExcludeTrademarkName).IsModified = true;
                    break;
            }
            await _repository.SaveChangesAsync();

        }

        public async Task<bool> UpdateTrademarkNameRecord(int tlTmkId, string updatedBy)
        {
            return await _tlUpdateRepository.UpdateTrademarkNameRecord(tlTmkId, updatedBy);
        }

        public async Task<bool> UpdateTrademarkNameRecords(TLUpdateCriteria criteria)
        {
            return await _tlUpdateRepository.UpdateTrademarkNameRecords(criteria);
        }

        public async Task<List<UpdateHistoryBatchDTO>> GetTrademarkNameUpdHistoryBatches(int tmkId, int revertType)
        {
            return await _repository.TLTmkNameUpdatesHistory.Where(h => (tmkId == 0 || h.TMSTmkId == tmkId) && (revertType == 2 || h.Reverted == revertType))
                 .Select(h => new UpdateHistoryBatchDTO { JobId = h.JobId, ChangeDate = h.ChangeDate }).Distinct().ToListAsync();
        }

        public async Task<List<TLTmkNameUpdateHistory>> GetTrademarkNameUpdHistory(int tmkId, int revertType, int jobId)
        {
            return await _repository.TLTmkNameUpdatesHistory.Where(h => (tmkId == 0 || h.TMSTmkId == tmkId) && (revertType == 2 || h.Reverted == revertType) && (jobId == 0 || h.JobId == jobId)).ToListAsync();
        }

        public async Task<bool> UndoTrademarkName(int jobId, int tmkId, int logId, string updatedBy)
        {
            return await _tlUpdateRepository.UndoTrademarkName(jobId, tmkId, logId, updatedBy);
        }
        #endregion

        #region Actions
        public async Task<bool> UpdateActionRecords(TLUpdateCriteria criteria) {
            return await _tlUpdateRepository.UpdateActionRecords(criteria);
        }
        public async Task<List<UpdateHistoryBatchDTO>> GetActionUpdHistoryBatches(int tmkId,
            int revertType)  {
            return await _repository.TLActionUpdatesHistory.Where(h => (tmkId == 0 || h.TMSTmkId == tmkId) && (revertType == 2 || h.Reverted == revertType))
                 .Select(h => new UpdateHistoryBatchDTO { JobId = h.JobId, ChangeDate = h.ChangeDate}).Distinct().ToListAsync();
        }

        public async Task<List<TLActionUpdateHistory>> GetActionUpdHistory(int tmkId, int revertType, int jobId)
        {
            return await _repository.TLActionUpdatesHistory.Where(h => (tmkId == 0 || h.TMSTmkId == tmkId) && (revertType == 2 || h.Reverted == revertType) && (jobId == 0 || h.JobId == jobId)).ToListAsync();
        }

        public async Task<bool> UndoActions(int jobId, int tmkId, int logId, string updatedBy)
        {
            return await _tlUpdateRepository.UndoActions(jobId, tmkId, logId, updatedBy);
        }

        public async Task ActionUpdateSetting(int tlTmkId, string actionType, string actionDue, DateTime? baseDate, bool exclude,string userName)
        {
            if (exclude)
            {
                var excludeRec = new TLActionUpdateExclude {TLTmkID = tlTmkId,
                    ActionType = actionType,
                    ActionDue = actionDue,
                    BaseDate = baseDate,
                    CreatedBy=userName,
                    DateCreated=DateTime.Now
                };
                _repository.TLActionUpdateExcludes.Add(excludeRec);
                await _repository.SaveChangesAsync();
            }
            else {
                await _repository.TLActionUpdateExcludes.Where(a => a.TLTmkID == tlTmkId && a.ActionType==actionType && a.ActionDue==actionDue && a.BaseDate==baseDate).ExecuteDeleteAsync();
            }

        }
        #endregion

        #region Goods
        public async Task<List<UpdateHistoryBatchDTO>> GetGoodsUpdHistoryBatches(int tmkId, int revertType)
        {
            return await _repository.TLGoodsUpdatesHistory.Where(h => (tmkId == 0 || h.TMSTmkId == tmkId) && (revertType == 2 || h.Reverted == revertType))
                 .Select(h => new UpdateHistoryBatchDTO { JobId = h.JobId, ChangeDate = h.ChangeDate }).Distinct().ToListAsync();
        }

        public async Task<List<TLGoodsUpdateHistory>> GetGoodsUpdHistory(int tmkId, int revertType, int jobId)
        {
            return await _repository.TLGoodsUpdatesHistory.Where(h => (tmkId == 0 || h.TMSTmkId == tmkId) && (revertType == 2 || h.Reverted == revertType) && (jobId == 0 || h.JobId == jobId)).ToListAsync();
        }

        public async Task<bool> UndoGoods(int jobId, int tmkId, int logId, string updatedBy)
        {
            return await _tlUpdateRepository.UndoGoods(jobId, tmkId, logId, updatedBy);
        }
        #endregion

        public IQueryable<Client> Clients => _repository.Clients.AsNoTracking();

        protected Expression<Func<T, bool>> RespOfficeFilter<T>() where T : TMSEntityFilter
        {
            return a => _repository.CPiUserSystemRoles.AsNoTracking().Any(r => r.UserId == _user.GetUserIdentifier() && r.SystemId == SystemType.Trademark && a.RespOffice == r.RespOffice);
        }

        protected Expression<Func<T, bool>> EntityFilter<T>() where T : TMSEntityFilter
        {
            string userIdentifier = _user.GetUserIdentifier();
            var userEntityFilters = _repository.CPiUserEntityFilters.AsNoTracking();

            switch (_user.GetEntityFilterType())
            {
                case CPiEntityType.Client:
                    return a => userEntityFilters.Any(f =>
                        f.UserId == userIdentifier && f.EntityId == a.ClientId);

                case CPiEntityType.Agent:
                    return a => userEntityFilters.Any(f => f.UserId == userIdentifier && f.EntityId == a.AgentId);

                case CPiEntityType.Owner:
                    return a => userEntityFilters.Any(f => f.UserId == userIdentifier && _repository.TmkOwners.Any(o => o.OwnerID == f.EntityId && o.TmkID == a.TMSTmkId));

                case CPiEntityType.Attorney:
                    return a => userEntityFilters.Any(f =>
                        f.UserId == userIdentifier && (f.EntityId == a.Attorney1Id ||
                                                       f.EntityId == a.Attorney2Id ||
                                                       f.EntityId == a.Attorney3Id));
            }
            return null;
        }

        protected Expression<Func<TLActionComparePTO, bool>> ActionCutOffFilter()
        {
            return a => a.DueDate >= _actionCutOffDate;
        }


    }


    public class TLUpdateType
    {
        public const string Biblio = "Biblio";
        public const string Action = "Action";
        public const string TrademarkName = "TmkName";
        public const string Goods = "Goods";
    }

    public class TLUpdateCriteria
    {
        public string? UpdatedBy { get; set; }
        public DateTime? LastWebUpdateFrom { get; set; }
        public DateTime? LastWebUpdateTo { get; set; }
        public string? TMSCaseNumber { get; set; }
        public string? Client { get; set; }
        public string? ActionType { get; set; }
        public bool ActiveSwitch { get; set; }
        public string? TMSCountry { get; set; }
    }

    public class TLBiblioUpdateField
    {
        public const string UpdateAppNo = "UpdateAppNo";
        public const string UpdatePubNo = "UpdatePubNo";
        public const string UpdateRegNo = "UpdateRegNo";
        public const string UpdateFilDate = "UpdateFilDate";
        public const string UpdatePubDate = "UpdatePubDate";
        public const string UpdateRegDate = "UpdateRegDate";
        public const string UpdateAllowanceDate = "UpdateAllowanceDate";
        public const string UpdateGoods = "UpdateGoods";
        public const string Exclude = "Exclude";
        public const string UpdateNextRenewalDate = "UpdateNextRenewalDate";
    }

    public class TLTrademarkNameUpdateField
    {
        public const string Update = "Update";
        public const string Exclude = "Exclude";
    }

}
