using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Core.Entities
{
    public class SearchCriteria : BaseEntity
    {
        [Key]
        public int CriteriaId { get; set; }
        public string?  ScreenName { get; set; }
        public string?  LoginName { get; set; }
        public List<SearchCriteriaDetail>? CriteriaDetails { get; set; }

        [NotMapped]
        public string?  CriteriaData { get; set; }
    }


    public class SearchCriteriaDetail : BaseEntity
    {
        [Key]
        public int CritDtlId { get; set; }

        public int CriteriaId { get; set; }

        [Required]
        [StringLength(50)]
        public string?  CriteriaName { get; set; }

        public string?  CriteriaData { get; set; }
        public bool? IsDefault { get; set; }
        public bool? HasMonitoring { get; set; }

        public SearchCriteria? SearchCriteria { get; set; }

        [NotMapped]
        public string?  ScreenName { get; set; }

        [NotMapped]
        public string?  LoadType { get; set; }
        [NotMapped]
        public string?  OldCriteriaName { get; set; }
        [NotMapped]
        public string?  Email { get; set; }
        [NotMapped]
        public int QESetupId { get; set; }
    }
}
