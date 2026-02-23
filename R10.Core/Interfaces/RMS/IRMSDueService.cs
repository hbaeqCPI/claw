using R10.Core.Entities.RMS;
using R10.Core.Entities.Trademark;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Interfaces.RMS
{
    public interface IRMSDueService : IEntityService<RMSDue>
    {
        /// <summary>
        /// Trademark repository without filters
        /// </summary>
        IQueryable<TmkTrademark> Trademarks { get; }

        /// <summary>
        /// Parent trademark of instructable due dates
        /// </summary>
        IQueryable<TmkTrademark> TrademarkList { get; }
        IQueryable<TmkDueDate> TmkDueDateList { get; }
        IQueryable<TmkDueDate> InstructableList { get; }
        IQueryable<TmkDueDate> ActionClosingList { get; }
        IQueryable<RMSDueCountry> RMSDueCountries { get; }
        IQueryable<RMSInstrxChangeLog> InstrxChangeLogList { get; }
        IQueryable<RMSInstrxTypeActionDetail> ActionInstrxTypes { get; }
        IQueryable<RMSInstrxTypeActionDetail> TicklerActionInstrxTypes { get; }
        IQueryable<TmkOwner> OwnerList { get; }
        IQueryable<TmkTrademarkClass> ClassList { get; }
        Task<(int DueId, byte[] tStamp)> SaveInstruction(int ddId, string instructionType, string reason, string source, List<string> countries, byte[] tStamp, string userName);
        Task<byte[]> SaveInstructionRemarks(int ddId, string remarks, byte[] tStamp, string userName);
        Task<byte[]> MarkDeleted(int dueId, bool ignoreRecord, byte[] tStamp, string userName);
        Task SaveClientLastReminderDate(int remId, DateTime remDate);
        Task SaveClientReceiptLetterSentDate(int logId, DateTime letterSentDate, List<string> clientCodes);
        Task SaveClientInstructionSentToAgent(int logId, DateTime letterSentDate, List<string> agentCodes);
        Task SaveClientInstructionSentToAgent(List<RMSDue> rmsDues, DateTime letterSentDate);
        Task<byte[]> SaveNextRenewalDate(int ddId, DateTime? nextRenewalDate, byte[] tStamp, string userName);
        Task<byte[]> SaveAgentPaymentDate(int ddId, DateTime? agentPaymentDate, byte[] tStamp, string userName);
        Task<byte[]> SaveExclude(int ddId, bool value, byte[] tStamp, string userName);
    }
}
