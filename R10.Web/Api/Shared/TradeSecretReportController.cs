using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using ActiveQueryBuilder.View.DatabaseSchemaView;
using AutoMapper;
using AutoMapper.QueryableExtensions;
//using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Validation.AspNetCore;
using R10.Core.DTOs;
using R10.Core.Entities.Patent;
using R10.Core.Entities.Shared;
using R10.Core.Helpers;
using R10.Core.Interfaces;
using R10.Core.Interfaces.DMS;
using R10.Core.Interfaces.Patent;
using R10.Core.Interfaces.Shared;
using R10.Core.Services;
//using R10.Web.Api.Models;
using R10.Web.Areas;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Areas.Shared.ViewModels.Report;
using R10.Web.Extensions;
using R10.Web.Filters;
using R10.Web.Helpers;
using R10.Web.Security;

namespace R10.Web.Api.Shared
{
    [Route("api/shared/[controller]")]
    [ServiceFilter(typeof(RequestHeaderFilter))]
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme, Policy = PatentAuthorizationPolicy.CanAccessTradeSecretReports)]

    public class TradeSecretReportController : Microsoft.AspNetCore.Mvc.Controller
    {
        private readonly IInventionService _inventionService;
        private readonly ICountryApplicationService _applicationService;
        private readonly ISystemSettings<PatSetting> _patSettings;
        private readonly IDisclosureService _disclosureService;
        private readonly ITradeSecretService _tradeSecretService;
        private readonly ClaimsPrincipal _user;

        public TradeSecretReportController(
            IInventionService inventionService,
            ICountryApplicationService applicationService,
            ISystemSettings<PatSetting> patSettings, 
            IDisclosureService disclosureService, 
            ITradeSecretService tradeSecretService, 
            ClaimsPrincipal user)
        {
            _inventionService = inventionService;
            _applicationService = applicationService;
            _patSettings = patSettings;
            _user = user;
            _tradeSecretService = tradeSecretService;
            _disclosureService = disclosureService;
        }

        [HttpGet("MasterList")]
        public async Task<IActionResult> MasterList(TradeSecretReportCriteriaViewModel criteria)
        {
            var patSettings = await _patSettings.GetSetting();

            var data = new List<TradeSecretMasterListReportViewModel>();

            if (criteria.PrintPatent)
            {
                var inventions = _inventionService.QueryableList.Where(i => i.IsTradeSecret == true);
                var inventionData = await inventions.ProjectTo<TradeSecretMasterListReportViewModel>().ToListAsync();

                foreach (var invention in inventionData)
                {
                    invention.LastViewDate = await _tradeSecretService.ActivityQueryableList.
                                                Where(a => a.RecId == invention.Id && a.ScreenId == "Invention")
                                                .MaxAsync(a => (DateTime?)a.ActivityDate);
                    invention.LastViewDate_Fmt = invention.LastViewDate?.ToString("MM/dd/yyyy HH:mm:ss");

                    await _tradeSecretService.LogActivity(TradeSecretScreen.Report, TradeSecretScreen.Invention, invention.Id, TradeSecretActivityCode.Report, 0);
                }

                data.AddRange(inventionData);

                for (var i = 0; i <= data.Count - 1; i++)
                {
                    //if (data[i].Abstracts.Count == 0)
                    //    data[i].Abstracts.Add(new AbstractExport()); //DUMMY ROW FOR SSRS

                    var row = 0;
                    for (var ii = 0; ii <= data[i].Abstracts.Count - 1; ii++)
                    {
                        data[i].Abstracts[ii].OrderOfEntry = ++row;
                    }
                }
            }

            if (criteria.PrintDMS)
            {
                var disclosures = _disclosureService.QueryableList.Where(d => d.IsTradeSecret == true);
                var disclosureData = await disclosures.ProjectTo<TradeSecretMasterListReportViewModel>().ToListAsync();
                
                foreach (var disclosure in disclosureData)
                {
                    disclosure.LastViewDate = await _tradeSecretService.ActivityQueryableList.
                                                Where(a => a.RecId == disclosure.Id && a.ScreenId == "DMSDisclosure")
                                                .MaxAsync(a => (DateTime?)a.ActivityDate);
                    disclosure.LastViewDate_Fmt = disclosure.LastViewDate?.ToString("MM/dd/yyyy HH:mm:ss");

                    await _tradeSecretService.LogActivity(TradeSecretScreen.Report, TradeSecretScreen.DMSDisclosure, disclosure.Id, TradeSecretActivityCode.Report, 0);
                }

                data.AddRange(disclosureData);
            }           

            return Ok(data);
        }

        [HttpGet("AuditLog")]
        public async Task<IActionResult> AuditLog(TradeSecretReportCriteriaViewModel criteria)
        {
            var data = new List<TradeSecretAuditLogReportViewModel>();

            var auditLogs = _tradeSecretService.AuditLogQueryableList;

            if (criteria.PrintPatent)
            {
                var userIds = await auditLogs.Where(a => a.TradeSecretActivity.ScreenId == "Invention" || a.TradeSecretActivity.ScreenId == "Abstract" || a.TradeSecretActivity.ScreenId == "CountryApplication")
                                             .Select(a => a.TradeSecretActivity.UserId).Distinct().ToListAsync();
                var emails = await _tradeSecretService.GetUserEmails(userIds);

                var inventionIDs = auditLogs.Where(log => log.TradeSecretActivity.ScreenId == "Invention").Select(log => log.TradeSecretActivity.RecId).Distinct();
                var abstractIDs = auditLogs.Where(log => log.TradeSecretActivity.ScreenId == "Abstract").Select(log => log.TradeSecretActivity.RecId).Distinct();
                var applicationIDs = auditLogs.Where(log => log.TradeSecretActivity.ScreenId == "CountryApplication").Select(log => log.TradeSecretActivity.RecId).Distinct();

                var inventionList = _inventionService.QueryableList.Where(inv => inventionIDs.Contains(inv.InvId));
                var applicationList = _applicationService.CountryApplications.Where(ca => applicationIDs.Contains(ca.AppId));
                var inventionAbstractList = _inventionService.QueryableList.Where(inv => inv.Abstracts.Any(a => abstractIDs.Contains(a.AbstractId)));

                var invLogs = from log in auditLogs
                                 where log.TradeSecretActivity.ScreenId == "Invention" 
                                 join inv in inventionList
                                    on log.TradeSecretActivity.RecId equals inv.InvId
                                 select new TradeSecretAuditLogReportViewModel
                                 {
                                        ParentId = inv.InvId,
                                        AuditLogId = log.AuditLogId,
                                        ActivityId = log.ActivityId,
                                        OldValues = log.OldValues,
                                        NewValues = log.NewValues,
                                        UpdatedFields = log.UpdatedFields,
                                        UserId = log.TradeSecretActivity.UserId,
                                        RecId = log.TradeSecretActivity.RecId,
                                        ScreenId = log.TradeSecretActivity.ScreenId,
                                        Sys = "Invention",
                                        CaseNumber = inv.CaseNumber,
                                        Country = "",
                                        SubCase = "",
                                     Title = inv.TradeSecret.InvTitle,
                                        Inventors = inv.Inventors != null ? string.Join(", ", inv.Inventors.Select(i => i.InventorInvInventor.Inventor)) : null,
                                        AbstractConcat = inv.Abstracts != null ? string.Join(", ", inv.Abstracts.Select(i => i.TradeSecret.Abstract)) : null,

                                        Abstracts = inv.Abstracts.Select(a => new AbstractExport()
                                        {
                                            Abstract = a.TradeSecret.Abstract,
                                            Language = a.LanguageName,
                                            OrderOfEntry = a.OrderOfEntry
                                        }).ToList(),
                                        UpdatedDate = log.TradeSecretActivity.ActivityDate.HasValue ? log.TradeSecretActivity.ActivityDate.Value.ToString("MM/dd/yyyy HH:mm:ss") : null,
                                        UpdatedBy = emails.ContainsKey(log.TradeSecretActivity.UserId) ? emails[log.TradeSecretActivity.UserId] : null
                                 };
                var appLogs = from log in auditLogs
                                 where log.TradeSecretActivity.ScreenId == "CountryApplication" 
                                 join ca in applicationList
                                    on log.TradeSecretActivity.RecId equals ca.AppId
                                 select new TradeSecretAuditLogReportViewModel
                                 {
                                        ParentId = ca.AppId,
                                        AuditLogId = log.AuditLogId,
                                        ActivityId = log.ActivityId,
                                        OldValues = log.OldValues,
                                        NewValues = log.NewValues,
                                        UpdatedFields = log.UpdatedFields,
                                        UserId = log.TradeSecretActivity.UserId,
                                        RecId = log.TradeSecretActivity.RecId,
                                        ScreenId = log.TradeSecretActivity.ScreenId,
                                        Sys = "Country Application",
                                        CaseNumber = ca.CaseNumber,
                                        Country = ca.Country,
                                        SubCase = ca.SubCase,
                                     Title = ca.TradeSecret.AppTitle,
                                        Inventors = ca.Inventors != null ? string.Join(", ", ca.Inventors.Select(i => i.InventorAppInventor.Inventor)) : null,
                                        AbstractConcat = ca.Invention.Abstracts != null ? string.Join(", ", ca.Invention.Abstracts.Select(i => i.TradeSecret.Abstract)) : null,

                                        Abstracts = ca.Invention.Abstracts.Select(a => new AbstractExport()
                                        {
                                            Abstract = a.TradeSecret.Abstract,
                                            Language = a.LanguageName,
                                            OrderOfEntry = a.OrderOfEntry
                                        }).ToList(),
                                        UpdatedDate = log.TradeSecretActivity.ActivityDate.HasValue ? log.TradeSecretActivity.ActivityDate.Value.ToString("MM/dd/yyyy HH:mm:ss") : null,
                                        UpdatedBy = emails.ContainsKey(log.TradeSecretActivity.UserId) ? emails[log.TradeSecretActivity.UserId] : null
                                 };
                var abstractLogs = from log in auditLogs
                                   where log.TradeSecretActivity.ScreenId == "Abstract"
                                   from inv in inventionAbstractList
                                   where inv.Abstracts.Any(a => a.AbstractId == log.TradeSecretActivity.RecId)
                                   select new TradeSecretAuditLogReportViewModel
                                   {
                                        ParentId = inv.InvId,
                                        AuditLogId = log.AuditLogId,
                                        ActivityId = log.ActivityId,
                                        OldValues = log.OldValues,
                                        NewValues = log.NewValues,
                                        UpdatedFields = log.UpdatedFields,
                                        UserId = log.TradeSecretActivity.UserId,
                                        RecId = log.TradeSecretActivity.RecId,
                                        ScreenId = log.TradeSecretActivity.ScreenId,
                                        Sys = "Invention",
                                        CaseNumber = inv.CaseNumber,
                                        Country = "",
                                        SubCase = "",
                                       Title = inv.TradeSecret.InvTitle,
                                        Inventors = inv.Inventors != null ? string.Join(", ", inv.Inventors.Select(i => i.InventorInvInventor.Inventor)) : null,
                                        AbstractConcat = inv.Abstracts != null ? string.Join(", ", inv.Abstracts.Select(i => i.TradeSecret.Abstract)) : null,
                                        Abstracts = inv.Abstracts.Select(a => new AbstractExport()
                                        {
                                            Abstract = a.TradeSecret.Abstract,
                                            Language = a.LanguageName,
                                            OrderOfEntry = a.OrderOfEntry
                                        }).ToList(),
                                       UpdatedDate = log.TradeSecretActivity.ActivityDate.HasValue ? log.TradeSecretActivity.ActivityDate.Value.ToString("MM/dd/yyyy HH:mm:ss") : null,
                                        UpdatedBy = emails.ContainsKey(log.TradeSecretActivity.UserId) ? emails[log.TradeSecretActivity.UserId] : null
                                    };

                data.AddRange(invLogs);
                data.AddRange(appLogs);
                data.AddRange(abstractLogs);

                data.AsParallel().ForAll(item => {
                    int row = 0;
                    item.Abstracts.ForEach(abstractItem => abstractItem.OrderOfEntry = ++row);
                });
            }

            if (criteria.PrintDMS)
            {
                var userIds = await auditLogs.Where(a => a.TradeSecretActivity.ScreenId == "DMSDisclosure")
                                        .Select(a => a.TradeSecretActivity.UserId)
                                        .Distinct().ToListAsync();
                var emails = await _tradeSecretService.GetUserEmails(userIds);


                var dmsLogData = from log in auditLogs
                                 where log.TradeSecretActivity.ScreenId == "DMSDisclosure"
                                 join dms in _disclosureService.QueryableList
                                    on log.TradeSecretActivity.RecId equals dms.DMSId
                                 select new TradeSecretAuditLogReportViewModel
                                 {
                                     ParentId = dms.DMSId,
                                     AuditLogId = log.AuditLogId,
                                     ActivityId = log.ActivityId,
                                     OldValues = log.OldValues,
                                     NewValues = log.NewValues,
                                     UpdatedFields = log.UpdatedFields,
                                     UserId = log.TradeSecretActivity.UserId,
                                     RecId = log.TradeSecretActivity.RecId,
                                     ScreenId = log.TradeSecretActivity.ScreenId,
                                     Sys = "Invention Disclosure",
                                     CaseNumber = dms.DisclosureNumber,
                                     Country = "",
                                     SubCase = "",
                                     Title = dms.TradeSecret.DisclosureTitle,
                                     Inventors = dms.Inventors != null ? string.Join(", ", dms.Inventors.Select(i => i.PatInventor.Inventor)) : null,
                                     UpdatedDate = log.TradeSecretActivity.ActivityDate.HasValue ? log.TradeSecretActivity.ActivityDate.Value.ToString("MM/dd/yyyy HH:mm:ss") : null,
                                     UpdatedBy = emails.ContainsKey(log.TradeSecretActivity.UserId) ? emails[log.TradeSecretActivity.UserId] : null
                                 };

                data.AddRange(dmsLogData);
            }

            return Ok(data);
        }

        [HttpGet("Violations")]
        public async Task<IActionResult> Violations(TradeSecretReportCriteriaViewModel criteria)
        {
            List<string> settingTypes = new List<string>() { "RestrictPatTradeSecret", "RestrictDMSTradeSecret" };

            var activities = _tradeSecretService.ActivityQueryableList.Where(a => a.Activity != "TimeOut");
            var activityUsers = await activities.Select(a => a.UserId).Distinct().ToListAsync();
            var emails = await _tradeSecretService.GetUserEmails(activityUsers);
            var logs = await _tradeSecretService.GetUserSettingLogs(activityUsers);

            var settingLogsbyUser = logs.Where(l => settingTypes.Any(s => s == l.SettingName)).OrderBy(l => l.ChangeDate).GroupBy(l => l.UserId).ToList();

            var activitiesByUser = activities.GroupBy(a => a.UserId).ToDictionary(g => g.Key, g => g.OrderBy(a => a.ActivityDate).ToList());

            var violations = new List<TradeSecretActivity>();

            foreach (var userLogGroup in settingLogsbyUser)
            {
                var userId = userLogGroup.Key;
                var userLogs = userLogGroup.ToList();
                var restrictionPeriods = new List<(DateTime Start, DateTime? End)>();

                for (int i = 0; i < userLogs.Count; i++)
                {
                    var log = userLogs[i];

                    if (string.Equals(log.NewValue, "true", StringComparison.OrdinalIgnoreCase))
                    {
                        var start = log.ChangeDate;
                        var end = userLogs
                            .Skip(i + 1)
                            .FirstOrDefault(l => string.Equals(l.NewValue, "false", StringComparison.OrdinalIgnoreCase))
                            ?.ChangeDate;

                        restrictionPeriods.Add((start, end));
                    }
                }

                if (activitiesByUser.TryGetValue(userId, out var userActivities))
                {
                    foreach (var period in restrictionPeriods)
                    {
                        var periodViolations = userActivities
                            .Where(a => a.ActivityDate > period.Start &&
                                        (period.End == null || a.ActivityDate < period.End))
                            .ToList();

                        violations.AddRange(periodViolations);
                    }
                }
            }


            var data = new List<TradeSecretViolationsReportViewModel>();

            if (criteria.PrintPatent)
            {
                var inventions = from v in violations
                                      where v.ScreenId == "Invention"
                                      join inv in _inventionService.QueryableList
                                          on v.RecId equals inv.InvId
                                      select new TradeSecretViolationsReportViewModel
                                      {
                                          ParentId = inv.InvId,
                                          UserEmail = emails.ContainsKey(v.UserId) ? emails[v.UserId] : null,
                                          ActivityId = v.ActivityId,
                                          Activity = v.Activity,
                                          ScreenId = v.ScreenId,
                                          ActivityDate = v.ActivityDate.HasValue ? v.ActivityDate.Value.ToString("MM/dd/yyyy HH:mm:ss") : null,
                                          RecId = v.RecId,
                                          Source = v.Source,
                                          CaseNumber = inv.CaseNumber,
                                          Title = inv.TradeSecret.InvTitle

                                      };
                data.AddRange(inventions);

                var applications = from v in violations
                                       where v.ScreenId == "CountryApplication"
                                       join ca in _applicationService.CountryApplications
                                           on v.RecId equals ca.AppId
                                       select new TradeSecretViolationsReportViewModel
                                       {
                                           ParentId = ca.AppId,
                                           UserEmail = emails.ContainsKey(v.UserId) ? emails[v.UserId] : null,
                                           ActivityId = v.ActivityId,
                                           Activity = v.Activity,
                                           ScreenId = v.ScreenId,
                                           ActivityDate = v.ActivityDate.HasValue ? v.ActivityDate.Value.ToString("MM/dd/yyyy HH:mm:ss") : null,
                                           RecId = v.RecId,
                                           Source = v.Source,
                                           CaseNumber = ca.CaseNumber,
                                           Country = ca.Country,
                                           SubCase = ca.SubCase,
                                           Title = ca.TradeSecret.AppTitle
                                       };
                data.AddRange(applications);
            }


            if (criteria.PrintDMS)
            {
                var disclosures = from v in violations
                                       where v.ScreenId == "Disclosure"
                                       join d in _disclosureService.QueryableList
                                           on v.RecId equals d.DMSId
                                       select new TradeSecretViolationsReportViewModel
                                       {
                                           ParentId = d.DMSId,
                                           UserEmail = emails.ContainsKey(v.UserId) ? emails[v.UserId] : null,
                                           ActivityId = v.ActivityId,
                                           Activity = v.Activity,
                                           ScreenId = v.ScreenId,
                                           ActivityDate = v.ActivityDate.HasValue ? v.ActivityDate.Value.ToString("MM/dd/yyyy HH:mm:ss") : null,
                                           RecId = v.RecId,
                                           Source = v.Source,
                                           CaseNumber = d.DisclosureNumber,
                                           Title = d.TradeSecret.DisclosureTitle
                                       };
                data.AddRange(disclosures);
            }

            return Ok(data);
        }
    }
}
