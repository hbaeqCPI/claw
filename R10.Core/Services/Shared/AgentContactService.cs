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
    public class AgentContactService : EntityContactService<Agent, AgentContact>, IAgentContactService
    {
        public AgentContactService(ICPiDbContext cpiDbContext, ClaimsPrincipal user) : base(cpiDbContext, user)
        {
        }

        public override async Task<AgentContact> GetByIdAsync(int agentContactID)
        {
            return await QueryableList.SingleOrDefaultAsync(c => c.AgentContactID == agentContactID);
        }

        public async Task SaveLastResponsibilityLetterSentDate(int agentContactID, DateTime sentDate, string userName)
        {
            Guard.Against.NoRecordPermission(await ValidatePermission(SystemType.Shared, CPiPermissions.FullModify, ""));

            var updated = await GetByIdAsync(agentContactID);

            Guard.Against.NoRecordPermission(updated != null);
            if (EntityType.IsEntityFilterType(typeof(Agent), _user.GetEntityFilterType()))
                Guard.Against.NoRecordPermission(await base.EntityFilterAllowed(updated.AgentID));

            _cpiDbContext.GetRepository<AgentContact>().Attach(updated);

            updated.LastResponsibilityLetterSentDate = sentDate;
            updated.UpdatedBy = userName;
            updated.LastUpdate = sentDate;

            await _cpiDbContext.SaveChangesAsync();

            _cpiDbContext.Detach(updated);
        }

        public async Task SaveLastRMSResponsibilityLetterSentDate(int agentContactID, DateTime sentDate, string userName)
        {
            Guard.Against.NoRecordPermission(await ValidatePermission(SystemType.Shared, CPiPermissions.FullModify, ""));

            var updated = await GetByIdAsync(agentContactID);

            Guard.Against.NoRecordPermission(updated != null);
            if (EntityType.IsEntityFilterType(typeof(Agent), _user.GetEntityFilterType()))
                Guard.Against.NoRecordPermission(await base.EntityFilterAllowed(updated.AgentID));

            _cpiDbContext.GetRepository<AgentContact>().Attach(updated);

            updated.RMSLastResponsibilityLetterSentDate = sentDate;
            updated.UpdatedBy = userName;
            updated.LastUpdate = sentDate;

            await _cpiDbContext.SaveChangesAsync();

            _cpiDbContext.Detach(updated);
        }

        public async Task SaveLastFFResponsibilityLetterSentDate(int agentContactID, DateTime sentDate, string userName)
        {
            Guard.Against.NoRecordPermission(await ValidatePermission(SystemType.Shared, CPiPermissions.FullModify, ""));

            var updated = await GetByIdAsync(agentContactID);

            Guard.Against.NoRecordPermission(updated != null);
            if (EntityType.IsEntityFilterType(typeof(Agent), _user.GetEntityFilterType()))
                Guard.Against.NoRecordPermission(await base.EntityFilterAllowed(updated.AgentID));

            _cpiDbContext.GetRepository<AgentContact>().Attach(updated);

            updated.FFLastResponsibilityLetterSentDate = sentDate;
            updated.UpdatedBy = userName;
            updated.LastUpdate = sentDate;

            await _cpiDbContext.SaveChangesAsync();

            _cpiDbContext.Detach(updated);
        }

        public async Task<byte[]> SaveAgentResponsibilityOption(int agentContactID, AgentResponsibilityOption option, bool value, byte[] tStamp, string userName)
        {
            Guard.Against.NoRecordPermission(
                            (await ValidatePermission(SystemType.Shared, CPiPermissions.FullModify, "")) ||
                            (await ValidatePermission(SystemType.AMS, CPiPermissions.FullModify, "")) ||
                            (await ValidatePermission(SystemType.RMS, CPiPermissions.FullModify, "")) ||
                            (await ValidatePermission(SystemType.ForeignFiling, CPiPermissions.FullModify, ""))
                            );

            var updated = await GetByIdAsync(agentContactID);

            Guard.Against.RecordNotFound(updated != null);

            if (EntityType.IsEntityFilterType(typeof(Agent), _user.GetEntityFilterType()))
                Guard.Against.NoRecordPermission(await base.EntityFilterAllowed(updated.AgentID));

            updated.tStamp = tStamp;
            _cpiDbContext.GetRepository<AgentContact>().Attach(updated);

            if (option == AgentResponsibilityOption.ReceiveAgentResponsibilityLetter)
                updated.ReceiveAgentResponsibilityLetter = value;
            else if (option == AgentResponsibilityOption.RMSReceiveAgentResponsibilityLetter)
                updated.RMSReceiveAgentResponsibilityLetter = value;
            else if (option == AgentResponsibilityOption.FFReceiveAgentResponsibilityLetter)
                updated.FFReceiveAgentResponsibilityLetter = value;
            else
                throw new NotImplementedException();

            updated.UpdatedBy = userName;
            updated.LastUpdate = DateTime.Now;

            await _cpiDbContext.SaveChangesAsync();

            return updated.tStamp;
        }
    }
}
