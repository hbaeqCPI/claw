using R10.Core.Entities.ReportScheduler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Interfaces
{
    public interface IRSPrintOptionService : IEntityService<RSPrintOption>
    {
        IQueryable<RSPrintOption> RSPrintOptions { get; }
        IQueryable<RSPrintOptionControl> RSPrintOptionControls { get; }
        bool DeleteRSPrintOptionById(int schedParamId);

        bool DeleteRSPrintOptionByTaskId(int taskId);

        bool UpdateRSPrintOption(RSPrintOption rSPrintOption);

        bool AddRSPrintOption(RSPrintOption rSPrintOption);

        IQueryable<RSPrintOption> GetRSPrintOptions(int taksId);

        RSPrintOption GetRSPrintOptionById(int schedParamId);

        Task<bool> Update(object key, string userName,
            IEnumerable<RSPrintOption> updated,
            IEnumerable<RSPrintOption> added,
            IEnumerable<RSPrintOption> deleted);
    }
}
