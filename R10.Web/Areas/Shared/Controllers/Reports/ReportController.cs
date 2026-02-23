using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
//using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using R10.Core;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using R10.Core.Entities.Shared;
using R10.Core.Entities.Trademark;
using R10.Core.Exceptions;
using R10.Core.Helpers;
using R10.Core.Interfaces;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Areas.Shared.ViewModels.Report;
using R10.Web.Helpers;
using R10.Web.Interfaces;
using R10.Web.Interfaces.Shared;
using R10.Web.Models;
using R10.Web.Security;

namespace R10.Web.Areas
{
    [Area("Shared"), Authorize]
    public class ReportController : Microsoft.AspNetCore.Mvc.Controller
    {
        private readonly IReportService reportService;
        private readonly IReportDeployService reportDeployService;
        private readonly ICustomReportService _customReportService;
        private readonly IDataQueryService _dataQueryService;
        private readonly ExportHelper _exportHelper;
        private readonly ISharedReportViewModelService _dueDateListViewModelService;
        private readonly IConfiguration _configuration;
        private readonly IAttorneyService _attorneyService;
        private readonly ISystemSettings<PatSetting> _patSettings;
        private readonly ISystemSettings<TmkSetting> _tmkSettings;
        private readonly ISystemSettings<DefaultSetting> _sharedSettings;
        private readonly IEmailSender _emailSender;
        private readonly IAuthorizationService _authService;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public ReportController(IReportService reportService
            , IReportDeployService reportDeployService
            , ICustomReportService customReportService
            , IDataQueryService dataQueryService
            , ExportHelper exportHelper
            , ISharedReportViewModelService dueDateListViewModelService
            , IAttorneyService attorneyRepository
            , IConfiguration configuration
            , ISystemSettings<PatSetting> patSettings
            , ISystemSettings<TmkSetting> tmkSettings
            , ISystemSettings<DefaultSetting> sharedSettings
            , IEmailSender emailSender
            , IAuthorizationService authService
            , IStringLocalizer<SharedResource> localizer
            )
        {
            this.reportService = reportService;
            this.reportDeployService = reportDeployService;
            _customReportService = customReportService;
            _dataQueryService = dataQueryService;
            _exportHelper = exportHelper;
            _dueDateListViewModelService = dueDateListViewModelService;
            _configuration = configuration;
            _attorneyService = attorneyRepository;
            _patSettings = patSettings;
            _tmkSettings = tmkSettings;
            _sharedSettings = sharedSettings;
            _emailSender = emailSender;
            _authService = authService;
            _localizer = localizer;
        }

        [Authorize(Policy = SharedAuthorizationPolicy.CanAccessDueDateList)]
        [HttpGet]
        public async Task<IActionResult> DueDate(string sys, bool d = true)
        {
            DueDateListReportViewModel duedate = new DueDateListReportViewModel();
            duedate.PrintSystems = sys != null ? sys : "P, T, G";
            ViewBag.Url = Url.Action("DueDate", "Report", new { sys });
            ViewBag.IsRTSOn = (await _patSettings.GetSetting()).IsRTSOn;
            ViewBag.IsTLOn = (await _tmkSettings.GetSetting()).IsTLOn;
            ViewBag.CanAddSchedule = (await _authService.AuthorizeAsync(User, SharedAuthorizationPolicy.FullModify)).Succeeded;
            ViewBag.LoadDefault = d;

            return View(duedate);
        }

