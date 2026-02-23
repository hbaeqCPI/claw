using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Core.DTOs
{
    public class RTSIDSCrossCheckCopyDTO
    {
        public int RelatedCasesId { get; set; }
        public int BaseAppId { get; set; }
        public int AppID { get; set; }
        public bool CopyToBase { get; set; }
    }
}
