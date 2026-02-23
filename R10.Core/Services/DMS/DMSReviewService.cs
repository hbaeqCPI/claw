using Microsoft.EntityFrameworkCore;
using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Entities.DMS;
using R10.Core.Exceptions;
using R10.Core.Helpers;
using R10.Core.Identity;
using R10.Core.Interfaces;
using R10.Core.Interfaces.DMS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Services
{
    public class DMSReviewService : DMSDisclosureChildService<DMSReview>, IDMSReviewService
    {
        private readonly ISystemSettings<DMSSetting> _settings;
        public DMSReviewService(
                    ICPiDbContext cpiDbContext,
                    ClaimsPrincipal user,
                    IDisclosureService  disclosureService,
                    ISystemSettings<DMSSetting> settings
                    ) : base(cpiDbContext, user, disclosureService)
        {
            _settings = settings;
        }

        public new async Task<(int DMSReviewId, CPiEntityType ReviewerType, int ReviewerId, byte[] tStamp)> Update(DMSReview review)
        {
            //Only admin/modify users or users with reviewer role can save ratings
            var cpiPermissions = CPiPermissions.Reviewer;
            cpiPermissions.Add("modify");
            await ValidatePermission(cpiPermissions);

            //Validate disclosure record
            var disclosure = await _disclosureService.ForReviewList.Where(r => r.DMSId == review.DMSId).FirstOrDefaultAsync();
            Guard.Against.NoRecordPermission(disclosure != null);

            _cpiDbContext.GetRepository<Disclosure>().Attach(disclosure);
            disclosure.UpdatedBy = review.UpdatedBy;
            disclosure.LastUpdate = review.LastUpdate;

            //Check if user is valid reviewer
            var settings = await _settings.GetSetting();
            var reviewerType = _user.GetEntityFilterType();
            var reviewerId = await _disclosureService.GetUserReviewerId(reviewerType);
            var reviewer = new DMSEntityReviewer();
            if (settings.IsDefaultReviewerOn)
            {
                reviewer = await _cpiDbContext.GetRepository<DMSEntityReviewer>().QueryableList
                                                .Where(r => r.EntityType == DMSReviewerType.None && r.EntityId == null &&
                                                            r.ReviewerType == reviewerType && r.ReviewerId == reviewerId).FirstOrDefaultAsync();
            }
            else
            {
                reviewer = await _cpiDbContext.GetRepository<DMSEntityReviewer>().QueryableList
                                                .Where(r => ((r.EntityType == DMSReviewerType.Client && r.EntityId == disclosure.ClientID) ||
                                                             (r.EntityType == DMSReviewerType.Area && r.EntityId == disclosure.AreaID)) &&
                                                            r.ReviewerType == reviewerType && r.ReviewerId == reviewerId).FirstOrDefaultAsync();
            }

            Guard.Against.NoRecordPermission(reviewer != null || await ValidatePermission(SystemType.DMS, CPiPermissions.FullModify, null));

            var updated = await _cpiDbContext.GetRepository<DMSReview>().QueryableList.SingleOrDefaultAsync(d => d.DMSReviewId == review.DMSReviewId);
            if (updated == null)
            {
                updated = review;
                updated.RatingDate = review.LastUpdate;
                if (reviewer == null)
                {
                    updated.ReviewerType = CPiEntityType.None;
                    updated.ReviewerId = 0;
                    updated.UserId = _user.GetUserIdentifier();
                }
                else
                {
                    updated.ReviewerType = reviewer.ReviewerType;
                    updated.ReviewerId = reviewer.ReviewerId;                    
                }                

                _cpiDbContext.GetRepository<DMSReview>().Add(updated);
            }
            else
            {
                _cpiDbContext.GetRepository<DMSReview>().Attach(updated);
                updated.RatingId = review.RatingId;
                updated.RatingDate = review.LastUpdate;
                updated.Remarks = review.Remarks;
                updated.UpdatedBy = review.UpdatedBy;
                updated.LastUpdate = review.LastUpdate;
                updated.tStamp = review.tStamp;
            }

            await _cpiDbContext.SaveChangesAsync();

            return (updated.DMSReviewId, updated.ReviewerType, (int)updated.ReviewerId, updated.tStamp);
        }

        public async Task UpdateValuation(DMSValuation valuation)
        {
            //Only users with reviewer role can save valuation
            var cpiPermissions = CPiPermissions.Reviewer;
            cpiPermissions.Add("modify");
            await ValidatePermission(cpiPermissions);

            //Validate disclosure record
            var disclosure = await _disclosureService.ForReviewList.Where(r => r.DMSId == valuation.DMSId).FirstOrDefaultAsync();
            Guard.Against.NoRecordPermission(disclosure != null);

            _cpiDbContext.GetRepository<Disclosure>().Attach(disclosure);
            disclosure.UpdatedBy = valuation.UpdatedBy;
            disclosure.LastUpdate = valuation.LastUpdate;

            var isModifyUser = await ValidatePermission(SystemType.DMS, CPiPermissions.FullModify, null);
            var reviewer = new DMSEntityReviewer();
            if (!isModifyUser)
            {
                //Check if user is valid reviewer
                var settings = await _settings.GetSetting();
                var reviewerType = _user.GetEntityFilterType();
                var reviewerId = await _disclosureService.GetUserReviewerId(reviewerType);                
                if (settings.IsDefaultReviewerOn)
                {
                    reviewer = await _cpiDbContext.GetRepository<DMSEntityReviewer>().QueryableList
                                                    .Where(r => r.EntityType == DMSReviewerType.None && r.EntityId == null &&
                                                                r.ReviewerType == reviewerType && r.ReviewerId == reviewerId).FirstOrDefaultAsync();
                }
                else
                {
                    reviewer = await _cpiDbContext.GetRepository<DMSEntityReviewer>().QueryableList
                                                    .Where(r => ((r.EntityType == DMSReviewerType.Client && r.EntityId == disclosure.ClientID) ||
                                                                 (r.EntityType == DMSReviewerType.Area && r.EntityId == disclosure.AreaID)) &&
                                                                r.ReviewerType == reviewerType && r.ReviewerId == reviewerId).FirstOrDefaultAsync();
                }
                Guard.Against.NoRecordPermission(reviewer != null);
            }            
            
            var updated = await _cpiDbContext.GetRepository<DMSValuation>().QueryableList.SingleOrDefaultAsync(d => d.DMSValId == valuation.DMSValId);
            if (updated == null)
            {
                updated = valuation;                
                updated.ReviewerType = reviewer != null ? reviewer.ReviewerType : CPiEntityType.None;
                updated.ReviewerId = reviewer != null ? reviewer.ReviewerId : 0;
                updated.UserId = _user.GetUserIdentifier();

                _cpiDbContext.GetRepository<DMSValuation>().Add(updated);
            }
            else
            {
                _cpiDbContext.GetRepository<DMSValuation>().Attach(updated);
                updated.RateId = valuation.RateId;
                updated.RatingDate = valuation.LastUpdate;
                updated.Remarks = valuation.Remarks;
                updated.UpdatedBy = valuation.UpdatedBy;
                updated.LastUpdate = valuation.LastUpdate;
                updated.tStamp = valuation.tStamp;
                updated.Weight = valuation.Weight;
            }

            await _cpiDbContext.SaveChangesAsync();
            _cpiDbContext.Detach(disclosure);
        }

        public async Task DeleteValuation(DMSValuation valuation)
        {
            //Only users with reviewer role can save valuation
            var cpiPermissions = CPiPermissions.Reviewer;
            await ValidatePermission(cpiPermissions);

            //Validate disclosure record
            var disclosure = await _disclosureService.ForReviewList.Where(r => r.DMSId == valuation.DMSId).FirstOrDefaultAsync();
            Guard.Against.NoRecordPermission(disclosure != null);

            //Check if user is valid reviewer
            var settings = await _settings.GetSetting();
            var reviewerType = _user.GetEntityFilterType();
            var reviewerId = await _disclosureService.GetUserReviewerId(reviewerType);
            var reviewer = new DMSEntityReviewer();
            if (settings.IsDefaultReviewerOn)
            {
                reviewer = await _cpiDbContext.GetRepository<DMSEntityReviewer>().QueryableList
                                                .Where(r => r.EntityType == DMSReviewerType.None && r.EntityId == null &&
                                                            r.ReviewerType == reviewerType && r.ReviewerId == reviewerId).FirstOrDefaultAsync();
            }
            else
            {
                reviewer = await _cpiDbContext.GetRepository<DMSEntityReviewer>().QueryableList
                                                .Where(r => ((r.EntityType == DMSReviewerType.Client && r.EntityId == disclosure.ClientID) ||
                                                             (r.EntityType == DMSReviewerType.Area && r.EntityId == disclosure.AreaID)) &&
                                                            r.ReviewerType == reviewerType && r.ReviewerId == reviewerId).FirstOrDefaultAsync();
            }

            Guard.Against.NoRecordPermission(reviewer != null);

            _cpiDbContext.GetRepository<DMSValuation>().Delete(valuation);
            await _cpiDbContext.SaveChangesAsync();
        }        
    }
}
