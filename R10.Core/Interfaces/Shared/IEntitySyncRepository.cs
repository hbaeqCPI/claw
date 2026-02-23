using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Interfaces
{
    public interface IEntitySyncRepository
    {
        Task SyncEntities(int[] ids, int syncType, string userName);
    }
}