        [Authorize(Policy = SharedAuthorizationPolicy.CanAccessDueDateList)]
        [HttpPost]
        public async Task<IActionResult> DueDate([FromBody] DueDateListReportViewModel duedate)
        {
            if (duedate.PrintGoods == null)
                duedate.PrintGoods = "0";
            if (ModelState.IsValid)
            {
                try
                {
                    if (duedate.SortOrder == 4)
                        return reportService.GetReport(duedate, ReportType.SharedDueDateListCalendar).Result;

                    if (duedate.SortOrder != 5)
                    {
                        return reportService.GetReport(duedate, duedate.LayoutFormat == 1 ? ReportType.SharedDueDateListConcise : ReportType.SharedDueDateList).Result;
                    }
                    else
                    {
                        duedate.SortOrder = 2;
                        duedate.ReportFormat = 0;

                        List<string> attorneysList = new List<string>();

                        using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                        {
                            using (SqlCommand cmd = new SqlCommand())
                            {
                                cmd.CommandText = "procWebSysDueDateList";
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Connection = sqlConnection;

                                foreach (PropertyInfo propertyInfo in duedate.GetType().GetProperties())
                                {
                                    if (!(propertyInfo.Name.Contains("Criteria") && propertyInfo.PropertyType.Name.Equals("String")) && propertyInfo.Name != "LanguageCode")
                                    {
                                        SqlParameter param = new SqlParameter();
                                        param.ParameterName = "@" + propertyInfo.Name;
                                        param.Value = propertyInfo.GetValue(duedate, null);
                                        cmd.Parameters.Add(param);
                                    }
                                }
                                SqlParameter param2 = new SqlParameter();
                                param2.ParameterName = "@UserID";
                                param2.Value = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").Value;
                                cmd.Parameters.Add(param2);

                                sqlConnection.Open();

                                SqlDataReader reader = cmd.ExecuteReader();
                                while (reader.Read())
                                {
                                    attorneysList.Add((string)reader["Attorney"]);
                                }
                            }
                        }

                        string[] attorneys = attorneysList.Distinct().Where(c => c != "").ToArray();
                        string errorMessage = "";
                        foreach (string attorney in attorneys)
                        {
                            Attorney a = _attorneyService.QueryableList.FirstOrDefault(c => c.AttorneyCode == attorney);

                            if (a.EMail != null && a.EMail != "")
                            {
                                duedate.Attorneys = "|" + a.AttorneyCode.ToString() + "|";
                                duedate.AttorneyNames = "|" + a.AttorneyName.ToString() + "|";

                                FileStreamResult fsr = (FileStreamResult)reportService.GetReport(duedate, duedate.LayoutFormat == 1 ? ReportType.SharedDueDateListConcise : ReportType.SharedDueDateList).Result;

                                Attachment attachment = new Attachment(fsr.FileStream, "Due Date Report for " + a.AttorneyName + ".pdf");
                                // send report to attorney here.
                                _emailSender.From = new MailAddress(User.Identity.Name);
                                _emailSender.ReplyTo = new List<MailAddress> { new MailAddress(User.Identity.Name) };

                                var emaillSubject = "Due Date List (by Attorney) - " + a.AttorneyName;
                                var emailBody = "Please see the attached report.";
                                var result = await _emailSender.SendEmailAsync(GetAddresses(a.EMail), emaillSubject, emailBody, attachment);

                                if (!result.Success)
                                {
                                    errorMessage += a.AttorneyCode.ToString() + "-" + a.EMail.ToString() + ", ";
                                    //log
                                    var errMsgSend = "Unable to send email to {0} <{1}>.\n";
                                    var err = String.Format(errMsgSend, a.AttorneyCode.ToString(), a.EMail.ToString());

                                }
                            }
                        }
                        if(errorMessage.Equals(""))
                            return BadRequest(_localizer["Email was successfully sent."].ToString());
                        return BadRequest(_localizer["Failed to send email to " + errorMessage.Substring(0, errorMessage.Length-2)].ToString());
                    }
                }
                catch (Exception ex)
                {
                    return BadRequest(_localizer[ex.Message].ToString());
                }
            }
            if (duedate.FromDate == null)
                return BadRequest(_localizer["Please Enter From Date"].ToString());
            if (duedate.ToDate == null)
                return BadRequest(_localizer["Please Enter To Date"].ToString());
            return BadRequest(_localizer[ModelState.Root.Errors[0].ErrorMessage].ToString());
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

        [HttpGet]
        public async Task<IActionResult> CostTracking(string sys, bool d = true)
        {
            var policy = "";
            switch (sys)
            {
                case SystemTypeCode.Trademark:
                    policy = TrademarkAuthorizationPolicy.CanAccessCostTracking;
                    break;
                case SystemTypeCode.GeneralMatter:
                    policy = GeneralMatterAuthorizationPolicy.CanAccessCostTracking;
                    break;
                default:
                    policy = PatentAuthorizationPolicy.CanAccessCostTracking;
                    break;

            }
            if (!(await _authService.AuthorizeAsync(User, policy)).Succeeded)
                return Forbid();

            CostTrackingReportViewModel costTracking = new CostTrackingReportViewModel();
            costTracking.PrintSystems = sys;
            ViewBag.Url = Url.Action("CostTracking", "Report", new { sys });
            ViewBag.LoadDefault = d;

            return View(costTracking);
        }
        [HttpPost]
        public IActionResult CostTracking([FromBody] CostTrackingReportViewModel costTracking)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    return reportService.GetReport(costTracking, ReportType.SharedCostTracking).Result;
                }
                catch
                {
                    return BadRequest(_localizer[reportService.GetErrorMessage()].ToString());
                }
            }

