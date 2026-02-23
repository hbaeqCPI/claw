using R10.Core.Entities;
using R10.Core.Entities.AMS;
using R10.Core.Queries.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Interfaces.AMS
{
    public interface IAMSDueService : IChildEntityService<AMSMain, AMSDue>
    {        
        IQueryable<AMSDue> InstructableList { get; }
        IQueryable<AMSDue> PortfolioReviewList { get; }
        IQueryable<AMSDue> InGracePeriodList { get; }
        IQueryable<AMSDue> PastDueWithGraceDateList { get; }
        IQueryable<AMSDue> InstructionsToCPiList { get; }
        IQueryable<AMSProjection> ProjectionList { get; }
        IQueryable<AMSInstrxChangeLog> InstrxChangeLogList { get; }
        IQueryable<AMSInstrxDecisionMgt> InstrxDecisionMgtList { get; }
        IQueryable<ClientContact> DecisionMakers { get; }
        IQueryable<AMSDue> PaidWithNoPostedReceiptList { get; }

        //Expression<Func<AMSDue, bool>> NeedsInstruction { get; }
        //Expression<Func<AMSDue, bool>> ExcludeInstructed { get; }
        Task<Expression<Func<AMSDue, bool>>> ExcludeInstructed();
        Expression<Func<AMSDue, bool>> ExcludeCPIInstructed { get; }
        Expression<Func<AMSDue, bool>> ExcludeNP { get; }
        Expression<Func<AMSDue, bool>> ClientPaymentNeeded { get; }

        Task<IQueryable<AMSDue>> GetAnnuities(bool outstandingOnly);
        Task<IQueryable<AMSDue>> GetInstructionsToCPi();
        IQueryable<ClientContact> GetDecisionMakers(int clientId);

        bool IsInGracePeriod(DateTime? graceDate, DateTime? paymentDate);
        bool IsInstructable(DateTime? dueDate, DateTime? paymentDate, DateTime? stopDate, DateTime? graceDate,
            string caseType, DateTime? issDate, string country, string status, DateTime? taxStartDate);
        bool IsClientPaymentNeeded(string instructionType, bool? payBeforeSending);

        decimal GetAnnuityCost(string paymentType, decimal? invoiceAmount, decimal? reviewAmount, decimal? reminderFaxAmount, decimal? settleAmount, decimal? secondReminderFaxAmount, decimal? serviceFee, decimal? vatAmount);

        Task<byte[]> SaveInstruction(int dueId, string instructionType, string reason, string source, byte[] tStamp, string userName);
        Task<byte[]> SaveInstructionRemarks(int dueId, string remarks, byte[] tStamp, string userName);
        Task<byte[]> MarkDeleted(int dueId, bool ignoreRecord, byte[] tStamp, string userName);

        Task<byte[]> SaveClientPaymentDate(int dueId, DateTime? paymentDate, byte[] tStamp, string userName);

        Task SaveClientLastReminderDate(int remId, DateTime remDate);
        Task SaveClientLastPrepayReminderDate(int remId, DateTime remDate);
        Task SaveClientReceiptLetterSentDate(int sendId, DateTime letterSentDate, List<string> clientCodes);
        Task SaveClientInstructionSentToAgent(int sendId, DateTime letterSentDate, List<string> agentCodes);

        Task SaveDecisionMgtInstructions(List<AMSInstrxDecisionMgt> instructions);

        /// <summary>
        /// Check if current user is a decision management user.
        /// </summary>
        /// <returns></returns>
        Task<bool> IsDecisionManagementUser();

        /// <summary>
        /// Check if instruction needs decision management.
        /// </summary>
        /// <returns></returns>
        Task<bool> IsUseDecisionManagement(int dueId);

        /// <summary>
        /// Get list of client codes where current user is a decision maker.
        /// </summary>
        /// <returns></returns>
        Task<List<string>> GetDecisionMakerClients();

        /// <summary>
        /// Get list of dueIds with decision conflict
        /// </summary>
        /// <param name="dueIds"></param>
        /// <returns></returns>
        Task<List<int>> GetDecisionConflicts(List<int> dueIds);

        /// <summary>
        /// Get list of dueIds with decision conflict
        /// </summary>
        /// <returns></returns>
        Task<List<int>> GetDecisionConflicts();

        Task<int> GetUserEntityId();
        Task<List<string>> GetDecisionMakerCpiCodes();

        Task<bool> IsProductsOn();
        Task<bool> IsLicenseesOn();
        Task<bool> IsPatentScoreOn();
    }
}
