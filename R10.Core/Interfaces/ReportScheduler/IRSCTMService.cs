using R10.Core.Entities.ReportScheduler;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Core.Interfaces
{
    public interface IRSCTMService
    {
        int GetCTMUniqueId(string taskName);
        bool InsertCTMSchedule(tblCTMMain entity);
        bool UpdateCTMSchedule(tblCTMMain entity);
        bool DeleteCTMSchedule(tblCTMMain entity);
        DateTime GetCTMDateTime();
        bool SyncWithCTM(int CTMId, int ActionId, DateTime? NextRunTime, string? ErrorMessage);
    }
}
