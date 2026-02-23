using System.Text.Json.Serialization;

namespace R10.Web.Models.NetDocumentsModels
{
    public class UserInfo
    {
        [JsonPropertyName("displayName")]
        public string? DisplayName { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("organization")]
        public string? Organization { get; set; }

        [JsonPropertyName("primaryCabinet")]
        public string? PrimaryCabinet { get; set; }

        [JsonPropertyName("sortLookupBy")]
        public string? SortLookupBy { get; set; }
    }
}
