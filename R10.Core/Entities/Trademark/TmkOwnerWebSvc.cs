using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Entities.Trademark
{
    public class TmkOwnerWebSvc : TmkOwnerWebSvcDetail
    {
        [Key]
        public int EntityId { get; set; }

        public int LogId { get; set; }

        public int TmkId { get; set; }

        public int TmkOwnerId { get; set; }
    }

    public class TmkOwnerWebSvcDetail
    {
        public int? OwnerID { get; set; }

        public double? Percentage { get; set; }
    }
}
