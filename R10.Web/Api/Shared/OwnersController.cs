using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Validation.AspNetCore;
using R10.Core;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using R10.Core.Entities.Trademark;
using R10.Core.Exceptions;
using R10.Core.Helpers;
using R10.Core.Interfaces;
using R10.Core.Services.Shared;
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
    public class OwnersController : WebApiControllerBase
    {
        private readonly ISystemSettings<PatSetting> _patSettings;
        private readonly ISystemSettings<TmkSetting> _tmkSettings;
        private readonly ILogger _logger;
        private readonly IOwnerService _ownerService;
        private readonly IOwnerViewModelService _ownerViewModelService;

        public OwnersController(
            ISystemSettings<PatSetting> patSettings, 
            ISystemSettings<TmkSetting> tmkSettings, 
            ILogger<OwnersController> logger, 
            IOwnerService ownerService,
            IOwnerViewModelService ownerViewModelService,
            IEntityService<WebServiceLog> logService) : base(logService)
        {
            _patSettings = patSettings;
            _tmkSettings = tmkSettings;
            _logger = logger;
            _ownerService = ownerService;
            _ownerViewModelService = ownerViewModelService;
        }

        /// <summary>
        /// Get owner by owner id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Owner data</returns>
        [Route("api/patent/[controller]/{id}")]
        [HttpGet]
        [Authorize(Policy = PatentAuthorizationPolicy.CanAccessSystem)]
        public async Task<IActionResult> GetPatOwner(int id)
        {
            return await GetOwner(id);
        }

        /// <summary>
        /// Get owner by owner id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Owner data</returns>
        [Route("api/trademark/[controller]/{id}")]
        [HttpGet]
        [Authorize(Policy = TrademarkAuthorizationPolicy.CanAccessSystem)]
        public async Task<IActionResult> GetTmkOwner(int id)
        {
            return await GetOwner(id);
        }

        /// <summary>
        /// Search owners
        /// GET api/patent/owners
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns>Paged list of owner data and the total number of records.</returns>
        [Route("api/patent/[controller]")]
        [HttpGet]
        [Authorize(Policy = PatentAuthorizationPolicy.CanAccessSystem)]
        public async Task<IActionResult> GetPatOwners([FromQuery] OwnerCriteria criteria)
        {
            return await GetOwners(criteria);
        }

        /// <summary>
        /// Search owners
        /// GET api/trademark/owners
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns>Paged list of owner data and the total number of records.</returns>
        [Route("api/trademark/[controller]")]
        [HttpGet]
        [Authorize(Policy = TrademarkAuthorizationPolicy.CanAccessSystem)]
        public async Task<IActionResult> GetTmkOwners([FromQuery] OwnerCriteria criteria)
        {
            return await GetOwners(criteria);
        }

        private async Task<IActionResult> GetOwners(OwnerCriteria criteria)
        {
            if (!criteria.IsValid())
                return BadRequest(criteria.ErrorMessage);

            var logId = await InitWebApiLog();
            var settings = await _patSettings.GetSetting();
            var owners = _ownerViewModelService.AddCriteria(_ownerService.QueryableList, criteria.ToQueryFilter());
            var total = await owners.CountAsync();
            var data = await owners.ApplyPaging(criteria.Page, criteria.PageSize, settings.MaxApiPageSize).ProjectTo<OwnerData>().ToListAsync();

            await UpdateWebApiLog(logId, CPiWebApiStatus.Success, data.Count);
            return Ok(new ApiResult<OwnerData>()
            {
                Data = data,
                Total = total
            });
        }

        private async Task<IActionResult> GetOwner(int id)
        {
            var logId = await InitWebApiLog();
            var data = await _ownerService.QueryableList.Where(o => o.OwnerID == id).ProjectTo<OwnerData>().FirstOrDefaultAsync();

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
