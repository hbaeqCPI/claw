using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Validation.AspNetCore;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using R10.Core.Entities.Trademark;
using R10.Core.Interfaces;
using R10.Web.Api.Models;
using R10.Web.Extensions;
using R10.Web.Helpers;
using R10.Web.Interfaces;
using R10.Web.Security;

namespace R10.Web.Api.Shared
{
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme, Policy = SharedAuthorizationPolicy.CanAccessSystem)]
    [ApiController]
    public class ClientsController : WebApiControllerBase
    {
        private readonly ISystemSettings<PatSetting> _patSettings;
        private readonly ISystemSettings<TmkSetting> _tmkSettings;
        private readonly ILogger _logger;
        private readonly IClientService _clientService;
        private readonly IClientViewModelService _clientViewModelService;

        public ClientsController(
            ISystemSettings<PatSetting> patSettings, 
            ISystemSettings<TmkSetting> tmkSettings, 
            ILogger<ClientsController> logger, 
            IClientService clientService, 
            IClientViewModelService clientViewModelService,
            IEntityService<WebServiceLog> logService) : base(logService)
        {
            _patSettings = patSettings;
            _tmkSettings = tmkSettings;
            _logger = logger;
            _clientService = clientService;
            _clientViewModelService = clientViewModelService;
        }

        /// <summary>
        /// Get client by client id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Client data</returns>
        [Route("api/patent/[controller]/{id}")]
        [HttpGet]
        [Authorize(Policy = PatentAuthorizationPolicy.CanAccessSystem)]
        public async Task<IActionResult> GetPatClient(int id)
        {
            return await GetClient(id);
        }

        /// <summary>
        /// Get client by client id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Client data</returns>
        [Route("api/trademark/[controller]/{id}")]
        [HttpGet]
        [Authorize(Policy = TrademarkAuthorizationPolicy.CanAccessSystem)]
        public async Task<IActionResult> GetTmkClient(int id)
        {
            return await GetClient(id);
        }

        /// <summary>
        /// Search clients
        /// GET api/patent/clients
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns>Paged list of client data and the total number of records.</returns>
        [Route("api/patent/[controller]")]
        [HttpGet]
        [Authorize(Policy = PatentAuthorizationPolicy.CanAccessSystem)]
        public async Task<IActionResult> GetPatClients([FromQuery] ClientCriteria criteria)
        {
            return await GetClients(criteria);
        }

        /// <summary>
        /// Search clients
        /// GET api/trademark/clients
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns>Paged list of client data and the total number of records.</returns>
        [Route("api/trademark/[controller]")]
        [HttpGet]
        [Authorize(Policy = TrademarkAuthorizationPolicy.CanAccessSystem)]
        public async Task<IActionResult> GetTmkClients([FromQuery] ClientCriteria criteria)
        {
            return await GetClients(criteria);
        }

        private async Task<IActionResult> GetClients(ClientCriteria criteria)
        {
            if (!criteria.IsValid())
                return BadRequest(criteria.ErrorMessage);

            var logId = await InitWebApiLog();
            var settings = await _patSettings.GetSetting();
            var clients = _clientViewModelService.AddCriteria(_clientService.QueryableList, criteria.ToQueryFilter());
            var total = await clients.CountAsync();
            var data = await clients.ApplyPaging(criteria.Page, criteria.PageSize, settings.MaxApiPageSize).ProjectTo<ClientData>().ToListAsync();

            await UpdateWebApiLog(logId, CPiWebApiStatus.Success, data.Count);
            return Ok(new ApiResult<ClientData>()
            {
                Data = data,
                Total = total
            });
        }

        private async Task<IActionResult> GetClient(int id)
        {
            var logId = await InitWebApiLog();
            var data = await _clientService.QueryableList.Where(c => c.ClientID == id).ProjectTo<ClientData>().FirstOrDefaultAsync();

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
