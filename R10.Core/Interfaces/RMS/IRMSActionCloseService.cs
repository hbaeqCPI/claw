using R10.Core.Entities.RMS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Interfaces.RMS
{
    public interface IRMSActionCloseService
    {
        string AbandonInstructionType { get; }
        string AbandonTrademarkStatus { get; }
        IQueryable<RMSActionCloseLog> RMSActionCloseLogs { get; }
        IQueryable<RMSActionCloseLogDue> RMSActionCloseLogDues { get; }
        IQueryable<RMSActionCloseLogEmail> RMSActionCloseLogEmails { get; }
        IQueryable<string> GetClients(int logId);
        IQueryable<string> GetAgents(int logId);
        Task<int> SaveActionCloseLog(IEnumerable<RMSActionCloseLogDue> details, DateTime closeDate, string filter, string userId);
        Task SaveRecipient(RMSActionCloseLogEmail recipient);
        Task SaveError(RMSActionCloseLogError error);
        Task<List<string>> CloseActions(IEnumerable<RMSActionCloseLogDue> details, DateTime closeDate, int logId, string userId);
    }
}
