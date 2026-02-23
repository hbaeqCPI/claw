using R10.Core;
using R10.Core.Entities.Patent;
using R10.Web.Areas.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Interfaces
{
    public interface IPatInventorAppAwardUpdateService
    {
        IQueryable<PatInventorAppAward> PatInventorAppAwards { get; }
        Task<List<PatInventorAppAward>> GetPatInventorAppAwardByInvId(int invId);
        Task UpdateInventorInvAwards(int invId, List<PatInventorInv> inventorInvs, string userName);
        Task UpdateInventorInvAwards(int invId, string userName);
        Task UpdateInventorAppAwards(int AppId, ApplicationModifiedFields applicationModified, bool fromInventorUpdate, string userName);
        Task UpdateInventorAppAwards(List<PatInventorAppAward> updated, string userName);
        Task DeleteInventorAppAwards(List<PatInventorApp> deleted, string userName);
        Task UpdateInventorAppAwardsByInventorAppId(int InventorAppId, string userName);

        Task<List<WorkflowEmailViewModel>> GenerationWorkflow(List<PatInventorAppAward> oldAwards, List<PatInventorAppAward> newAwards, CountryApplication app, string emailUrl);
    }
}
