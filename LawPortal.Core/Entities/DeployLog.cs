using System;
using System.ComponentModel.DataAnnotations;

namespace LawPortal.Core.Entities
{
    public class DeployLog
    {
        [Key]
        public int DeployLogId { get; set; }
        public int DeployPasswordId { get; set; }

        /// <summary>PopulateTables | GenerateScript | PushMdbs</summary>
        public string? Action { get; set; }

        /// <summary>Pat | Tmk — only set for PushMdbs</summary>
        public string? Side { get; set; }

        public string? PerformedBy { get; set; }
        public DateTime PerformedAt { get; set; }

        /// <summary>Success | Error</summary>
        public string? Status { get; set; }

        public string? Detail { get; set; }
    }
}
