using System.Text.Json.Serialization;

namespace R10.Web.Models.NetDocumentsModels
{
    public class Workspace : Container
    {
    }

    public class WorkspaceResponse
    {
        [JsonPropertyName("standardAttributes")]
        public WorkspaceStandardAttributes? StandardAttributes { get; set; }
    }

    public class WorkspaceStandardAttributes
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("envId")]
        public string? EnvId { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }
}
