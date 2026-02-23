using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using R10.Web.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using R10.Web.Helpers;

namespace R10.Web.MiddleWares
{
    public class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;
        public SecurityHeadersMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            //https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Content-Security-Policy
            //  context.Response.Headers.Add("Content-Security-Policy","default-src 'self'; script-src 'self';");

            //https://www.acunetix.com/vulnerabilities/web/insecure-referrer-policy/
            context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");

            await _next(context);
        }
        


    }


}


