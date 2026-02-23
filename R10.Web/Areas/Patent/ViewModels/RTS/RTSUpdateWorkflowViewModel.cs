using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using R10.Core.DTOs;

namespace R10.Web.Areas.Patent.ViewModels
{
    public class RTSUpdateWorkflowViewModel
    {
        public int QESetupId { get; set; }
        public bool AutoAttachImages { get; set; }
        public int AppId { get; set; }
        public int InvId { get; set; }
        public int ActionTypeId { get; set; }
        public int ActionValueId { get; set; }
        public string BatchId { get; set; }
        public DateTime? PubDate { get; set; }
        public DateTime? IssDate { get; set; }
    }

    public class RTSAutoDocketActionWorkflowViewModel
    {
        public int QESetupId { get; set; }
        public bool AutoAttachImages { get; set; }
        public int AppId { get; set; }
        public int InvId { get; set; }
        public int ActId { get; set; }
    }


}
