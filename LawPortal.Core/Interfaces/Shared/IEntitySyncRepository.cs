using LawPortal.Core.DTOs;
using LawPortal.Core.Entities;
using LawPortal.Core.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LawPortal.Core.Interfaces
{
    public interface IEntitySyncRepository
    {
        Task SyncEntities(int[] ids, int syncType, string userName);
    }
}
