using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Api.Models
{
    public class AnnuityDueData
    {
        public string? CPIClientCode { get; set; }

        public string? CaseNumber { get; set; }
        public string? Country { get; set; }
        public string? CountryName { get; set; }
        public string? SubCase { get; set; }

        public string? CPICaseType { get; set; }
        public string? CPITitle { get; set; }
        public string? CPIStatus { get; set; }
        public string? CPIAbstract { get; set; }
        public string? CPIOwner { get; set; }
        public string? CPIInventors { get; set; }
        public string? CPIClient { get; set; }
        public string? ClientRef { get; set; }
        public string? CPIAppNo { get; set; }
        public string? CPIPatNo { get; set; }
        public DateTime? CPIExpireDate { get; set; }

        public string? PaymentType { get; set; }
        public string? AnnuityYear { get; set; }
        public string? AnnuityNoDue { get; set; }
        public DateTime? AnnuityDueDate { get; set; }
        public decimal TotalCost { get; set; }
        public decimal CostToExpiration { get; set; }

        public DateTime? CPIInstructionDate { get; set; }
        public string? CPIInstructionType { get; set; }
        public DateTime? CPIPaymentDate { get; set; }
        public DateTime? CPIInvoiceDate { get; set; }
        public string? CPIInvoiceNo { get; set; }
        public decimal CPIInvoiceAmount { get; set; }
        public decimal CPIInvoicePaidAmount { get; set; }
        public DateTime? CPIInvoicePaidDate { get; set; }

        public string? ClientInstruction { get; set; }
        public DateTime? ClientInstructionDate { get; set; }
        public string? ClientInstrxRemarks { get; set; }

        public List<ProductData>? Products { get; set; }
        public List<LicenseeData>? Licensees { get; set; }
    }
}
