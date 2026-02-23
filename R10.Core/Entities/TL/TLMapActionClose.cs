using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Core.Entities.Trademark
{
    public class TLMapActionClose : BaseEntity
    {
        [Key]
        public int MapCloseId { get; set; }
        public int CloseSourceId { get; set; }
        public string? MapGroup { get; set; }
        public int MapSourceId { get; set; }
        
        [NotMapped]
        [Display(Name ="PTO Action")]
        public string? MapSearchAction { get; set; }

        public TLMapActionDueSource? ActionSource { get; set; }
    }
}
