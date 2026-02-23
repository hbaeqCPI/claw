using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.AMS
{
    public class AMSProjection : AMSProjectionDetail
    {
        public AMSMain AMSMain { get; set; }
        public AMSDue AMSDue { get; set; }
        public CurrencyType? CurrencyType { get; set; }
    }

    public class AMSProjectionDetail : BaseEntity
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
        public string AnnuityYear { get; set; }

        [StringLength(5)]
        public string AnnuityNoDue { get; set; }

        public DateTime? AnnuityDueDate { get; set; }

        [StringLength(5)]
        public string PaidThru { get; set; }

        public decimal CostToExpiration { get; set; }

        public DateTime? CPIInstructionDate { get; set; }

        [StringLength(10)]
        public string CPIInstructionType { get; set; }

        public DateTime? CPIPaymentDate { get; set; }

        public DateTime? CPIInvoiceDate { get; set; }

        [StringLength(20)]
        public string CPIInvoiceNo { get; set; }

        public decimal CPIInvoiceAmount { get; set; }

        public decimal CPIInvoicePaidAmount { get; set; }

        public DateTime? CPIInvoicePaidDate { get; set; }

        public DateTime? CPIInvoiceCancelDate { get; set; }

        public DateTime? CPISettleDate { get; set; }

        public decimal CPISettleAmount { get; set; }

        [StringLength(20)]
        public string CPISettleNo { get; set; }

        public DateTime? CPISettlePaidDate { get; set; }

        public DateTime? CPIReceiptPostDate { get; set; }

        public decimal CPIReviewAmount { get; set; }

        public DateTime? CPIReviewDate { get; set; }

        public DateTime? CPINonPaymentDate { get; set; }

        public decimal CPIClientFee { get; set; }

        public decimal CPIReminderFaxAmount { get; set; }

        public DateTime? CPIGraceDate { get; set; }

        public DateTime? ClientLastReminderDate { get; set; }

        [StringLength(5)]
        public string ClientLastReminderSequence { get; set; }

        public DateTime? ClientInstructionDate { get; set; }

        [StringLength(5)]
        public string ClientInstructionType { get; set; }

        [StringLength(20)]
        public string ClientInstructionBy { get; set; }

        //*** AMSDUE ***
        //public int ClientInstructionLogId { get; set; }

        [StringLength(1)]
        public string ClientInstructionSource { get; set; }

        public DateTime? ClientInstructionSentToCPI { get; set; }

        public DateTime? ClientPaymentDate { get; set; }

        public DateTime? ClientPaymentSentToCPI { get; set; }

        public DateTime? ClientPaymentLetterSentDate { get; set; }

        public DateTime? ClientReceiptLetterSentDate { get; set; }

        public DateTime? ClientInstructionSentToAgent { get; set; }

        [StringLength(1000)]
        public string ClientInstrxRemarks { get; set; }

        [StringLength(5)]
        public string PrePayLastRemSeq { get; set; }

        public DateTime? PrePayLastRemDate { get; set; }

        public bool ServiceFeePerFamily { get; set; }

        public decimal ServiceFee { get; set; }

        public bool IgnoreRecord { get; set; }

        public Int16 UpdateStatus { get; set; }

        public decimal VATAmount { get; set; }

        public bool CPIDeleteFlag { get; set; }

        public bool? CPIEndOfGrace { get; set; }

        [StringLength(3)]
        public string? InvCurrency { get; set; }

        public DateTime? TMRDate { get; set; }

        public decimal? TMRAmount { get; set; }

        public DateTime? SAInvDate { get; set; }

        public DateTime? SASetDate { get; set; }

        public decimal? SACredAmt { get; set; }

        [StringLength(12)]
        public string SANumber { get; set; }

        [StringLength(50)]
        public string AMSFile { get; set; }

        //*** UPDATEDBY? ***
        [StringLength(20)]
        public string UserId { get; set; }

        public int CPIChangeDate { get; set; }

        public DateTime? SQL_LastUpdate { get; set; }

        public DateTime? SQL_DateCreated { get; set; }

        public bool CPIInstructable { get; set; }

        //*** AMSDUE ***
        //public bool ClientInstructionSentToCPIFlag { get; set; }

        //*** AMSDUE ***
        //public bool ClientPaymentDateFlag { get; set; }

        //*** AMSDUE ***
        //public bool SendToCPI { get; set; }

        public decimal? CPIExchangeRate { get; set; }

        public decimal? CPIExchangeRateAmt { get; set; }

        public decimal? CPI2ndReminderFaxAmt { get; set; }

        //*** AMSDUE ***
        //[StringLength(20)]
        //public string CPIPaymentStatus { get; set; }
    }
}
