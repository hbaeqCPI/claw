using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace R10.Web.Models.IManageModels
{
    /// <summary>
    /// The container profile varies depending upon the type of container as follows:
    /// A workspace profile consists of author, class, custom properties, default security, and so on.
    /// A regular folder profile consists of name, owner, custom properties, and default security, and so on.
    /// A folder shortcut profile consists of additional information about the target folder such as folder ID, library, folder type, subtype, and so on.
    /// </summary>
    public class Container
    {
        [JsonPropertyName("database")]
        public string? Database { get; set; }

        [JsonPropertyName("default_security")]
        public string? DefaultSecurity { get; set; }

        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("has_subfolders")]
        public bool? HasSubFolders { get; set; }

        [JsonPropertyName("wstype")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ContainerType? ContainerType { get; set; }
    }

    public class ContainerResponse
    {
        [JsonPropertyName("data")]
        public Container? Data { get; set; }
    }

    public enum ContainerType
    {
        Undefined,
        [EnumMember(Value = "workspace")]
        Workspace,
        [EnumMember(Value = "folder")]
        Folder,
        [EnumMember(Value = "folder_shortcut")]
        FolderShortcut
    }
}
