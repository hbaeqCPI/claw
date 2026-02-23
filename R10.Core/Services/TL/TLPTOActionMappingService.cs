using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using R10.Core.Entities.Trademark;
using R10.Core.Exceptions;
using R10.Core.Helpers;
using R10.Core.Identity;
using R10.Core.Interfaces;
using R10.Core.Interfaces.Patent;

namespace R10.Core.Services
{
    public class TLPTOActionMappingService : ITLPTOActionMappingService
    {
     
        private readonly IApplicationDbContext _repository;
        public TLPTOActionMappingService(IApplicationDbContext repository)
        {
            _repository = repository;
        }

        public async Task MappingsUpdate(IList<TLMapActionDue> updatedTLMapActionDues, IList<TLMapActionDue> newTLMapActionDues)
        {
            if (updatedTLMapActionDues.Any())
            {
                _repository.TLMapActionDues.UpdateRange(updatedTLMapActionDues);
            }

            if (newTLMapActionDues.Any())
            {
                _repository.TLMapActionDues.AddRange(newTLMapActionDues);
            }
            await _repository.SaveChangesAsync();
        }

        public async Task MappingDelete(TLMapActionDue deletedTLMapActionDue)
        {
            _repository.TLMapActionDues.Remove(deletedTLMapActionDue);
            await _repository.SaveChangesAsync();
        }

        public async Task ActionToCloseUpdate(IList<TLMapActionClose> updatedTLMapActionsClose, IList<TLMapActionClose> newTLMapActionsClose)
        {
            if (updatedTLMapActionsClose.Any())
            {
                _repository.TLMapActionsClose.UpdateRange(updatedTLMapActionsClose);
            }

            if (newTLMapActionsClose.Any())
            {
                _repository.TLMapActionsClose.AddRange(newTLMapActionsClose);
            }
            await _repository.SaveChangesAsync();
        }

        public async Task ActionToCloseDelete(TLMapActionClose deletedTLMapActionClose)
        {
            _repository.TLMapActionsClose.Remove(deletedTLMapActionClose);
            await _repository.SaveChangesAsync();
        }

        public IQueryable<TLMapActionDueSource> TLMapActionDueSources => _repository.TLMapActionDueSources.AsNoTracking();
        public IQueryable<TLMapActionDue> TLMapActionDues => _repository.TLMapActionDues.AsNoTracking();
        public IQueryable<TLMapActionClose> TLMapActionsClose => _repository.TLMapActionsClose.AsNoTracking();
        public IQueryable<TmkActionType> TmkActionTypes => _repository.TmkActionTypes.AsNoTracking();
        public IQueryable<TmkActionParameter> TmkActionParameters => _repository.Set<TmkActionParameter>().AsNoTracking();
    }
}
