using Microsoft.AspNetCore.Identity;
using LawPortal.Core.Identity;
using Sustainsys.Saml2;
using System.Security.Claims;

namespace LawPortal.Web.Services
{
    public class Saml2ClaimsFactory : IUserClaimsPrincipalFactory<CPiUser>
    {
        IUserClaimsPrincipalFactory<CPiUser> _inner;
        ExternalLoginInfo _externalLoginInfo;

        public Saml2ClaimsFactory(
            IUserClaimsPrincipalFactory<CPiUser> inner,
            ExternalLoginInfo externalLoginInfo)
        {
            _inner = inner;
            _externalLoginInfo = externalLoginInfo;
        }

        public async Task<ClaimsPrincipal> CreateAsync(CPiUser user)
        {
            var principal = await _inner.CreateAsync(user);

            //saml2 slo 
            //ff claims needed by sustainsys to invoke slo
            //http://Sustainsys.se/Saml2/LogoutNameIdentifier
            //http://Sustainsys.se/Saml2/SessionIndex
            var logoutInfo = _externalLoginInfo.Principal.FindFirst(Saml2ClaimTypes.LogoutNameIdentifier);
            var sessionIndex = _externalLoginInfo.Principal.FindFirst(Saml2ClaimTypes.SessionIndex);

            var identity = principal.Identities.Single();
            identity.AddClaim(logoutInfo);
            identity.AddClaim(sessionIndex);

            return principal;
        }
    }
}
