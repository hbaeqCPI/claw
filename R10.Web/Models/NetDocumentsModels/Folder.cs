using System.Text.Json.Serialization;

namespace R10.Web.Models.NetDocumentsModels
{
    public class Folder : Container
    {
        public string? ParentId => Ancestors?.FirstOrDefault(a => string.Equals(a.Type, "Folder", StringComparison.OrdinalIgnoreCase))?.Id;
        public bool? HasSubFolders => SubFolders?.Any();
        public bool? HasDocuments => false;

        public List<Folder>? SubFolders { get; set; }
    }

    public class FolderResponse : Folder
    {
    }

    public class FoldersResponse
    {
        public List<Folder>? Results { get; set; }
    }

    /// <summary>
    /// v1 endpoint response
    /// </summary>
    public class  FolderCreateResponse
    {
        [JsonPropertyName("standardAttributes")]
        public StandardAttributes? StandardAttributes { get; set; }
    }

    /// <summary>
    /// v1 standard attributes
    /// </summary>
    public class StandardAttributes
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("envId")]
        public string? EnvId { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }
}
