using R10.Core.Entities.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Entities.Patent
{
    public class PatAssignmentHistoryWebSvc : PatAssignmentHistoryWebSvcDetail
    {
        [Key]
        public int EntityId { get; set; }

        public int LogId { get; set; }
    }

    public class PatAssignmentHistoryWebSvcDetail : AssignmentHistoryWebSvc
    {
        [Required]
        public int AppId { get; set; }
    }
}
