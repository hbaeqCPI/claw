using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Validation.AspNetCore;
using R10.Core.Entities;
using R10.Core.Entities.Trademark;
using R10.Core.Interfaces;
using R10.Web.Api.Models;
using R10.Web.Extensions;
using R10.Web.Helpers;
using R10.Web.Interfaces;
using R10.Web.Security;

namespace R10.Web.Api.Trademark
{
    [Route("api/trademark/[controller]")]
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme, Policy = TrademarkAuthorizationPolicy.CanAccessSystem)]
    [ApiController]
    public class CaseTypesController : WebApiControllerBase
    {
        private readonly ISystemSettings<TmkSetting> _tmkSettings;
        private readonly ILogger _logger;
        private readonly IEntityService<TmkCaseType> _auxService;
        private readonly IViewModelService<TmkCaseType> _viewModelService;

        public CaseTypesController(
            ISystemSettings<TmkSetting> tmkSettings,
            ILogger<CaseTypesController> logger,
            IEntityService<TmkCaseType> auxService,
            IViewModelService<TmkCaseType> viewModelService,
            IEntityService<WebServiceLog> logService) : base(logService)
        {
            _tmkSettings = tmkSettings;
            _logger = logger;
            _auxService = auxService;
            _viewModelService = viewModelService;
        }

        /// <summary>
        /// Get case type by case type id
        /// GET api/trademark/casetypes/{caseTypeId}
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Case type data</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCaseType(int id)
        {
            var logId = await InitWebApiLog();
            var data = await _auxService.QueryableList.Where(c => c.CaseTypeId == id).ProjectTo<CaseTypeData>().FirstOrDefaultAsync();

            if (data == null)
            {
                await UpdateWebApiLog(logId, CPiWebApiStatus.NotFound);
                return NotFound();
            }

            await UpdateWebApiLog(logId, CPiWebApiStatus.Success, 1);

            return Ok(data);
        }

        /// <summary>
        /// Search case type
        /// GET api/trademark/casetypes/?casetype={caseType}
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns>Paged list of case type data and the total number of records.</returns>
        [HttpGet]
        public async Task<IActionResult> GetCaseTypes([FromQuery] CaseTypeCriteria criteria)
        {
            if (!criteria.IsValid())
                return BadRequest(criteria.ErrorMessage);

            var logId = await InitWebApiLog();
            var settings = await _tmkSettings.GetSetting();
            var result = _viewModelService.AddCriteria(_auxService.QueryableList, criteria.ToQueryFilter());
            var total = await result.CountAsync();
            var data = await result.ApplyPaging(criteria.Page, criteria.PageSize, settings.MaxApiPageSize).ProjectTo<CaseTypeData>().ToListAsync();

            await UpdateWebApiLog(logId, CPiWebApiStatus.Success, data.Count);
            return Ok(new ApiResult<CaseTypeData>()
            {
                Data = data,
                Total = total
            });
        }
    }
}
