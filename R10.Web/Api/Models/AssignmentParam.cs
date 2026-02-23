using NJsonSchema.Annotations;
using R10.Core.Entities.Patent;
using R10.Core.Entities.Shared;

namespace R10.Web.Api.Models
{
    [JsonSchemaFlattenAttribute]
    public class AssignmentParam : AssignmentHistoryWebSvc
    {
    }
}
