using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.ForeignFiling
{
    public class FFDue : FFDueDetail
    {
        public PatDueDate PatDueDate { get; set; }
        public FFInstrxType ClientInstrxType { get; set; }
        public FFInstrxChangeLog InstrxChangeLog { get; set; }
        public List<FFDueCountry> FFDueCountries { get; set; }
    }

    public class FFDueDetail : BaseEntity
    {
        [Key]
        public int DueId { get; set; }

        [Required]
        public int DDId { get; set; }

        public DateTime? ClientLastReminderDate { get; set; }

        [Display(Name = "Instruction Date")]
        public DateTime? ClientInstructionDate { get; set; }

        [StringLength(5)]
        public string? ClientInstructionType { get; set; }

        public int? ClientInstructionLogId { get; set; }

        [StringLength(1)]
        public string? ClientInstructionSource { get; set; }

        [StringLength(1000)]
        [Display(Name = "Remarks")]
        public string? ClientInstrxRemarks { get; set; }

        public bool? IsActionClosed { get; set; }

        public DateTime? CloseDate { get; set; }

        [Display(Name = "Agent Responsibility Sent")]
        public DateTime? ClientInstructionSentToAssoc { get; set; }

        [Display(Name = "Payment Date")]
        public DateTime? ClientPaymentDate { get; set; }

        [Display(Name = "Payment Sent")]
        public DateTime? ClientPaymentSentToAssoc { get; set; }

        [Display(Name = "Payment Letter Sent")]
        public DateTime? ClientPaymentLetterSentDate { get; set; }

        [Display(Name = "Receipt Letter Sent")]
        public DateTime? ClientReceiptLetterSentDate { get; set; }

        public bool? IgnoreRecord { get; set; }
        public bool? Exclude { get; set; } //do not process
    }
}
