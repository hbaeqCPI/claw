using OpenIddict.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LawPortal.Web.Models.AuthorizationViewModels
{
    public class AuthorizeViewModel
    {
        [Display(Name = "Application")]
        public string ApplicationName { get; set; }

        [BindNever]
        public string RequestId { get; set; }

        [BindNever]
        public IEnumerable<KeyValuePair<string, OpenIddictParameter>> Parameters { get; set; }

        [Display(Name = "Scope")]
        public string Scope { get; set; }
    }
}
