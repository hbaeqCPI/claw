
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using OpenIddict.Abstractions;
using OpenIddict.Validation;
using OpenIddict.Validation.AspNetCore;
using LawPortal.Core.Identity;
using System.Threading.Tasks;

namespace LawPortal.Web.Controllers
{
    // adapted with mods from OpenIddict 2.0 sample project
    public class UserInfoController : Microsoft.AspNetCore.Mvc.Controller
    {
        private readonly UserManager<CPiUser> _userManager;

        public UserInfoController(UserManager<CPiUser> userManager)
        {
            _userManager = userManager;
        }

        //
        // GET: /api/userinfo
        [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        [HttpGet("~/connect/userinfo"), Produces("application/json")]
        public async Task<IActionResult> UserInfo()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return BadRequest(new OpenIddictResponse
                {
                    Error = OpenIddictConstants.Errors.InvalidGrant,
                    ErrorDescription = "The user profile is no longer available."
                });
            }

            var claims = new JObject();

            // Note: the "sub" claim is a mandatory claim and must be included in the JSON response.
            claims[OpenIddictConstants.Claims.Subject] = await _userManager.GetUserIdAsync(user);

            if (User.HasClaim(OpenIddictConstants.Claims.Scope, OpenIddictConstants.Scopes.Email))
            {
                claims[OpenIddictConstants.Claims.Email] = await _userManager.GetEmailAsync(user);
                claims[OpenIddictConstants.Claims.EmailVerified] = await _userManager.IsEmailConfirmedAsync(user);
            }

            if (User.HasClaim(OpenIddictConstants.Claims.Scope, OpenIddictConstants.Scopes.Phone))
            {
                claims[OpenIddictConstants.Claims.PhoneNumber] = await _userManager.GetPhoneNumberAsync(user);
                claims[OpenIddictConstants.Claims.PhoneNumberVerified] = await _userManager.IsPhoneNumberConfirmedAsync(user);
            }

            if (User.HasClaim(OpenIddictConstants.Claims.Scope, OpenIddictConstants.Scopes.Roles))
            {
                claims[OpenIddictConstants.Claims.Role] = JArray.FromObject(await _userManager.GetRolesAsync(user));
            }

            // Note: the complete list of standard claims supported by the OpenID Connect specification
            // can be found here: http://openid.net/specs/openid-connect-core-1_0.html#StandardClaims

            return Json(claims);
        }
    }
}
