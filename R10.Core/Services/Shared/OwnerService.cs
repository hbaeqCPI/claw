using Microsoft.EntityFrameworkCore;
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
    public class OwnerService : ParentEntityService<Owner, OwnerContact>, IOwnerService
    {
        private readonly ISystemSettings<DMSSetting> _settings;
        private readonly IEntitySyncRepository _entitySyncRepository;
        protected readonly IInventionService _inventionService;
        protected readonly ICountryApplicationService _countryApplicationService;
        protected readonly ITmkTrademarkService _trademarkService;
        protected readonly IDisclosureService _disclosureService;

        public OwnerService(
            ICPiDbContext cpiDbContext,
            ClaimsPrincipal user,
            IEntitySyncRepository entitySyncRepository,
            ISystemSettings<DMSSetting> settings,
            IInventionService inventionService,
            ICountryApplicationService countryApplicationService,
            ITmkTrademarkService trademarkService,
            IDisclosureService disclosureService
            ) : base(cpiDbContext, user)
        {
            _settings = settings;
            _entitySyncRepository = entitySyncRepository;
            _inventionService = inventionService;
            _countryApplicationService = countryApplicationService;
            _trademarkService = trademarkService;
            _disclosureService = disclosureService;

            ChildService = new EntityContactService<Owner, OwnerContact>(cpiDbContext, user);
        }

        public override IQueryable<Owner> QueryableList
        {
            get
            {
                var owners = base.QueryableList;

                if (_user.HasEntityFilter())
                    owners = owners.Where(EntityFilter());

                if (_user.IsSharedLimited() && (!_user.HasEntityFilter() || (_user.HasEntityFilter() && _user.GetEntityFilterType() != CPiEntityType.Owner)))
                    owners = owners.Where(o =>
                        (_user.IsInSystem(SystemType.Patent) && (
                            _inventionService.QueryableList.Any(i => i.Owners.Any(io => io.OwnerID == o.OwnerID)) ||
                            _countryApplicationService.CountryApplications.Any(ca => ca.Owners.Any(cao => cao.OwnerID == o.OwnerID)))) ||
                        (_user.IsInSystem(SystemType.Trademark) &&
                            _trademarkService.TmkTrademarks.Any(t => t.Owners.Any(to=> to.OwnerID == o.OwnerID)) ||
                        (_user.IsInSystem(SystemType.DMS) &&
                            _disclosureService.QueryableList.Any(d => d.OwnerID == o.OwnerID))));

                return owners;
            }
        }

        public override IChildEntityService<Owner, OwnerContact> ChildService { get;  }

        protected Expression<Func<Owner, bool>> EntityFilter()
        {
            switch (_user.GetEntityFilterType())
            {
                case CPiEntityType.Owner:
                    return a => UserEntityFilters.Any(f => f.UserId == UserId && f.EntityId == a.OwnerID);               
            }

            return c => true;
        }

        public override async Task<Owner> GetByIdAsync(int entityId)
        {
            return await QueryableList.SingleOrDefaultAsync(a => a.OwnerID == entityId);
        }

        public override async Task Add(Owner entity)
        {
            await ValidateOwner(entity);
            await base.Add(entity);
        }

        public override async Task Update(Owner entity)
        {
            await ValidatePermission(entity.OwnerID);
            await ValidateOwner(entity);
            await base.Update(entity);
        }

        public override async Task UpdateRemarks(Owner entity)
        {
            await ValidatePermission(entity.OwnerID);
            await base.UpdateRemarks(entity);
        }

        public override async Task Delete(Owner entity)
        {
            await ValidatePermission(entity.OwnerID);
            await base.Delete(entity);
        }

        public async Task AddOwnerToClient(int[] ids)
        {
            await _entitySyncRepository.SyncEntities(ids, 2, _user.GetUserName());
        }

        public async Task SyncOwnerWithClient(int[] ids)
        {
            await _entitySyncRepository.SyncEntities(ids, 4, _user.GetUserName());
        }

        protected async Task ValidatePermission(int ownerId)
        {
            if (_user.GetEntityFilterType() == CPiEntityType.Owner)
                Guard.Against.NoRecordPermission(await QueryableList.AnyAsync(a => a.OwnerID == ownerId));
        }

        protected async Task ValidateOwner(Owner owner)
        {
            if (_user.GetEntityFilterType() == CPiEntityType.Owner)
            {
                var ownerLabel = (await _settings.GetSetting()).LabelOwner;
                Guard.Against.ValueNotAllowed(await base.EntityFilterAllowed(owner.OwnerID), ownerLabel);
            }
        }

        public async Task<List<SysCustomFieldSetting>> GetCustomFields()
        {
            var customFieldSettings = _cpiDbContext.GetRepository<SysCustomFieldSetting>().QueryableList;
            return await customFieldSettings.Where(s => s.TableName == "tblOwner" && s.Visible == true).OrderBy(s => s.OrderOfEntry).ToListAsync();
        }
    }
}
