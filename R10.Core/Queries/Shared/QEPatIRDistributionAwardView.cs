using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Queries.Shared
{
    public class QEPatIRDistributionAwardView
    {
        public int InventorInvID { get; set; }
        public int InvId { get; set; }
        public int InventorId { get; set; }
        public int DistributionId { get; set; }
        public string? CaseNumber { get; set; }
        public string? Client { get; set; }
        public string? ClientName { get; set; }
        public string? Title { get; set; }
        public string? Inventor { get; set; }
        public string? Position { get; set; }
        public double? PercentageofInvention { get; set; }
        public int? PositionA { get; set; }
        public int? PositionB { get; set; }
        public int? PositionC { get; set; }
        public int? PercentageofOwnership { get; set; }
        public double? InitialPayment { get; set; }
        public string? InventorRemarks { get; set; }
        public int? Year { get; set; }
        public double Amount { get; set; }
        public DateTime? PaidDate { get; set; }
        public string? DistributionRemarks { get; set; }
        public string? LastName { get; set; }
        public string? FirstName { get; set; }
        public string? MiddleInitial { get; set; }
        public string? Language { get; set; }
        public string? Email { get; set; }
        public DateTime? DateTimeToday { get; set; }

    }
}
