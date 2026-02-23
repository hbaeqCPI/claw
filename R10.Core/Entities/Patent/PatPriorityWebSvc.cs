using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Entities.Patent
{
    public class PatPriorityWebSvc : PatPriorityWebSvcDetail
    {
        [Key]
        public int EntityId { get; set; }

        public int LogId { get; set; }

        public int PriId { get; set; }

        public int InvId { get; set; }

        public int? ParentAppId { get; set; }

        public string? AppNumberSearch { get; set; }
    }

    public class PatPriorityWebSvcDetail
    {
        [StringLength(5)]
        public string? Country { get; set; }

        [StringLength(3)]
        public string? CaseType { get; set; }

        [StringLength(20)]
        public string? AppNumber { get; set; }

        public DateTime? FilDate { get; set; }

        [StringLength(12)]
        public string? AccessCode { get; set; }
    }
}
