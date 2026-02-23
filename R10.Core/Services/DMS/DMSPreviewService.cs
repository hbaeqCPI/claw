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
    public class DMSPreviewService : DMSDisclosureChildService<DMSPreview>, IDMSPreviewService
    {
        private readonly ISystemSettings<DMSSetting> _settings;
        public DMSPreviewService(
                    ICPiDbContext cpiDbContext,
                    ClaimsPrincipal user,
                    IDisclosureService  disclosureService,
                    ISystemSettings<DMSSetting> settings
                    ) : base(cpiDbContext, user, disclosureService)
        {
            _settings = settings;
        }

        public new async Task<(int DMSPreviewId, CPiEntityType PreviewerType, int PreviewerId, byte[] tStamp)> Update(DMSPreview preview)
        {
            //Only admin/modify users or users with previewer role can save
            //Previewer can be inventor/reviewer/user
            var cpiPermissions = CPiPermissions.Previewer;
            cpiPermissions.Add("modify");            
            await ValidatePermission(cpiPermissions);

            //Validate disclosure record
            var disclosure = await _disclosureService.ForPreviewList.Where(r => r.DMSId == preview.DMSId).FirstOrDefaultAsync();
            Guard.Against.NoRecordPermission(disclosure != null);            

            //Check if user is valid previewer
            var settings = await _settings.GetSetting();
            var previewerType = _user.GetEntityFilterType();
            var previewerId = await _disclosureService.GetUserPreviewerId(previewerType);
            
            Guard.Against.NoRecordPermission(previewerId > 0 || await ValidatePermission(SystemType.DMS, CPiPermissions.FullModify, null));

            var saveUpdate = false;
            var updated = await _cpiDbContext.GetRepository<DMSPreview>().QueryableList.SingleOrDefaultAsync(d => d.DMSPreviewId == preview.DMSPreviewId);
            if (updated == null)
            {
                updated = preview;
                
                if (previewerId == 0)
                {
                    updated.PreviewerType = CPiEntityType.None;
                    updated.PreviewerId = 0;
                    updated.UserId = _user.GetUserIdentifier();
                }
                else
                {
                    updated.PreviewerType = previewerType;
                    updated.PreviewerId = previewerId;
                    updated.UserId = _user.GetUserIdentifier();
                }                

                _cpiDbContext.GetRepository<DMSPreview>().Add(updated);                               
                saveUpdate = true;
            }
            else
            {
                if (updated.Remarks != preview.Remarks)
                {
                    saveUpdate = true;
                    _cpiDbContext.GetRepository<DMSPreview>().Attach(updated);                
                    updated.Remarks = preview.Remarks;
                    updated.UpdatedBy = preview.UpdatedBy;
                    updated.LastUpdate = preview.LastUpdate;
                    updated.tStamp = preview.tStamp;
                }                
            }

            if (saveUpdate)
            {
                _cpiDbContext.GetRepository<Disclosure>().Attach(disclosure);
                disclosure.UpdatedBy = preview.UpdatedBy;
                disclosure.LastUpdate = preview.LastUpdate;

                //Update status to Final Review and log
                var defaultFinalReviewStatus = settings.FinalReviewStatus;
                if (string.IsNullOrEmpty(defaultFinalReviewStatus))
                    defaultFinalReviewStatus = "Final Review";

                var reviewStatus = await _disclosureService.GetOrCreateCPIDisclosureStatus(defaultFinalReviewStatus, canReview: true);

                if (!string.IsNullOrEmpty(reviewStatus) && !string.IsNullOrEmpty(disclosure.DisclosureStatus) && reviewStatus != disclosure.DisclosureStatus)
                {
                    disclosure.DisclosureStatus = reviewStatus;
                    disclosure.DisclosureStatusDate = DateTime.Now.Date;

                    var statusHistory = new DMSDisclosureStatusHistory()
                    {
                        DMSId = disclosure.DMSId,
                        DisclosureStatus = disclosure.DisclosureStatus,
                        DisclosureStatusDate = disclosure.DisclosureStatusDate,
                        DisclosureDate = disclosure.DisclosureDate,
                        Recommendation = disclosure.Recommendation,
                        CreatedBy = disclosure.UpdatedBy,
                        DateChanged = disclosure.LastUpdate,
                        ChangeType = DMSStatusHistoryChangeType.Status
                    };
                    _cpiDbContext.GetRepository<DMSDisclosureStatusHistory>().Add(statusHistory);
                } 

                await _cpiDbContext.SaveChangesAsync();
            }

            return (updated.DMSPreviewId, updated.PreviewerType, (int)updated.PreviewerId, updated.tStamp);
        }
    }
}
