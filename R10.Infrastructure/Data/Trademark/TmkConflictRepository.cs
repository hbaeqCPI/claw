using Microsoft.EntityFrameworkCore;
using R10.Core.Interfaces;
using System.Data;
using R10.Core.Entities.Trademark;

namespace R10.Infrastructure.Data
{
    public class TmkConflictRepository : EFRepository<TmkConflict>, ITmkConflictRepository
    {
        public TmkConflictRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }



        // main screen CRUD are in the base class

        // below are for conflict tab grid on trademark screen
        public async Task<List<TmkConflict>> GetConflicts(int tmkId)
        {
            return await _dbContext.TmkConflicts.Where(c => c.TmkId == tmkId).ToListAsync();
        }


        public async Task<bool> ConflictsUpdate(int tmkId, string userName, IList<TmkConflict> updatedConflicts, IList<TmkConflict> deletedConflicts)
        {
            foreach (var item in deletedConflicts)
            {
                _dbContext.Set<TmkConflict>().Remove(item);
            }

            foreach (var item in updatedConflicts)
            {
                if (_dbContext.Entry(item).State != EntityState.Deleted && _dbContext.Entry(item).State != EntityState.Modified)
                    _dbContext.Entry(item).State = EntityState.Modified;
            }

           

            var trademark = _dbContext.TmkTrademarks.FirstOrDefault(t => t.TmkId == tmkId);
            if (trademark != null)
            {
                trademark.UpdatedBy = userName;
                trademark.LastUpdate = DateTime.Now;
            }

            await _dbContext.SaveChangesAsync();
            return true;
        }


        public async Task ConflictDelete(TmkConflict deletedConflict)
        {
            _dbContext.Set<TmkConflict>().Remove(deletedConflict);
            var trademark = _dbContext.TmkTrademarks.FirstOrDefault(t => t.TmkId == deletedConflict.TmkId);
            if (trademark != null)
            {
                trademark.UpdatedBy = deletedConflict.UpdatedBy;
                trademark.LastUpdate = DateTime.Now;
            }

            await _dbContext.SaveChangesAsync();
        }

    }
}
