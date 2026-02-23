using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using R10.Core.Entities.Patent;

namespace R10.Core.Interfaces
{
    public interface IPatCEGeneralSetupService
    {
        Task AddCEGeneralSetup(PatCEGeneralSetup generalSetup);
        Task UpdateCEGeneralSetup(PatCEGeneralSetup generalSetup);
        Task DeleteCEGeneralSetup(PatCEGeneralSetup generalSetup);

        Task CopyCEGeneralSetup(int oldCEGeneralId, int newCEGeneralId, string userName, bool copyCosts);

        Task UpdateChild<T>(int parentId, string userName, IEnumerable<PatCEGeneralCost> updated, IEnumerable<PatCEGeneralCost> added, IEnumerable<T> deleted) where T : BaseEntity;        
        Task ReorderCEGeneralCost(int id, string userName, int newIndex);

        IQueryable<PatCEGeneralSetup> PatCEGeneralSetups { get; }
        IQueryable<PatCEGeneralCost> PatCEGeneralCosts { get; }
    }
}
