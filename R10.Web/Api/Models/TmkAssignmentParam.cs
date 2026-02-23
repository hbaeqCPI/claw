using NJsonSchema.Annotations;
using R10.Core.Entities.Trademark;

namespace R10.Web.Api.Models
{
    [JsonSchemaFlattenAttribute]
    public class TmkAssignmentParam : TmkAssignmentHistoryWebSvcDetail
    {
    }
}
