using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using R10.Core.DTOs;

namespace R10.Web.Areas.Patent.ViewModels
{
    public class EPOWorkflowViewModel
    {
        public int QESetupId { get; set; }
        public bool AutoAttachImages { get; set; }
        public string? DataKey { get; set; }
        public int DataKeyValue { get; set; }
        public int DocId { get; set; }
        public int CommActId { get; set; }
        public int DDActId { get; set; }
        public string? Error { get; set; } = string.Empty;
        public string? AttachmentFilter { get; set; }
    }
}
