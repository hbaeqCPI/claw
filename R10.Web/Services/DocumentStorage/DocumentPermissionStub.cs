using Microsoft.AspNetCore.Authorization;
using R10.Core.Entities;
using R10.Web.Helpers;
using R10.Web.Security;
using System.Security.Claims;
using System.Threading.Tasks;

namespace R10.Web.Services.DocumentStorage
{
    /// <summary>
    /// Minimal document permission implementation for debloated app.
    /// Performs policy-only checks; entity-level access (invention/application/trademark etc.) is not validated.
    /// </summary>
    public class DocumentPermissionStub : IDocumentPermission
    {
        private readonly IAuthorizationService _authorizationService;

        public DocumentPermissionStub(IAuthorizationService authorizationService)
        {
            _authorizationService = authorizationService;
        }

        public async Task<bool> HasPermission(ClaimsPrincipal user, string system, string screenCode, int key, string fileName, ImageHelper.CPiSavedFileType fileType)
        {
            var policy = GetPolicy(system, screenCode);
            var result = await _authorizationService.AuthorizeAsync(user, policy);
            return result.Succeeded;
        }

        private static string GetPolicy(string system, string screenCode)
        {
            switch ((screenCode ?? "").ToLower())
            {
                case "rms":
                    return RMSAuthorizationPolicy.CanAccessSystem;
            }

            switch ((system ?? "").ToLower())
            {
                case "patent":
                    return PatentAuthorizationPolicy.CanAccessSystem;
                case "trademark":
                    return TrademarkAuthorizationPolicy.CanAccessSystem;
                case "generalmatter":
                    return GeneralMatterAuthorizationPolicy.CanAccessSystem;
                case "dms":
                    return DMSAuthorizationPolicy.CanAccessSystem;
                case "clearance":
                    return SearchRequestAuthorizationPolicy.CanAccessSystem;
                case "patclearance":
                    return PatentClearanceAuthorizationPolicy.CanAccessSystem;
                default:
                    return SharedAuthorizationPolicy.CanAccessSystem;
            }
        }
    }
}
