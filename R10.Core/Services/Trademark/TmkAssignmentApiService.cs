using Microsoft.EntityFrameworkCore;
using R10.Core.Entities.Trademark;
using R10.Core.Exceptions;
using R10.Core.Helpers;
using R10.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Services.Trademark
{
    public interface ITmkAssignmentApiService : IWebApiBaseService<TmkAssignmentHistoryWebSvc, TmkAssignmentHistory>
    {
        IQueryable<TmkAssignmentStatus> AssignmentStatuses { get; }
        Task Delete(TmkAssignmentHistory assignment);
    }

    public class TmkAssignmentApiService : WebApiBaseService<TmkAssignmentHistoryWebSvc>, ITmkAssignmentApiService
    {
        private readonly ITmkTrademarkService _trademarkService;

        public TmkAssignmentApiService(
            ITmkTrademarkService trademarkService,
            ICPiDbContext cpiDbContext, ClaimsPrincipal user) : base(cpiDbContext, user)
        {
            _trademarkService = trademarkService;
        }

        IQueryable<TmkAssignmentHistory> IWebApiBaseService<TmkAssignmentHistoryWebSvc, TmkAssignmentHistory>.QueryableList => _cpiDbContext.GetRepository<TmkAssignmentHistory>().QueryableList;

        public IQueryable<TmkAssignmentStatus> AssignmentStatuses => _cpiDbContext.GetRepository<TmkAssignmentStatus>().QueryableList;

        public Task<int> Add(TmkAssignmentHistoryWebSvc webApiEntity, DateTime runDate)
        {
            throw new NotImplementedException();
        }

        public async Task<List<int>> Import(List<TmkAssignmentHistoryWebSvc> assignments, DateTime runDate)
        {
            var tmkId = assignments.Select(a => a.TmkId).FirstOrDefault();

            Guard.Against.NullOrZero(tmkId, "TmkId");

            var statuses = assignments.Select(a => a.AssignmentStatus).ToList();
            if (statuses.Count > 0)
                Guard.Against.ValueNotAllowed(await _cpiDbContext.GetReadOnlyRepositoryAsync<TmkAssignmentStatus>().QueryableList
                    .AnyAsync(s => statuses.Contains(s.AssignmentStatus)), "AssignmentStatus");

            var updated = new List<TmkAssignmentHistory>();
            var deleted = new List<TmkAssignmentHistory>();
            var added = new List<TmkAssignmentHistory>();

            foreach (var assignment in assignments)
            {
                Guard.Against.NullOrEmpty(assignment.AssignmentFrom, "AssignmentFrom");
                Guard.Against.NullOrEmpty(assignment.AssignmentTo, "AssignmentTo");
                Guard.Against.Null(assignment.AssignmentDate, "AssignmentDate");

                if (!string.IsNullOrEmpty(assignment.AssignmentFrom) && !string.IsNullOrEmpty(assignment.AssignmentTo))
                    added.Add(new TmkAssignmentHistory()
                    {
                        TmkId = tmkId,
                        AssignmentFrom = assignment.AssignmentFrom,
                        AssignmentTo = assignment.AssignmentTo,
                        AssignmentDate = assignment.AssignmentDate,
                        AssignmentStatus = assignment.AssignmentStatus,
                        Reel = assignment.Reel,
                        Frame = assignment.Frame,
                        DateCreated = runDate,
                        CreatedBy = _user.GetUserName(),
                        LastUpdate = runDate,
                        UpdatedBy = _user.GetUserName()
                    });
            }

            await _trademarkService.UpdateChild(tmkId, _user.GetUserName(), updated, added, deleted);
            return added.Select(a => a.HistoryId).ToList();
        }

        public Task Update(int id, TmkAssignmentHistoryWebSvc webApiEntity, DateTime runDate)
        {
            throw new NotImplementedException();
        }

        public Task Update(List<TmkAssignmentHistoryWebSvc> webApiEntities, DateTime runDate)
        {
            throw new NotImplementedException();
        }

        public async Task Delete(TmkAssignmentHistory assignment)
        {
            var deleted = new List<TmkAssignmentHistory>() { assignment };
            await _trademarkService.UpdateChild(assignment.TmkId, _user.GetUserName(), new List<TmkAssignmentHistory>(), new List<TmkAssignmentHistory>(), deleted);
        }
    }
}
