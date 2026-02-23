using R10.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Interfaces
{
    public interface IAgentContactService : IChildEntityService<Agent, AgentContact>
    {
        Task SaveLastResponsibilityLetterSentDate(int agentContactID, DateTime sentDate, string userName);
        Task SaveLastRMSResponsibilityLetterSentDate(int agentContactID, DateTime sentDate, string userName);
        Task SaveLastFFResponsibilityLetterSentDate(int agentContactID, DateTime sentDate, string userName);
        Task<byte[]> SaveAgentResponsibilityOption(int agentContactID, AgentResponsibilityOption option, bool value, byte[] tStamp, string userName);
    }
}
