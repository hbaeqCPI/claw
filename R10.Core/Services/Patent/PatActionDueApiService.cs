using Microsoft.EntityFrameworkCore;
using R10.Core.Entities.Patent;
using R10.Core.Exceptions;
using R10.Core.Helpers;
using R10.Core.Interfaces;
using R10.Core.Interfaces.Patent;
using System.Security.Claims;

namespace R10.Core.Services.Patent
{
    public interface IPatActionDueApiService : IWebApiBaseService<PatActionDueWebSvc, PatActionDue>
    {
        IQueryable<PatDueDate> DueDates { get; }
        Task<List<int>> AddDueDates(int actId, List<PatDueDateWebSvc> webApiDueDates, DateTime runDate);
        Task UpdateDueDates(int actId, List<PatDueDateWebSvc> webApiDueDates, DateTime runDate);
        Task LogApiDueDates(List<PatDueDateWebSvc> webApiDueDates);
    }

    public class PatActionDueApiService : WebApiBaseService<PatActionDueWebSvc>, IPatActionDueApiService
    {
        private readonly IActionDueDeDocketService<PatActionDue, PatDueDate> _actionDueService;
        private readonly IDueDateService<PatActionDue, PatDueDate> _dueDateService;
        private readonly ICountryApplicationService _countryAppService;

        public IQueryable<PatDueDate> DueDates => _dueDateService.QueryableList;

        IQueryable<PatActionDue> IWebApiBaseService<PatActionDueWebSvc, PatActionDue>.QueryableList => _actionDueService.QueryableList;

        public PatActionDueApiService(
            IActionDueDeDocketService<PatActionDue, PatDueDate> actionDueService, 
            IDueDateService<PatActionDue, PatDueDate> dueDateService,
            ICountryApplicationService countryAppService,
            ICPiDbContext cpiDbContext, 
            ClaimsPrincipal user) : base(cpiDbContext, user)
        {
            _actionDueService = actionDueService;
            _dueDateService = dueDateService;
            _countryAppService = countryAppService;
        }

        public async Task<int> Add(PatActionDueWebSvc webApiActionDue, DateTime runDate)
        {
            await ValidateActionDue(0, webApiActionDue, true);
            return await SaveActionDue(webApiActionDue, new PatActionDue(), runDate);
        }

        public async Task<List<int>> Import(List<PatActionDueWebSvc> webApiActionsDue, DateTime runDate)
        {
            await ValidateActionsDue(webApiActionsDue, true);

            var actIds = new List<int>();
            foreach (var webApiActionDue in webApiActionsDue)
            {
                actIds.Add(await SaveActionDue(webApiActionDue, new PatActionDue(), runDate));
            }

            return actIds;
        }

        public async Task Update(int id, PatActionDueWebSvc webApiActionDue, DateTime runDate)
        {
            await ValidateActionDue(id, webApiActionDue, false);

            var actionDue = await _actionDueService.QueryableList.FirstOrDefaultAsync(a => a.ActId == id);
            if (actionDue != null)
                await SaveActionDue(webApiActionDue, actionDue, runDate);
        }

        public async Task Update(List<PatActionDueWebSvc> webApiActionsDue, DateTime runDate)
        {
            await ValidateActionsDue(webApiActionsDue, false);

            foreach (var webApiActionDue in webApiActionsDue)
            {
                var actionDue = await _actionDueService.QueryableList
                                    .FirstOrDefaultAsync(c => c.CaseNumber == webApiActionDue.CaseNumber && c.Country == webApiActionDue.Country && c.SubCase == webApiActionDue.SubCase &&
                                                c.ActionType == webApiActionDue.ActionType && c.BaseDate == webApiActionDue.BaseDate);

                if (actionDue != null)
                    await SaveActionDue(webApiActionDue, actionDue, runDate);
            }
        }

        public async Task<List<int>> AddDueDates(int actId, List<PatDueDateWebSvc> webApiDueDates, DateTime runDate)
        {
            await ValidateDueDates(actId, webApiDueDates, true);

            var ddIds = new List<int>();
            foreach (var webApiDueDate in webApiDueDates)
            {
                ddIds.Add(await SaveDueDate(actId, webApiDueDate, new PatDueDate(), runDate));
            }

            return ddIds;
        }

        public async Task UpdateDueDates(int actId, List<PatDueDateWebSvc> webApiDueDates, DateTime runDate)
        {
            await ValidateDueDates(actId, webApiDueDates);

            foreach (var webApiDueDate in webApiDueDates)
            {
                var dueDate = await _dueDateService.QueryableList
                                    .Where(d => d.ActId == actId && d.ActionDue == webApiDueDate.ActionDue && d.DueDate == webApiDueDate.DueDate)
                                    .FirstOrDefaultAsync();

                if (dueDate != null)
                    await SaveDueDate(actId, webApiDueDate, dueDate, runDate);
            }
        }

