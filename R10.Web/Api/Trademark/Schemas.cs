using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NJsonSchema;
using R10.Web.Api.Models;
using R10.Web.Extensions;

namespace R10.Web.Api.Trademark
{
    [Route("api/trademark/[controller]")]
    [ApiController]
    public class Schemas : ControllerBase
    {
        private Dictionary<string, JsonSchema> schemas = new Dictionary<string, JsonSchema>()
        {
            //trademark
            { "trademark", JsonSchema.FromType<TrademarkData>() },
            { "trademark-list", JsonSchema.FromType<List<TrademarkData>>() },
            { "trademark-search-result", JsonSchema.FromType<ApiResult<TrademarkData>>() },
            { "trademark-create-parameter", JsonSchema.FromType<TrademarkParam>() },
            { "trademark-update-parameter", JsonSchema.FromType<TrademarkParam>() },
            { "trademark-batch-create-parameter", JsonSchema.FromType<List<TrademarkParam>>() },
            { "trademark-batch-update-parameter", JsonSchema.FromType<List<TrademarkParam>>() },
            //tmk owners
            { "trademark-owner-list", JsonSchema.FromType<List<TmkOwnerData>>() },
            { "trademark-owner-create-parameter", JsonSchema.FromType<List<TmkOwnerCreateParam>>() },
            { "trademark-owner-update-parameter", JsonSchema.FromType<List<TmkOwnerUpdateParam>>() },
            { "trademark-owner-delete-parameter", JsonSchema.FromType<List<TmkOwnerDeleteParam>>() },
            //actions
            { "action-due", JsonSchema.FromType<ActionDueData>() },
            { "action-due-list", JsonSchema.FromType<List<ActionDueData>>() },
            { "action-due-search-result", JsonSchema.FromType<ApiResult<ActionDueData>>() },
            { "action-due-create-parameter", JsonSchema.FromType<ActionDueCreateParam>() },
            { "action-due-update-parameter", JsonSchema.FromType<ActionDueUpdateParam>() },
            { "action-due-batch-create-parameter", JsonSchema.FromType<List<ActionDueCreateParam>>() },
            { "action-due-batch-update-parameter", JsonSchema.FromType<List<ActionDueUpdateParam>>() },
            //classes
            { "trademark-class", JsonSchema.FromType<TrademarkClassData>() },
            { "trademark-class-list", JsonSchema.FromType<List<TrademarkClassData>>() },
            { "trademark-class-create-parameter", JsonSchema.FromType<TrademarkClassParam>() },
            { "trademark-class-update-parameter", JsonSchema.FromType<TrademarkClassParam>() },
            //assignments
            { "assignment-list", JsonSchema.FromType<List<TmkAssignmentData>>() },
            { "assignment-create-parameter", JsonSchema.FromType<List<AssignmentParam>>() },
            { "assignment-batch-create-parameter", JsonSchema.FromType<List<TmkAssignmentParam>>() },
            { "assignment-status-list", JsonSchema.FromType<List<AssignmentStatusData>>() },
            //auxiliary
            { "agent-search-result", JsonSchema.FromType<ApiResult<AgentData>>() },
            { "owner-search-result", JsonSchema.FromType<ApiResult<OwnerData>>() },
            { "attorney-search-result", JsonSchema.FromType<ApiResult<AttorneyData>>() },
            { "client-search-result", JsonSchema.FromType<ApiResult<ClientData>>() },
            { "trademark-status-search-result", JsonSchema.FromType<ApiResult<TrademarkStatusData>>() },
            { "mark-type-search-result", JsonSchema.FromType<ApiResult<MarkTypeData>>() },
            { "country-search-result", JsonSchema.FromType<ApiResult<CountryData>>() },
            { "case-type-search-result", JsonSchema.FromType<ApiResult<CaseTypeData>>() },
            { "standard-goods-search-result", JsonSchema.FromType<ApiResult<StandardGoodsData>>() }
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
