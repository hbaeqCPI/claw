using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using R10.Core.Entities;
using R10.Core.Entities.Shared;
using R10.Core.Helpers;
using R10.Core.Identity;
using R10.Core.Interfaces;
using R10.Web.Extensions;

namespace R10.Web.Controllers
{
    public class BulletinController : Controller
    {
        private readonly ILogger _logger;
        private readonly ISystemSettings<DefaultSetting> _defaultSettings;
        private readonly ICPiSystemSettingManager _systemSettingManager;

        public BulletinController(ILogger<BulletinController> logger,
            ISystemSettings<DefaultSetting> defaultSettings,
            ICPiSystemSettingManager systemSettingManager)
        {
            _logger = logger;
            _defaultSettings = defaultSettings;
            _systemSettingManager = systemSettingManager;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetLoginMessage()
        {
            var settings = await _defaultSettings.GetSetting();
            var apiUrl = GetBulletinUri(settings.LoginMessageApiUrl);
            var message = settings.LoginMessage;

            if (!string.IsNullOrEmpty(apiUrl))
            {
                using (var httpClient = new HttpClient())
                {
                    using (var response = await httpClient.GetAsync(apiUrl))
                    {
                        if (response.IsSuccessStatusCode)
                            message = await response.Content.ReadAsStringAsync();
                        else
                        {
                            var error = await response.GetErrorMessage();
                            _logger.LogError("Bulletin API error: {Error}", error);
                            return BadRequest(error);
                        }
                    }
                }
            }

            return Ok(message);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetNotifications()
        {
            var settings = await _defaultSettings.GetSetting();
            var apiUrl = GetBulletinUri(settings.NotificationsApiUrl);
            var statuses = new List<string>();
            var systemStatus = await GetSystemStatus();

            if (systemStatus != null && systemStatus.Notification != null && systemStatus.Notification.Active && !string.IsNullOrEmpty(systemStatus.Notification.Message))
                statuses.Add(systemStatus.Notification.Message.Trim());

            if (!string.IsNullOrEmpty(apiUrl))
            {
                using (var httpClient = new HttpClient())
                {
                    using (var response = await httpClient.GetAsync(apiUrl))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            var apiResponse = await response.Content.ReadAsStringAsync();
                            var notifications = JsonConvert.DeserializeObject<List<string>>(apiResponse);
                            if (notifications != null)
                                statuses.AddRange(notifications);
                        }
                        else
                        {
                            var error = await response.GetErrorMessage();
                            _logger.LogError("Bulletin API error: {Error}", error);
                            return BadRequest(error);
                        }
                    }
                }
            }

            return Ok(statuses);
        }

        /// <summary>
        /// Append application base url to apiUrl querystring
        /// </summary>
        /// <param name="apiUrl"></param>
        /// <returns></returns>
        private string? GetBulletinUri(string? apiUrl)
        {
            if (string.IsNullOrEmpty(apiUrl))
                return apiUrl;

            var uriBuilder = new UriBuilder(apiUrl);
            var queryString = uriBuilder.Uri.ParseQueryString();
            if (!queryString.AllKeys.Any(k => (k ?? "").ToLower() == "appid"))
            {
                queryString["appid"] = Url.ApplicationBaseUrl(Request.Scheme);
                uriBuilder.Query = queryString.ToString();
            }

            return uriBuilder.ToString();
        }

        private async Task<SystemStatus> GetSystemStatus()
        {
            var systemStatus = await _systemSettingManager.GetSystemSetting<SystemStatus>("");
            User.SetSystemStatus(systemStatus.StatusType);
            return systemStatus;
        }
    }
}
