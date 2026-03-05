using R10.Core.Entities;
using R10.Core.Helpers;
using R10.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Services
{
    public class WebApiBaseService<T> : EntityService<T> where T : class
    {
        private bool? _hasSharedAuxModify;

        public WebApiBaseService(ICPiDbContext cpiDbContext, ClaimsPrincipal user) : base(cpiDbContext, user)
        {
        }

        protected bool HasSharedAuxModify {
            get
            {
                if (_hasSharedAuxModify == null)
                    _hasSharedAuxModify = _user.IsInRoles(SystemType.Shared, CPiPermissions.FullModify);

                return (bool)_hasSharedAuxModify;
            }
        }

        protected DateTime EmptyDate => new DateTime(0001, 1, 1);

        protected string FormatErrorMessage(int index, string message, params string?[] keys)
        {
            return $"[{index}] {String.Join("|", keys.Where(k => !string.IsNullOrEmpty(k)))} : {message}";
        }

        public async Task LogApiData(List<T> entities)
        {
            _cpiDbContext.GetRepository<T>().Add(entities);
            await _cpiDbContext.SaveChangesAsync();
        }

        public async Task LogApiData(T entity)
        {
            _cpiDbContext.GetRepository<T>().Add(entity);
            await _cpiDbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Reset db entity states to clear db exceptions
        /// </summary>
        public void ClearDbException()
        {
            _cpiDbContext.DetachAll();
        }
    }
}
