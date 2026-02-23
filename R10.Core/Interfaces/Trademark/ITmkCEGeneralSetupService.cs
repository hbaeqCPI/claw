using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using R10.Core.Entities.Trademark;

namespace R10.Core.Interfaces
{
    public interface ITmkCEGeneralSetupService
    {
        Task AddCEGeneralSetup(TmkCEGeneralSetup generalSetup);
        Task UpdateCEGeneralSetup(TmkCEGeneralSetup generalSetup);
        Task DeleteCEGeneralSetup(TmkCEGeneralSetup generalSetup);

        Task CopyCEGeneralSetup(int oldCEGeneralId, int newCEGeneralId, string userName, bool copyCosts);

        Task UpdateChild<T>(int parentId, string userName, IEnumerable<TmkCEGeneralCost> updated, IEnumerable<TmkCEGeneralCost> added, IEnumerable<T> deleted) where T : BaseEntity;        
        Task ReorderCEGeneralCost(int id, string userName, int newIndex);

        IQueryable<TmkCEGeneralSetup> TmkCEGeneralSetups { get; }
        IQueryable<TmkCEGeneralCost> TmkCEGeneralCosts { get; }
    }
}
