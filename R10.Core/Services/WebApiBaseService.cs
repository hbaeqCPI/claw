using Microsoft.EntityFrameworkCore;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using R10.Core.Entities.Trademark;
using R10.Core.Helpers;
using R10.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Services
{
    public class WebApiBaseService<T> : EntityService<T> where T : class
    {
        private bool? _hasSharedAuxModify;

        public WebApiBaseService(ICPiDbContext cpiDbContext, ClaimsPrincipal user) : base(cpiDbContext, user)
        {
        }

        protected bool HasSharedAuxModify {
            get
            {
                if (_hasSharedAuxModify == null)
                    _hasSharedAuxModify = _user.IsInRoles(SystemType.Shared, CPiPermissions.FullModify);

                return (bool)_hasSharedAuxModify;
            }
        }

        protected DateTime EmptyDate => new DateTime(0001, 1, 1);

        protected async Task<bool> HasAgent(string agentCode)
        {
            return await _cpiDbContext.GetReadOnlyRepositoryAsync<Agent>().QueryableList.AnyAsync(a => a.AgentCode == agentCode);
        }

        protected async Task<bool> HasAttorney(string attorneyCode)
        {
            return await _cpiDbContext.GetReadOnlyRepositoryAsync<Attorney>().QueryableList.AnyAsync(a => a.AttorneyCode == attorneyCode);
        }

        protected async Task<bool> HasAttorneys(List<string> attorneyCodes)
        {
            return (await _cpiDbContext.GetReadOnlyRepositoryAsync<Attorney>().QueryableList.CountAsync(a => attorneyCodes.Contains(a.AttorneyCode))) == attorneyCodes.Count;
        }

        protected async Task<int> GetAgentID(string agentCode, DateTime runDate)
        {
            return await GetAgentID(agentCode, agentCode, runDate);
        }

        protected async Task<int> GetAgentID(string agentCode, string? agentName, DateTime runDate)
        {
            var agentId = await _cpiDbContext.GetReadOnlyRepositoryAsync<Agent>().QueryableList
                            .Where(a => a.AgentCode == agentCode).Select(a => a.AgentID).FirstOrDefaultAsync();

            if (agentId == 0)
            {
                var agent = new Agent()
                {
                    AgentCode = agentCode,
                    AgentName = agentName ?? agentCode,
                    CreatedBy = _user.GetUserName(),
                    UpdatedBy = _user.GetUserName(),
                    DateCreated = runDate,
                    LastUpdate = runDate
                };
                _cpiDbContext.GetRepository<Agent>().Add(agent);
                await _cpiDbContext.SaveChangesAsync();
                agentId = agent.AgentID;
            }

            return agentId;
        }

        protected async Task<int> GetAttorneyID(string attorneyCode, DateTime runDate)
        {
            var attorneyId = await _cpiDbContext.GetReadOnlyRepositoryAsync<Attorney>().QueryableList
                                .Where(a => a.AttorneyCode == attorneyCode).Select(a => a.AttorneyID).FirstOrDefaultAsync();

            if (attorneyId == 0)
            {
                var attorney = new Attorney()
                {
                    AttorneyCode = attorneyCode,
                    AttorneyName = attorneyCode,
                    CreatedBy = _user.GetUserName(),
                    UpdatedBy = _user.GetUserName(),
                    DateCreated = runDate,
                    LastUpdate = runDate
                };
                _cpiDbContext.GetRepository<Attorney>().Add(attorney);
                await _cpiDbContext.SaveChangesAsync();
                attorneyId = attorney.AttorneyID;
            }

            return attorneyId;
        }

        public async Task<int> GetInventorID(PatInventorWebSvc webApiInventor, DateTime runDate)
        {
            //find inventor using email
            var inventorId = await _cpiDbContext.GetReadOnlyRepositoryAsync<PatInventor>().QueryableList
                                .Where(i => i.EMail == webApiInventor.EMail).Select(i => i.InventorID).FirstOrDefaultAsync();

            //create inventor if email not found
            if (inventorId == 0)
            {
                var inventor = new PatInventor()
                {
                    EMail = webApiInventor.EMail,
                    LastName = webApiInventor.LastName,
                    FirstName = webApiInventor.FirstName,
                    MiddleInitial = webApiInventor.MiddleInitial,
                    CreatedBy = _user.GetUserName(),
                    DateCreated = DateTime.Now,
                    UpdatedBy = _user.GetUserName(),
                    LastUpdate = DateTime.Now
                };
                _cpiDbContext.GetRepository<PatInventor>().Add(inventor);
                await _cpiDbContext.SaveChangesAsync();
                inventorId = inventor.InventorID;
            }

            return inventorId;
        }

        protected string FormatErrorMessage(int index, string message, params string?[] keys)
        {
            return $"[{index}] {String.Join("|", keys.Where(k => !string.IsNullOrEmpty(k)))} : {message}";
        }

        public async Task LogApiData(List<T> entities)
        {
            _cpiDbContext.GetRepository<T>().Add(entities);
            await _cpiDbContext.SaveChangesAsync();
        }

        public async Task LogApiData(T entity)
        {
            _cpiDbContext.GetRepository<T>().Add(entity);
            await _cpiDbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Reset db entity states to clear db exceptions
        /// </summary>
        public void ClearDbException()
        {
            _cpiDbContext.DetachAll();
        }
    }
}
