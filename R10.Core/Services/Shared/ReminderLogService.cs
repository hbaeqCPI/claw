using Microsoft.EntityFrameworkCore;
using R10.Core.Entities;
using R10.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Services.Shared
{
    public  abstract class ReminderLogService<TDue, TRemLogDue> : IReminderLogService<TDue, TRemLogDue> where TRemLogDue:RemLogDue
    {
        protected readonly ICPiDbContext _cpiDbContext;
        protected readonly ClaimsPrincipal _user;

        public IQueryable<RemLog<TDue, TRemLogDue>> RemLogs => _cpiDbContext.GetRepository<RemLog<TDue, TRemLogDue>>().QueryableList;

        public IQueryable<TRemLogDue> RemLogDues => _cpiDbContext.GetRepository<TRemLogDue>().QueryableList;

        public ReminderLogService(ICPiDbContext cpiDbContext, ClaimsPrincipal user)
        {
            _cpiDbContext = cpiDbContext;
            _user = user;
        }

        public async Task<int> SaveRemLog(IQueryable<TDue> dueDateList, DateTime remDate, string filter, string userId)
        {
            var remLog = new RemLog<TDue, TRemLogDue>()
            {
                RemDate = remDate,
                Filter = filter,
                UserId = userId
            };

            _cpiDbContext.GetRepository<RemLog<TDue, TRemLogDue>>().Add(remLog);
            AddRemLogDues(remLog, dueDateList);
            await _cpiDbContext.SaveChangesAsync();
            _cpiDbContext.Detach(remLog);

            return remLog.RemId;
        }

        public abstract void AddRemLogDues(RemLog<TDue, TRemLogDue> remLog, IQueryable<TDue> dueDateList);

        public async Task SaveRecipient(RemLogEmail<TDue, TRemLogDue> recipient)
        {
            _cpiDbContext.GetRepository<RemLogEmail<TDue, TRemLogDue>>().Add(recipient);
            await _cpiDbContext.SaveChangesAsync();
        }

        public async Task SaveError(RemLogError<TDue, TRemLogDue> error)
        {
            _cpiDbContext.GetRepository<RemLogError<TDue, TRemLogDue>>().Add(error);
            await _cpiDbContext.SaveChangesAsync();
        }

        public async Task UpdateStatus(RemLog<TDue, TRemLogDue> remLog, ReminderStatus status)
        {
            _cpiDbContext.GetRepository<RemLog<TDue, TRemLogDue>>().Update(remLog);
            remLog.Status = status;
            await _cpiDbContext.SaveChangesAsync();
            _cpiDbContext.Detach(remLog);
        }

        public async Task<ReminderStatus?> GetStatus(int remId)
        {
            var remLog = await RemLogs.FirstOrDefaultAsync(r => r.RemId == remId);
            return remLog?.Status;
        }
    }
}
