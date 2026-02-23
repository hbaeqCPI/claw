using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Validation.AspNetCore;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using R10.Core.Entities.Trademark;
using R10.Core.Interfaces;
using R10.Core.Services.Shared;
using R10.Web.Api.Models;
using R10.Web.Extensions;
using R10.Web.Helpers;
using R10.Web.Interfaces;
using R10.Web.Security;
using R10.Web.Services;

namespace R10.Web.Api.Shared
{
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme, Policy = SharedAuthorizationPolicy.CanAccessSystem)]
    [ApiController]
    public class AttorneysController : WebApiControllerBase
    {
        private readonly ISystemSettings<PatSetting> _patSettings;
        private readonly ISystemSettings<TmkSetting> _tmkSettings;
        private readonly ILogger _logger;
        private readonly IAttorneyService _attorneyService;
        private readonly IViewModelService<Attorney> _attorneyViewModelService;

        public AttorneysController(
            ISystemSettings<PatSetting> patSettings, 
            ISystemSettings<TmkSetting> tmkSettings, 
            ILogger<AttorneysController> logger, 
            IAttorneyService attorneyService, 
            IViewModelService<Attorney> attorneyViewModelService,
            IEntityService<WebServiceLog> logService) : base(logService)
        {
            _patSettings = patSettings;
            _tmkSettings = tmkSettings;
            _logger = logger;
            _attorneyService = attorneyService;
            _attorneyViewModelService = attorneyViewModelService;
        }

        /// <summary>
        /// Get attorney by attorney id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>attorney data</returns>
        [Route("api/patent/[controller]/{id}")]
        [HttpGet]
        [Authorize(Policy = PatentAuthorizationPolicy.CanAccessSystem)]
        public async Task<IActionResult> GetPatAttorney(int id)
        {
            return await GetAttorney(id);
        }

        /// <summary>
        /// Get attorney by attorney id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>attorney data</returns>
        [Route("api/trademark/[controller]/{id}")]
        [HttpGet]
        [Authorize(Policy = TrademarkAuthorizationPolicy.CanAccessSystem)]
        public async Task<IActionResult> GetTmkAttorney(int id)
        {
            return await GetAttorney(id);
        }

        /// <summary>
        /// Search attorneys
        /// GET api/patent/attorneys
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns>Paged list of attorney data and the total number of records.</returns>
        [Route("api/patent/[controller]")]
        [HttpGet]
        [Authorize(Policy = PatentAuthorizationPolicy.CanAccessSystem)]
        public async Task<IActionResult> GetPatAttorneys([FromQuery] AttorneyCriteria criteria)
        {
            return await GetAttorneys(criteria);
        }

        /// <summary>
        /// Search attorneys
        /// GET api/trademark/attorneys
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns>Paged list of attorney data and the total number of records.</returns>
        [Route("api/trademark/[controller]")]
        [HttpGet]
        [Authorize(Policy = TrademarkAuthorizationPolicy.CanAccessSystem)]
        public async Task<IActionResult> GetTmkattorneys([FromQuery] AttorneyCriteria criteria)
        {
            return await GetAttorneys(criteria);
        }

        private async Task<IActionResult> GetAttorneys(AttorneyCriteria criteria)
        {
            if (!criteria.IsValid())
                return BadRequest(criteria.ErrorMessage);

            var logId = await InitWebApiLog();
            var settings = await _patSettings.GetSetting();
            var attorneys = _attorneyViewModelService.AddCriteria(_attorneyService.QueryableList, criteria.ToQueryFilter());
            var total = await attorneys.CountAsync();
            var data = await attorneys.ApplyPaging(criteria.Page, criteria.PageSize, settings.MaxApiPageSize).ProjectTo<AttorneyData>().ToListAsync();

            await UpdateWebApiLog(logId, CPiWebApiStatus.Success, data.Count);
            return Ok(new ApiResult<AttorneyData>()
            {
                Data = data,
                Total = total
            });
        }

        private async Task<IActionResult> GetAttorney(int id)
        {
            var logId = await InitWebApiLog();
            var data = await _attorneyService.QueryableList.Where(a => a.AttorneyID == id).ProjectTo<AttorneyData>().FirstOrDefaultAsync();

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
