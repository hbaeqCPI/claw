using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Patent.ViewModels.Report
{
    public class PatIDSCrossCheckDataViewModel
    {
        public int BaseAppId { get; set; }
        public string BaseCaseNumber { get; set; }
        public string BaseCountry { get; set; }
        public string BaseSubCase { get; set; }
        public string BaseCaseType { get; set; }
        [Key]
        public int RowID { get; set; }
        public int AppID { get; set; }
        public int RelatedCasesId { get; set; }
        public string CaseNumber { get; set; }
        public string Country { get; set; }
        public string SubCase { get; set; }
        public string RelCountries { get; set; }
        public string CaseType { get; set; }
        public string RelPubNo { get; set; }
        public DateTime? RelPubDate { get; set; }
        public string RelPatNo { get; set; }
        public DateTime?  RelIssDate { get; set; }
        public bool CitedInMaster { get; set; }
        public bool CitedInCompare { get; set; }
        public DateTime? RelatedDateFiledInMaster { get; set; }
        public DateTime? RelatedDateFiledInCompare { get; set; }
        public bool ActiveSwitchInMaster { get; set; }
        public bool ActiveSwitchInCompare { get; set; }
    }
}
