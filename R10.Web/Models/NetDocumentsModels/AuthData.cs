using System.Text.Json.Serialization;

namespace R10.Web.Models.NetDocumentsModels
{
    public class AuthData
    {
        public AuthData(string? accessToken, UserInfo? data)
        {
            AccessToken = accessToken;
            Data = data;
        }

        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }

        [JsonPropertyName("data")]
        public UserInfo? Data { get; set; }
    }
}
