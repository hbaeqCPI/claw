using R10.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Interfaces
{
    public interface IClientContactService : IChildEntityService<Client, ClientContact>
    {
        Task<byte[]> SaveReminderOption(int clientContactID, ReminderOption option, bool value, byte[] tStamp, string userName);
        Task SaveLastReminderSentDate(int clientContactID, DateTime sentDate, string userName);
        Task SaveLastPrepayReminderSentDate(int clientContactID, DateTime sentDate, string userName);
        Task SaveLastConfirmationLetterSentDate(int clientContactID, DateTime sentDate, string userName);
        Task SaveLastRenewalConfirmationLetterSentDate(int clientContactID, DateTime sentDate, string userName);
        Task SaveLastRenewalReminderSentDate(int clientContactID, DateTime sentDate, string userName);
        Task SaveLastForeignFilingReminderSentDate(int clientContactID, DateTime sentDate, string userName);
        Task SaveLastForeignFilingConfirmationLetterSentDate(int clientContactID, DateTime sentDate, string userName);
    }
}
