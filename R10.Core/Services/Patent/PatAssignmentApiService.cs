using Microsoft.EntityFrameworkCore;
using R10.Core.Entities.Patent;
using R10.Core.Exceptions;
using R10.Core.Helpers;
using R10.Core.Interfaces;
using R10.Core.Interfaces.Patent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Services.Patent
{
    public interface IPatAssignmentApiService : IWebApiBaseService<PatAssignmentHistoryWebSvc, PatAssignmentHistory>
    {
        IQueryable<PatAssignmentStatus> AssignmentStatuses { get; }
        Task Delete(PatAssignmentHistory assignment);
    }

    public class PatAssignmentApiService : WebApiBaseService<PatAssignmentHistoryWebSvc>, IPatAssignmentApiService
    {
        private readonly ICountryApplicationService _countryAppService;

        public PatAssignmentApiService(
            ICountryApplicationService countryAppService,
            ICPiDbContext cpiDbContext, ClaimsPrincipal user) : base(cpiDbContext, user)
        {
            _countryAppService = countryAppService;
        }

        IQueryable<PatAssignmentHistory> IWebApiBaseService<PatAssignmentHistoryWebSvc, PatAssignmentHistory>.QueryableList => _cpiDbContext.GetRepository<PatAssignmentHistory>().QueryableList;

        public IQueryable<PatAssignmentStatus> AssignmentStatuses => _cpiDbContext.GetRepository<PatAssignmentStatus>().QueryableList;

        public Task<int> Add(PatAssignmentHistoryWebSvc webApiEntity, DateTime runDate)
        {
            throw new NotImplementedException();
        }

        public async Task<List<int>> Import(List<PatAssignmentHistoryWebSvc> assignments, DateTime runDate)
        {
            var appId = assignments.Select(a => a.AppId).FirstOrDefault();

            Guard.Against.NullOrZero(appId, "AppId");

            var statuses = assignments.Select(a => a.AssignmentStatus).ToList();
            if (statuses.Count > 0)
                Guard.Against.ValueNotAllowed(await _cpiDbContext.GetReadOnlyRepositoryAsync<PatAssignmentStatus>().QueryableList
                    .AnyAsync(s => statuses.Contains(s.AssignmentStatus)), "AssignmentStatus");

            var updated = new List<PatAssignmentHistory>();
            var deleted = new List<PatAssignmentHistory>();
            var added = new List<PatAssignmentHistory>();

            foreach (var assignment in assignments)
            {
                Guard.Against.NullOrEmpty(assignment.AssignmentFrom, "AssignmentFrom");
                Guard.Against.NullOrEmpty(assignment.AssignmentTo, "AssignmentTo");
                Guard.Against.Null(assignment.AssignmentDate, "AssignmentDate");

                if (!string.IsNullOrEmpty(assignment.AssignmentFrom) && !string.IsNullOrEmpty(assignment.AssignmentTo))
                    added.Add(new PatAssignmentHistory()
                    {
                        AppId = appId,
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

            await _countryAppService.UpdateChild(appId, _user.GetUserName(), updated, added, deleted);
            return added.Select(a => a.HistoryId).ToList();
        }

        public Task Update(int id, PatAssignmentHistoryWebSvc webApiEntity, DateTime runDate)
        {
            throw new NotImplementedException();
        }

        public Task Update(List<PatAssignmentHistoryWebSvc> webApiEntities, DateTime runDate)
        {
            throw new NotImplementedException();
        }

        public async Task Delete(PatAssignmentHistory assignment)
        {
            var deleted = new List<PatAssignmentHistory>() { assignment };
            await _countryAppService.UpdateChild(assignment.AppId, _user.GetUserName(), new List<PatAssignmentHistory>(), new List<PatAssignmentHistory>(), deleted);
        }
    }
}
