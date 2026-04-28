using Microsoft.EntityFrameworkCore;
using LawPortal.Core.DTOs;
using LawPortal.Core.Identity;
using LawPortal.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LawPortal.Infrastructure.Identity
{
    public class CPiUserEntityFilterStore : ICPiUserEntityFilterRepository
    {
        private readonly CPiUserDbContext _dbContext;

        public CPiUserEntityFilterStore(CPiUserDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IQueryable<CPiUserEntityFilter> UserEntityFilters => _dbContext.CPiUserEntityFilters.AsNoTracking();

        public IQueryable<EntityFilterDTO> EntityFilters => _dbContext.EntityFilters.AsNoTracking();

        public async Task<List<CPiUserEntityFilter>> GetUserEntityFilters(string userId)
        {
            return await _dbContext.CPiUserEntityFilters.Where(f => f.UserId == userId).ToListAsync();
        }

        public async Task CreateAsync(CPiUserEntityFilter userEntityFilter)
        {
            _dbContext.CPiUserEntityFilters.Add(userEntityFilter);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(CPiUserEntityFilter userEntityFilter)
        {
            _dbContext.CPiUserEntityFilters.Remove(userEntityFilter);
            await _dbContext.SaveChangesAsync();
        }

        public async Task CreateAsync(List<CPiUserEntityFilter> userEntityFilters)
        {
            _dbContext.CPiUserEntityFilters.AddRange(userEntityFilters);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(List<CPiUserEntityFilter> userEntityFilters)
        {
            _dbContext.CPiUserEntityFilters.RemoveRange(userEntityFilters);
            await _dbContext.SaveChangesAsync();
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~CPiUserEntityFilterStore() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
