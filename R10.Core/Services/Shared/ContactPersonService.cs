using Microsoft.EntityFrameworkCore;
using R10.Core.Entities;
// using R10.Core.Entities.DMS; // Removed during deep clean
using R10.Core.Helpers;
using R10.Core.Identity;
using R10.Core.Interfaces;
// using R10.Core.Interfaces.DMS; // Removed during deep clean
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.ConstrainedExecution;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Services.Shared
{
    public class ContactPersonService : EntityService<ContactPerson>, IContactPersonService
    {
        protected readonly IClientService _clientService;
        protected readonly IAgentService _agentService;
        protected readonly IOwnerService _ownerService;
        public ContactPersonService(
            ICPiDbContext cpiDbContext,
            ClaimsPrincipal user,
            IClientService clientService,
            IAgentService agentService,
            IOwnerService ownerService) : base(cpiDbContext, user)
        {
            _clientService = clientService;
            _agentService = agentService;
            _ownerService = ownerService;
        }

        public override IQueryable<ContactPerson> QueryableList
        {
            get
            {
                var contacts = base.QueryableList;

                if (_user.GetEntityFilterType() == CPiEntityType.ContactPerson)
                    contacts = contacts.Where(EntityFilter());

                else if (_user.IsSharedLimited())
                    contacts = contacts.Where(c =>
                        _clientService.QueryableList.Any(cl => cl.ClientContacts.Any(cc => cc.ContactID == c.ContactID)) ||
                        _agentService.QueryableList.Any(a => a.AgentContacts.Any(ac => ac.ContactID == c.ContactID)) ||
                        _ownerService.QueryableList.Any(o => o.OwnerContacts.Any(oc => oc.ContactID == c.ContactID)));

                return contacts;
            }
        }

        private Expression<Func<ContactPerson, bool>> EntityFilter()
        {
            return c => UserEntityFilters.Any(f => f.UserId == UserId && f.EntityId == c.ContactID);
        }

        public async override Task Update(ContactPerson entity)
        {
            //USER ACCOUNT VALIDATION
            //do not allow email change if already linked to a user account
            var email = await QueryableList.Where(e => e.ContactID == entity.ContactID).Select(e => e.EMail).FirstOrDefaultAsync();
            if (!string.IsNullOrEmpty(email) && email != entity.EMail)
            {
                if (await _cpiDbContext.GetRepository<CPiUserEntityFilter>().QueryableList.AnyAsync(e => e.CPiUser.Email == email && e.EntityId == entity.ContactID && e.CPiUser.EntityFilterType == CPiEntityType.ContactPerson))
                    throw new Exception("Unable to update email. Email is linked to a user account.");
            }

            await base.Update(entity);
        }

        public async Task<List<SysCustomFieldSetting>> GetCustomFields()
        {
            var customFieldSettings = _cpiDbContext.GetRepository<SysCustomFieldSetting>().QueryableList;
            return await customFieldSettings.Where(s => s.TableName == "tblContactPerson" && s.Visible == true).OrderBy(s => s.OrderOfEntry).ToListAsync();
        }
    }
}
