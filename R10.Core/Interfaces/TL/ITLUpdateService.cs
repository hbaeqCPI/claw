using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using R10.Core.Entities.Trademark;
using R10.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Interfaces
{
    public interface ITLUpdateService
    {
        IQueryable<T> TLUpdates<T>() where T : TMSEntityFilter;
        Task<IQueryable<TLActionComparePTO>> TLActionUpdates();

        Task InitializeUpdate(string updateType);
        Task<List<TLCompareGoodsDTO>> CompareGoods(int tlTmkId);
        Task BiblioUpdateSetting(int tlTmkId, string fieldName, bool update, string tStamp);
        Task<int> UpdateBiblioRecord(int tlTmkId, string updatedBy);
        Task<int> UpdateBiblioRecords(TLUpdateCriteria criteria);
        Task<List<UpdateHistoryBatchDTO>> GetBiblioUpdHistoryBatches(int tmkId, int revertType);
        Task<List<TLBiblioUpdateHistory>> GetBiblioUpdHistory(int tmkId, int revertType, int jobId);
        Task<bool> UndoBiblio(int jobId, int tmkId, int logId, string updatedBy);
        Task<List<TLUpdateWorkflow>> GetUpdateWorkflowRecs(int jobId, int tlTmkId);

        Task TrademarkNameUpdateSetting(int tlTmkId, string fieldName, bool update, string tStamp);
        Task<bool> UpdateTrademarkNameRecord(int tlTmkId, string updatedBy);
        Task<bool> UpdateTrademarkNameRecords(TLUpdateCriteria criteria);
        Task<List<UpdateHistoryBatchDTO>> GetTrademarkNameUpdHistoryBatches(int tmkId, int revertType);
        Task<List<TLTmkNameUpdateHistory>> GetTrademarkNameUpdHistory(int tmkId, int revertType, int jobId);
        Task<bool> UndoTrademarkName(int jobId, int tmkId, int logId, string updatedBy);

        Task<bool> UpdateActionRecords(TLUpdateCriteria criteria);
        Task<List<UpdateHistoryBatchDTO>> GetActionUpdHistoryBatches(int tmkId, int revertType);
        Task<List<TLActionUpdateHistory>> GetActionUpdHistory(int tmkId, int revertType, int jobId);
        Task<bool> UndoActions(int jobId, int tmkId, int logId, string updatedBy);
        Task ActionUpdateSetting(int tlTmkId, string actionType, string actionDue, DateTime? baseDate,bool exclude, string userName);

        Task<List<UpdateHistoryBatchDTO>> GetGoodsUpdHistoryBatches(int tmkId, int revertType);
        Task<List<TLGoodsUpdateHistory>> GetGoodsUpdHistory(int tmkId, int revertType, int jobId);
        Task<bool> UndoGoods(int jobId, int tmkId, int logId, string updatedBy);

        IQueryable<Client> Clients { get; }
    }
}
