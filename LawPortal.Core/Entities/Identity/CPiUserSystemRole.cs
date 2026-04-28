using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace LawPortal.Core.Identity
{
    public class CPiUserSystemRole
    {
        [Key]
        public int Id { get; set; }

        [StringLength(450)]
        [Required]
        public string UserId { get; set; }

        [StringLength(450)]
        [Required]
        [UIHint("UserSystem")]
        public string SystemId { get; set; }

        [StringLength(450)]
        [Required]
        [UIHint("UserRole")]
        public string RoleId { get; set; }

        [StringLength(10)]
        [UIHint("UserRespOffice")]
        public string? RespOffice { get; set; }

        public CPiRespOffice UserRespOffice { get; set; }

        public List<Claim> ToClaims()
        {
            var claims = new List<Claim>();

            claims.Add(new Claim(ClaimTypes.System, SystemId));
            claims.Add(new Claim(ClaimTypes.Role, string.Concat(SystemId, RoleId, string.IsNullOrEmpty(RespOffice) ? "" : $"|{RespOffice}")));

            return claims;
        }

        public List<CPiUserClaim> ToCPiUserClaims()
        {
            var userClaims = new List<CPiUserClaim>();
            foreach (var claim in this.ToClaims())
            {
                userClaims.Add(new CPiUserClaim()
                {
                    UserId = this.UserId,
                    ClaimType = claim.Type,
                    ClaimValue = claim.Value
                });
            }
            return userClaims;
        }

        public CPiUser CPiUser { get; set; }
        public CPiSystem CPiSystem { get; set; }
        public CPiRole CPiRole { get; set; }
    }
}
