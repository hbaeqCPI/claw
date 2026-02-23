using NJsonSchema.Annotations;
using R10.Core.Entities.Patent;

namespace R10.Web.Api.Models
{
    [JsonSchemaFlattenAttribute]
    public class AppAssignmentParam : PatAssignmentHistoryWebSvcDetail
    {
    }
}
