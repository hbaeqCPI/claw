using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace R10.Core.Entities.Patent
{
    public class PatIDSRelatedCasesInfo : BaseEntity
    {
        [Key]
        public int RelatedCasesId { get; set; }
        public int AppId { get; set; }

        [StringLength(10)]
        [Display(Name = "Group Art Unit")]
        public string? GroupArtUnit { get; set; }
        [StringLength(50)]
        [Display(Name = "Examiner")]
        public string? Examiner { get; set; }
        [StringLength(25)]
        [Display(Name = "Attorney Docket No")]
        public string? AttorneyDocketNo { get; set; }
        [StringLength(50)]
        [Display(Name = "Customer No")]
        public string? CustomerNo { get; set; }
        public bool IncludeInPatCenterDownload { get; set; }


        public DateTime? FirstTierPaidDate { get; set; }
        public decimal FirstTierPaidAmount { get; set; }
        public DateTime? SecondTierPaidDate { get; set; }
        public decimal SecondTierPaidAmount { get; set; }
        public DateTime? ThirdTierPaidDate { get; set; }
        public decimal ThirdTierPaidAmount { get; set; }

        public CountryApplication? CountryApplication { get; set; }
    }
}



