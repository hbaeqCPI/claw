using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace LawPortal.Core.Identity
{
    public class CPiUserTypeSystemRole
    {
        [Key]
        public int Id { get; set; }

        public CPiUserType UserType { get; set; }

        [StringLength(450)]
        [Required]
        public string SystemId { get; set; }

        [StringLength(450)]
        [Required]
        public string RoleId { get; set; }

        [StringLength(10)]
        public string? RespOffice { get; set; }

        public List<Claim> ToClaims()
        {
            CPiUserSystemRole userSystemRole = new CPiUserSystemRole { SystemId = SystemId, RoleId = RoleId, RespOffice = RespOffice };
            return userSystemRole.ToClaims();
        }

        public CPiSystem System { get; set; }
    }
}
