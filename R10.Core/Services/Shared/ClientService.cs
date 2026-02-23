using Microsoft.EntityFrameworkCore;
using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Entities.DMS;
using R10.Core.Entities.GeneralMatter;
using R10.Core.Entities.Patent;
using R10.Core.Entities.Shared;
using R10.Core.Entities.Trademark;
using R10.Core.Exceptions;
using R10.Core.Helpers;
using R10.Core.Identity;
using R10.Core.Interfaces;
using R10.Core.Interfaces.AMS;
using R10.Core.Interfaces.DMS;
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
        protected readonly IAMSFeeService _feeService;
        protected readonly IAMSVATRateService _vatService;
        protected readonly ISystemSettings<DMSSetting> _dmsSettings;
        protected readonly IEntitySyncRepository _entitySyncRepository;
        protected readonly IInventionService _inventionService;
        protected readonly ITmkTrademarkService _trademarkService;
        protected readonly IGMMatterService _gMMatterService;
        protected readonly IDisclosureService _disclosureService;
        protected readonly IPacClearanceService _pacClearanceService;
        protected readonly ITmcClearanceService _tmcClearanceService;

        public ClientService(
            ICPiDbContext cpiDbContext,
            ClaimsPrincipal user,
            IAMSFeeService feeService,
            IAMSVATRateService vatService,
            ISystemSettings<DMSSetting> dmsSettings,
            IEntitySyncRepository entitySyncRepository,
            IInventionService inventionService,
            ITmkTrademarkService trademarkService,
            IGMMatterService gMMatterService,
            IDisclosureService disclosureService,
            IPacClearanceService pacClearanceService,
            ITmcClearanceService tmcClearanceService
            ) : base(cpiDbContext, user)
        {
            _feeService = feeService;
            _vatService = vatService;
            _dmsSettings = dmsSettings;
            _entitySyncRepository = entitySyncRepository;
            _inventionService = inventionService;
            _trademarkService = trademarkService;
            _gMMatterService = gMMatterService;
            _disclosureService = disclosureService;
            _pacClearanceService = pacClearanceService;
            _tmcClearanceService = tmcClearanceService;

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
                        (_user.IsInSystem(SystemType.Trademark) && _trademarkService.TmkTrademarks.Any(t => t.ClientID == c.ClientID)) ||
                        (_user.IsInSystem(SystemType.GeneralMatter) && _gMMatterService.QueryableList.Any(g => g.ClientID == c.ClientID)) ||
                        (_user.IsInSystem(SystemType.DMS) && _disclosureService.QueryableList.Any(d => d.ClientID == c.ClientID)) ||
                        (_user.IsInSystem(SystemType.PatClearance) && _pacClearanceService.QueryableList.Any(p => p.ClientID == c.ClientID)) ||
                        (_user.IsInSystem(SystemType.SearchRequest) && _tmcClearanceService.QueryableList.Any(t => t.ClientID == c.ClientID)));

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
                    //REVIEWER ENTITY TYPE DEFAULTS TO CLIENT
                    if (_dmsSettings.GetSetting().Result.ReviewerEntityType != DMSReviewerType.Area)
                        //AMS DECISION MAKERS
                        //DMS REVIEWERS
                        return c => UserEntityFilters.Any(f => f.UserId == UserId && (
                                        c.ClientContacts.Any(cp => cp.ContactID == f.EntityId) ||
                                        c.Reviewers.Any(cr => cr.ReviewerId == f.EntityId && cr.ReviewerType == CPiEntityType.ContactPerson)
                                        ));
                    else
                        //AMS DECISION MAKERS
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

            await _feeService.RecalculateServiceFee(entity.FeeSetupName, entity.ClientCode);
            await _vatService.RecalculateVATRate(entity.ClientCode);
        }

        public override async Task Update(Client entity)
        {
            await ValidatePermission(entity.ClientID);
            await ValidateClient(entity);
            //EF.core will not update Client field because it is set as principal key in tblAMSMain.CPIClient -> tblClient.Client mapping.
            await _cpiDbContext.GetRepository<Client>().UpdateKeyAsync(entity, "Client", "ClientID", entity.ClientCode, entity.ClientID,_user.GetUserName());
            await base.Update(entity);

            await _feeService.RecalculateServiceFee(entity.FeeSetupName, entity.ClientCode);
            await _vatService.RecalculateVATRate(entity.ClientCode);
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
                var clientLabel = (await _dmsSettings.GetSetting()).LabelClient;
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
