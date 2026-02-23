using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace R10.Core.DTOs
{
    public class CountryApplicationDTO : CountryApplicationDetail
    {
        [Display(Name = "Country Name")]
        public string? CountryName { get; set; }
        public string? AgentCode { get; set; }
        public string? AgentName { get; set; }
        public string? ClientName { get; set; }
        public string? OwnerCode { get; set; }
        public string? OwnerName { get; set; }
        public int? Attorney1ID { get; set; }
        public int? Attorney2ID { get; set; }
        public int? Attorney3ID { get; set; }
        public string? Attorney1 { get; set; }
        public string? Attorney2 { get; set; }
        public string? Attorney3 { get; set; }
        public string? Attorney1Name { get; set; }
        public string? Attorney2Name { get; set; }
        public string? Attorney3Name { get; set; }
        public string? ImageFile { get; set; }
        public string? PriorityCountry { get; set; }
        public string? PriorityNumber { get; set; }
        public DateTime? PriorityDate { get; set; }
        public string? ParentCase { get; set; }

        [NotMapped]
        public string? CopyOptions { get; set; }

        #region IDS
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
        public int? RelatedCasesId { get; set; }
        #endregion

        [NotMapped]
        public new CountryApplicationTradeSecret? TradeSecret { get; set; }
    }

}


