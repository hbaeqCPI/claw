using System.Text.Json.Serialization;

namespace R10.Web.Models.IManageModels
{
    public class SearchFolder : Folder
    {
        [JsonPropertyName("searchprofile")]
        public SearchProfile? SearchProfile { get; set; }
    }

    public class SearchProfile
    {
        [JsonPropertyName("alias")]
        public string? Alias { get; set; }

        [JsonPropertyName("author")]
        public string? Author { get; set; }

        [JsonPropertyName("body")]
        public string? Body { get; set; }

        [JsonPropertyName("class")]
        public string? Class { get; set; }

        [JsonPropertyName("comment")]
        public string? Comment { get; set; }

        [JsonPropertyName("comments_description_fulltext")]
        public string? CommentsDescriptionFullText { get; set; }

        [JsonPropertyName("comments_fulltext")]
        public string? CommentsFullText { get; set; }

        [JsonPropertyName("container")]
        public int? Container { get; set; }

        [JsonPropertyName("content_type")]
        public string? Content_Type { get; set; }

        [JsonPropertyName("contenttype")]
        public string? ContentType { get; set; }

        [JsonPropertyName("create_date_end")]
        public string? CreateDateEnd { get; set; }

        [JsonPropertyName("create_date_relative")]
        public string? CreateDateRelative { get; set; }

        [JsonPropertyName("create_date_start")]
        public string? CreateDateStart { get; set; }

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
        public string? Custom17 { get; set; }

        [JsonPropertyName("custom18")]
        public string? Custom18 { get; set; }

        [JsonPropertyName("custom19")]
        public string? Custom19 { get; set; }

        [JsonPropertyName("custom20")]
        public string? Custom20 { get; set; }

        [JsonPropertyName("custom21_from")]
        public string? Custom21From { get; set; }

        [JsonPropertyName("custom21_relative")]
        public string? Custom21Relative { get; set; }

        [JsonPropertyName("custom21_to")]
        public string? Custom21To { get; set; }

        [JsonPropertyName("custom22_from")]
        public string? Custom22From { get; set; }

        [JsonPropertyName("custom22_relative")]
        public string? Custom22Relative { get; set; }

        [JsonPropertyName("custom22_to")]
        public string? Custom22To { get; set; }

        [JsonPropertyName("custom23_from")]
        public string? Custom23From { get; set; }

        [JsonPropertyName("custom23_relative")]
        public string? Custom23Relative { get; set; }

        [JsonPropertyName("custom23_to")]
        public string? Custom23To { get; set; }

        [JsonPropertyName("custom24_from")]
        public string? Custom24From { get; set; }

        [JsonPropertyName("custom24_relative")]
        public string? Custom24Relative { get; set; }

        [JsonPropertyName("custom24_to")]
        public string? Custom24To { get; set; }

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

        [JsonPropertyName("databases")]
        public string? Databases { get; set; }

        [JsonPropertyName("description_fulltext")]
        public string? DescriptionFullText { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("docnum")]
        public string? DocNum { get; set; }

        [JsonPropertyName("document_detect_value")]
        public string? DocumentDetectValue { get; set; }

        [JsonPropertyName("edit_date_end")]
        public string? EditDateEnd { get; set; }

        [JsonPropertyName("edit_date_relative")]
        public string? EditDateRelative { get; set; }

        [JsonPropertyName("edit_date_start")]
        public string? EditDateStrart { get; set; }

        [JsonPropertyName("edit_profile_end_date")]
        public string? EditProfileEndDate { get; set; }

        [JsonPropertyName("edit_profile_start_date")]
        public string? EditProfileStartDate { get; set; }

        [JsonPropertyName("fulltext")]
        public string? FullText { get; set; }

        [JsonPropertyName("languageid")]
        public string? LanguageId { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("operator")]
        public string? Operator { get; set; }

        [JsonPropertyName("size")]
        public int? Size { get; set; }

        [JsonPropertyName("subclass")]
        public string? SubClass { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("version")]
        public string? Version { get; set; }

        [JsonPropertyName("checked_out")]
        public bool? CheckedOut { get; set; }

        [JsonPropertyName("in_use")]
        public bool? InUse { get; set; }

        [JsonPropertyName("documents_only")]
        public bool? DocumentsOnly { get; set; }

        [JsonPropertyName("emails_only")]
        public bool? EmailsOnly { get; set; }

        [JsonPropertyName("share_active")]
        public bool? ShareActive { get; set; }

        [JsonPropertyName("share_end_date")]
        public string? ShareEndDate { get; set; }

        [JsonPropertyName("share_end_date_from")]
        public string? ShareEndDateFrom { get; set; }

        [JsonPropertyName("share_end_date_to")]
        public string? ShareEndDateTo { get; set; }
    }

    public class SearchFolderResponse
    {
        [JsonPropertyName("data")]
        public SearchFolder? Data { get; set; }
    }
}
