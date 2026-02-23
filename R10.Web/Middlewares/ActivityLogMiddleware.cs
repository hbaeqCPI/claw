using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using R10.Core.Entities;
using R10.Core.Helpers;
using R10.Core.Interfaces;
using R10.Web.Services;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace R10.Web.MiddleWares
{
    public class ActivityLogMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _errorLogger;

        public ActivityLogMiddleware(RequestDelegate next,
            ILogger<ActivityLogMiddleware> errorLogger)
        {
            _next = next;
            _errorLogger = errorLogger;
        }

        public async Task Invoke(HttpContext context, 
            ILoggerService<ActivityLog> activityLogger,
            IOptions<ActivityLogSettings> activityLogSettings )
        {
            try
            {
                await _next(context);
            }
            finally
            {
                try
                {
                    var settings = activityLogSettings.Value;
                    var request = context.Request;
                    var path = request.Path.Value ?? "";
                    var userAgent = request.Headers["User-Agent"].ToString();
                    var xff = request.Headers["X-Forwarded-For"].ToString();

                    if (settings.Enabled && !settings.ExcludePaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)) && !string.Equals(userAgent, "AlwaysOn", StringComparison.OrdinalIgnoreCase))
                    {
                        var hostName = Dns.GetHostName();
                        var ipHostEntry = Dns.GetHostEntry(hostName);

                        var activity = new ActivityLog()
                        {
                            ActivityDate = DateTime.Now,
                            UserId = context.User.GetEmail(),
                            HostName = hostName,
                            HostIP = string.Join(",", (object[])ipHostEntry.AddressList),
                            RequestUrl = UriHelper.GetDisplayUrl(request),
                            RequestMethod = request.Method,
                            StatusCode = context.Response?.StatusCode,
                            RemoteAddress = !string.IsNullOrEmpty(xff) ? xff : request.HttpContext.Connection.RemoteIpAddress?.ToString(),
                            UserAgent = userAgent,
                            AcceptLanguage = request.Headers["accept-language"].ToString()
                        };

                        if (request.HasFormContentType)
                        {
                            var form = request.Form.ToDictionary(f => f.Key, f => f.Value.ToString());

                            //hide password
                            var sensitiveData = form.Where(f => f.Key.ToLower().Contains("password")).ToList();
                            foreach (var data in sensitiveData)
                            {
                                form[data.Key] = "****";
                            }

                            activity.RequestForm = JsonConvert.SerializeObject(form);
                        }

                        await activityLogger.Add(activity);
                    }
                }
                catch (Exception e)
                {
                    _errorLogger.LogError(e.Message);
                }
            }
        }
    }
}
