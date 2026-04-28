using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LawPortal.Core.Identity
{
    public class CPiSSOClaimUser
    {
        [Key]
        public int Id { get; set; }

        [StringLength(450)]
        public string Claim { get; set; }

        [StringLength(30)]
        public string? UserType { get; set; } //CPiUserType enum name

        [StringLength(30)]
        public string? UserStatus { get; set; } //CPiUserStatus enum name
    }
}
