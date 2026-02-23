using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.DTOs
{
    [Keyless]
    public class RTSPFSWorkflowBatch
    {
        public string BatchId { get; set; }
    }

    [Keyless]
    public class RTSPFSWorkflowApp
    {
        public int AppId { get; set; }
        public int InvId { get; set; }
        public int? ClientId { get; set; }
        public string BatchId { get; set; }
        public DateTime? PubDate { get; set; }
        public DateTime? IssDate { get; set; }
    }
}