        private async Task<int> SaveActionDue(PatActionDueWebSvc webApiActionDue, PatActionDue actionDue, DateTime runDate)
        {
            await SetData(webApiActionDue, actionDue, runDate);

            if (actionDue.ActId == 0)
                await _actionDueService.Add(actionDue);
            else
                await _actionDueService.Update(actionDue);

            return actionDue.ActId;
        }

        private async Task<int> SaveDueDate(int actId, PatDueDateWebSvc webApiDueDate, PatDueDate dueDate, DateTime runDate)
        {
            await SetDueDateData(actId, webApiDueDate, dueDate, runDate);

            if (dueDate.DDId == 0)
                await _dueDateService.Add(dueDate);
            else
                await _dueDateService.Update(dueDate);

            return dueDate.DDId;
        }

        private async Task ValidateActionDue(int id, PatActionDueWebSvc webApiActionDue, bool forInsert)
        {
            try
            {
                await ValidateData(id, webApiActionDue, forInsert);
            }
            catch (Exception ex)
            {
                throw new WebApiValidationException(FormatErrorMessage(id, ex.Message, webApiActionDue.CaseNumber, webApiActionDue.Country, webApiActionDue.SubCase, webApiActionDue.ActionType));
            }
        }

        private async Task ValidateActionsDue(List<PatActionDueWebSvc> webApiActionsDue, bool forInsert)
        {
            var errors = new List<string>();
            var duplicates = webApiActionsDue.GroupBy(a => new { a.CaseNumber, a.Country, a.SubCase, a.ActionType, a.BaseDate }).Where(g => g.Count() > 1).ToList();

            if (duplicates.Count > 0)
                throw new WebApiValidationException("Duplicate records found.", duplicates.Select(d => $"{d.Key}").ToList());

            for (int i = 0; i < webApiActionsDue.Count; i++)
            {
                var webApiActionDue = webApiActionsDue[i];

                try
                {
                    await ValidateData(0, webApiActionDue, forInsert);
                }
                catch (Exception ex)
                {
                    errors.Add(FormatErrorMessage(i, ex.Message, webApiActionDue.CaseNumber, webApiActionDue.Country, webApiActionDue.SubCase, webApiActionDue.ActionType));
                }
            }

            if (errors.Count > 0)
                throw new WebApiValidationException(errors);
        }

        private async Task ValidateData(int id, PatActionDueWebSvc webApiActionDue, bool forInsert)
        {
            //check key fields
            Guard.Against.NullOrEmpty(webApiActionDue.CaseNumber, "CaseNumber");
            Guard.Against.NullOrEmpty(webApiActionDue.Country, "Country");
            webApiActionDue.SubCase = webApiActionDue.SubCase ?? "";

            Guard.Against.NullOrEmpty(webApiActionDue.ActionType, "ActionType");
            Guard.Against.Null(webApiActionDue.BaseDate, "BaseDate");

            //use queryable list without filters when adding new records
            //use queryable list from action due service when updating records to enforce resp office and entity filters
            var actions = forInsert ? 
                _cpiDbContext.GetReadOnlyRepositoryAsync<PatActionDue>().QueryableList : 
                _actionDueService.QueryableList;
            var isFound = await actions
                                .AnyAsync(a => (id != 0 && a.ActId == id) || (id == 0 &&
                                    a.CaseNumber == webApiActionDue.CaseNumber && a.Country == webApiActionDue.Country && a.SubCase == webApiActionDue.SubCase &&
                                    a.ActionType == webApiActionDue.ActionType && a.BaseDate == webApiActionDue.BaseDate));

            if (forInsert)
            {
                //check if action already exists
                Guard.Against.RecordExists(isFound);

                //check if ctry app exists or if user has permissions
                Guard.Against.ValueNotAllowed(await _countryAppService.CountryApplications.AnyAsync(ca => ca.CaseNumber == webApiActionDue.CaseNumber && ca.Country == webApiActionDue.Country && ca.SubCase == webApiActionDue.SubCase), "Country Application");

                //check due dates when creating actions due
                //due dates are ignored when updating actions due
                //use due dates endpoint to update due dates
                if (webApiActionDue.DueDates != null)
                {
                    //required fields
                    if (webApiActionDue.DueDates.Exists(a => string.IsNullOrEmpty(a.ActionDue)))
                        Guard.Against.NullOrEmpty("", "ActionDue");

                    //DueDate is never equal to null
                    if (webApiActionDue.DueDates.Exists(a => a.DueDate == EmptyDate))
                        Guard.Against.NullOrEmpty("", "DueDate");

                    if (webApiActionDue.DueDates.Exists(a => string.IsNullOrEmpty(a.Indicator)))
                        Guard.Against.NullOrEmpty("", "Indicator");

                    //check indicator
                    var indicators = webApiActionDue.DueDates.Select(d => d.Indicator).Distinct().ToList();
                    if (indicators.Any())
                        Guard.Against.ValueNotAllowed(indicators.Intersect(await _cpiDbContext.GetReadOnlyRepositoryAsync<PatIndicator>().QueryableList.Select(i => i.Indicator).Distinct().ToListAsync()).Count() == indicators.Count, "Indicator");

                    //check shared aux permission if attorney does not exist
                    var attorneys = webApiActionDue.DueDates.Where(d => !string.IsNullOrEmpty(d.Attorney)).Select(d => d.Attorney ?? "").Distinct().ToList();
                    if (attorneys.Any() && !HasSharedAuxModify)
                        Guard.Against.ValueNotAllowed(await HasAttorneys(attorneys), "Attorney");
                }
            }
            else
            {
                //check if record exists or if user has permissions
                Guard.Against.RecordNotFound(isFound);
            }

            //check shared aux permission if attorney does not exist
            if (!string.IsNullOrEmpty(webApiActionDue.Attorney) && !HasSharedAuxModify)
                Guard.Against.ValueNotAllowed(await HasAttorney(webApiActionDue.Attorney), "Attorney");
        }

