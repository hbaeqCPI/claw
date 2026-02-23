using NJsonSchema.Annotations;
using R10.Core.Entities.Patent;

namespace R10.Web.Api.Models
{
    [JsonSchemaFlattenAttribute]
    public class PatPriorityData : PatPriorityWebSvcDetail
    {
        public int PriId { get; set; }

        public int InvId { get; set; }

        public string? CountryName { get; set; }
    }
}
