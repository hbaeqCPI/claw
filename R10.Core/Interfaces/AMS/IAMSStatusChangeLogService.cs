using R10.Core.Entities.AMS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Interfaces.AMS
{
    public interface IAMSStatusChangeLogService : IEntityService<AMSStatusChangeLog>
    {
        IQueryable<AMSStatusChangeLog> AMSStatusChangeLogs { get; }
        IQueryable<AMSStatusChangeLog> AMSStatusUpdateList { get; }
        Task<int> SaveStatusChangeLog(DateTime sendDate);
        string GetNewStatus(string instructionType, string instructionStatus
            , DateTime? filDate, DateTime? pubDate, DateTime? issDate);
        Task<byte[]> SaveProcessFlag(int id, bool processFlag, byte[] tStamp, string userName);
        Task<byte[]> SaveRemarks(int id, string remarks, byte[] tStamp, string userName);
        Task UpdateStatus(AMSStatusChangeLog updated);
    }
}
