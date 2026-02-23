using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.DTOs
{
    public class DelegationParameter
    {
        public int ReportFormat { get; set; }
        public bool PrintActionDueRemarks { get; set; }
        public bool PrintDueDateRemarks { get; set; }
        public bool PrintRemarks { get; set; }
        public string? PrintGoods { get; set; }
        public bool PrintImage { get; set; }
        public bool PrintImageDetail { get; set; }
        public bool PrintInventors { get; set; }
        public string? PrintSystems { get; set; }
        public string? ActiveSwitch { get; set; }
        public string? UserID { get; set; }
        public bool DeDocketInstructionOnly { get; set; }
        public int StartDateAdjustment { get; set; }
        public int ReminderDateAdjustment { get; set; }
        public int ReminderToDateAdjustment { get; set; }
        public int TaskId { get; set; }
    }

    public class DelegateUser
    {
        public string? UserID { get; set; }
        public string? Email { get; set; }
    }
}
