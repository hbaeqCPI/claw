using R10.Core.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;

namespace R10.Core.Identity
{
    public class CPiSSOClaimSystemRole
    {
        [Key]
        public int Id { get; set; }

        [StringLength(450)]
        public string? Claim { get; set; }

        [StringLength(450)]
        public string? SystemId { get; set; }

        [StringLength(450)]
        public string? RoleId { get; set; }

        [StringLength(10)]
        public string? RespOffice { get; set; }

        public List<Claim> ToClaims()
        {
            CPiUserSystemRole userSystemRole = new CPiUserSystemRole { SystemId = (SystemId ?? ""), RoleId = (RoleId ?? ""), RespOffice = RespOffice };
            return userSystemRole.ToClaims();
        }

        public CPiSystem? System { get; set; }
    }
}
