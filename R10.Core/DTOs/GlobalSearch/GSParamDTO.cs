using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace R10.Core.DTOs
{
    public class GSParamDTO
    {
        public string? SearchMode { get; set; }                                  // s = basic, a = advanced

        // simple search criteria
        public string? SystemScreens { get; set; }
        public string? DocumentTypes { get; set; }
        public string? BasicSearchTerm { get; set; }                             // for basic searching
        public string? DocSearchMode { get; set; }                               // Azure search mode: any, all
        public string? DocQueryType { get; set; }                                // Azure search query type: simple, full
        
        // common criteria
        public IEnumerable<GSMoreFilter> MoreFilters { get; set; }

        // advanced search criteria
        public IEnumerable<GSDataFilterBase> DataFilters { get; set; }
        public IEnumerable<GSDocFilterBase> DocFilters { get; set; }


    }

    public class GSMoreFilter
    {
        public string? FieldName { get; set; }
        public string? FieldValue { get; set; }
    }

    public class GSDataFilterBase
    {
        public int FieldId { get; set; }
        [Display(Name = "Search Term")]
        public string? Criteria { get; set; }
        public int OrderEntry { get; set; } = 1;
        public string? LogicalOperator { get; set; }

        [Display(Name = "(")]
        public string? LeftParen { get; set; } = "";          // left parenthesis for grouping
        [Display(Name = ")")]
        public string? RightParen { get; set; } = "";          // right parenthesis for grouping
    }

    public class GSDocFilterBase
    {
        public int FieldId { get; set; }

        [Display(Name = "Search Term")]
        public string? Criteria { get; set; }

        [Display(Name = "Word Match")]
        public string? DocSearchMode { get; set; }

        [Display(Name = "Query Type")]
        public string? DocQueryType { get; set; }
    }

    public class AzureSearchDocList {
        public int FieldId { get; set; }
        public string? SystemScreens { get; set; }
        public string? DocumentTypes { get; set; }
        public string? Criteria { get; set; }
        public string? DocSearchMode { get; set; }
        public string? DocQueryType { get; set; }
    }

    public class GSDocParamDTO 
    {
        public int RecordId { get; set; }
        public string? SystemType { get; set; }
        public string? ScreenCode { get; set; }
        public int ParentId { get; set; }
        public string? DocumentType { get; set; }
        public int LogId { get; set; }                      // originally LetLogId
        //public long FileSize { get; set; }                // not available from blob document metadata
        public string? FilePath { get; set; }
        public string? FileName { get; set; }
        public decimal SearchScore { get; set; }
    }

    public class GSDownloadParamDTO
    {
        public int RecordId { get; set; }
        public string? SystemType { get; set; }
        public string? ScreenCode { get; set; }
        public string? DocumentType { get; set; }
        public int ParentId { get; set; }
        public int LogId { get; set; }
        public string? DocFileName { get; set; }
        public string? UserFileName { get; set; }
    }
}
