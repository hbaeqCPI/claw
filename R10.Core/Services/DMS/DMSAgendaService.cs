using System;
using R10.Core.Interfaces;
using System.Threading.Tasks;
using System.Collections.Generic;
using R10.Core.Entities;
using R10.Core.Interfaces.DMS;
using R10.Core.Entities.DMS;
using R10.Core.Identity;
using System.Linq.Expressions;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using R10.Core.Helpers;
using R10.Core.Exceptions;

namespace R10.Core.Services
{
    public class DMSAgendaService : EntityService<DMSAgenda>, IDMSAgendaService
    {
        private readonly ISystemSettings<DMSSetting> _settings;
        private readonly IDisclosureService _disclosureService;

        public DMSAgendaService(
            ICPiDbContext cpiDbContext,
            ClaimsPrincipal user,
            ISystemSettings<DMSSetting> settings,
            IDisclosureService disclosureService
            ) : base(cpiDbContext, user)

        {
            _settings = settings;
            _disclosureService = disclosureService;
        }

        private DMSReviewerType ReviewerEntityType => _settings.GetSetting().Result.ReviewerEntityType;

        public override IQueryable<DMSAgenda> QueryableList
        {
            get
            {
                IQueryable<DMSAgenda> agendas = base.QueryableList;

                if (_user.HasEntityFilter())
                    agendas = agendas.Where(EntityFilter());

                return agendas;
            }
        }       
        
        public Expression<Func<DMSAgenda, bool>> EntityFilter()
        {
            switch (_user.GetEntityFilterType())
            {
                case CPiEntityType.Client:
                    return d => UserEntityFilters.Any(f => f.UserId == UserId && f.EntityId == d.ClientID);

                //case CPiEntityType.Owner:
                //    return d => UserEntityFilters.Any(f => f.UserId == UserId && f.EntityId == d.OwnerID);
            }

            return a => true; ;
        }
       
        public override async Task<DMSAgenda> GetByIdAsync(int agendaId)
        {
            var agenda = await QueryableList.SingleOrDefaultAsync(d => d.AgendaId == agendaId);
            return agenda ?? new DMSAgenda();
        }               

        public async Task AddAgenda(DMSAgenda agenda, bool hasReviewersCopy = false, bool hasDisclosuresCopy = false)
        {
            //ONLY MODIFY USERS CAN ADD AGENDA MEETING            
            Guard.Against.NoRecordPermission(await ValidateRole(CPiPermissions.FullModify));

            _cpiDbContext.GetRepository<DMSAgenda>().Add(agenda);
            await _cpiDbContext.SaveChangesAsync();

            await AddDefaults(agenda, hasReviewersCopy, hasDisclosuresCopy);
        }
        
        public override async Task Update(DMSAgenda agenda)
        {            
            //ONLY MODIFY USERS CAN UPDATE AGENDA
            Guard.Against.NoRecordPermission(await ValidateRole(CPiPermissions.FullModify));                        
            
            _cpiDbContext.GetRepository<DMSAgenda>().Update(agenda);            
            await _cpiDbContext.SaveChangesAsync();

            await AddDefaults(agenda);
        }

        public override async Task Delete(DMSAgenda agenda)
        {            
            //ONLY MODIFY CAN UPDATE AGENDA
            Guard.Against.NoRecordPermission(await ValidateRole(CPiPermissions.FullModify));
            
            _cpiDbContext.GetRepository<DMSAgenda>().Delete(agenda);
            await _cpiDbContext.SaveChangesAsync();
        }

        public async Task CopyAgenda(int oldAgendaId, int newAgendaId, string userName, bool copyReviewers, bool copyDisclosures)
        {                     
            var today = DateTime.Now;
            if (copyReviewers)
            {
                var reviewers = await _cpiDbContext.GetReadOnlyRepositoryAsync<DMSAgendaReviewer>().QueryableList
                                            .Where(d => d.AgendaId == oldAgendaId)
                                            .Select(d => new DMSAgendaReviewer
                                            {
                                                AgendaId = newAgendaId,
                                                ReviewerType = d.ReviewerType,
                                                ReviewerId = d.ReviewerId,
                                                CreatedBy = userName,
                                                UpdatedBy = userName,
                                                DateCreated = today,
                                                LastUpdate = today
                                            }).ToListAsync();
                if (reviewers != null && reviewers.Count > 0) 
                    _cpiDbContext.GetRepository<DMSAgendaReviewer>().Add(reviewers);
            }
            if (copyDisclosures)
            {
                var relatedDisclosures = await _cpiDbContext.GetReadOnlyRepositoryAsync<DMSAgendaRelatedDisclosure>().QueryableList
                                            .Where(d => d.AgendaId == oldAgendaId)
                                            .Select(d => new DMSAgendaRelatedDisclosure
                                            {
                                                AgendaId = newAgendaId,
                                                DMSId = d.DMSId,
                                                CreatedBy = userName,
                                                UpdatedBy = userName,
                                                DateCreated = today,
                                                LastUpdate = today
                                            })
                                            .ToListAsync();
                if (relatedDisclosures != null && relatedDisclosures.Count > 0)
                    _cpiDbContext.GetRepository<DMSAgendaRelatedDisclosure>().Add(relatedDisclosures);
            }

            if (copyReviewers || copyDisclosures)
                await _cpiDbContext.SaveChangesAsync();
        }

