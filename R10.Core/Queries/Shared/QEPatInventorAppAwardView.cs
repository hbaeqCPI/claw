using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Core.Queries.Shared
{
    public class QEPatInventorAppAwardView
    {
        public int AwardId { get; set; }
        public int AppId { get; set; }
        public int InventorId { get; set; }
        public decimal Amount { get; set; }
        public DateTime? AwardDate { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string? Remarks { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime? LastUpdate { get; set; }
        public string? AwardType { get; set; }
        public string? CaseNumber { get; set; }
        public string? Country { get; set; }
        public string? SubCase { get; set; }
        public string? LastName { get; set; }
        public string? FirstName { get; set; }
        public string? MiddleInitial { get; set; }
        public string? Language { get; set; }
        public string? Email { get; set; }
        public string? CountryName { get; set; }
        public DateTime? DateTimeToday { get; set; }

    }
}
