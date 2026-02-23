using Microsoft.EntityFrameworkCore;
using R10.Core.Entities;
using R10.Core.Entities.DMS;
using R10.Core.Entities.Patent;
using R10.Core.Exceptions;
using R10.Core.Identity;
using R10.Core.Interfaces;
using R10.Core.Interfaces.DMS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Services
{
    public class DMSEntityReviewerService : IDMSEntityReviewerService
    {
        protected readonly ICPiDbContext _cpiDbContext;
        private readonly ISystemSettings<DMSSetting> _dmsSettings;
        public DMSEntityReviewerService(ICPiDbContext cpiDbContext, ISystemSettings<DMSSetting> dmsSettings)
        {
            _cpiDbContext = cpiDbContext;
            _dmsSettings = dmsSettings;
        }

        public IQueryable<DMSEntityReviewer> QueryableList => _cpiDbContext.GetRepository<DMSEntityReviewer>().QueryableList;

        public async Task<bool> Update(int entityId, DMSReviewerType entityType, string userName, IEnumerable<DMSEntityReviewer> updated, IEnumerable<DMSEntityReviewer> added, IEnumerable<DMSEntityReviewer> deleted)
        {
            var lastUpdate = DateTime.Now;

            await ValidateParentEntity(entityId, entityType, userName);

            foreach (var item in updated)
            {
                Guard.Against.NullOrZero(item.ReviewerId, "Reviewer");

                item.EntityId = entityId;
                item.EntityType = entityType;
                item.UpdatedBy = userName;
                item.LastUpdate = lastUpdate;
            }

            foreach (var item in added)
            {
                Guard.Against.NullOrZero(item.ReviewerId, "Reviewer");

                item.EntityId = entityId;
                item.EntityType = entityType;
                item.CreatedBy = userName;
                item.DateCreated = lastUpdate;
                item.UpdatedBy = userName;
                item.LastUpdate = lastUpdate;
            }

            var repository = _cpiDbContext.GetRepository<DMSEntityReviewer>();
            repository.Delete(deleted);
            repository.Update(updated);
            repository.Add(added);

            await _cpiDbContext.SaveChangesAsync();
            return true;
        }

        private async Task ValidateParentEntity(int entityId, DMSReviewerType entityType, string userName)
        {
            switch (entityType)
            {
                case DMSReviewerType.Client:
                    var client = await _cpiDbContext.GetRepository<Client>().GetByIdAsync(entityId);
                    Guard.Against.NoRecordPermission(client != null);
                    if (client != null)
                    {
                        _cpiDbContext.GetRepository<Client>().Attach(client);
                        client.UpdatedBy = userName;
                        client.LastUpdate = DateTime.Now;
                    }                    
                    break;

                case DMSReviewerType.Area:
                    var patArea = await _cpiDbContext.GetRepository<PatArea>().GetByIdAsync(entityId);
                    Guard.Against.NoRecordPermission(patArea != null);
                    if (patArea != null)
                    {
                        _cpiDbContext.GetRepository<PatArea>().Attach(patArea);
                        patArea.UpdatedBy = userName;
                        patArea.LastUpdate = DateTime.Now;
                    }                    
                    break;

                default:
                    Guard.Against.ValueNotAllowed(false, "EntityType");
                    break;
            }
        }
        
        public async Task<bool> IsDefaultReviewer(CPiEntityType reviewerType, int reviewerId)
        {
            return await QueryableList.AnyAsync(d => d.EntityType == DMSReviewerType.None && d.EntityId == null
                                                                        && d.ReviewerType == reviewerType && d.ReviewerId == reviewerId);
        }

        public async Task UpdateDefaultReviewer(CPiEntityType reviewerType, int reviewerId, string userName, bool isReviewer)
        {

            var reviewer = await QueryableList.SingleOrDefaultAsync(d => d.EntityType == DMSReviewerType.None && d.EntityId == null
                                                                        && d.ReviewerType == reviewerType && d.ReviewerId == reviewerId);
            if (isReviewer)
            {
                if (reviewer == null)
                {
                    var newReviewer = new DMSEntityReviewer()
                    {
                        EntityType = DMSReviewerType.None,
                        EntityId = null,
                        ReviewerType = reviewerType,
                        ReviewerId = reviewerId,
                        CreatedBy = userName,
                        UpdatedBy = userName,
                        DateCreated = DateTime.Now,
                        LastUpdate = DateTime.Now
                    };
                    _cpiDbContext.GetRepository<DMSEntityReviewer>().Add(newReviewer);
                    await _cpiDbContext.SaveChangesAsync();
                }
            }
            else
            {
                if (reviewer != null)
                {
                    _cpiDbContext.GetRepository<DMSEntityReviewer>().Delete(reviewer);
                    await _cpiDbContext.SaveChangesAsync();
                }
            }
        }
    }
}
