using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using R10.Core.Entities.ReportScheduler;
using R10.Core.Helpers;
using R10.Core.Interfaces;
using R10.Web.Areas.Shared.ViewModels.ReportScheduler;
using R10.Web.Extensions;
using R10.Web.Extensions.ActionResults;
using R10.Web.Models;
using R10.Web.Security;

using R10.Web.Areas;

namespace R10.Web.Areas.Shared.Controllers.ReportScheduler
{
    [Area("Shared"), Authorize(Policy = SharedAuthorizationPolicy.CanAccessSystem)]
    public class RSHistoryController : BaseController
    {
        private readonly IRSHistoryService _rSHistoryService;
        private readonly IRSMainService _rSMainService;
        private readonly IMapper _mapper;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public RSHistoryController(IRSHistoryService rSHistoryService,
                                    IRSMainService rSMainService,       
                                    IMapper mapper,
                                    IStringLocalizer<SharedResource> localizer)
        {
            _rSHistoryService = rSHistoryService;
            _rSMainService = rSMainService;
            _mapper = mapper;
            _localizer = localizer;
        }

        public IActionResult GetHistorysList(
            [DataSourceRequest] DataSourceRequest request, int parentId
            )
        {
            var rSHistory = _rSHistoryService.GetRSHistorys(parentId);

            var result = rSHistory.Select(c => new
            {
                LogId = c.LogId,
                ScheduleName = c.Name,
                Frequency = c.Frequency,
                Action = c.Action,
                StartTime = c.StartDateTime,
                EndTime = c.EndDateTime,
                ElapsedTime = c.ElapsedTime,
                RunResult = c.Message,
            }).OrderByDescending(c=>c.StartTime).ToList();
            return Json(result.ToDataSourceResult(request));
        }

        public IActionResult HistoryView(int logId)
        {
            var history = _rSHistoryService.GetRSHistory(logId);
            int reportId = _rSMainService.RSMains.FirstOrDefault(c => c.TaskId == history.TaskId).ReportId;
            ViewBag.ReportId = reportId;
            return PartialView("_HistoryEntry", history);
        }

        public async Task<IActionResult> GridActionHistoryRead(
            [DataSourceRequest] DataSourceRequest request, int parentId
            )
        {
            var rSActionHistory = _rSHistoryService.GetRSActionHistorys(parentId);

            var result = await rSActionHistory.Select(c => new
            {
                LogId = c.LogId,
                ActionName = c.ActionName,
                OutputFormat = c.OutputFormat,
                SortOrder = c.SortOrder,
                Setting = GetActionHistorySetting(c),
            }).ToListAsync();

            return Json(result.ToDataSourceResult(request));
        }

        private static string GetActionHistorySetting(RSActionHistory history)
        {
            string result = "";
            //Email
            if (history.ActionName.IsCaseInsensitiveEqual("Email"))
            {
                result += history.EmailTo.IsCaseInsensitiveEqual("") ? "" : ("§To: " + history.EmailTo + Environment.NewLine);
                result += history.EmailCopyTo.IsCaseInsensitiveEqual("") ? "" : ("§CC: " + history.EmailCopyTo + Environment.NewLine);
                result += history.EmailSubject.IsCaseInsensitiveEqual("") ? "" : ("§Subject: " + history.EmailSubject + Environment.NewLine);
                result += history.EmailBody.IsCaseInsensitiveEqual("") ? "" : ("§Body: " + history.EmailBody + Environment.NewLine);
            }
            //FTP Upload
            else
            {
                result += history.FTPAddress.IsCaseInsensitiveEqual("") ? "" : ("§Host: " + history.FTPAddress + Environment.NewLine);
                result += history.FTPUserID.IsCaseInsensitiveEqual("") ? "" : ("§User: " + history.FTPUserID + Environment.NewLine);
                result += history.FTPPassword.IsCaseInsensitiveEqual("") ? "" : ("§Password: " + history.FTPPassword + Environment.NewLine);
            }
            if (!result.Equals(""))
                result = result.Remove(result.Length - 1);

            return result;
        }

        public IActionResult GridPrintOptionHistoryRead(
            [DataSourceRequest] DataSourceRequest request, int parentId
            )
        {
            var rSPrintOptionHistory = _rSHistoryService.GetRSPrintOptionHistorys(parentId);
            if (rSPrintOptionHistory == null)
            {
                rSPrintOptionHistory = new List<RSPrintOptionHistory>().AsQueryable();
            }
            return Json(rSPrintOptionHistory.ToDataSourceResult(request));
        }

        public IActionResult GridCriteriaHistoryRead(
            [DataSourceRequest] DataSourceRequest request, int parentId
            )
        {
            var rSCriteriaHistory = _rSHistoryService.GetRSCriteriaHistorys(parentId);
            if (rSCriteriaHistory == null)
            {
                rSCriteriaHistory = new List<RSCriteriaHistory>().AsQueryable();
            }
            return Json(rSCriteriaHistory.ToDataSourceResult(request));
        }
    }
}