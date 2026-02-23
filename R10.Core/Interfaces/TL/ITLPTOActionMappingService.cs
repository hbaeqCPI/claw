using R10.Core.Entities.Patent;
using R10.Core.Entities.Trademark;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Interfaces
{
    public interface ITLPTOActionMappingService
    {
        Task MappingsUpdate(IList<TLMapActionDue> updatedTLMapActionDues, IList<TLMapActionDue> newTLMapActionDues);
        Task MappingDelete(TLMapActionDue deletedTLMapActionDue);

        Task ActionToCloseUpdate(IList<TLMapActionClose> updatedTLMapActionsClose, IList<TLMapActionClose> newTLMapActionsClose);
        Task ActionToCloseDelete(TLMapActionClose deletedTLMapActionClose);

        IQueryable<TLMapActionDueSource> TLMapActionDueSources { get; }
        IQueryable<TLMapActionDue> TLMapActionDues { get; }
        IQueryable<TLMapActionClose> TLMapActionsClose { get; }
        IQueryable<TmkActionType> TmkActionTypes { get; }
        IQueryable<TmkActionParameter> TmkActionParameters { get; }
    }
}
