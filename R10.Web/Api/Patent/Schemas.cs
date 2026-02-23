using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NJsonSchema;
using OpenIddict.Validation.AspNetCore;
using R10.Core.Services;
using R10.Web.Api.Models;
using R10.Web.Extensions;
using R10.Web.Security;
using System.Text;

namespace R10.Web.Api.Patent
{
    [Authorize(AuthenticationSchemes = $"Identity.Application,{OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme}", Policy = PatentAuthorizationPolicy.CanAccessSystem)]
    [Route("api/patent/[controller]")]
    [ApiController]
    public class Schemas : ControllerBase
    {
        private Dictionary<string, JsonSchema> schemas = new Dictionary<string, JsonSchema>()
        {
            //invention
            { "invention", JsonSchema.FromType<InventionData>() },
            { "invention-list", JsonSchema.FromType<List<InventionData>>() },
            { "invention-search-result", JsonSchema.FromType<ApiResult<InventionData>>() },
            { "invention-create-parameter", JsonSchema.FromType<InventionParam>() },
            { "invention-update-parameter", JsonSchema.FromType<InventionParam>() },
            { "invention-batch-create-parameter", JsonSchema.FromType<List<InventionParam>>() },
            { "invention-batch-update-parameter", JsonSchema.FromType<List<InventionParam>>() },
            //priorities
            { "priority-list", JsonSchema.FromType<List<PatPriorityData>>() },
            { "priority-create-parameter", JsonSchema.FromType<List<PatPriorityCreateParam>>() },
            { "priority-update-parameter", JsonSchema.FromType<List<PatPriorityUpdateParam>>() },
            { "priority-delete-parameter", JsonSchema.FromType<List<PatPriorityDeleteParam>>() },
            //country app
            { "application", JsonSchema.FromType<CountryApplicationData>() },
            { "application-list", JsonSchema.FromType<List<CountryApplicationData>>() },
            { "application-search-result", JsonSchema.FromType<ApiResult<CountryApplicationData>>() },
            { "application-create-parameter", JsonSchema.FromType<CountryApplicationParam>() },
            { "application-update-parameter", JsonSchema.FromType<CountryApplicationParam>() },
            { "application-batch-create-parameter", JsonSchema.FromType<List<CountryApplicationParam>>() },
            { "application-batch-update-parameter", JsonSchema.FromType<List<CountryApplicationParam>>() },
            //app owners
            { "application-owner-list", JsonSchema.FromType<List<AppOwnerData>>() },
            { "application-owner-create-parameter", JsonSchema.FromType<List<AppOwnerCreateParam>>() },
            { "application-owner-update-parameter", JsonSchema.FromType<List<AppOwnerUpdateParam>>() },
            { "application-owner-delete-parameter", JsonSchema.FromType<List<AppOwnerDeleteParam>>() },
            //related cases
            { "related-case-list", JsonSchema.FromType<List<PatRelatedCaseData>>() },
            //assignments
            { "assignment-list", JsonSchema.FromType<List<AppAssignmentData>>() },
            { "assignment-create-parameter", JsonSchema.FromType<List<AssignmentParam>>() },
            { "assignment-batch-create-parameter", JsonSchema.FromType<List<AppAssignmentParam>>() },
            //actions
            { "action-due", JsonSchema.FromType<ActionDueData>() },
            { "action-due-list", JsonSchema.FromType<List<ActionDueData>>() },
            { "action-due-search-result", JsonSchema.FromType<ApiResult<ActionDueData>>() },
            { "action-due-create-parameter", JsonSchema.FromType<ActionDueCreateParam>() },
            { "action-due-update-parameter", JsonSchema.FromType<ActionDueUpdateParam>() },
            { "action-due-batch-create-parameter", JsonSchema.FromType<List<ActionDueCreateParam>>() },
            { "action-due-batch-update-parameter", JsonSchema.FromType<List<ActionDueUpdateParam>>() },
            //due dates
            { "action-due-date-list", JsonSchema.FromType<List<DueDateData>>() },
            { "action-due-date-create-parameter", JsonSchema.FromType<List<DueDateParam>>() },
            { "action-due-date-update-parameter", JsonSchema.FromType<List<DueDateParam>>() },
            //cost tracking
            { "cost", JsonSchema.FromType<PatCostTrackingData>() },
            { "cost-list", JsonSchema.FromType<List<PatCostTrackingData>>() },
            { "cost-search-result", JsonSchema.FromType<ApiResult<PatCostTrackingData>>() },
            { "cost-create-parameter", JsonSchema.FromType<PatCostTrackingParam>() },
            { "cost-update-parameter", JsonSchema.FromType<PatCostTrackingParam>() },
            { "cost-batch-create-parameter", JsonSchema.FromType<List<PatCostTrackingParam>>() },
            { "cost-batch-update-parameter", JsonSchema.FromType<List<PatCostTrackingParam>>() },
            //auxiliary
            { "agent-search-result", JsonSchema.FromType<ApiResult<AgentData>>() },
            { "owner-search-result", JsonSchema.FromType<ApiResult<OwnerData>>() },
            { "attorney-search-result", JsonSchema.FromType<ApiResult<AttorneyData>>() },
            { "client-search-result", JsonSchema.FromType<ApiResult<ClientData>>() },
            { "inventor-search-result", JsonSchema.FromType<ApiResult<InventorData>>() },
            { "application-status-search-result", JsonSchema.FromType<ApiResult<ApplicationStatusData>>() },
            { "assignment-status-search-result", JsonSchema.FromType<ApiResult<AssignmentStatusData>>() },
            { "country-search-result", JsonSchema.FromType<ApiResult<CountryData>>() },
            { "case-type-search-result", JsonSchema.FromType<ApiResult<CaseTypeData>>() },
            { "disclosure-status-search-result", JsonSchema.FromType<ApiResult<DisclosureStatusData>>() }
        };

        [HttpGet("{id}")]
        public IActionResult Get(string id)
        {
            var schema = new JsonSchema();

            if (schemas.TryGetValue(id, out schema))
                return schema.ToJsonFileContentResult();

            return NotFound();
        }
    }
}
