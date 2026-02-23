using System;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class StepperStatusViewModel
    {    
        public string? Label { get; set; }
        public string? Icon { get; set; }
        public bool Enabled { get; set; }
        public bool Selected { get; set; }
        public DateTime? DateChanged { get; set; }
        public string? GroupName { get; set; }
        public int? WorkflowOrder { get; set; }
        public string? CSSClass { get; set; }  
        public int DaysLimit { get; set; }
        public string? Value { get; set; }
    }
}