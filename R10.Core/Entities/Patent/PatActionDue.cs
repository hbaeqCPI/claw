using R10.Core.Entities.Documents;
using R10.Core.Entities.ForeignFiling;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace R10.Core.Entities.Patent
{

    public class PatActionDue : PatActionDueDetail
    {
        public CountryApplication? CountryApplication { get; set; }
        
        public Attorney? Responsible { get; set; }
        public PatCountry? PatCountry { get; set; }

        public List<PatDueDate>? DueDates { get; set; }
        //public List<PatImageAct>? Images { get; set; }

        [NotMapped]
        public string? FollowUpAction { get; set; }

        [NotMapped]
        public List<DocFolder>? DocFolders { get; set; }

        public FFInstrxTypeAction? FFInstrxTypeAction { get; set; }
        public List<PatDueDateDelegation>? Delegations { get; set; }
        
        [NotMapped]
        public bool CloseDueDates { get; set; }
    }

    public class PatActionDueDetail : BaseEntity
    {
        [Key]
        public int ActId { get; set; }
        public int AppId { get; set; }

        [Required]
        [StringLength(25)]
        public string CaseNumber { get; set; }

        [Required]
        [StringLength(5)]
        public string Country { get; set; }

        [StringLength(8)]
        public string? SubCase { get; set; }

        [Required, Display(Name = "Action Type")]
        [StringLength(60)]
        public string? ActionType { get; set; }

        [Required, Display(Name = "Base Date")]
        public DateTime BaseDate { get; set; }

        [Display(Name = "Response Date")]
        public DateTime? ResponseDate { get; set; }

        [Display(Name = "Action Received Date")]
        public DateTime? VerifyDate { get; set; }

        public bool ComputerGenerated { get; set; }

        public bool? IsElectronic { get; set; }

        public int? ResponsibleID { get; set; }

        public string? Remarks { get; set; }

        [Display(Name = "Office Action?")]
        public bool IsOfficeAction { get; set; }
        public string? AutoDocketWorkflowStatus { get; set; }


        /// <summary>
        /// Verification-Action Verified By
        /// </summary>
        [Display(Name = "Action Verified By")]
        public string? VerifiedBy { get; set; }
        /// <summary>
        /// Verification-Action Verified Date
        /// </summary>
        [Display(Name = "Action Verified Date")]
        public DateTime? DateVerified { get; set; }
        /// <summary>
        /// Verification-Action Verifier Id
        /// </summary>
        [StringLength(450)]
        public string? VerifierId { get; set; }
        /// <summary>
        /// Verification-CheckDocket
        /// </summary>
        [Display(Name = "Check Docket?")]
        public bool CheckDocket { get; set; }
    }
}
