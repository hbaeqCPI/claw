using Microsoft.EntityFrameworkCore;
using R10.Core.Entities;
// using R10.Core.Entities.AMS; // Removed during deep clean
// using R10.Core.Entities.RMS; // Removed during deep clean
using R10.Core.Entities.Shared;
using R10.Core.Exceptions;
using R10.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Services
{
    public class EmailTypeService : ParentEntityService<EmailType, EmailSetup>, IParentEntityService<EmailType, EmailSetup>
    {
        public EmailTypeService(ICPiDbContext cpiDbContext, ClaimsPrincipal user) : base(cpiDbContext, user)
        {
        }

        public override IChildEntityService<EmailType, EmailSetup> ChildService => new EmailSetupService(_cpiDbContext, _user);

        public override async Task Delete(EmailType entity)
        {
            await ValidateReferenceConstraint(entity.Name);
            await base.Delete(entity);
        }

        public override async Task Update(EmailType entity)
        {
            if(!entity.IsEnabled)
                await ValidateReferenceConstraint(entity.Name, "Record cannot be disabled, it is already in use. The conflict occurred in table \"dbo.{0}\".");

            await CascadeUpdate(entity);
            await base.Update(entity);
        }

        /// <summary>
        /// Validate if email is in use.
        /// </summary>
        /// <param name="emailName"></param>
        /// <returns></returns>
        private async Task ValidateReferenceConstraint(string emailName, string innerException = "")
        {
            //mock sql delete reference constraint exception
            var message = "An error occurred while updating the entries. See the inner exception for details.";
            if (string.IsNullOrEmpty(innerException))
                innerException = "The DELETE statement conflicted with the REFERENCE constraint \"---\". The conflict occurred in database \"---\", table \"dbo.{0}\", column '---'.\r\nThe statement has been terminated.";

            //client letters
            if (await _cpiDbContext.GetRepository<Client>().QueryableList.AnyAsync(c => 
                            c.ConfirmationCoverLetter == emailName || 
                            c.PrePayCoverLetter == emailName || 
                            c.ReminderCoverLetter == emailName ||
                            c.RMSConfirmationCoverLetter == emailName ||
                            c.RMSReminderCoverLetter == emailName)
                )
                throw new DbUpdateException(message, new Exception(string.Format(innerException, "tblClient")));

            //agent letters
            if (await _cpiDbContext.GetRepository<Agent>().QueryableList.AnyAsync(a =>
                            a.AgentResponsibilityCoverLetter == emailName ||
                            a.RMSAgentResponsibilityCoverLetter == emailName)
                )
                throw new DbUpdateException(message, new Exception(string.Format(innerException, "tblClient")));

            //default letters in tblpuboptions
            if (await _cpiDbContext.GetReadOnlyRepositoryAsync<Option>().QueryableList.AnyAsync(o => 
                            DefaultLetterSettingNames.Contains(o.OptionSubKey) &&
                            o.OptionValue == emailName)
                )
                throw new DbUpdateException(message, new Exception(string.Format(innerException, "tblSettings")));

            //todo: more referential integrity
            //user account emails, ams notifications

            return;
        }

        private async Task CascadeUpdate(EmailType emailType)
        {
            var emailName = await QueryableList.Where(e => e.EmailTypeId == emailType.EmailTypeId).Select(e => e.Name).FirstOrDefaultAsync();
            Guard.Against.NoRecordPermission(!string.IsNullOrEmpty(emailName));

            if (emailName == emailType.Name)
                return;

            //client letters
            var clients = await _cpiDbContext.GetRepository<Client>().QueryableList.Where(c => 
                                        c.ConfirmationCoverLetter == emailName || 
                                        c.PrePayCoverLetter == emailName || 
                                        c.ReminderCoverLetter == emailName ||
                                        c.RMSConfirmationCoverLetter == emailName ||
                                        c.RMSReminderCoverLetter == emailName).ToListAsync();
            foreach(var client in clients)
            {
                _cpiDbContext.GetRepository<Client>().Attach(client);

                if (client.ConfirmationCoverLetter == emailName)
                    client.ConfirmationCoverLetter = emailType.Name;

                if (client.PrePayCoverLetter == emailName)
                    client.PrePayCoverLetter = emailType.Name;

                if (client.ReminderCoverLetter == emailName)
                    client.ReminderCoverLetter = emailType.Name;

                if (client.RMSConfirmationCoverLetter == emailName)
                    client.RMSConfirmationCoverLetter = emailType.Name;

                if (client.RMSReminderCoverLetter == emailName)
                    client.RMSReminderCoverLetter = emailType.Name;
            }

            //agent letters
            var agents = await _cpiDbContext.GetRepository<Agent>().QueryableList.Where(a =>
                                        a.AgentResponsibilityCoverLetter == emailName ||
                                        a.RMSAgentResponsibilityCoverLetter == emailName).ToListAsync();
            foreach (var agent in agents)
            {
                _cpiDbContext.GetRepository<Agent>().Attach(agent);

                if (agent.AgentResponsibilityCoverLetter == emailName)
                    agent.AgentResponsibilityCoverLetter = emailType.Name;

                if (agent.RMSAgentResponsibilityCoverLetter == emailName)
                    agent.RMSAgentResponsibilityCoverLetter = emailType.Name;
            }


            //default letters in tblpuboptions
            var settings = await _cpiDbContext.GetRepository<Option>().QueryableList.Where(o =>
                            DefaultLetterSettingNames.Contains(o.OptionSubKey) &&
                            o.OptionValue == emailName).ToListAsync();
            foreach (var setting in settings)
            {
                _cpiDbContext.GetRepository<Option>().Attach(setting);

                if (setting.OptionValue == emailName)
                    setting.OptionValue = emailType.Name;
            }

            //todo: more referential integrity

            return;
        }

        private List<string> DefaultLetterSettingNames => new List<string>()
            {
                // Removed during deep clean - AMS module removed
                // nameof(AMSSetting.ReminderCoverLetter),
                // nameof(AMSSetting.ClientConfirmationCoverLetter),
                // nameof(AMSSetting.DecisionMakerNotification),
                // nameof(AMSSetting.PrepayReminderCoverLetter),
                // nameof(AMSSetting.AttorneySummaryCoverLetter),
                // nameof(AMSSetting.AgentConfirmationCoverLetter),
                // nameof(AMSSetting.AbandonmentCoverLetter),
                // nameof(AMSSetting.InstructionNotification),
                // nameof(AMSSetting.ClearedInstructionNotification),
                // nameof(AMSSetting.InstructionGraceDateWarning),
                // nameof(AMSSetting.InstructionsToCPINotification),
                // Removed during deep clean - RMS module removed
                // nameof(RMSSetting.ReminderCoverLetter),
                // nameof(RMSSetting.ClientConfirmationCoverLetter),
                // nameof(RMSSetting.DecisionMakerNotification),
                // nameof(RMSSetting.AgentConfirmationCoverLetter),
                // nameof(RMSSetting.InstructionNotification),
                // nameof(RMSSetting.ClearedInstructionNotification),
                //USER SETUP
                nameof(DefaultSetting.NewPasswordNotification),
                nameof(DefaultSetting.TemporaryPasswordNotification),
                nameof(DefaultSetting.ResetPasswordLinkNotification),
                nameof(DefaultSetting.NeedsConfirmationNotification),
                nameof(DefaultSetting.ConfirmEmailLinkNotification),
                nameof(DefaultSetting.PendingRegistrationNotification),
                nameof(DefaultSetting.AccountApprovalNotification),
                //TRADE SECRET
                nameof(DefaultSetting.TradeSecretAccessCodeNotification),
                nameof(DefaultSetting.TradeSecretRequestNotification),
                //TASK SCHEDULER
                nameof(DefaultSetting.TaskSchedulerNotification)
            };
    }
}
