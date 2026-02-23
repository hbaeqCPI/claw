using AutoMapper;
using System.Text.Json.Serialization;

namespace R10.Web.Models.IManageModels
{
    public class Workspace : Container
    {
    }

    public class WorkspaceResponse
    {
        [JsonPropertyName("data")]
        public Workspace? Data { get; set; }
    }

    public class WorkspacesResponse
    {
        [JsonPropertyName("data")]
        public WorkspacesData? Data { get; set; }
    }

    public class WorkspacesData
    {
        [JsonPropertyName("results")]
        public List<Workspace>? Results { get; set; }
    }

    public class NameValuePairsResponse
    {
        [JsonPropertyName("data")]
        public Dictionary<string, string>? Data { get; set; }
    }
}
