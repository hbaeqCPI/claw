using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Trademark
{
    public class TmkConflict : TmkConflictDetail
    {
        public TmkConflictStatus? TmkConflictStatus { get; set; }
        public TmkCountry? TmkCountry { get; set; }
    }

    public class TmkConflictDetail : BaseEntity
    {
        [Key]
        public int ConflictId { get; set; }
        public int TmkId { get; set; }

        [Required, StringLength(25)]
        public string? CaseNumber { get; set; }

        [Required, StringLength(5)]
        public string? Country { get; set; }

        [StringLength(8)]
        [Display(Name = "Sub Case")]
        public string? SubCase { get; set; }

        [Required, StringLength(20)]
        [Display(Name = "Conflict Status")]
        public string? ConflictStatus { get; set; }

        [Display(Name = "Status Date")]
        public DateTime? StatusDate { get; set; }

        [Required, StringLength(20)]
        [Display(Name = "Conflict/Opposition No.")]
        public string? ConflictOppNumber { get; set; }

        [StringLength(15)]
        [Display(Name = "Direction")]
        public string? Direction { get; set; }

        [StringLength(50)]
        [Display(Name = "Other Party")]
        public string? OtherParty { get; set; }

        [StringLength(50)]
        [Display(Name = "Other Party Mark")]
        public string? OtherPartyMark { get; set; }

        [StringLength(20)]
        [Display(Name = "Other Party Application No.")]
        public string? OPAppNumber { get; set; }

        [StringLength(20)]
        [Display(Name = "Other Party Publication No.")]
        public string? OPPubNumber { get; set; }

        [StringLength(20)]
        [Display(Name = "Other Party Registration No.")]
        public string? OPRegNumber { get; set; }

        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }

        public int? AgentId { get; set; }
    }
}
