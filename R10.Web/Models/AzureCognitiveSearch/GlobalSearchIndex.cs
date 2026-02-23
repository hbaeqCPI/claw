using System;
using System.Collections.Generic;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.Spatial;
using Newtonsoft.Json;

namespace R10.Web.Models.GlobalSearch

{
    public class GlobalSearchIndex
    {
        [IsSearchable, IsRetrievable(false), Analyzer(AnalyzerName.AsString.StandardLucene) ]
        public string content { get; set; }

        [IsFilterable]
        public string SystemType { get; set; }

        [IsFilterable]
        public string ScreenCode { get; set; }

        [IsFilterable]
        public string ParentId { get; set; }

        [IsFilterable]
        public string DocumentType { get; set; }

        public string LogId { get; set; }

        [IsRetrievable(false)]
        public string metadata_storage_content_type { get; set; }

        [IsRetrievable(false)]
        public Int64 metadata_storage_size { get; set; }

        [IsRetrievable(false)]
        public DateTimeOffset metadata_storage_last_modified { get; set; }

        [IsRetrievable(false)]
        public string metadata_storage_content_md5 { get; set; }

        public string metadata_storage_name { get; set; }

        [System.ComponentModel.DataAnnotations.Key]
        public string metadata_storage_path { get; set; }

        [IsRetrievable(false)]
        public string metadata_storage_file_extension { get; set; }

        [IsRetrievable(false)]
        public string metadata_content_type { get; set; }

        [IsSearchable, IsRetrievable(false), Analyzer(AnalyzerName.AsString.StandardLucene)]
        public string merged_content { get; set; }

        [IsSearchable, IsRetrievable(false), Analyzer(AnalyzerName.AsString.StandardLucene)]
        public ICollection<string> text { get; set; }
        
        [IsSearchable, IsRetrievable(false), Analyzer(AnalyzerName.AsString.StandardLucene)]
        public ICollection<string> layoutText { get; set; }


        

    }
}
