using R10.Core.Entities.Trademark;

namespace R10.Core.Interfaces
{
    public interface ITmkConflictService 
    {
        IQueryable<TmkConflict> TmkConflicts { get; }

        // main screen CRUD
        Task<TmkConflict> GetByIdAsync(int conflictId);
        Task AddConflict(TmkConflict tmkConflict);
        Task UpdateConflict(TmkConflict tmkConflict);
        Task DeleteConflict(TmkConflict tmkConflict);

        // Read for conflict tab grid on trademark screen
        Task<List<TmkConflict>> GetByParentIdAsync(int tmkId);
       
    }
}
