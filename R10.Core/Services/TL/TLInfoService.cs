using R10.Core.Interfaces;
using System.Threading.Tasks;
using System.Collections.Generic;
using R10.Core.Entities;
using System.Linq;
using R10.Core.Entities.Trademark;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using R10.Core.DTOs;
using R10.Core.Exceptions;
using R10.Core.Helpers;
using R10.Core.Entities.Patent;

namespace R10.Core.Services
{
    public class TLInfoService : ITLInfoService
    {
        private readonly ITLInfoRepository _tlInfoRepository;
        private readonly ITmkTrademarkService _trademarkService;
        readonly ClaimsPrincipal _user;

        public TLInfoService(ITLInfoRepository tlInfoRepository, ITmkTrademarkService trademarkService,
            ClaimsPrincipal user)
        {
            _tlInfoRepository = tlInfoRepository;
            _trademarkService = trademarkService;
            _user = user;
        }

        public async Task<List<TLInfoSettingsMenu>> GetMenu(string country)
        {
            return await _tlInfoRepository.GetMenu(country);
        }

        public TLSearchBiblioDTO GetCaseInfo(int tlTmkId)
        {
            var biblio =  _tlInfoRepository.GetBiblio(tlTmkId);
            return biblio;
        }

        public TLSearchImageDTO GetImage(int tlTmkId)
        {
            var image =  _tlInfoRepository.GetImage(tlTmkId);
            return image;
        }

        public async Task<List<TLSearchAssignmentDTO>> GetAssignments(int tlTmkId)
        {
            var assignments = await _tlInfoRepository.GetAssignments(tlTmkId);
            return assignments;
        }

        public async Task<List<TLSearchGoodsDTO>> GetGoods(int tlTmkId)
        {
            var goods = await _tlInfoRepository.GetGoods(tlTmkId);
            return goods;
        }

        public async Task<List<TLSearchActionAsDownloadedDTO>> GetActions(int tlTmkId, bool asDownloaded)
        {
            var actions = await _tlInfoRepository.GetActions(tlTmkId, asDownloaded);
            return actions;
        }

        public async Task<List<TLSearchActionAsDownloadedDTO>> GetActionsAsDownloaded(int tlTmkId)
        {
            await ValidateRecordFilterPermission(tlTmkId);
            return await _tlInfoRepository.GetActions(tlTmkId, true);
        }

        public async Task<List<TLSearchActionAsDownloadedDTO>> GetActionsAsMatched(int tlTmkId)
        {
            await ValidateRecordFilterPermission(tlTmkId);
            return await _tlInfoRepository.GetActions(tlTmkId, false);
        }

        public async Task<List<TLSearchDocDTO>> GetDocuments(int tlTmkId)
        {
            var docs = await _tlInfoRepository.GetDocuments(tlTmkId);
            return docs;
        }

        public async Task<List<TLSearchTTABDTO>> GetTTABs(int tlTmkId)
        {
            var ttabs = await _tlInfoRepository.GetTTABs(tlTmkId);
            return ttabs;
        }

        public async Task<bool> HasTL(string country) {
            return await _tlInfoRepository.HasTL(country);
        }

        public async Task ClearPTOData(int tlTmkId) {
            await _tlInfoRepository.ClearPTOData(tlTmkId);
        }

        public IQueryable<TLSearch> TLSearchRecords
        {
            get
            {
                var list = _tlInfoRepository.TLSearchRecords;
                if (_user.HasRespOfficeFilter(SystemType.Trademark) || _user.HasEntityFilter())
                {
                    list = list.Where(s => this.Trademarks.Any(c => c.TmkId == s.TMSTmkId));
                }
                return list;
            }
        }

        //no resp office/entity filter
        public IQueryable<TLSearchDocument> TLSearchApplicableIFWs
        {
            get
            {
                var list = _tlInfoRepository.TLSearchDocuments.Where(i =>  i.FileName.Length > 0 && (bool)i.Transferred).AsNoTracking();
                return list;
            }
        }

        public async Task<List<TLSearchDocument>> GetUntransferredDocs()
        {
            return await _tlInfoRepository.TLSearchDocuments.Where(i => i.FileName.Length > 0 && (!(bool)i.Transferred || i.Transferred == null))
                            .Select(i =>
                            new TLSearchDocument
                            {
                                TLTmkId = i.TLTmkId,
                                FileName = i.FileName
                            }).Distinct().ToListAsync();
        }

        public void MarkDocAsTransferred(string fileName)
        {
            _tlInfoRepository.MarkDocAsTransferred(fileName);
        }

        public async Task<List<TLSearchImage>> GetUntransferredImages()
        {
            return await _tlInfoRepository.TLSearchImages.Where(i =>  (!(bool)i.Transferred || i.Transferred == null))
                            .Select(i =>
                            new TLSearchImage
                            {
                                TLTmkId = i.TLSearch.TMSTmkId,
                                OrigFileName = i.OrigFileName
                            }).Distinct().ToListAsync();
        }

        public void MarkImageAsTransferred(string fileName)
        {
            _tlInfoRepository.MarkImageAsTransferred(fileName);
        }

        private async Task ValidateRecordFilterPermission(int tlTmkId)
        {
            if (_user.HasRespOfficeFilter(SystemType.Trademark) || _user.HasEntityFilter())
            {
                Guard.Against.NoRecordPermission(await TLSearchRecords.AnyAsync(s => this.Trademarks.Any(c => c.TmkId == s.TMSTmkId)));
            }
        }

        public void MarkTLAutoDocketActionWorkflowAsGenerated(int actId)
        {
            _tlInfoRepository.MarkTLAutoDocketActionWorkflowAsGenerated(actId);
        }

        public async Task<List<EmailNotificationDTO>> GetTrademarkWatchRecipients(int tlTmkId, int docId)
        { 
            return await _tlInfoRepository.GetTrademarkWatchRecipients(tlTmkId,docId);
        }
        public IQueryable<TmkTrademark> Trademarks => _trademarkService.TmkTrademarks;
    }
}
