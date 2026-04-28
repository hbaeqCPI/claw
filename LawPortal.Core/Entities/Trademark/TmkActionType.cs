using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LawPortal.Core.Entities.Trademark
{
    public class TmkActionType : BaseEntity
    {
        [NotMapped]
        public string? CopyOptions { get; set; }

        [Key]
        public int ActionTypeID { get; set; }

        [Required]
        [StringLength(60)]
        [Display(Name = "Action Type")]
        public string? ActionType { get; set; }

        [Display(Name = "Country")]
        public string? Country { get; set; }

        public int? CDueId { get; set; }

        [StringLength(60)]
        [Display(Name = "Follow Up Action")]
        public string? FollowUpMsg { get; set; }

        [Display(Name = "Follow up Terms Month")]
        public int FollowUpMonth { get; set; }
        [Display(Name = "Day")]
        public int FollowUpDay { get; set; }

        [Display(Name = "Indicator")]
        public string? FollowUpIndicator { get; set; }

        [Display(Name = "Follow up Based On")]
        public short FollowUpGen { get; set; }

        [Display(Name = "Responsible")]
        public int? ResponsibleID { get; set; }

        public string? Remarks { get; set; }

        [Display(Name = "Office Action?")]
        public bool IsOfficeAction { get; set; }
    }
}