        private async Task<bool> ValidateRole(List<string> roles)
        {
            return await ValidatePermission(SystemType.DMS, roles, null);
        }        

        private async Task AddDefaults(DMSAgenda agenda, bool hasReviewersCopy = false, bool hasDisclosuresCopy = false)
        {
            var reviewers = new List<DMSAgendaReviewer>();
            var relatedDisclosures = new List<DMSAgendaRelatedDisclosure>();

            if (agenda != null && agenda.AgendaId > 0 && (agenda.ClientID > 0 || agenda.AreaID > 0))
            {
                //Populate default reviewers based on ClientID or AreaID; Only add if agenda doesn't have any reviewers
                var hasReviewers = await _cpiDbContext.GetReadOnlyRepositoryAsync<DMSAgendaReviewer>().QueryableList.AnyAsync(d => d.AgendaId == agenda.AgendaId);      
                if (!hasReviewers && !hasReviewersCopy)
                {
                    reviewers = await _cpiDbContext.GetReadOnlyRepositoryAsync<DMSEntityReviewer>().QueryableList
                                            .Where(d =>
                                                (ReviewerEntityType == DMSReviewerType.Client && agenda.ClientID > 0 && d.EntityType == DMSReviewerType.Client && d.EntityId == agenda.ClientID) ||
                                                (ReviewerEntityType == DMSReviewerType.Area && agenda.AreaID > 0 && d.EntityType == DMSReviewerType.Area && d.EntityId == agenda.AreaID)
                                            )
                                            .Select(d => new DMSAgendaReviewer
                                            {
                                                AgendaId = agenda.AgendaId,
                                                ReviewerType = d.ReviewerType,
                                                ReviewerId = d.ReviewerId,
                                                CreatedBy = agenda.UpdatedBy,
                                                UpdatedBy = agenda.UpdatedBy,
                                                DateCreated = agenda.LastUpdate,
                                                LastUpdate = agenda.LastUpdate
                                            }).ToListAsync();
                }
                    
                //Populate default submitted disclosures without recommendation regardless of any existing ratings
                var hasRelatedDisclosures = await _cpiDbContext.GetReadOnlyRepositoryAsync<DMSAgendaRelatedDisclosure>().QueryableList.AnyAsync(d => d.AgendaId == agenda.AgendaId);
                if (!hasRelatedDisclosures && !hasDisclosuresCopy)
                {
                    //var statusList = new List<string> { InitialReviewStatus, FinalReviewStatus, UnderReviewStatus };
                    relatedDisclosures = await _disclosureService.ForReviewList
                                            .Where(d =>
                                                //!string.IsNullOrEmpty(d.DisclosureStatus) && statusList.Any(s => s.ToLower() == d.DisclosureStatus.ToLower()) &&
                                                string.IsNullOrEmpty(d.Recommendation) &&
                                                ((ReviewerEntityType == DMSReviewerType.Client && agenda.ClientID > 0 && d.ClientID == agenda.ClientID) ||
                                                (ReviewerEntityType == DMSReviewerType.Area && agenda.AreaID > 0 && d.AreaID == agenda.AreaID))
                                            )
                                            .Select(d => new DMSAgendaRelatedDisclosure
                                            {
                                                AgendaId = agenda.AgendaId,
                                                DMSId = d.DMSId,
                                                CreatedBy = agenda.UpdatedBy,
                                                UpdatedBy = agenda.UpdatedBy,
                                                DateCreated = agenda.LastUpdate,
                                                LastUpdate = agenda.LastUpdate
                                            })
                                            .ToListAsync();
                }
                    
            }

            if (reviewers.Count > 0)
                _cpiDbContext.GetRepository<DMSAgendaReviewer>().Add(reviewers);

            if (relatedDisclosures.Count > 0)
                _cpiDbContext.GetRepository<DMSAgendaRelatedDisclosure>().Add(relatedDisclosures);

            if (reviewers.Count > 0 || relatedDisclosures.Count > 0)
                await _cpiDbContext.SaveChangesAsync();            
        }
    }
}