using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Core.Entities.ReportScheduler
{
    public class tblCTMMain
    {
        public int UniqueID;
        public int SchedID;
        public string TaskCode;
        public string TaskName;
        public int TaskType;
        public string SQLServer;
        public string DBName;
        public string DBConfigName;
        public bool Active;
        public bool NeedsRefresh;
        public string WorkStationID;
        public string Notes;
        public string URL;
        public DateTime? NextProcessDate;
        public DateTime? DateCreated;
        public DateTime? LastUpdated;
        public string? TaskSubType;
    }
}
