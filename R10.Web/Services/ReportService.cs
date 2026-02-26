using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using R10.Core.Entities;
using R10.Web.Areas;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Helpers;
using RS2005;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.ServiceModel;
using System.Threading.Tasks;
using R10.Web.Interfaces;
using Microsoft.AspNetCore.Http;
using R10.Core.Interfaces;
using R10.Core.Entities.Patent;
using R10.Core.Entities.Trademark;
// using R10.Core.Entities.GeneralMatter; // Removed during deep clean
using R10.Core.Entities.Shared;
using Microsoft.Extensions.Localization;
using R10.Web.Models;
using R10.Core.Helpers;
using System.Globalization;
using System.Threading;
using R10.Core.Identity;
using R10.Web.Security;
using Microsoft.AspNetCore.Authorization;
using System.Net.Mail;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;
using System.Data;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;
using Newtonsoft.Json;

namespace R10.Web.Services
{
    public class ReportService : IReportService
    {
        private readonly ReportSettings _reportSettings;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private string errorMessage = "Failed to print report. Please limit your criteria.";
        private string unhandledErrorMessage = "Unhandled error.";
        private readonly ISystemSettings<PatSetting> _patSettings;
        private readonly ISystemSettings<TmkSetting> _tmkSettings;
        private readonly ISystemSettings<DefaultSetting> _defaultSettings;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly IAuthorizationService _authorizationService;
        private readonly IEmailSender _emailSender;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IConfiguration _configuration;
        private readonly ICPiUserSettingManager _userSettingManager;
        private readonly IReportParameterService _reportParametersService;
        private readonly ServiceAccount _serviceAccount;

        public ReportService(IOptions<ReportSettings> reportSettings, IHttpContextAccessor httpContextAccessor, ISystemSettings<PatSetting> patSettings,
            ISystemSettings<TmkSetting> tmkSettings, ISystemSettings<DefaultSetting> defaultSettings, IStringLocalizer<SharedResource> localizer,
            IAuthorizationService authorizationService, IEmailSender emailSender, IHostingEnvironment hostingEnvironment,
            IConfiguration configuration, 
            ICPiUserSettingManager userSettingManager,
            IReportParameterService reportParametersService,
            IOptions<ServiceAccount> serviceAccount)
        {
            _reportSettings = reportSettings.Value;
            _httpContextAccessor = httpContextAccessor;
            _patSettings = patSettings;
            _tmkSettings = tmkSettings;
            _defaultSettings = defaultSettings;
            _localizer = localizer;
            _authorizationService = authorizationService;
            _emailSender = emailSender;
            _hostingEnvironment = hostingEnvironment;
            _configuration = configuration;
            _userSettingManager = userSettingManager;
            _reportParametersService = reportParametersService;
            _serviceAccount = serviceAccount.Value;
        }

        [HttpGet("getpdfreport")]
        public async Task<IActionResult> GetReport(Object obj, ReportType rt)
        {
            string reportName = rt.ToString();
            Dictionary<string, object> parameters = await GetReportParameters(obj, rt);

            Object type;
            parameters.TryGetValue("ReportFormat", out type);
            ReportOutputType reportOutputType;
            ReportOutputType.TryParse(type.ToString(), out reportOutputType);

            //pass language code as parameter
            //use current thread culture if empty
            //report can be in language of recipient (emailed reports)
            //instead of language of user generating it.
            CultureInfo cultureInfo;
            if (parameters.ContainsKey("LanguageCode") && !string.IsNullOrEmpty((string)parameters["LanguageCode"]))
                cultureInfo = new CultureInfo((string)parameters["LanguageCode"], false);
            else
                cultureInfo = Thread.CurrentThread.CurrentCulture;

            parameters.Add("SettingLanguageCode", cultureInfo.Name);
            parameters.Add("SettingDecimalSeperator", cultureInfo.NumberFormat.NumberDecimalSeparator);

            string reportFileName = obj.GetType() == typeof(CustomReportDetailViewModel) ? "CR_" +(obj as CustomReportDetailViewModel).ReportId.ToString() : rt.ToString();

            //TODO: BETTER ERROR HANDLING FOR EASIER DEBUGGING
            try
            {
                RenderResponse reportContent = await this.RenderReport(reportFileName, parameters, cultureInfo.Name, reportOutputType, obj.GetType() == typeof(CustomReportDetailViewModel));

                MemoryStream stream = new MemoryStream(reportContent.Result);

                FileStreamResult fsr = new FileStreamResult(stream, reportContent.MimeType);

                //if (reportOutputType == ReportOutputType.Excel)
                //    fsr.FileDownloadName = rt.ToString() + ".xls";
                //else if (reportOutputType == ReportOutputType.CSV)
                //    fsr.FileDownloadName = rt.ToString() + ".csv";
                //else if (reportOutputType == ReportOutputType.image)
                //{
                //    fsr.FileDownloadName = rt.ToString() + ".tif";
                //}
                if (reportOutputType == ReportOutputType.HTML)
                {
                    var fileStream = fsr.FileStream;
                    StreamReader sr = new StreamReader(fileStream);
                    //for images
                    var contents = sr.ReadToEnd();
                    var reportServiceUrl = contents.Substring(contents.IndexOf("<body"));
                    reportServiceUrl = reportServiceUrl.Substring(reportServiceUrl.IndexOf("SRC=\"")+5);
                    reportServiceUrl = reportServiceUrl.Substring(0, reportServiceUrl.IndexOf("/ReportServer")+14);

                    var reportServiceFullUrl = "";

                    if (_hostingEnvironment.IsDevelopment())
                        reportServiceFullUrl = reportServiceUrl;
                    else
                        reportServiceFullUrl = reportServiceUrl.Substring(0, reportServiceUrl.IndexOf("/ReportServer")) + _reportSettings.ReportServerDomain + reportServiceUrl.Substring(reportServiceUrl.IndexOf("/ReportServer"));

                    var replacedUrl = _httpContextAccessor.HttpContext.Request.Scheme + "://" + _httpContextAccessor.HttpContext.Request.Host + _httpContextAccessor.HttpContext.Request.PathBase + "/FileViewer/GetSSRSHtmlImage?ReportServerUrl=" + reportServiceFullUrl.Substring(0, reportServiceFullUrl.Length - 1);
                    // SRC = "
                    //for images
                    contents = contents.Replace(
                        reportServiceUrl,
                        replacedUrl)
                        ;

                    //for default images size
                    var contentsImgs = contents.Split("<IMG");
                    for (int i = 1; i < contentsImgs.Length; i++)
                    {
                        var startIndex = contentsImgs[i - 1].LastIndexOf("style=");
                        var endIndex = contentsImgs[i - 1].LastIndexOf("mm;\"");
                        if(endIndex > 0)
                        {
                            if (endIndex < startIndex)
                            {
                                startIndex = contentsImgs[i - 1].Substring(0, endIndex).LastIndexOf("style=");
                            }
                            if (endIndex < startIndex || endIndex == contentsImgs[i - 1].Length)
                                continue;
                            var style = contentsImgs[i - 1].Substring(startIndex, endIndex - startIndex + 4);
                            if (!style.Contains("min"))
                                continue;
                            style = style.Replace("min", "max");
                            contentsImgs[i] = " " + style + contentsImgs[i];
                        }
                    }
                    contents = string.Join("<IMG", contentsImgs);

                    //for fix width
                    contents = contents.Replace("100%;direction:ltr", "10.4in;direction:ltr");

                    //change hyperlink redirect to new tab
                    contents = contents.Replace("text-decoration:underline;color:White;\"", "text-decoration:underline;color:Black;\" target=\"_blank\"");
                    contents = contents.Replace("color:White;\"", "color:Black;\" target=\"_blank\"");


                    MemoryStream streamResult = new MemoryStream();
                    new StreamWriter(streamResult).Write(contents);

                    return new FileContentResult(streamResult.ToArray(), "text/html");
                }


                return fsr;
            }
            catch (Exception e)
            {
                //TODO LOG ERROR? 
                //SHOW ACTUAL ERROR MESSAGE FOR DEBUGGING (NOT NEEDED IF LOGGED)
                errorMessage = e.InnerException?.Message ?? e.Message;

                //BUBBLE UP
                throw;
            }
        }

        private async Task<Dictionary<string, object>> GetReportParameters(object obj, ReportType rt)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            if (obj.GetType() == typeof(CustomReportDetailViewModel))
            {
                parameters.Add("p1", _httpContextAccessor.HttpContext.User.GetUserIdentifier());
                parameters.Add("p2", (await _defaultSettings.GetSetting()).CustomReportAPIKey);
                parameters.Add("p4", _httpContextAccessor.HttpContext.Request.Scheme + "://" + _httpContextAccessor.HttpContext.Request.Host + _httpContextAccessor.HttpContext.Request.PathBase);
                parameters.Add("p5", _httpContextAccessor.HttpContext.Request.Scheme + "://" + _httpContextAccessor.HttpContext.Request.Host + _httpContextAccessor.HttpContext.Request.PathBase + $"/FileViewer/GetSSRSImage?imageFile=");

                parameters.Add("ReportFormat", (obj as CustomReportDetailViewModel).ReportFormat);
            }
            else
            {
                if (obj.GetType() == typeof(DataTable))
                {
                    DataTable dt = obj as DataTable;
                    DataRow dr = dt.Rows[0];

                    for (int i = 0; i < dt.Columns.Count; i++)
                    {
                        if (dt.Columns[i].ColumnName.Equals("ToDate"))
                        {
                            var value = dt.Rows[0][i] != DBNull.Value ? dt.Rows[0][i] : null;
                            if (value != null)
                            {
                                var dateTimeValue = (DateTime)value;
                                if (dateTimeValue.Second == 0)
                                {
                                    dateTimeValue = dateTimeValue.AddDays(1).AddSeconds(-1);
                                }                                
                                    parameters.Add(dt.Columns[i].ColumnName, dateTimeValue);
                            }
                        }
                        else
                        {
                            parameters.Add(dt.Columns[i].ColumnName, dt.Rows[0][i] != DBNull.Value ? dt.Rows[0][i] : null);
                        }
                    }
                }
                else
                {
                    foreach (PropertyInfo propertyInfo in obj.GetType().GetProperties())
                    {
                        if (propertyInfo.PropertyType.Name.Equals("String"))
                        {
                            var value = propertyInfo.GetValue(obj, null);

                            //workaround ssrs limitation of using querystring to connect to api:
                            //save api authentication token to database to avoid exceeding querystring max length
                            //use [ServiceFilter(typeof(RequestHeaderFilter))] attribute on api controller to retrieve saved token
                            if (propertyInfo.Name.ToLower() == "token" && value?.ToString().Length > 36)
                            {
                                var tokenId = await _reportParametersService.SaveParameter(value.ToString());

                                if (!string.IsNullOrEmpty(tokenId))
                                    value = tokenId;
                            }

                            if (value != null)
                                propertyInfo.SetValue(obj, value.ToString().Replace('*', '%').Replace('?', '_'));
                            else
                            {
                                if (propertyInfo.Name.EndsWith("Op"))
                                {
                                    propertyInfo.SetValue(obj, "eq");
                                }
                            }
                        }

                        if (propertyInfo.Name.Equals("ToDate"))
                        {
                            var value = propertyInfo.GetValue(obj, null);
                            if (value != null)
                            {
                                var dateTimeValue = (DateTime)value;
                                if (dateTimeValue.Second == 0)
                                {
                                    dateTimeValue = dateTimeValue.AddDays(1).AddSeconds(-1);
                                    propertyInfo.SetValue(obj, dateTimeValue);
                                }
                            }
                        }
                    }

                    parameters = obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).ToDictionary(prop => prop.Name, prop => prop.GetValue(obj, null));
                }

                await AddLabelParameters(parameters, rt);
                await AddSettingParameters(parameters, rt);

                //Add Report Parameters here
                parameters.Add("PatentFilePath", _httpContextAccessor.HttpContext.Request.Scheme + "://" + _httpContextAccessor.HttpContext.Request.Host + _httpContextAccessor.HttpContext.Request.PathBase + $"/FileViewer/GetSSRSImage?system={SystemType.Patent}&imageFile=");
                parameters.Add("TrademarkFilePath", _httpContextAccessor.HttpContext.Request.Scheme + "://" + _httpContextAccessor.HttpContext.Request.Host + _httpContextAccessor.HttpContext.Request.PathBase + $"/FileViewer/GetSSRSImage?system={SystemType.Trademark}&imageFile=");
                parameters.Add("GeneralFilePath", _httpContextAccessor.HttpContext.Request.Scheme + "://" + _httpContextAccessor.HttpContext.Request.Host + _httpContextAccessor.HttpContext.Request.PathBase + $"/FileViewer/GetSSRSImage?system={SystemType.GeneralMatter}&imageFile=");
                parameters.Add("ThumbnailsFilePath", _httpContextAccessor.HttpContext.Request.Scheme + "://" + _httpContextAccessor.HttpContext.Request.Host + _httpContextAccessor.HttpContext.Request.PathBase + $"/FileViewer/GetSSRSImage?system=Thumbnails&imageFile=");

                //Skip if param already has UserID
                if (!parameters.ContainsKey("UserID"))
                    parameters.Add("UserID", _httpContextAccessor.HttpContext.User.GetUserIdentifier());

                parameters.Add("LogoUrl", _httpContextAccessor.HttpContext.Request.Scheme + "://" + _httpContextAccessor.HttpContext.Request.Host + _httpContextAccessor.HttpContext.Request.PathBase + "/images/site_report_logo.png");
                parameters.Add("BaseUrl", _httpContextAccessor.HttpContext.Request.Scheme + "://" + _httpContextAccessor.HttpContext.Request.Host + _httpContextAccessor.HttpContext.Request.PathBase);
                if (_hostingEnvironment.IsDevelopment())
                    parameters.Add("ApiUrl", _reportSettings.ClientApiUrl);
                else
                    parameters.Add("ApiUrl", _httpContextAccessor.HttpContext.Request.Scheme + "://" + _httpContextAccessor.HttpContext.Request.Host + _httpContextAccessor.HttpContext.Request.PathBase + "/api");
                parameters.Add("IconDelegatedUrl", _httpContextAccessor.HttpContext.Request.Scheme + "://" + _httpContextAccessor.HttpContext.Request.Host + _httpContextAccessor.HttpContext.Request.PathBase + "/images/icon_delegated.png");
                parameters.Add("IconLicensedUrl", _httpContextAccessor.HttpContext.Request.Scheme + "://" + _httpContextAccessor.HttpContext.Request.Host + _httpContextAccessor.HttpContext.Request.PathBase + "/images/icon_licensed.png");
                parameters.Add("IconUnmappedUrl", _httpContextAccessor.HttpContext.Request.Scheme + "://" + _httpContextAccessor.HttpContext.Request.Host + _httpContextAccessor.HttpContext.Request.PathBase + "/images/icon_unmapped.png");
                parameters.Add("IconExtendedUrl", _httpContextAccessor.HttpContext.Request.Scheme + "://" + _httpContextAccessor.HttpContext.Request.Host + _httpContextAccessor.HttpContext.Request.PathBase + "/images/icon_extended.png");

                if (!parameters.ContainsKey("token"))
                {
                    var token = await GetToken(_httpContextAccessor.HttpContext.Request.Scheme + "://" + _httpContextAccessor.HttpContext.Request.Host + _httpContextAccessor.HttpContext.Request.PathBase, _serviceAccount.UserName, _serviceAccount.Password);
                    //var token = _httpContextAccessor.HttpContext.User
                    parameters.Add("token", token);
                }

            }

