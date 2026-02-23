using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.AMS
{
    public class AMSDue : AMSDueDetail
    {
        public AMSMain AMSMain { get; set; }
        public AMSProjection AMSProjection { get; set; }

        public AMSInstrxType ClientInstrxType { get; set; }

        public AMSInstrxType CPIInstrxType { get; set; }

        public List<AMSRemLogDue> AMSRemLogDues { get; set; }

        public List<AMSInstrxChangeLog> AMSInstrxChangeLogs { get; set; }

        public List<AMSInstrxCPiLogDetail> AMSInstrxCPiLogDetails { get; set; }

        public List<AMSStatusChangeLog> AMSStatusChangeLog { get; set; }

        public List<AMSInstrxDecisionMgt> AMSInstrxDecisionMgt { get; set; }

        public AMSCostExportLog? AMSCostExportLog { get; set; }
    }

    public class AMSDueDetail : BaseEntity
    {
        [Key]
        public int DueID { get; set; }

        [Required]
        public int AnnID { get; set; }

        [Required]
        [StringLength(10)]
        public string PaymentType { get; set; }

        [Required]
        [StringLength(10)]
        [Display(Name = "Year")]
        public string AnnuityYear { get; set; }

        [StringLength(5)]
        [Display(Name = "No")]
        public string? AnnuityNoDue { get; set; }

        [Display(Name = "Due Date")]
        public DateTime? AnnuityDueDate { get; set; }

        [StringLength(5)]
        public string? PaidThru  { get; set; }

        [Display(Name = "Cost to Expire")]
        public decimal CostToExpiration { get; set; }

        [Display(Name = "Instruction Date")]
        public DateTime? CPIInstructionDate  { get; set; }

        [StringLength(10)]
        [Display(Name = "Instruction")]
        public string? CPIInstructionType  { get; set; }

        [Display(Name = "Payment Date")]
        public DateTime? CPIPaymentDate { get; set; }

        [Display(Name = "Invoice Date")]
        public DateTime? CPIInvoiceDate { get; set; }

        [StringLength(20)]
        [Display(Name = "Invoice No")]
        public string? CPIInvoiceNo { get; set; }

        public decimal CPIInvoiceAmount { get; set; }

        public decimal CPIInvoicePaidAmount { get; set; }

        [Display(Name = "Invoice Paid")]
        public DateTime? CPIInvoicePaidDate { get; set; }

        public DateTime? CPIInvoiceCancelDate { get; set; }

        [Display(Name = "Settlement Date")]
        public DateTime? CPISettleDate { get; set; }

        [Display(Name = "Settlement Amount")]
        public decimal CPISettleAmount { get; set; }

        [Display(Name = "Settlement No")]
        [StringLength(20)] 
        public string? CPISettleNo { get; set; }

        [Display(Name = "Settlement Paid Date")]
        public DateTime? CPISettlePaidDate { get; set; }

        [Display(Name = "Receipt Date")]
        public DateTime? CPIReceiptPostDate { get; set; }

        public decimal CPIReviewAmount { get; set; }

        public DateTime? CPIReviewDate { get; set; }

        public DateTime? CPINonPaymentDate { get; set; }

        public decimal CPIClientFee { get; set; }

        public decimal CPIReminderFaxAmount { get; set; }

        public DateTime? CPIGraceDate { get; set; }

        public DateTime? ClientLastReminderDate { get; set; }

        [StringLength(5)] 
        public string? ClientLastReminderSequence { get; set; }

        [Display(Name = "Instruction Date")]
        public DateTime? ClientInstructionDate { get; set; }

        [StringLength(5)]
        public string? ClientInstructionType { get; set; }

        [StringLength(20)] 
        public string? ClientInstructionBy { get; set; }

        public int ClientInstructionLogId  { get; set; }

        [StringLength(1)]
        public string? ClientInstructionSource { get; set; }

        [Display(Name = "Sent To CPI")]
        public DateTime? ClientInstructionSentToCPI { get; set; }

        [Display(Name = "Payment Date")]
        public DateTime? ClientPaymentDate { get; set; }

        public DateTime? ClientPaymentSentToCPI { get; set; }

        public DateTime? ClientPaymentLetterSentDate { get; set; }

        [Display(Name = "Client Confirmation")]
        public DateTime? ClientReceiptLetterSentDate { get; set; }

        [Display(Name = "Agent Responsibility")]
        public DateTime? ClientInstructionSentToAgent { get; set; }

        [StringLength(1000)]
        [Display(Name = "Remarks")]
        public string? ClientInstrxRemarks { get; set; }

        [StringLength(5)] 
        public string? PrePayLastRemSeq { get; set; }

        public DateTime? PrePayLastRemDate { get; set; }

        public bool ServiceFeePerFamily  { get; set; }

        [Display(Name = "Service Fee")]
        public decimal ServiceFee { get; set; }

        public bool IgnoreRecord  { get; set; }

        public Int16 UpdateStatus  { get; set; }

        [Display(Name = "VAT")]
        public decimal VATAmount { get; set; }

        public bool CPIDeleteFlag { get; set; }

        public bool? CPIEndOfGrace  { get; set; }

        [StringLength(3)] 
        public string? InvCurrency { get; set; }

        public DateTime? TMRDate { get; set; }

        public decimal? TMRAmount { get; set; }

        public DateTime? SAInvDate { get; set; }

        public DateTime? SASetDate { get; set; }

        public decimal? SACredAmt { get; set; }

        [StringLength(12)] 
        public string? SANumber { get; set; }

        [StringLength(50)] 
        public string? AMSFile { get; set; }

        public int CPIChangeDate  { get; set; }

        public DateTime? SQL_LastUpdate { get; set; }

        public DateTime? SQL_DateCreated { get; set; }

        public bool CPIInstructable { get; set; }

        public bool ClientInstructionSentToCPIFlag { get; set; }

        public bool ClientPaymentDateFlag { get; set; }

        public bool SendToCPI { get; set; }

        [Display(Name = "Exchange Rate")]
        public decimal? CPIExchangeRate { get; set; }

        [Display(Name = "Exchange Rate Amount")]
        public decimal? CPIExchangeRateAmt { get; set; }

        public decimal? CPI2ndReminderFaxAmt  { get; set; }

        [Display(Name = "Payment Status")]
        [StringLength(20)] 
        public string? CPIPaymentStatus { get; set; }
    }
}
