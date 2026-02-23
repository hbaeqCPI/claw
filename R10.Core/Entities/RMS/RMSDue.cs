using R10.Core.Entities.Trademark;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.RMS
{
    public class RMSDue : RMSDueDetail
    {
        public TmkDueDate TmkDueDate { get; set; }
        public RMSInstrxType ClientInstrxType { get; set; }
        public RMSInstrxChangeLog RMSInstrxChangeLog { get; set; }

        public List<RMSDueCountry>? RMSDueCountries { get; set; }
    }

    public class RMSDueDetail : BaseEntity
    {
        [Key]
        public int DueId { get; set; }
        [Required]
        public int DDId { get; set; }

        public DateTime? InvoiceDate { get; set; }
        [StringLength(50)]
        public string? InvoiceNumber { get; set; }
        public decimal? InvoiceAmount { get; set; }
        public decimal? InvoicePaidAmount { get; set; }
        public DateTime? InvoicePaidDate { get; set; }
        public DateTime? InvoiceCancelDate { get; set; }

        public DateTime? ClientLastReminderDate { get; set; }

        [Display(Name = "Instruction Date")]
        public DateTime? ClientInstructionDate { get; set; }

        [StringLength(5)]
        public string? ClientInstructionType { get; set; }

        public int? ClientInstructionLogId { get; set; }

        [StringLength(1)]
        public string? ClientInstructionSource { get; set; }

        public bool? IsActionClosed { get; set; } //process flag for action closing
        public DateTime? CloseDate { get; set; } //process date for action closing

        [Display(Name = "Agent Responsibility Sent")]
        public DateTime? ClientInstructionSentToAssoc { get; set; } //agent letter sent date

        [Display(Name = "Agent Payment Date")]                      //agent paid date
        public DateTime? AgentPaymentDate { get; set; }

        [Display(Name = "Payment Date")]
        public DateTime? ClientPaymentDate { get; set; }

        [Display(Name = "Payment Sent")]
        public DateTime? ClientPaymentSentToAssoc { get; set; }

        [Display(Name = "Payment Letter Sent")]
        public DateTime? ClientPaymentLetterSentDate { get; set; }

        [Display(Name = "Receipt Letter Sent")]
        public DateTime? ClientReceiptLetterSentDate { get; set; }
        public DateTime? NextRenewalDate { get; set; } //manually entered next renewal date in action closing

        public bool? IgnoreRecord { get; set; } //mark deleted flag (not used)
        public bool? Exclude { get; set; } //do not process

        [StringLength(1000)]
        [Display(Name = "Remarks")]
        public string? ClientInstrxRemarks { get; set; }
    }
}
