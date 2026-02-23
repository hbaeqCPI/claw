using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using R10.Core.DTOs;
using R10.Core.Entities;

namespace R10.Core.Interfaces.Shared
{
    public interface IDocsOutService
    {

        Task<List<DocsOutDTO>> GetDocsOut(DocsOutCriteriaDTO criteria);
        Task<QELog> GetQELogByIdAsync(int id);
        Task<LetterLog> GetLetterLogByIdAsync(int id);
        Task<string> GetEFSLogFileNameByIdAsync(int id);
        Task<SystemScreen?> GetScreenInfo(int screenId);
        Task<RemLogEmailDetail?> GetRemLogEmailDetail<TDue, TRemLogDue>(int logEmailId) where TRemLogDue : RemLogDue;
        Task LetterLogDelete(int id);
        Task EFSLogDelete(int id);
        
    }
}
