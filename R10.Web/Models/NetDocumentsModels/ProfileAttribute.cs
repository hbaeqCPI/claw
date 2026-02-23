using System.Text.Json.Serialization;

namespace R10.Web.Models.NetDocumentsModels
{
    public class ProfileAttribute
    {
        [JsonPropertyName("key")]
        public string? Key { get; set; }

        [JsonPropertyName("parent")]
        public string? Parent { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("hold")]
        public bool? Hold { get; set; }

        [JsonPropertyName("defaulting")]
        public string? Defaulting { get; set; }
    }
}
