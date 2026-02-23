using R10.Core.Entities.AMS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Interfaces.AMS
{
    public interface IAMSInstrxCPiLogService
    {
        IQueryable<AMSInstrxCPiLog> AMSInstrxCPiLogs { get; }
        IQueryable<AMSInstrxCPiLogDetail> AMSInstrxCPiLogDetails { get; }
        IQueryable<AMSInstrxCPiLogEmail> AMSInstrxCPiLogEmails { get; }
        IQueryable<string> GetClients(int sendId);
        IQueryable<string> GetAgents(int sendId);
        IQueryable<string> GetAttorneys(int sendId);
        Task<int> SaveInstrxCPiLog(IEnumerable<AMSInstrxCPiLogDetail> details, DateTime sendToCPiDate, string userId);
        Task SaveRecipient(AMSInstrxCPiLogEmail recipient);
        Task SaveError(AMSInstrxCPiLogError error);
    }
}
