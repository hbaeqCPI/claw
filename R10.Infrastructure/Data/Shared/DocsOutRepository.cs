using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Interfaces;

namespace R10.Infrastructure.Data
{
    public class DocsOutRepository: IDocsOutRepository
    {
        protected readonly ApplicationDbContext _dbContext;
        private readonly IAsyncRepository<QELog> _qeLogRepository;
        private readonly IAsyncRepository<LetterLog> _letLogRepository;
        protected readonly IAsyncRepository<EFSLog> _efsLogRepository;


        public DocsOutRepository(ApplicationDbContext dbContext, IAsyncRepository<QELog> qeLogRepository,
            IAsyncRepository<LetterLog> letLogRepository, IAsyncRepository<EFSLog> efsLogRepository)
        {
            _dbContext = dbContext;
            _qeLogRepository = qeLogRepository;
            _letLogRepository = letLogRepository;
            _efsLogRepository = efsLogRepository;
        }

        public async Task<List<DocsOutDTO>> GetDocsOut(DocsOutCriteriaDTO criteria)
        {
            var sql = SqlHelper.BuildSql("exec procSysDocsOut", criteria);
            var parameters = SqlHelper.BuildSqlParameters(criteria).ToArray();

            var result = await _dbContext.DocsOutDTO.FromSqlRaw(sql, parameters).AsNoTracking().ToListAsync();
            return result;
        }

        public async Task<QELog> GetQELogByIdAsync(int id)
        {
            return await _qeLogRepository.GetByIdAsync(id);
        }
        public async Task<LetterLog> GetLetterLogByIdAsync(int id)
        {
            return await _letLogRepository.GetByIdAsync(id);
        }

        public async Task LetterLogDelete(int id)
        {
            await _letLogRepository.QueryableList.Where(l=> l.LetLogId==id).ExecuteDeleteAsync();
        }
        
        public async Task EFSLogDelete(int id)
        {
            await _efsLogRepository.QueryableList.Where(l => l.EfsLogId == id).ExecuteDeleteAsync();
        }

        public async Task<string> GetEFSLogFileNameByIdAsync(int id)
        {
            var log = await _efsLogRepository.GetByIdAsync(id);
            return (log != null ? log.EfsFile : "");
        }

        public IQueryable<SystemScreen> SystemScreens => _dbContext.SystemScreens.AsNoTracking();

    }
}
