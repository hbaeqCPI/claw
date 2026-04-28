using Microsoft.AspNetCore.Identity;
using LawPortal.Core.Identity;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace LawPortal.Core.Interfaces
{
    public interface ICPiExternalLoginManager
    {
        Task<List<CPiSSOClaimSystemRole>> GetSSOClaimSystemRoleAsync(string claim);
        Task<List<Claim>> GetClaimsAsync(string claim);
        Task<List<CPiUserSystemRole>> GetUserSystemRolesAsync(CPiUser user, string claim);
        Task<IdentityResult> AddRolesAsync(CPiUser user, string claim);
        Task<bool> IsSSOClaimExists(string claim);

        Task<CPiUser> GetSSOUserAsync(string claim);
    }
}
