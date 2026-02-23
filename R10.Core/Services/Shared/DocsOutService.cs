using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Interfaces;
using R10.Core.Interfaces.Shared;

namespace R10.Core.Services.Shared
{
    public class DocsOutService: IDocsOutService
    {
        protected readonly IDocsOutRepository _docsOutRepository;
        protected readonly ICPiDbContext _cpiDbContext;


        public DocsOutService(IDocsOutRepository docsOutRepository, ICPiDbContext cpiDbContext)
        {
            _docsOutRepository = docsOutRepository;
            _cpiDbContext = cpiDbContext;
        }

        public async Task<List<DocsOutDTO>> GetDocsOut(DocsOutCriteriaDTO criteria)
        {
            return await _docsOutRepository.GetDocsOut(criteria);
        }

        public async Task<QELog> GetQELogByIdAsync(int id)
        {
            return await _docsOutRepository.GetQELogByIdAsync(id);
        }

        public async Task<LetterLog> GetLetterLogByIdAsync(int id)
        {
            return await _docsOutRepository.GetLetterLogByIdAsync(id);
        }

        public async Task LetterLogDelete(int id)
        {
            await _docsOutRepository.LetterLogDelete(id);
        }

        public async Task EFSLogDelete(int id)
        {
            await _docsOutRepository.EFSLogDelete(id);
        }

        public async Task<string> GetEFSLogFileNameByIdAsync(int id)
        {
            return await _docsOutRepository.GetEFSLogFileNameByIdAsync(id);
        }

        public async Task<SystemScreen?> GetScreenInfo(int screenId)
        {
            return await _docsOutRepository.SystemScreens.FirstOrDefaultAsync(s=> s.ScreenId==screenId);
        }

        public async Task<RemLogEmailDetail?> GetRemLogEmailDetail<TDue, TRemLogDue>(int logEmailId) where TRemLogDue : RemLogDue
        {
            return await _cpiDbContext.GetReadOnlyRepositoryAsync<RemLogEmail<TDue, TRemLogDue>>()
                    .QueryableList
                    .Where(r => r.LogEmailId == logEmailId)
                    .Select(r => new RemLogEmailDetail()
                    {
                        LogEmailId = r.LogEmailId,
                        Attachment = r.Attachment,
                        Body = r.Body,
                        Client = r.Client,
                        ClientName = r.ClientName,
                        Contact = r.Contact,
                        ContactName = r.ContactName,
                        Email = r.Email,
                        RemId =r.RemId,
                        Sender = r.Sender,
                        SendOption = r.SendOption,
                        SentDate = r.SentDate,
                        Subject = r.Subject,
                        tStamp = r.tStamp
                    })
                    .FirstOrDefaultAsync();
        }
    }
}
