using System.Text.Json.Serialization;

namespace R10.Web.Models.NetDocumentsModels
{
    public class Document
    {
        /// <summary>
        /// The document's DocId.
        /// DocId is saved in tblDocFile.DriveItemId.
        /// </summary>
        public virtual string? Id => DocId;

        /// <summary>
        /// Please use Id property to get the document or container id
        /// </summary>
        public string? DocId { get; set; }

        public string? EnvId { get; set; }
        public int DocNum { get; set; }
        public DocumentAttributes? Attributes { get; set; }
        public List<DocumentAncestor>? Ancestors { get; set; }
        public bool? LimitAceess {  get; set; }
        public string? Checksum { get; set; }
        public string? ChecksumAlgorithm { get; set; }
    }

    public class DocumentAttributes
    {
        public string? Name { get; set; }
        public string? Ext { get; set; }
        public int Size { get; set; }
        public string? CreatedBy { get; set; }
        public string? CreatedByGuid { get; set; }
        public DateTime? Created { get; set; }
        public string? ModifiedBy { get; set; }
        public string? ModifiedByGuid { get; set; }
        public DateTime? Modified { get; set; }
    }

    public class DocumentAncestor
    {
        public string? Id { get; set; } // EnvId
        public string? Type { get; set; }
    }

    public class DocumentsResponse
    {

        [JsonPropertyName("Results")]
        public List<Document>? Results { get; set; }

        [JsonPropertyName("TotalFound")]
        public int TotalCount { get; set; }

        [JsonPropertyName("SkipToken")]
        public string? SkipToken { get; set; }
    }

    /// <summary>
    /// Response from v1/document endpoint
    /// </summary>
    public class DocumentUploadResponse
    {
        [JsonPropertyName("envId")]
        public string? EnvId { get; set; }

        [JsonPropertyName("id")]
        public string? DocId { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("extension")]
        public string? Ext { get; set; }

        [JsonPropertyName("size")]
        public int Size { get; set; }

        [JsonPropertyName("url")]
        public string? Url { get; set; }
    }

    public class UpdatableDocumentProfile
    {
        [JsonPropertyName("standardAttributes")]
        public UpdatableStandardAttributes? StandardAttributes { get; set; }
    }


    public class UpdatableStandardAttributes
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }
}
