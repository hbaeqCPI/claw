using R10.Core.DTOs;
using R10.Core.Entities;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class QuickDocketDeDocketViewModel
    {
        public int DeDocketId { get; set; }
        public byte[]? tStamp { get; set; }
        public string? Instruction { get; set; }
        public string? Remarks { get; set; }
        public string? InstructedBy { get; set; }
        public DateTime? InstructionDate { get; set; }
        public string? CompletedDesc { get; set; }
        public List<WorkflowEmailViewModel>? emailWorkflows { get; set; }
    }

    
}
