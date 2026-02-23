using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace R10.Core.Entities.GeneralMatter
{
    public class GMActionType : GMActionTypeDetail
    {
        public List<GMActionParameter>? ActionParameters { get; set; }
        public Attorney? Responsible { get; set; }
        public GMCountry? GMCountry { get; set; }

        [NotMapped]
        public string? CopyOptions { get; set; }
    }

    public class GMActionTypeDetail : BaseEntity
    {
        [Key]
        public int ActionTypeID { get; set; }

        [Required]
        [StringLength(60)]
        [Display(Name = "Action Type")]
        public string? ActionType { get; set; }

        [Display(Name = "Country")]
        public string? Country { get; set; }

        [StringLength(60)]
        [Display(Name = "Follow Up Action")]
        public string? FollowUpMsg { get; set; }

        [Required]
        [Display(Name = "Month")]
        public int FollowUpMonth { get; set; }

        [Required]
        [Display(Name = "Day")]
        public int FollowUpDay { get; set; }

        [Display(Name = "Indicator")]
        public string? FollowUpIndicator { get; set; }

        [Display(Name = "Follow up Based On")]
        public short FollowUpGen { get; set; }

        [Display(Name = "Responsible Attorney")]
        public int? ResponsibleID { get; set; }

        public string? Remarks { get; set; }
    }
}
