using NJsonSchema.Annotations;
using R10.Core.Entities.Patent;

namespace R10.Web.Api.Models
{
    [JsonSchemaFlattenAttribute]
    public class AppOwnerCreateParam : PatOwnerAppWebSvcDetail
    {
    }

    [JsonSchemaFlattenAttribute]
    public class AppOwnerUpdateParam : PatOwnerAppWebSvcDetail
    {
        public int OwnerAppId { get; set; }
    }

    [JsonSchemaFlattenAttribute]
    public class AppOwnerDeleteParam
    {
        public int OwnerAppId { get; set; }
    }
}
