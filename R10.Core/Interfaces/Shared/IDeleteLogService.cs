using R10.Core.Queries.Shared;

namespace R10.Core.Interfaces
{
    public interface IDeleteLogService
    {

        IQueryable<QEPatCountryAppDeletedView> PatCountryAppsDeleted { get; }
        IQueryable<QETmkTrademarkDeletedView> TmkTrademarksDeleted { get; }
        IQueryable<QEGmMatterDeletedView> GmMattersDeleted { get; }

        IQueryable<QEPatActionDueDeletedView> PatActionsDueDeleted { get; }
        IQueryable<QEPatActionDueInvDeletedView> PatActionsDueInvDeleted { get; }
        IQueryable<QETmkActionDueDeletedView> TmkActionsDueDeleted { get; }
        IQueryable<QEGmActionDueDeletedView> GmActionsDueDeleted { get; }
        
    }
}
