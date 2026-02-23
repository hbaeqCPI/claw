using NJsonSchema.Annotations;
using R10.Core.Entities.Patent;

namespace R10.Web.Api.Models
{
    [JsonSchemaFlattenAttribute]
    public class InventionParam : InventionWebSvcDetail
    {
        public List<InventorParam>? Inventors { get; set; }
    }

    [JsonSchemaFlattenAttribute]
    public class InventorParam : PatInventorWebSvc
    {
    }
}
