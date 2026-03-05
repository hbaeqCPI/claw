using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using R10.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace R10.Web.Filters
{
    //SSRS CANNOT PASS HTTP HEADERS WHEN CONNECTING TO A WEB API
    //USE QUERY STRING TO PASS AUTHORIZATION AND ACCEPT HEADERS:
    //https://r9/api/endpoint?t={token}&type=XML
    //USED SAVED TOKEN TO AVOID EXCEEDING QUERYSTRING MAX LENGTH:
    //https://r9/api/endpoint?t={tokenId}&type=XML
    public class RequestHeaderFilter : Attribute, IAsyncAuthorizationFilter
    {
        //protected readonly IReportParameterService _reportParametersService;

        public RequestHeaderFilter(/*IReportParameterService reportParametersService*/)
        {
            //_reportParametersService = reportParametersService;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var token = context.HttpContext.Request.Query["t"];
            var accept = context.HttpContext.Request.Query["type"];
            var language = context.HttpContext.Request.Query["lang"];

            if (string.IsNullOrEmpty(token))
                token = context.HttpContext.Request.Query["token"];

            //get tokenId from querystring
            var tokenId = context.HttpContext.Request.Query["tokenId"];

            //assume 36-char token value is tokenId if tokenId is not found
            if (string.IsNullOrEmpty(tokenId) && !string.IsNullOrEmpty(token) && token.ToString().Length == 36)
                tokenId = token;

            //retrieve auth token from saved params using tokenId
            // IReportParameterService removed during debloat
            //if (!string.IsNullOrEmpty(tokenId))
            //{
            //    token = await _reportParametersService.GetParameter<string>(tokenId);
            //    await _reportParametersService.DeleteParameter(tokenId);
            //}

            if (string.IsNullOrEmpty(context.HttpContext.Request.Headers["Authorization"]) && !string.IsNullOrEmpty(token))
                context.HttpContext.Request.Headers["Authorization"] = $"Bearer {token}";

            if (!string.IsNullOrEmpty(accept) && accept.ToString().ToUpper() == "XML")
                context.HttpContext.Request.Headers["Accept"] = "application/xml";

            if (!string.IsNullOrEmpty(language))
                context.HttpContext.Request.Headers["Accept-Language"] = language;

            if (context.HttpContext.Request.Headers["Accept-Language"].Count > 0)
                Thread.CurrentThread.CurrentCulture = new CultureInfo(context.HttpContext.Request.Headers["Accept-Language"][0]);
        }
    }
}
