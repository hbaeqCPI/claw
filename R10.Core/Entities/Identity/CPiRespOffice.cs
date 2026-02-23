using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace R10.Core.Identity
{
    public class CPiRespOffice
    {
        [Key]
        [StringLength(10)]
        public string RespOffice { get; set; }

        [StringLength(256)]
        public string? Name { get; set; }
        
        [StringLength(10)]
        public string? SystemTypes { get; set; }

        public List<CPiUserSystemRole> UserSystemRoles { get; set; }
    }
}
