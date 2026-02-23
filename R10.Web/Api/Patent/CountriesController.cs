using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Validation.AspNetCore;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using R10.Core.Interfaces;
using R10.Web.Api.Models;
using R10.Web.Extensions;
using R10.Web.Helpers;
using R10.Web.Interfaces;
using R10.Web.Security;

namespace R10.Web.Api.Patent
{
    [Route("api/patent/[controller]")]
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme, Policy = PatentAuthorizationPolicy.CanAccessSystem)]
    [ApiController]
    public class CountriesController : WebApiControllerBase
    {
        private readonly ISystemSettings<PatSetting> _patSettings;
        private readonly ILogger _logger;
        private readonly IEntityService<PatCountry> _auxService;
        private readonly IViewModelService<PatCountry> _viewModelService;

        public CountriesController(
            ISystemSettings<PatSetting> patSettings, 
            ILogger<CountriesController> logger, 
            IEntityService<PatCountry> auxService, 
            IViewModelService<PatCountry> viewModelService,
            IEntityService<WebServiceLog> logService) : base(logService)
        {
            _patSettings = patSettings;
            _logger = logger;
            _auxService = auxService;
            _viewModelService = viewModelService;
        }

        /// <summary>
        /// Get country by country id
        /// GET api/patent/countries/{countriesId}
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Country data</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCountry(int id)
        {
            var logId = await InitWebApiLog();
            var data = await _auxService.QueryableList.Where(c => c.CountryID == id).ProjectTo<CountryData>().FirstOrDefaultAsync();

            if (data == null)
            {
                await UpdateWebApiLog(logId, CPiWebApiStatus.NotFound);
                return NotFound();
            }

            await UpdateWebApiLog(logId, CPiWebApiStatus.Success, 1);

            return Ok(data);
        }

        /// <summary>
        /// Search country
        /// GET api/patent/countries/?country={country}
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns>Paged list of country data and the total number of records.</returns>
        [HttpGet]
        public async Task<IActionResult> SearchCountries([FromQuery] CountryCriteria criteria)
        {
            if (!criteria.IsValid())
                return BadRequest(criteria.ErrorMessage);

            var logId = await InitWebApiLog();
            var settings = await _patSettings.GetSetting();
            var result = _viewModelService.AddCriteria(_auxService.QueryableList, criteria.ToQueryFilter());
            var total = await result.CountAsync();
            var data = await result.ApplyPaging(criteria.Page, criteria.PageSize, settings.MaxApiPageSize).ProjectTo<CountryData>().ToListAsync();

            await UpdateWebApiLog(logId, CPiWebApiStatus.Success, data.Count);
            return Ok(new ApiResult<CountryData>()
            {
                Data = data,
                Total = total
            });
        }
    }
}
