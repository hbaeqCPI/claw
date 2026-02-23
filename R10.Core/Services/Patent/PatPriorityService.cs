using Microsoft.EntityFrameworkCore;
using R10.Core.Entities.Patent;
using R10.Core.Exceptions;
using R10.Core.Helpers;
using R10.Core.Identity;
using R10.Core.Interfaces;
using R10.Core.Interfaces.Patent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace R10.Core.Services
{
    public class PatPriorityService : PatInventionChildService<PatPriority>
    {
        private readonly ICountryApplicationService _countryApplicationService;
        private readonly ISystemSettings<PatSetting> _settings;
        public PatPriorityService(ICPiDbContext cpiDbContext, ClaimsPrincipal user, 
                                  IInventionService inventionService, ICountryApplicationService countryApplicationService,
                                  ISystemSettings<PatSetting> settings) : base(cpiDbContext, user, inventionService)
        {
            _countryApplicationService = countryApplicationService;
            _settings = settings;
        }

        public override async Task<bool> Update(object key, string userName, IEnumerable<PatPriority> updated, IEnumerable<PatPriority> added, IEnumerable<PatPriority> deleted)
        {
            int invId = (int)key;
            var invention = await ValidateInvention(invId);
            invention.UpdatedBy = userName;
            invention.LastUpdate = DateTime.Now;

            if (updated.Any() || added.Any())
                await ValidatePermission(CPiPermissions.FullModify, invention.RespOffice);

            if (deleted.Any())
                await ValidatePermission(CPiPermissions.CanDelete, invention.RespOffice);

            foreach (var item in updated)
            {
                item.UpdatedBy = invention.UpdatedBy;
                item.LastUpdate = invention.LastUpdate;
            }

            foreach (var item in added)
            {
                item.InvId = invId;
                item.UpdatedBy = invention.UpdatedBy;
                item.LastUpdate = invention.LastUpdate;
                item.CreatedBy = invention.UpdatedBy;
                item.DateCreated = invention.LastUpdate;
            }

            var settings = await _settings.GetSetting();
            using (var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }, TransactionScopeAsyncFlowOption.Enabled)) {
                var repository = _cpiDbContext.GetRepository<PatPriority>();
                repository.Delete(deleted);
                repository.Update(updated);
                repository.Add(added);
                _cpiDbContext.GetRepository<Invention>().Update(invention);

                await _cpiDbContext.SaveChangesAsync();

                if (updated.Any() || added.Any())
                    await _countryApplicationService.GenerateCountryLawFromPriority(invId, userName);

                if (settings.IsRelatedCasesMassCopyOn)
                    await _inventionService.RelatedCasesMassCopy(invId, userName);

                    scope.Complete();
            }

            return true;
        }

        

        
    }
}
