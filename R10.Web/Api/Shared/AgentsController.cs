using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Validation.AspNetCore;
using R10.Core;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using R10.Core.Entities.Shared;
using R10.Core.Entities.Trademark;
using R10.Core.Exceptions;
using R10.Core.Helpers;
using R10.Core.Interfaces;
using R10.Web.Api.Models;
using R10.Web.Extensions;
using R10.Web.Helpers;
using R10.Web.Interfaces;
using R10.Web.Security;

namespace R10.Web.Api.Shared
{
    //[Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme, Policy = SharedAuthorizationPolicy.CanAccessSystem)]
    [ApiController]
    public class AgentsController : WebApiControllerBase
    {
        private readonly ISystemSettings<PatSetting> _patSettings;
        private readonly ISystemSettings<TmkSetting> _tmkSettings;
        private readonly ILogger _logger;
        private readonly IAgentService _agentService;
        private readonly IAgentViewModelService _agentViewModelService;

        public AgentsController(
            ISystemSettings<PatSetting> patSettings,
            ISystemSettings<TmkSetting> tmkSettings,
            ILogger<AgentsController> logger,
            IAgentService agentService,
            IAgentViewModelService agentViewModelService,
            IEntityService<WebServiceLog> logService) : base(logService)
        {
            _patSettings = patSettings;
            _tmkSettings = tmkSettings;
            _logger = logger;
            _agentService = agentService;
            _agentViewModelService = agentViewModelService;
        }

        /// <summary>
        /// Get agent by agent id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Agent data</returns>
        [Route("api/patent/[controller]/{id}")]
        [HttpGet]
        [Authorize(Policy = PatentAuthorizationPolicy.CanAccessSystem)]
        public async Task<IActionResult> GetPatAgent(int id)
        {
            return await GetAgent(id);
        }

        /// <summary>
        /// Get agent by agent id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Agent data</returns>
        [Route("api/trademark/[controller]/{id}")]
        [HttpGet]
        [Authorize(Policy = TrademarkAuthorizationPolicy.CanAccessSystem)]
        public async Task<IActionResult> GetTmkAgent(int id)
        {
            return await GetAgent(id);
        }

        /// <summary>
        /// Search agents
        /// GET api/patent/agents
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns>Paged list of agent data and the total number of records.</returns>
        [Route("api/patent/[controller]")]
        [HttpGet]
        [Authorize(Policy = PatentAuthorizationPolicy.CanAccessSystem)]
        public async Task<IActionResult> GetPatAgents([FromQuery] AgentCriteria criteria)
        {
            return await GetAgents(criteria);
        }

        /// <summary>
        /// Search agents
        /// GET api/trademark/agents
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns>Paged list of agent data and the total number of records.</returns>
        [Route("api/trademark/[controller]")]
        [HttpGet]
        [Authorize(Policy = TrademarkAuthorizationPolicy.CanAccessSystem)]
        public async Task<IActionResult> GetTmkAgents([FromQuery] AgentCriteria criteria)
        {
            return await GetAgents(criteria);
        }

        private async Task<IActionResult> GetAgents(AgentCriteria criteria)
        {
            if (!criteria.IsValid())
                return BadRequest(criteria.ErrorMessage);

            var logId = await InitWebApiLog();
            var settings = await _patSettings.GetSetting();
            var agents = _agentViewModelService.AddCriteria(_agentService.QueryableList, criteria.ToQueryFilter());
            var total = await agents.CountAsync();
            var data = await agents.ApplyPaging(criteria.Page, criteria.PageSize, settings.MaxApiPageSize).ProjectTo<AgentData>().ToListAsync();

            await UpdateWebApiLog(logId, CPiWebApiStatus.Success, data.Count);
            return Ok(new ApiResult<AgentData>()
            {
                Data = data,
                Total = total
            });
        }

        private async Task<IActionResult> GetAgent(int id)
        {
            var logId = await InitWebApiLog();
            var data = await _agentService.QueryableList.Where(a => a.AgentID == id).ProjectTo<AgentData>().FirstOrDefaultAsync();

            if (data == null)
            {
                await UpdateWebApiLog(logId, CPiWebApiStatus.NotFound);
                return NotFound();
            }

            await UpdateWebApiLog(logId, CPiWebApiStatus.Success, 1);

            return Ok(data);
        }
    }
}
