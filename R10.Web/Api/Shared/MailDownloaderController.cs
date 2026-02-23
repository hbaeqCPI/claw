using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using OpenIddict.Validation.AspNetCore;
using Polly;
using R10.Core.Entities;
using R10.Core.Entities.Shared;
using R10.Core.Interfaces;
using R10.Web.Extensions;
using R10.Web.Filters;
using R10.Web.Security;
using R10.Web.Services;
using System.Net.Http.Headers;

namespace R10.Web.Api.Shared
{
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    [ApiController]
    public class MailDownloaderController : ControllerBase
    {
        private readonly ISystemSettings<DefaultSetting> _settings;
        private readonly ILogger<MailDownloaderController> _logger;

        public MailDownloaderController(
            ISystemSettings<DefaultSetting> settings, 
            ILogger<MailDownloaderController> logger)
        {
            _settings = settings;
            _logger = logger;
        }

        [HttpGet]
        [ServiceFilter(typeof(MailAuthorizationFilter))]
        public async Task<IActionResult> DownloadEmails(string mailbox)
        {
            var settings = await _settings.GetSetting();

            // Get token
            var authToken = HttpContext.Request.GetBearerToken();
            if (string.IsNullOrEmpty(authToken))
                return Unauthorized();

            try
            {
                using (var httpClient = new HttpClient())
                {
                    // Controllers must allow OpenId authentication scheme:
                    // Authorize(AuthenticationSchemes = $"Identity.Application,{OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme}")
                    var mailDownloaderController = settings.DocumentStorage == DocumentStorageOptions.SharePoint ? "SharePointGraph" :
                                                   settings.DocumentStorage == DocumentStorageOptions.iManage ? "iManageWork" :
                                                   settings.DocumentStorage == DocumentStorageOptions.NetDocuments ? "NetDocs" : "DocDocuments";
                    var area = settings.DocumentStorage == DocumentStorageOptions.iManage ||
                               settings.DocumentStorage == DocumentStorageOptions.NetDocuments ? "" : "Shared";
                    var requestUri = Url.Action("DownloadEmails", mailDownloaderController, new { area = area, mailbox = mailbox }, protocol: Request.Scheme);

                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
                    using (var response = await httpClient.GetAsync(requestUri))
                    {
                        if (!response.IsSuccessStatusCode)
                        {
                            var error = await response.GetErrorMessage();
                            _logger.LogError("Mail Downloader API error: {Error}", error);
                            return BadRequest(error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }

            return Ok();
        }
    }
}
