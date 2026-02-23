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
using R10.Core.Exceptions;
using R10.Core.Helpers;
using R10.Core.Identity;
using R10.Core.Interfaces;
using R10.Core.Interfaces.Patent;

namespace R10.Core.Services
{
    public class RTSPTOActionMappingService : IRTSPTOActionMappingService
    {
     
        private readonly IApplicationDbContext _repository;
        public RTSPTOActionMappingService(IApplicationDbContext repository)
        {
            _repository = repository;
        }

        public async Task MappingsUpdate(IList<RTSMapActionDue> updatedRTSMapActionDues, IList<RTSMapActionDue> newRTSMapActionDues)
        {
            if (updatedRTSMapActionDues.Any())
            {
                _repository.RTSMapActionDues.UpdateRange(updatedRTSMapActionDues);
            }

            if (newRTSMapActionDues.Any())
            {
                _repository.RTSMapActionDues.AddRange(newRTSMapActionDues);
            }
            await _repository.SaveChangesAsync();
        }

        public async Task MappingDelete(RTSMapActionDue deletedRTSMapActionDue)
        {
            _repository.RTSMapActionDues.Remove(deletedRTSMapActionDue);
            await _repository.SaveChangesAsync();
        }

        public async Task ActionToCloseUpdate(IList<RTSMapActionClose> updatedRTSMapActionsClose, IList<RTSMapActionClose> newRTSMapActionsClose)
        {
            if (updatedRTSMapActionsClose.Any())
            {
                _repository.RTSMapActionsClose.UpdateRange(updatedRTSMapActionsClose);
            }

            if (newRTSMapActionsClose.Any())
            {
                _repository.RTSMapActionsClose.AddRange(newRTSMapActionsClose);
            }
            await _repository.SaveChangesAsync();
        }

        public async Task ActionToCloseDelete(RTSMapActionClose deletedRTSMapActionClose)
        {
            _repository.RTSMapActionsClose.Remove(deletedRTSMapActionClose);
            await _repository.SaveChangesAsync();
        }

        public IQueryable<RTSMapActionDueSource> RTSMapActionDueSources => _repository.RTSMapActionDueSources.AsNoTracking();
        public IQueryable<RTSMapActionDue> RTSMapActionDues => _repository.RTSMapActionDues.AsNoTracking();
        public IQueryable<RTSMapActionClose> RTSMapActionsClose => _repository.RTSMapActionsClose.AsNoTracking();
        public IQueryable<PatActionType> PatActionTypes => _repository.PatActionTypes.AsNoTracking();
        public IQueryable<PatActionParameter> PatActionParameters => _repository.Set<PatActionParameter>().AsNoTracking();
    }
}
