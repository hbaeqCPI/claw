using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Trademark
{

    public class TLActionComparePTO : TLActionCompare
    { 
    }

    public class TLActionCompare: TMSEntityFilter
    {

        public int TLTmkId { get; set; }
        public string? TrademarkStatus { get; set; }

        public string? ActionType { get; set; }
        public string? ActionDue { get; set; }
        public string? Indicator { get; set; }

        [Display(Name = "Action")]
        public string? Action { get; set; }

        [Display(Name = "PTO Base Date")]
        public DateTime BaseDate { get; set; }
        [Display(Name = "PTO Due Date")]
        public DateTime? DueDate { get; set; }
        public DateTime? DateCompleted { get; set; }
        public DateTime? LastWebUpdate { get; set; }

        public bool ActiveSwitch { get; set; }

        [Display(Name = "Exclude")]
        public bool Exclude { get; set; }
    }

}
