using R10.Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.Patent
{
    public class PatCaseType : BaseEntity
    {
        public int CaseTypeId { get; set; }

        [Key]
        [Required]
        [StringLength(3)]
        [Display(Name = "Case Type")]
        public string CaseType { get; set; }

        [StringLength(255)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        public bool? LockPatRecord { get; set; }

        public List<PatCountryLaw>? CaseTypeCountryLaws { get; set; }

        public List<PatDesCaseType>? ParentPatDesCaseTypes { get; set; }
        public List<PatDesCaseType>? ChildPatDesCaseTypes { get; set; }
    }
}
