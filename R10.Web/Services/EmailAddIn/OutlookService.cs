using Newtonsoft.Json;
using OpenIddict.Core;
using R10.Core.DTOs;
using R10.Core.Entities.Shared;
using R10.Core.Interfaces;
using R10.Web.Interfaces;
using R10.Web.Models.EmailAddInModels;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using OpenIddict.EntityFrameworkCore.Models;
using OpenIddict.Abstractions;
using R10.Core.Helpers;
using R10.Web.Extensions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace R10.Web.Services.EmailAddIn
{
    public class OutlookService : IOutlookService
    {
        protected readonly ISystemSettings<DefaultSetting> _settings;
        static readonly HttpClient _httpClient = new HttpClient();

        private readonly string defaultBaseUrl = "https://outlook.office.com/api/v2.0/me";
        private readonly OpenIddictApplicationManager<OpenIddictEntityFrameworkCoreApplication> _applicationManager;
        private readonly IConfiguration _config;
        public OutlookService(ISystemSettings<DefaultSetting> settings, OpenIddictApplicationManager<OpenIddictEntityFrameworkCoreApplication> applicationManager, IConfiguration config)
        {
            _settings = settings;
            _applicationManager = applicationManager;
            _config = config;
        }

        public async Task<OutlookEmail> GetEmailMessage(string olItemId, string accessToken)
        {
            var settings = await _settings.GetSetting();
            var apiBaseUrl = settings.OutlookApiBaseUrl;
            if(string.IsNullOrEmpty(apiBaseUrl))
            {
                apiBaseUrl = defaultBaseUrl;
            }
                
            var url = $"{apiBaseUrl}/messages/{olItemId}";

            var message = await GetOutlookData<OutlookEmail>(url, accessToken);
            return message;
        }

        public async Task<List<OutlookAttachment>> GetEmailAttachments(string olItemId, string[] attachmentIDs, string accessToken)
        {
            var settings = await _settings.GetSetting();
            var apiBaseUrl = settings.OutlookApiBaseUrl;
            if (string.IsNullOrEmpty(apiBaseUrl))
            {
                apiBaseUrl = defaultBaseUrl;
            }
            var attachBaseUrl = $"{apiBaseUrl}/messages/{olItemId}/attachments/";

            var attachments = new List<OutlookAttachment>();

            foreach (var item in attachmentIDs)
            {
                var attachUrl = $"{attachBaseUrl}{item}";
                var attachment = await GetOutlookData<OutlookAttachment>(attachUrl, accessToken);
                attachments.Add(attachment);
            }
            return attachments;
        }


        private async Task<T> GetOutlookData<T>(string url, string accessToken)
        {
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            var data = await response.Content.ReadAsStreamAsync();

            //System.Text.Json DEPENDENCIES BREAK EXISTING CODE. 
            //USE JsonTextReader TO DESERIALIZE STREAM.
            //return await JsonSerializer.DeserializeAsync<T>(data);        // note, newtonsoft does not have a stream deserializer

            var serializer = new JsonSerializer();
            using (var sr = new StreamReader(data))
            using (var jsonTextReader = new JsonTextReader(sr))
            {
                return serializer.Deserialize<T>(jsonTextReader);
            }

        }

        public async Task<RegisterClientResult> RegisterOutlookAddInClient(string email, string clientSecret)
        {
            try
            {
                var clientCode = await SystemSettingsExtensions.GetMainCPIClientCode(_settings);
                var clientIdOrig = email.Split('@')[0].ToLower().Left(20) + clientCode.ToLower().Left(20);
                var domain = email.Split('@')[1].Replace(".", string.Empty).ToLower();
                var clientId = clientIdOrig;
                var baseURL = _config.GetValue<string>("EmailAddIn:OutlookOriginUrl");
                baseURL = baseURL[baseURL.Length - 1] == '/' ? baseURL : baseURL + '/';
                var loginURL = baseURL + "public/login.html";
                var loginCallbackURL = baseURL + "public/login-callback.html";
                var silentURL = baseURL + "public/silent-renew.html";

                if (await _applicationManager.FindByClientIdAsync(clientIdOrig) != null)
                {
                    clientId = clientIdOrig + domain;
                }

                if (await _applicationManager.FindByClientIdAsync(clientId) == null)
                {
                    await _applicationManager.CreateAsync(new OpenIddictApplicationDescriptor
                    {
                        ClientId = clientId,
                        ClientSecret = clientSecret,
                        DisplayName = clientId + " client application",
                        Permissions =
                    {
                        Permissions.Endpoints.Authorization,
                        Permissions.Endpoints.Logout,
                        Permissions.Endpoints.Token,
                        Permissions.GrantTypes.AuthorizationCode,
                        Permissions.GrantTypes.RefreshToken,
                        Permissions.Scopes.Email,
                        Permissions.Scopes.Profile,
                        Permissions.Scopes.Roles,
                        Permissions.Prefixes.Scope + "api",
                        Permissions.ResponseTypes.Code
                    },
                        RedirectUris =
                    {
                        new Uri(loginURL),
                        new Uri(loginCallbackURL),
                        new Uri(silentURL)
                    },
                        PostLogoutRedirectUris =
                    {
                        new Uri(baseURL)
                    }
                    });

                    return new RegisterClientResult { Success = true, ClientIdSecret = new Tuple<string, string>(clientId, clientSecret) };
                }
                else
                {
                    return new RegisterClientResult { Success = false };
                }
            }
            catch (Exception e)
            {
                return new RegisterClientResult { Success = false };
            }

        }

        public async Task<bool> DeleteOutlookAddInClient(string clientId)
        {
            try
            {
                var app = await _applicationManager.FindByClientIdAsync(clientId);
                if (app != null)
                {
                    await _applicationManager.DeleteAsync(app);
                }
                return true;
            }
            catch (Exception e)
            {
                return false;
            }

        }
    }

    public class RegisterClientResult
    {
        public bool Success { get; set; }
        public Tuple<string, string> ClientIdSecret { get; set; }
    }
}
