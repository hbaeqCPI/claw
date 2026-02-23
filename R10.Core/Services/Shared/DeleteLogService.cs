using R10.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using R10.Core.Queries.Shared;

namespace R10.Core.Services.Shared
{
    public class DeleteLogService: IDeleteLogService
    {
        private readonly IApplicationDbContext _repository;

        public DeleteLogService(IApplicationDbContext repository)
        {
            _repository = repository;
        }

        public IQueryable<QEPatCountryAppDeletedView> PatCountryAppsDeleted => _repository.QEPatCountryAppDeletedView.AsNoTracking();
        public IQueryable<QETmkTrademarkDeletedView> TmkTrademarksDeleted => _repository.QETmkTrademarkDeletedView.AsNoTracking();
        public IQueryable<QEGmMatterDeletedView> GmMattersDeleted => _repository.QEGmMatterDeletedView.AsNoTracking();
        

        public IQueryable<QEPatActionDueDeletedView> PatActionsDueDeleted => _repository.QEPatActionDueDeletedView.AsNoTracking();
        public IQueryable<QEPatActionDueInvDeletedView> PatActionsDueInvDeleted => _repository.QEPatActionDueInvDeletedView.AsNoTracking();
        public IQueryable<QETmkActionDueDeletedView> TmkActionsDueDeleted => _repository.QETmkActionDueDeletedView.AsNoTracking();
        public IQueryable<QEGmActionDueDeletedView> GmActionsDueDeleted => _repository.QEGmActionDueDeletedView.AsNoTracking();
    }
}