            return parameters;
        }

        private async Task<string> GetToken(string Baseurl, string userName, string password)
        {
            using (var client = new HttpClient())
            {
                using (var request = new HttpRequestMessage())
                {
                    request.RequestUri = new Uri(Baseurl + "/connect/token");
                    request.Method = HttpMethod.Post;

                    var body = new Dictionary<string, string>();
                    body.Add("grant_type", "password");
                    body.Add("username", userName);
                    body.Add("password", password);

                    var content = new FormUrlEncodedContent(body);
                    content.Headers.Clear();
                    content.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                    request.Content = content;

                    HttpResponseMessage response = await client.SendAsync(request);
                    var stringResponse = response.Content.ReadAsStringAsync().Result;
                    var results = JsonConvert.DeserializeObject<Dictionary<string, string>>(stringResponse);

                    var result = results.GetValueOrDefault("access_token");
                    return result;
                }
            }
        }

        public async Task<EmailReportViewModel> SaveEmailReport(Object obj, ReportType rt)
        {
            deleteGeneratedReport();

            string title = "";
            if (obj.GetType() == typeof(CustomReportDetailViewModel))
            {
                title = (obj as CustomReportDetailViewModel).ReportName;
            }
            else
            {
                ReportTitleHelper titleHelper = new ReportTitleHelper(await GetReportParameterLabels());
                title = _localizer[titleHelper.GetType().GetProperty(rt.ToString()).GetValue(titleHelper).ToString()].ToString();
                title = title.Replace("/", "_");// '/' can not use as file name.
            }

            FileStreamResult fsr = (FileStreamResult) await GetReport(obj, rt);
            
            string fileName = title + "_";
            Object rf;
            obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).ToDictionary(prop => prop.Name, prop => prop.GetValue(obj, null)).TryGetValue("ReportFormat", out rf);
            int reportFormat = (int)rf;
            fileName = GetExportFileName(fileName, reportFormat);
            var filePathName = ReportFolder(fileName);
            using (FileStream outputFileStream = new FileStream(filePathName, FileMode.Create))
            {
                fsr.FileStream.CopyTo(outputFileStream);
            }

            EmailReportViewModel emailReport = new EmailReportViewModel
            {
                Subject = title,
                CopyTo = _httpContextAccessor.HttpContext.User.Identity.Name,
                FromAddress = _httpContextAccessor.HttpContext.User.Identity.Name,
                ReplyToAddress = _httpContextAccessor.HttpContext.User.Identity.Name,
                Body = "Please see the attached report.",
                GeneratedReportName = fileName,
                EmailReportName = title + GetOutputFormatExtension(reportFormat)
            };

            return emailReport;
        }

        public IActionResult GetGeneratedReport(string generatedReportName)
        {
            var filePathName = ReportFolder(generatedReportName);
            byte[] buffer;
            using (var fileStream = new FileStream(filePathName, FileMode.Open, FileAccess.Read))
            {
                    int length = (int)fileStream.Length;  // get file length
                    buffer = new byte[length];            // create buffer
                    int count;                            // actual number of bytes read
                    int sum = 0;                          // total number of bytes read

                    // read until Read method returns 0 (end of the stream has been reached)
                    while ((count = fileStream.Read(buffer, sum, length - sum)) > 0)
                        sum += count;  // sum is a buffer offset for next reading
            }
            FileStreamResult fsr = new FileStreamResult(new MemoryStream(buffer),GetFileMimeType(generatedReportName));

            return fsr;
        }

        public async Task<EmailSenderResult> EmailReport(EmailReportViewModel emailData)
        {
            _emailSender.From = new MailAddress(emailData.FromAddress);
            _emailSender.To = GetAddresses(emailData.To);
            _emailSender.Cc = GetAddresses(emailData.CopyTo);
            _emailSender.Bcc = GetAddresses(emailData.Bcc);
            _emailSender.ReplyTo = GetAddresses(emailData.ReplyToAddress);
            FileStreamResult fsr = (FileStreamResult)GetGeneratedReport(emailData.GeneratedReportName);
            Attachment attachment = new Attachment(fsr.FileStream, emailData.EmailReportName);

            //test email result
            //EmailSenderResult result = new EmailSenderResult()
            //{
            //    Success = true,
            //    ErrorMessage = "Test Error Message"
            //};

            var result = await _emailSender.SendEmailAsync(emailData.Subject, emailData.Body, attachment);
            if(result.Success)
                deleteGeneratedReport();
            return result;
        }

        private List<MailAddress> GetAddresses(string addresses)
        {
            var newAddresses = new List<MailAddress>();

            if (addresses == null)
                return newAddresses;

            addresses = addresses.Replace(",", ";");
            foreach (var address in addresses.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (!string.IsNullOrEmpty(address.Trim()))
                    newAddresses.Add(new MailAddress(address));
            }
            return newAddresses;
        }


        /// <summary>
        /// </summary>
        /// <param name="reportName">
        ///  report name.
        /// </param>
        /// <param name="parameters">report's required parameters</param>
        /// <param name="exportFormat">value = "PDF" or "EXCEL". By default it is pdf.</param>
        /// <param name="languageCode">
        ///   value = 'en-us', 'fr-ca', 'es-us', 'zh-chs'. 
        /// </param>
        /// <returns></returns>
        private async Task<RenderResponse> RenderReport(string reportName, Dictionary<string, object> parameters, string languageCode, ReportOutputType reportOutputType, bool isCustomReport)
        {
            //
            // SSRS report path. Note: Need to include parent folder directory and report name.
            // Such as value = "/[report folder]/[report name]".
            //
            string adjustedReportName = reportName;
            if ((!reportName.Equals("PatCECostEstimatorPrintScreen") && !reportName.Equals("TmkCECostEstimatorPrintScreen")) || reportOutputType == ReportOutputType.XML || reportOutputType == ReportOutputType.CSV)
            {
                if ((reportOutputType == ReportOutputType.Excel || reportOutputType == ReportOutputType.XML || reportOutputType == ReportOutputType.CSV) && !isCustomReport)
                    adjustedReportName += "_Excel";
            }

            string reportPath = string.Format("{0}{1}", _reportSettings.ClientFolder, adjustedReportName);
            //
            // Binding setup, since ASP.NET Core apps don't use a web.config file
            //
            HttpBindingBase binding;
            if (_reportSettings.ReportServiceUrl.StartsWith("https:"))
            {
                binding = new BasicHttpsBinding(BasicHttpsSecurityMode.Transport);
                if (_reportSettings.UseNtlmAuthentication) 
                    ((BasicHttpsBinding)binding).Security.Transport.ClientCredentialType = HttpClientCredentialType.Ntlm;
                else 
                    ((BasicHttpBinding)binding).Security.Transport.ClientCredentialType = HttpClientCredentialType.Windows;
            }
            else
            {
                binding = new BasicHttpBinding(BasicHttpSecurityMode.TransportCredentialOnly);
                if (_reportSettings.UseNtlmAuthentication)
                    ((BasicHttpBinding)binding).Security.Transport.ClientCredentialType = HttpClientCredentialType.Ntlm;
                else 
                    ((BasicHttpBinding)binding).Security.Transport.ClientCredentialType = HttpClientCredentialType.Windows;
            }

            binding.MaxReceivedMessageSize = _reportSettings.MaxFileSizeInMB*1000000; //Convert MB to Bytes
            binding.CloseTimeout = _reportSettings.CloseTimeout;
            binding.OpenTimeout = _reportSettings.OpenTimeout;
            binding.ReceiveTimeout = _reportSettings.ReceiveTimeout;
            binding.SendTimeout = _reportSettings.SendTimeout;

            //Create the execution service SOAP Client
            ReportExecutionServiceSoapClient reportClient = new ReportExecutionServiceSoapClient(binding, new EndpointAddress(_reportSettings.ReportServiceUrl));

            string historyID = null;
            TrustedUserHeader trustedUserHeader = new TrustedUserHeader();
            ExecutionHeader execHeader = new ExecutionHeader();

            if (_reportSettings.UseNtlmAuthentication)
            {
                //Setup access credentials. Here use windows credentials.
                var clientCredentials = new NetworkCredential(_reportSettings.UserName, _reportSettings.Password, _reportSettings.Domain);

                reportClient.ClientCredentials.Windows.AllowedImpersonationLevel = System.Security.Principal.TokenImpersonationLevel.Impersonation;
                reportClient.ClientCredentials.Windows.ClientCredential = clientCredentials;

                trustedUserHeader.UserName = clientCredentials.UserName;
            }

            //This handles the problem of "Missing session identifier"
            //reportClient.Endpoint.EndpointBehaviors.Add(new ReportingServiceEndPointBehavior());

            //
            // Load the report
            //
            var taskLoadReport = await reportClient.LoadReportAsync(trustedUserHeader, reportPath, historyID);
            // Fixed the exception of "session identifier is missing".
            execHeader.ExecutionID = taskLoadReport.executionInfo.ExecutionID;

            //
            //Set the parameteres asked for by the report
            //
            ParameterValue[] reportParameters = null;
            if (parameters != null && parameters.Count > 0)
            {
                reportParameters = taskLoadReport.executionInfo.Parameters.Where(x => parameters.ContainsKey(x.Name) && !(parameters[x.Name] is null)).Select(x => new ParameterValue() { Name = x.Name, Value = parameters[x.Name].ToString() }).ToArray();
            }

            await reportClient.SetExecutionParametersAsync(execHeader, trustedUserHeader, reportParameters, languageCode);
            // run the report
            const string deviceInfo = @"<DeviceInfo><Toolbar>False</Toolbar></DeviceInfo>";

            string temp;

            if (reportOutputType == ReportOutputType.HTML)
            {
                //temp = "mhtml";
                temp = "html4.0";
                //temp = "html5.0";
            }
            else if (reportOutputType == ReportOutputType.Excel)
            {
                temp = "EXCELOPENXML";
            }
            else if (reportOutputType == ReportOutputType.Word)
            {
                temp = "WORDOPENXML";
            }
            else
            {
                temp = reportOutputType.ToString();
            }
            //var test = await reportClient.ListRenderingExtensionsAsync(trustedUserHeader);
            var response = await reportClient.RenderAsync(new RenderRequest(execHeader, trustedUserHeader, temp ?? "PDF", deviceInfo));

            //spit out the result
            return response;
        }

        private async Task AddLabelParameters(Dictionary<string, object> parameters, ReportType rt)
        {
            SystemTypeForReport systemType = GetSystemType(rt);
            object mainSetting = await GetSetting(systemType);
            var patSettings = await _patSettings.GetSetting();

            var labelAgent = mainSetting.GetType().GetProperty("LabelAgent").GetValue(mainSetting).ToString();
            var labelAgentName = mainSetting.GetType().GetProperty("LabelAgentName").GetValue(mainSetting).ToString();
            var labelAgentRef = mainSetting.GetType().GetProperty("LabelAgentRef").GetValue(mainSetting).ToString();
            var labelCaseNumber = systemType == SystemTypeForReport.Patent || systemType == SystemTypeForReport.Trademark || systemType == SystemTypeForReport.GeneralMatter ?
                    ((bool)mainSetting.GetType().GetProperty("IsClientMatterOn").GetValue(mainSetting) ?
                    mainSetting.GetType().GetProperty("LabelClientMatter").GetValue(mainSetting).ToString() :
                    mainSetting.GetType().GetProperty("LabelCaseNumber").GetValue(mainSetting).ToString()) :
                    mainSetting.GetType().GetProperty("LabelCaseNumber").GetValue(mainSetting).ToString();
            var labelClient = mainSetting.GetType().GetProperty("LabelClient").GetValue(mainSetting).ToString();
            var labelClientName = mainSetting.GetType().GetProperty("LabelClientName").GetValue(mainSetting).ToString();
            var labelClientRef = mainSetting.GetType().GetProperty("LabelClientRef").GetValue(mainSetting).ToString();
            var labelDisclosureNumber = systemType == SystemTypeForReport.Shared || systemType == SystemTypeForReport.DMS ?
                    mainSetting.GetType().GetProperty("LabelDisclosureNumber").GetValue(mainSetting).ToString() :
                    "Disclosure Number";
            var labelGMCaseNumber = "Case Number";
            var labelKeyword = mainSetting.GetType().GetProperty("LabelKeyword").GetValue(mainSetting).ToString();
            var labelOldCaseNumber = systemType == SystemTypeForReport.Patent || systemType == SystemTypeForReport.Trademark ?
                    mainSetting.GetType().GetProperty("LabelOldCaseNumber").GetValue(mainSetting).ToString() :
                    "Old Case Number";
            var labelOwner = mainSetting.GetType().GetProperty("LabelOwner").GetValue(mainSetting).ToString();
            var labelOwnerName = mainSetting.GetType().GetProperty("LabelOwnerName").GetValue(mainSetting).ToString();
            var labelAttorney1 = mainSetting.GetType().GetProperty("LabelAttorney1").GetValue(mainSetting).ToString();
            var labelAttorney2 = mainSetting.GetType().GetProperty("LabelAttorney2").GetValue(mainSetting).ToString();
            var labelAttorney3 = mainSetting.GetType().GetProperty("LabelAttorney3").GetValue(mainSetting).ToString();
            var labelAttorney4 = mainSetting.GetType().GetProperty("LabelAttorney4").GetValue(mainSetting).ToString();
            var labelAttorney5 = mainSetting.GetType().GetProperty("LabelAttorney5").GetValue(mainSetting).ToString();

            parameters.Add("LabelAbandon", _localizer["Abandon"].ToString());
            parameters.Add("LabelAbstract", _localizer["Abstract"].ToString());
            parameters.Add("LabelActionDue", _localizer["Action Due"].ToString());
            parameters.Add("LabelActionDues", _localizer["Action Dues"].ToString());
            parameters.Add("LabelActionRemarks", _localizer["Action Due Remarks"].ToString());
            parameters.Add("LabelAction", _localizer["Action"].ToString());
            parameters.Add("LabelActionParameters", _localizer["Action Parameters"].ToString());
            parameters.Add("LabelActions", _localizer["Actions"].ToString());
            parameters.Add("LabelActionTermMapping", _localizer["Action Term Mapping"].ToString());
            parameters.Add("LabelActionType", _localizer["Action Type"].ToString());
            parameters.Add("LabelActionTypes", _localizer["Action Types"].ToString());
            parameters.Add("LabelActive", _localizer["Active"].ToString());
            parameters.Add("LabelActiveStatus", _localizer["Active Status"].ToString());
            parameters.Add("LabelActiveSwitch", _localizer["Status Code"].ToString());
            parameters.Add("LabelActivity", _localizer["Activity"].ToString());
            parameters.Add("LabelActivityDate", _localizer["Activity Date"].ToString());
            parameters.Add("LabelActual", _localizer["Actual"].ToString());
            parameters.Add("LabelActualAmount", _localizer["Actual Amount"].ToString());
            parameters.Add("LabelAddress", _localizer["Address"].ToString());
            parameters.Add("LabelAddress1", _localizer["Address1"].ToString());
            parameters.Add("LabelAddress2", _localizer["Address2"].ToString());
            parameters.Add("LabelAddress3", _localizer["Address3"].ToString());
            parameters.Add("LabelAddress4", _localizer["Address4"].ToString());
            parameters.Add("LabelAfrica", _localizer["Africa"].ToString());
            parameters.Add("LabelAgenda", _localizer["Agenda"].ToString());
            parameters.Add("LabelAgent", _localizer[labelAgent].ToString());
            parameters.Add("LabelAgentName", _localizer[labelAgentName].ToString());
            parameters.Add("LabelAgentRef", _localizer[labelAgentRef].ToString());
            parameters.Add("LabelAgentResponsibility", _localizer[labelAgent+ " Responsibility"].ToString());
            parameters.Add("LabelAgreementType", _localizer["Agreement Type"].ToString());
            parameters.Add("LabelAll", _localizer["All"].ToString());
            parameters.Add("LabelAllowanceDate", _localizer["Allowance Date"].ToString());
            parameters.Add("LabelAllowOverride", _localizer["Allow Override"].ToString());
            parameters.Add("LabelAllStatus", _localizer["All Status"].ToString());
            parameters.Add("LabelAlternateCost", _localizer["Alternate Cost"].ToString());
            parameters.Add("LabelAmendment", _localizer["Amendment"].ToString());
            parameters.Add("LabelAmount", _localizer["Amount"].ToString());
            parameters.Add("LabelAmountFrom", _localizer["Amount From"].ToString());
            parameters.Add("LabelAmountTo", _localizer["Amount To"].ToString());
            parameters.Add("LabelAnnClientCode", _localizer["Client Code"].ToString());
            parameters.Add("LabelAnnuities", _localizer["Annuities"].ToString());
            parameters.Add("LabelAnnuity", _localizer["Annuity"].ToString());
            parameters.Add("LabelAnnuityCost", _localizer["Annuity Cost"].ToString());
            parameters.Add("LabelAnnuityYear", _localizer["Annuity Year"].ToString());
            parameters.Add("LabelAnswer", _localizer["Answer"].ToString());
            parameters.Add("LabelAPosition", _localizer["A Position"].ToString());
            parameters.Add("LabelApplicable", _localizer["Applicable"].ToString());
            parameters.Add("LabelApplicant", _localizer["Applicant"].ToString());
            parameters.Add("LabelApplicantIndex", _localizer["Applicant Index"].ToString());
            parameters.Add("LabelApplicationLastUpdate", _localizer["Application Last Update"].ToString());
            parameters.Add("LabelApplicationLastUpdateDate", _localizer["Application Last Update Date"].ToString());
            parameters.Add("LabelApplicationStatuses", _localizer["Application Statuses"].ToString());
            parameters.Add("LabelApplicationType", _localizer["Application Type"].ToString());
            parameters.Add("LabelAppNumber", _localizer["Application Number"].ToString());
            parameters.Add("LabelAppNo", _localizer["Application No."].ToString());
            parameters.Add("LabelAppPatNo", _localizer["App/Pat No."].ToString());
            parameters.Add("LabelAppStatus", _localizer["Application Status"].ToString());
            parameters.Add("LabelAppTitle", _localizer["Application Title"].ToString());
            parameters.Add("LabelArea", _localizer["Area"].ToString());
            parameters.Add("LabelAreaMembership", _localizer["Area Membership"].ToString());
            parameters.Add("LabelArtwork", _localizer["Artwork"].ToString());
            parameters.Add("LabelAsOf", _localizer["As Of"].ToString());
            parameters.Add("LabelAsia", _localizer["Asia"].ToString());
            parameters.Add("LabelAssignmentDate", _localizer["Date Recorded"].ToString());
            parameters.Add("LabelAssignmentFrom", _localizer["Assignment From"].ToString());
            parameters.Add("LabelAssignments", _localizer["Assignments"].ToString());
            parameters.Add("LabelAssignmentStatus", _localizer["Assignment Status"].ToString());
            parameters.Add("LabelAssignmentTo", _localizer["Assignment To"].ToString());
            parameters.Add("LabelAttorney", _localizer["Attorney"].ToString());
            parameters.Add("LabelAttorney1", _localizer[labelAttorney1].ToString());
            parameters.Add("LabelAttorney2", _localizer[labelAttorney2].ToString());
            parameters.Add("LabelAttorney3", _localizer[labelAttorney3].ToString());
            parameters.Add("LabelAttorney4", _localizer[labelAttorney4].ToString());
            parameters.Add("LabelAttorney5", _localizer[labelAttorney5].ToString());
            parameters.Add("LabelAttorneyFilters", _localizer["Attorney Filter"].ToString());
            parameters.Add("LabelAttorneyName", _localizer["Attorney Name"].ToString());
            parameters.Add("LabelAttorneys", _localizer["Attorneys"].ToString());
            parameters.Add("LabelAuditLogs", _localizer["Audit Logs"].ToString());
            parameters.Add("LabelAutoDocketedActions", _localizer["Auto-Docketed Actions"].ToString());
            parameters.Add("LabelAutoGenerateAction", _localizer["Auto-Generate Action"].ToString());
            parameters.Add("LabelAvailableDocumentsList", _localizer["Available Documents List"].ToString());
            parameters.Add("LabelAwardDate", _localizer["Award Date"].ToString());
            parameters.Add("LabelAwards", _localizer["Awards"].ToString());
            parameters.Add("LabelAwardType", _localizer["Award Type"].ToString());
            parameters.Add("LabelBar", _localizer["Bar"].ToString());
            parameters.Add("LabelBase", _localizer["Base"].ToString());
            parameters.Add("LabelBaseApplication", _localizer["Base Application"].ToString());
            parameters.Add("LabelBaseCase", _localizer["Base Case"].ToString());
            parameters.Add("LabelBaseDate", _localizer["Base Date"].ToString());
            parameters.Add("LabelBaseDateFiled", _localizer["Base Date Filed"].ToString());
            parameters.Add("LabelBasedOn", _localizer["Base On"].ToString());
            parameters.Add("LabelBCreatingTheTaskOfTheInvention", _localizer["B Creating the task of the invention"].ToString());
            parameters.Add("LabelBibliographicData", _localizer["Bibliographic Data"].ToString());
            parameters.Add("LabelBillingNumber", _localizer["Billing Number"].ToString());
            parameters.Add("LabelBrand", _localizer["Brand"].ToString());
            parameters.Add("LabelBrandInformation", _localizer["Brand Information"].ToString());
            parameters.Add("LabelBrandType", _localizer["Brand Type"].ToString());
            parameters.Add("LabelBudget", _localizer["Budget"].ToString());
            parameters.Add("LabelBudgetAmount", _localizer["Budget Amount"].ToString());
            parameters.Add("LabelBudgetCaseType", _localizer["Budget Case Type"].ToString());
            parameters.Add("LabelBudgetCountry", _localizer["Budget Country"].ToString());
            parameters.Add("LabelBudgetCurrency", _localizer["Budget Currency"].ToString());
            parameters.Add("LabelBudgetForYear", _localizer["Budget for year"].ToString());
            parameters.Add("LabelByAgent", _localizer["by Agent"].ToString());
            parameters.Add("LabelByAttorney", _localizer["by Attorney"].ToString());
            parameters.Add("LabelByAttorneyThenClient", _localizer["by Attorney then " + labelClient].ToString());
            parameters.Add("LabelByAttorneyThenCountry", _localizer["by Attorney then Country"].ToString());
            parameters.Add("LabelByAwardDate", _localizer["by Award Date"].ToString());
            parameters.Add("LabelByCaseNumber", _localizer["by " + labelCaseNumber].ToString());
            parameters.Add("LabelByClient", _localizer["by " + labelClient].ToString());
            parameters.Add("LabelByClientThenCaseNumber", _localizer["by " + labelClient + " then " + labelCaseNumber].ToString());
            parameters.Add("LabelByClientThenCountry", _localizer["by " + labelClient + " then Country"].ToString());
            parameters.Add("LabelByClientThenDisclosureNumber", _localizer["by " + labelClient+ " then " + labelDisclosureNumber].ToString());
            parameters.Add("LabelByClientThenDueDate", _localizer["by " + labelClient + " then Due Date"].ToString());
            parameters.Add("LabelByCostType", _localizer["by Cost Type"].ToString());
            parameters.Add("LabelByCountry", _localizer["by Country"].ToString());
            parameters.Add("LabelByCountryThenCaseNumber", _localizer["by Country then " + labelCaseNumber].ToString());
            parameters.Add("LabelByCountryThenClient", _localizer["by Country then " + labelClient].ToString());
            parameters.Add("LabelByDueDate", _localizer["by Due Date"].ToString());
            parameters.Add("LabelByDueDateCalendar", _localizer["by Due Date (Calendar)"].ToString());
            parameters.Add("LabelByDisclosureNumber", _localizer["by " + labelDisclosureNumber].ToString());
            parameters.Add("LabelByFamilyNumber", _localizer["by Family Number"].ToString());
            parameters.Add("LabelByGroupArtUnit", _localizer["by Group Art Unit"].ToString());
            parameters.Add("LabelByInventor", _localizer["by Inventor"].ToString());
            parameters.Add("LabelByInvoiceDate", _localizer["by Invoice Date"].ToString());
            parameters.Add("LabelByInvoiceNumber", _localizer["by Invoice Number"].ToString());
            parameters.Add("LabelByMatterType", _localizer["by Matter Type"].ToString());
            parameters.Add("LabelByOtherParty", _localizer["by Other Party"].ToString());
            parameters.Add("LabelByOtherPartyTrademark", _localizer["by Other Party Trademark"].ToString());
            parameters.Add("LabelByOwner", _localizer["by " + labelOwner].ToString());
            parameters.Add("LabelByPaymentDate", _localizer["by Payment Date"].ToString());
            parameters.Add("LabelByProduct", _localizer["by Product"].ToString());
            parameters.Add("LabelByRequestorName", _localizer["by Requestor Name"].ToString());
            parameters.Add("LabelByRespOffice", _localizer["by Responsible Office"].ToString());
            parameters.Add("LabelByTitle", _localizer["by Title"].ToString());
            parameters.Add("LabelByTitleOrTrademark", _localizer["by Title or Trademark"].ToString());
            parameters.Add("LabelByTrademark", _localizer["by Trademark"].ToString());
            parameters.Add("LabelByYear", _localizer["by Year"].ToString());
            parameters.Add("LabelCalculate", _localizer["Calculate"].ToString());
            parameters.Add("LabelCaseInfomation", _localizer["Case Information"].ToString());
            parameters.Add("LabelCaseNumber", _localizer[labelCaseNumber].ToString());
            parameters.Add("LabelCaseRemarks", _localizer["Case Remarks"].ToString());
            parameters.Add("LabelCases", _localizer["Cases"].ToString());
            parameters.Add("LabelCaseStatus", _localizer["Case Status"].ToString());
            parameters.Add("LabelCaseType", _localizer["Case Type"].ToString());
            parameters.Add("LabelCaseTypes", _localizer["Case Types"].ToString());
            parameters.Add("LabelCategory", _localizer["Category"].ToString());
            parameters.Add("LabelCertificateNo", _localizer["Certificate No"].ToString());
            parameters.Add("LabelChangedBy", _localizer["Changed By"].ToString());
            parameters.Add("LabelChartOnly", _localizer["Chart Only"].ToString());
            parameters.Add("LabelChartType", _localizer["Chart Type"].ToString());
            parameters.Add("LabelChildContinuity", _localizer["Child Continuity"].ToString());
            parameters.Add("LabelClaims", _localizer["Claims"].ToString());
            parameters.Add("LabelCitedBy", _localizer["Cited By"].ToString());
            parameters.Add("LabelCity", _localizer["City"].ToString());
            parameters.Add("LabelClass", _localizer["Class"].ToString());
            parameters.Add("LabelClassOrSubClass", _localizer["Class/Subclass"].ToString());
            parameters.Add("LabelClasses", _localizer["Classes"].ToString());
            parameters.Add("LabelClassesAndGoods", _localizer["Classes and Goods"].ToString());
            parameters.Add("LabelClassesOnly", _localizer["Classes Only"].ToString());
            parameters.Add("LabelClassOrGoods", _localizer["Class/Goods"].ToString());
            parameters.Add("LabelClassSubclassIndex", _localizer["Class/Subclass Index"].ToString());
            parameters.Add("LabelClassType", _localizer["Class Type"].ToString());
            parameters.Add("LabelSearchRequestStatus", _localizer["Search Request Status"].ToString());
            parameters.Add("LabelSetupName", _localizer["Setup Name"].ToString());
            parameters.Add("LabelClient", _localizer[labelClient].ToString());
            parameters.Add("LabelClientConfirmation", _localizer[labelClient + " Confirmation"].ToString());
            parameters.Add("LabelClientData", _localizer[labelClient+" Data"].ToString());
            parameters.Add("LabelClientInstruction", _localizer[labelClient + " Instruction"].ToString());
            parameters.Add("LabelClientInstructionDate", _localizer[labelClient + " Instruction Date"].ToString());
            parameters.Add("LabelClients", _localizer[labelClient+"s"].ToString());
            parameters.Add("LabelClientName", _localizer[labelClientName].ToString());
            parameters.Add("LabelClientRef", _localizer[labelClientRef].ToString());
            parameters.Add("LabelCompareAction", _localizer["Compare Action"].ToString());
            parameters.Add("LabelComparing", _localizer["Comparing"].ToString());
            parameters.Add("LabelCompleted", _localizer["Completed"].ToString());
            parameters.Add("LabelComprehensive", _localizer["Comprehensive"].ToString());
            parameters.Add("LabelConcise", _localizer["Concise"].ToString());
            parameters.Add("LabelConfirmationNumber", _localizer["Confirmation No."].ToString());
            parameters.Add("LabelConfirmationNumberIndex", _localizer["Confirmation Number Index"].ToString());
            parameters.Add("LabelConflictOpposition", _localizer["Conflict/Opposition"].ToString());
            parameters.Add("LabelConflictOppositionNumber", _localizer["Conflict/Opposition Number"].ToString());
            parameters.Add("LabelConflictStatus", _localizer["Conflict Status"].ToString());
            parameters.Add("LabelContact", _localizer["Contact"].ToString());
            parameters.Add("LabelContactName", _localizer["Contact Name"].ToString());
            parameters.Add("LabelContactPerson", _localizer["Contact Person"].ToString());
            parameters.Add("LabelContacts", _localizer["Contacts"].ToString());
            parameters.Add("LabelContactTitle", _localizer["Contact Title"].ToString());
            parameters.Add("LabelContinuingObligationAfterTheTerminationDate", _localizer["Continuing Obligation After the Termination Date"].ToString());
            parameters.Add("LabelContinuityData", _localizer["Continuity Data"].ToString());
            parameters.Add("LabelCorrespondence", _localizer["Correspondence"].ToString());
            parameters.Add("LabelCostEstimator", _localizer["Cost Estimator"].ToString());
            parameters.Add("LabelCostEstimatorName", _localizer["Cost Estimator Name"].ToString());
            parameters.Add("LabelCosts", _localizer["Costs"].ToString());
            parameters.Add("LabelCostSetup", _localizer["Cost Setup"].ToString());
            parameters.Add("LabelCostToExpiration", _localizer["Cost To Expiration"].ToString());
            parameters.Add("LabelCostTracking", _localizer["Cost Tracking"].ToString());
            parameters.Add("LabelCostType", _localizer["Cost Type"].ToString());
            parameters.Add("LabelCount", _localizer["Count"].ToString());
            parameters.Add("LabelCountries", _localizer["Countries"].ToString());
            parameters.Add("LabelCountry", _localizer["Country"].ToString());
            parameters.Add("LabelCountryApplication", _localizer["Country Application"].ToString());
            parameters.Add("LabelCountryInformation", _localizer["Country Information"].ToString());
            parameters.Add("LabelCountryMembership", _localizer["Country Membership"].ToString());
            parameters.Add("LabelCountryName", _localizer["Country Name"].ToString());
            parameters.Add("LabelCountrySpecificQuestion", _localizer["Country Specific Question"].ToString());
            parameters.Add("LabelCountrySpecificQuestions", _localizer["Country Specific Question(s)"].ToString());
            parameters.Add("LabelCourt", _localizer["Court"].ToString());
            parameters.Add("LabelCourtDocketNumber", _localizer["Court Docket Number"].ToString());
            parameters.Add("LabelCPI", _localizer["CPI"].ToString());
            parameters.Add("LabelCPICode", _localizer["CPI Code"].ToString());
            parameters.Add("LabelCPIData", _localizer["CPI Data"].ToString());
            parameters.Add("LabelCPIPays", _localizer["CPI Pays"].ToString());
            parameters.Add("LabelCPIWillNotPay", _localizer["CPI will not pay"].ToString());
            parameters.Add("LabelCreatedBy", _localizer["Created By"].ToString());
            parameters.Add("LabelCreatedByAndDate", _localizer["Created"].ToString());
            parameters.Add("LabelCreateInvention", _localizer["Create Invention"].ToString());
            parameters.Add("LabelCSolutionOfTheProblem", _localizer["C Solution of the problem"].ToString());
            parameters.Add("LabelCurrency", _localizer["Currency"].ToString());
            parameters.Add("LabelCurrencyType", _localizer["Currency Type"].ToString());
            parameters.Add("LabelCustomFields", _localizer["Custom Fields"].ToString());
            parameters.Add("LabelDate", _localizer["Date"].ToString());
            parameters.Add("LabelDateBy", _localizer["Date By"].ToString());
            parameters.Add("LabelDateChanged", _localizer["Date Changed"].ToString());
            parameters.Add("LabelDateCompleted", _localizer["Date Completed"].ToString());
            parameters.Add("LabelDateCreated", _localizer["Date Created"].ToString());
            parameters.Add("LabelDateRequested", _localizer["Date Requested"].ToString());
            parameters.Add("LabelDateTaken", _localizer["Date Taken"].ToString()); 
            parameters.Add("LabelDeDocketInstructionOnly", _localizer["DeDocket Instruction Only"].ToString());
            parameters.Add("LabelDefault", _localizer["Default"].ToString());
            parameters.Add("LabelDefaultCost", _localizer["Default Cost"].ToString());
            parameters.Add("LabelDefaultExchangeRate", _localizer["Default Exchange Rate"].ToString());
            parameters.Add("LabelDefaultImage", _localizer["Default Image"].ToString());
            parameters.Add("LabelDefaultValue", _localizer["Default Value"].ToString());
            parameters.Add("LabelDefendant", _localizer["Defendant"].ToString());
            parameters.Add("LabelDelegated", _localizer["Delegated"].ToString());
            parameters.Add("LabelDescription", _localizer["Description"].ToString());
            parameters.Add("LabelDesignatedCountries", _localizer["Designated Countries"].ToString());
            parameters.Add("LabelDesignatedCountry", _localizer["Designated Country"].ToString());
            parameters.Add("LabelDesignatedStates", _localizer["Designated States"].ToString());
            parameters.Add("LabelDirection", _localizer["Direction"].ToString());
            parameters.Add("LabelDisclaimerPatentNumber", _localizer["Disclaimer Patent Number"].ToString());
            parameters.Add("LabelDisclosure", _localizer["Disclosure"].ToString());
            parameters.Add("LabelDisclosureDate", _localizer["Disclosure Date"].ToString());
            parameters.Add("LabelDisclosureNumber", _localizer[labelDisclosureNumber].ToString());
            parameters.Add("LabelDisclosureStatus", _localizer["Disclosure Status"].ToString());
            parameters.Add("LabelDisclosureStatusAndDate", _localizer["Disclosure Status and Date"].ToString());
            parameters.Add("LabelDisclosureStatuses", _localizer["Disclosure Statuses"].ToString());
            parameters.Add("LabelDiscussions", _localizer["Discussions"].ToString());
            parameters.Add("LabelNDA", _localizer["NDA"].ToString());
            parameters.Add("LabelDocDate", _localizer["Doc Date"].ToString());
            parameters.Add("LabelDocNo", _localizer["Doc #"].ToString());
            parameters.Add("LabelDocument", _localizer["Document"].ToString());
            parameters.Add("LabelDocuments", _localizer["Documents"].ToString());
            parameters.Add("LabelDocumentType", _localizer["Document Type"].ToString());
            parameters.Add("LabelDoNotRenew", _localizer["Do Not Renew"].ToString());
            parameters.Add("LabelDontGenerate", _localizer["Don't Generate"].ToString());
            parameters.Add("LabelDontPrint", _localizer["Don't Print"].ToString());
            parameters.Add("LabelDueDate", _localizer["Due Date"].ToString());
            parameters.Add("LabelDueDateAttorney", _localizer["Due Date Attorney"].ToString());
            parameters.Add("LabelDueDateRemarks", _localizer["Due Date Remarks"].ToString());
            parameters.Add("LabelDueDates", _localizer["Due Dates"].ToString());
            parameters.Add("LabelDueRemarks", _localizer["Due Date Remarks"].ToString());
            parameters.Add("LabelDy", _localizer["Dy"].ToString());
            parameters.Add("LabelEffectiveDate", _localizer["Effective Date"].ToString());
            parameters.Add("LabelEffectiveEnd", _localizer["Effective End"].ToString());
            parameters.Add("LabelEffectiveStart", _localizer["Effective Start"].ToString());
            parameters.Add("LabelEffectiveOpen", _localizer["Effective/Open"].ToString());
            parameters.Add("LabelEmail", _localizer["Email"].ToString());
            parameters.Add("LabelEmployeeID", _localizer["Employee ID"].ToString());
            parameters.Add("LabelEmployeePosition", _localizer["Employee Title"].ToString());
            parameters.Add("LabelEndDate", _localizer["End Date"].ToString());
            parameters.Add("LabelEntityStatus", _localizer["Entity Status"].ToString());
            parameters.Add("LabelEstimatedCost", _localizer["Estimated Cost"].ToString());
            parameters.Add("LabelEurope", _localizer["Europe"].ToString());
            parameters.Add("LabelExaminer", _localizer["Examiner"].ToString());
            parameters.Add("LabelExaminerIndex", _localizer["Examiner Index"].ToString());
            parameters.Add("LabelExchangeRate", _localizer["Exchange Rate"].ToString());
            parameters.Add("LabelExchangeRateAmt", _localizer["Exchange Rate Amt"].ToString());
            parameters.Add("LabelExpiration", _localizer["Expiration"].ToString());
            parameters.Add("LabelExpirationDate", _localizer["Expiration Date"].ToString());
            parameters.Add("LabelExpirationTerms", _localizer["Expiration Terms"].ToString());
            parameters.Add("LabelExportControl", _localizer["Export Control"].ToString());
            parameters.Add("LabelExtended", _localizer["Extended"].ToString());
            parameters.Add("LabelExtent", _localizer["Extent"].ToString());
            parameters.Add("LabelExtractedTerm", _localizer["Extracted Term"].ToString());
            parameters.Add("LabelFamilyCostSavings", _localizer["Family Cost Savings"].ToString());
            parameters.Add("LabelFamilyNumber", _localizer["Family Number"].ToString());
            parameters.Add("LabelFamilyTotal", _localizer["Family Total"].ToString());
            parameters.Add("LabelFaxNo", _localizer["Fax No."].ToString());
            parameters.Add("LabelFeeAsOf", _localizer["Fee as of"].ToString());
            parameters.Add("LabelFeeCode", _localizer["Fee Code"].ToString());
            parameters.Add("LabelFees", _localizer["Fees"].ToString());
            parameters.Add("LabelFilDate", _localizer["Filing Date"].ToString());
            parameters.Add("LabelFiled", _localizer["Filed"].ToString());
            parameters.Add("LabelFiling", _localizer["Filing"].ToString());
            parameters.Add("LabelFileNameContains", _localizer["File Name Contains"].ToString());
            parameters.Add("LabelFileThroughEPO", _localizer["File through EPO"].ToString());
            parameters.Add("LabelFileThroughPCT", _localizer["File through PCT"].ToString());
            parameters.Add("LabelFinalDate", _localizer["Final Date"].ToString());
            parameters.Add("LabelFirstName", _localizer["First Name"].ToString());
            parameters.Add("LabelFollowUpAction", _localizer["Follow Up Action"].ToString());
            parameters.Add("LabelFollowUpBasedOn", _localizer["Follow Up Based On"].ToString());
            parameters.Add("LabelFollowUpTerm", _localizer["Follow Up Term"].ToString());
            parameters.Add("LabelFollowUpTermMessage", _localizer["[m] Month(s) / [d] Day(s)"].ToString());
            parameters.Add("LabelFor", _localizer["For"].ToString());
            parameters.Add("LabelForecastCost", _localizer["Forecast Cost"].ToString());
            parameters.Add("LabelFormula", _localizer["Formula"].ToString());
            parameters.Add("LabelFrame", _localizer["Frame"].ToString());
            parameters.Add("LabelFrom", _localizer["From"].ToString());
            parameters.Add("LabelFromDate", _localizer["From Date"].ToString());
            parameters.Add("LabelGazetteDate", _localizer["Gazette Date"].ToString());
            parameters.Add("LabelGenApp", _localizer["Gen App?"].ToString());
            parameters.Add("LabelGeneral", _localizer["General"].ToString());
            parameters.Add("LabelGeneralMatter", _localizer["General Matter"].ToString());
            parameters.Add("LabelGeneralQuestion", _localizer["General Question"].ToString());
            parameters.Add("LabelGeneralQuestions", _localizer["General Question(s)"].ToString());
            parameters.Add("LabelGenerated", _localizer["Generated"].ToString());
            parameters.Add("LabelGeneratedBy", _localizer["Generated By"].ToString());
            parameters.Add("LabelGeneratedOn", _localizer["Generated On"].ToString());
            parameters.Add("LabelGenerateInvention", _localizer["Generate Invention"].ToString());
            parameters.Add("LabelGMCaseNumber", _localizer[labelGMCaseNumber].ToString());
            parameters.Add("LabelGoods", _localizer["Goods"].ToString());
            parameters.Add("LabelGoodsOnly", _localizer["Goods Only"].ToString());
            parameters.Add("LabelGrandTotal", _localizer["Grand Total"].ToString());
            parameters.Add("LabelGranted", _localizer["Granted"].ToString());
            parameters.Add("LabelGreeting", _localizer["Greeting"].ToString());
            parameters.Add("LabelGroupArtUnit", _localizer["Group Art Unit"].ToString());
            parameters.Add("LabelGroupArtUnitIndex", _localizer["Group Art Unit Index"].ToString());
            parameters.Add("LabelHigh", _localizer["High"].ToString());
            parameters.Add("LabelIDSFileDate", _localizer["IDS File Date"].ToString());
            parameters.Add("LabelIFWDocumentType", _localizer["IFW Document Type"].ToString());
            parameters.Add("LabelInactiveStatus", _localizer["Inactive Status"].ToString());
            parameters.Add("LabelIncludeFamily", _localizer["Include Family"].ToString());
            parameters.Add("LabelIncludeServiceFee", _localizer["Include Service Fee"].ToString());
            parameters.Add("LabelIncludeSoftDocket", _localizer["Include Soft Docket"].ToString());
            parameters.Add("LabelIncludeVAT", _localizer["Include VAT"].ToString());
            parameters.Add("LabelIndicator", _localizer["Indicator"].ToString());
            parameters.Add("LabelIndicators", _localizer["Indicators"].ToString());
            parameters.Add("LabelIndividualAmount", _localizer["Individual Amount"].ToString());
            parameters.Add("LabelInitial", _localizer["Initial"].ToString());
            parameters.Add("LabelInitialPayment", _localizer["Initial Payment"].ToString());
            parameters.Add("LabelInstructedBy", _localizer["Instructed By"].ToString());
            parameters.Add("LabelInstruction", _localizer["Instruction"].ToString());
            parameters.Add("LabelInstructionDate", _localizer["Instruction Date"].ToString());
            parameters.Add("LabelInstructionFromClient", _localizer["Instruction From "+labelClient].ToString());
            parameters.Add("LabelInstructionHistory", _localizer["Instruction History"].ToString());
            parameters.Add("LabelInstructionRemarks", _localizer["Instruction Remarks"].ToString());
            parameters.Add("LabelInstructionSentToCPI", _localizer["Instruction Sent to CPI"].ToString());
            parameters.Add("LabelIntenttoUse", _localizer["Intent to Use"].ToString());
            parameters.Add("LabelInternationalClassIndex", _localizer["International Class Index"].ToString());
            parameters.Add("LabelInUse", _localizer["In Use"].ToString());
            parameters.Add("LabelInventor", _localizer["Inventor"].ToString());
            parameters.Add("LabelInventorIndex", _localizer["Inventor Index"].ToString());
            parameters.Add("LabelInventors", _localizer["Inventors"].ToString());
            parameters.Add("LabelInventionLastUpdate", _localizer["Invention Last Update "].ToString());
            parameters.Add("LabelInventionLastUpdateDate", _localizer["Invention Last Update Date"].ToString());
            parameters.Add("LabelInventionInventors", _localizer["Invention Inventors"].ToString());
            parameters.Add("LabelInvoiceAmount", _localizer["Invoice Amount"].ToString());
            parameters.Add("LabelInvoiceCost", _localizer["Invoice Cost"].ToString());
            parameters.Add("LabelInvoiceDate", _localizer["Invoice Date"].ToString());
            parameters.Add("LabelInvoiceDateNumberAmount", _localizer["Invoice Date, Number, Amount"].ToString());
            parameters.Add("LabelInvoiceInfo", _localizer["Invoice Info"].ToString());
            parameters.Add("LabelInvoiceNo", _localizer["Invoice No"].ToString());
            parameters.Add("LabelInvoiceNumber", _localizer["Invoice Number"].ToString());
            parameters.Add("LabelInvoicePaid", _localizer["Invoice Paid"].ToString());
            parameters.Add("LabelInvoiceTotal", _localizer["Invoice(s) Total"].ToString());
            parameters.Add("LabelInvTitle", _localizer["Invention Title"].ToString());
            parameters.Add("LabelIssDate", _localizer["Issue Date"].ToString());
            parameters.Add("LabelIssRegDate", _localizer["Issue/Reg Date"].ToString());
            parameters.Add("LabelIssue", _localizer["Issue"].ToString());
            parameters.Add("LabelIssued", _localizer["Issued"].ToString());
            parameters.Add("LabelItem", _localizer["Item"].ToString());
            parameters.Add("LabelJudgeMagistrate", _localizer["Judge Magistrate"].ToString());
            parameters.Add("LabelKD", _localizer["KD"].ToString());
            parameters.Add("LabelKeyFeatures", _localizer["Key Features"].ToString());
            parameters.Add("LabelKeyword", _localizer[labelKeyword].ToString());
            //parameters.Add("LabelLastUpdateDate", _localizer["Last Update Date"].ToString());
            parameters.Add("LabelLanguage", _localizer["Language"].ToString());
            parameters.Add("LabelLargeEntity", _localizer["Large Entity"].ToString());
            parameters.Add("LabelLastAccessDate", _localizer["Last Access Date"].ToString());
            parameters.Add("LabelLastGrantedDate", _localizer["Last Granted Date"].ToString());
            parameters.Add("LabelLastName", _localizer["Last Name"].ToString());
            parameters.Add("LabelLastRenewalDate", _localizer["Last Renewal Date"].ToString());
            parameters.Add("LabelLastRenewalNumber", _localizer["Last Renewal Number"].ToString());
            parameters.Add("LabelLastRevokedDate", _localizer["Last Revoked Date"].ToString());
            parameters.Add("LabelLastUpdate", _localizer["Last Update"].ToString());
            parameters.Add("LabelLatamCaribbean", _localizer["Latam & Caribbean"].ToString());
            parameters.Add("LabelLayout", _localizer["Layout"].ToString());
            parameters.Add("LabelLayoutFormat", _localizer["Layout Format"].ToString());
            parameters.Add("LabelLawActions", _localizer["Law Actions"].ToString());
            parameters.Add("LabelLawHighLights", _localizer["Law HighLights"].ToString());
            parameters.Add("LabelLeadAmount", _localizer["Lead Amount"].ToString());
            parameters.Add("LabelLegalRepresentative", _localizer["Legal Representative"].ToString());
            parameters.Add("LabelLegalRepresentativeName", _localizer["Legal Representative Name"].ToString());
            parameters.Add("LabelLegalStatus", _localizer["Legal Status"].ToString());
            parameters.Add("LabelLegend", _localizer["Legend"].ToString());
            parameters.Add("LabelLengthOfContinuingObligation", _localizer["Length of Continuing Obligation"].ToString());
            parameters.Add("LabelLetterSent", _localizer["Letter Sent"].ToString());
            parameters.Add("LabelLicensee", _localizer["Licensee"].ToString());
            parameters.Add("LabelLicensed", _localizer["Licensed"].ToString());
            parameters.Add("LabelLicenseExpire", _localizer["License Expire"].ToString());
            parameters.Add("LabelLicenseFactor", _localizer["License Factor"].ToString());
            parameters.Add("LabelLicenseNo", _localizer["License No"].ToString());
            parameters.Add("LabelLicenses", _localizer["Licenses"].ToString());
            parameters.Add("LabelLicenseStart", _localizer["License Start"].ToString());
            parameters.Add("LabelLicensor", _localizer["Licensor"].ToString());
            parameters.Add("LabelLine", _localizer["Line"].ToString());
            parameters.Add("LabelList", _localizer["List"].ToString());
            parameters.Add("LabelLiterature", _localizer["Literature"].ToString());
            parameters.Add("LabelLow", _localizer["Low"].ToString());
            parameters.Add("LabelLumpSum", _localizer["Lump Sum"].ToString());
            parameters.Add("LabelMadridProtocal", _localizer["Madrid Protocal"].ToString());
            parameters.Add("LabelMailRoomDate", _localizer["Mail Room Date"].ToString());
            parameters.Add("LabelMainOwnerName", _localizer["Main " + labelOwner + " Name"].ToString());
            parameters.Add("LabelManager", _localizer["Manager"].ToString());
            parameters.Add("LabelManufacturer", _localizer["Manufacturer"].ToString());
            parameters.Add("LabelMarkInformation", _localizer["Mark Information"].ToString());
            parameters.Add("LabelMarkName", _localizer["Mark Name"].ToString());
            parameters.Add("LabelMarkType", _localizer["Mark Type"].ToString());
            parameters.Add("LabelMatrixType", _localizer["Matrix Type"].ToString());
            parameters.Add("LabelMatterStatus", _localizer["Matter Status"].ToString());
            parameters.Add("LabelMatterType", _localizer["Matter Type"].ToString());
            parameters.Add("LabelMatterTitle", _localizer["Matter Title"].ToString());
            parameters.Add("LabelMax", _localizer["Max"].ToString());
            parameters.Add("LabelMaxAmount", _localizer["Max Amount"].ToString());
            parameters.Add("LabelMaxValue", _localizer["Max Value"].ToString());
            parameters.Add("LabelMessage", _localizer["Message"].ToString());
            parameters.Add("LabelMeetingDate", _localizer["Meeting Date"].ToString());
            parameters.Add("LabelMeetingResults", _localizer["Meeting Results"].ToString());
            parameters.Add("LabelMicroEntity", _localizer["Micro Entity"].ToString());
            parameters.Add("LabelMiddleEast", _localizer["Middle East"].ToString());
            parameters.Add("LabelMiddleName", _localizer["Middle Name"].ToString());
            parameters.Add("LabelMin", _localizer["Min"].ToString());
            parameters.Add("LabelMinAmount", _localizer["Min Amount"].ToString());
            parameters.Add("LabelMinValue", _localizer["Min Value"].ToString());
            parameters.Add("LabelMo", _localizer["Mo"].ToString());
            parameters.Add("LabelMobileNo", _localizer["Mobile No."].ToString());
            parameters.Add("LabelMonthly", _localizer["Monthly"].ToString());
            parameters.Add("LabelMultiplierCost", _localizer["Multiplier Cost"].ToString());
            parameters.Add("LabelName", _localizer["Name"].ToString());
            parameters.Add("LabelNationalNumber", _localizer["National No"].ToString());
            parameters.Add("LabelNationalNo", _localizer["National No"].ToString());
            parameters.Add("LabelNetAmount", _localizer["Net Amount"].ToString());
            parameters.Add("LabelNetCost", _localizer["Net Cost"].ToString());
            parameters.Add("LabelNewValue", _localizer["New Value"].ToString());
            parameters.Add("LabelNext", _localizer["Next"].ToString());
            parameters.Add("LabelNextRenewalDate", _localizer["Next Renewal"].ToString());
            parameters.Add("LabelNo", _localizer["No"].ToString());
            parameters.Add("LabelNoDot", _localizer["No."].ToString());
            parameters.Add("LabelNone", _localizer["None"].ToString());
            parameters.Add("LabelNonEmployeeInventors", _localizer["Non-Employee Inventors"].ToString());
            parameters.Add("LabelNonPatentLiterature", _localizer["Non Patent Literature"].ToString());
            parameters.Add("LabelNoOfInventorsToAward", _localizer["Max Number of Inventor to Reward"].ToString());
            parameters.Add("LabelNoOfPages", _localizer["No. Of Pages"].ToString());
            parameters.Add("LabelNoRecords", _localizer["There are no records to display."].ToString());
            parameters.Add("LabelNorthAmerica", _localizer["North America"].ToString());
            parameters.Add("LabelNote", _localizer["Note"].ToString());
            parameters.Add("LabelNoteOfCostEstimator", _localizer["These estimates are based on fee schedules from public data and other sources. The fees are for processing simple applications. The fees do not include charges for responses to office actions and other work involved. The annuity cost estimates are based on one claim for certain countries where the annuity fee is based on the number of claims. The fees are subject to any exchange rate fluctuations."].ToString());
            parameters.Add("LabelNoticeRequiredForTermination", _localizer["Notice Required for Termination"].ToString());
            parameters.Add("LabelNotPaid", _localizer["Not Paid"].ToString());
            parameters.Add("LabelOceana", _localizer["Oceana"].ToString());
            parameters.Add("LabelOfficeAction", _localizer["Office Action"].ToString());
            parameters.Add("LabelOldCaseNumber", _localizer[labelOldCaseNumber].ToString());
            parameters.Add("LabelOldValue", _localizer["Old Value"].ToString());
            parameters.Add("LabelOpponent", _localizer["Opponent"].ToString());
            parameters.Add("LabelOurClientReference", _localizer["Our "+labelClient+" Reference"].ToString());
            parameters.Add("LabelOurPatents", _localizer["Our Patents"].ToString());
            parameters.Add("LabelOurReference", _localizer["Our Reference"].ToString());
            parameters.Add("LabelOurTrademarks", _localizer["Our Trademarks"].ToString());
            parameters.Add("LabelOtherInventors", _localizer["Other Inventors"].ToString());
            parameters.Add("LabelOtherKeywords", _localizer["Other " + labelKeyword].ToString());
            parameters.Add("LabelOtherParties", _localizer["Other Parties"].ToString());
            parameters.Add("LabelOtherParty", _localizer["Other Party"].ToString());
            parameters.Add("LabelOtherPartyMarks", _localizer["Other Party Marks"].ToString());
            parameters.Add("LabelOtherPartyPatents", _localizer["Other Party Patents"].ToString());
            parameters.Add("LabelOtherPartyTrademark", _localizer["Other Party Trademark"].ToString());
            parameters.Add("LabelOtherPartyTrademarks", _localizer["Other Party Trademarks"].ToString());
            parameters.Add("LabelOtherPartyType", _localizer["Other Party Type"].ToString());
            parameters.Add("LabelOtherProducts", _localizer["Other Products"].ToString());
            parameters.Add("LabelOtherReferenceNo", _localizer["Other Reference No."].ToString());
            parameters.Add("LabelOwner", _localizer[labelOwner].ToString());
            parameters.Add("LabelOwnerName", _localizer[labelOwnerName].ToString());
            parameters.Add("LabelOwners", _localizer[labelOwner+"s"].ToString());
            parameters.Add("LabelPaid", _localizer["Paid"].ToString());
            parameters.Add("LabelPaidByLumpSum", _localizer["Paid By Lump Sum"].ToString());
            parameters.Add("LabelPaidDate", _localizer["Paid Date"].ToString());
            parameters.Add("LabelPaidThru", _localizer["Paid Thru"].ToString());
            parameters.Add("LabelPage", _localizer["Page"].ToString());
            parameters.Add("LabelParent", _localizer["Parent"].ToString());
            parameters.Add("LabelParentContinuity", _localizer["Parent Continuity"].ToString());
            parameters.Add("LabelParentMatter", _localizer["Parent Matter"].ToString());
            parameters.Add("LabelParentOrPCTDate", _localizer["Parent/PCT Date"].ToString());
            parameters.Add("LabelParentOrPCTNumber", _localizer["Parent/PCT Number"].ToString());
            parameters.Add("LabelPatCaseNumber", _localizer[patSettings.IsClientMatterOn ? patSettings.LabelClientMatter : patSettings.LabelCaseNumber].ToString());
            parameters.Add("LabelPatent", _localizer["Patent"].ToString());
            parameters.Add("LabelPatentLinks", _localizer["Patent Links"].ToString());
            parameters.Add("LabelPatentTermAdjustment", _localizer["Patent Term Adjustment"].ToString());
            parameters.Add("LabelPatNo", _localizer["Patent No."].ToString());
            parameters.Add("LabelPatNumber", _localizer["Patent Number"].ToString());
            parameters.Add("LabelPatRegNumber", _localizer["Patent/Reg Number"].ToString());
            parameters.Add("LabelPaymentAndReceipts", _localizer["Payment and Receipts"].ToString());
            parameters.Add("LabelPaymentDate", _localizer["Payment Date"].ToString());
            parameters.Add("LabelPaymentNeeded", _localizer["Payment Needed"].ToString());
            parameters.Add("LabelPaymentType", _localizer["Payment Type"].ToString());
            parameters.Add("LabelParty", _localizer["Party"].ToString());
            parameters.Add("LabelPCTNumber", _localizer["PCT Number"].ToString());
            parameters.Add("LabelPCTDate", _localizer["PCT Date"].ToString());
            parameters.Add("LabelPCTNationalPhase", _localizer["PCT National Phase"].ToString());
            parameters.Add("LabelPending", _localizer["Pending"].ToString());
            parameters.Add("LabelPercentOfBudgetRemaining", _localizer["% of Budget Remaining"].ToString());
            parameters.Add("LabelPercentOfInvention", _localizer["% of Invention"].ToString());
            parameters.Add("LabelPercentOfOwnership", _localizer["% of Ownership"].ToString());
            parameters.Add("LabelPeriodType", _localizer["Period Type"].ToString());
            parameters.Add("LabelPie", _localizer["Pie"].ToString());
            parameters.Add("LabelPlaintiff", _localizer["Plaintiff"].ToString());
            parameters.Add("LabelPOApplicationNo", _localizer["P.O. Application No"].ToString());
            parameters.Add("LabelPOBox", _localizer["P.O. Box"].ToString());
            parameters.Add("LabelPOFilingDate", _localizer["P.O. Filing Date"].ToString());
            parameters.Add("LabelPoint", _localizer["Point"].ToString());
            parameters.Add("LabelPosition", _localizer["Title"].ToString());
            parameters.Add("LabelPostalOrZipCode", _localizer["Postal/Zip Code"].ToString());
            parameters.Add("LabelPostedBy", _localizer["Posted By"].ToString());
            parameters.Add("LabelPostedOn", _localizer["Posted On"].ToString());
            parameters.Add("LabelPrintAbstract", _localizer["Print Abstract"].ToString());
            parameters.Add("LabelPrintActionDueRemarks", _localizer["Print Action Due Remarks"].ToString());
            parameters.Add("LabelPrintActions", _localizer["Print Actions"].ToString());
            parameters.Add("LabelPrintArtwork", _localizer["Print Artwork"].ToString());
            parameters.Add("LabelPrintAssignments", _localizer["Print Assignments"].ToString());
            parameters.Add("LabelPrintBrandInformation", _localizer["Print Brand Information"].ToString());
            parameters.Add("LabelPrintCaseRemarks", _localizer["Print Case Remarks"].ToString());
            parameters.Add("LabelPrintCorrespondence", _localizer["Print Correspondence"].ToString());
            parameters.Add("LabelPrintCountryApplication", _localizer["Print Country Application"].ToString());
            parameters.Add("LabelPrintCountryInformation", _localizer["Print Custom Fields"].ToString());
            parameters.Add("LabelPrintCustomFields", _localizer["Print Custom Fields"].ToString());
            parameters.Add("LabelPrintDesignatedCountries", _localizer["Print Designated Countries"].ToString());
            parameters.Add("LabelPrintDocuments", _localizer["Print Documents"].ToString());
            parameters.Add("LabelPrintDueDateRemarks", _localizer["Print Due Date Remarks"].ToString());
            parameters.Add("LabelPrintFinalDeadlines", _localizer["Print Final Deadline(s)"].ToString());
            parameters.Add("LabelPrintGeneral", _localizer["Print General"].ToString());
            parameters.Add("LabelPrintGoods", _localizer["Print Goods"].ToString());
            parameters.Add("LabelPrintInventors", _localizer["Print Inventors"].ToString());
            parameters.Add("LabelPrintKeywords", _localizer["Print "+labelKeyword].ToString());
            parameters.Add("LabelPrintLicenses", _localizer["Print Licenses"].ToString());
            parameters.Add("LabelPrintList", _localizer["Print List"].ToString());
            parameters.Add("LabelPrintMarkInformation", _localizer["Print Mark Information"].ToString());
            parameters.Add("LabelPrintAnnuityManagement", _localizer["Print Annuity Management"].ToString());
            parameters.Add("LabelPrintOurPatents", _localizer["Print Our Patents"].ToString());
            parameters.Add("LabelPrintOurTrademarks", _localizer["Print Our Trademarks"].ToString());
            parameters.Add("LabelPrintOtherParties", _localizer["Print Other Parties"].ToString());
            parameters.Add("LabelPrintOtherPartyPatents", _localizer["Print Other Party Patents"].ToString());
            parameters.Add("LabelPrintOtherPartyTrademarks", _localizer["Print Other Party Trademarks"].ToString());
            parameters.Add("LabelPrintOutstandingActions", _localizer["Print Outstanding Actions"].ToString());
            parameters.Add("LabelPrintPastReminders", _localizer["Print Past Reminders"].ToString());
            parameters.Add("LabelPrintPriorities", _localizer["Print Priorities"].ToString());
            parameters.Add("LabelPrintProducts", _localizer["Print Products"].ToString());
            parameters.Add("LabelPrintRatingRemarks", _localizer["Print Rating Remarks"].ToString());
            parameters.Add("LabelPrintRelatedCases", _localizer["Print Related Cases"].ToString());
            parameters.Add("LabelPrintRelatedMatters", _localizer["Print Related Matters"].ToString());
            parameters.Add("LabelPrintRelatedTrademarks", _localizer["Print Related Trademarks"].ToString());
            parameters.Add("LabelPrintRemarks", _localizer["Print Remarks"].ToString());
            parameters.Add("LabelPrintRequestedTerms", _localizer["Print Requested Terms"].ToString());
            parameters.Add("LabelPrintStatusHistory", _localizer["Print History"].ToString());
            parameters.Add("LabelPrintStatusOfUse", _localizer["Print Status Of Use"].ToString());
            parameters.Add("LabelPrintSubjectMatters", _localizer["Print Subject Matters"].ToString());
            parameters.Add("LabelPriorities", _localizer["Priorities"].ToString());
            parameters.Add("LabelPriority", _localizer["Priority"].ToString());
            parameters.Add("LabelPriorityCountry", _localizer["Priority Country"].ToString());
            parameters.Add("LabelPriorityDate", _localizer["Priority Date"].ToString());
            parameters.Add("LabelPriorityNumber", _localizer["Priority Number"].ToString());
            parameters.Add("LabelPriorityNumberIndex", _localizer["Priority Number Index"].ToString());
            parameters.Add("LabelProceedingFilingDate", _localizer["Proceeding Filing Date"].ToString());
            parameters.Add("LabelProceedingNo", _localizer["Proceeding No"].ToString());
            parameters.Add("LabelProduct", _localizer["Product"].ToString());
            parameters.Add("LabelProductCategory", _localizer["Product Category"].ToString());
            parameters.Add("LabelProductCode", _localizer["Product Code"].ToString());
            parameters.Add("LabelProductGroup", _localizer["Product Group"].ToString());
            parameters.Add("LabelProductName", _localizer["Product Name"].ToString());
            parameters.Add("LabelProducts", _localizer["Products"].ToString());
            parameters.Add("LabelProductSales", _localizer["Product/Sales"].ToString());
            parameters.Add("LabelProjectedFilingDate", _localizer["Projected Filing Date"].ToString());
            parameters.Add("LabelProjectName", _localizer["Project Name"].ToString());
            parameters.Add("LabelProgram", _localizer["Program"].ToString());
            parameters.Add("LabelProposedUse", _localizer["Proposed Use"].ToString());
            parameters.Add("LabelPTOBaseDate", _localizer["PTO Base Date"].ToString());
            parameters.Add("LabelPTODueDate", _localizer["PTO Due Date"].ToString());
            parameters.Add("LabelPubDate", _localizer["Publication Date"].ToString());
            parameters.Add("LabelPubDateShort", _localizer["Pub Date"].ToString());
            parameters.Add("LabelPublicDisclosure", _localizer["Public Disclosure"].ToString());
            parameters.Add("LabelPublished", _localizer["Published"].ToString());
            parameters.Add("LabelPubNumber", _localizer["Publication Number"].ToString());
            parameters.Add("LabelPubNo", _localizer["Publication No."].ToString());
            parameters.Add("LabelQ1", _localizer["Q1"].ToString());
            parameters.Add("LabelQ2", _localizer["Q2"].ToString());
            parameters.Add("LabelQ3", _localizer["Q3"].ToString());
            parameters.Add("LabelQ4", _localizer["Q4"].ToString());
            parameters.Add("LabelQuarterly", _localizer["Quarterly"].ToString());
            parameters.Add("LabelQuestion", _localizer["Question"].ToString());
            parameters.Add("LabelQuestionnaire", _localizer["Questionnaire"].ToString());
            parameters.Add("LabelQuantitySold", _localizer["Quantity Sold"].ToString());
            parameters.Add("LabelRating", _localizer["Rating"].ToString());
            parameters.Add("LabelRatingRemarks", _localizer["Rating Remarks"].ToString());
            parameters.Add("LabelRatingValue", _localizer["Rating Value"].ToString());
            parameters.Add("LabelRatioPercent", _localizer["Ratio(%)"].ToString());
            parameters.Add("LabelRealAmount", _localizer["Real Amount"].ToString());
            parameters.Add("LabelRealCost", _localizer["Real Cost"].ToString());
            parameters.Add("LabelReasonForChange", _localizer["Reason For Change"].ToString());
            parameters.Add("LabelReceiptPostDate", _localizer["Receipt Post Date"].ToString());
            parameters.Add("LabelRecommendation", _localizer["Recommendation"].ToString());
            parameters.Add("LabelRecordCount", _localizer["Record Count"].ToString());
            parameters.Add("LabelRecurring", _localizer["Recurring"].ToString());
            parameters.Add("LabelReduction", _localizer["Reduction"].ToString());
            parameters.Add("LabelReel", _localizer["Reel"].ToString());
            parameters.Add("LabelReference", _localizer["Reference"].ToString());
            parameters.Add("LabelReferenceDate", _localizer["Reference Date"].ToString());
            parameters.Add("LabelReferences", _localizer["References"].ToString());
            parameters.Add("LabelReferenceSrc", _localizer["Reference Src"].ToString());
            parameters.Add("LabelRegDate", _localizer["Registration Date"].ToString());
            parameters.Add("LabelRegDateShort", _localizer["Reg Date"].ToString());
            parameters.Add("LabelRegistered", _localizer["Registered"].ToString());
            parameters.Add("LabelRegistration", _localizer["Registration"].ToString());
            parameters.Add("LabelRegNumber", _localizer["Registration Number"].ToString());
            parameters.Add("LabelRegistrationNumber", _localizer["Registration Number"].ToString());
            parameters.Add("LabelRegNo", _localizer["Registration No"].ToString());
            parameters.Add("LabelRelatedApplications", _localizer["Related Applications"].ToString());
            parameters.Add("LabelRelatedCase", _localizer["Related Case"].ToString());
            parameters.Add("LabelRelatedCases", _localizer["Related Cases"].ToString());
            parameters.Add("LabelRelatedDisclosures", _localizer["Related Disclosures"].ToString());
            parameters.Add("LabelRelatedInventions", _localizer["Related Inventions"].ToString());
            parameters.Add("LabelRelatedMatter", _localizer["Related Matter"].ToString());
            parameters.Add("LabelRelatedMatters", _localizer["Related Matters"].ToString());
            parameters.Add("LabelRelatedPatent", _localizer["Related Patent"].ToString());
            parameters.Add("LabelRelatedPatents", _localizer["Related Patents"].ToString());
            parameters.Add("LabelRelatedProducts", _localizer["Related Products"].ToString());
            parameters.Add("LabelRelatedSearchRequest", _localizer["Related Search Request"].ToString());
            parameters.Add("LabelRelatedSearchRequests", _localizer["Related Search Requests"].ToString());
            parameters.Add("LabelRelatedTMK", _localizer["Related TMK"].ToString());
            parameters.Add("LabelRelatedTrademarks", _localizer["Related Trademarks"].ToString());
            parameters.Add("LabelRelationship", _localizer["Relationship"].ToString());
            parameters.Add("LabelRemainingAmount", _localizer["Remaining Amount"].ToString());
            parameters.Add("LabelRemarks", _localizer["Remarks"].ToString());
            parameters.Add("LabelRemarksUpdate", _localizer["Remarks Update"].ToString());
            parameters.Add("LabelReminderBatchID", _localizer["Reminder Batch ID"].ToString());
            parameters.Add("LabelRemuneration", _localizer["Remuneration"].ToString());
            parameters.Add("LabelRemunerationType", _localizer["RemunerationType"].ToString());
            parameters.Add("LabelRenew", _localizer["Renew"].ToString());
            parameters.Add("LabelRenewalInstruction", _localizer["Renewal Instruction"].ToString());
            parameters.Add("LabelRenewalReview", _localizer["Renewal Review"].ToString());
            parameters.Add("LabelReplyMessage", _localizer["Reply Message"].ToString());
            parameters.Add("LabelRepresentative", _localizer["Representative"].ToString());
            parameters.Add("LabelRepresentativeIndex", _localizer["Representative Index"].ToString());
            parameters.Add("LabelReportCriteria", _localizer["Report Criteria"].ToString());
            parameters.Add("LabelReportOptions", _localizer["Report Options"].ToString());
            parameters.Add("LabelReportType", _localizer["Report Type"].ToString());
            parameters.Add("LabelRequestedTerms", _localizer["Requested Terms"].ToString());
            parameters.Add("LabelRequestorName", _localizer["Requestor's Name"].ToString());
            parameters.Add("LabelRequestorNames", _localizer["Requestors' Names"].ToString());
            parameters.Add("LabelRespDocketing", _localizer["Responsible (Docketing)"].ToString());
            parameters.Add("LabelRespOffice", _localizer["Responsible Office"].ToString());
            parameters.Add("LabelRespOffices", _localizer["Responsible Offices"].ToString());
            parameters.Add("LabelResponseDate", _localizer["Response Date"].ToString());
            parameters.Add("LabelResponseSentDate", _localizer["Response Sent Date"].ToString());
            parameters.Add("LabelResponsible", _localizer["Responsible"].ToString());
            parameters.Add("LabelResponsibleName", _localizer["Responsible Name"].ToString());
            parameters.Add("LabelResponsiblePerson", _localizer["Responsible Person"].ToString());
            parameters.Add("LabelRespReporting", _localizer["Responsible (Reporting)"].ToString());
            parameters.Add("LabelResultRoyalty", _localizer["Result Royalty"].ToString());
            parameters.Add("LabelReviewerName", _localizer["Reviewer Name"].ToString());
            parameters.Add("LabelReviewers", _localizer["Reviewers"].ToString());
            parameters.Add("LabelReviewerType", _localizer["Reviewer Type"].ToString());
            parameters.Add("LabelRunDate", _localizer["Run Date"].ToString());
            parameters.Add("LabelSales", _localizer["Sales"].ToString());
            parameters.Add("LabelScore", _localizer["Score"].ToString());
            parameters.Add("LabelScreen", _localizer["Screen"].ToString());
            parameters.Add("LabelSentToCPI", _localizer["Sent To CPI"].ToString());
            parameters.Add("LabelServiceFee", _localizer["Service Fee"].ToString());
            parameters.Add("LabelSettlementAmount", _localizer["Settlement Amount"].ToString());
            parameters.Add("LabelSettlementDate", _localizer["Settlement Date"].ToString());
            parameters.Add("LabelSettlementInfo", _localizer["Settlement Info"].ToString());
            parameters.Add("LabelSettlementNo", _localizer["Settlement No"].ToString());
            parameters.Add("LabelShowAllCountries", _localizer["Show All Countries"].ToString());
            parameters.Add("LabelShowDefaultImage", _localizer["Show Default Image"].ToString());
            parameters.Add("LabelShowEntireFamily", _localizer["Show Entire Family"].ToString());
            parameters.Add("LabelShowImage", _localizer["Show Image"].ToString());
            parameters.Add("LabelShowValidatedCountries", _localizer["Show Validated Countries"].ToString());
            parameters.Add("LabelSmallEntity", _localizer["Small Entity"].ToString());
            parameters.Add("LabelSortOrder", _localizer["Sort Order"].ToString());
            parameters.Add("LabelSource", _localizer["Source"].ToString());
            parameters.Add("LabelStackedBar", _localizer["Stacked Bar"].ToString());
            parameters.Add("LabelStage", _localizer["Stage"].ToString());
            parameters.Add("LabelStageOrder", _localizer["Stage Order"].ToString());
            parameters.Add("LabelStandardGoods", _localizer["Standard Goods"].ToString());
            parameters.Add("LabelStartDate", _localizer["Start Date"].ToString());
            parameters.Add("LabelStateOrRegion", _localizer["State/Region"].ToString());
            parameters.Add("LabelStatisticsOf", _localizer["Statistics Of"].ToString());
            parameters.Add("LabelStatus", _localizer["Status"].ToString());
            parameters.Add("LabelStatusDate", _localizer["Status Date"].ToString());
            parameters.Add("LabelStatuses", _localizer["Statuses"].ToString());
            parameters.Add("LabelStatusHistory", _localizer["Status History"].ToString());
            parameters.Add("LabelStatusOfUse", _localizer["Status Of Use-Plans for use"].ToString());
            parameters.Add("LabelStorage", _localizer["Storage"].ToString());
            parameters.Add("LabelSubCase", _localizer["Sub Case"].ToString());
            parameters.Add("LabelSubjectMatters", _localizer["Subject Matters"].ToString());
            parameters.Add("LabelSubmittedDate", _localizer["Submitted Date"].ToString());
            parameters.Add("LabelSubtotal", _localizer["Subtotal"].ToString());
            parameters.Add("LabelSub_Total", _localizer["Sub-Total"].ToString());
            parameters.Add("LabelSummaryOfInvention", _localizer["Summary of Invention"].ToString());
            parameters.Add("LabelSupplier", _localizer["Supplier"].ToString());
            parameters.Add("LabelSystem", _localizer["System"].ToString());
            parameters.Add("LabelTableAndChart", _localizer["Table and Chart"].ToString());
            parameters.Add("LabelTableOnly", _localizer["Table Only"].ToString());
            parameters.Add("LabelTaxAgent", _localizer["Tax " + labelAgent].ToString());
            parameters.Add("LabelTaxAgentName", _localizer["Tax " + labelAgentName].ToString());
            parameters.Add("LabelTaxAmount", _localizer["Tax Amount"].ToString());
            parameters.Add("LabelTaxCost", _localizer["Tax Cost"].ToString());
            parameters.Add("LabelTaxSchedule", _localizer["Tax Schedule"].ToString());
            parameters.Add("LabelTaxScheduleLabel", _localizer["Tax Schedule Label"].ToString());
            parameters.Add("LabelTaxYear", _localizer["Tax Year"].ToString());
            parameters.Add("LabelTelephoneNo", _localizer["Telephone No."].ToString());
            parameters.Add("LabelTerminalDisclaimer", _localizer["Terminal Disclaimer"].ToString());
            parameters.Add("LabelTerminateEnd", _localizer["Terminate/End"].ToString());
            parameters.Add("LabelTerms", _localizer["Terms"].ToString());
            parameters.Add("LabelThis", _localizer["This"].ToString());
            parameters.Add("LabelThisDateFiled", _localizer["This Date Filed"].ToString());
            parameters.Add("LabelThisMonth", _localizer["This Month"].ToString());
            parameters.Add("LabelTimeTrackerAttorney", _localizer["Time Tracker Attorney"].ToString());
            parameters.Add("LabelTitle", _localizer["Title"].ToString());
            parameters.Add("LabelTitleTrademark", _localizer["Title Or Trademark"].ToString());
            parameters.Add("LabelTLUpdateStarDescription", _localizer["* - Trademark Office Data for update"].ToString());
            parameters.Add("LabelTmkStatus", _localizer["Trademark Status"].ToString());
            parameters.Add("LabelTmkTitle", _localizer["Trademark"].ToString());
            parameters.Add("LabelTrackOne", _localizer["Track One"].ToString());
            parameters.Add("LabelTrademark", _localizer["Trademark"].ToString());
            parameters.Add("LabelTrademarkLinks", _localizer["Trademark Links"].ToString());
            parameters.Add("LabelTrademarkName", _localizer["Trademark Name"].ToString());
            parameters.Add("LabelTrademarkOffice", _localizer["Trademark Office"].ToString());
            parameters.Add("LabelTrademarkOfficeAction", _localizer["Trademark Office Action"].ToString());
            parameters.Add("LabelTrademarkOfficeData", _localizer["Trademark Office Data"].ToString());
            parameters.Add("LabelTrademarkOrTagLine", _localizer["Trademark(s)/Tagline"].ToString());
            parameters.Add("LabelTrademarkStatus", _localizer["Trademark Status"].ToString());
            parameters.Add("LabelTransactionDate", _localizer["Transaction Date"].ToString());
            parameters.Add("LabelTransactionHistory", _localizer["Transaction History"].ToString());
            parameters.Add("LabelTransactions", _localizer["Transactions"].ToString());
            parameters.Add("LabelTradeSecretDate", _localizer["Trade Secret Date"].ToString());
            parameters.Add("LabelTranslation", _localizer["Translation"].ToString());
            parameters.Add("LabelTrigger", _localizer["Trigger"].ToString());
            parameters.Add("LabelTriggerValue", _localizer["Trigger Value"].ToString());
            parameters.Add("LabelTOApplicationNo", _localizer["T.O. Application No"].ToString());
            parameters.Add("LabelTOFilingDate", _localizer["T.O. Filing Date"].ToString());
            parameters.Add("LabelTo", _localizer["To"].ToString());
            parameters.Add("LabelToDate", _localizer["To Date"].ToString());
            parameters.Add("LabelTotal", _localizer["Total"].ToString());
            parameters.Add("LabelTotalAmount", _localizer["Total Amount"].ToString());
            parameters.Add("LabelTotalCost", _localizer["Total Cost"].ToString());
            parameters.Add("LabelTotalCostEstimate", _localizer["Total Cost Estimate"].ToString());
            parameters.Add("LabelTotalCostSavings", _localizer["Total Cost Savings"].ToString());
            parameters.Add("LabelTotalInvoice", _localizer["Total Invoice(s)"].ToString());
            parameters.Add("LabelTotalPayOnly", _localizer["Total (Pay Only)"].ToString());
            parameters.Add("LabelTotalsIncludingAnnuities", _localizer["Totals Including Annuities"].ToString());
            parameters.Add("LabelTurnOver", _localizer["Turn Over"].ToString());
            parameters.Add("LabelType", _localizer["Type"].ToString());
            parameters.Add("LabelUnitaryEffectRegDate", _localizer["Unitary Effect Registration Date"].ToString());
            parameters.Add("LabelUnitaryEffectReqDate", _localizer["Unitary Effect Request Date"].ToString());
            parameters.Add("LabelUnitPrice", _localizer["Unit Price"].ToString());
            parameters.Add("LabelUnmapped", _localizer["Unmapped"].ToString());
            parameters.Add("LabelUPCStatus", _localizer["UPC Status"].ToString());
            parameters.Add("LabelUPCStatusDate", _localizer["UPC Status Date"].ToString());
            parameters.Add("LabelUpdatedBy", _localizer["Updated By"].ToString());
            parameters.Add("LabelUpdatedByAndDate", _localizer["Updated"].ToString());
            parameters.Add("LabelUpdatedDate", _localizer["Updated Date"].ToString());
            parameters.Add("LabelUpdatedField", _localizer["Updated Field"].ToString());
            parameters.Add("LabelUploadedBy", _localizer["Uploaded By"].ToString());
            parameters.Add("LabelUploadedDate", _localizer["Uploaded Date"].ToString());
            parameters.Add("LabelUseDefaultExchangeRate", _localizer["Use Default Exchange Rate"].ToString());
            parameters.Add("LabelUser", _localizer["User"].ToString());
            parameters.Add("LabelUserRemarks", _localizer["User Remarks"].ToString());
            parameters.Add("LabelValidatedCountries", _localizer["Validated Countries"].ToString());
            parameters.Add("LabelValuationMatrix", _localizer["Valuation Matrix"].ToString());
            parameters.Add("LabelValue", _localizer["Value"].ToString());
            parameters.Add("LabelVariable", _localizer["Variable"].ToString());
            parameters.Add("LabelVariance", _localizer["Variance"].ToString());
            parameters.Add("LabelVAT", _localizer["VAT"].ToString());
            parameters.Add("LabelWebSite", _localizer["Web Site"].ToString());
            parameters.Add("LabelWorkflowName", _localizer["Workflow Name"].ToString());
            parameters.Add("LabelWorkflowOrder", _localizer["Workflow Order"].ToString());
            parameters.Add("LabelYear", _localizer["Year"].ToString());
            parameters.Add("LabelYearly", _localizer["Yearly"].ToString());
            parameters.Add("LabelYearToDate", _localizer["Year to date"].ToString());
            parameters.Add("LabelYes", _localizer["Yes"].ToString());
            parameters.Add("LabelYour", _localizer["Your"].ToString());
            parameters.Add("LabelYourBaseDate", _localizer["Your Base Date"].ToString());
            parameters.Add("LabelYourData", _localizer["Your Data"].ToString());
            parameters.Add("LabelYourMatchingActionTypeOrDue", _localizer["Your Matching Action Type/Action Due"].ToString());
            parameters.Add("LabelYourReference", _localizer["Your Reference"].ToString());
            parameters.Add("LabelYourStatus", _localizer["Your Status"].ToString());
            parameters.Add("LabelYr", _localizer["Yr"].ToString());
            parameters.Add("LabelEPOValidation", _localizer["EPO Validation"].ToString());
            parameters.Add("LabelExclude", _localizer["Exclude?"].ToString());
            parameters.Add("LabelPatentScore", _localizer["Patent Score"].ToString());
            parameters.Add("LabelFRFirstPayment", _localizer["Invention Report Award"].ToString());
            parameters.Add("LabelFRSecondPayment", _localizer["First filing Award"].ToString());
            parameters.Add("LabelFRThridPayment", _localizer["Use (first sell) Award"].ToString());
            parameters.Add("LabelFRFirstPaymentDate", _localizer["Payment Date"].ToString());
            parameters.Add("LabelFRSecondPaymentDate", _localizer["Payment Date"].ToString());
            parameters.Add("LabelFRThridPaymentDate", _localizer["Payment Date"].ToString());
            parameters.Add("LabelIDSCrossCheckFooter", _localizer["Discrepancy between Base Case and Compared Case"].ToString());
            parameters.Add("LabelLetSubCategory", _localizer["Letter Sub Category"].ToString());
            parameters.Add("LabelDQCategory", _localizer["Custom Query Category"].ToString());
            parameters.Add("LabelQECategory", _localizer["Quick Email Category"].ToString());
            parameters.Add("LabelDMSRecommendationTradeSecret", _localizer["Confidential Data"].ToString());

            if (!parameters.ContainsKey("LabelReportTitle")) {
                ReportTitleHelper titleHelper = new ReportTitleHelper(await GetReportParameterLabels());
                parameters.Add("LabelReportTitle", _localizer[titleHelper.GetType().GetProperty(rt.ToString()).GetValue(titleHelper).ToString()].ToString());
            }

            //for individual report
            await AddReportParameters(parameters, rt);
        }

        private async Task AddSettingParameters(Dictionary<string, object> parameters, ReportType rt)
        {
            SystemTypeForReport systemType = GetSystemType(rt);
            var patSettings = await _patSettings.GetSetting();
            var tmkSettings = await _tmkSettings.GetSetting();
            var defaultSettings = await _defaultSettings.GetSetting();

            switch (systemType)
                {
                    case SystemTypeForReport.Patent:
                        parameters.Add("SettingIsPatRespOfficeON", _httpContextAccessor.HttpContext.User.IsRespOfficeOn(SystemType.Patent));
                        parameters.Add("SettingIsProductsON", patSettings.IsProductsOn);
                        parameters.Add("SettingIsInventionProductON", patSettings.IsInventionProductOn);
                        parameters.Add("SettingIsSubjectMattersOn", patSettings.IsSubjectMattersOn);
                        parameters.Add("SettingIsCorporation", defaultSettings.IsCorporation);
                        parameters.Add("SettingIsPatentScoreOn", patSettings.IsPatentScoreOn);
                    break;
                    case SystemTypeForReport.Trademark:
                        parameters.Add("SettingIsTmkRespOfficeON", _httpContextAccessor.HttpContext.User.IsRespOfficeOn(SystemType.Trademark));
                        parameters.Add("SettingIsProductsON", tmkSettings.IsProductsOn);
                        break;
                default:
                        parameters.Add("SettingIsPatRespOfficeON", _httpContextAccessor.HttpContext.User.IsRespOfficeOn(SystemType.Patent));
                        parameters.Add("SettingIsTmkRespOfficeON", _httpContextAccessor.HttpContext.User.IsRespOfficeOn(SystemType.Trademark));
                        parameters.Add("SettingIsProductsON", patSettings.IsProductsOn || patSettings.IsInventionProductOn || tmkSettings.IsProductsOn);
                        parameters.Add("SettingIsSoftDocketOn", defaultSettings.IsSoftDocketOn);
                    break;
                }

            parameters.Add("SettingIsDeDocketOn", defaultSettings.IsDeDocketOn);
            parameters.Add("IsShowCustomFieldOn", defaultSettings.IsShowCustomFieldOn);
            parameters.Add("SettingIsSharePointIntegrationOn", defaultSettings.IsSharePointIntegrationOn);
            parameters.Add("SettingDateFormat", defaultSettings.ReportDateFormat);
            parameters.Add("SettingDateTimeFormat", defaultSettings.ReportDateTimeFormat);
            parameters.Add("SettingCurrencyFormat", defaultSettings.ReportCurrencyFormat);
            parameters.Add("SettingRecordDelimiter", defaultSettings.ReportExcelRecordDelimiter);
            parameters.Add("SettingFieldDelimiter", defaultSettings.ReportExcelFieldDelimiter);
            parameters.Add("SettingReportDetailFontSize", defaultSettings.ReportDetailFontSize);
            parameters.Add("SettingReportCriteriaFontSize", defaultSettings.ReportCriteriaFontSize);
            parameters.Add("SettingReportReportHideSubFormLabel", defaultSettings.ReportHideSubFormLabel);
            parameters.Add("SettingReportHeaderShadingColor", defaultSettings.ReportHeaderShadingColor);
            parameters.Add("SettingReportMainInfoShadingColor", defaultSettings.ReportMainInfoShadingColor);
            parameters.Add("SettingReportSubReportsHeaderShadingColor", defaultSettings.ReportSubReportsHeaderShadingColor);
            parameters.Add("SettingReportCriteriaHeaderShadingColor", defaultSettings.ReportCriteriaHeaderShadingColor);
            parameters.Add("SettingMultiCurrencyOn", defaultSettings.ReportMultiCurrencyOn);
            parameters.Add("SettingDefaultCurrencySymbol", defaultSettings.ReportCurrencyFormat.Substring(1, 1));
            parameters.Add("SettingIsPatentWatchOn", defaultSettings.ReportPatentWatchOn);
            parameters.Add("SettingIsDelegationOn", defaultSettings.IsDelegationOn);
            parameters.Add("SettingClientName", defaultSettings.ClientName);
            parameters.Add("SettingClientCode", await _defaultSettings.GetValue<string>("CPIClientCode", "1"));
        }

        private async Task AddReportParameters(Dictionary<string, object> parameters, ReportType rt)
        {
            var patSettings = await _patSettings.GetSetting();
            var tmkSettings = await _tmkSettings.GetSetting();
            var sharedSettings = await _defaultSettings.GetSetting();
            var labelAttorney1 = sharedSettings.GetType().GetProperty("LabelAttorney1").GetValue(sharedSettings).ToString();
            var labelAttorney2 = sharedSettings.GetType().GetProperty("LabelAttorney2").GetValue(sharedSettings).ToString();
            var labelAttorney3 = sharedSettings.GetType().GetProperty("LabelAttorney3").GetValue(sharedSettings).ToString();
            var labelAttorney4 = sharedSettings.GetType().GetProperty("LabelAttorney4").GetValue(sharedSettings).ToString();
            var labelAttorney5 = sharedSettings.GetType().GetProperty("LabelAttorney5").GetValue(sharedSettings).ToString();

            string patCostEstimatorCurrencyFormat = patSettings.CostEstimatorCurrencyFormat ?? "";
            string[] patCurrencyParts = patCostEstimatorCurrencyFormat.Split('|');
            string patCurrencyCode = "USD";
            string patCurrencySymbol = "$";
            string patCurrencyDescription = "in US Dollars";
            if (patCurrencyParts.Length >= 3) 
            {
                patCurrencyCode = patCurrencyParts[0];
                patCurrencySymbol = patCurrencyParts[1];
                patCurrencyDescription = patCurrencyParts[2];
            }
            
            string tmkCostEstimatorCurrencyFormat = tmkSettings.CostEstimatorCurrencyFormat ?? "";
            string[] tmkCurrencyParts = tmkCostEstimatorCurrencyFormat.Split('|');
            string tmkCurrencyCode = "USD";
            string tmkCurrencySymbol = "$";
            string tmkCurrencyDescription = "in US Dollars";
            if (tmkCurrencyParts.Length >= 3) 
            {
                tmkCurrencyCode = tmkCurrencyParts[0];
                tmkCurrencySymbol = tmkCurrencyParts[1];
                tmkCurrencyDescription = tmkCurrencyParts[2];
            }

            switch (rt)
            {
                case ReportType.PatCountryApplicationPrintScreen:
                    parameters.Add("SettingIsBilingNoON", patSettings.IsBillingNoOn);
                    parameters.Add("SettingIsStorageON", patSettings.IsStorageOn);
                    parameters.Add("SettingIsExportControlON", patSettings.IsExportControlOn && !_httpContextAccessor.HttpContext.User.RestrictExportControl());
                    parameters.Add("SettingIsTerminalDisclaimerOn", patSettings.IsTerminalDisclaimerOn);
                    parameters.Add("SettingCountriesWithTaxSchedAndClaimField", patSettings.CountriesWithTaxSchedAndClaimField);
                    parameters.Add("SettingCountriesWithNationalField", patSettings.CountriesWithNationalField);
                    parameters.Add("SettingCountriesWithConfirmationField", patSettings.CountriesWithConfirmationField);
                    //parameters.Add("LabelGMCaseNumber", gmSettings.IsClientMatterOn?gmSettings.LabelClientMatter:gmSettings.LabelCaseNumber);
                    parameters.Add("LabelTmkCaseNumber", _localizer[tmkSettings.IsClientMatterOn? tmkSettings.LabelClientMatter: tmkSettings.LabelCaseNumber].ToString());
                    parameters.Add("LabelDMSCaseNumber", _localizer["Case Number"]);
                    parameters.Add("LabelCustomFieldsLabel", _localizer[patSettings.CACustomFieldsTabLabel]);
                    parameters.Add("HidePDFAttorney1Label", labelAttorney1.Equals("Attorney 1"));
                    parameters.Add("HidePDFAttorney2Label", labelAttorney2.Equals("Attorney 2"));
                    parameters.Add("HidePDFAttorney3Label", labelAttorney3.Equals("Attorney 3"));
                    parameters.Add("HidePDFAttorney4Label", labelAttorney4.Equals("Attorney 4"));
                    parameters.Add("HidePDFAttorney5Label", labelAttorney5.Equals("Attorney 5"));
                    break;
                case ReportType.PatPatentList:
                    parameters.Add("SettingIsInAMS", _httpContextAccessor.HttpContext.User.IsInSystem(SystemType.AMS));
                    break;
                case ReportType.PatInventionPrintScreen:
                    parameters.Add("LabelCustomFieldsLabel", _localizer[patSettings.InvCustomFieldsTabLabel]);
                    parameters.Add("HidePDFAttorney1Label", labelAttorney1.Equals("Attorney 1"));
                    parameters.Add("HidePDFAttorney2Label", labelAttorney2.Equals("Attorney 2"));
                    parameters.Add("HidePDFAttorney3Label", labelAttorney3.Equals("Attorney 3"));
                    parameters.Add("HidePDFAttorney4Label", labelAttorney4.Equals("Attorney 4"));
                    parameters.Add("HidePDFAttorney5Label", labelAttorney5.Equals("Attorney 5"));
                    break;
                case ReportType.TmkTrademarkPrintScreen:
                    parameters.Add("LabelCustomFieldsLabel", _localizer[tmkSettings.TmkCustomFieldsTabLabel]);
                    parameters.Add("HidePDFAttorney1Label", labelAttorney1.Equals("Attorney 1"));
                    parameters.Add("HidePDFAttorney2Label", labelAttorney2.Equals("Attorney 2"));
                    parameters.Add("HidePDFAttorney3Label", labelAttorney3.Equals("Attorney 3"));
                    parameters.Add("HidePDFAttorney4Label", labelAttorney4.Equals("Attorney 4"));
                    parameters.Add("HidePDFAttorney5Label", labelAttorney5.Equals("Attorney 5"));
                    break;
                case ReportType.SRSearchRequestPrintScreen:
                    parameters.Add("LabelTmkCaseNumber", _localizer[tmkSettings.IsClientMatterOn ? tmkSettings.LabelClientMatter : tmkSettings.LabelCaseNumber].ToString());
                    break;
                case ReportType.SRSearchRequestList:
                    parameters.Add("LabelTmkCaseNumber", _localizer[tmkSettings.IsClientMatterOn ? tmkSettings.LabelClientMatter : tmkSettings.LabelCaseNumber].ToString());
                    parameters.Add("SettingIsInTrademark", _httpContextAccessor.HttpContext.User.IsInSystem(SystemType.Trademark));
                    break;
                case ReportType.SharedProductPrintScreen:
                    parameters.Add("LabelTmkCaseNumber", _localizer[tmkSettings.IsClientMatterOn ? tmkSettings.LabelClientMatter : tmkSettings.LabelCaseNumber].ToString());
                    break;
                case ReportType.SharedClientPrintScreen:
                    parameters.Add("SettingIsInPatent", _httpContextAccessor.HttpContext.User.IsInSystem(SystemType.Patent));
                    parameters.Add("SettingIsInTrademark", _httpContextAccessor.HttpContext.User.IsInSystem(SystemType.Trademark));
                    parameters.Add("SettingIsShowReviewer", false);
                    break;
                case ReportType.SharedDueDateList:
                    parameters.Add("SettingIsInPatentOrIDS", _httpContextAccessor.HttpContext.User.IsInSystem(SystemType.Patent)|| _httpContextAccessor.HttpContext.User.IsInRoles(SystemType.DMS, CPiPermissions.DMSDueDateList));
                    parameters.Add("SettingIsInTrademark", _httpContextAccessor.HttpContext.User.IsInSystem(SystemType.Trademark));
                    parameters.Add("SettingIsInIDSPermissionOnly", !(_httpContextAccessor.HttpContext.User.IsInSystem(SystemType.Patent) ||_httpContextAccessor.HttpContext.User.IsInSystem(SystemType.Trademark) || _httpContextAccessor.HttpContext.User.IsInSystem(SystemType.GeneralMatter) || _httpContextAccessor.HttpContext.User.IsInRoles(SystemType.AMS, CPiPermissions.RegularUser)));
                    parameters.Add("SettingIsLicenseesOn",
                        (_httpContextAccessor.HttpContext.User.IsInSystem(SystemType.Patent) && patSettings.IsLicenseesOn)
                        || (_httpContextAccessor.HttpContext.User.IsInSystem(SystemType.Trademark) && tmkSettings.IsLicenseesOn));
                    parameters.Add("SettingIsInRTS", _httpContextAccessor.HttpContext.User.IsInSystem(SystemType.Patent) && (patSettings.IsRTSOn));
                    parameters.Add("SettingIsPatSystemOn", _httpContextAccessor.HttpContext.User.IsInSystem(SystemType.Patent));
                    break;
                case ReportType.SharedDueDateListConcise:
                    parameters.Add("SettingIsInPatentOrIDS", _httpContextAccessor.HttpContext.User.IsInSystem(SystemType.Patent) || _httpContextAccessor.HttpContext.User.IsInRoles(SystemType.DMS, CPiPermissions.DMSDueDateList));
                    parameters.Add("SettingIsInTrademark", _httpContextAccessor.HttpContext.User.IsInSystem(SystemType.Trademark));
                    parameters.Add("SettingIsInIDSPermissionOnly", !(_httpContextAccessor.HttpContext.User.IsInSystem(SystemType.Patent) || _httpContextAccessor.HttpContext.User.IsInSystem(SystemType.Trademark) || _httpContextAccessor.HttpContext.User.IsInSystem(SystemType.GeneralMatter) || _httpContextAccessor.HttpContext.User.IsInRoles(SystemType.AMS, CPiPermissions.RegularUser)));
                    parameters.Add("SettingIsLicenseesOn",
                        (_httpContextAccessor.HttpContext.User.IsInSystem(SystemType.Patent) && patSettings.IsLicenseesOn)
                        || (_httpContextAccessor.HttpContext.User.IsInSystem(SystemType.Trademark) && tmkSettings.IsLicenseesOn));
                    parameters.Add("SettingIsInRTS", _httpContextAccessor.HttpContext.User.IsInSystem(SystemType.Patent) && (patSettings.IsRTSOn));
                    parameters.Add("SettingIsPatSystemOn", _httpContextAccessor.HttpContext.User.IsInSystem(SystemType.Patent));
                    break;
                case ReportType.SharedDueDateDelegationReport:
                    parameters.Add("SettingIsInPatentOrIDS", _httpContextAccessor.HttpContext.User.IsInSystem(SystemType.Patent) || _httpContextAccessor.HttpContext.User.IsInRoles(SystemType.DMS, CPiPermissions.DMSDueDateList));
                    parameters.Add("SettingIsInTrademark", _httpContextAccessor.HttpContext.User.IsInSystem(SystemType.Trademark));
                    parameters.Add("SettingIsInIDSPermissionOnly", !(_httpContextAccessor.HttpContext.User.IsInSystem(SystemType.Patent) || _httpContextAccessor.HttpContext.User.IsInSystem(SystemType.Trademark) || _httpContextAccessor.HttpContext.User.IsInSystem(SystemType.GeneralMatter) || _httpContextAccessor.HttpContext.User.IsInRoles(SystemType.AMS, CPiPermissions.RegularUser)));
                    parameters.Add("SettingIsLicenseesOn",
                        (_httpContextAccessor.HttpContext.User.IsInSystem(SystemType.Patent) && patSettings.IsLicenseesOn)
                        || (_httpContextAccessor.HttpContext.User.IsInSystem(SystemType.Trademark) && tmkSettings.IsLicenseesOn));
                    parameters.Add("SettingIsInRTS", _httpContextAccessor.HttpContext.User.IsInSystem(SystemType.Patent) && (patSettings.IsRTSOn));
                    break;
                case ReportType.SharedDueDateListCalendar:
                    parameters.Add("SettingIsInTrademark", _httpContextAccessor.HttpContext.User.IsInSystem(SystemType.Trademark));
                    parameters.Add("SettingIsInIDSPermissionOnly", !(_httpContextAccessor.HttpContext.User.IsInSystem(SystemType.Patent) || _httpContextAccessor.HttpContext.User.IsInSystem(SystemType.Trademark) || _httpContextAccessor.HttpContext.User.IsInSystem(SystemType.GeneralMatter) || _httpContextAccessor.HttpContext.User.IsInRoles(SystemType.AMS, CPiPermissions.RegularUser)));
                    parameters.Add("SettingIsPatSystemOn", _httpContextAccessor.HttpContext.User.IsInSystem(SystemType.Patent));
                    break;
                case ReportType.PLLegalStatusReport:
                    parameters.Add("LabelActiveLegalStatusOption", _localizer["Active Cases in Legal Status but Inactive in the Country Application"].ToString());
                    parameters.Add("LabelInactiveLegalStatusOption", _localizer["Inactive Cases in Legal Status but Active in the Country Application"].ToString());
                    parameters.Add("LabelAllLegalStatusOption", _localizer["All statuses in Legal Status and Country Application that do not match"].ToString());
                    break;
                case ReportType.PatFamilyTreePrintScreen:
                    parameters.Add("LabelFamilyTreeView", _localizer["Family Tree View"].ToString());
                    parameters.Add("LabelFamilyTreeViewDetails", _localizer["Family Tree View Detail"].ToString());
                    break;
                case ReportType.TmkFamilyTreePrintScreen:
                    parameters.Add("LabelFamilyTreeView", _localizer["Family Tree View"].ToString());
                    parameters.Add("LabelFamilyTreeViewDetails", _localizer["Family Tree View Detail"].ToString());
                    break;
                case ReportType.PatInventorPrintScreen:
                    parameters.Add("LabelTotalIssuedActiveCasesToDate", _localizer["Total Issued Active Cases to Date"].ToString());
                    parameters.Add("LabelTotalIssuedActiveCasesThisYear", _localizer["Total Issued Active Cases This Year"].ToString());
                    parameters.Add("LabelTotalAwards", _localizer["Total Awards"].ToString());
                    parameters.Add("SettingRestrictInventorInfo", (await _userSettingManager.GetUserSetting<UserAccountSettings>(_httpContextAccessor.HttpContext.User.GetUserIdentifier())).RestrictInventorInfo);
                    break;
                case ReportType.PatMasterList:
                    parameters.Add("SettingSplitApplicationFields", patSettings.ReportSplitMasterListApplicationFields);
                    parameters.Add("SettingIsInGeneralMatter", _httpContextAccessor.HttpContext.User.IsInSystem(SystemType.GeneralMatter));
                    break;
                case ReportType.PatCECostEstimatorPrintScreen:
                    parameters.Add("LabelCostEstimatorDetail", _localizer["Cost Estimator Detail"].ToString());
                    parameters.Add("LabelCostEstimatorSummary", _localizer["Cost Estimator Summary"].ToString());
                    parameters.Add("SettingPatCostEstimatorCurrencyCode", _localizer[patCurrencyCode].ToString());
                    parameters.Add("SettingPatCostEstimatorCurrencySymbol", _localizer[patCurrencySymbol].ToString());
                    parameters.Add("SettingPatCostEstimatorCurrencyDescription", _localizer[patCurrencyDescription].ToString());
                    break;
                case ReportType.TmkCECostEstimatorPrintScreen:
                    parameters.Add("LabelCostEstimatorDetail", _localizer["Cost Estimator Detail"].ToString());
                    parameters.Add("LabelCostEstimatorSummary", _localizer["Cost Estimator Summary"].ToString());
                    parameters.Add("SettingTmkCostEstimatorCurrencyCode", _localizer[tmkCurrencyCode].ToString());
                    parameters.Add("SettingTmkCostEstimatorCurrencySymbol", _localizer[tmkCurrencySymbol].ToString());
                    parameters.Add("SettingTmkCostEstimatorCurrencyDescription", _localizer[tmkCurrencyDescription].ToString());
                    break;
                case ReportType.DMSAgendaPrintScreen:
                    parameters.Add("SettingReviewerEntityType", 0);
                    break;
                default:
                    break;
            }

        }

        private SystemTypeForReport GetSystemType(ReportType rt)
        {
            if (rt.ToString().StartsWith("Pat") || rt.ToString().StartsWith("PL"))
            {
                return SystemTypeForReport.Patent;
            }
            else if (rt.ToString().StartsWith("Tmk") || rt.ToString().StartsWith("TL"))
            {
                return SystemTypeForReport.Trademark;
            }
            else if (rt.ToString().StartsWith("GM"))
            {
                return SystemTypeForReport.GeneralMatter;
            }
            else if (rt.ToString().StartsWith("AMS"))
            {
                return SystemTypeForReport.AMS;
            }
            else if (rt.ToString().StartsWith("Tmc"))
            {
                return SystemTypeForReport.Tmc;
            }
            else if (rt.ToString().StartsWith("Pac"))
            {
                return SystemTypeForReport.Pac;
            }
            else if (rt.ToString().StartsWith("SR"))
            {
                return SystemTypeForReport.Tmc;
            }
            else if (rt.ToString().StartsWith("DMS"))
            {
                return SystemTypeForReport.DMS;
            }
            else
            {
                return SystemTypeForReport.Shared;
            }
        }

        private async Task<object> GetSetting(SystemTypeForReport systemType)
        {
            switch (systemType)
            {
                case SystemTypeForReport.Patent:
                    return await _patSettings.GetSetting();
                case SystemTypeForReport.Trademark:
                    return await _tmkSettings.GetSetting();
                case SystemTypeForReport.GeneralMatter:
                case SystemTypeForReport.AMS:
                case SystemTypeForReport.DMS:
                    return await _defaultSettings.GetSetting();
                default:
                    return await _defaultSettings.GetSetting();
            }
        }

        private class ReportTitleHelper
        {
            Dictionary<string, object> parameters;

            public ReportTitleHelper(Dictionary<string, object> parameters)
            {
                this.parameters = parameters;
            }

            public string SharedDueDateList { get { return "Due Date List"; } }
            public string SharedDueDateListConcise { get { return "Due Date List"; } }
            public string PatMasterList { get { return "Master List Report"; } }
            public string SharedClientPrintScreen { get { return parameters.GetValueOrDefault("ClientLabel") + " Detail"; } }
            public string SharedAgentPrintScreen { get { return parameters.GetValueOrDefault("AgentLabel") + " Detail"; } }
            public string SharedOwnerPrintScreen { get { return parameters.GetValueOrDefault("OwnerLabel") + " Detail"; } }
            public string SharedAttorneyPrintScreen { get { return "Attorney Detail"; } }
            public string SharedContactPersonPrintScreen { get { return "Contact Person Detail"; } }
            public string PatInventorPrintScreen { get { return "Patent Inventor Detail"; } }
            public string TmkTrademarkPrintScreen { get { return "Trademark Detail"; } }
            public string TmkTrademarkList { get { return "Trademark List Report"; } }
            public string TmkPendingRegistered { get { return "Pending/Registered Trademark"; } }
            public string PatCountryApplicationPrintScreen { get { return "Country Application Detail"; } }
            public string PatInventionPrintScreen { get { return "Invention Detail"; } }
            public string PatDisclosureStatus { get { return "Disclosure Status Report"; } }
            public string PatPatentList { get { return "Patent Status List Report"; } }
            public string PatPendingGrantedList { get { return "Pending/Granted Report"; } }
            public string GMMatterList { get { return "Matter List Report"; } }
            public string PatInventorIndex { get { return "Inventor Index Report"; } }
            public string PatKeywordIndex { get { return parameters.GetValueOrDefault("KeywordLabel") + " Index Report"; } }
            public string TmkRenewal { get { return "Trademark Renewal Report"; } }
            public string PatStatistics { get { return "Statistics Report"; } }
            public string SharedCostTracking { get { return "Cost Tracking Report"; } }
            public string AMSCostProjection { get { return "Cost Projection Report"; } }
            public string TmkStatistics { get { return "Statistics Report"; } }
            public string TmkConflict { get { return "Trademark Conflict Report"; } }
            public string PatActionTypePrintScreen { get { return "Patent Action Type Detail"; } }
            public string TmkActionTypePrintScreen { get { return "Trademark Action Type Detail"; } }
            public string GMActionTypePrintScreen { get { return "General Matter Action Type Detail"; } }
            public string AMSCostSummary { get { return "Cost Summary Report"; } }
            public string AMSPaymentConfirmation { get { return "Payment Confirmation List"; } }
            public string PatCostTrackingPrintScreen { get { return "Patent Cost Tracking Detail"; } }
            public string PatActionDuePrintScreen { get { return "Patent Action Due Detail"; } }
            public string PatActionDueInvPrintScreen { get { return "Patent Invention Action Due Detail"; } }
            public string TmkCostTrackingPrintScreen { get { return "Trademark Cost Tracking Detail"; } }
            public string TmkActionDuePrintScreen { get { return "Trademark Action Due Detail"; } }
            public string GMCostTrackingPrintScreen { get { return "General Matter Cost Tracking Detail"; } }
            public string GMActionDuePrintScreen { get { return "General Matter Action Due Detail"; } }
            public string PatCountryLawPrintScreen { get { return "Patent Country Law Detail"; } }
            public string TmkCountryLawPrintScreen { get { return "Trademark Country Law Detail"; } }
            public string TmkConflictPrintScreen { get { return "Trademark Conflict Detail"; } }
            public string PatAreaPrintScreen { get { return "Patent Area Detail"; } }
            public string PatAssignmentStatusPrintScreen { get { return "Patent Assignment Status Detail"; } }
            public string PatCaseTypePrintScreen { get { return "Patent Case Type Detail"; } }
            public string TmkAreaPrintScreen { get { return "Trademark Area Detail"; } }
            public string TmkAssignmentStatusPrintScreen { get { return "Trademark Assignment Status Detail"; } }
            public string TmkCaseTypePrintScreen { get { return "Trademark Case Type Detail"; } }
            public string GMAreaPrintScreen { get { return "General Matter Area Detail"; } }
            public string PatCountryPrintScreen { get { return "Patent Country Detail"; } }
            public string PatCostTypePrintScreen { get { return "Patent Cost Type Detail"; } }
            public string TmkCountryPrintScreen { get { return "Trademark Country Detail"; } }
            public string TmkCostTypePrintScreen { get { return "Trademark Cost Type Detail"; } }
            public string GMCountryPrintScreen { get { return "General Matter Country Detail"; } }
            public string GMCostTypePrintScreen { get { return "General Matter Cost Type Detail"; } }
            public string SharedLanguagePrintScreen { get { return "Language Detail"; } }
            public string SharedCurrencyTypePrintScreen { get { return "Currency Type Detail"; } }
            public string PatTaxSchedulePrintScreen { get { return "Patent Tax Schedule Detail"; } }
            public string PatApplicationStatusPrintScreen { get { return "Patent Application Status Detail"; } }
            public string PatIndicatorPrintScreen { get { return "Patent Indicator Detail"; } }
            public string PatDisclosureStatusPrintScreen { get { return "Patent Disclosure Status Detail"; } }
            public string TmkIndicatorPrintScreen { get { return "Trademark Indicator Detail"; } }
            public string TmkTrademarkStatusPrintScreen { get { return "Trademark Status Detail"; } }
            public string GMIndicatorPrintScreen { get { return "General Matter Indicator Detail"; } }
            public string GMMatterStatusPrintScreen { get { return "General Matter Status Detail"; } }
            public string PatIDSCrossCheck { get { return "IDS Cross Check Report"; } }
            public string TmkConflictStatusPrintScreen { get { return "Trademark Conflict Status Detail"; } }
            public string TmkMarkTypePrintScreen { get { return "Trademark Mark Type Detail"; } }
            public string TmkStandardGoodPrintScreen { get { return "Trademark Standard Good Detail"; } }
            public string SharedDeDocketInstructionPrintScreen { get { return "DeDocket Instruction Detail"; } }
            public string PatIDSReferenceSourcePrintScreen { get { return "Patent IDS Reference Src Detail"; } }
            public string GMMatterPrintScreen { get { return "General Matter Detail"; } }
            public string GMOtherPartyTypePrintScreen { get { return "General Matter Other Party Type Detail"; } }
            public string GMOtherPartyPrintScreen { get { return "General Matter Other Party Detail"; } }
            public string GMMatterTypePrintScreen { get { return "General Matter Type Detail"; } }
            public string GMExtentPrintScreen { get { return "General Matter Extent Detail"; } }
            public string GMAgreenmentTypePrintScreen { get { return "General Matter Agreement Type Detail"; } }
            public string DMSRecommendationPrintScreen { get { return "Invention Disclosure Recommendation Detail"; } }
            public string DMSRatingPrintScreen { get { return "Invention Disclosure Rating Detail"; } }
            public string DMSIndicatorPrintScreen { get { return "Invention Disclosure Indicator Detail"; } }
            public string DMSDisclosureStatusPrintScreen { get { return "Invention Disclosure Status Detail"; } }
            public string DMSActionTypePrintScreen { get { return "Invention Disclosure Action Type Detail"; } }
            public string DMSDisclosurePrintScreen { get { return "Invention Disclosure Detail"; } }
            public string DMSDisclosureDocuSignPrintScreen { get { return "Invention Disclosure Detail"; } }
            public string DMSReviewPrintScreen { get { return "Invention Disclosure Review Detail"; } }
            public string DMSActionDuePrintScreen { get { return "Invention Disclosure Action Due Detail"; } }
            public string DMSAgendaPrintScreen { get { return "Invention Disclosure Agenda Meeting Detail"; } }
            public string PLUSReport { get { return "USPTO Reports"; } }//determined in report file
            public string PLEPOReport { get { return "EPO Reports"; } }//determined in report file
            public string PLActionCompareException { get { return "Action Compare Exception"; } }
            public string PLLegalStatusReport { get { return "Legal Status Report"; } }
            public string PatFamilyTreePrintScreen { get { return "Family Tree Detail"; } }//static in Report file
            public string TmkFamilyTreePrintScreen { get { return "Family Tree Detail"; } }//static in Report file
            public string AMSReminder { get { return "Patent Renewal Instruction List"; } }
            public string AMSAnnuitiesDuePrintScreen { get { return "Annuity Due Detail"; } }
            public string AMSMainDetailPrintScreen { get { return "Annuity Main Detail"; } }
            public string PatCostSummaryReport { get { return "Cost Summary Report"; } }
            public string TmkCostSummaryReport { get { return "Cost Summary Report"; } }
            public string PatInventorAwardCriteriaPrintScreen { get { return "Inventor Award Criteria Detail"; } }
            public string PLOpponentIndexReport { get { return "Opponent Index"; } }
            public string TLUSTTAB { get { return "TTAB Report"; } }
            public string PatProductIndex { get { return "Product Index Report"; } }
            public string AMSClientConfirmationReport => "Confirmation Report";
            public string AMSAgentConfirmationReport => "Agent Responsibility Report";
            public string AMSInstructionsToCPi => "Instructions to Computer Packages Inc.";
            public string AMSPortfolioReport => "Patent Instruction Report";
            public string AMSPortfolioReportFamily => "Patent Portfolio Report";
            public string AMSPortfolioReportWithImage => "Patent Instruction Report";
            public string AMSPortfolioReportWithImageFamily => "Patent Portfolio Report";

            public string TmkProductIndex { get { return "Product Index Report"; } }
            public string SharedDueDateListCalendar { get { return "Due Date List(Calendar)"; } }//no title needed

            public string PatOfficeActionPrintScreen { get { return "Patent Office Action Detail"; } }
            public string PatInventorAwards { get { return "Inventor Awards"; } }
            public string SharedQuickDocket { get { return "Quick Docket"; } }
            public string DMSDisclosureStatus { get { return "Disclosure Status Report"; } }
            public string DMSInventorIndex { get { return "Inventor Index Report"; } }
            public string DMSKeywordIndex { get { return parameters.GetValueOrDefault("KeywordLabel") + " Index Report"; } }
            public string AMSReceiptManagement { get { return "Receipt Management"; } }
            public string SRSearchRequestPrintScreen { get { return "Search Request Detail"; } }
            public string SRSearchRequestWorkflowPrintScreen { get { return "Search Request Workflow Detail"; } }
            public string SRSearchRequestStatusPrintScreen { get { return "Search Request Status Detail"; } }
            public string SRSearchRequestList { get { return "Search Request List"; } }
            public string TmkKeywordIndex { get { return parameters.GetValueOrDefault("KeywordLabel") + " Index Report"; } }
            public string PatWorkflowPrintScreen { get { return "Workflow Detail"; } }
            public string TmkWorkflowPrintScreen { get { return "Workflow Detail"; } }
            public string GMWorkflowPrintScreen { get { return "Workflow Detail"; } }
            public string DMSWorkflowPrintScreen { get { return "Workflow Detail"; } }
            public string SharedProductCategoryPrintScreen { get { return "Product Category Detail"; } }
            public string SharedProductGroupPrintScreen { get { return "Product Group Detail"; } }
            public string SharedProductPrintScreen { get { return "Product Detail"; } }
            public string SharedBrandPrintScreen { get { return "Brand Detail"; } }
            public string TLOppositionIndex { get { return "Opposition Index"; } }
            public string PatBudgetReport { get { return "Budget Report"; } }//determined in report file
            public string TmkBudgetReport { get { return "Budget Report"; } }//determined in report file
            public string GMBudgetReport { get { return "Budget Report"; } }//determined in report file
            public string PacClearancePrintScreen { get { return "Patent Clearance Detail"; } }
            public string PacClearanceWorkflowPrintScreen { get { return "Patent Clearance Workflow Detail"; } }
            public string PacClearanceStatusPrintScreen { get { return "Patent Clearance Status Detail"; } }
            public string PacClearanceList { get { return "Patent Clearance List"; } }
            public string PatScoreReport { get { return "Patent Score Report"; } }
            public string AMSDecisionManagementConflict { get { return "Decision Management Instruction Conflict"; } }

            //rms
            public string RMSClientConfirmationReport => "Confirmation Report";
            public string RMSAgentConfirmationReport => "Agent Responsibility Report";
            public string RMSActionClose => "Action Closing";
            public string RMSReminder => "Trademark Renewal Instruction List";
            public string RMSPortfolioReport => "Trademark Renewal Report";

            //foreign filing
            public string FFReminder => "Foreign Filing Instruction List";
            public string FFPortfolioReport => "Foreign Filing Renewal Report";
            public string FFClientConfirmationReport => "Confirmation Report";
            public string FFAgentConfirmationReport => "Agent Responsibility Report";
            public string FFActionClose => "Action Closing";
            public string TLUpdateBibliographic => "Trademark Office Update";
            public string TLUpdateTrademarkName => "Trademark Links Trademark Name Update";
            public string TLUpdateAction => "Trademark Links Action Update";
            public string TLActionCompareException { get { return "Action Compare Exception"; } }
            public string TmkOfficeActionPrintScreen { get { return "Trademark Office Action Detail"; } }
            public string PatIREmployeePositionPrintScreen { get { return "Employee Title Detail"; } }
            public string PatIRTurnOverPrintScreen { get { return "Turn Over Detail"; } }
            public string AMSProductIndex { get { return "Product Index Report"; } }
            public string PatRemunerationReport { get { return "Remuneration Report"; } }
            public string SharedDueDateDelegationReport { get { return "Delegation Report"; } }
            public string PatCEFeeSetupPrintScreen => "Fee Setup Detail";
            public string PatCEGeneralCostSetupPrintScreen => "General Cost Setup Detail";
            public string PatCECountryCostSetupPrintScreen => "Country Cost Setup Detail";
            public string PatCECostEstimatorPrintScreen => "Cost Estimator Detail";
            public string PatIRValorizationRulePrintScreen => "Valorization Rule Detail";
            public string PatIRRemunerationSettingsPrintScreen => "Remuneration Settings Detail";
            public string PatIRFormulaFactorPrintScreen => "Formula Factor Detail";
            public string PatIRValuationMatrixPrintScreen => "Valuation Matrix Detail";
            public string PatIRRemunerationPrintScreen => "Remuneration Detail";
            public string PatIFWActionMappingPrintScreen => "IFW Action Mapping Detail";
            public string PatUPCStatusPrintScreen => "UPC Status Detail";
            public string PatCEStagePrintScreen => "Stage Detail";
            public string PatIRStaggingPrintScreen => "Stagging Detail";
            public string PatIRExchangeRatePrintScreen => "Exchange Rate Detail";
            public string PatIRFREmployeePositionPrintScreen { get { return "Employee Title Detail"; } }
            public string PatIRFRTurnOverPrintScreen { get { return "Turn Over Detail"; } }
            public string PatIRFRValorizationRulePrintScreen => "Valorization Rule Detail";
            public string PatIRFRRemunerationSettingsPrintScreen => "Remuneration Settings Detail";
            public string PatIRFRFormulaFactorPrintScreen => "Formula Factor Detail";
            public string PatIRFRValuationMatrixPrintScreen => "Valuation Matrix Detail";
            public string PatIRFRRemunerationPrintScreen => "Remuneration Detail";
            public string PatIRFRStaggingPrintScreen => "Stagging Detail";
            public string PatIRFRExchangeRatePrintScreen => "Exchange Rate Detail";
            public string PatCostTrackingInvPrintScreen { get { return "Invention Cost Tracking Detail"; } }
            public string TmkCECostEstimatorPrintScreen => "Cost Estimator Detail";
            public string TmkCEFeeSetupPrintScreen => "Fee Setup Detail";
            public string TmkCEGeneralCostSetupPrintScreen => "General Cost Setup Detail";
            public string TmkCECountryCostSetupPrintScreen => "Country Cost Setup Detail";
            public string TmkCEStagePrintScreen => "Stage Detail";
            public string SharedDocVerificationNewDocPrintScreen => "Documents For Review";
            public string SharedDocVerificationDocPrintScreen => "Docketing Requests";
            public string SharedDocVerificationActionDocPrintScreen => "Dockets For Verification";
            public string SharedDocVerificationCommDocPrintScreen => "Dockets For Sending";
            public string LetterSubCategoryPrintScreen => "Letter Sub Category";
            public string DataQueryCategoryPrintScreen => "Custom Query Category";
            public string QECategoryPrintScreen => "Quick Email Category";
            public string SharedTradeSecretMasterList => "Trade Secret Master List";
            public string SharedTradeSecretAccessHistory => "Trade Secret Access History";
            public string SharedTradeSecretAccessLevel => "Trade Secret Access Level";
            public string SharedTradeSecretAuditLog => "Trade Secret Audit Log";
            public string SharedTradeSecretViolations => "Trade Secret Violation or Breach";
            public string SharedProductIndex { get { return "Product Index Report"; } }
        }

        private enum SystemTypeForReport
        {
            Patent,
            Trademark,
            GeneralMatter,
            AMS,
            DMS,
            Shared,
            Tmc,
            Pac
        }

        public string GetErrorMessage()
        {
            return errorMessage;
        }

        public string GetUnhandledErrorMessage()
        {
            return unhandledErrorMessage;
        }

        private string GetUserDecimalSeperator(string languageCode)
        {
            switch (languageCode)
            {
                case "en":
                    return ".";
                case "en-GB":
                    return ".";
                case "fr":
                    return ",";
                case "de":
                    return ",";
                case "es":
                    return ",";
                default:
                    return ".";
            } 
        }

        public string GetOutputFormatExtension(int reportFormat)
        {
            switch (reportFormat)
            {
                case 0:
                    return ".pdf";
                case 2:
                    return ".docx";
                case 1:
                    return ".xlsx";
                default:
                    return ".pdf";
            }
        }

        private string GetExportFileName(string exportFileName, int reportFormat)
        {
            var localDate = DateTime.Now.ToString("dd-MMM-yyyy-hh-mm-ss-fff", CultureInfo.InvariantCulture);
            return exportFileName + "_" + localDate + GetOutputFormatExtension(reportFormat);
        }

        private string ReportFolder(string strFileName = "")
        {
            string strRet = "";
            string strPathName = Path.Combine(_hostingEnvironment.ContentRootPath, "UserFiles/GeneratedReports/" + _httpContextAccessor.HttpContext.User.GetUserName());

            try
            {
                if (!Directory.Exists(strPathName))
                {
                    Directory.CreateDirectory(strPathName);
                }
                strRet = strPathName + "/" + strFileName;
            }
            catch (Exception e)
            {
                var error = e.Message;
            }
            return strRet;
        }

        private void deleteGeneratedReport()
        {
            var path = ReportFolder("");
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }

        private async Task<Dictionary<string, object>> GetReportParameterLabels()
        {
            var settings = await _defaultSettings.GetSetting();
            var labelAgent = settings.LabelAgent;
            var labelClient = settings.LabelClient;
            var labelKeyword = settings.LabelKeyword;
            var labelOwner = settings.LabelOwner;

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("AgentLabel", labelAgent);
            parameters.Add("ClientLabel", labelClient);
            parameters.Add("KeywordLabel", labelKeyword);
            parameters.Add("OwnerLabel", labelOwner);

            return parameters;
        }

        private string GetFileMimeType(string fileName)
        {
            if (fileName.EndsWith(".pdf"))
            {
                return "application/pdf";
            }
            if (fileName.EndsWith(".doc"))
            {
                return "application/msword";
            }
            if (fileName.EndsWith(".xls"))
            {
                return "application/vnd.ms-excel";
            }
            if (fileName.EndsWith(".docx"))
            {
                return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
            }
            if (fileName.EndsWith(".xlsx"))
            {
                return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            }
            return "application/`";
        }

    }
}

