using R10.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Interfaces
{
    public interface IOwnerService : IParentEntityService<Owner, OwnerContact>
    {
        Task AddOwnerToClient(int[] ids);
        Task SyncOwnerWithClient(int[] ids);
        Task<List<SysCustomFieldSetting>> GetCustomFields();
    }
}
