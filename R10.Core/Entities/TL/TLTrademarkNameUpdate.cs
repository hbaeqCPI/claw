using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Trademark
{
    public class TLTrademarkNameUpdate: TMSEntityFilter
    {
        //[Key]
        public int TLTmkId { get; set; }

        [Display(Name = "TMS Trademark Name")]
        public string? TMSTrademarkName { get; set; }

        [Display(Name = "PTO Trademark Name")]
        public string? PTOTrademarkName { get; set; }

        [Display(Name = "Exclude")]
        public bool Exclude { get; set; }
        public bool UpdateTrademarkName { get; set; }

        public DateTime? LastWebUpdate { get; set; }
        
        public bool? ActiveSwitch { get; set; }
        public byte[] tStamp { get; set; }

    }

}
