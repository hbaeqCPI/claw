using R10.Core.DTOs;
using R10.Core.Entities.Trademark;

namespace R10.Core.Interfaces
{
    public interface ITLInfoService
    {
        Task<List<TLInfoSettingsMenu>> GetMenu(string country);
        TLSearchBiblioDTO GetCaseInfo(int tlTmkId);
        TLSearchImageDTO GetImage(int tlTmkId);
        Task<List<TLSearchAssignmentDTO>> GetAssignments(int tlTmkId);
        Task<List<TLSearchGoodsDTO>> GetGoods(int tlTmkId);
        Task<List<TLSearchActionAsDownloadedDTO>> GetActionsAsDownloaded(int tlTmkId);
        Task<List<TLSearchActionAsDownloadedDTO>> GetActionsAsMatched(int tlTmkId);
        Task<List<TLSearchDocDTO>> GetDocuments(int tlTmkId);
        Task<List<TLSearchTTABDTO>> GetTTABs(int tlTmkId);
        Task<bool> HasTL(string country);
        Task ClearPTOData(int tlTmkId);
        Task<List<TLSearchDocument>> GetUntransferredDocs();
        void MarkDocAsTransferred(string fileName);
        Task<List<TLSearchImage>> GetUntransferredImages();
        void MarkImageAsTransferred(string fileName);
        void MarkTLAutoDocketActionWorkflowAsGenerated(int actId);
        Task<List<EmailNotificationDTO>> GetTrademarkWatchRecipients(int tlTmkId, int docId);
        
        IQueryable<TmkTrademark> Trademarks { get; }
        IQueryable<TLSearch> TLSearchRecords { get; }
        IQueryable<TLSearchDocument> TLSearchApplicableIFWs { get; }
    }
}
