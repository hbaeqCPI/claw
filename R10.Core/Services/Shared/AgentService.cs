using Microsoft.EntityFrameworkCore;
using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Entities.GeneralMatter;
using R10.Core.Entities.Patent;
using R10.Core.Entities.Shared;
using R10.Core.Entities.Trademark;
using R10.Core.Exceptions;
using R10.Core.Helpers;
using R10.Core.Identity;
using R10.Core.Interfaces;
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
    public class AgentService :  EntityService<Agent>, IAgentService
    {
        private readonly ISystemSettings<DefaultSetting> _settings;
        protected readonly ICountryApplicationService _countryApplicationService;
        protected readonly ITmkTrademarkService _trademarkService;
        protected readonly IGMMatterService _gMMatterService;

        public AgentService(
            ICPiDbContext cpiDbContext,
            ClaimsPrincipal user,
            ISystemSettings<DefaultSetting> settings,
            ICountryApplicationService countryApplicationService,
            ITmkTrademarkService trademarkService,
            IGMMatterService gMMatterService
            ) : base(cpiDbContext, user)
        {
            _settings = settings;
            _countryApplicationService = countryApplicationService;
            _trademarkService = trademarkService;
            _gMMatterService = gMMatterService;

            //ChildService = new EntityContactService<Agent, AgentContact>(cpiDbContext, user);
            ChildService = new AgentContactService(cpiDbContext, user);
        }

        public override IQueryable<Agent> QueryableList
        {
            get
            {
                var agents = base.QueryableList;

                if (_user.GetEntityFilterType() == CPiEntityType.Agent)
                    agents = agents.Where(EntityFilter());

                else if (_user.IsSharedLimited())
                    agents = agents.Where(a =>
                        (_user.IsInSystem(SystemType.Patent) && _countryApplicationService.CountryApplications.Any(i => i.AgentID == a.AgentID)) ||
                        (_user.IsInSystem(SystemType.Trademark) && _trademarkService.TmkTrademarks.Any(t => t.AgentID == a.AgentID)) ||
                        (_user.IsInSystem(SystemType.GeneralMatter) && _gMMatterService.QueryableList.Any(g => g.AgentID == a.AgentID)));

                return agents;
            }
        }

        protected Expression<Func<Agent, bool>> EntityFilter()
        {
            return a => UserEntityFilters.Any(f => f.UserId == UserId && f.EntityId == a.AgentID);
        }

        public override async Task<Agent> GetByIdAsync(int entityId)
        {
            return await QueryableList.SingleOrDefaultAsync(a => a.AgentID == entityId);
        }

        public override async Task Add(Agent entity)
        {
            await ValidateAgent(entity);
            await base.Add(entity);
        }

        public override async Task Update(Agent entity)
        {
            await ValidatePermission(entity.AgentID);
            await ValidateAgent(entity);
            //EF.core will not update Agent field because it is set as principal key in tblAMSMain.CPIAgent -> tblAgent.Agent mapping.
            await _cpiDbContext.GetRepository<Agent>().UpdateKeyAsync(entity, "Agent", "AgentID", entity.AgentCode, entity.AgentID, _user.GetUserName());
            await base.Update(entity);
        }

        public override async Task UpdateRemarks(Agent entity)
        {
            await ValidatePermission(entity.AgentID);
            await base.UpdateRemarks(entity);
        }

        public override async Task Delete(Agent entity)
        {
            await ValidatePermission(entity.AgentID);
            await base.Delete(entity);
        }

        private async Task ValidatePermission(int entityId)
        {
            if (_user.GetEntityFilterType() == CPiEntityType.Agent)
                Guard.Against.NoRecordPermission(await QueryableList.AnyAsync(a => a.AgentID == entityId));
        }

        private async Task ValidateAgent(Agent entity)
        {
            if (_user.GetEntityFilterType() == CPiEntityType.Agent)
            {
                var agentLabel = (await _settings.GetSetting()).LabelAgent;
                Guard.Against.ValueNotAllowed(await base.EntityFilterAllowed(entity.AgentID), agentLabel);
            }
        }

        //public override IChildEntityService<Agent, AgentContact> ChildService { get; }
        public IAgentContactService ChildService { get; }

        public async Task<List<SysCustomFieldSetting>> GetCustomFields()
        {
            var customFieldSettings = _cpiDbContext.GetRepository<SysCustomFieldSetting>().QueryableList;
            return await customFieldSettings.Where(s => s.TableName == "tblAgent" && s.Visible == true).OrderBy(s => s.OrderOfEntry).ToListAsync();
        }
    }
}
