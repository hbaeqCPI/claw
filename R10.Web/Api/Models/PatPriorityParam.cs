using NJsonSchema.Annotations;
using R10.Core.Entities.Patent;

namespace R10.Web.Api.Models
{
    [JsonSchemaFlattenAttribute]
    public class PatPriorityCreateParam : PatPriorityWebSvcDetail
    {
    }

    [JsonSchemaFlattenAttribute]
    public class PatPriorityUpdateParam : PatPriorityWebSvcDetail
    {
        public int PriId { get; set; }
    }

    [JsonSchemaFlattenAttribute]
    public class PatPriorityDeleteParam
    {
        public int PriId { get; set; }
    }
}
