using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Trademark.ViewModels
{
    public class TmkTrademarkCopyViewModel : TmkTrademarkCopyEntity
    {
        public List<TmkTrademarkCopyCountry> CountryList;
        public List<TmkTrademarkCopyRange> CountryRangeList;
    }

    public class TmkTrademarkCopyParameters : TmkTrademarkCopyEntity
    {
        public string? CopiedCountries { get; set; }

    }

    public class TmkTrademarkCopyEntity
    {
        public int CopyTmkId { get; set; }

        [Required]
        public string? CopyCaseNumber { get; set; }

        [Display(Name = "Sub Case")]
        public string? CopySubCase { get; set; }

        [Display(Name = "Case Info")]
        public bool CopyCaseInfo { get; set; } = true;

        [Display(Name = "Remarks")]
        public bool CopyRemarks { get; set; } = true;

        [Display(Name = "Assignments")]
        public bool CopyAssignments { get; set; } = true;

        [Display(Name = "Goods")]
        public bool CopyGoods { get; set; } = true;

        [Display(Name = "Documents")]
        public bool CopyImages { get; set; } = true;

        public bool CopyOwners { get; set; } = true;

        [Display(Name = "Keywords")]
        public bool CopyKeywords { get; set; } = true;

        [Display(Name = "Designated Countries")]
        public bool CopyDesCountries { get; set; } = true;

        [Display(Name = "Products")]
        public bool CopyProducts { get; set; } = true;

        [Display(Name = "Licenses")]
        public bool CopyLicenses { get; set; } = true;

        [Display(Name = "Related Cases")]
        public bool CopyRelatedCases { get; set; } = true;

        [Display(Name = "Relationship")]
        public string? CopyRelationship { get; set; }
        
    }

    public class TmkTrademarkCopyCountry
    {
        public string? Country { get; set; }
        public string? CountryName { get; set; }
    }

    public class TmkTrademarkCopyRange
    {
        public string? RangeLabel { get; set; }
        public string? CountryStart { get; set; }
        public string? CountryEnd { get; set; }
    }

    public class TmkTrademarkCopyResult
    {
        public string? AddedRecords { get; set; }
        public string? ExistingRecords { get; set; }
        public string? NoCLRecords { get; set; }
    }

}
