using System.Text.Json.Serialization;

namespace R10.Web.Models.IManageModels
{
    public class Document
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("document_number")]
        public int DocumentNumber { get; set; }

        [JsonPropertyName("author")]
        public string? Author {  get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("extension")]
        public string? Extension { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("size")]
        public int Size { get; set; }

        [JsonPropertyName("is_checked_out")]
        public bool IsCheckedOut { get; set; }

        [JsonPropertyName("is_in_use")]
        public bool IsInUse { get; set; }

        [JsonPropertyName("in_use_by")]
        public string? InUseBy { get; set; }

        [JsonPropertyName("create_date")]
        public DateTime? CreateDate { get; set; }

        [JsonPropertyName("edit_date")]
        public DateTime? EditDate { get; set; }

        [JsonPropertyName("last_user")]
        public string? LastUser { get; set; }

        [JsonPropertyName("version")]
        public int Version { get; set; }

        [JsonPropertyName("workspace_id")]
        public string? WorkspaceId { get; set; }
    }

    public class DocumentResponse
    {
        [JsonPropertyName("data")]
        public Document? Data { get; set; }
    }

    public class DocumentsResponse
    {
        [JsonPropertyName("cursor")]
        public string? Cursor { get; set; }


        [JsonPropertyName("data")]
        public DocumentsData? Data { get; set; }

        [JsonPropertyName("total_count")]
        public int TotalCount { get; set; }
    }

    public class DocumentsData
    {
        [JsonPropertyName("results")]
        public List<Document>? Results { get; set; }
    }

    public class DocumentType
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("app_extension")]
        public string? AppExtension { get; set; }
    }

    public class DocumentTypesResponse
    {
        [JsonPropertyName("data")]
        public List<DocumentType>? Data { get; set; }
    }

    public class DocumentSearchResponse
    {

        [JsonPropertyName("data")]
        public List<Document>? Data { get; set; }

        [JsonPropertyName("overflow")]
        public bool Overflow { get; set; }
    }

    public class UpdatableDocumentProfile
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }
    }
}
