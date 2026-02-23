using System.Text.Json.Serialization;

namespace R10.Web.Models.IManageModels
{
    public class AuthResult
    {
        [JsonPropertyName("access_token")]
        public string? XAuthToken { get; set; } //X-Auth-Token header

        [JsonPropertyName("refresh_token")]
        public string? RefreshToken { get; set; }

        [JsonPropertyName("token_type")]
        public string? TokenType { get; set; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; } //seconds

        [JsonPropertyName("scope")]
        public string? Scope { get; set; }
    }
}
