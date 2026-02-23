using R10.Core.DTOs;
using R10.Core.Entities.ReportScheduler;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Interfaces
{
    public interface IRSMainService : IEntityService<RSMain>
    {
        IQueryable<RSMain> RSMains { get; }
        IQueryable<RSReportType> RSReportTypes { get; }
        IQueryable<RSFrequencyType> RSFrequencyTypes { get; }
        IQueryable<RSDateTypeControl> RSDateTypeControls { get; }
        IQueryable<RSPrintOptionControl> RSPrintOptionControls { get; }
        IQueryable<RSCriteriaControl> RSCriteriaControls { get; }
        IQueryable<RSAction> RSActions { get; }

        IQueryable<RSCriteria> RSCriterias { get; }

        IQueryable<RSPrintOption> RSPrintOptions { get; }

        //bool DeleteRSMainById(int taskId);

        //bool UpdateRSMain(RSMain rSMain);
        //bool AddRSMain(RSMain rSMain);

        RSMain GetRSMainById(int taskId);

        RSHistory CreateRSHistory(int taskId, int actionId);

        IQueryable<RSFrequencyType> GetRSFrequencyTypes();

        Task<Tuple<string, string>> CopySchedule(int CopyTaskId, string newScheduleName,
                           bool CopySettings, bool CopyActions, bool CopyCriteria, bool CopyPrintOptions, string createdBy);

        int GetReportId(int TaskId);
        List<RSDueDate> GetDueDates(int TaskId);

        List<RSPatentListPreview> GetPatentListPreviewList(int TaskId);
        List<RSTrademarkListPreview> GetTrademarkListPreviewList(int TaskId);
        List<RSMatterListPreview> GetMatterListPreviewList(int TaskId);
        DataTable GetReportParameters(int taskId, int actionId, int reportId);
        List<RSReportAttorney> GetReportAttorneys(int taskId, int actionId, int reportId);
    }
}
