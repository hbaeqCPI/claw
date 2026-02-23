using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Interfaces
{
    public interface IRTSPTOActionMappingService
    {
        Task MappingsUpdate(IList<RTSMapActionDue> updatedRTSMapActionDues, IList<RTSMapActionDue> newRTSMapActionDues);
        Task MappingDelete(RTSMapActionDue deletedRTSMapActionDue);

        Task ActionToCloseUpdate(IList<RTSMapActionClose> updatedRTSMapActionsClose, IList<RTSMapActionClose> newRTSMapActionsClose);
        Task ActionToCloseDelete(RTSMapActionClose deletedRTSMapActionClose);

        IQueryable<RTSMapActionDueSource> RTSMapActionDueSources { get; }
        IQueryable<RTSMapActionDue> RTSMapActionDues { get; }
        IQueryable<RTSMapActionClose> RTSMapActionsClose { get; }
        IQueryable<PatActionType> PatActionTypes { get; }
        IQueryable<PatActionParameter> PatActionParameters { get; }
    }
}
