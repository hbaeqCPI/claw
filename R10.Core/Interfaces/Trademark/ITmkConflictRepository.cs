using R10.Core.Entities.Trademark;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace R10.Core.Interfaces
{
    public interface ITmkConflictRepository : IAsyncRepository<TmkConflict>, IEntityFilterRepository
    {
        // main screen CRUD are in the base class

        // below are for conflict tab grid on trademark screen
        Task<List<TmkConflict>> GetConflicts(int tmkId);

        Task<bool> ConflictsUpdate(int tmkId, string userName, IList<TmkConflict> updatedConflicts, IList<TmkConflict> deletedConflicts);

        Task ConflictDelete(TmkConflict deletedConflict);

    }
}
