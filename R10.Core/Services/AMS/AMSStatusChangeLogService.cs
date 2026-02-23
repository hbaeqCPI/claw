using Microsoft.EntityFrameworkCore;
using R10.Core.Entities;
using R10.Core.Entities.AMS;
using R10.Core.Entities.Patent;
using R10.Core.Exceptions;
using R10.Core.Helpers;
using R10.Core.Interfaces;
using R10.Core.Interfaces.AMS;
using R10.Core.Interfaces.Patent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Services.AMS
{
    public class AMSStatusChangeLogService : EntityService<AMSStatusChangeLog>, IAMSStatusChangeLogService
    {
        private readonly ICountryApplicationService _countryApplicationService;
        private readonly ISystemSettings<AMSSetting> _amsSettings;

        public AMSStatusChangeLogService(
            ICPiDbContext cpiDbContext, 
            ClaimsPrincipal user, 
            ICountryApplicationService countryApplicationService,
            ISystemSettings<AMSSetting> amsSettings) : base(cpiDbContext, user)
        {
            _countryApplicationService = countryApplicationService;
            _amsSettings = amsSettings;
        }

        public IQueryable<AMSStatusChangeLog> AMSStatusChangeLogs => _cpiDbContext.GetRepository<AMSStatusChangeLog>().QueryableList;

        public IQueryable<AMSStatusChangeLog> AMSStatusUpdateList => AMSStatusChangeLogs
            .Where(s => s.UpdateDate == null && s.AMSDue.AMSMain.CountryApplication != null);

        public string GetNewStatus(string instructionType, string instructionStatus, DateTime? filDate, DateTime? pubDate, DateTime? issDate)
        {
            if (instructionType == "Y")
            {
                if (issDate != null)
                    return "Granted";
                if (pubDate != null)
                    return "Published";
                if (filDate != null)
                    return "Pending";

                return "Unfiled";
            }
            else
                return instructionStatus;
        }
        private DateTime GetNewStatusDate(CountryApplication application)
        {
            var statusDate = DateTime.Now.Date;

            switch (application.ApplicationStatus)
            {
                case "Granted":
                    return application.IssDate ?? statusDate;
                case "Published":
                    return application.PubDate ?? statusDate;
                case "Pending":
                    return application.FilDate ?? statusDate;
            }

            return statusDate;
        }

        public async Task<byte[]> SaveProcessFlag(int id, bool processFlag, byte[] tStamp, string userName)
        {
            var updated = new AMSStatusChangeLog() 
            { 
                LogID = id,
                ProcessFlag = !processFlag,
                tStamp = tStamp
            };

            _cpiDbContext.GetRepository<AMSStatusChangeLog>().Attach(updated);

            updated.ProcessFlag = processFlag;
            updated.UpdatedBy = userName;
            updated.LastUpdate = DateTime.Now;

            await _cpiDbContext.SaveChangesAsync();

            return updated.tStamp;
        }

        public async Task<byte[]> SaveRemarks(int id, string remarks, byte[] tStamp, string userName)
        {
            var updated = new AMSStatusChangeLog()
            {
                LogID = id,
                Remarks = string.IsNullOrEmpty(remarks) ? "-" : "",
                tStamp = tStamp
            };

            _cpiDbContext.GetRepository<AMSStatusChangeLog>().Attach(updated);

            updated.Remarks = remarks;
            updated.UpdatedBy = userName;
            updated.LastUpdate = DateTime.Now;

            await _cpiDbContext.SaveChangesAsync();

            return updated.tStamp;
        }

        public async Task UpdateStatus(AMSStatusChangeLog updated)
        {
            var application = new CountryApplication();
            if (updated.ProcessFlag)
            {
                var amsSettings = await _amsSettings.GetSetting();
                var amsMain = _cpiDbContext.GetRepository<AMSMain>().QueryableList
                    .Where(m => m.AMSDue.Any(d => d.DueID == updated.DueID));

                application = await _countryApplicationService.CountryApplications
                    .Where(c => amsMain.Any(m => m.CountryApplication.AppId == c.AppId))
                    .FirstOrDefaultAsync();

                if (application == null)
                    throw new NoRecordPermissionException();

                Guard.Against.NoRecordPermission(await ValidatePermission(SystemType.Patent, CPiPermissions.FullModify, application.RespOffice));
                
                _cpiDbContext.GetRepository<CountryApplication>().Attach(application);

                application.Remarks = $"Status change " +
                    $"from {application.ApplicationStatus} " +
                    $"to {updated.NewStatus} " +
                    $"by {updated.UpdatedBy} " +
                    $"on {updated.LastUpdate?.ToString("dd-MMM-yyyy hh:mm tt")} through AMS." +
                    $"\r\n{updated.Remarks}" +
                    $"\r\n\r\n{application.Remarks}";
                application.ApplicationStatus = updated.NewStatus;

                if (amsSettings.HasStatusDateUpdate)
                    application.ApplicationStatusDate = GetNewStatusDate(application);

                application.UpdatedBy = updated.UpdatedBy;
                application.LastUpdate = updated.LastUpdate;
            }
            
            var statusChangeLog = new AMSStatusChangeLog()
            {
                LogID = updated.LogID,
                tStamp = updated.tStamp
            };
            _cpiDbContext.GetRepository<AMSStatusChangeLog>().Attach(statusChangeLog);

            if (updated.ProcessFlag)
                statusChangeLog.NewStatus = updated.NewStatus;

            statusChangeLog.UpdateDate = updated.LastUpdate;
            statusChangeLog.UpdatedBy = updated.UpdatedBy;
            statusChangeLog.LastUpdate = updated.LastUpdate;

            await _cpiDbContext.SaveChangesAsync();

            _cpiDbContext.Detach(application);
            _cpiDbContext.Detach(statusChangeLog);
        }

        public async Task<int> SaveStatusChangeLog(DateTime sendDate)
        {
            //validate if logs already exist
            var existingStatusUpdateList = await AMSStatusChangeLogs.Where(s => s.ClientInstructionSentToCPI == sendDate).ToListAsync();
            if (existingStatusUpdateList.Count > 0)
                return existingStatusUpdateList.Where(s => s.UpdateDate == null).Count();

            //create logs
            var lastUpdate = DateTime.Now;
            var updatedBy = _user.GetUserName();
            var newStatusUpdateList = await _cpiDbContext.GetRepository<AMSInstrxCPiLogDetail>().QueryableList
               .Where(l => l.AMSInstrxCPiLog.SendDate == sendDate && l.SentInstructionType == l.AMSDue.ClientInstructionType &&
                           ((l.SentInstructionType == "A" && l.AMSDue.AMSMain.CountryApplication.PatApplicationStatus.ActiveSwitch) ||
                           (l.SentInstructionType == "Y" && !l.AMSDue.AMSMain.CountryApplication.PatApplicationStatus.ActiveSwitch)))
               .Select(l => new AMSStatusChangeLog()
               {
                   DueID = l.DueId,
                   AnnID = l.AMSDue.AnnID,
                   ClientInstructionSentToCPI = l.AMSInstrxCPiLog.SendDate,
                   TriggerInstructionType = l.SentInstructionType,
                   OldStatus = l.AMSDue.AMSMain.CountryApplication.ApplicationStatus,
                   CreatedBy = updatedBy,
                   DateCreated = lastUpdate,
                   UpdatedBy = updatedBy,
                   LastUpdate = lastUpdate                    
               })
               .ToListAsync();

            if (newStatusUpdateList.Count == 0)
                return 0;

            //update unprocessed old dueIds
            var newDueIds = newStatusUpdateList.Select(s => s.DueID).ToList();
            var oldStatusUpdateList = await AMSStatusChangeLogs.Where(s => s.UpdateDate == null && newDueIds.Any(dueId => dueId == s.DueID)).ToListAsync();
            if (oldStatusUpdateList.Any())
            {
                _cpiDbContext.GetRepository<AMSStatusChangeLog>().Attach(oldStatusUpdateList);
                oldStatusUpdateList.ForEach(s =>
                {
                    s.UpdateDate = lastUpdate;
                    s.UpdatedBy = updatedBy;
                    s.LastUpdate = lastUpdate;
                });
            }

            _cpiDbContext.GetRepository<AMSStatusChangeLog>().Add(newStatusUpdateList);
            await _cpiDbContext.SaveChangesAsync();

            return newStatusUpdateList.Count;
        }
    }
}
