using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace R10.Web.Models.IManageModels
{
    public class Folder : Container
    {

        [JsonPropertyName("parent_id")]
        public string? ParentId { get; set; }

        [JsonPropertyName("has_documents")]
        public bool? HasDocuments { get; set; }

        [JsonPropertyName("workspace_id")]
        public string? WorkspaceId { get; set; }

        [JsonPropertyName("workspace_name")]
        public string? WorkspaceName { get; set; }

        [JsonPropertyName("folder_type")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public FolderType? FolderType { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("profile")]
        public FolderProfile? Profile {  get; set; }

        public List<Folder>? SubFolders { get; set; }
    }

    public class FolderResponse
    {
        [JsonPropertyName("data")]
        public Folder? Data { get; set; }
    }

    public class FoldersResponse
    {
        [JsonPropertyName("data")]
        public List<Folder>? Data { get; set; }
    }

    public class ParentFoldersResponse
    {
        [JsonPropertyName("data")]
        public List<ParentFolder>? Data { get; set; }
    }

    public class ParentFolder
    {
        [JsonPropertyName("document")]
        public int DocumentNumber { get; set; }

        [JsonPropertyName("folder")]
        public int FolderNumber { get; set; }

        [JsonPropertyName("create_date")]
        public DateTime? CreateDate { get; set; }
    }

    public class ParentFolderViewModel
    {
        public string? DocumentId { get; set; }
        public string? FolderId { get; set; }
    }

    public class FolderPathsResponse
    {
        [JsonPropertyName("data")]
        public List<List<FolderPath>>? Data { get; set; }
    }

    public class FolderPath
    {
        [JsonPropertyName("create_date")]
        public DateTime? CreateDate { get; set; }

        [JsonPropertyName("database")]
        public string? Database { get; set; }

        [JsonPropertyName("default_security")]
        public string? DefaultSecurity { get; set; }

        [JsonPropertyName("edit_date")]
        public DateTime? EditDate { get; set; }

        [JsonPropertyName("has_subfolders")]
        public bool? HasSubfolders { get; set; }

        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("is_declared")]
        public bool? IsDeclared { get; set; }

        [JsonPropertyName("is_external")]
        public bool? IsExternal { get; set; }

        [JsonPropertyName("is_external_as_normal")]
        public bool? IsExternalAsNormal { get; set; }

        [JsonPropertyName("last_user")]
        public string? LastUser { get; set; }

        [JsonPropertyName("last_user_description")]
        public string? LastUserDescription { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("subtype")]
        public string? Subtype { get; set; }

        [JsonPropertyName("create_profile_date")]
        public DateTime? CreateProfileDate { get; set; }

        [JsonPropertyName("document_number")]
        public int? DocumentNumber { get; set; }

        [JsonPropertyName("is_hipaa")]
        public bool? IsHipaa { get; set; }

        [JsonPropertyName("iwl")]
        public string? Iwl { get; set; }

        [JsonPropertyName("retain_days")]
        public int? RetainDays { get; set; }

        [JsonPropertyName("system_edit_date")]
        public DateTime? SystemEditDate { get; set; }

        [JsonPropertyName("version")]
        public int? Version { get; set; }

        [JsonPropertyName("wstype")]
        public string? Wstype { get; set; }

        [JsonPropertyName("folder_type")]
        public string? FolderType { get; set; }

        [JsonPropertyName("has_documents")]
        public bool? HasDocuments { get; set; }

        [JsonPropertyName("inherited_default_security")]
        public string? InheritedDefaultSecurity { get; set; }

        [JsonPropertyName("is_container_saved_search")]
        public bool? IsContainerSavedSearch { get; set; }

        [JsonPropertyName("is_content_saved_search")]
        public bool? IsContentSavedSearch { get; set; }

        [JsonPropertyName("owner_description")]
        public string? OwnerDescription { get; set; }

        [JsonPropertyName("owner")]
        public string? Owner { get; set; }

        [JsonPropertyName("parent_id")]
        public string? ParentId { get; set; }

        [JsonPropertyName("view_type")]
        public string? ViewType { get; set; }

        [JsonPropertyName("workspace_name")]
        public string? WorkspaceName { get; set; }

        [JsonPropertyName("workspace_id")]
        public string? WorkspaceId { get; set; }
    }

    public class FolderProfile
    {
        [JsonPropertyName("author")]
        public string? Author { get; set; }

        [JsonPropertyName("class")]
        public string? ClassName { get; set; }

        [JsonPropertyName("subclass")]
        public string? SubClass { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("custom1")]
        public string? Custom1 { get; set; }

        [JsonPropertyName("custom2")]
        public string? Custom2 { get; set; }

        [JsonPropertyName("custom3")]
        public string? Custom3 { get; set; }

        [JsonPropertyName("custom4")]
        public string? Custom4 { get; set; }

        [JsonPropertyName("custom5")]
        public string? Custom5 { get; set; }

        [JsonPropertyName("custom6")]
        public string? Custom6 { get; set; }

        [JsonPropertyName("custom7")]
        public string? Custom7 { get; set; }

        [JsonPropertyName("custom8")]
        public string? Custom8 { get; set; }

        [JsonPropertyName("custom9")]
        public string? Custom9 { get; set; }

        [JsonPropertyName("custom10")]
        public string? Custom10 { get; set; }

        [JsonPropertyName("custom11")]
        public string? Custom11 { get; set; }

        [JsonPropertyName("custom12")]
        public string? Custom12 { get; set; }

        [JsonPropertyName("custom13")]
        public string? Custom13 { get; set; }

        [JsonPropertyName("custom14")]
        public string? Custom14 { get; set; }

        [JsonPropertyName("custom15")]
        public string? Custom15 { get; set; }

        [JsonPropertyName("custom16")]
        public string? Custom16 { get; set; }

        [JsonPropertyName("custom17")]
        public double? Custom17 { get; set; }

        [JsonPropertyName("custom18")]
        public double? Custom18 { get; set; }

        [JsonPropertyName("custom19")]
        public double? Custom19 { get; set; }

        [JsonPropertyName("custom20")]
        public double? Custom20 { get; set; }

        [JsonPropertyName("custom21")]
        public string? Custom21 { get; set; }

        [JsonPropertyName("custom22")]
        public string? Custom22 { get; set; }

        [JsonPropertyName("custom23")]
        public string? Custom23 { get; set; }

        [JsonPropertyName("custom24")]
        public string? Custom24 { get; set; }

        [JsonPropertyName("custom25")]
        public bool? Custom25 { get; set; }

        [JsonPropertyName("custom26")]
        public bool? Custom26 { get; set; }

        [JsonPropertyName("custom27")]
        public bool? Custom27 { get; set; }

        [JsonPropertyName("custom28")]
        public bool? Custom28 { get; set; }

        [JsonPropertyName("custom29")]
        public string? Custom29 { get; set; }

        [JsonPropertyName("custom30")]
        public string? Custom30 { get; set; }

        [JsonPropertyName("custom31")]
        public string? Custom31 { get; set; }
    }

    public enum FolderType
    {
        Undefined,
        [EnumMember(Value = "regular")]
        Regular,
        [EnumMember(Value = "search")]
        Search,
        [EnumMember(Value = "tab")]
        Tab,
        [EnumMember(Value = "category")]
        Category,
        [EnumMember(Value = "my_matters")]
        MyMatters,
        [EnumMember(Value = "my_favorites")]
        MyFavorites
    }
}
