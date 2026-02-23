using System.Text.Json.Serialization;

namespace R10.Web.Models.IManageModels
{
    public class AuthData
    {

        public AuthData(string? xAuthToken, ApiData? data)
        {
            XAuthToken = xAuthToken;
            Data = data;
        }

        [JsonPropertyName("access_token")]
        public string? XAuthToken { get; set; } //X-Auth-Token header

        [JsonPropertyName("data")]
        public ApiData? Data { get; set; }
    }

    //{
    //  "data":{
    //      "app":{ "id":"afd5f8d1-983b-477e-9e7d-d49e16e317d6"},
    //      "auth_status":"authenticated",
    //      "capabilities":["app_store","global_users","oauth"],
    //      "dms_version":"10.2.3812",
    //      "user":{
    //          "anonymous_id":"100470043383228681",
    //          "customer_id":5948,
    //          "email":"jdivinagracia@computerpackages.com",
    //          "id":"JDIVINAGRACIA",
    //          "name":"JDIVINAGRACIA",
    //          "ssid":"100470043383228681",
    //          "user_type":"enterprise"
    //      },
    //      "versions":[{
    //          "name":"v2",
    //          "url":"https://cloudimanage.com/work/api/v2",
    //          "version":"2.1.1094"
    //      }],
    //      "work":{
    //          "libraries":[{
    //              "alias":"Dev",
    //              "attributes":{ "knowledge":false},
    //              "is_classic_client_compatible":false,
    //              "status":"ready",
    //              "type":"worksite"
    //          }],
    //          "preferred_library":"Dev"
    //      }
    //  }
    //}
    public class ApiData
    {
        [JsonPropertyName("auth_status")]
        public string? AuthStatus { get; set; }

        [JsonPropertyName("user")]
        public AuthUserData? User { get; set; }

        [JsonPropertyName("versions")]
        public List<ApiVersion>? Versions { get; set; }

    }

    public class ApiVersion
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("url")]
        public string? Url { get; set; }

        [JsonPropertyName("version")]
        public string? Version { get; set; }
    }

    //"user":{
    //    "anonymous_id":"100470043383228681",
    //    "customer_id":5948,
    //    "email":"jdivinagracia@computerpackages.com",
    //    "id":"JDIVINAGRACIA",
    //    "name":"JDIVINAGRACIA",
    //    "ssid":"100470043383228681",
    //    "user_type":"enterprise"
    //},
    public class AuthUserData
    {
        [JsonPropertyName("anonymous_id")]
        public string? AnonymousId { get; set; }

        [JsonPropertyName("customer_id")]
        public int CustomerId { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("ssid")]
        public string? SSID { get; set; }

        [JsonPropertyName("user_type")]
        public string? UserType { get; set; }
    }
}
