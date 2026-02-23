using R10.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace R10.Core.Interfaces
{
    public interface IAgentService : IEntityService<Agent>
    {
        IAgentContactService ChildService { get; }
        Task<List<SysCustomFieldSetting>> GetCustomFields();
    }
}