            return BadRequest(_localizer[ModelState.Root.Errors[0].ErrorMessage].ToString());
        }

        [HttpPost]
        public async Task<IActionResult> EmailCostTracking(CostTrackingReportViewModel criteria)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var emailReport = await reportService.SaveEmailReport(criteria, ReportType.SharedCostTracking);
                    return PartialView("_EmailReport", emailReport);
                }
                catch
                {
                    return BadRequest(_localizer[reportService.GetErrorMessage()].ToString());
                }
            }
            try
            {
                return BadRequest(_localizer[ModelState.Root.Errors[0].ErrorMessage].ToString());
            }
            catch
            {
                return BadRequest(_localizer[reportService.GetUnhandledErrorMessage()].ToString());
            }

        }

        [Authorize(Policy = SharedAuthorizationPolicy.CanAccessDueDateList)]
        [HttpPost]
        public async Task<IActionResult> EmailDueDate(DueDateListReportViewModel criteria)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var emailReport = await reportService.SaveEmailReport(criteria, criteria.SortOrder == 4 ? ReportType.SharedDueDateListCalendar : criteria.LayoutFormat == 0 ? ReportType.SharedDueDateList : ReportType.SharedDueDateListConcise);
                    return PartialView("_EmailReport", emailReport);
                }
                catch
                {
                    return BadRequest(_localizer[reportService.GetErrorMessage()].ToString());
                }
            }
            try
            {
                return BadRequest(_localizer[ModelState.Root.Errors[0].ErrorMessage].ToString());
            }
            catch
            {
                return BadRequest(_localizer[reportService.GetUnhandledErrorMessage()].ToString());
            }

        }

        [HttpPost]
        public async Task<IActionResult> CustomReport([FromBody] CustomReportDetailViewModel customReport)
        {
            if (!(await _sharedSettings.GetSetting()).IsCustomReportON)
                return NotFound();
            try
            {
                var t = HttpContext.Request;
                string permissionText = await _customReportService.GetDataPermissionTextByReportId(customReport.ReportId);
                if (permissionText != "")
                {
                    return BadRequest(_localizer[permissionText].ToString());
                }
                customReport.ReportFormat = customReport.ReportFormatForCustomReport;
                if (customReport.ReportFormat == 0 || customReport.ReportFormat == 1 || customReport.ReportFormat == 2 || customReport.ReportFormat == 4)
                    return reportService.GetReport(customReport, ReportType.CustomReport).Result;
                else if (customReport.ReportFormat == 3)
                {
                    int queryId = await reportDeployService.GetCustomQueryId(customReport.ReportName);
                    var dt = await _customReportService.RunQuery(User.GetUserIdentifier(), queryId);
                    var fileStream = _exportHelper.DataTableToXMLMemoryStream(dt, "CPiReport");
                    return File(fileStream.ToArray(), ImageHelper.GetContentType(".xml"), customReport.ReportName + ".xml");
                }
                else
                    return BadRequest(_localizer["Unsupported Report Format."].ToString());
            }
            catch
            {
                return BadRequest(_localizer[reportService.GetErrorMessage()].ToString());
            }
        }

        [HttpPost]
        public async Task<IActionResult> EmailCustomReport(CustomReportDetailViewModel criteria)
        {
            if (!(await _sharedSettings.GetSetting()).IsCustomReportON)
                return NotFound();
            try
            {
                string permissionText = await _customReportService.GetDataPermissionTextByReportId(criteria.ReportId);
                if (permissionText != "")
                {
                    return BadRequest(_localizer[permissionText].ToString());
                }
                criteria.ReportFormat = criteria.ReportFormatForCustomReport;
                if (criteria.ReportFormat == 0 || criteria.ReportFormat == 1 || criteria.ReportFormat == 2 || criteria.ReportFormat == 4)
                {
                    var emailReport = await reportService.SaveEmailReport(criteria, ReportType.CustomReport);
                    return PartialView("_EmailReport", emailReport);
                }
                else
                    return BadRequest(_localizer["Unsupported Report Format."].ToString());

            }
            catch
            {
                return BadRequest(_localizer[reportService.GetErrorMessage()].ToString());
            }
        }

        [HttpPost]
        public async Task<IActionResult> SendEmail(EmailReportViewModel emailData)
        {
            return Json(await reportService.EmailReport(emailData));
        }

        [HttpGet]
        public IActionResult DownloadReport(string generatedReportName)
        {
            return reportService.GetGeneratedReport(generatedReportName);
        }

        [HttpGet]
        public async Task<IActionResult> TradeSecretReport(string sys, bool d = true)
        {
            var policy = "";
            switch (sys)
            {
                case SystemTypeCode.DMS:
                    policy = DMSAuthorizationPolicy.CanAccessTradeSecretReports;
                    break;
                default:
                    policy = PatentAuthorizationPolicy.CanAccessTradeSecretReports;
                    break;

            }
            if (!(await _authService.AuthorizeAsync(User, policy)).Succeeded)
                return Forbid();

            TradeSecretReportCriteriaViewModel tradeSecret = new TradeSecretReportCriteriaViewModel();
            tradeSecret.PrintPatent = sys == "P";
            tradeSecret.PrintDMS = sys == "D";
            ViewBag.Url = Url.Action("TradeSecretReport", "Report");
            ViewBag.LoadDefault = d;
            return View(tradeSecret);
        }

        [HttpPost]
        public IActionResult TradeSecretReport([FromBody] TradeSecretReportCriteriaViewModel tradeSecretReport)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    ReportType reportType;

                    switch (tradeSecretReport.ReportOption)
                    {
                        case 1:
                            reportType = ReportType.SharedTradeSecretMasterList;
                            break;
                        case 2:
                            reportType = ReportType.SharedTradeSecretAccessHistory;
                            break;
                        case 3:
                            reportType = ReportType.SharedTradeSecretAccessLevel;
                            break;
                        case 4:
                            reportType = ReportType.SharedTradeSecretAuditLog;
                            break;
                        case 5:
                            reportType = ReportType.SharedTradeSecretViolations;
                            break;
                        default:
                            reportType = ReportType.SharedTradeSecretMasterList;
                            break;
                    }
                    return reportService.GetReport(tradeSecretReport, reportType).Result;
                }
                catch
                {
                    return BadRequest(_localizer[reportService.GetErrorMessage()].ToString());
                }
            }

            return BadRequest(_localizer[ModelState.Root.Errors[0].ErrorMessage].ToString());
        }

        [HttpPost]
        public async Task<IActionResult> EmailTradeSecretReport(TradeSecretReportCriteriaViewModel tradeSecretReport)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    ReportType reportType;

                    switch (tradeSecretReport.ReportOption)
                    {
                        case 1:
                            reportType = ReportType.SharedTradeSecretMasterList;
                            break;
                        case 2:
                            reportType = ReportType.SharedTradeSecretAccessHistory;
                            break;
                        case 3:
                            reportType = ReportType.SharedTradeSecretAccessLevel;
                            break;
                        default:
                            reportType = ReportType.SharedTradeSecretMasterList;
                            break;
                    }

                    var emailReport = await reportService.SaveEmailReport(tradeSecretReport, reportType);
                    return PartialView("_EmailReport", emailReport);
                }
                catch
                {
                    return BadRequest(_localizer[reportService.GetErrorMessage()].ToString());
                }
            }
            try
            {
                return BadRequest(_localizer[ModelState.Root.Errors[0].ErrorMessage].ToString());
            }
            catch
            {
                return BadRequest(_localizer[reportService.GetUnhandledErrorMessage()].ToString());
            }

        }


        [HttpGet]
        public async Task<IActionResult> ProductIndex(string sys, bool d = true)
        {
            var policy = "";
            switch (sys)
            {
                case SystemTypeCode.Trademark:
                    policy = TrademarkAuthorizationPolicy.CanAccessSystem;
                    break;
                case SystemTypeCode.GeneralMatter:
                    policy = GeneralMatterAuthorizationPolicy.CanAccessSystem;
                    break;
                default:
                    policy = PatentAuthorizationPolicy.CanAccessSystem;
                    break;

            }
            if (!(await _authService.AuthorizeAsync(User, policy)).Succeeded)
                return Forbid();

            ProductIndexReportViewModel vm = new ProductIndexReportViewModel();
            vm.PrintPatent = sys == "P";
            vm.PrintTrademark = sys == "T";
            vm.PrintGenMatter = sys == "G";
            ViewBag.Url = Url.Action("ProductIndex", "Report");
            ViewBag.LoadDefault = d;
            return View(vm);
        }

        [HttpPost]
        public IActionResult ProductIndex([FromBody] ProductIndexReportViewModel productIndex)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    return reportService.GetReport(productIndex, ReportType.SharedProductIndex).Result;
                }
                catch
                {
                    return BadRequest(_localizer[reportService.GetErrorMessage()].ToString());
                }
            }

            return BadRequest(_localizer[ModelState.Root.Errors[0].ErrorMessage].ToString());
        }

        [HttpPost]
        public async Task<IActionResult> EmailProductIndex(ProductIndexReportViewModel criteria)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var emailReport = await reportService.SaveEmailReport(criteria, ReportType.SharedProductIndex);
                    return PartialView("_EmailReport", emailReport);
                }
                catch
                {
                    return BadRequest(_localizer[reportService.GetErrorMessage()].ToString());
                }
            }
            try
            {
                return BadRequest(_localizer[ModelState.Root.Errors[0].ErrorMessage].ToString());
            }
            catch
            {
                return BadRequest(_localizer[reportService.GetUnhandledErrorMessage()].ToString());
            }
        }
    }
}