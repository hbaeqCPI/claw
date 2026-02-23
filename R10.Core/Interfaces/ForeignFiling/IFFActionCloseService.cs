using R10.Core.Entities.ForeignFiling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Interfaces.ForeignFiling
{
    public interface IFFActionCloseService
    {
        IQueryable<FFActionCloseLog> FFActionCloseLogs { get; }
        IQueryable<FFActionCloseLogDue> FFActionCloseLogDues { get; }
        IQueryable<FFActionCloseLogEmail> FFActionCloseLogEmails { get; }
        IQueryable<string> GetClients(int logId);
        IQueryable<string> GetAgents(int logId);
        Task<int> SaveActionCloseLog(IEnumerable<FFActionCloseLogDue> details, DateTime closeDate, string filter, string userId);
        Task SaveRecipient(FFActionCloseLogEmail recipient);
        Task SaveError(FFActionCloseLogError error);
        Task CloseActions(IEnumerable<FFActionCloseLogDue> details, DateTime closeDate, string userId);
    }
}
