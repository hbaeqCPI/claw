using R10.Core.Entities;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace R10.Core.Identity
{
    public class CPiUserEntityFilter
    {
        [StringLength(450)]
        public string UserId { get; set; }

        public int EntityId { get; set; }

        public CPiUser CPiUser { get; set; }

        [NotMapped]
        public ContactPerson? ContactPerson { get; set; }

        [NotMapped]
        public PatInventor? PatInventor { get; set; }

        [NotMapped]
        public Attorney? Attorney { get; set; }
    }
}
