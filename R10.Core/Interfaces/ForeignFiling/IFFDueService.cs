using R10.Core.Entities.ForeignFiling;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Interfaces.ForeignFiling
{
    public interface IFFDueService : IEntityService<FFDue>
    {
        IQueryable<CountryApplication> CountryApplicationList { get; }
        IQueryable<PatDueDate> PatDueDateList { get; }
        IQueryable<PatDueDate> InstructableList { get; }
        IQueryable<PatDueDate> ActionClosingList { get; }
        IQueryable<FFInstrxChangeLog> InstrxChangeLogList { get; }
        IQueryable<FFInstrxTypeActionDetail> ActionInstrxTypes { get; }
        IQueryable<FFInstrxTypeActionDetail> TicklerActionInstrxTypes { get; }

        Task<bool> IsForeignFiling(int ddId);

        Task<(int DueId, byte[] tStamp)> SaveInstruction(int dueId, string instructionType, string reason, string source, 
            List<string> ctryEP, List<string> ctryWO, List<string> ctryAll, byte[] tStamp, string userName);
        Task<byte[]> SaveInstructionRemarks(int ddId, string remarks, byte[] tStamp, string userName);

        Task SaveClientLastReminderDate(int remId, DateTime remDate);
        Task SaveClientReceiptLetterSentDate(int logId, DateTime letterSentDate, List<string> clientCodes);
        Task SaveClientInstructionSentToAgent(int logId, DateTime letterSentDate, List<string> agentCodes);

        Task<byte[]> SaveExclude(int ddId, bool value, byte[] tStamp, string userName);
    }
}
