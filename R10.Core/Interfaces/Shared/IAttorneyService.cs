using R10.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Interfaces
{
    public interface IAttorneyService : IEntityService<Attorney>
    {
        Task<byte[]> SaveReminderOption(int attorneyID, ReminderOption option, bool value, byte[] tStamp, string userName);
        Task SaveLastReminderSentDate(int attorneyID, DateTime sentDate, string userName);
        Task SaveLastPrepayReminderSentDate(int attorneyID, DateTime sentDate, string userName);
        Task SaveLastConfirmationLetterSentDate(int attorneyID, DateTime sentDate, string userName);
        IQueryable<Attorney> ClearanceQueryableList { get; }
        IQueryable<Attorney> QueryableListWithoutFilter { get; }
        Task<List<SysCustomFieldSetting>> GetCustomFields();

    }
}
