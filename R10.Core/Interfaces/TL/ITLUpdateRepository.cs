using R10.Core.DTOs;
using R10.Core.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace R10.Core.Interfaces
{
    public interface ITLUpdateRepository
    {
        Task<List<TLNumberFormatDTO>> GetNumbersToStandardize();
        Task SaveStandardNumber(List<TLNumberFormatDTO> numbers);
        Task<List<TLCompareGoodsDTO>> CompareGoods(int tlTmkId);
        Task<int> UpdateBiblioRecord(int tlTmkId, string updatedBy);
        Task<int> UpdateBiblioRecords(TLUpdateCriteria criteria);
        Task MarkBiblioDiscrepancies();
        Task<bool> UndoBiblio(int jobId, int tmkId, int logId, string updatedBy);
        Task<bool> UndoGoods(int jobId, int tmkId, int logId, string updatedBy);
        Task<List<TLUpdateWorkflow>> GetUpdateWorkflowRecs(int jobId, int tlTmkId);

        Task<bool> UpdateTrademarkNameRecord(int tlTmkId, string updatedBy);
        Task<bool> UpdateTrademarkNameRecords(TLUpdateCriteria criteria);
        Task<bool> UndoTrademarkName(int jobId, int tmkId, int logId, string updatedBy);

        Task<bool> UpdateActionRecords(TLUpdateCriteria criteria);
        Task<bool> UndoActions(int jobId, int tmkId, int logId, string updatedBy);
    }
}
