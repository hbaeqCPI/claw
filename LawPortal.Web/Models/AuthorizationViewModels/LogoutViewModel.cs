using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace LawPortal.Web.Models.AuthorizationViewModels
{
    public class LogoutViewModel
    {
        [BindNever]
        public string RequestId { get; set; }

        [BindNever]
        public string PostLogoutRedirectUri { get; set; }           // pass this to redirect to proper client page
    }
}
