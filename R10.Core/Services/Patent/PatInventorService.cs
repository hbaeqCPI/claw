using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using R10.Core.Entities;
// using R10.Core.Entities.DMS; // Removed during deep clean
using R10.Core.Entities.Patent;
using R10.Core.Exceptions;
using R10.Core.Helpers;
using R10.Core.Identity;
using R10.Core.Interfaces;
// using R10.Core.Interfaces.DMS; // Removed during deep clean
using R10.Core.Interfaces.Patent;

namespace R10.Core.Services
{
    public class PatInventorService : EntityService<PatInventor>, IPatInventorService//IEntityService<PatInventor>
    {
        protected readonly IInventionService _inventionService;
        protected readonly ICountryApplicationService _countryApplicationService;
        public PatInventorService(ICPiDbContext cpiDbContext, ClaimsPrincipal user,
            IInventionService inventionService,
            ICountryApplicationService countryApplicationService) : base(cpiDbContext, user)
        {
            _inventionService = inventionService;
            _countryApplicationService = countryApplicationService;
        }

        public override IQueryable<PatInventor> QueryableList
        {
            get
            {
                var inventors = base.QueryableList;

                if (_user.GetEntityFilterType() == CPiEntityType.Inventor)
                    inventors = inventors.Where(EntityFilter());

                else if (_user.IsAuxiliaryLimited(SystemType.Patent))
                    inventors = inventors.Where(i =>
                        _inventionService.QueryableList.Any(inv => inv.Inventors.Any(ii => ii.InventorID == i.InventorID)) ||
                        _countryApplicationService.CountryApplications.Any(app => app.Inventors.Any(cai => cai.InventorID == i.InventorID)));

                return inventors;
            }
        }

        private Expression<Func<PatInventor, bool>> EntityFilter()
        {
            return a => UserEntityFilters.Any(f => f.UserId == _user.GetUserIdentifier() && f.EntityId == a.InventorID);
        }

        public override async Task<PatInventor> GetByIdAsync(int inventorID)
        {
            return await QueryableList.SingleOrDefaultAsync(i => i.InventorID == inventorID);
        }

        public override async Task Add(PatInventor inventor)
        {
            await ValidateInventor(inventor);
            await base.Add(inventor);
        }

        public override async Task Delete(PatInventor inventor)
        {
            await ValidatePermission(inventor.InventorID);

            //Clear Orphan Letters
            await ClearOrphanLettersSettings(inventor.InventorID);          

            await base.Delete(inventor);
        }

        public override async Task Update(PatInventor inventor)
        {
            await ValidatePermission(inventor.InventorID);
            await ValidateInventor(inventor);

            //Clear Orphan Letters if GenAllLetters <> Specific (2)
            if (inventor.GenAllLetters != (int)LetterOption.Specific) {
                await ClearOrphanLettersSettings(inventor.InventorID);               
            }           

            await base.Update(inventor);
        }

        public async Task ValidatePermission(int inventorID)
        {
            if (_user.GetEntityFilterType() == CPiEntityType.Inventor)
                Guard.Against.NoRecordPermission(await QueryableList.AnyAsync(i => i.InventorID == inventorID));
        }

        private async Task ValidateInventor(PatInventor inventor)
        {
            if (_user.GetEntityFilterType() == CPiEntityType.Inventor)
                Guard.Against.ValueNotAllowed(await EntityFilterAllowed(inventor.InventorID), "Inventor");
        }

        private async Task ClearOrphanLettersSettings(int entityId)
        {
            List<LetterEntitySetting> orphanLetters = new List<LetterEntitySetting>();
            orphanLetters.AddRange(await _cpiDbContext.GetRepository<LetterEntitySetting>().QueryableList.Where(l => l.EntityType == "I" && l.EntityId == entityId && l.ContactId == 0).ToListAsync());

            if (orphanLetters.Any())
                _cpiDbContext.GetRepository<LetterEntitySetting>().Delete(orphanLetters);
        }

        public async Task<List<SysCustomFieldSetting>> GetCustomFields()
        {
            var customFieldSettings = _cpiDbContext.GetRepository<SysCustomFieldSetting>().QueryableList;
            return await customFieldSettings.Where(s => s.TableName == "tblPatInventor" && s.Visible == true).OrderBy(s => s.OrderOfEntry).ToListAsync();
        }
    }
}
