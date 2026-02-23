using Microsoft.EntityFrameworkCore;
using R10.Core.Entities;

namespace R10.Infrastructure.Data.Admin
{
    public class LogsRepository : EFRepository<Log>
    {
        public LogsRepository(ApplicationDbContext dbContext) : base(dbContext) { }

        public IQueryable<Log> Logs => _dbContext.Logs.AsNoTracking();

    }
}
