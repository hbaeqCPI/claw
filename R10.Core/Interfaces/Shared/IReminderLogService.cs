using R10.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Interfaces
{
    public interface IReminderLogService<TDue, TRemLogDue> where TRemLogDue:RemLogDue
    {
        IQueryable<RemLog<TDue, TRemLogDue>> RemLogs { get; }
        IQueryable<TRemLogDue> RemLogDues { get; }
        Task<int> SaveRemLog(IQueryable<TDue> dueDateList, DateTime remDate, string filter, string userId);
        Task SaveRecipient(RemLogEmail<TDue, TRemLogDue> recipient);
        Task SaveError(RemLogError<TDue, TRemLogDue> error);
        Task UpdateStatus(RemLog<TDue, TRemLogDue> remLog, ReminderStatus status);
        Task<ReminderStatus?> GetStatus(int remId);
    }
}
