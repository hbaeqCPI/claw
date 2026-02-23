using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using R10.Core.Entities.Trademark;

namespace R10.Core.Interfaces
{
    public interface ITLInfoRepository
    {
        Task<List<TLInfoSettingsMenu>> GetMenu(string country);
        TLSearchBiblioDTO GetBiblio(int tlTmkId);
        TLSearchImageDTO GetImage(int tlTmkId);
        Task<List<TLSearchAssignmentDTO>> GetAssignments(int tlTmkId);
        Task<List<TLSearchGoodsDTO>> GetGoods(int tlTmkId);
        Task<List<TLSearchActionAsDownloadedDTO>> GetActions(int tlTmkId, bool asDownloaded);
        Task<List<TLSearchDocDTO>> GetDocuments(int tlTmkId);
        Task<List<TLSearchTTABDTO>> GetTTABs(int tlTmkId);
        Task<bool> HasTL(string country);
        Task ClearPTOData(int tlTmkId);
        void MarkDocAsTransferred(string fileName);
        void MarkImageAsTransferred(string fileName);
        void MarkTLAutoDocketActionWorkflowAsGenerated(int actId);
        Task<List<EmailNotificationDTO>> GetTrademarkWatchRecipients(int tlTmkId, int docId);

        IQueryable<TLSearch> TLSearchRecords { get; }
        IQueryable<TLSearchDocument> TLSearchDocuments { get; }
        IQueryable<TLSearchImage> TLSearchImages { get; }
    }
}
