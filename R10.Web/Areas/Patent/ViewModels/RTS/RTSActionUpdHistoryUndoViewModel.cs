using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using R10.Core.DTOs;

namespace R10.Web.Areas.Patent.ViewModels
{
    public class RTSActionUpdHistoryUndoViewModel
    {
        public int PLAppId { get; set; }
        public int RevertType { get; set; }
        public int JobId { get; set; }
    }
}
