using Microsoft.Extensions.Configuration;
using R10.Core.Entities.ReportScheduler;
using R10.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace R10.Core.Services
{
    public class RSCTMService : IRSCTMService
    {
        private readonly RSCTMRepository repository;

        public RSCTMService(IConfiguration configuration)
        {
            repository = new RSCTMRepository(configuration);
        }

        public int GetCTMUniqueId(string taskName)
        {
            DataTable dt = repository.GetScheduleById(taskName);
            int uniqueId = 0;

            if (dt.Rows[0]["UniqueID"] != DBNull.Value)
                uniqueId = Convert.ToInt32(dt.Rows[0]["UniqueID"]);

            return uniqueId;
        }

        public bool InsertCTMSchedule(tblCTMMain entity)
        {
            //IServiceCallStatus callStatus = Validate(scheduleRow);
            //if (!callStatus.Success) return callStatus;

            return repository.Save(entity, 1);
        }

        public bool UpdateCTMSchedule(tblCTMMain entity)
        {
            return repository.Save(entity, 2);
        }

        public bool DeleteCTMSchedule(tblCTMMain entity)
        {
            //TODO: Add Validation
            return repository.Save(entity, 3);
        }

        public DateTime GetCTMDateTime()
        {
            return repository.GetCTMDateTime();
        }

        public bool SyncWithCTM(int CTMId, int ActionId, DateTime? NextRunTime, string? ErrorMessage)
        {
            return repository.SyncWithCTM(CTMId, ActionId, NextRunTime, ErrorMessage);
        }
    }

}
