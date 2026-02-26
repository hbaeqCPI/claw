using Microsoft.EntityFrameworkCore;
using R10.Core.DTOs;
using R10.Core.Entities;
// using R10.Core.Entities.DMS; // Removed during deep clean
// using R10.Core.Entities.GeneralMatter; // Removed during deep clean
using R10.Core.Entities.Patent;
using R10.Core.Entities.Shared;
using R10.Core.Entities.Trademark;
using R10.Core.Exceptions;
using R10.Core.Helpers;
using R10.Core.Identity;
using R10.Core.Interfaces;
// using R10.Core.Interfaces.AMS; // Removed during deep clean
// using R10.Core.Interfaces.DMS; // Removed during deep clean
using R10.Core.Interfaces.Patent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Services.Shared
{
    public class ClientService : EntityService<Client>, IClientService 
    {
        // Removed during deep clean - DMSSetting no longer exists
        // protected readonly ISystemSettings<DMSSetting> _dmsSettings;
        protected readonly IEntitySyncRepository _entitySyncRepository;
        protected readonly IInventionService _inventionService;
        protected readonly ITmkTrademarkService _trademarkService;

        public ClientService(
            ICPiDbContext cpiDbContext,
            ClaimsPrincipal user,
            // Removed during deep clean - DMSSetting no longer exists
            // ISystemSettings<DMSSetting> dmsSettings,
            IEntitySyncRepository entitySyncRepository,
            IInventionService inventionService,
            ITmkTrademarkService trademarkService) : base(cpiDbContext, user)
        {
            // Removed during deep clean - DMSSetting no longer exists
            // _dmsSettings = dmsSettings;
            _entitySyncRepository = entitySyncRepository;
            _inventionService = inventionService;
            _trademarkService = trademarkService;

            ChildService = new ClientContactService(cpiDbContext, user);
        }

        public override IQueryable<Client> QueryableList
        {
            get
            {
                var clients = base.QueryableList;

                if (_user.HasEntityFilter())
                    clients = clients.Where(EntityFilter());
                
                if (_user.IsSharedLimited() && (!_user.HasEntityFilter() || (_user.HasEntityFilter() && _user.GetEntityFilterType() != CPiEntityType.Client)))
                    clients = clients.Where(c =>
                        (_user.IsInSystem(SystemType.Patent) && _inventionService.QueryableList.Any(i => i.ClientID == c.ClientID)) ||
                        (_user.IsInSystem(SystemType.Trademark) && _trademarkService.TmkTrademarks.Any(t => t.ClientID == c.ClientID)));

                return clients;
            }
        }

        public IClientContactService ChildService { get; }

        protected Expression<Func<Client, bool>> EntityFilter()
        {
            switch (_user.GetEntityFilterType())
            {
                case CPiEntityType.Client:
                    return c => UserEntityFilters.Any(f => f.UserId == UserId && f.EntityId == c.ClientID);

                case CPiEntityType.ContactPerson:
                    // Removed during deep clean - DMSSetting/DMSReviewerType no longer exist
                    // Original code checked _dmsSettings.GetSetting().Result.ReviewerEntityType != DMSReviewerType.Area
                    // Defaulting to AMS DECISION MAKERS path only
                    return c => UserEntityFilters.Any(f => f.UserId == UserId && (
                                    c.ClientContacts.Any(cp => cp.ContactID == f.EntityId)
                                    ));
            }

            return c => true;
        }

        public IQueryable<Client> ClearanceQueryableList
        {
            get
            {
                var clients = base.QueryableList;

                if (_user.HasEntityFilter())
                    clients = clients.Where(EntityFilter());
                
                return clients;
            }
        }

        public override async Task<Client> GetByIdAsync(int entityId)
        {
            return await QueryableList.SingleOrDefaultAsync(a => a.ClientID == entityId);
        }

        public override async Task Add(Client entity)
        {
            await ValidateClient(entity);
            await base.Add(entity);
        }

        public override async Task Update(Client entity)
        {
            await ValidatePermission(entity.ClientID);
            await ValidateClient(entity);
            //EF.core will not update Client field because it is set as principal key in tblAMSMain.CPIClient -> tblClient.Client mapping.
            await _cpiDbContext.GetRepository<Client>().UpdateKeyAsync(entity, "Client", "ClientID", entity.ClientCode, entity.ClientID,_user.GetUserName());
            await base.Update(entity);
        }

        public override async Task Delete(Client entity)
        {
            await ValidatePermission(entity.ClientID);
            await base.Delete(entity);
        }

        public async Task SyncClientWithOwner(int[] ids)
        {
            await _entitySyncRepository.SyncEntities(ids, 3, _user.GetUserName());
        }

        public async Task AddClientToOwner(int[] ids)
        {
            await _entitySyncRepository.SyncEntities(ids, 1, _user.GetUserName());
        }

        protected async Task ValidatePermission(int clientId)
        {
            if (_user.GetEntityFilterType() == CPiEntityType.Client)
                Guard.Against.NoRecordPermission(await QueryableList.AnyAsync(a => a.ClientID == clientId));
        }

        protected async Task ValidateClient(Client client)
        {
            if (_user.GetEntityFilterType() == CPiEntityType.Client)
            {
                // Removed during deep clean - DMSSetting no longer exists, using hardcoded label
                var clientLabel = "Client";
                Guard.Against.ValueNotAllowed(await base.EntityFilterAllowed(client.ClientID), clientLabel);
            }
        }

        public async Task<List<SysCustomFieldSetting>> GetCustomFields()
        {
            var customFieldSettings = _cpiDbContext.GetRepository<SysCustomFieldSetting>().QueryableList;
            return await customFieldSettings.Where(s => s.TableName == "tblClient" && s.Visible == true).OrderBy(s => s.OrderOfEntry).ToListAsync();
        }
    }
}
