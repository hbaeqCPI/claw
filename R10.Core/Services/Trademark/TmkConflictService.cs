using Microsoft.EntityFrameworkCore;
using R10.Core.Entities;
using R10.Core.Entities.Trademark;
using R10.Core.Exceptions;
using R10.Core.Helpers;
using R10.Core.Identity;
using R10.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Services
{
    public class TmkConflictService : ITmkConflictService
    {
        private readonly ITmkConflictRepository _conflictRepository;
        private readonly IEntityService<TmkConflictStatus> _conflictStatusService;
        private readonly ITmkTrademarkService _trademarkService;
        private readonly ClaimsPrincipal _user;
        private readonly ISystemSettings<TmkSetting> _settings;

        public TmkConflictService(
            ITmkConflictRepository conflictRepository,
            IEntityService<TmkConflictStatus> conflictStatusService,
            ITmkTrademarkService trademarkService,
            ClaimsPrincipal user,
            ISystemSettings<TmkSetting> settings
            )
        {
            _conflictRepository = conflictRepository;
            _conflictStatusService = conflictStatusService;
            _trademarkService = trademarkService;
            _user = user;
            _settings = settings;
        }

        public IQueryable<TmkConflict> TmkConflicts
        {
            get
            {
                var conflicts = _conflictRepository.QueryableList;

                if (_user.HasEntityFilter() || _user.HasRespOfficeFilter(SystemType.Trademark))
                    conflicts = conflicts.Where(a => _trademarkService.TmkTrademarks.Any(tm => tm.TmkId == a.TmkId));

                return conflicts;
            }
        }

        //public IQueryable<TmkConflict> TmkConflicts
        //{
        //    get
        //    {
        //        var conflicts = _conflictRepository.QueryableList;

        //        if (_user.HasRespOfficeFilter(SystemType.Trademark))
        //            conflicts = conflicts.Where(RespOfficeFilter());

        //        // no entity filter for conflicts
        //        //if (HasEntityFilter())
        //        //    conflicts = conflicts.Where(EntityFilter());

        //        return conflicts;
        //    }
        //}

        protected bool HasEntityFilter()
        {
            return false;       // no entity filter for conflicts
        }
        //protected Expression<Func<TmkConflict, bool>> EntityFilter()
        //{
        //    return c => true;   // no entity filter for conflicts
        //}

        public Expression<Func<TmkConflict, bool>> RespOfficeFilter()
        {
            return a => _conflictRepository.CPiUserSystemRoles.Any(r => r.UserId == _user.GetUserIdentifier() && r.SystemId == SystemType.Trademark 
                                                        && a.TmkTrademark.RespOffice == r.RespOffice);
        }

        

        // main screen CRUD
        public async Task<TmkConflict> GetByIdAsync(int conflictId)
        {
            return await TmkConflicts.SingleOrDefaultAsync(c => c.ConflictId == conflictId);
        }

        public async Task AddConflict(TmkConflict tmkConflict)
        {
            await ValidateConflict(tmkConflict);
            await _conflictRepository.AddAsync(tmkConflict);
        }

        public async Task UpdateConflict(TmkConflict tmkConflict)
        {
            await ValidatePermission(tmkConflict.ConflictId);
            await ValidateConflict(tmkConflict);
            await _conflictRepository.UpdateAsync(tmkConflict);
        }

        public async Task DeleteConflict(TmkConflict tmkConflict)
        {
            await ValidatePermission(tmkConflict.ConflictId);
            await _conflictRepository.DeleteAsync(tmkConflict);
        }


        // Read for conflict tab grid on trademark screen
        public async Task<List<TmkConflict>> GetByParentIdAsync(int tmkId)
        {
            return await TmkConflicts.Where(c => c.TmkId == tmkId).ToListAsync();
        }


        // validation
        protected async Task ValidatePermission(int conflictId)
        {
            if (HasEntityFilter() || _user.HasRespOfficeFilter(SystemType.Trademark))
                Guard.Against.NoRecordPermission(await TmkConflicts.AnyAsync(c => c.ConflictId == conflictId));
        }

        protected async Task ValidateConflict(TmkConflict tmkConflict)
        {
            
            tmkConflict.SubCase = tmkConflict.SubCase ?? "";

            var trademark = await _trademarkService.TmkTrademarks
                .Where(t =>
                    t.CaseNumber == tmkConflict.CaseNumber &&
                    t.Country == tmkConflict.Country &&
                    t.SubCase == tmkConflict.SubCase)
                .SingleOrDefaultAsync();

            var caseNumberLabel = (await _settings.GetSetting()).LabelCaseNumber;
            Guard.Against.ValueNotAllowed(trademark?.TmkId > 0, $"{caseNumberLabel}/Country/Sub Case");

            tmkConflict.TmkId = trademark.TmkId;
        }


    }
}