        private async Task ValidateDueDates(int actId, List<PatDueDateWebSvc> webApiDueDates, bool forInsert = false)
        {
            var errors = new List<string>();
            var duplicates = webApiDueDates.GroupBy(d => new { d.ActionDue, d.DueDate }).Where(g => g.Count() > 1).ToList();

            if (duplicates.Count > 0)
                throw new WebApiValidationException("Duplicate records found.", duplicates.Select(d => $"{d.Key}").ToList());

            for (int i = 0; i < webApiDueDates.Count; i++)
            {
                var webApiDueDate = webApiDueDates[i];

                try
                {
                    await ValidateDueDateData(actId, webApiDueDate, forInsert);
                }
                catch (Exception ex)
                {
                    errors.Add(FormatErrorMessage(i, ex.Message, webApiDueDate.ActionDue, webApiDueDate.DueDate.ToString()));
                }
            }

            if (errors.Count > 0)
                throw new WebApiValidationException(errors);
        }

        private async Task ValidateDueDateData(int actId, PatDueDateWebSvc webApiDueDate, bool forInsert = false)
        {
            //check key fields
            Guard.Against.NullOrEmpty(webApiDueDate.ActionDue, "ActionDue");
            //DueDate is never equal to null
            Guard.Against.ValueNotAllowed(webApiDueDate.DueDate != EmptyDate, "DueDate");
            Guard.Against.NullOrEmpty(webApiDueDate.Indicator, "Indicator");

            //use queryable list without filters when adding new records
            //use queryable list from due date service when updating records to enforce resp office and entity filters
            var dueDates = forInsert ? 
                _cpiDbContext.GetReadOnlyRepositoryAsync<PatDueDate>().QueryableList : 
                _dueDateService.QueryableList;
            var isFound = await dueDates.AnyAsync(d => d.ActId == actId && d.ActionDue == webApiDueDate.ActionDue && d.DueDate == webApiDueDate.DueDate);

            if (forInsert)
            {
                //check if due date already exists
                Guard.Against.RecordExists(isFound);

                //check if action due exists or if user has permissions
                Guard.Against.ValueNotAllowed(await _actionDueService.QueryableList.AnyAsync(a => a.ActId == actId), "Action Due");
            }
            else
            {
                //check if record exists or if user has permissions
                Guard.Against.RecordNotFound(isFound);
            }

            //check lookup fields
            Guard.Against.ValueNotAllowed(await _cpiDbContext.GetReadOnlyRepositoryAsync<PatIndicator>().QueryableList.AnyAsync(i => i.Indicator == webApiDueDate.Indicator), "Indicator");

            //check shared aux permission if attorney does not exist
            if (!string.IsNullOrEmpty(webApiDueDate.Attorney) && !HasSharedAuxModify)
                Guard.Against.ValueNotAllowed(await HasAttorney(webApiDueDate.Attorney), "Attorney");
        }

