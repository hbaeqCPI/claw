using R10.Core.Entities.ReportScheduler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Interfaces
{
    public interface IRSLogService
    {
        bool AddReportSchedulerLog(RSLog rsLog);
    }
}
