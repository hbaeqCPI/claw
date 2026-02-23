using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Patent.ViewModels
{
    public class PatCostTrackingInvInvInfoViewModel
    {
        public int InvId { get; set; }
        public string? CaseNumber { get; set; }
        public string? InvTitle { get; set; }
        public string? DisclosureStatus { get; set; }
        //public int AgentID { get; set; }
        //public string? AgentName { get; set; }
        //public string? AgentCode { get; set; }
    }
}