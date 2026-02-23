using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Core
{
    public class TmkTrademarkRenewalFields
    {
        public DateTime? CalcRenewalDate { get; set; }
        public bool EqualRenewalDate { get; set; }
        public int NoOfRenDates{ get; set; }
        public string Message { get; set; }
        public string MessageTitle { get; set; }
        public string OkLabel { get; set; }
        public string CancelLabel { get; set; }
    }

    public class TmkTrademarkRenewalParameters
    {
        public int TmkId { get; set; }
        public string Country { get; set; }
        public string CaseType { get; set; }
        public DateTime? FilDate { get; set; }
        public DateTime? PubDate { get; set; }
        public DateTime? RegDate { get; set; }
        public DateTime? PriDate { get; set; }
        public DateTime? ParentFilDate { get; set; }
        public DateTime? AllowanceDate { get; set; }
        public DateTime? LastRenewalDate { get; set; }
        public DateTime? NextRenewalDate { get; set; }
    }
}
