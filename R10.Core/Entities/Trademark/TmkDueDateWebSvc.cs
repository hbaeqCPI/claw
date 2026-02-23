using R10.Core.Entities.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Entities.Trademark
{
    public class TmkDueDateWebSvc : DueDateWebSvcDetail
    {
        [Key]
        public int EntityId { get; set; }

        public int LogId { get; set; }

        public int ParentId { get; set; }

        public TmkActionDueWebSvc? Action { get; set; }
    }
}
