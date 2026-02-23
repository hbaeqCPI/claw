/*
 * Licensed under the Apache License, Version 2.0 (http://www.apache.org/licenses/LICENSE-2.0)
 * See https://github.com/openiddict/openiddict-core for more information concerning
 * the license and the contributors participating to this project.
 */

using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Core;
using OpenIddict.EntityFrameworkCore.Models;
using OpenIddict.Server;
using R10.Core.Identity;
using R10.Web.Filters;
using R10.Web.Models.AuthorizationViewModels;
using static OpenIddict.Abstractions.OpenIddictConstants;
using OpenIddict.Server.AspNetCore;
using Microsoft.AspNetCore;
using R10.Web.Services;

namespace R10.Web.Controllers
{
    public class AuthorizationController : Microsoft.AspNetCore.Mvc.Controller
    {
        private readonly IOpenIddictApplicationManager _applicationManager;
        private readonly CPiSignInManager _signInManager;
        private readonly CPiUserManager _userManager;
        private readonly IOpenIddictScopeManager _scopeManager;

        public AuthorizationController(
            IOpenIddictApplicationManager applicationManager,
            CPiSignInManager signInManager,
            CPiUserManager userManager,
            IOpenIddictScopeManager scopeManager)
        {
            _applicationManager = applicationManager;
            _signInManager = signInManager;
            _userManager = userManager;
            _scopeManager = scopeManager;
        }

        #region Authorization code, implicit and implicit flows
        // Note: to support interactive flows like the code flow,
        // you must provide your own authorization endpoint action:

        [Authorize, HttpGet("~/connect/authorize")]
        public async Task<IActionResult> Authorize()
        {
            var request = HttpContext.GetOpenIddictServerRequest();
            //Debug.Assert(request.IsAuthorizationRequest(),
            //    "The OpenIddict binder for ASP.NET Core MVC is not registered. " +
            //    "Make sure services.AddOpenIddict().AddServer().UseMvc() is correctly called.");

            // Retrieve the application details from the database.
            var application = await _applicationManager.FindByClientIdAsync(request.ClientId);
            if (application == null)
            {
                return View("Error", new ErrorViewModel
                {
                    Error = OpenIddictConstants.Errors.InvalidClient,
                    ErrorDescription = "Invalid client application."
                });
            }

            // Flow the request_id to allow OpenIddict to restore
            // the original authorization request from the cache.
            if (request.HasPrompt(OpenIddictConstants.Prompts.Consent))  // check if we need to open consent prompt
            {
                return View(new AuthorizeViewModel
                {
                    ApplicationName = await _applicationManager.GetDisplayNameAsync(application),
                    RequestId = request.RequestId,
                    Parameters = request.GetParameters(),
                    Scope = request.Scope
                });
            }

            return await Accept();
        }

        [Authorize, FormValueRequired("submit.Accept")]
        [HttpPost("~/connect/authorize"), ValidateAntiForgeryToken]
        public async Task<IActionResult> Accept()
        {
            var request = HttpContext.GetOpenIddictServerRequest();
            //Debug.Assert(request.IsAuthorizationRequest(),
            //    "The OpenIddict binder for ASP.NET Core MVC is not registered. " +
            //    "Make sure services.AddOpenIddict().AddServer().UseMvc() is correctly called.");

            // Retrieve the profile of the logged in user.
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return View("Error", new ErrorViewModel
                {
                    Error = OpenIddictConstants.Errors.ServerError,
                    ErrorDescription = "Invalid user."
                });
            }

            // Create a new authentication ticket.
            var principal = await CreateTicketAsync(request, user);

