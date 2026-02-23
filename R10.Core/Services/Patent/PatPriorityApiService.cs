using Microsoft.EntityFrameworkCore;
using R10.Core.Entities;
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
    public interface IPatPriorityApiService : IWebApiBaseService<PatPriorityWebSvc, PatPriority>
    {
        Task Delete(List<PatPriorityWebSvc> priorities, DateTime runDate);
    }

    public class PatPriorityApiService : WebApiBaseService<PatPriorityWebSvc>, IPatPriorityApiService
    {
        private readonly IChildEntityService<Invention, PatPriority> _priorityService;

        public PatPriorityApiService(
            IChildEntityService<Invention, PatPriority> priorityService,
            ICPiDbContext cpiDbContext, ClaimsPrincipal user) : base(cpiDbContext, user)
        {
            _priorityService = priorityService;
        }

        IQueryable<PatPriority> IWebApiBaseService<PatPriorityWebSvc, PatPriority>.QueryableList => _priorityService.QueryableList;

        public Task<int> Add(PatPriorityWebSvc webApiEntity, DateTime runDate)
        {
            throw new NotImplementedException();
        }

        public Task Update(int id, PatPriorityWebSvc webApiEntity, DateTime runDate)
        {
            throw new NotImplementedException();
        }

        public async Task<List<int>> Import(List<PatPriorityWebSvc> priorities, DateTime runDate)
        {
            var invId = priorities.Select(p => p.InvId).FirstOrDefault();
            Guard.Against.NullOrZero(invId, "InvId");

            var added = new List<PatPriority>();
            var errors = new List<string>();

            for (int i = 0; i < priorities.Count; i++)
            {
                var priority = priorities[i];
                try
                {
                    //set empty dates to null
                    if (priority.FilDate == EmptyDate) priority.FilDate = null;

                    added.Add(new PatPriority()
                    {
                        InvId = invId,
                        Country = priority.Country,
                        CaseType = priority.CaseType,
                        AppNumber = priority.AppNumber,
                        FilDate = priority.FilDate,
                        AccessCode = priority.AccessCode,
                        DateCreated = runDate,
                        CreatedBy = _user.GetUserName(),
                        LastUpdate = runDate,
                        UpdatedBy = _user.GetUserName()
                    });
                }
                catch (Exception ex)
                {
                    errors.Add(FormatErrorMessage(i, ex.Message, priority.InvId.ToString(), priority.PriId.ToString()));
                }
            }

            if (errors.Count > 0)
                throw new WebApiValidationException(errors);

            await _priorityService.Update(invId, _user.GetUserName(), new List<PatPriority>(), added, new List<PatPriority>());

            //get new PriIds using unique key
            foreach (var priority in priorities)
            {
                var patPriority = added.FirstOrDefault(p => p.InvId == priority.InvId && p.Country == priority.Country && p.CaseType == priority.CaseType && p.AppNumber == priority.AppNumber && p.FilDate == priority.FilDate);
                if (patPriority != null)
                    priority.PriId = patPriority.PriId;
            }

            return added.Select(p => p.PriId).ToList();
        }

        public async Task Update(List<PatPriorityWebSvc> priorities, DateTime runDate)
        {
            var invId = priorities.Select(p => p.InvId).FirstOrDefault();
            Guard.Against.NullOrZero(invId, "InvId");

            var priIds = priorities.Select(p => p.PriId).ToList();
            var patPriorities = await _priorityService.QueryableList.Where(p => p.InvId == invId && priIds.Contains(p.PriId)).ToListAsync();
            var updated = new List<PatPriority>();
            var errors = new List<string>();

            for (int i = 0; i < priorities.Count; i++)
            {
                var priority = priorities[i];
                var patPriority = patPriorities.FirstOrDefault(p => p.PriId == priority.PriId);
                try
                {
                    Guard.Against.RecordNotFound(patPriority != null);

                    if (patPriority != null)
                    {
                        //set text fields if values are not null or empty
                        //set text fields to null if values are empty string
                        if (!string.IsNullOrEmpty(priority.Country)) patPriority.Country = priority.Country;
                        else if (priority.Country == "") patPriority.Country = null;

                        if (!string.IsNullOrEmpty(priority.CaseType)) patPriority.CaseType = priority.CaseType;
                        else if (priority.CaseType == "") patPriority.CaseType = null;

                        if (!string.IsNullOrEmpty(priority.AppNumber)) patPriority.AppNumber = priority.AppNumber;
                        else if (priority.AppNumber == "") patPriority.AppNumber = null;

                        if (!string.IsNullOrEmpty(priority.AccessCode)) patPriority.AccessCode = priority.AccessCode;
                        else if (priority.AccessCode == "") patPriority.AccessCode = null;

                        //set date fields if values are not null or EmptyDate
                        //set date fields to null if value is EmptyDate
                        if (priority.FilDate != null && priority.FilDate != EmptyDate) patPriority.FilDate = priority.FilDate;
                        else if (priority.FilDate == EmptyDate) patPriority.FilDate = null;

                        patPriority.LastUpdate = runDate;
                        patPriority.UpdatedBy = _user.GetUserName();

                        updated.Add(patPriority);
                    }
                }
                catch (Exception ex)
                {
                    errors.Add(FormatErrorMessage(i, ex.Message, priority.InvId.ToString(), priority.PriId.ToString()));
                }
            }

            if (errors.Count > 0)
                throw new WebApiValidationException(errors);

            await _priorityService.Update(invId, _user.GetUserName(), updated, new List<PatPriority>(), new List<PatPriority>());
        }

        public async Task Delete(List<PatPriorityWebSvc> priorities, DateTime runDate)
        {
            var invId = priorities.Select(p => p.InvId).FirstOrDefault();
            Guard.Against.NullOrZero(invId, "InvId");

            var priIds = priorities.Select(p => p.PriId).ToList();
            var patPriorities = await _priorityService.QueryableList.Where(p => p.InvId == invId && priIds.Contains(p.PriId)).ToListAsync();
            var deleted = new List<PatPriority>();
            var errors = new List<string>();

            for (int i = 0; i < priorities.Count; i++)
            {
                var priority = priorities[i];
                var patPriority = patPriorities.FirstOrDefault(p => p.PriId == priority.PriId);

                try
                {
                    Guard.Against.RecordNotFound(patPriority != null);

                    if (patPriority != null)
                    {
                        priority.Country = patPriority.Country;
                        priority.CaseType = patPriority.CaseType;
                        priority.AppNumber = patPriority.AppNumber;
                        priority.FilDate = patPriority.FilDate;
                        priority.AccessCode = patPriority.AccessCode;
                        priority.ParentAppId = patPriority.ParentAppId;
                        priority.AppNumberSearch = patPriority.AppNumberSearch;

                        deleted.Add(patPriority);
                    }
                }
                catch (Exception ex)
                {
                    errors.Add(FormatErrorMessage(i, ex.Message, priority.InvId.ToString(), priority.PriId.ToString()));
                }
            }

            if (errors.Count > 0)
                throw new WebApiValidationException(errors);

            await _priorityService.Update(invId, _user.GetUserName(), new List<PatPriority>(), new List<PatPriority>(), deleted);
        }
    }
}
