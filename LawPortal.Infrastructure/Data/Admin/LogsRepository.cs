using Microsoft.EntityFrameworkCore;
using LawPortal.Core.Entities;

namespace LawPortal.Infrastructure.Data.Admin
{
    public class LogsRepository : EFRepository<Log>
    {
        public LogsRepository(ApplicationDbContext dbContext) : base(dbContext) { }

        public IQueryable<Log> Logs => _dbContext.Logs.AsNoTracking();

    }
}
