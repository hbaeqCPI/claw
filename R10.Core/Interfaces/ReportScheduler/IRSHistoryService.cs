using R10.Core.Entities.ReportScheduler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Interfaces
{
    public interface IRSHistoryService
    {
        IQueryable<RSHistory> RSHistorys { get; }
        IQueryable<RSActionHistory> RSActionHistorys { get; }
        IQueryable<RSCriteriaHistory> RSCriteriaHistorys { get; }
        IQueryable<RSPrintOptionHistory> RSPrintOptionHistorys { get; }

        RSHistory GetRSHistory(int logId);
        IQueryable<RSHistory> GetRSHistorys(int taskId);
        IQueryable<RSActionHistory> GetRSActionHistorys(int logId);
        IQueryable<RSCriteriaHistory> GetRSCriteriaHistorys(int logId);
        IQueryable<RSPrintOptionHistory> GetRSPrintOptionHistorys(int logId);
        bool AddRSHistory(RSHistoryView history);
        Task<bool> UpdateRSHistory(RSHistory history);

    }
}
