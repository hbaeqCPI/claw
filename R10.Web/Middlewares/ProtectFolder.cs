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
    // Reference: https://odetocode.com/blogs/scott/archive/2015/10/06/authorization-policies-and-middleware-in-asp-net-5.aspx

    public class ProtectFolder
    {
        private readonly RequestDelegate _next;
        private readonly PathString _path;
        private readonly string _policyName;

        private struct FileInfo
        {
            public string system;
            public string fileName;
        }

        public ProtectFolder(RequestDelegate next, ProtectFolderOptions options)
        {
            _next = next;
            _path = options.Path;
            _policyName = options.PolicyName;
        }

        public async Task Invoke(HttpContext context,
                            IAuthorizationService authorizationService)
        {
            //if (context.Request.Path.StartsWithSegments(_path))
            if (context.Request.Path.ToString().Contains("UserFiles/Images"))
            {

                // extract system & path from request path
                var requestPath = context.Request.Path.ToString();
                var fileInfo = ExtractFileInfo(requestPath);

                // check if user has permission to the specified path
                var policyName = GetPolicy(fileInfo.system);
                var authorized = await authorizationService.AuthorizeAsync(context.User, policyName);
                if (!authorized.Succeeded)
                {
                    // no permission
                    string response = GenerateResponse(context);
                    context.Response.ContentType = GetContentType();
                    await context.Response.WriteAsync(response);
                    return;
                }
                else
                {
                   // has permission
                    var imagePath = ImageHelper.GetPhysicalFilePath(fileInfo.system, fileInfo.fileName, ImageHelper.CPiSavedFileType.Image);

                    // Check if File exists
                    if (!System.IO.File.Exists(imagePath))
                    {
                        context.Response.ContentType = GetContentType();
                        await context.Response.WriteAsync("File Not Found.");
                        return;
                    }
                }

            }

            await _next(context);
        }
        
        private string GenerateResponse(HttpContext context)
        {
            return "Access denied.";
        }

        private string GetContentType()
        {
            return "text/plain";
        }

        private string GetPolicy(string path)
        {
            path = path.ToLower();

            switch (path)
            {
                case "patent": return PatentAuthorizationPolicy.CanAccessSystem; 
                case "trademark": return PatentAuthorizationPolicy.CanAccessSystem; 
                case "generalmatter": return PatentAuthorizationPolicy.CanAccessSystem; 
                case "dms": return PatentAuthorizationPolicy.CanAccessSystem; 
                case "ids": return PatentAuthorizationPolicy.CanAccessSystem; 
                default: return SharedAuthorizationPolicy.CanAccessSystem;
            }

            //if (path.Contains("patent"))
            //{
            //    return PatentAuthorizationPolicy.CanAccessSystem;
            //}
            //else if (path.Contains("trademark"))
            //{
            //    return TrademarkAuthorizationPolicy.CanAccessSystem;
            //}
            //else if (path.Contains("generalmatter"))
            //{
            //    return GeneralMatterAuthorizationPolicy.CanAccessSystem;
            //}
            //else if (path.Contains("dms"))
            //{
            //    return DMSAuthorizationPolicy.CanAccessSystem;
            //}
            //if (path.Contains("ids"))
            //{
            //    return IDSAuthorizationPolicy.CanAccessSystem;
            //}
            //else
            //{
            //    return SharedAuthorizationPolicy.CanAccessSystem;
            //}
        }

        
        private FileInfo ExtractFileInfo(string requestPath)
        {
            FileInfo fileInfo = new FileInfo();

            var pathArray = requestPath.Split("/");
            var eltCount = pathArray.Length;

            if (eltCount > 0 ) fileInfo.fileName = pathArray[eltCount - 1].Trim();
            if (eltCount > 1 ) fileInfo.system = pathArray[eltCount - 2].Trim();

            if (string.IsNullOrEmpty(fileInfo.system)) fileInfo.system = "Shared";
            return fileInfo; 
        }

        //private string GetFileName(string path)
        //{
        //    var pathArray = path.Split("/");
        //    var fileName = pathArray[pathArray.Length - 1];

        //    return fileName.Trim();
        //}

        //private string GetSystem(string path)
        //{
        //    path = path.ToLower();

        //    if (path.Contains("patent"))
        //    {
        //        return "Patent";
        //    }
        //    else if (path.Contains("trademark"))
        //    {
        //        return "Trademark";
        //    }
        //    else if (path.Contains("generalmatter"))
        //    {
        //        return "GeneralMatter";
        //    }
        //    else if (path.Contains("dms"))
        //    {
        //        return "DMS";
        //    }
        //    if (path.Contains("ids"))
        //    {
        //        return "IDS";
        //    }
        //    else
        //    {
        //        return "Shared";
        //    }
        //}

    }

    public class ProtectFolderOptions
    {
        public PathString Path { get; set; }
        public string PolicyName { get; set; }
    }

    public static class ImageMiddlewareExtensions
    {
        public static IApplicationBuilder UseProtectFolder(this IApplicationBuilder builder, ProtectFolderOptions options)
        {
            return builder.UseMiddleware<ProtectFolder>(options);
        }
    }
}


