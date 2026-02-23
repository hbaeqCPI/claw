using Microsoft.EntityFrameworkCore;
using R10.Core.Entities;
using R10.Core.Identity;
using R10.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Infrastructure.Data
{

    public class EFRepository<T> : IAsyncRepository<T> where T: class
    {
        protected readonly ApplicationDbContext _dbContext;

        public EFRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public virtual async Task<T> AddAsync(T entity)
        {
            _dbContext.Set<T>().Add(entity);
            await _dbContext.SaveChangesAsync();

            return entity;
        }

        public virtual async Task UpdateAsync(T entity)
        {
            _dbContext.Entry(entity).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
        }

        public virtual async Task DeleteAsync(T entity)
        {
            _dbContext.Set<T>().Remove(entity);
            await _dbContext.SaveChangesAsync();
        }

        public virtual async Task<T> GetByIdAsync(int id)
        {
            return await _dbContext.Set<T>().FindAsync(id);
        }

        public IQueryable<T> QueryableList => _dbContext.Set<T>().AsNoTracking();

        public IQueryable<CPiUserEntityFilter> UserEntityFilters => _dbContext.CPiUserEntityFilters.AsNoTracking();
        public IQueryable<CPiUserSystemRole> CPiUserSystemRoles => _dbContext.CPiUserSystemRoles.AsNoTracking();
        public IQueryable<CPiRespOffice> CPiRespOffices => _dbContext.Set<CPiRespOffice>().AsNoTracking();


        //todo: remove business logic/validation from repository. use implementation in base service.
        public async Task<bool> RespOfficeAllowed(string userIdentifier, string respOffice, string systemType)
        {
            return await this.CPiUserSystemRoles.AnyAsync(r => r.UserId == userIdentifier && r.SystemId == systemType && r.RespOffice == respOffice);
        }

        //todo: remove business logic/validation from repository. use implementation in base service.
        public async Task<bool> EntityFilterAllowed(string userIdentifier, int? entityId)
        {
            return await this.UserEntityFilters.AnyAsync(f => f.UserId == userIdentifier && f.EntityId == entityId);
        }

        //todo: remove business logic/validation from repository. use implementation in base service.
        public async Task<List<CPiUserEntityFilter>> GetUserEntityFilters(string userId)
        {
            return await this.UserEntityFilters.Where(f => f.UserId == userId).ToListAsync();
        }

        //todo: remove business logic/validation from repository. use implementation in base service.
        public async Task<List<CPiRespOffice>> GetUserRespOffices(string userId, string systemId, List<string> roles)
        {
            return await _dbContext.Set<CPiRespOffice>().AsNoTracking().Where(ro => ro.UserSystemRoles.Any(r => r.UserId == userId && r.SystemId == systemId && (!roles.Any() || roles.Contains(r.RoleId)) && r.RespOffice == ro.RespOffice)).ToListAsync();
        }
    }
}
