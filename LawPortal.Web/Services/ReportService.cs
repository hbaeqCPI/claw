using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using LawPortal.Web.Areas;
using LawPortal.Web.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.ServiceModel;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using RS2005;

namespace LawPortal.Web.Services
{
    public class ReportService : IReportService
    {
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _httpCtx;
        private readonly IHttpClientFactory _clientFactory;

        public ReportService(IConfiguration config, IHttpContextAccessor httpCtx, IHttpClientFactory clientFactory)
        {
            _config = config;
            _httpCtx = httpCtx;
            _clientFactory = clientFactory;
        }

        public async Task<IActionResult> GetReport(object obj, ReportType rt)
        {
            try
            {
                var token = await GetAccessTokenAsync();
                var bytes = await RenderReportAsync(rt, obj, token);
                var format = GetFormat(obj);
                var (mime, ext) = FormatMeta(format);
                return new FileContentResult(bytes, mime)
                {
                    FileDownloadName = $"{rt}.{ext}"
                };
            }
            catch (Exception ex)
            {
                return new ContentResult
                {
                    Content = $"Report error: {ex.Message}",
                    ContentType = "text/plain",
                    StatusCode = 500
                };
            }
        }

        // ── JWT acquisition ────────────────────────────────────────────────────
        // POST to the local connect/token endpoint, forwarding the auth cookie so
        // the AuthorizationController's cookie-authenticated branch runs and skips
        // the password check.
        private async Task<string> GetAccessTokenAsync()
        {
            var ctx = _httpCtx.HttpContext!;
            var username = ctx.User.Identity?.Name ?? throw new InvalidOperationException("No authenticated user.");

            var req = ctx.Request;
            var baseUrl = $"{req.Scheme}://{req.Host}";
            var tokenUrl = $"{baseUrl}/connect/token";

            var cookieHeader = string.Join("; ",
                req.Cookies.Select(c => $"{c.Key}={c.Value}"));

            var body = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string,string>("grant_type", "password"),
                new KeyValuePair<string,string>("username", username),
                new KeyValuePair<string,string>("password", "_"),
            });

            var client = _clientFactory.CreateClient("report-token");
            using var httpReq = new HttpRequestMessage(HttpMethod.Post, tokenUrl) { Content = body };
            if (!string.IsNullOrEmpty(cookieHeader))
                httpReq.Headers.Add("Cookie", cookieHeader);

            using var resp = await client.SendAsync(httpReq);
            var json = await resp.Content.ReadAsStringAsync();
            if (!resp.IsSuccessStatusCode)
                throw new InvalidOperationException($"Token endpoint returned {resp.StatusCode}: {json}");

            using var doc = JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty("access_token", out var tok))
                throw new InvalidOperationException($"No access_token in response: {json}");
            return tok.GetString()!;
        }

        // ── SSRS rendering ────────────────────────────────────────────────────
        private async Task<byte[]> RenderReportAsync(ReportType rt, object vm, string token)
        {
            var reportUrl  = _config["Report:ReportServiceUrl"]
                ?? "http://localhost/ReportServer/ReportExecution2005.asmx";
            var folder     = (_config["Report:ClientFolder"] ?? "/CLIENTNAME/Reports/").TrimEnd('/');
            var reportPath = $"{folder}/{rt}";

            var binding  = new BasicHttpBinding
            {
                MaxBufferSize         = int.MaxValue,
                MaxReceivedMessageSize= int.MaxValue,
                AllowCookies          = true,
            };
            // Switch to HTTPS transport security when the endpoint is https.
            if (reportUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                binding.Security = new BasicHttpSecurity { Mode = BasicHttpSecurityMode.Transport };

            var endpoint = new EndpointAddress(reportUrl.Replace("?wsdl", ""));
            var client   = new ReportExecutionServiceSoapClient(binding, endpoint);

            // Windows/NTLM credentials when configured.
            if (_config.GetValue<bool>("Report:UseNtlmAuthentication"))
            {
                var user   = _config["Report:UserName"];
                var pass   = _config["Report:Password"];
                var domain = _config["Report:Domain"];
                if (!string.IsNullOrEmpty(user))
                    client.ClientCredentials.Windows.ClientCredential =
                        new System.Net.NetworkCredential(user, pass, domain);
            }

            var trusted = new TrustedUserHeader();

            // 1. Load report
            var loadResp = await client.LoadReportAsync(trusted, reportPath, null);
            var execHeader = loadResp.ExecutionHeader;

            // 2. Build parameters
            var parameters = BuildParameters(vm, token);

            // 3. Set parameters
            await client.SetExecutionParametersAsync(
                execHeader,
                trusted,
                parameters.ToArray(),
                "en-us");

            // 4. Render
            var format    = GetFormat(vm);
            var formatStr = FormatString(format);
            var renderResp = await client.RenderAsync(
                new RenderRequest
                {
                    ExecutionHeader   = execHeader,
                    TrustedUserHeader = trusted,
                    Format            = formatStr,
                    DeviceInfo        = null,
                });

            return renderResp.Result;
        }

        // ── Parameter building ────────────────────────────────────────────────
        // Builds SSRS ParameterValue list from the view model via reflection, plus
        // the required `token` parameter.
        private static List<ParameterValue> BuildParameters(object vm, string token)
        {
            var list = new List<ParameterValue>
            {
                new ParameterValue { Name = "token", Value = token }
            };

            foreach (var prop in vm.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                // Skip the inherited base properties that are not report data params.
                if (prop.Name == "ReportFormat" || prop.Name == "LanguageCode")
                    continue;
                var val = prop.GetValue(vm);
                if (val == null) continue;
                list.Add(new ParameterValue
                {
                    Name  = prop.Name,
                    Value = val is bool b ? (b ? "true" : "false") : val.ToString()
                });
            }

            return list;
        }

        // ── Helpers ────────────────────────────────────────────────────────────
        private static int GetFormat(object vm)
        {
            var prop = vm.GetType().GetProperty("ReportFormat");
            return prop?.GetValue(vm) is int f ? f : 0;
        }

        private static string FormatString(int format) => format switch
        {
            1 => "EXCELOPENXML",
            2 => "WORDOPENXML",
            _ => "PDF",
        };

        private static (string mime, string ext) FormatMeta(int format) => format switch
        {
            1 => ("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "xlsx"),
            2 => ("application/vnd.openxmlformats-officedocument.wordprocessingml.document", "docx"),
            _ => ("application/pdf", "pdf"),
        };
    }
}
