using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Queries.Shared
{
    public class QEPatIRFRRemunerationAwardView
    {
        public int InventorInvID { get; set; }
        public int InvId { get; set; }
        public int InventorId { get; set; }
        public string? CaseNumber { get; set; }
        public string? Client { get; set; }
        public string? ClientName { get; set; }
        public string? Title { get; set; }
        public string? Inventor { get; set; }
        //public string? Position { get; set; }
        public double? PercentageofInvention { get; set; }
        public double? InventionReportAward { get; set; }
        public DateTime? InventionReportAwardDate { get; set; }
        public double? FirstFilingAward { get; set; }
        public DateTime? FirstFilingAwardDate { get; set; }
        public double? InUseAward { get; set; }
        public DateTime? InUseAwardDate { get; set; }
        //public int? PositionA { get; set; }
        //public int? PositionB { get; set; }
        //public int? PositionC { get; set; }
        //public int? PercentageofOwnership { get; set; }
        //public decimal? InitialPayment { get; set; }
        public string? Remarks { get; set; }
        //public string? YearlyAwards { get; set; }
        public string? LastName { get; set; }
        public string? FirstName { get; set; }
        public string? MiddleInitial { get; set; }
        public string? Language { get; set; }
        public string? Email { get; set; }
        public DateTime? DateTimeToday { get; set; }

    }
}
