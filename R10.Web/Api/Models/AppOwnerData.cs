using NJsonSchema.Annotations;
using R10.Core.Entities.Shared;

namespace R10.Web.Api.Models
{
    [JsonSchemaFlattenAttribute]
    public class AppOwnerData : OwnerData
    {
        public int AppId { get; set; }

        public int OwnerAppId { get; set; }

        public double? Percentage { get; set; }
    }
}
