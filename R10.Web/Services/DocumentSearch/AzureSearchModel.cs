using Azure.Search.Documents.Models;
using R10.Web.Services.DocumentStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace R10.Web.Services.DocumentSearch
{
    public class SearchOutput
    {
        [JsonPropertyName("count")]
        public long? Count { get; set; }
        [JsonPropertyName("results")]
        public List<SearchResult<SearchDocument>> Results { get; set; }
        [JsonPropertyName("facets")]
        public Dictionary<String, IList<FacetValue>> Facets { get; set; }
    }

    public class FacetValue
    {
        public string value { get; set; }
        public long? count { get; set; }
    }

    //public class DocumentSearchResult : DocumentStorageHeader
    //{
    //    public long DocumentSize { get; set; }
    //    public string DocumentPath { get; set; }
    //    public float SearchScore { get; set; }
    //}
}
