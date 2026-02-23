using DocumentFormat.OpenXml.Office2013.Drawing.ChartStyle;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OpenIddict.Validation.AspNetCore;
using R10.Core.Entities;
using R10.Core.Helpers;
using R10.Core.Interfaces;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Interfaces.Shared;
using R10.Web.Security;

namespace R10.Web.Api.Shared
{
    [Route("api/CustomQuery")]
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme, Policy = SharedAuthorizationPolicy.CanAccessPowerBIConnector)]
    [ApiController]
    public class PowerBIConnectorController : ControllerBase
    {
        private readonly IDataQueryService _dataQueryService;

        public PowerBIConnectorController(IDataQueryService dataQueryService)
        {
            _dataQueryService = dataQueryService;
        }

        private IQueryable<DataQueryMain> DataQueryMain => _dataQueryService.DataQueriesMain
            .Where(q => (q.IsShared || q.OwnedBy == User.GetEmail()) && !string.IsNullOrEmpty(q.SQLExpr));

        [HttpGet("Names")]
        public async Task<IActionResult> Names()
        {
            var names =  await DataQueryMain.Select(q => q.QueryName).ToListAsync();

            return Ok(names.OrderBy(q => q));
        }

        public async Task<IActionResult> Get(string queryName)
        {
            var dataQuery = await DataQueryMain.Where(q => q.QueryName == queryName).FirstOrDefaultAsync();

            if (dataQuery == null)
                return Unauthorized();

            if (string.IsNullOrEmpty(dataQuery.SQLExpr))
                return BadRequest("Invalid query");

            try
            {
                var dt = _dataQueryService.RunCRQuery(dataQuery.SQLExpr, User.GetUserIdentifier(), User.HasEntityFilter(), User.HasRespOfficeFilter());

                return Ok(JsonConvert.SerializeObject(dt));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
