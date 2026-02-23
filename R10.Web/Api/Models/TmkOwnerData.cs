using NJsonSchema.Annotations;

namespace R10.Web.Api.Models
{
    [JsonSchemaFlattenAttribute]
    public class TmkOwnerData : OwnerData
    {
        public int TmkId { get; set; }

        public int TmkOwnerId { get; set; }

        public double? Percentage { get; set; }
    }
}
