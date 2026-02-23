using R10.Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace R10.Core.Entities.Patent
{
    public class RTSSearch
    {
        [Key]
        public int PLAppId { get; set; }
        public int PMSAppId { get; set; }

        public string? PMSCaseNumber { get; set; }
        public string? PMSCountry { get; set; }
        public string? PMSSubCase { get; set; }
        public string? PMSCaseType { get; set; }
        public string? PMSAppNo { get; set; }
        public string? PMSPubNo { get; set; }
        public string? PMSPatNo { get; set; }
        public DateTime? PMSFilDate { get; set; }
        public DateTime? PMSPubDate { get; set; }
        public DateTime? PMSIssDate { get; set; }
        
        //public string? PMSStdAppNo { get; set; }
        //public string? PMSStdPubNo { get; set; }
        //public string? PMSStdPatNo { get; set; }
        //public string? PMSTempAppNo { get; set; }
        //public string? PMSTempPubNo { get; set; }
        //public string? PMSTempPatNo { get; set; }
        //public string? PMSYearAppNo { get; set; }
        //public string? PMSYearPubNo { get; set; }
        //public string? PMSYearPatNo { get; set; }
        //public string? PMSChkDgtAppNo { get; set; }
        //public string? PMSChkDgtPubNo { get; set; }
        //public string? PMSChkDgtPatNo { get; set; }
        //public string? PMSCityAppNo { get; set; }
        //public string? PMSCityPubNo { get; set; }
        //public string? PMSCityPatNo { get; set; }
        public string? PLAppNo { get; set; }
        public string? PLPubNo { get; set; }
        public string? PLPatNo { get; set; }
        public DateTime? PLFilDate { get; set; }
        public DateTime? PLPubDate { get; set; }
        public DateTime? PLIssDate { get; set; }

        //public string? PLStdAppNo { get; set; }
        //public string? PLStdPubNo { get; set; }
        //public string? PLStdPatNo { get; set; }
        //public string? PLTempAppNo { get; set; }
        //public string? PLTempPubNo { get; set; }
        //public string? PLTempPatNo { get; set; }
        //public string? PLYearAppNo { get; set; }
        //public string? PLYearPubNo { get; set; }
        //public string? PLYearPatNo { get; set; }
        //public string? PLCityAppNo { get; set; }
        //public string? PLCityPubNo { get; set; }
        //public string? PLCityPatNo { get; set; }
        //public string? MarkAppNo { get; set; }
        //public string? MarkPubNo { get; set; }
        //public string? MarkPatNo { get; set; }
        //public string? MarkFilDate { get; set; }
        //public string? MarkPubDate { get; set; }
        //public string? MarkRegDate { get; set; }
        //public string? MarkAllowanceDate { get; set; }
        //public DateTime? LastWebCheckStart { get; set; }
        //public DateTime? LastWebCheckDate { get; set; }

        //public DateTime? LastNumFmtDate { get; set; }

        public DateTime? LastWebUpdate { get; set; }
        public List<RTSSearchAction> RTSSearchActions { get; set; }
        public List<RTSSearchUSIFW> RTSSearchUSIFWs { get; set; }

        [ForeignKey("PMSAppId")]
        public CountryApplication CountryApplication { get; set; }
    }

}
