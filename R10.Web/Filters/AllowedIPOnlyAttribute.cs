using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using R10.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using R10.Core.Interfaces;
using System.Data.SqlClient;
using R10.Core.Entities;
using System.Text.Encodings.Web;
using Microsoft.Extensions.Configuration;

namespace R10.Web.Filters
{
    public class AllowedIPOnlyAttribute: Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var configuration = (IConfiguration)context.HttpContext.RequestServices.GetService(typeof(IConfiguration));
            var allowedIPs = configuration["Report:AllowedIPPrefix"].Split(',');
            var clientIP = context.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            if (!allowedIPs.Any(a=> clientIP.StartsWith(a)))
                context.Result = new UnauthorizedResult();
        }
    }

    
}
