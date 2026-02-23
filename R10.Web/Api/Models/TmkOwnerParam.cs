using NJsonSchema.Annotations;
using R10.Core.Entities.Trademark;

namespace R10.Web.Api.Models
{
    [JsonSchemaFlattenAttribute]
    public class TmkOwnerCreateParam : TmkOwnerWebSvcDetail
    {
    }

    [JsonSchemaFlattenAttribute]
    public class TmkOwnerUpdateParam : TmkOwnerWebSvcDetail
    {
        public int TmkOwnerId { get; set; }
    }

    [JsonSchemaFlattenAttribute]
    public class TmkOwnerDeleteParam
    {
        public int TmkOwnerId { get; set; }
    }
}
