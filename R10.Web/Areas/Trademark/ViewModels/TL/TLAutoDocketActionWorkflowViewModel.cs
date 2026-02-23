using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using R10.Core.DTOs;

namespace R10.Web.Areas.Trademark.ViewModels
{

    public class TLAutoDocketActionWorkflowViewModel
    {
        public int QESetupId { get; set; }
        public bool AutoAttachImages { get; set; }
        public int TmkId { get; set; }
        public int ActId { get; set; }
    }


}