            // Returning a SignInResult will ask OpenIddict to issue the appropriate access/identity tokens.
            return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        [Authorize, FormValueRequired("submit.Deny")]
        [HttpPost("~/connect/authorize"), ValidateAntiForgeryToken]
        public IActionResult Deny()
        {
            // Notify OpenIddict that the authorization grant has been denied by the resource owner
            // to redirect the user agent to the client application using the appropriate response_mode.
            return Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        // Note: the logout action is only useful when implementing interactive
        // flows like the authorization code flow or the implicit flow.

        [HttpGet("~/connect/logout")]
        public IActionResult Logout(OpenIddictRequest request)
        {
            //Debug.Assert(request.IsLogoutRequest(),
            //    "The OpenIddict binder for ASP.NET Core MVC is not registered. " +
            //    "Make sure services.AddOpenIddict().AddServer().UseMvc() is correctly called.");

            // Flow the request_id to allow OpenIddict to restore
            // the original logout request from the distributed cache.
            return View(new LogoutViewModel
            {
                RequestId = request.RequestId,
                PostLogoutRedirectUri = request.PostLogoutRedirectUri       // pass this to redirect to proper client page on logout post
            });
        }

        [HttpPost("~/connect/logout"), ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // Ask ASP.NET Core Identity to delete the local and external cookies created
            // when the user agent is redirected from the external identity provider
            // after a successful authentication flow (e.g Google or Facebook).
            await _signInManager.SignOutAsync();

            // Returning a SignOutResult will ask OpenIddict to redirect the user agent
            // to the post_logout_redirect_uri specified by the client application.
            //return SignOut(
            //    authenticationSchemes: OpenIddictServerDefaults.AuthenticationScheme,
            //    properties: new AuthenticationProperties
            //    {
            //        RedirectUri = "/"                                         // Noooo, this prevents redirect back to client app!
            //    });

            return SignOut(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

        }
        #endregion

        #region Password, authorization code, refresh token flows, client credentials (modified to include username)
        // Note: to support non-interactive flows like password,
        // you must provide your own token endpoint action:

        [HttpPost("~/connect/token"), Produces("application/json")]
        public async Task<IActionResult> Exchange()
        {
            var request = HttpContext.GetOpenIddictServerRequest();
            //Debug.Assert(request.IsTokenRequest(),
            //    "The OpenIddict binder for ASP.NET Core MVC is not registered. " +
            //    "Make sure services.AddOpenIddict().AddMvcBinders() is correctly called.");

            
            if (request.IsPasswordGrantType())
            {
                CPiUser user;
                if (User.Identity.IsAuthenticated && User.Identity.Name.Equals(request.Username))
                {
                    user = await _userManager.FindByNameAsync(User.Identity.Name);
                    if (user == null)
                    {
                        return BadRequest(new OpenIddictResponse
                        {
                            Error = OpenIddictConstants.Errors.InvalidGrant,
                            ErrorDescription = "Authentication failed."
                        });
                    }
                }
                else
                {
                    user = await _userManager.FindByNameAsync(request.Username);
                    if (user == null)
                    {
                        return BadRequest(new OpenIddictResponse
                        {
                            Error = OpenIddictConstants.Errors.InvalidGrant,
                            ErrorDescription = "Invalid login attempt."
                        });
                    }

                    // Validate CPI user requirements
                    // Validate the username/password parameters and ensure the account is not locked out.
                    var result = await _signInManager.CheckWebApiUserRequirements(user, request.Password ?? "", true);
                    if (!result.Succeeded)
                    {
                        return BadRequest(new OpenIddictResponse
                        {
                            Error = OpenIddictConstants.Errors.InvalidGrant,
                            ErrorDescription = "Invalid login attempt."
                        });
                    }
                }

                // Create a new authentication ticket.
                var principal = await CreateTicketAsync(request, user);

                return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            else if (request.IsAuthorizationCodeGrantType() || request.IsRefreshTokenGrantType())
            {
                // Retrieve the claims principal stored in the authorization code/refresh token.
                var info = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

                // Retrieve the user profile corresponding to the authorization code/refresh token.
                // Note: if you want to automatically invalidate the authorization code/refresh token
                // when the user password/roles change, use the following line instead:
                // var user = _signInManager.ValidateSecurityStampAsync(info.Principal);
                var user = await _userManager.GetUserAsync(info.Principal);
                if (user == null)
                {
                    return BadRequest(new OpenIddictResponse
                    {
                        Error = OpenIddictConstants.Errors.InvalidGrant,
                        ErrorDescription = "The token is no longer valid."
                    });
                }

                // Validate CPI user requirements
                var result = await _signInManager.CheckWebApiUserRequirements(user);
                if (!result.Succeeded)
                {
                    return BadRequest(new OpenIddictResponse
                    {
                        Error = OpenIddictConstants.Errors.InvalidGrant,
                        ErrorDescription = "The user is no longer allowed to sign in."
                    });
                }

                // Ensure the user is still allowed to sign in.
                if (!await _signInManager.CanSignInAsync(user))
                {
                    return BadRequest(new OpenIddictResponse
                    {
                        Error = OpenIddictConstants.Errors.InvalidGrant,
                        ErrorDescription = "The user is no longer allowed to sign in."
                    });
                }

                // Create a new authentication ticket, but reuse the properties stored in the
                // authorization code/refresh token, including the scopes originally granted.
                var principal = await CreateTicketAsync(request, user);

                return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            else if (request.IsClientCredentialsGrantType())
            {
                // modified client credential flow, requires username on top of clientid, clientsecret
                var user = await _userManager.FindByNameAsync(request.Username);
                if (user == null)
                {
                    return BadRequest(new OpenIddictResponse
                    {
                        Error = OpenIddictConstants.Errors.InvalidGrant,
                        ErrorDescription = "Authentication failed."
                    });
                }

                // Validate CPI user requirements
                var result = await _signInManager.CheckWebApiUserRequirements(user);
                if (!result.Succeeded)
                {
                    return BadRequest(new OpenIddictResponse
                    {
                        Error = OpenIddictConstants.Errors.InvalidGrant,
                        ErrorDescription = "Authentication failed."
                    });
                }

                // Create a new authentication ticket.
                var principal = await CreateTicketAsync(request, user);

                return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

                // version 2.0 ------------- original code
                //var identity = new ClaimsIdentity(OpenIddictServerDefaults.AuthenticationScheme);
                //identity.AddClaim(ClaimTypes.NameIdentifier, request.ClientId,
                //    OpenIdConnectConstants.Destinations.AccessToken);

                //// Create a new authentication ticket holding the user identity.
                //var ticket = new AuthenticationTicket(
                //    new ClaimsPrincipal(identity),
                //    new AuthenticationProperties(),
                //    OpenIddictServerDefaults.AuthenticationScheme);

                //return SignIn(ticket.Principal, ticket.Properties, ticket.AuthenticationScheme);
            }

            return BadRequest(new OpenIddictResponse
            {
                Error = OpenIddictConstants.Errors.UnsupportedGrantType,
                ErrorDescription = $"The specified grant type is not supported. {request.GrantType}"
            });
        }
        #endregion

        private async Task<ClaimsPrincipal> CreateTicketAsync(
            OpenIddictRequest request, CPiUser user,
            AuthenticationProperties properties = null)
        {
            // Create a new ClaimsPrincipal containing the claims that
            // will be used to create an id_token, a token or a code.
            var principal = await _signInManager.CreateUserPrincipalAsync(user);

            // Add mandatory subject claim
            var identity = (ClaimsIdentity)principal.Identity;
            var subClaim = new Claim(Claims.Subject, user.Id);
            subClaim.SetDestinations(Destinations.AccessToken, Destinations.IdentityToken);
            identity.AddClaim(subClaim);

            // Create a new authentication ticket holding the user identity.


            if (!request.IsAuthorizationCodeGrantType() && !request.IsRefreshTokenGrantType())
            {
                // Note: in this sample, the granted scopes match the requested scope
                // but you may want to allow the user to uncheck specific scopes.
                // For that, simply restrict the list of scopes before calling SetScopes.
                principal.SetScopes(request.GetScopes());
                var scopes = _scopeManager.ListResourcesAsync(principal.GetScopes());
                await foreach (string scope in scopes)
                {
                    principal.SetResources(scope);
                }

                //principal.SetResources(await _scopeManager.ListResourcesAsync(principal.GetScopes()).ToListAsync()); //this won't work in C# 8, use the code above
                //principal.SetResources("resource_server");
            }

            foreach (var claim in principal.Claims)
            {
                claim.SetDestinations(GetDestinations(claim, principal));
            }

            return principal;
        }

        private IEnumerable<string> GetDestinations(Claim claim, ClaimsPrincipal principal)
        {
            // Note: by default, claims are NOT automatically included in the access and identity tokens.
            // To allow OpenIddict to serialize them, you must attach them a destination, that specifies
            // whether they should be included in access tokens, in identity tokens or in both.

            switch (claim.Type)
            {
                case Claims.Name:
                    yield return Destinations.AccessToken;

                    if (principal.HasScope(Scopes.Profile))
                        yield return Destinations.IdentityToken;

                    yield break;

                case Claims.Email:
                    yield return Destinations.AccessToken;

                    if (principal.HasScope(Scopes.Email))
                        yield return Destinations.IdentityToken;

                    yield break;

                case Claims.Role:
                    yield return Destinations.AccessToken;

                    if (principal.HasScope(Scopes.Roles))
                        yield return Destinations.IdentityToken;

                    yield break;

                // Never include the security stamp in the access and identity tokens, as it's a secret value.
                case "AspNet.Identity.SecurityStamp": yield break;

                default:
                    yield return Destinations.AccessToken;
                    yield break;
            }
        }
    }
}