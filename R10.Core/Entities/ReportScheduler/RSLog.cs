using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.ReportScheduler
{
    public class RSLog
    {
        public RSLog() { }

        public RSLog(int taskId, int actionId, int logId = 0, string status = "", string info = "")
        {
            TaskId = taskId;
            ActionId = actionId;
            LogId = logId;
            Status = status;
            Info = info;
        }

        [Key]
        public int RSLogId { get; set; }
        public int? TaskId { get; set; }
        public int? ActionId { get; set; }
        public int? LogId { get; set; }
        public string? Name { get; set; }
        public string? Info { get; set; }
        public string? Status { get; set; }
        public DateTime? LogDate { get; set; }

    }
}
