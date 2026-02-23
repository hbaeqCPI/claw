using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using R10.Core.Identity;
using R10.Core.Entities.Patent;

namespace R10.Core.Entities.DMS
{
    public class DMSDueDateDateTakenLog : DueDateDateTakenLog
    {
        public DMSDueDate? DMSDueDate { get; set; }
    }
}
