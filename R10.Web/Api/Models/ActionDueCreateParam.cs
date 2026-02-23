using NJsonSchema.Annotations;
using R10.Core.Entities.Shared;

namespace R10.Web.Api.Models
{
    [JsonSchemaFlattenAttribute]
    public class ActionDueCreateParam : ActionDueWebSvcDetail
    {
        public List<DueDateParam>? DueDates { get; set; }
    }
}