        private async Task SetData(PatActionDueWebSvc webApiActionDue, PatActionDue actionDue, DateTime runDate)
        {
            //only update ResponseDate, ResponsibleID, Remarks fields if country law action
            if (actionDue.ActId == 0 || !actionDue.ComputerGenerated)
            {
                //set key fields
                actionDue.CaseNumber = webApiActionDue.CaseNumber ?? "";
                actionDue.Country = webApiActionDue.Country ?? "";
                actionDue.SubCase = webApiActionDue.SubCase ?? "";
                actionDue.ActionType = webApiActionDue.ActionType ?? "";
                actionDue.BaseDate = webApiActionDue.BaseDate ?? EmptyDate;

                //check due dates when creating actions due
                //due dates are ignored when updating actions due
                //use due dates endpoint to update due dates
                if (actionDue.ActId == 0 && webApiActionDue.DueDates != null)
                {
                    var dueDates = new List<PatDueDate>();
                    foreach (var dueDate in webApiActionDue.DueDates)
                    {
                        dueDates.Add(new PatDueDate()
                        {
                            ActionDue = dueDate.ActionDue ?? "",
                            DueDate = dueDate.DueDate,
                            Indicator = dueDate.Indicator,
                            DateTaken = dueDate.DateTaken,
                            AttorneyID = string.IsNullOrEmpty(dueDate.Attorney) ? null : await GetAttorneyID(dueDate.Attorney, runDate),
                            CreatedBy = actionDue.CreatedBy,
                            DateCreated = actionDue.DateCreated,
                            UpdatedBy = actionDue.UpdatedBy,
                            LastUpdate = actionDue.LastUpdate
                        });
                    }
                    actionDue.DueDates = dueDates;
                }
            }

            //set date fields if values are not null or EmptyDate
            //set date fields to null if value is EmptyDate
            if (webApiActionDue.ResponseDate != null && webApiActionDue.ResponseDate != EmptyDate) actionDue.ResponseDate = webApiActionDue.ResponseDate;
            else if (webApiActionDue.ResponseDate == EmptyDate) actionDue.ResponseDate = null;

            //set entity id fields if entity code values are not null or empty
            //set entity id fields to null if entity code values are empty string
            if (!string.IsNullOrEmpty(webApiActionDue.Attorney)) actionDue.ResponsibleID = await GetAttorneyID(webApiActionDue.Attorney, runDate);
            else if (webApiActionDue.Attorney == "") actionDue.ResponsibleID = null;

            //set text fields if values are not null or empty
            //set text fields to null if values are empty string
            if (!string.IsNullOrEmpty(webApiActionDue.Remarks)) actionDue.Remarks = webApiActionDue.Remarks;
            else if (webApiActionDue.Remarks == "") actionDue.Remarks = null;

            //update user and date stamp
            actionDue.UpdatedBy = _user.GetUserName();
            actionDue.LastUpdate = runDate;
            if (actionDue.ActId == 0)
            {
                actionDue.CreatedBy = _user.GetUserName();
                actionDue.DateCreated = runDate;
            }
        }

        private async Task SetDueDateData(int actId, PatDueDateWebSvc webApiDueDate, PatDueDate dueDate, DateTime runDate)
        {
            //only update DateTaken, AttorneyID, Remarks fields if country law action
            var isCountryLawAction = await _actionDueService.QueryableList.Where(a => a.ActId == actId).Select(a => a.ComputerGenerated).FirstOrDefaultAsync();

            if (dueDate.DDId == 0 || !isCountryLawAction)
            {
                dueDate.ActId = actId;
                dueDate.ActionDue = webApiDueDate.ActionDue ?? "";
                dueDate.DueDate = webApiDueDate.DueDate;
                dueDate.Indicator = webApiDueDate.Indicator ?? "";
            }

            //set date fields if values are not null or EmptyDate
            //set date fields to null if value is EmptyDate
            if (webApiDueDate.DateTaken != null && webApiDueDate.DateTaken != EmptyDate) dueDate.DateTaken = webApiDueDate.DateTaken;
            else if (webApiDueDate.DateTaken == EmptyDate) dueDate.DateTaken = null;

            //set entity id fields if entity code values are not null or empty
            //set entity id fields to null if entity code values are empty string
            if (!string.IsNullOrEmpty(webApiDueDate.Attorney))
                dueDate.AttorneyID = await GetAttorneyID(webApiDueDate.Attorney, runDate);
            else if (webApiDueDate.Attorney == "")
                dueDate.AttorneyID = null;

            //update user and date stamp
            dueDate.UpdatedBy = _user.GetUserName();
            dueDate.LastUpdate = runDate;
            if (dueDate.DDId == 0)
            {
                dueDate.CreatedBy = _user.GetUserName();
                dueDate.DateCreated = runDate;
            }
        }

        public async Task LogApiDueDates(List<PatDueDateWebSvc> webApiDueDates)
        {
            _cpiDbContext.GetRepository<PatDueDateWebSvc>().Add(webApiDueDates);
            await _cpiDbContext.SaveChangesAsync();
        }
    }
}
