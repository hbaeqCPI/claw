using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using LawPortal.Core.Identity;
using LawPortal.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace LawPortal.Web.Services
{
    public class CPiExternalLoginManager : ICPiExternalLoginManager
    {
        private readonly ICPiDbContext _cpiDbContext;

        public CPiExternalLoginManager(ICPiDbContext cpiDbContext)
        {
            _cpiDbContext = cpiDbContext;
        }

        private IQueryable<CPiSSOClaimSystemRole> SSOClaimSystemRoles => _cpiDbContext.GetReadOnlyRepositoryAsync<CPiSSOClaimSystemRole>().QueryableList;

        public async Task<IdentityResult> AddRolesAsync(CPiUser user, string claim)
        {
            if (!string.IsNullOrEmpty(claim))
            {
                var roles = await _cpiDbContext.GetRepository<CPiUserSystemRole>().QueryableList.Where(e => e.UserId == user.Id).ToListAsync();
                var claims = await _cpiDbContext.GetRepository<CPiUserClaim>().QueryableList.Where(e => e.UserId == user.Id).ToListAsync();

                var userRoles = await GetUserSystemRolesAsync(user, claim);

                if (userRoles.Count > 0)
                {
                    var userClaims = new List<CPiUserClaim>();
                    foreach (CPiUserSystemRole userRole in userRoles)
                    {
                        userClaims.AddRange(userRole.ToCPiUserClaims());
                    }
                    _cpiDbContext.GetRepository<CPiUserSystemRole>().Add(userRoles);
                    _cpiDbContext.GetRepository<CPiUserClaim>().Add(userClaims);

                    if (roles.Any())
                        _cpiDbContext.GetRepository<CPiUserSystemRole>().Delete(roles);

                    if (claims.Any())
                        _cpiDbContext.GetRepository<CPiUserClaim>().Delete(claims);

                    try
                    {
                        await _cpiDbContext.SaveChangesAsync();
                        //detach for possible tracking on same call
                        _cpiDbContext.Detach(userRoles);
                        _cpiDbContext.Detach(userClaims);
                    }
                    catch (Exception e) //catch (DbUpdateException e)
                    {
                        var message = e.Message;
                        IdentityError err = new IdentityError();
                        err.Code = "AddRolesAsync";
                        err.Description = "An error occurred while saving to the database.";

                        return IdentityResult.Failed(err);
                    }
                }
            }

            return IdentityResult.Success;
        }

        public async Task<List<Claim>> GetClaimsAsync(string claim)
        {
            var claims = new List<Claim>();

            foreach (CPiSSOClaimSystemRole role in await GetSSOClaimSystemRoleAsync(claim))
            {
                claims.AddRange(role.ToClaims());
            }

            return claims;
        }

        public async Task<List<CPiSSOClaimSystemRole>> GetSSOClaimSystemRoleAsync(string claim)
        {
            List<CPiSSOClaimSystemRole> defaultRoles = await SSOClaimSystemRoles.Where(r => r.Claim == claim && r.System.IsEnabled).ToListAsync();

            return defaultRoles;
        }

        public async Task<List<CPiUserSystemRole>> GetUserSystemRolesAsync(CPiUser user, string claim)
        {
            var userSystemRoles = new List<CPiUserSystemRole>();

            foreach (CPiSSOClaimSystemRole role in await GetSSOClaimSystemRoleAsync(claim))
            {
                userSystemRoles.Add(new CPiUserSystemRole { UserId = user.Id, SystemId = role.SystemId, RoleId = role.RoleId, RespOffice = role.RespOffice });
            }

            return userSystemRoles;
        }

        public async Task<CPiUser> GetSSOUserAsync(string claim)
        {
            var cpiUser = CPiUser.NewExternalLogin;

            if (!string.IsNullOrEmpty(claim))
            {
                var ssoUser = await _cpiDbContext.GetReadOnlyRepositoryAsync<CPiSSOClaimUser>().QueryableList.FirstOrDefaultAsync(c => c.Claim == claim);
                if (ssoUser != null)
                {
                    var userType = cpiUser.UserType;
                    var userStatus = cpiUser.Status;

                    if (Enum.TryParse(ssoUser.UserType, out userType))
                        cpiUser.UserType = userType;

                    if (Enum.TryParse(ssoUser.UserStatus, out userStatus))
                        cpiUser.Status = userStatus;

                    switch (cpiUser.UserType)
                    {
                        case CPiUserType.Inventor:
                            cpiUser.EntityFilterType = CPiEntityType.Inventor; 
                            break;

                        case CPiUserType.ContactPerson:
                            cpiUser.EntityFilterType = CPiEntityType.ContactPerson; 
                            break;

                        case CPiUserType.Attorney:
                            cpiUser.EntityFilterType = CPiEntityType.Attorney; 
                            break;
                    }
                }
            }

            return cpiUser;
        }

        public async Task<bool> IsSSOClaimExists(string claim)
        {
            if (!string.IsNullOrEmpty(claim))
                return await SSOClaimSystemRoles.AnyAsync(c => c.Claim.ToLower() == claim.ToLower());

            return false;
        }
    }
}
