using Microsoft.EntityFrameworkCore;
using R10.Core.Entities;
using R10.Core.Exceptions;
using R10.Core.Helpers;
using R10.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Services.Shared
{
    public class ClientContactService : EntityContactService<Client, ClientContact>, IClientContactService
    {
        public ClientContactService(ICPiDbContext cpiDbContext, ClaimsPrincipal user) : base(cpiDbContext, user)
        {
        }

        public override async Task<ClientContact> GetByIdAsync(int clientContactID)
        {
            return await QueryableList.SingleOrDefaultAsync(c => c.ClientContactID == clientContactID);
        }

        public async Task<byte[]> SaveReminderOption(int clientContactID, ReminderOption option, bool value, byte[] tStamp, string userName)
        {
            Guard.Against.NoRecordPermission(
                            (await ValidatePermission(SystemType.Shared, CPiPermissions.FullModify, "")) ||
                            (await ValidatePermission(SystemType.AMS, CPiPermissions.FullModify, "")) ||
                            (await ValidatePermission(SystemType.ForeignFiling, CPiPermissions.FullModify, "")) ||
                            (await ValidatePermission(SystemType.RMS, CPiPermissions.FullModify, ""))
                            );

            var updated = await GetByIdAsync(clientContactID);

            Guard.Against.NoRecordPermission(updated != null);
            if (EntityType.IsEntityFilterType(typeof(Client), _user.GetEntityFilterType()))
                Guard.Against.NoRecordPermission(await base.EntityFilterAllowed(updated.ClientID));

            updated.tStamp = tStamp;
            _cpiDbContext.GetRepository<ClientContact>().Attach(updated);

            if (option == ReminderOption.ReceiveReminderOnline)
                updated.ReceiveReminderOnline = value;
            else if (option == ReminderOption.ReceiveReminderReport)
                updated.ReceiveReminderReport = value;
            else if (option == ReminderOption.ReceivePrepayReminder)
                updated.ReceivePrepayReminder = value;
            else if (option == ReminderOption.ReceiveRMSReminder)
                updated.RMSReceiveReminder = value;
            else if (option == ReminderOption.ReceiveRMSReminderReport)
                updated.RMSReceiveReminderReport = value;
            else if (option == ReminderOption.ReceiveFFReminder)
                updated.FFReceiveReminder = value;
            else if (option == ReminderOption.ReceiveFFReminderReport)
                updated.FFReceiveReminderReport = value;
            else
                throw new NotImplementedException();

            updated.UpdatedBy = userName;
            updated.LastUpdate = DateTime.Now;

            await _cpiDbContext.SaveChangesAsync();

            return updated.tStamp;
        }

        public async Task SaveLastReminderSentDate(int clientContactID, DateTime sentDate, string userName)
        {
            Guard.Against.NoRecordPermission(
                            (await ValidatePermission(SystemType.Shared, CPiPermissions.FullModify, "")) ||
                            (await ValidatePermission(SystemType.AMS, CPiPermissions.FullModify, ""))
                            );

            var updated = await GetByIdAsync(clientContactID);

            Guard.Against.NoRecordPermission(updated != null);
            if (EntityType.IsEntityFilterType(typeof(Client), _user.GetEntityFilterType()))
                Guard.Against.NoRecordPermission(await base.EntityFilterAllowed(updated.ClientID));

            _cpiDbContext.GetRepository<ClientContact>().Attach(updated);

            updated.LastReminderSentDate = sentDate;
            updated.UpdatedBy = userName;
            updated.LastUpdate = sentDate;

            await _cpiDbContext.SaveChangesAsync();

            _cpiDbContext.Detach(updated);
        }

        public async Task SaveLastPrepayReminderSentDate(int clientContactID, DateTime sentDate, string userName)
        {
            Guard.Against.NoRecordPermission(
                            (await ValidatePermission(SystemType.Shared, CPiPermissions.FullModify, "")) ||
                            (await ValidatePermission(SystemType.AMS, CPiPermissions.FullModify, ""))
                            );

            var updated = await GetByIdAsync(clientContactID);

            Guard.Against.NoRecordPermission(updated != null);
            if (EntityType.IsEntityFilterType(typeof(Client), _user.GetEntityFilterType()))
                Guard.Against.NoRecordPermission(await base.EntityFilterAllowed(updated.ClientID));

            _cpiDbContext.GetRepository<ClientContact>().Attach(updated);

            updated.LastPrepayReminderSentDate = sentDate;
            updated.UpdatedBy = userName;
            updated.LastUpdate = sentDate;

            await _cpiDbContext.SaveChangesAsync();

            _cpiDbContext.Detach(updated);
        }

        public async Task SaveLastConfirmationLetterSentDate(int clientContactID, DateTime sentDate, string userName)
        {
            Guard.Against.NoRecordPermission(
                            (await ValidatePermission(SystemType.Shared, CPiPermissions.FullModify, "")) ||
                            (await ValidatePermission(SystemType.AMS, CPiPermissions.FullModify, ""))
                            );

            var updated = await GetByIdAsync(clientContactID);

            Guard.Against.NoRecordPermission(updated != null);
            if (EntityType.IsEntityFilterType(typeof(Client), _user.GetEntityFilterType()))
                Guard.Against.NoRecordPermission(await base.EntityFilterAllowed(updated.ClientID));

            _cpiDbContext.GetRepository<ClientContact>().Attach(updated);

            updated.LastConfirmationLetterSentDate = sentDate;
            updated.UpdatedBy = userName;
            updated.LastUpdate = sentDate;

            await _cpiDbContext.SaveChangesAsync();

            _cpiDbContext.Detach(updated);
        }

        public async Task SaveLastRenewalConfirmationLetterSentDate(int clientContactID, DateTime sentDate, string userName)
        {
            Guard.Against.NoRecordPermission(
                            (await ValidatePermission(SystemType.Shared, CPiPermissions.FullModify, "")) ||
                            (await ValidatePermission(SystemType.RMS, CPiPermissions.FullModify, ""))
                            );

            var updated = await GetByIdAsync(clientContactID);

            Guard.Against.NoRecordPermission(updated != null);
            if (EntityType.IsEntityFilterType(typeof(Client), _user.GetEntityFilterType()))
                Guard.Against.NoRecordPermission(await base.EntityFilterAllowed(updated.ClientID));

            _cpiDbContext.GetRepository<ClientContact>().Attach(updated);

            updated.RMSLastConfirmationLetterSentDate = sentDate;
            updated.UpdatedBy = userName;
            updated.LastUpdate = sentDate;

            await _cpiDbContext.SaveChangesAsync();

            _cpiDbContext.Detach(updated);
        }

        public async Task SaveLastRenewalReminderSentDate(int clientContactID, DateTime sentDate, string userName)
        {
            Guard.Against.NoRecordPermission(
                            (await ValidatePermission(SystemType.Shared, CPiPermissions.FullModify, "")) ||
                            (await ValidatePermission(SystemType.RMS, CPiPermissions.FullModify, ""))
                            );

            var updated = await GetByIdAsync(clientContactID);

            Guard.Against.NoRecordPermission(updated != null);
            if (EntityType.IsEntityFilterType(typeof(Client), _user.GetEntityFilterType()))
                Guard.Against.NoRecordPermission(await base.EntityFilterAllowed(updated.ClientID));

            _cpiDbContext.GetRepository<ClientContact>().Attach(updated);

            updated.RMSLastReminderSentDate = sentDate;
            updated.UpdatedBy = userName;
            updated.LastUpdate = sentDate;

            await _cpiDbContext.SaveChangesAsync();

            _cpiDbContext.Detach(updated);
        }

        public async Task SaveLastForeignFilingConfirmationLetterSentDate(int clientContactID, DateTime sentDate, string userName)
        {
            Guard.Against.NoRecordPermission(
                            (await ValidatePermission(SystemType.Shared, CPiPermissions.FullModify, "")) ||
                            (await ValidatePermission(SystemType.ForeignFiling, CPiPermissions.FullModify, ""))
                            );

            var updated = await GetByIdAsync(clientContactID);

            Guard.Against.NoRecordPermission(updated != null);
            if (EntityType.IsEntityFilterType(typeof(Client), _user.GetEntityFilterType()))
                Guard.Against.NoRecordPermission(await base.EntityFilterAllowed(updated.ClientID));

            _cpiDbContext.GetRepository<ClientContact>().Attach(updated);

            updated.FFLastConfirmationLetterSentDate = sentDate;
            updated.UpdatedBy = userName;
            updated.LastUpdate = sentDate;

            await _cpiDbContext.SaveChangesAsync();

            _cpiDbContext.Detach(updated);
        }

        public async Task SaveLastForeignFilingReminderSentDate(int clientContactID, DateTime sentDate, string userName)
        {
            Guard.Against.NoRecordPermission(
                            (await ValidatePermission(SystemType.Shared, CPiPermissions.FullModify, "")) ||
                            (await ValidatePermission(SystemType.ForeignFiling, CPiPermissions.FullModify, ""))
                            );

            var updated = await GetByIdAsync(clientContactID);

            Guard.Against.NoRecordPermission(updated != null);
            if (EntityType.IsEntityFilterType(typeof(Client), _user.GetEntityFilterType()))
                Guard.Against.NoRecordPermission(await base.EntityFilterAllowed(updated.ClientID));

            _cpiDbContext.GetRepository<ClientContact>().Attach(updated);

            updated.FFLastReminderSentDate = sentDate;
            updated.UpdatedBy = userName;
            updated.LastUpdate = sentDate;

            await _cpiDbContext.SaveChangesAsync();

            _cpiDbContext.Detach(updated);
        }
    }
}
