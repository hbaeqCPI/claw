using Microsoft.Extensions.Configuration;
using R10.Core.Entities.ReportScheduler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Helpers
{
    public class RSCTMScheduleBuilder
    {
        private readonly ReportSchedulerHelper reportSchedulerHelper;

        public RSCTMScheduleBuilder(IConfiguration configuration)
        {
            reportSchedulerHelper = new ReportSchedulerHelper(configuration);
        }

        
        public tblCTMMain CreateSchedule(int taskId, string userID)
        {
            //string taskName = "Seyfarth ";
            tblCTMMain schedule = new tblCTMMain();
            schedule.SchedID = taskId;
            schedule.TaskType = 6;
            schedule.SQLServer = reportSchedulerHelper.GetServerName();
            schedule.DBName = reportSchedulerHelper.GetDatabaseName();
            schedule.WorkStationID = userID;
            schedule.TaskSubType = "B";
            return schedule;
        }

        public tblCTMMain CreateSchedule(int taskId, string scheduleName, bool isEnabled, string trigger, DateTime nextRunDate, string userID)
        {
            string taskName = "Seyfarth ";

            //this will produce a duplicate error since schedule is of 50 chars limit
            if (scheduleName.Length > 29)
                taskName = taskName + scheduleName.Substring(1, 29);
            else
                taskName = taskName + scheduleName;

            tblCTMMain schedule = new tblCTMMain();
            schedule.SchedID = taskId;
            schedule.TaskCode = "Seyfarth-RS"; //<First 17 LETTERS of the client name from tblPubOptions, no spaces, no special characters> + 
            schedule.TaskName = taskName; //<First 17 LETTERS of the client name from tblPubOptions, no spaces, no special characters> + " RS " + <Upto 29 character report description>
            schedule.TaskType = 6;
            schedule.SQLServer = reportSchedulerHelper.GetServerName();
            schedule.DBName = reportSchedulerHelper.GetDatabaseName();
            schedule.DBConfigName = "";
            schedule.Active = isEnabled;
            schedule.NeedsRefresh = true; //<Set to TRUE if record just added or edited>
            schedule.WorkStationID = userID;
            schedule.Notes = trigger;
            schedule.NextProcessDate = nextRunDate;
            schedule.TaskSubType = "B";
            return schedule;
        }

        public tblCTMMain CreateSchedule(tblCTMMain reportSchedule)
        {
            tblCTMMain schedule = new tblCTMMain();
            //schedule.UniqueID = reportSchedule.UniqueID;
            schedule.SchedID = reportSchedule.SchedID;
            schedule.TaskCode = reportSchedule.TaskCode; //<First 17 LETTERS of the client name from tblPubOptions, no spaces, no special characters> + 
            schedule.TaskName = reportSchedule.TaskName; //<First 17 LETTERS of the client name from tblPubOptions, no spaces, no special characters> + " RS " + <Upto 29 character report description>
            schedule.TaskType = 6;
            schedule.SQLServer = reportSchedulerHelper.GetServerName();
            schedule.DBName = reportSchedulerHelper.GetDatabaseName();
            schedule.DBConfigName = reportSchedulerHelper.GetDatabaseName();
            schedule.Active = reportSchedule.Active;
            schedule.NeedsRefresh = true; //<Set to TRUE if record just added or edited>
            schedule.WorkStationID = reportSchedule.WorkStationID;
            schedule.Notes = reportSchedule.Notes;
            schedule.URL = reportSchedule.URL;
            schedule.NextProcessDate = reportSchedule.NextProcessDate;
            schedule.TaskSubType = reportSchedule.TaskSubType;
            return schedule;
        }
    }
}
