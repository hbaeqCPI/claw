using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Entities.Patent
{
    public class InventionWebSvc : InventionWebSvcDetail
    {
        [Key]
        public int EntityId { get; set; }

        public int LogId { get; set; }

        public List<PatInventorInvWebSvc>? Inventors { get; set; }
    }

    public class InventionWebSvcDetail
    {
        [Required]
        [StringLength(25)]
        public string? CaseNumber { get; set; }

        [StringLength(255)]
        [Display(Name = "Title")]
        public string? Title { get; set; }

        [StringLength(20)]
        [Display(Name = "Disclosure Status")]
        public string? DisclosureStatus { get; set; }

        [Display(Name = "Disclosure Date")]
        public DateTime? DisclosureDate { get; set; }

        [StringLength(10)]
        public string? RespOffice { get; set; }
    }
}
