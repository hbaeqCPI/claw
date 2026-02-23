using System.Text.Json.Serialization;

namespace R10.Web.Models.NetDocumentsModels
{
    public class AuthResult
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }

        [JsonPropertyName("refresh_token")]
        public string? RefreshToken { get; set; }

        [JsonPropertyName("token_type")]
        public string? TokenType { get; set; }

        [JsonPropertyName("expires_in")]
        public string? ExpiresIn { get; set; } //seconds (netdocs return expires_in as string type)
    }
}
