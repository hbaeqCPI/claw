using AutoMapper.QueryableExtensions;
using Kendo.Mvc.UI;
using Microsoft.EntityFrameworkCore;
using R10.Core.DTOs;
using R10.Core.Entities.Patent;
using R10.Core.Interfaces.Patent;
using R10.Web.Areas.Patent.ViewModels;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;
using R10.Web.Helpers;
using R10.Web.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Kendo.Mvc;
using R10.Core.Interfaces;
using R10.Core.Entities;
using R10.Core;
using R10.Core.Helpers;
using R10.Web.Services.SharePoint;
using R10.Web.Services;
using Microsoft.Extensions.Options;
using R10.Web.Areas.Shared.ViewModels.SharePoint;
using ActiveQueryBuilder.View;
using Kendo.Mvc.Extensions;
using System.ComponentModel;
using R10.Core.Entities.Documents;
using R10.Core.Entities.Shared;
using System.Security.Claims;
using R10.Core.Interfaces.Shared;
using ActiveQueryBuilder.View.DatabaseSchemaView;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.SharePoint.Client;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Bibliography;

namespace R10.Web.Areas.Patent.Services
{
    public class CountryApplicationViewModelService : ICountryApplicationViewModelService
    {
        private readonly ICountryApplicationService _applicationService;
        private readonly IInventionService _inventionService;
        private readonly IRTSService _rtsService;
        private readonly IApplicationDbContext _repository;
        private readonly ISystemSettings<PatSetting> _settings;
        private readonly IDocumentService _docService;
        private readonly IEntityService<PatDisclosureStatus> _disclosureStatusService;
        private readonly ISharePointService _sharePointService;
        private readonly GraphSettings _graphSettings;
        private readonly IWorkflowViewModelService _workflowViewModelService;
        private readonly IActionDueDeDocketService<PatActionDue, PatDueDate> _actionDueService;
        private readonly ITradeSecretService _tradeSecretService;
        private readonly ClaimsPrincipal _user;

        public CountryApplicationViewModelService(ICountryApplicationService applicationService,
                                                  IInventionService inventionService,
                                                  IRTSService rtsService,
                                                  IApplicationDbContext repository,
                                                  ISystemSettings<PatSetting> settings,
                                                  IDocumentService docService,
                                                  IEntityService<PatDisclosureStatus> disclosureStatusService,
                                                  ISharePointService sharePointService, IOptions<GraphSettings> graphSettings,
                                                  IWorkflowViewModelService workflowViewModelService, IActionDueDeDocketService<PatActionDue, PatDueDate> actionDueService,
                                                  ITradeSecretService tradeSecretService, ClaimsPrincipal user)
        {
            _applicationService = applicationService;
            _rtsService = rtsService;
            _inventionService = inventionService;
            _repository = repository;
            _settings = settings;
            _docService = docService;
            _disclosureStatusService = disclosureStatusService;
            _sharePointService = sharePointService;
            _graphSettings = graphSettings.Value;
            _workflowViewModelService = workflowViewModelService;
            _actionDueService = actionDueService;
            _tradeSecretService = tradeSecretService;
            _user = user;
        }

        public IQueryable<CountryApplication> AddCriteria(IQueryable<CountryApplication> applications, List<QueryFilterViewModel> mainSearchFilters)
        {
            if (mainSearchFilters.Count > 0)
            {
                var settings = _settings.GetSetting().GetAwaiter().GetResult();

                var countryOp = mainSearchFilters.GetFilterOperator("CountryOp");
                var country = mainSearchFilters.FirstOrDefault(f => f.Property == "Country");
                if (country != null)
                {
                    country.Operator = countryOp;
                    var countries = country.GetValueList();

                    if (countries.Count > 0)
                    {
                        if (country.Operator == "eq")
                            applications = applications.Where(m => countries.Contains(m.Country));
                        else
                            applications = applications.Where(m => !countries.Contains(m.Country));

                        mainSearchFilters.Remove(country);
                    }
                }

                var caseTypeOp = mainSearchFilters.GetFilterOperator("CaseTypeOp");
                var caseType = mainSearchFilters.FirstOrDefault(f => f.Property == "CaseType");
                if (caseType != null)
                {
                    caseType.Operator = caseTypeOp;
                    var caseTypes = caseType.GetValueList();

                    if (caseTypes.Count > 0)
                    {
                        if (caseType.Operator == "eq")
                            applications = applications.Where(m => caseTypes.Contains(m.CaseType));
                        else
                            applications = applications.Where(m => !caseTypes.Contains(m.CaseType));

                        mainSearchFilters.Remove(caseType);
                    }
                }

                var applicationStatusOp = mainSearchFilters.GetFilterOperator("ApplicationStatusOp");
                var applicationStatus = mainSearchFilters.FirstOrDefault(f => f.Property == "ApplicationStatus");
                if (applicationStatus != null)
                {
                    applicationStatus.Operator = applicationStatusOp;
                    var statuses = applicationStatus.GetValueList();

                    if (statuses.Count > 0)
                    {
                        if (applicationStatus.Operator == "eq")
                            applications = applications.Where(m => statuses.Contains(m.ApplicationStatus));
                        else
                            applications = applications.Where(m => !statuses.Contains(m.ApplicationStatus));

                        mainSearchFilters.Remove(applicationStatus);
                    }
                }

                var area = mainSearchFilters.FirstOrDefault(f => f.Property == "Area");
                if (area != null)
                {
                    applications = applications.Where(a => a.PatCountry.PatCountryAreas.Any(ca => ca.Area.Area == area.Value));
                    mainSearchFilters.Remove(area);
                }

                var indicatorOp = mainSearchFilters.GetFilterOperator("IndicatorOp");
                if (mainSearchFilters.FirstOrDefault(f => f.Property.StartsWith("DueDate.")) != null)
                {
                    Expression<Func<PatDueDate, bool>> dueDatePredicate = (item) => false;
                    Expression<Func<PatDueDate, bool>> dueDateDummyPredicate = (item) => false;

                    var actionDue = mainSearchFilters.FirstOrDefault(f => f.Property == "DueDate.ActionDue");
                    if (actionDue != null)
                    {
                        var actionDues = actionDue.GetValueListForLoop();
                        if (actionDues.Count > 0)
                        {
                            var actionDuePredicate = R10.Web.Helpers.ExpressionHelper.BuildLoopNestedPredicate<PatDueDate>("ActionDue", actionDues, false);
                            if (dueDatePredicate.ToString() == dueDateDummyPredicate.ToString())
                                dueDatePredicate = dueDatePredicate.Or(actionDuePredicate);
                            else
                                dueDatePredicate = dueDatePredicate.And(actionDuePredicate);
                        }
                    }

                    var indicator = mainSearchFilters.FirstOrDefault(f => f.Property == "DueDate.Indicator");
                    if (indicator != null)
                    {
                        indicator.Operator = indicatorOp;
                        var indicators = indicator.GetValueListForLoop();
                        if (indicators.Count > 0)
                        {
                            Expression<Func<PatDueDate, bool>> indicatorPredicate = dd => ((indicator.Operator == "eq" && indicators.Contains(dd.Indicator))
                                                                                                || (indicator.Operator != "eq" && !indicators.Contains(dd.Indicator)));
                            if (dueDatePredicate.ToString() == dueDateDummyPredicate.ToString())
                                dueDatePredicate = dueDatePredicate.Or(indicatorPredicate);
                            else
                                dueDatePredicate = dueDatePredicate.And(indicatorPredicate);
                        }
                    }

                    var dueDateFrom = mainSearchFilters.FirstOrDefault(f => f.Property == "DueDate.DueDateFrom");
                    var dueDateTo = mainSearchFilters.FirstOrDefault(f => f.Property == "DueDate.DueDateTo");
                    var dateTakenFrom = mainSearchFilters.FirstOrDefault(f => f.Property == "DueDate.DateTakenFrom");
                    var dateTakenTo = mainSearchFilters.FirstOrDefault(f => f.Property == "DueDate.DateTakenTo");
                    var outstandingOnly = mainSearchFilters.FirstOrDefault(f => f.Property == "DueDate.ShowOutstandingActionsOnly");

                    Expression<Func<PatDueDate, bool>> dueDateCombinedPredicate = d => (
                                                                    (dueDateFrom == null || d.DueDate >= Convert.ToDateTime(dueDateFrom.Value)) &&
                                                                    (dueDateTo == null || d.DueDate <= Convert.ToDateTime(dueDateTo.Value)) &&
                                                                    (dateTakenFrom == null || d.DateTaken >= Convert.ToDateTime(dateTakenFrom.Value)) &&
                                                                    (dateTakenTo == null || d.DateTaken <= Convert.ToDateTime(dateTakenTo.Value)) &&
                                                                    (outstandingOnly == null || d.DateTaken == null)
                                                                );

                    if (dueDatePredicate.ToString() == dueDateDummyPredicate.ToString())
                        dueDatePredicate = dueDatePredicate.Or(dueDateCombinedPredicate);
                    else
                        dueDatePredicate = dueDatePredicate.And(dueDateCombinedPredicate);


                    var ddAnyPredicate = R10.Core.ExpressionHelper.BuildAnyPredicate<PatActionDue>("DueDates", dueDatePredicate);
                    var actionAnyPredicate = R10.Core.ExpressionHelper.BuildAnyPredicate<CountryApplication>("ActionDues", ddAnyPredicate);

                    applications = applications.Where(actionAnyPredicate);

                    //applications = applications.Where(a => a.ActionDues.Any(ad => ad.DueDates.Any(d => (actionDue == null || EF.Functions.Like(d.ActionDue, actionDue.Value)) &&
                    //                                                                                   (indicator == null || EF.Functions.Like(d.Indicator, indicator.Value)) &&
                    //                                                                                   (dueDateFrom == null || d.DueDate >= Convert.ToDateTime(dueDateFrom.Value)) &&
                    //                                                                                   (dueDateTo == null || d.DueDate <= Convert.ToDateTime(dueDateTo.Value)) &&
                    //                                                                                   (dateTakenFrom == null || d.DateTaken >= Convert.ToDateTime(dateTakenFrom.Value)) &&
                    //                                                                                   (dateTakenTo == null || d.DateTaken <= Convert.ToDateTime(dateTakenTo.Value)) &&
                    //                                                                                   (outstandingOnly == null || d.DateTaken == null))));

                    mainSearchFilters.Remove(actionDue);
                    mainSearchFilters.Remove(indicator);
                    mainSearchFilters.Remove(dueDateFrom);
                    mainSearchFilters.Remove(dueDateTo);
                    mainSearchFilters.Remove(dateTakenFrom);
                    mainSearchFilters.Remove(dateTakenTo);
                    mainSearchFilters.Remove(outstandingOnly);
                }

                if (mainSearchFilters.FirstOrDefault(f => f.Property.StartsWith("AssignmentsHistory.")) != null)
                {
                    Expression<Func<PatAssignmentHistory, bool>> assignmentPredicate = (item) => false;
                    Expression<Func<PatAssignmentHistory, bool>> assignmentDummyPredicate = (item) => false;

                    var assignFrom = mainSearchFilters.FirstOrDefault(f => f.Property == "AssignmentsHistory.AssignmentFrom");
                    if (assignFrom != null)
                    {
                        var assignFroms = assignFrom.GetValueListForLoop();
                        if (assignFroms.Count > 0)
                        {
                            var assignFromPredicate = R10.Web.Helpers.ExpressionHelper.BuildLoopNestedPredicate<PatAssignmentHistory>("AssignmentFrom", assignFroms, false);
                            if (assignmentPredicate.ToString() == assignmentDummyPredicate.ToString())
                                assignmentPredicate = assignmentPredicate.Or(assignFromPredicate);
                            else
                                assignmentPredicate = assignmentPredicate.And(assignFromPredicate);
                        }
                    }

                    var assignTo = mainSearchFilters.FirstOrDefault(f => f.Property == "AssignmentsHistory.AssignmentTo");
                    if (assignTo != null)
                    {
                        var assignTos = assignTo.GetValueListForLoop();
                        if (assignTos.Count > 0)
                        {
                            var assignToPredicate = R10.Web.Helpers.ExpressionHelper.BuildLoopNestedPredicate<PatAssignmentHistory>("AssignmentTo", assignTos, false);
                            if (assignmentPredicate.ToString() == assignmentDummyPredicate.ToString())
                                assignmentPredicate = assignmentPredicate.Or(assignToPredicate);
                            else
                                assignmentPredicate = assignmentPredicate.And(assignToPredicate);
                        }
                    }

                    var assignDateFrom = mainSearchFilters.FirstOrDefault(f => f.Property == "AssignmentsHistory.AssignmentDateFrom");
                    var assignDateTo = mainSearchFilters.FirstOrDefault(f => f.Property == "AssignmentsHistory.AssignmentDateTo");
                    var assignStatus = mainSearchFilters.FirstOrDefault(f => f.Property == "AssignmentsHistory.AssignmentStatus");
                    var reel = mainSearchFilters.FirstOrDefault(f => f.Property == "AssignmentsHistory.Reel");
                    var frame = mainSearchFilters.FirstOrDefault(f => f.Property == "AssignmentsHistory.Frame");

                    Expression<Func<PatAssignmentHistory, bool>> assignCombinedPredicate = a => (
                                                        (assignDateFrom == null || a.AssignmentDate >= Convert.ToDateTime(assignDateFrom.Value)) &&
                                                        (assignDateTo == null || a.AssignmentDate <= Convert.ToDateTime(assignDateTo.Value).AddDays(1).AddSeconds(-1)) &&
                                                        (assignStatus == null || EF.Functions.Like(a.AssignmentStatus, assignStatus.Value)) &&
                                                        (reel == null || EF.Functions.Like(a.Reel, reel.Value)) &&
                                                        (frame == null || EF.Functions.Like(a.Frame, frame.Value))
                                                    );

                    if (assignmentPredicate.ToString() == assignmentDummyPredicate.ToString())
                        assignmentPredicate = assignmentPredicate.Or(assignCombinedPredicate);
                    else
                        assignmentPredicate = assignmentPredicate.And(assignCombinedPredicate);

                    var assignmentAnyPredicate = R10.Core.ExpressionHelper.BuildAnyPredicate<CountryApplication>("AssignmentsHistory", assignmentPredicate);
                    applications = applications.Where(assignmentAnyPredicate);

                    //applications = applications.Where(ca => ca.AssignmentsHistory
                    //                    .Any(a => (assignFrom == null || EF.Functions.Like(a.AssignmentFrom, assignFrom.Value)) &&
                    //                                (assignTo == null || EF.Functions.Like(a.AssignmentTo, assignTo.Value)) &&
                    //                                (assignDateFrom == null || a.AssignmentDate >= Convert.ToDateTime(assignDateFrom.Value)) &&
                    //                                (assignDateTo == null || a.AssignmentDate <= Convert.ToDateTime(assignDateTo.Value).AddDays(1).AddSeconds(-1)) &&
                    //                                (assignStatus == null || EF.Functions.Like(a.AssignmentStatus, assignStatus.Value)) &&
                    //                                (reel == null || EF.Functions.Like(a.Reel, reel.Value)) &&
                    //                                (frame == null || EF.Functions.Like(a.Frame, frame.Value))));

                    mainSearchFilters.Remove(assignFrom);
                    mainSearchFilters.Remove(assignTo);
                    mainSearchFilters.Remove(assignDateFrom);
                    mainSearchFilters.Remove(assignDateTo);
                    mainSearchFilters.Remove(assignStatus);
                    mainSearchFilters.Remove(reel);
                    mainSearchFilters.Remove(frame);
                }

                if (mainSearchFilters.FirstOrDefault(f => f.Property.StartsWith("Licensees.")) != null)
                {
                    Expression<Func<PatLicensee, bool>> licanseePredicate = (item) => false;
                    Expression<Func<PatLicensee, bool>> licanseeDummyPredicate = (item) => false;
                    var licensor = mainSearchFilters.FirstOrDefault(f => f.Property == "Licensees.Licensor");
                    if (licensor != null)
                    {
                        var licensors = licensor.GetValueListForLoop();
                        if (licensors.Count > 0)
                        {
                            var licensorPredicate = R10.Web.Helpers.ExpressionHelper.BuildLoopNestedPredicate<PatLicensee>("Licensor", licensors, false);
                            if (licanseePredicate.ToString() == licanseeDummyPredicate.ToString())
                                licanseePredicate = licanseePredicate.Or(licensorPredicate);
                            else
                                licanseePredicate = licanseePredicate.And(licensorPredicate);
                        }
                    }

                    var licensee = mainSearchFilters.FirstOrDefault(f => f.Property == "Licensees.Licensee");
                    if (licensee != null)
                    {
                        var licensees = licensee.GetValueListForLoop();
                        if (licensees.Count > 0)
                        {
                            var licenseePredicate = R10.Web.Helpers.ExpressionHelper.BuildLoopNestedPredicate<PatLicensee>("Licensee", licensees, false);
                            if (licanseePredicate.ToString() == licanseeDummyPredicate.ToString())
                                licanseePredicate = licanseePredicate.Or(licenseePredicate);
                            else
                                licanseePredicate = licanseePredicate.And(licenseePredicate);
                        }
                    }

                    var licenseNo = mainSearchFilters.FirstOrDefault(f => f.Property == "Licensees.LicenseNo");
                    var licenseType = mainSearchFilters.FirstOrDefault(f => f.Property == "Licensees.LicenseType");
                    var licenseStartFrom = mainSearchFilters.FirstOrDefault(f => f.Property == "Licensees.LicenseStartFrom");
                    var licenseStartTo = mainSearchFilters.FirstOrDefault(f => f.Property == "Licensees.LicenseStartTo");
                    var licenseExpireFrom = mainSearchFilters.FirstOrDefault(f => f.Property == "Licensees.LicenseExpireFrom");
                    var licenseExpireTo = mainSearchFilters.FirstOrDefault(f => f.Property == "Licensees.LicenseExpireTo");

                    Expression<Func<PatLicensee, bool>> licenseeCombinedPredicate = l => (
                                                        (licenseNo == null || EF.Functions.Like(l.LicenseNo, licenseNo.Value)) &&
                                                        (licenseType == null || EF.Functions.Like(l.LicenseType, licenseType.Value)) &&
                                                        (licenseStartFrom == null || l.LicenseStart >= Convert.ToDateTime(licenseStartFrom.Value)) &&
                                                        (licenseStartTo == null || l.LicenseStart <= Convert.ToDateTime(licenseStartTo.Value)) &&
                                                        (licenseExpireFrom == null || l.LicenseExpire >= Convert.ToDateTime(licenseExpireFrom.Value)) &&
                                                        (licenseExpireTo == null || l.LicenseExpire <= Convert.ToDateTime(licenseExpireTo.Value))
                                                    );

                    if (licanseePredicate.ToString() == licanseeDummyPredicate.ToString())
                        licanseePredicate = licanseePredicate.Or(licenseeCombinedPredicate);
                    else
                        licanseePredicate = licanseePredicate.And(licenseeCombinedPredicate);

                    var licenseeAnyPredicate = R10.Core.ExpressionHelper.BuildAnyPredicate<CountryApplication>("Licensees", licanseePredicate);
                    applications = applications.Where(licenseeAnyPredicate);

                    mainSearchFilters.Remove(licensor);
                    mainSearchFilters.Remove(licensee);
                    mainSearchFilters.Remove(licenseNo);
                    mainSearchFilters.Remove(licenseType);
                    mainSearchFilters.Remove(licenseStartFrom);
                    mainSearchFilters.Remove(licenseStartTo);
                    mainSearchFilters.Remove(licenseExpireFrom);
                    mainSearchFilters.Remove(licenseExpireTo);
                }

                var costTypeOp = mainSearchFilters.GetFilterOperator("CostTypeOp");
                if (mainSearchFilters.FirstOrDefault(f => f.Property.StartsWith("CostTrackings.")) != null)
                {
                    Expression<Func<PatCostTrack, bool>> costTrackPredicate = (item) => false;
                    Expression<Func<PatCostTrack, bool>> costTrackDummyPredicate = (item) => false;

                    var costType = mainSearchFilters.FirstOrDefault(f => f.Property == "CostTrackings.CostType");
                    if (costType != null)
                    {
                        costType.Operator = costTypeOp;
                        var costTypes = costType.GetValueListForLoop();
                        if (costTypes.Count > 0)
                        {
                            Expression<Func<PatCostTrack, bool>> predicate = (item) => false;
                            if (costType.Operator == "eq")
                            {
                                foreach (var val in costTypes)
                                {
                                    predicate = predicate.Or(ct => EF.Functions.Like(ct.CostType, val));
                                }
                            }
                            else
                            {
                                foreach (var val in costTypes)
                                {
                                    predicate = predicate.Or(ct => !EF.Functions.Like(ct.CostType, val));
                                }
                            }

                            if (costTrackPredicate.ToString() == costTrackDummyPredicate.ToString())
                                costTrackPredicate = costTrackPredicate.Or(predicate);
                            else
                                costTrackPredicate = costTrackPredicate.And(predicate);
                        }
                    }

                    var invoiceNumber = mainSearchFilters.FirstOrDefault(f => f.Property == "CostTrackings.InvoiceNumber");
                    var invoiceDateFrom = mainSearchFilters.FirstOrDefault(f => f.Property == "CostTrackings.InvoiceDateFrom");
                    var invoiceDateTo = mainSearchFilters.FirstOrDefault(f => f.Property == "CostTrackings.InvoiceDateTo");
                    var paymentDateFrom = mainSearchFilters.FirstOrDefault(f => f.Property == "CostTrackings.PayDateFrom");
                    var paymentDateTo = mainSearchFilters.FirstOrDefault(f => f.Property == "CostTrackings.PayDateTo");

                    Expression<Func<PatCostTrack, bool>> costTrackCombinedPredicate = c => (
                                                    (invoiceNumber == null || EF.Functions.Like(c.InvoiceNumber, invoiceNumber.Value)) &&
                                                    (invoiceDateFrom == null || c.InvoiceDate >= Convert.ToDateTime(invoiceDateFrom.Value)) &&
                                                    (invoiceDateTo == null || c.InvoiceDate <= Convert.ToDateTime(invoiceDateTo.Value)) &&
                                                    (paymentDateFrom == null || c.PayDate >= Convert.ToDateTime(paymentDateFrom.Value)) &&
                                                    (paymentDateTo == null || c.PayDate <= Convert.ToDateTime(paymentDateTo.Value))
                                                );

                    if (costTrackPredicate.ToString() == costTrackDummyPredicate.ToString())
                        costTrackPredicate = costTrackPredicate.Or(costTrackCombinedPredicate);
                    else
                        costTrackPredicate = costTrackPredicate.And(costTrackCombinedPredicate);

                    var costTrackAnyPredicate = R10.Core.ExpressionHelper.BuildAnyPredicate<CountryApplication>("CostTrackings", costTrackPredicate);
                    applications = applications.Where(costTrackAnyPredicate);

                    //applications = applications.Where(a => a.CostTrackings
                    //                    .Any(c => (costType == null || EF.Functions.Like(c.CostType, costType.Value)) &&
                    //                                (invoiceNumber == null || EF.Functions.Like(c.InvoiceNumber, invoiceNumber.Value)) &&
                    //                                (invoiceDateFrom == null || c.InvoiceDate >= Convert.ToDateTime(invoiceDateFrom.Value)) &&
                    //                                (invoiceDateTo == null || c.InvoiceDate <= Convert.ToDateTime(invoiceDateTo.Value)) &&
                    //                                (paymentDateFrom == null || c.PayDate >= Convert.ToDateTime(paymentDateFrom.Value)) &&
                    //                                (paymentDateTo == null || c.PayDate <= Convert.ToDateTime(paymentDateTo.Value))));

                    mainSearchFilters.Remove(costType);
                    mainSearchFilters.Remove(invoiceNumber);
                    mainSearchFilters.Remove(invoiceDateFrom);
                    mainSearchFilters.Remove(invoiceDateTo);
                    mainSearchFilters.Remove(paymentDateFrom);
                    mainSearchFilters.Remove(paymentDateTo);
                }

                if (mainSearchFilters.FirstOrDefault(f => f.Property.StartsWith("Client.")) != null)
                {
                    Expression<Func<CountryApplication, bool>> appPredicate = (item) => false;
                    Expression<Func<CountryApplication, bool>> appDummyPredicate = (item) => false;
                    var clientCode = mainSearchFilters.FirstOrDefault(f => f.Property == "Client.ClientCode");
                    if (clientCode != null)
                    {
                        var clientCodes = clientCode.GetValueListForLoop();
                        if (clientCodes.Count > 0)
                        {
                            Expression<Func<CountryApplication, bool>> clienCodePredicate = (item) => false;
                            foreach (var val in clientCodes)
                            {
                                clienCodePredicate = clienCodePredicate.Or(app => EF.Functions.Like(app.Invention.Client.ClientCode, val));
                            }

                            if (appPredicate.ToString() == appDummyPredicate.ToString())
                                appPredicate = appPredicate.Or(clienCodePredicate);
                            else
                                appPredicate = appPredicate.And(clienCodePredicate);
                        }
                    }

                    var clientName = mainSearchFilters.FirstOrDefault(f => f.Property == "Client.ClientName");
                    if (clientName != null)
                    {
                        var clientNames = clientName.GetValueListForLoop();
                        if (clientNames.Count > 0)
                        {
                            Expression<Func<CountryApplication, bool>> clienNamePredicate = (item) => false;
                            foreach (var val in clientNames)
                            {
                                clienNamePredicate = clienNamePredicate.Or(app => EF.Functions.Like(app.Invention.Client.ClientName, val));
                            }

                            if (appPredicate.ToString() == appDummyPredicate.ToString())
                                appPredicate = appPredicate.Or(clienNamePredicate);
                            else
                                appPredicate = appPredicate.And(clienNamePredicate);
                        }
                    }

                    applications = applications.Where(appPredicate);

                    //applications = applications.Where(a =>
                    //    (clientCode == null || EF.Functions.Like(a.Invention.Client.ClientCode, clientCode.Value)) &&
                    //    (clientName == null || EF.Functions.Like(a.Invention.Client.ClientName, clientName.Value))
                    //);

                    mainSearchFilters.Remove(clientCode);
                    mainSearchFilters.Remove(clientName);
                }

                if (mainSearchFilters.FirstOrDefault(f => f.Property.StartsWith("Agent.")) != null)
                {
                    var agentCode = mainSearchFilters.FirstOrDefault(f => f.Property == "Agent.AgentCode");
                    var agentName = mainSearchFilters.FirstOrDefault(f => f.Property == "Agent.AgentName");

                    if (agentCode != null)
                    {
                        var agentCodes = agentCode.GetValueListForLoop();
                        if (agentCodes.Count > 0)
                        {
                            Expression<Func<CountryApplication, bool>> agentCodePredicate = (item) => false;
                            foreach (var val in agentCodes)
                            {
                                agentCodePredicate = agentCodePredicate.Or(app => EF.Functions.Like(app.Agent.AgentCode, val));
                            }
                            applications = applications.Where(agentCodePredicate);
                        }
                    }
                    if (agentName != null)
                    {
                        var agentNames = agentName.GetValueListForLoop();
                        if (agentNames.Count > 0)
                        {
                            Expression<Func<CountryApplication, bool>> agentNamePredicate = (item) => false;
                            foreach (var val in agentNames)
                            {
                                agentNamePredicate = agentNamePredicate.Or(app => EF.Functions.Like(app.Agent.AgentName, val));
                            }
                            applications = applications.Where(agentNamePredicate);
                        }
                    }

                    //applications = applications.Where(a => (agentCode == null || EF.Functions.Like(a.Agent.AgentCode, agentCode.Value)) &&
                    //                                       (agentName == null || EF.Functions.Like(a.Agent.AgentName, agentName.Value)));
                    mainSearchFilters.Remove(agentCode);
                    mainSearchFilters.Remove(agentName);
                }

                if (mainSearchFilters.FirstOrDefault(f => f.Property.StartsWith("TaxAgent.")) != null)
                {
                    var agentCode = mainSearchFilters.FirstOrDefault(f => f.Property == "TaxAgent.AgentCode");
                    var agentName = mainSearchFilters.FirstOrDefault(f => f.Property == "TaxAgent.AgentName");

                    if (agentCode != null)
                    {
                        var agentCodes = agentCode.GetValueListForLoop();
                        if (agentCodes.Count > 0)
                        {
                            Expression<Func<CountryApplication, bool>> agentCodePredicate = (item) => false;
                            foreach (var val in agentCodes)
                            {
                                agentCodePredicate = agentCodePredicate.Or(app => EF.Functions.Like(app.TaxAgent.AgentCode, val));
                            }
                            applications = applications.Where(agentCodePredicate);
                        }
                    }
                    if (agentName != null)
                    {
                        var agentNames = agentName.GetValueListForLoop();
                        if (agentNames.Count > 0)
                        {
                            Expression<Func<CountryApplication, bool>> agentNamePredicate = (item) => false;
                            foreach (var val in agentNames)
                            {
                                agentNamePredicate = agentNamePredicate.Or(app => EF.Functions.Like(app.TaxAgent.AgentName, val));
                            }
                            applications = applications.Where(agentNamePredicate);
                        }
                    }

                    mainSearchFilters.Remove(agentCode);
                    mainSearchFilters.Remove(agentName);
                }

                if (mainSearchFilters.FirstOrDefault(f => f.Property.StartsWith("LegalRepresentative.")) != null)
                {
                    var agentCode = mainSearchFilters.FirstOrDefault(f => f.Property == "LegalRepresentative.AgentCode");
                    var agentName = mainSearchFilters.FirstOrDefault(f => f.Property == "LegalRepresentative.AgentName");

                    if (agentCode != null)
                    {
                        var agentCodes = agentCode.GetValueListForLoop();
                        if (agentCodes.Count > 0)
                        {
                            Expression<Func<CountryApplication, bool>> agentCodePredicate = (item) => false;
                            foreach (var val in agentCodes)
                            {
                                agentCodePredicate = agentCodePredicate.Or(app => EF.Functions.Like(app.LegalRepresentative.AgentCode, val));
                            }
                            applications = applications.Where(agentCodePredicate);
                        }
                    }
                    if (agentName != null)
                    {
                        var agentNames = agentName.GetValueListForLoop();
                        if (agentNames.Count > 0)
                        {
                            Expression<Func<CountryApplication, bool>> agentNamePredicate = (item) => false;
                            foreach (var val in agentNames)
                            {
                                agentNamePredicate = agentNamePredicate.Or(app => EF.Functions.Like(app.LegalRepresentative.AgentName, val));
                            }
                            applications = applications.Where(agentNamePredicate);
                        }
                    }

                    mainSearchFilters.Remove(agentCode);
                    mainSearchFilters.Remove(agentName);
                }


                if (mainSearchFilters.FirstOrDefault(f => f.Property.StartsWith("Owner.")) != null)
                {
                    Expression<Func<PatOwnerApp, bool>> ownerPredicate = (item) => false;
                    Expression<Func<PatOwnerApp, bool>> ownerDummyPredicate = (item) => false;
                    var ownerCode = mainSearchFilters.FirstOrDefault(f => f.Property == "Owner.OwnerCode");
                    if (ownerCode != null)
                    {
                        var ownerCodes = ownerCode.GetValueListForLoop();
                        if (ownerCodes.Count > 0)
                        {
                            var ownerCodePredicate = R10.Web.Helpers.ExpressionHelper.BuildLoopNestedPredicate<PatOwnerApp>("Owner.OwnerCode", ownerCodes, false);
                            if (ownerPredicate.ToString() == ownerDummyPredicate.ToString())
                                ownerPredicate = ownerPredicate.Or(ownerCodePredicate);
                            else
                                ownerPredicate = ownerPredicate.And(ownerCodePredicate);
                        }
                    }
                    var ownerName = mainSearchFilters.FirstOrDefault(f => f.Property == "Owner.OwnerName");
                    if (ownerName != null)
                    {
                        var ownerNames = ownerName.GetValueListForLoop();
                        if (ownerNames.Count > 0)
                        {
                            var ownerNamePredicate = R10.Web.Helpers.ExpressionHelper.BuildLoopNestedPredicate<PatOwnerApp>("Owner.OwnerName", ownerNames, false);
                            if (ownerPredicate.ToString() == ownerDummyPredicate.ToString())
                                ownerPredicate = ownerPredicate.Or(ownerNamePredicate);
                            else
                                ownerPredicate = ownerPredicate.And(ownerNamePredicate);
                        }
                    }
                    var appAnyPredicate = R10.Core.ExpressionHelper.BuildAnyPredicate<CountryApplication>("Owners", ownerPredicate);
                    applications = applications.Where(appAnyPredicate);

                    //applications = applications.Where(a => (ownerCode == null || a.Owners.Any(ao => EF.Functions.Like(ao.Owner.OwnerCode, ownerCode.Value))) &&
                    //                                       (ownerName == null || a.Owners.Any(ao => EF.Functions.Like(ao.Owner.OwnerName, ownerName.Value))));
                    mainSearchFilters.Remove(ownerCode);
                    mainSearchFilters.Remove(ownerName);
                }

                if (mainSearchFilters.FirstOrDefault(f => f.Property.StartsWith("Product.")) != null)
                {
                    Expression<Func<PatProduct, bool>> productPredicate = (item) => false;
                    Expression<Func<PatProduct, bool>> productDummyPredicate = (item) => false;
                    var productName = mainSearchFilters.FirstOrDefault(f => f.Property == "Product.ProductName");
                    if (productName != null)
                    {
                        var productNames = productName.GetValueListForLoop();
                        if (productNames.Count > 0)
                        {
                            var productNamePredicate = R10.Web.Helpers.ExpressionHelper.BuildLoopNestedPredicate<PatProduct>("Product.ProductName", productNames, false);
                            if (productPredicate.ToString() == productDummyPredicate.ToString())
                                productPredicate = productPredicate.Or(productNamePredicate);
                            else
                                productPredicate = productPredicate.And(productNamePredicate);
                        }
                    }
                    var productCategory = mainSearchFilters.FirstOrDefault(f => f.Property == "Product.ProductCategory");
                    if (productCategory != null)
                    {
                        var productCategories = productCategory.GetValueListForLoop();
                        if (productCategories.Count > 0)
                        {
                            var productCategoryPredicate = R10.Web.Helpers.ExpressionHelper.BuildLoopNestedPredicate<PatProduct>("Product.ProductCategory", productCategories, false);
                            if (productPredicate.ToString() == productDummyPredicate.ToString())
                                productPredicate = productPredicate.Or(productCategoryPredicate);
                            else
                                productPredicate = productPredicate.And(productCategoryPredicate);
                        }
                    }

                    var appAnyPredicate = R10.Core.ExpressionHelper.BuildAnyPredicate<CountryApplication>("Products", productPredicate);
                    applications = applications.Where(appAnyPredicate);

                    //applications = applications.Where(a => (productName == null || a.Products.Any(ap => EF.Functions.Like(ap.Product.ProductName, productName.Value))) &&
                    //                                        (productCategory == null || a.Products.Any(ap => EF.Functions.Like(ap.Product.ProductCategory, productCategory.Value))));

                    mainSearchFilters.Remove(productName);
                    mainSearchFilters.Remove(productCategory);
                }

                if (mainSearchFilters.FirstOrDefault(f => f.Property.StartsWith("PatentScore.")) != null)
                {
                    var patScoreRemarks = mainSearchFilters.FirstOrDefault(f => f.Property == "PatentScore.Remarks");
                    var patScoreFrom = mainSearchFilters.FirstOrDefault(f => f.Property == "PatentScore.ScoreFrom");
                    var patScoreTo = mainSearchFilters.FirstOrDefault(f => f.Property == "PatentScore.ScoreTo");

                    if (patScoreRemarks != null)
                    {
                        applications = applications.Where(a => a.PatScores.Any(ap => EF.Functions.Like(ap.Remarks, patScoreRemarks.Value)));
                        mainSearchFilters.Remove(patScoreRemarks);
                    }

                    if (patScoreFrom != null || patScoreTo != null)
                    {
                        applications = applications.Where(a => _repository.PatAverageScoreDTO.Any(av => av.AppId == a.AppId &&
                          (
                            (patScoreFrom == null || av.AverageScore >= Convert.ToDouble(patScoreFrom.Value)) &&
                            (patScoreTo == null || av.AverageScore <= Convert.ToDouble(patScoreTo.Value))
                          )
                        ));
                        mainSearchFilters.Remove(patScoreFrom);
                        mainSearchFilters.Remove(patScoreTo);
                    }

                }

                if (mainSearchFilters.FirstOrDefault(f => f.Property.StartsWith("Attorney.")) != null)
                {
                    Expression<Func<CountryApplication, bool>> appPredicate = (item) => false;
                    Expression<Func<CountryApplication, bool>> appDummyPredicate = (item) => false;
                    var attorneyCode = mainSearchFilters.FirstOrDefault(f => f.Property == "Attorney.AttorneyCode");
                    if (attorneyCode != null)
                    {
                        var attorneyCodes = attorneyCode.GetValueListForLoop();
                        if (attorneyCodes.Count > 0)
                        {
                            Expression<Func<CountryApplication, bool>> attorneyCodePredicate = (item) => false;
                            foreach (var val in attorneyCodes)
                            {
                                attorneyCodePredicate = attorneyCodePredicate.Or(a => EF.Functions.Like(a.Invention.Attorney1.AttorneyCode, val) ||
                                                                                      EF.Functions.Like(a.Invention.Attorney2.AttorneyCode, val) ||
                                                                                      EF.Functions.Like(a.Invention.Attorney3.AttorneyCode, val) ||
                                                                                      EF.Functions.Like(a.Invention.Attorney4.AttorneyCode, val) ||
                                                                                      EF.Functions.Like(a.Invention.Attorney5.AttorneyCode, val));
                            }

                            if (appPredicate.ToString() == appDummyPredicate.ToString())
                                appPredicate = appPredicate.Or(attorneyCodePredicate);
                            else
                                appPredicate = appPredicate.And(attorneyCodePredicate);
                        }
                    }

                    var attorneyName = mainSearchFilters.FirstOrDefault(f => f.Property == "Attorney.AttorneyName");
                    if (attorneyName != null)
                    {
                        var attorneyNames = attorneyName.GetValueListForLoop();
                        if (attorneyNames.Count > 0)
                        {
                            Expression<Func<CountryApplication, bool>> attorneyNamePredicate = (item) => false;
                            foreach (var val in attorneyNames)
                            {
                                attorneyNamePredicate = attorneyNamePredicate.Or(a => EF.Functions.Like(a.Invention.Attorney1.AttorneyName, val) ||
                                                                                    EF.Functions.Like(a.Invention.Attorney2.AttorneyName, val) ||
                                                                                    EF.Functions.Like(a.Invention.Attorney3.AttorneyName, val) ||
                                                                                    EF.Functions.Like(a.Invention.Attorney4.AttorneyName, val) ||
                                                                                    EF.Functions.Like(a.Invention.Attorney5.AttorneyName, val));
                            }

                            if (appPredicate.ToString() == appDummyPredicate.ToString())
                                appPredicate = appPredicate.Or(attorneyNamePredicate);
                            else
                                appPredicate = appPredicate.And(attorneyNamePredicate);
                        }
                    }

                    applications = applications.Where(appPredicate);
                    mainSearchFilters.Remove(attorneyCode);
                    mainSearchFilters.Remove(attorneyName);
                }

                var inventor = mainSearchFilters.FirstOrDefault(f => f.Property == "AppInventor");
                if (inventor != null)
                {
                    var inventors = inventor.GetValueListForLoop();
                    if (inventors.Count > 0)
                    {
                        Expression<Func<PatInventorApp, bool>> predicate = (item) => false;
                        foreach (var invt in inventors)
                        {
                            predicate = predicate.Or(a => EF.Functions.Like(a.InventorAppInventor.Inventor, invt));
                        }
                        var appAnyPredicate = R10.Core.ExpressionHelper.BuildAnyPredicate<CountryApplication>("Inventors", predicate);
                        applications = applications.Where(appAnyPredicate);
                    }
                    //applications = applications.Where(a => a.Inventors.Any(i => EF.Functions.Like(i.InventorAppInventor.Inventor, inventor.Value)));
                    mainSearchFilters.Remove(inventor);
                }

                var parentCase = mainSearchFilters.FirstOrDefault(f => f.Property == "ParentCase");
                if (parentCase != null)
                {
                    applications = applications.Where(a => _applicationService.ParentApplications.Any(p => EF.Functions.Like(p.ParentCase, parentCase.Value) && p.ParentId == a.ParentAppId));
                    mainSearchFilters.Remove(parentCase);
                }

                var parentCaseTD = mainSearchFilters.FirstOrDefault(f => f.Property == "ParentCaseTD");
                if (parentCaseTD != null)
                {
                    applications = applications.Where(a => _applicationService.TerminalDisclaimerParents.Any(p => EF.Functions.Like(p.ParentCase, parentCaseTD.Value) && a.PatTerminalDisclaimers.Any(td => p.ParentId == td.TerminalDisclaimerAppId)));
                    mainSearchFilters.Remove(parentCaseTD);
                }

                var rtsDownloaded = mainSearchFilters.FirstOrDefault(f => f.Property == "RTS.Display");
                if (rtsDownloaded != null && !string.IsNullOrEmpty(rtsDownloaded.Value))
                {
                    applications = applications.Where(a => _rtsService.RTSSearchRecords.Any(s =>
                        s.PMSAppId == a.AppId && ((rtsDownloaded.Value == "downloaded" && s.LastWebUpdate != null) || (rtsDownloaded.Value != "downloaded" && s.LastWebUpdate == null))));
                    mainSearchFilters.Remove(rtsDownloaded);
                }

                var rtsAction = mainSearchFilters.FirstOrDefault(f => f.Property == "RTS.Action");
                if (rtsAction != null && !string.IsNullOrEmpty(rtsAction.Value))
                {
                    applications = applications.Where(a => _rtsService.RTSSearchRecords.Any(s =>
                        s.PMSAppId == a.AppId && s.RTSSearchActions.Any(ac => EF.Functions.Like(ac.SearchAction, rtsAction.Value))));
                    mainSearchFilters.Remove(rtsAction);
                }

                var familyNumber = mainSearchFilters.FirstOrDefault(f => f.Property == "FamilyNumber");
                if (familyNumber != null && !string.IsNullOrEmpty(familyNumber.Value))
                {
                    applications = applications.Where(a => EF.Functions.Like(a.Invention.FamilyNumber, familyNumber.Value));
                    mainSearchFilters.Remove(familyNumber);
                }

                if (mainSearchFilters.FirstOrDefault(f => f.Property.StartsWith("Images.")) != null)
                {
                    var docName = mainSearchFilters.FirstOrDefault(f => f.Property == "Images.DocName");
                    var tag = mainSearchFilters.FirstOrDefault(f => f.Property == "Images.Tag");
                    var dateCreatedFrom = mainSearchFilters.FirstOrDefault(f => f.Property == "Images.DateCreatedFrom");
                    var dateCreatedTo = mainSearchFilters.FirstOrDefault(f => f.Property == "Images.DateCreatedTo");
                    var isVerified = mainSearchFilters.FirstOrDefault(f => f.Property == "Images.IsVerified");

                    if (settings.IsSharePointIntegrationOn && settings.IsSharePointListRealTime)
                    {
                        var graphClient = _sharePointService.GetGraphClient();
                        var docs = new List<SharePointGraphDocPicklistViewModel>();

                        if (settings.IsSharePointIntegrationByMetadataOn)
                            docs = graphClient.GetSiteDocumentNamesByMetadata(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, SharePointDocLibrary.Patent, SharePointDocLibraryFolder.Application, docName != null ? docName.Value : "").GetAwaiter().GetResult();
                        else
                            docs = graphClient.GetSiteDocumentNames(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, SharePointDocLibrary.Patent, new List<string> { SharePointDocLibraryFolder.Application }, docName != null ? docName.Value : "").GetAwaiter().GetResult();

                        if (dateCreatedFrom != null)
                        {
                            docs = docs.Where(d => d.DateModified >= Convert.ToDateTime(dateCreatedFrom.Value)).ToList();
                        }
                        if (dateCreatedTo != null)
                        {
                            docs = docs.Where(d => d.DateModified <= Convert.ToDateTime(dateCreatedTo.Value)).ToList();
                        }

                        if (docs.Count > 0)
                        {
                            docs.ForEach(d =>
                            {
                                var caseNumber = "";
                                var country = "";
                                var subCase = "";

                                if (settings.IsSharePointIntegrationByMetadataOn)
                                {
                                    var recKeys = d.RecKey.Split(SharePointSeparator.Field);
                                    caseNumber = recKeys[0];
                                    country = recKeys[1];
                                    if (recKeys.Length > 2)
                                    {
                                        subCase = recKeys[2];
                                    }
                                }
                                else
                                {
                                    var folders = d.Folder.Split("/");
                                    caseNumber = folders[1];
                                    var countrySubCase = folders[2].Split(SharePointSeparator.Field);
                                    country = countrySubCase[0];
                                    if (countrySubCase.Length > 1)
                                    {
                                        subCase = countrySubCase[1];
                                    }
                                }
                                var application = _applicationService.CountryApplications.Where(a => a.CaseNumber == caseNumber && a.Country == country && a.SubCase == subCase).FirstOrDefaultAsync().GetAwaiter().GetResult();
                                if (application != null)
                                    d.ParentId = application.AppId;
                            });
                            var recKeys = docs.Select(d => d.ParentId).ToList();
                            applications = applications.Where(a => recKeys.Contains(a.AppId));
                        }
                        else
                        {
                            applications = applications.Where(a => false);
                        }
                    }
                    else
                    {
                        var tagsList = "";
                        if (tag != null)
                        {
                            var tags = tag.GetValueListForLoop();
                            if (tags.Count > 1)
                            {
                                foreach (var val in tags)
                                {
                                    tagsList = tagsList + val + "~";
                                }
                            }
                        }

                        Expression<Func<CountryApplication, bool>> docsPredicate = (item) => true;
                        docsPredicate = docsPredicate.And(a =>
                          _docService.DocDocuments.Any(d =>
                              (d.DocFolder.SystemType == SystemTypeCode.Patent && d.DocFolder.DataKey == "AppId" && d.DocFolder.DataKeyValue == a.AppId) &&
                              (docName == null || EF.Functions.Like(d.DocName, docName.Value)) &&
                              (tag == null || (string.IsNullOrEmpty(tagsList) && d.DocDocumentTags.Any(t => EF.Functions.Like(t.Tag, tag.Value))) || (!string.IsNullOrEmpty(tagsList) && d.DocDocumentTags.Any(t => EF.Functions.Like(tagsList, '%' + t.Tag + '%')))) &&
                              (isVerified == null || d.IsVerified == Convert.ToBoolean(isVerified.Value)) &&
                              (dateCreatedFrom == null || d.DateCreated >= Convert.ToDateTime(dateCreatedFrom.Value)) &&
                              (dateCreatedTo == null || d.DateCreated <= Convert.ToDateTime(dateCreatedTo.Value))
                          )
                        );
                        applications = applications.Where(docsPredicate);
                    }


                    mainSearchFilters.Remove(docName);
                    mainSearchFilters.Remove(tag);
                    mainSearchFilters.Remove(dateCreatedFrom);
                    mainSearchFilters.Remove(dateCreatedTo);
                    mainSearchFilters.Remove(isVerified);
                }

                var inventionCustomFieldFilters = mainSearchFilters.Where(f => f.Property.StartsWith("InventionCustomField.")).ToList();
                if (inventionCustomFieldFilters.Any())
                {

                    foreach (var item in inventionCustomFieldFilters)
                    {
                        Expression<Func<CountryApplication, bool>> customFieldPredicate = (item) => true;

                        var field = item.Property.Split(".");
                        if (field[1].ToLower() != "d")
                        {
                            var value = item.Value.ToLower();
                            var expr = R10.Web.Helpers.ExpressionHelper.BuildNestedPredicate<CountryApplication>($"Invention.{field[2]}", value);
                            customFieldPredicate = customFieldPredicate.And(expr);
                        }
                        else if (field[1].ToLower() == "d")
                        {
                            var filterOperator = "eq";
                            if (field[3].ToLower() == "from")
                                filterOperator = "gte";
                            else if (field[3].ToLower() == "to")
                                filterOperator = "lte";

                            var value = item.Value.ToLower();
                            var expr = R10.Web.Helpers.ExpressionHelper.BuildNestedPredicate<CountryApplication>($"Invention.{field[2]}", value, true, filterOperator);
                            customFieldPredicate = customFieldPredicate.And(expr);
                        }
                        applications = applications.Where(customFieldPredicate);
                        mainSearchFilters.Remove(item);
                    }
                }


                if (mainSearchFilters.FirstOrDefault(f => f.Property.StartsWith("DocVerify.")) != null)
                {
                    var actionType = mainSearchFilters.FirstOrDefault(f => f.Property == "DocVerify.ActionType");
                    var verifiedBy = mainSearchFilters.FirstOrDefault(f => f.Property == "DocVerify.VerifiedBy");
                    var verifiedDateFrom = mainSearchFilters.FirstOrDefault(f => f.Property == "DocVerify.VerifiedDateFrom");
                    var verifiedDateTo = mainSearchFilters.FirstOrDefault(f => f.Property == "DocVerify.VerifiedDateTo");

                    string idType = "";
                    int idValue = 0;
                    if (actionType != null)
                    {
                        var actionTypeArray = actionType.Value.Split("|");
                        idType = actionTypeArray[0].ToString().ToLower();
                        idValue = int.Parse(actionTypeArray[1]);
                    }

                    applications = applications.Where(a => a.ActionDues.Any(ad => (verifiedBy == null || EF.Functions.Like(ad.VerifiedBy, verifiedBy.Value))
                                                        && (verifiedDateFrom == null || ad.DateVerified >= Convert.ToDateTime(verifiedDateFrom.Value))
                                                        && (verifiedDateTo == null || ad.DateVerified <= Convert.ToDateTime(verifiedDateTo.Value))));

                    if (!string.IsNullOrEmpty(idType) && idValue > 0)
                    {
                        if (settings.IsSharePointIntegrationOn && settings.IsSharePointListRealTime)
                        {
                            var graphClient = _sharePointService.GetGraphClient();
                            var docs = new List<SharePointGraphDocPicklistViewModel>();

                            if (settings.IsSharePointIntegrationByMetadataOn)
                                docs = graphClient.GetSiteDocumentNamesByMetadata(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, SharePointDocLibrary.Patent, SharePointDocLibraryFolder.Application, "").GetAwaiter().GetResult();
                            else
                                docs = graphClient.GetSiteDocumentNames(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, SharePointDocLibrary.Patent, new List<string> { SharePointDocLibraryFolder.Application }, "").GetAwaiter().GetResult();

                            if (docs.Count > 0)
                            {
                                docs.ForEach(d =>
                                {
                                    var caseNumber = "";
                                    var country = "";
                                    var subCase = "";

                                    if (settings.IsSharePointIntegrationByMetadataOn)
                                    {
                                        var recKeys = d.RecKey.Split(SharePointSeparator.Field);
                                        caseNumber = recKeys[0];
                                        country = recKeys[1];
                                        if (recKeys.Length > 2)
                                        {
                                            subCase = recKeys[2];
                                        }
                                    }
                                    else
                                    {
                                        var folders = d.Folder.Split("/");
                                        caseNumber = folders[1];
                                        var countrySubCase = folders[2].Split(SharePointSeparator.Field);
                                        country = countrySubCase[0];
                                        if (countrySubCase.Length > 1)
                                        {
                                            subCase = countrySubCase[1];
                                        }
                                    }

                                    var application = _applicationService.CountryApplications.Where(a => a.CaseNumber == caseNumber && a.Country == country && a.SubCase == subCase).FirstOrDefaultAsync().GetAwaiter().GetResult();
                                    if (application != null)
                                        d.ParentId = application.AppId;
                                });
                                var docIds = docs.Where(d => d.ParentId > 0).Select(d => new { DriveItemId = d.Id, AppId = d.ParentId }).ToList();
                                var verifications = _docService.DocVerifications.Where(dv => docIds.Any(d => d.DriveItemId == dv.DocDocument.DocFile.DriveItemId)
                                                                && ((idType == "actid" && dv.ActId == idValue) || (idType == "actiontypeid" && dv.ActionTypeID == idValue))
                                                            )
                                                            .Select(d => d.DocDocument.DocFile.DriveItemId).ToListAsync().GetAwaiter().GetResult();

                                var appIds = docIds.Where(d => verifications.Contains(d.DriveItemId)).Select(d => d.AppId).ToList();

                                applications = applications.Where(a => appIds.Contains(a.AppId));
                            }
                            else
                            {
                                applications = applications.Where(a => false);
                            }
                        }
                        else
                        {
                            applications = applications.Where(a =>
                                                    _docService.DocDocuments.Any(d => d.DocFolder != null
                                                        && (d.DocFolder.SystemType ?? "").ToLower() == SystemTypeCode.Patent.ToLower()
                                                        && (d.DocFolder.ScreenCode ?? "").ToLower() == ScreenCode.Application.ToLower()
                                                        && (d.DocFolder.DataKey ?? "").ToLower() == "appid"
                                                        && d.DocFolder.DataKeyValue == a.AppId
                                                        && d.DocVerifications != null
                                                        && d.DocVerifications.Any(dv => (idType == "actid" && dv.ActId == idValue) || (idType == "actiontypeid" && dv.ActionTypeID == idValue))
                                                    )
                                                );
                        }
                    }

                    mainSearchFilters.Remove(actionType);
                    mainSearchFilters.Remove(verifiedBy);
                    mainSearchFilters.Remove(verifiedDateFrom);
                    mainSearchFilters.Remove(verifiedDateTo);
                }

                var noofOwners = mainSearchFilters.FirstOrDefault(f => f.Property == "NoOfOwners");
                if (noofOwners != null)
                {
                    if (noofOwners.Value == "s")
                    {
                        applications = applications.Where(a => a.Owners != null && a.Owners.Count == 1);
                    }
                    else
                    {
                        applications = applications.Where(a => a.Owners != null && a.Owners.Count > 1);
                    }
                    mainSearchFilters.Remove(noofOwners);
                }

                //number search
                var appNumber = mainSearchFilters.FirstOrDefault(f => f.Property == "AppNumber");
                if (appNumber != null)
                {
                    var appNumberSearch = QueryHelper.ExtractSignificantNumbers(appNumber.Value);
                    applications = applications.Where(a => (EF.Functions.Like(a.AppNumber, appNumber.Value) || EF.Functions.Like(a.AppNumberSearch, appNumberSearch)));
                    mainSearchFilters.Remove(appNumber);
                }

                var pubNumber = mainSearchFilters.FirstOrDefault(f => f.Property == "PubNumber");
                if (pubNumber != null)
                {
                    var pubNumberSearch = QueryHelper.ExtractSignificantNumbers(pubNumber.Value);
                    applications = applications.Where(a => (EF.Functions.Like(a.PubNumber, pubNumber.Value) || EF.Functions.Like(a.PubNumberSearch, pubNumberSearch)));
                    mainSearchFilters.Remove(pubNumber);
                }

                var patNumber = mainSearchFilters.FirstOrDefault(f => f.Property == "PatNumber");
                if (patNumber != null)
                {
                    var patNumberSearch = QueryHelper.ExtractSignificantNumbers(patNumber.Value);
                    applications = applications.Where(a => (EF.Functions.Like(a.PatNumber, patNumber.Value) || EF.Functions.Like(a.PatNumberSearch, patNumberSearch)));
                    mainSearchFilters.Remove(patNumber);
                }

                var parentAppNumber = mainSearchFilters.FirstOrDefault(f => f.Property == "ParentAppNumber");
                if (parentAppNumber != null)
                {
                    var parentAppNumberSearch = QueryHelper.ExtractSignificantNumbers(parentAppNumber.Value);
                    applications = applications.Where(a => (EF.Functions.Like(a.ParentAppNumber, parentAppNumber.Value) || EF.Functions.Like(a.ParentAppNumberSearch, parentAppNumberSearch)));
                    mainSearchFilters.Remove(parentAppNumber);
                }

                var parentPatNumber = mainSearchFilters.FirstOrDefault(f => f.Property == "ParentPatNumber");
                if (parentPatNumber != null)
                {
                    var parentPatNumberSearch = QueryHelper.ExtractSignificantNumbers(parentPatNumber.Value);
                    applications = applications.Where(a => (EF.Functions.Like(a.ParentPatNumber, parentPatNumber.Value) || EF.Functions.Like(a.ParentPatNumberSearch, parentPatNumberSearch)));
                    mainSearchFilters.Remove(parentPatNumber);
                }

                var pctNumber = mainSearchFilters.FirstOrDefault(f => f.Property == "PCTNumber");
                if (pctNumber != null)
                {
                    var pctNumberSearch = QueryHelper.ExtractSignificantNumbers(pctNumber.Value);
                    applications = applications.Where(a => (EF.Functions.Like(a.PCTNumber, pctNumber.Value) || EF.Functions.Like(a.PCTNumberSearch, pctNumberSearch)));
                    mainSearchFilters.Remove(pctNumber);
                }

                if (mainSearchFilters.Any())
                {
                    applications = QueryHelper.BuildCriteria<CountryApplication>(applications, mainSearchFilters);
                }
            }
            return applications;
        }

        public async Task<CPiDataSourceResult> CreateViewModelForGrid(DataSourceRequest request, IQueryable<CountryApplication> applications)
        {
            IQueryable<CountryApplicationSearchResultViewModel> model;

            var settings = await _settings.GetSetting();

            if (settings.IsSharePointIntegrationOn)
                model = applications.ProjectTo<CountryApplicationSearchResultSharePointViewModel>();
            else
                model = applications.ProjectTo<CountryApplicationSearchResultViewModel>();

            if (request.Sorts != null && request.Sorts.Any())
                model = model.ApplySorting(request.Sorts);
            else
                model = model.OrderBy(app => app.CaseNumber).ThenBy(app => app.Country).ThenBy(app => app.SubCase);


            var data = await model.ApplyPaging(request.Page, request.PageSize).ToListAsync();

            if (!settings.IsSharePointIntegrationOn)
            {
                data.ForEach(ca =>
                {
                    if (!string.IsNullOrEmpty(ca.ImageFile))
                    {
                        var cols = ca.ImageFile.Split("~");
                        ca.ImageFile = cols[0];
                        ca.ThumbnailFile = cols[1];
                        ca.ImageScreenCode = cols[2];
                        ca.ImageParentId = Convert.ToInt32(cols[3]);
                    }
                });
            }

            // check trade secret
            if (data.Any(ca => ca.IsTradeSecret ?? false))
            {
                var tsInvIds = data.Where(ca => ca.IsTradeSecret ?? false).Select(ca => ca.InvId).ToList();
                var tsInventions = await _inventionService.Inventions.Include(i => i.TradeSecretRequests).Where(i => (i.IsTradeSecret ?? false) && tsInvIds.Contains(i.InvId)).ToListAsync();
                foreach (var item in data.Where(ca => (ca.IsTradeSecret ?? false)))
                {
                    var tsInvention = tsInventions.SingleOrDefault(ts => ts.InvId ==  item.InvId);
                    var tsRequest = tsInvention?.TradeSecretRequests?.Where(ts => ts.UserId == _user.GetUserIdentifier()).OrderByDescending(ts => ts.RequestDate).FirstOrDefault();
                    if (tsRequest != null && tsRequest.IsCleared && item.TradeSecret != null)
                    {
                        // show trade secret if last request is cleared
                        item.RestoreTradeSecret(item.TradeSecret, true);
                        await _tradeSecretService.LogActivity(TradeSecretScreen.CountryApplicationSearch, TradeSecretScreen.CountryApplication, item.AppId, TradeSecretActivityCode.View, tsRequest.RequestId);
                    }
                    else
                    {
                        // hide image if not cleared
                        item.ImageFile = null;
                        item.ThumbnailFile = null;

                        if (item is CountryApplicationSearchResultSharePointViewModel)
                        {
                            var spItem = (CountryApplicationSearchResultSharePointViewModel)item;
                            spItem.SharePointRecKey = null;
                            spItem.ThumbnailUrl = null;
                        }

                        // log redacted view
                        await _tradeSecretService.LogActivity(TradeSecretScreen.CountryApplicationSearch, TradeSecretScreen.CountryApplication, item.AppId, TradeSecretActivityCode.RedactedView, 0);
                    }
                }
            }


            var ids = await model.Select(app => app.AppId).ToArrayAsync();

            return new CPiDataSourceResult()
            {
                Data = data,
                Total = ids.Length,
                Ids = ids
            };
        }


        //public async Task<CountryApplicationDTO> CreateViewModelForDetailScreen(int id)
        //{
        //    var vm = await _applicationService.GetDetails(id);
        //    if (vm?.ImageFile != null)
        //    {
        //        vm.ImageFile = ImageHelper.GetImageUrlPath("Patent") + vm.ImageFile;
        //    }

        //    return vm;
        //}
        public async Task<CountryApplicationDetailViewModel> CreateViewModelForDetailScreen(int id)
        {
            var viewModel = new CountryApplicationDetailViewModel();

            viewModel.Priority = new CountryApplicationPriorityViewModel();
            var setting = await _settings.GetSetting();

            if (id > 0)
            {
                viewModel = await _applicationService.CountryApplications.ProjectTo<CountryApplicationDetailViewModel>()
                    .SingleOrDefaultAsync(ca => ca.AppId == id);

                if (viewModel == null)
                    return viewModel;


                viewModel.LabelTaxSchedule = (await _applicationService.GetTaxScheduleLabel(viewModel.Country, viewModel.CaseType)) ?? "Tax Schedule";

                var showTaxSheduleAndClaim = setting.CountriesWithTaxSchedAndClaimField.Contains(viewModel.Country);
                viewModel.ShowClaimField = showTaxSheduleAndClaim || viewModel.LabelTaxSchedule.ToLower().StartsWith("claim");
                viewModel.ShowTaxScheduleField = showTaxSheduleAndClaim || !viewModel.LabelTaxSchedule.ToLower().StartsWith("claim");

                viewModel.ShowNationalField = setting.CountriesWithNationalField.Contains(viewModel.Country);
                viewModel.ShowConfirmationField = setting.CountriesWithConfirmationField.Contains(viewModel.Country);

                if (setting.IsTerminalDisclaimerOn)
                {
                    viewModel.TerminalDisclaimerAppId = await _applicationService.GetActiveTerminalDisclaimerAppId(id);
                }

            }
            viewModel.ShowBillingNumberField = setting.IsBillingNoOn;
            viewModel.IsOwnerRequired = _applicationService.IsOwnerRequired;
            viewModel.IsInventorRequired = _applicationService.IsInventorRequired;

            return viewModel;
        }

        public async Task<List<InventionCountryApplicationViewModel>> GetInventionCountryApplications(int invId)
        {
            var setting = await _settings.GetSetting();
            if (setting.IsFamilyNumberOn)
            {
                var familyNumber = (await _inventionService.GetByIdAsync(invId))?.FamilyNumber;
                return await _applicationService.CountryApplications
                       .Where(c => c.InvId == invId || (!string.IsNullOrEmpty(familyNumber) && c.Invention.FamilyNumber == familyNumber))
                       //  .OrderBy(c => c.CaseNumber).ThenBy(c => c.Country).ThenBy(c => c.SubCase)
                       .ProjectTo<InventionCountryApplicationViewModel>().ToListAsync();
            }
            else
            {
                return await _applicationService.CountryApplications
                       .Where(c => c.InvId == invId)
                       //.OrderBy(c => c.CaseNumber).ThenBy(c => c.Country).ThenBy(c => c.SubCase)
                       .ProjectTo<InventionCountryApplicationViewModel>().ToListAsync();
            }
        }

        public async Task<List<PatIDSMassCopyFamilyDTO>> GetRelatedApplications(int invId, int appId, string relatedBy, bool activeOnly)
        {
            switch (relatedBy)
            {
                case RelatedBy.CaseNumber:
                    return await _applicationService.CountryApplications
                       .Where(c => c.InvId == invId && c.AppId != appId && ((activeOnly && c.PatApplicationStatus.ActiveSwitch == activeOnly) || !activeOnly)).OrderBy(c => c.CaseNumber).ThenBy(c => c.Country).ThenBy(c => c.SubCase)
                       .ProjectTo<PatIDSMassCopyFamilyDTO>().ToListAsync();

                case RelatedBy.FamilyNumber:
                    var familyNumber = (await _inventionService.GetByIdAsync(invId))?.FamilyNumber;
                    return await _applicationService.CountryApplications
                           .Where(c => c.AppId != appId && ((activeOnly && c.PatApplicationStatus.ActiveSwitch == activeOnly) || !activeOnly) && (c.InvId == invId || (!string.IsNullOrEmpty(familyNumber) && c.Invention.FamilyNumber == familyNumber)))
                           .OrderBy(c => c.CaseNumber).ThenBy(c => c.Country).ThenBy(c => c.SubCase)
                           .ProjectTo<PatIDSMassCopyFamilyDTO>().ToListAsync();

                case RelatedBy.Keyword:
                    return await _applicationService.CountryApplications
                           .Where(c => c.AppId != appId && ((activeOnly && c.PatApplicationStatus.ActiveSwitch == activeOnly) || !activeOnly) && c.Invention.Keywords.Any(rkwd => _repository.PatKeywords.Any(kwd => kwd.InvId == invId && kwd.Keyword == rkwd.Keyword)))
                           .OrderBy(c => c.CaseNumber).ThenBy(c => c.Country).ThenBy(c => c.SubCase)
                           .ProjectTo<PatIDSMassCopyFamilyDTO>().ToListAsync();

                case RelatedBy.RelatedCase:
                    return await _applicationService.CountryApplications
                           .Where(c => c.AppId != appId && ((activeOnly && c.PatApplicationStatus.ActiveSwitch == activeOnly) || !activeOnly) && _repository.PatRelatedCases.Any(rc => rc.AppId == appId && rc.RelatedAppId == c.AppId))
                           .OrderBy(c => c.CaseNumber).ThenBy(c => c.Country).ThenBy(c => c.SubCase)
                           .ProjectTo<PatIDSMassCopyFamilyDTO>().ToListAsync();
            }
            return null;
        }

        public IQueryable<CountryApplication> GetFamilyReferenceList()
        {
            return _applicationService.CountryApplications;
        }

        public IQueryable<CaseNumberLookupViewModel> GetCaseNumbersList(IQueryable<CountryApplication> countryApplications, DataSourceRequest request, string textProperty, string text, FilterType filterType)
        {
            if (request.Filters?.Count > 0)
            {
                text = ((FilterDescriptor)request.Filters[0]).Value as string;
            }

            countryApplications = QueryHelper.BuildCriteria(countryApplications, textProperty, text, filterType);
            var result = countryApplications.Select(a => new CaseNumberLookupViewModel { Id = a.AppId, CaseNumber = a.CaseNumber }).OrderBy(a => a.CaseNumber);
            return result;
        }

        public async Task<CaseNumberLookupViewModel> CaseNumberSearchValueMapper(IQueryable<CountryApplication> countryApplications, string value)
        {
            var result = await countryApplications.Where(i => i.CaseNumber == value)
                .Select(a => new CaseNumberLookupViewModel { Id = a.InvId, CaseNumber = a.CaseNumber }).FirstOrDefaultAsync();
            return result;
        }

        #region Patent Score
        public async Task<double> GetPatentScore(int appId)
        {
            var average = await _repository.PatAverageScoreDTO.FirstOrDefaultAsync(av => av.AppId == appId);
            if (average != null)
                return average.AverageScore;
            return 0;
        }
        public async Task<List<PatScoreDTO>> GetPatentScores(int appId)
        {
            var result = await _repository.PatScoreDTO.Where(s => s.AppId == appId).ToListAsync();
            return result;
        }

        //public async Task<List<PatScore>> GetPatentScores(int appId)
        //{
        //    var result = await _repository.PatScores.Where(s => s.AppId == appId).Include(c=> c.ScoreCategory).ToListAsync();
        //    return result;
        //}

        public async Task<List<PatScoreCategory>> GetPatentScoreCategories()
        {
            return await _repository.PatScoreCategories.OrderBy(c => c.Category).ToListAsync();
        }
        #endregion


        #region Family Tree View
        public async Task<FamilyTreeDiagram> GetFamilyTreeDiagram(string paramType, string paramValue)
        {
            var setting = await _settings.GetSetting();

            var inventions = _inventionService.QueryableList.ProjectTo<InventionDetailViewModel>();
            var applications = _applicationService.CountryApplications.ProjectTo<CountryApplicationFamilyTreeViewModel>();
            var desCaseTypes = _repository.PatDesCaseTypes.Select(dc => dc.DesCaseType).Distinct();

            FamilyTreeDiagram graph = new FamilyTreeDiagram();
            graph.Header.LabelCaseNumber = setting.IsClientMatterOn ? setting.LabelClientMatter : setting.LabelCaseNumber;
            graph.Header.LabelTitle = "Title";
            graph.Header.LabelClient = setting.LabelClient;
            //graph.Area = FamilyTreeDiagramArea.Regular;

            if (paramType == "F") // Family level node
            {
                graph.Nodes.Add(new FamilyTreeDiagramDTO
                {
                    Text = paramValue,
                    Type = "F",
                    Client = "",
                    Title = "",
                });
                graph.Header.CaseNumber = paramValue;
                GetFamilyTreeDiagramFamilyChildren(graph, inventions, applications, desCaseTypes);
            }
            else
            {
                InventionDetailViewModel? invention = null;
                if (paramType == "I")
                {
                    int invId;
                    if (int.TryParse(paramValue, out invId))
                        invention = inventions.FirstOrDefault(c => c.InvId == invId);

                    if (invention == null)
                        invention = inventions.First(c => c.CaseNumber == paramValue);
                }
                else    // paramType == "C"
                {
                    var application = applications.First(c => c.AppId == int.Parse(paramValue));
                    invention = inventions.First(c => c.CaseNumber == application.CaseNumber);
                }

                if (!string.IsNullOrEmpty(invention.FamilyNumber))  // Family level node
                {
                    graph.Nodes.Add(new FamilyTreeDiagramDTO
                    {
                        Text = invention.FamilyNumber,  // Family Number
                        Type = "F",
                        Client = "",
                        Title = "",
                    });
                    graph.Header.CaseNumber = invention.FamilyNumber;
                    GetFamilyTreeDiagramFamilyChildren(graph, inventions, applications, desCaseTypes);
                }
                else    // Invention level node
                {
                    graph.Nodes.Add(new FamilyTreeDiagramDTO
                    {
                        Text = invention.CaseNumber + "/" + invention.DisclosureStatus,
                        Type = "I",
                        Client = invention.ClientName ?? "",
                        Title = invention.InvTitle ?? "",
                        Id = invention.InvId.ToString() + "-I",
                        KeyId = invention.InvId,
                        CaseNumber = invention.CaseNumber,
                    });

                    graph.Header.CaseNumber = invention.CaseNumber;
                    graph.Header.Title.Add(invention.InvTitle ?? "");
                    graph.Header.Client.Add(invention.ClientName ?? "");

                    var isTradeSecret = invention.IsTradeSecret ?? false;
                    if (isTradeSecret)
                    {
                        graph.Area = FamilyTreeDiagramArea.TradeSecret;
                        return graph;
                    }

                    GetFamilyTreeDiagramInventionChildren(graph, applications, desCaseTypes);
                }
            }
            return graph;
        }


        /// <summary>
        /// Retrieve nodes from Family level to Invnetion level
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="invs"></param>
        /// <param name="apps"></param>
        /// <param name="desCaseTypes"></param>
        private void GetFamilyTreeDiagramFamilyChildren(FamilyTreeDiagram graph, IQueryable<InventionDetailViewModel> invs,
            IQueryable<CountryApplicationFamilyTreeViewModel> apps, IQueryable<string?> desCaseTypes)
        {
            var familyLevelNode = graph.Nodes.Last();
            var inventions = invs.Where(c => c.FamilyNumber == familyLevelNode.Text);
            foreach (var inv in inventions)
            {
                FamilyTreeDiagramDTO inventionLevelNode = new FamilyTreeDiagramDTO()
                {
                    Text = inv.CaseNumber + "/" + inv.DisclosureStatus,
                    Type = "I",
                    Client = inv.ClientName ?? "",
                    Title = inv.InvTitle ?? "",
                    Id = inv.InvId.ToString() + "-I",
                    KeyId = inv.InvId,
                    CaseNumber = inv.CaseNumber,
                };

                var isTradeSecret = inv.IsTradeSecret ?? false;
                if (isTradeSecret)
                {
                    graph.Area = FamilyTreeDiagramArea.TradeSecret;
                    return;
                }

                graph.Nodes.Add(inventionLevelNode);

                var hasChildren = GetFamilyTreeDiagramInventionChildren(graph, apps, desCaseTypes);
                if (hasChildren)
                {
                    graph.Header.Title.Add(inv.InvTitle ?? "");
                    graph.Header.Client.Add(inv.ClientName ?? "");
                    graph.Edges.Add(new FamilyTreeDiagramEdge { StartId = familyLevelNode.Id, EndId = inventionLevelNode.Id });
                }
                else
                {
                    graph.Nodes.Remove(inventionLevelNode);
                }
            }
        }

        /// <summary>
        /// Retrieve nodes from Invention level to application level
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="apps"></param>
        /// <param name="desCaseTypes"></param>
        /// <returns></returns>
        private bool GetFamilyTreeDiagramInventionChildren(FamilyTreeDiagram graph, IQueryable<CountryApplicationFamilyTreeViewModel> apps, IQueryable<string?> desCaseTypes)
        {
            var inventionLevelNode = graph.Nodes.Last();
            var applications = apps.Where(c => c.CaseNumber == inventionLevelNode.CaseNumber && (c.ParentAppId == null || c.ParentAppId == 0)
                && (c.ParentAppNumber == null || c.ParentAppNumber == "")
                );
            if (!applications.Any())
                return false;

            var priority = new CountryApplicationPriorityViewModel();

            foreach (var ca in applications)
            {
                FamilyTreeDiagramDTO appLevelNode = CreateFamilyTreeDiagramDTO(ca);

                if (desCaseTypes.Contains(ca.CaseType))
                    graph.Stats.ValidatedApps += 1;

                if (ca.ApplicationStatus == "Expired")
                    graph.Stats.ExpiredPatents += 1;

                if (ca.IsActive)
                    graph.Stats.ActiveCount += 1;
                else
                    graph.Stats.InactiveCount += 1;

                if ((ca.Priority?.PriorityDate ?? DateTime.Today) < (priority?.PriorityDate ?? DateTime.Today))
                    priority = ca.Priority;

                graph.Nodes.Add(appLevelNode);
                graph.Edges.Add(new FamilyTreeDiagramEdge { StartId = inventionLevelNode.Id, EndId = appLevelNode.Id, Label = ca.CaseType });

                GetFamilyTreeDiagramApplicationChildren(graph, apps, desCaseTypes, priority, 0);
            }

            graph.Stats.PriNumber.Add(priority.PriorityNumber);
            graph.Stats.PriCountry.Add(priority.PriorityCountry);
            graph.Stats.PriDate.Add(priority.PriorityDate);

            return true;
        }


        /// <summary>
        /// Retrieve nodes from Applicaiton level to child applications
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="apps"></param>
        /// <param name="desCaseTypes"></param>
        /// <param name="index"></param>
        /// <param name="priority"></param>
        private void GetFamilyTreeDiagramApplicationChildren(FamilyTreeDiagram graph, IQueryable<CountryApplicationFamilyTreeViewModel> apps,
            IQueryable<string?> desCaseTypes, CountryApplicationPriorityViewModel? priority, int index)
        {
            var appLevelNode = graph.Nodes.Last();
            if (index >= 10)
                return;
            index++; // to avoid infinite loop
            int.TryParse(appLevelNode.Id.Split('-')[0], out int appLevelId);

            var applications = apps.Where(c => (c.ParentAppId != null && c.ParentAppId == appLevelId) || (c.ParentAppNumber != null && c.ParentAppNumber == appLevelNode.AppNumber));
            foreach (var ca in applications)
            {
                FamilyTreeDiagramDTO appChildNode = CreateFamilyTreeDiagramDTO(ca);

                if (desCaseTypes.Contains(ca.CaseType))
                    graph.Stats.ValidatedApps += 1;

                if (ca.ApplicationStatus == "Expired")
                    graph.Stats.ExpiredPatents += 1;

                if (ca.IsActive)
                    graph.Stats.ActiveCount += 1;
                else
                    graph.Stats.InactiveCount += 1;

                if ((ca.Priority?.PriorityDate ?? DateTime.Today) < (priority?.PriorityDate ?? DateTime.Today))
                    priority = ca.Priority;

                graph.Nodes.Add(appChildNode);
                graph.Edges.Add(new FamilyTreeDiagramEdge { StartId = appLevelNode.Id, EndId = appChildNode.Id, Label = ca.CaseType });
                GetFamilyTreeDiagramApplicationChildren(graph, apps, desCaseTypes, priority, index);
            }
        }


        private FamilyTreeDiagramDTO CreateFamilyTreeDiagramDTO(CountryApplicationFamilyTreeViewModel ca)
        {
            return new FamilyTreeDiagramDTO()
            {
                Text = ca.CaseNumber + "/" + ca.Country + (string.IsNullOrEmpty(ca.SubCase) ? "" : "-" + ca.SubCase) + "/" + ca.ApplicationStatus,
                CaseType = ca.CaseTypeDescription,
                AppNumber = ca.AppNumber,
                PatNumber = ca.PatNumber,
                PubNumber = ca.PubNumber,
                IssDate = ca.IssDate,
                PubDate = ca.PubDate,
                FilDate = ca.FilDate,
                Type = "C",
                Client = ca.ClientName,
                Title = ca.AppTitle ?? "",
                Id = ca.AppId.ToString() + "-C",
                KeyId = ca.AppId,
                PatentTermAdj = ca.PatentTermAdj,
                TerminalDisclaimer = ca.TerminalDisclaimer ?? false,
                ExpDate = ca.ExpDate,
                ParentPatNumber = ca.ParentPatNumber,
                ParentIssDate = ca.ParentIssDate,
                ParentAppNumber = ca.ParentAppNumber,
                ParentFilDate = ca.ParentFilDate,
                PCTDate = ca.PCTDate,
                PCTNumber = ca.PCTNumber,
                Active = ca.IsActive,

                Owners = string.Join(", ", _applicationService.QueryableChildList<PatOwnerApp>()
                        .Where(o => o.AppId == ca.AppId)
                        .OrderBy(o => o.OrderOfEntry)
                        .Select(m => m.Owner.OwnerName)),

                Inventors = string.Join(", ", _applicationService.QueryableChildList<PatInventorApp>()
                        .Where(o => o.AppId == ca.AppId)
                        .OrderBy(o => o.OrderOfEntry)
                        .Select(m => m.InventorAppInventor.Inventor)),
            };
        }




        public async Task<FamilyTreeDiagram> GetTerminalDisclaimerDiagram(int appId)
        {
            var setting = await _settings.GetSetting();
            var currentApp = _applicationService.CountryApplications.FirstOrDefault(d => d.AppId == appId);

            var graph = new FamilyTreeDiagram
            {
                Area = FamilyTreeDiagramArea.TerminalDisclaimer,
                Header = new FamilyTreeDiagramHeader()
                {
                    LabelCaseNumber = setting.IsClientMatterOn ? setting.LabelClientMatter : setting.LabelCaseNumber,
                    LabelTitle = "Title",
                    LabelExpDate = "Expiration Date",
                    CaseNumber = currentApp?.CaseNumber,
                    Title = [currentApp?.AppTitle],
                    ExpDate = currentApp?.ExpDate,
                }
            };
            if (currentApp == null) return graph;

            var currentNode = AddTerminalDisclaimerFamilyTreeDiagramDTO(graph, currentApp, "TDC");

            // Add TD applications pointing to current application
            var tdApplications = _applicationService.CountryApplications
                .Where(ca => ca.PatChildTerminalDisclaimers.Any(td => td.AppId == appId));

            foreach (var tdApp in tdApplications)
            {
                bool isWithinFamily = tdApp.CaseNumber == currentApp.CaseNumber;
                var tdNode = AddTerminalDisclaimerFamilyTreeDiagramDTO(graph, tdApp, "TD", isWithinFamily);

                AddEdgeIfNotExists(graph, tdNode.Id, currentNode.Id);
                ResolveDateConflict(tdNode, currentNode);

                if (isWithinFamily)
                {
                    var tdcApplications = _applicationService.CountryApplications
                        .Where(ca => ca.PatTerminalDisclaimers.Any(td => td.TerminalDisclaimerAppId == tdApp.AppId));

                    foreach (var tdcApp in tdcApplications)
                    {
                        if (tdcApp.AppId == appId) continue;

                        var tdcNode = AddTerminalDisclaimerFamilyTreeDiagramDTO(graph, tdcApp, "TDC");
                        AddEdgeIfNotExists(graph, tdNode.Id, tdcNode.Id);
                        ResolveDateConflict(tdNode, tdcNode);
                    }
                }
            }

            // Navigate up to find base application that is PRI/PRO
            var baseApp = FindRootApplication(currentApp);
            var hasStartingNode = false;

            if (baseApp != null)
            {
                var appLevelNode = graph.Nodes.FirstOrDefault(p => p.Id.Contains(baseApp.AppId.ToString()))
                    ?? AddTerminalDisclaimerFamilyTreeDiagramDTO(graph, baseApp, "", true);

                if (appLevelNode.Type == "TD")
                {
                    appLevelNode.IsStartingNode = true;
                    hasStartingNode = true;
                    var possibleAppNode = graph.Nodes.FirstOrDefault(p => p.Id == appLevelNode.Id.Replace("App", "Pat"));
                    if (possibleAppNode != null)
                        appLevelNode = possibleAppNode;
                }

                var apps = _applicationService.CountryApplications.ProjectTo<CountryApplicationFamilyTreeViewModel>();
                GetTerminalDisclaimerFamilyTreeDiagramApplicationChildren(graph, apps, 0, appLevelNode);
            }

            if (!hasStartingNode)
            {
                var fallbackNode = graph.Nodes.FirstOrDefault(p => p.Type == "TD" && p.Id.Contains("App"));
                if (fallbackNode != null)
                {
                    fallbackNode.IsStartingNode = true;
                    var newParentNodeId = fallbackNode.Id.Replace("App", "Pat");
                    var newParentNode = graph.Nodes.First(n => n.Id == newParentNodeId);
                    graph.Edges.ForEach(e =>
                    {
                        if (e.StartId == fallbackNode.Id && e.EndId != newParentNodeId)
                        {
                            e.StartId = newParentNodeId;
                            var tempNode = graph.Nodes.First(n => n.Id == e.EndId);
                            if (tempNode.BackwardPendingDate == null)
                                tempNode.BackwardPendingDate = newParentNode;
                            if (newParentNode.ForwardPendingDate == null)
                                newParentNode.ForwardPendingDate = tempNode;
                        }
                    });
                }
            }

            ApplyDefaultDate(graph);
            return graph;
        }



        private void ApplyDefaultDate(FamilyTreeDiagram graph)
        {
            var pendingNodes = graph.Nodes
                .Where(n => n.BackwardPendingDate != null || n.ForwardPendingDate != null)
                .ToList();

            bool oneDateExists = graph.Nodes.Any(n => n.Date != null);

            while (pendingNodes.Count > 0)
            {
                var node = pendingNodes[0];
                DateTime? childNodeDate = node.ForwardPendingDate?.Date;
                DateTime? parentNodeDate = node.BackwardPendingDate?.Date;
                DateTime? potentialDate = null;

                if (!oneDateExists)
                {
                    node.Date = DateTime.Today;
                    if (node.FilDate == null) node.FilDate = node.Date;
                    if (node.IssDate == null) node.IssDate = node.Date;

                    node.ForwardPendingDate = null;
                    node.BackwardPendingDate = null;
                    node.Modified = true;
                    oneDateExists = true;
                    pendingNodes.RemoveAt(0);
                    continue;
                }

                if (childNodeDate.HasValue && parentNodeDate.HasValue && !node.ForwardPendingDate.Id.Contains("PTA"))
                {
                    TimeSpan span = childNodeDate.Value - parentNodeDate.Value;
                    potentialDate = parentNodeDate.Value + TimeSpan.FromTicks(span.Ticks / 2);
                }
                else if (parentNodeDate.HasValue)
                {
                    potentialDate = parentNodeDate.Value.AddYears(1);
                }
                else if (childNodeDate.HasValue)
                {
                    potentialDate = childNodeDate.Value.AddYears(-1);
                }

                if (potentialDate.HasValue)
                {
                    node.Date = potentialDate;
                    if (node.FilDate == null)
                    {
                        node.FilDate = node.Date;
                        if (node.ForwardPendingDate != null)
                            node.ForwardPendingDate.FilDate = node.Date;

                        if (node.BackwardPendingDate != null)
                            node.BackwardPendingDate.FilDate = node.Date;
                    }

                    if (node.IssDate == null)
                    {
                        node.IssDate = node.Date;
                        if (node.ForwardPendingDate != null)
                            node.ForwardPendingDate.IssDate = node.Date;

                        if (node.BackwardPendingDate != null)
                            node.BackwardPendingDate.IssDate = node.Date;
                    }

                    node.Modified = true;
                    node.ForwardPendingDate = null;
                    node.BackwardPendingDate = null;
                    pendingNodes.RemoveAt(0);
                }
                else
                {
                    pendingNodes.RemoveAt(0);
                    pendingNodes.Add(node);
                }
            }
        }

        private void AddEdgeIfNotExists(FamilyTreeDiagram graph, string fromId, string toId)
        {
            if (!graph.Edges.Any(e => e.StartId == fromId && e.EndId == toId))
            {
                graph.Edges.Add(new FamilyTreeDiagramEdge
                {
                    StartId = fromId,
                    EndId = toId,
                    Label = ""
                });
            }
        }

        private CountryApplication? FindRootApplication(CountryApplication app)
        {
            while (app != null && app.ParentAppId != null && app.CaseType != "PRI" && app.CaseType != "PRO")
            {
                var parent = _applicationService.CountryApplications.FirstOrDefault(d => d.AppId == app.ParentAppId);
                if (parent == null || parent.Country != "US")
                    return null;
                app = parent;
            }
            return app;
        }


        private void GetTerminalDisclaimerFamilyTreeDiagramApplicationChildren(FamilyTreeDiagram graph, 
            IQueryable<CountryApplicationFamilyTreeViewModel> apps, int index, BaseFamilyTreeDiagramDTO parentNode)
        {
            if (index >= 10)
                return;
            index++; // to avoid infinite loop

            var parentNodeId = parentNode.Id.AsSpan(0, parentNode.Id.IndexOf('-')).ToString();

            var applications = apps.Where(c => (c.ParentAppId != null && c.ParentAppId.ToString() == parentNodeId) || (c.ParentAppNumber != null && c.ParentAppNumber == parentNode.AppNumber));

            foreach (var ca in applications)
            {
                if (ca.Country != "US") continue;

                BaseFamilyTreeDiagramDTO? childNode = graph.Nodes.FirstOrDefault(p => p.Id.Contains(ca.AppId.ToString()));
                if (childNode == null)
                {
                    childNode = AddTerminalDisclaimerFamilyTreeDiagramDTO(graph, ca, "");
                }
                else if (parentNode.IsStartingNode)
                {
                    var childNodePat = graph.Nodes.FirstOrDefault(p => p.Id == childNode.Id.Replace("App", "Pat"));
                    if (childNodePat != null)
                        childNode = childNodePat;
                }

                FamilyTreeDiagramEdge? childNodeEdge = graph.Edges.FirstOrDefault(
                    e => e.StartId.Contains(parentNodeId) && e.EndId.Contains(ca.AppId.ToString()));

                if (childNodeEdge == null)
                    graph.Edges.Add(new FamilyTreeDiagramEdge { StartId = parentNode.Id, EndId = childNode.Id, Label = ca.CaseType });
                else
                    childNodeEdge.Label = ca.CaseType;

                ResolveDateConflict(parentNode, childNode);
                GetTerminalDisclaimerFamilyTreeDiagramApplicationChildren(graph, apps, index, childNode);
            }
        }


        private void ResolveDateConflict(BaseFamilyTreeDiagramDTO parentNode, BaseFamilyTreeDiagramDTO childNode)
        {
            bool parentHasDate = parentNode.Date.HasValue;
            bool childHasDate = childNode.Date.HasValue;

            if (parentHasDate && childHasDate)
                return;

            if (!parentHasDate && childHasDate)
            {
                if (parentNode.ForwardPendingDate == null ||
                    parentNode.ForwardPendingDate.Date == null ||
                    parentNode.ForwardPendingDate.Date < childNode.Date)
                {
                    parentNode.ForwardPendingDate = childNode;
                }
                return;
            }
            
            if (parentHasDate && !childHasDate)
            {
                if (childNode.BackwardPendingDate == null ||
                    childNode.BackwardPendingDate.Date == null ||
                    childNode.BackwardPendingDate.Date < parentNode.Date)
                {
                    childNode.BackwardPendingDate = parentNode;
                }
                return;
            }

            if (parentNode.ForwardPendingDate == null)
                parentNode.ForwardPendingDate = childNode;

            if (childNode.BackwardPendingDate == null)
                childNode.BackwardPendingDate = parentNode;
        }



        private BaseFamilyTreeDiagramDTO AddTerminalDisclaimerFamilyTreeDiagramDTO(FamilyTreeDiagram graph, CountryApplicationDetail ca, string type = "", bool isStartingNode = false)
        {
            var appNode = new BaseFamilyTreeDiagramDTO()
            {
                Id = ca.AppId.ToString() + "-App",
                Text = (ca.AppNumber ?? "") + " Appl.",
                Type = type,
                IsStartingNode = isStartingNode,
                Title = ca.AppTitle ?? "",
                AppNumber = ca.AppNumber,
                CaseNumber = ca.CaseNumber,
                PatentTermAdj = ca.PatentTermAdj,
                TerminalDisclaimer = ca.TerminalDisclaimer ?? false,
                FilDate = ca.FilDate,
                IssDate = ca.IssDate,
                ExpDate = ca.ExpDate,
                Date = ca.FilDate,
            };
            graph.Nodes.Add(appNode);


            BaseFamilyTreeDiagramDTO patNode = new BaseFamilyTreeDiagramDTO()
            {
                Id = ca.AppId.ToString() + "-Pat",
                Text = (ca.PatNumber ?? "") + " Patent",
                Type = type,
                IsStartingNode = isStartingNode,
                Title = ca.AppTitle ?? "",
                AppNumber = ca.AppNumber,
                CaseNumber = ca.CaseNumber,
                PatentTermAdj = ca.PatentTermAdj,
                TerminalDisclaimer = ca.TerminalDisclaimer ?? false,
                ExpDate = ca.ExpDate,
                FilDate = ca.FilDate,
                IssDate = ca.IssDate,
                Date = ca.IssDate
            };

            if (ca.FilDate.HasValue && ca.IssDate.HasValue && ca.FilDate > ca.IssDate)
            {
                patNode.Date = ca.FilDate.Value.AddYears(1);
                patNode.Modified = true;
            }

            ResolveDateConflict(appNode, patNode);


            graph.Nodes.Add(patNode);
            graph.Edges.Add(new FamilyTreeDiagramEdge { StartId = appNode.Id, EndId = patNode.Id, Label = "" });

            if (type == "TDC")
            {
                var ptaNode = new BaseFamilyTreeDiagramDTO()
                {
                    Id = ca.AppId.ToString() + "-PTA",
                    Text = ca.PatentTermAdj != 0 ? ca.PatentTermAdj + " Days PTA" : "No PTA",
                    Type = type,
                    Title = ca.AppTitle ?? "",
                    AppNumber = ca.AppNumber,
                    CaseNumber = ca.CaseNumber,
                    PatentTermAdj = ca.PatentTermAdj,
                    TerminalDisclaimer = ca.TerminalDisclaimer ?? false,
                    ExpDate = ca.ExpDate,
                    FilDate = ca.FilDate,
                    IssDate = ca.IssDate,
                    Date = ca.ExpDate,
                };


                //if (ca.IssDate.HasValue && ca.ExpDate.HasValue && ca.IssDate > ca.ExpDate)
                //{
                //    ptaNode.Date = ca.IssDate.Value.AddYears(2);
                //    ptaNode.Modified = true;
                //}
                ResolveDateConflict(patNode, ptaNode);


                graph.Nodes.Add(ptaNode);
                graph.Edges.Add(new FamilyTreeDiagramEdge { StartId = patNode.Id, EndId = ptaNode.Id, Label = "" });
            }

            return isStartingNode ? patNode : appNode;

        }



        public async Task<IEnumerable<FamilyTreeDTO>> GetFamilyTree(string paramType, string paramValue, string paramParent)
        {
            return await _applicationService.GetFamilyTree(paramType, paramValue, paramParent);
        }

        public FamilyTreePatDTO GetNodeDetails(string paramType, string paramValue)
        {
            return _applicationService.GetNodeDetails(paramType, paramValue);
        }

        public void UpdateParent(int childAppId, int newParentId, string parentInfo, string userName)
        {
            _applicationService.UpdateParent(childAppId, newParentId, parentInfo, userName);
        }

        public async Task<List<SysCustomFieldSetting>> GetCustomFields()
        {
            return await _repository.SysCustomFieldSettings.Where(s => s.TableName == "tblPatCountryApplication" && s.Visible == true).OrderBy(s => s.OrderOfEntry).ToListAsync();
        }

        public async Task<List<SysCustomFieldSetting>> GetInventionCustomFieldsForSearch()
        {
            return await _repository.SysCustomFieldSettings.Where(s => s.TableName == "tblPatInvention" && s.Visible == true && s.CountryAppSearch == true).OrderBy(s => s.OrderOfEntry).ToListAsync();
        }


        //public string GetExpandedNodes(string paramType, string paramValue)
        //{
        //    return _applicationService.GetExpandedNodes(paramType, paramValue);
        //}

        #endregion

        public async Task<List<WorkflowEmailViewModel>> ProcessSaveWorkflow(CountryApplication app, bool checkStatusChangeWorkFlow, string? oldApplicationStatus, string? emailUrl, string? userName, string? actionEmailUrl,
                                                        bool checkDisclosureStatusChangeWorkFlow, string? oldDisclosureStatus, string? disclosureStatusChangeEmailUrl)
        {
            var workFlows = new List<WorkflowViewModel>();
            var inventorRenEmailWorkflows = new List<WorkflowEmailViewModel>();
            var disclosureStatusChangeEmailWorkflows = new List<WorkflowEmailViewModel>();
            var dateCreated = DateTime.Now;
            var setting = await _settings.GetSetting();

            if (checkStatusChangeWorkFlow)
            {
                var workflowActions = await _workflowViewModelService.GetCountryApplicationWorkflowActions(app, PatWorkflowTriggerType.StatusChanged, false);
                if (workflowActions.Any())
                {
                    var newApplicationStatus = app.PatApplicationStatus;
                    if (newApplicationStatus.ApplicationStatus != oldApplicationStatus)
                    {
                        var newApplicationStatusId = newApplicationStatus.StatusId;
                        var workFlowActions = workflowActions.Where(a => a.Workflow.TriggerValueId == newApplicationStatusId || a.Workflow.TriggerValueId == 0 || (a.Workflow.TriggerValueId == -1 && newApplicationStatus.ActiveSwitch) || (a.Workflow.TriggerValueId == -2 && !newApplicationStatus.ActiveSwitch)).ToList();
                        workFlowActions = _workflowViewModelService.ClearPatBaseWorkflowActions(workFlowActions);

                        foreach (var item in workFlowActions)
                        {
                            workFlows.Add(new WorkflowViewModel
                            {
                                ActionTypeId = item.ActionTypeId,
                                ActionValueId = item.ActionValueId,
                                Preview = item.Preview,
                                AutoAttachImages = item.IncludeAttachments,
                                EmailUrl = emailUrl,
                                Id = app.AppId,
                                AttachmentFilter = item.AttachmentFilter
                            });
                        }
                    }
                }
            }

            if (checkDisclosureStatusChangeWorkFlow)
            {
                var statusChangeWorkflowActions = await _workflowViewModelService.GetCountryApplicationWorkflowActions(app, PatWorkflowTriggerType.DisclosureStatusChanged, false);
                if (statusChangeWorkflowActions.Any())
                {
                    var newDisclosureStatus = await _disclosureStatusService.QueryableList.Where(st => st.DisclosureStatus == app.Invention.DisclosureStatus).FirstOrDefaultAsync();
                    var newDisclosureStatusId = newDisclosureStatus.DisclosureStatusID;
                    statusChangeWorkflowActions = statusChangeWorkflowActions.Where(a => a.Workflow.TriggerValueId == newDisclosureStatusId || a.Workflow.TriggerValueId == 0).ToList();
                    statusChangeWorkflowActions = _workflowViewModelService.ClearPatBaseWorkflowActions(statusChangeWorkflowActions);
                    if (statusChangeWorkflowActions.Any())
                    {
                        foreach (var wf in statusChangeWorkflowActions)
                        {
                            disclosureStatusChangeEmailWorkflows.Add(new WorkflowEmailViewModel
                            {
                                isAutoEmail = !wf.Preview,
                                qeSetupId = wf.ActionValueId,
                                autoAttachImages = wf.IncludeAttachments,
                                id = app.Invention.InvId,
                                fileNames = new string[] { },
                                emailUrl = disclosureStatusChangeEmailUrl,
                                attachmentFilter = wf.AttachmentFilter
                            });
                        }
                    }

                }
            }

            //inventor remuneration
            if (setting.IsInventorRemunerationOn)
            {
                if (app.ApplicationStatus.ToLower() == "granted" && oldApplicationStatus.ToLower() != app.ApplicationStatus.ToLower()
                    && await _applicationService.HasProducts(app.AppId))
                {

                    var workflowActions = await _workflowViewModelService.GetCountryApplicationWorkflowActions(app, PatWorkflowTriggerType.InventorRemuneration, true);
                    if (workflowActions.Any())
                    {
                        if (workflowActions.Any())
                        {
                            foreach (var wf in workflowActions)
                            {
                                inventorRenEmailWorkflows.Add(new WorkflowEmailViewModel
                                {
                                    isAutoEmail = !wf.Preview,
                                    qeSetupId = wf.ActionValueId,
                                    autoAttachImages = wf.IncludeAttachments,
                                    id = app.AppId,
                                    fileNames = new string[] { },
                                    emailUrl = emailUrl,
                                    attachmentFilter = wf.AttachmentFilter
                                });
                            }
                        }
                    }
                }
            }

            //country law actions (new actions)
            var workflowCLActionsMain = await _workflowViewModelService.GetCountryApplicationWorkflowActions(app, PatWorkflowTriggerType.NewAction, false);
            workflowCLActionsMain = workflowCLActionsMain.Where(w => w.Workflow.TriggerValueId <= 0).ToList();
            workflowCLActionsMain = _workflowViewModelService.ClearPatBaseWorkflowActions(workflowCLActionsMain);

            List<WorkflowViewModel> workflowCLActions = workflowCLActionsMain.Select(w =>
                    new WorkflowViewModel
                    {
                        TriggerValueId = Math.Abs(w.Workflow.TriggerValueId),
                        ActionTypeId = w.ActionTypeId,
                        ActionValueId = w.ActionValueId,
                        Preview = w.Preview,
                        AutoAttachImages = w.IncludeAttachments,
                        EmailUrl = emailUrl,
                        AttachmentFilter = w.AttachmentFilter
                    }).ToList();

            if (workflowCLActions.Any())
            {
                Expression<Func<PatCountryDue, bool>> predicate = (item) => false;
                foreach (var item in workflowCLActions)
                {
                    predicate = predicate.Or(cd => cd.CDueId == item.TriggerValueId);
                }

                var baseActionTypes = await _applicationService.PatCountryDues.Where(predicate).Select(cd => cd.ActionType).ToListAsync();
                var actionTypes = await _applicationService.PatCountryDues.Where(cd => baseActionTypes.Any(at => at == cd.ActionType)).ToListAsync();
                if (actionTypes.Any() || workflowCLActions.Any(wf => wf.TriggerValueId == 0))
                {
                    //dateCreated = dateCreated.AddTicks(-(dateCreated.Ticks % TimeSpan.TicksPerSecond)); //remove the ms
                    dateCreated = dateCreated.AddSeconds(-25); //remove 25 secs
                    var newCLActions = await _applicationService.QueryableChildList<PatActionDue>().Where(a => a.AppId == app.AppId && a.ComputerGenerated && a.CreatedBy == userName && a.DateCreated >= dateCreated).Include(ad => ad.CountryApplication).ToListAsync();

                    if (workflowCLActions.Any(wf => wf.TriggerValueId > 0))
                    {
                        newCLActions = newCLActions.Where(a => actionTypes.Any(at => at.ActionType == a.ActionType)).ToList();
                    }

                    //trigger is specific action
                    foreach (var item in newCLActions)
                    {
                        var actionType = actionTypes.FirstOrDefault(at => at.Country == item.Country && at.CaseType == item.CountryApplication.CaseType && at.ActionType == item.ActionType);
                        if (actionType != null)
                        {
                            var workFlowAction = workflowCLActions.FirstOrDefault(a => actionTypes.Any(at => at.ActionType == item.ActionType && at.CDueId == a.TriggerValueId));
                            if (workFlowAction.ActionValueId != 0)
                                workFlows.Add(new WorkflowViewModel
                                {
                                    ActionTypeId = workFlowAction.ActionTypeId,
                                    ActionValueId = workFlowAction.ActionValueId,
                                    Preview = workFlowAction.Preview,
                                    AutoAttachImages = workFlowAction.AutoAttachImages,
                                    EmailUrl = actionEmailUrl,
                                    Id = item.ActId,
                                    AttachmentFilter = workFlowAction.AttachmentFilter
                                });
                        }
                    }

                    //trigger is all actions
                    if (workflowCLActions.Any(wf => wf.TriggerValueId == 0))
                    {
                        var workFlowAction = workflowCLActions.FirstOrDefault(wf => wf.TriggerValueId == 0);
                        if (workFlowAction.ActionValueId != 0 && !workFlows.Any(w => w.ActionTypeId == workFlowAction.ActionTypeId && w.ActionValueId == workFlowAction.ActionValueId))
                        {
                            foreach (var item in newCLActions)
                            {
                                workFlows.Add(new WorkflowViewModel
                                {
                                    ActionTypeId = workFlowAction.ActionTypeId,
                                    ActionValueId = workFlowAction.ActionValueId,
                                    Preview = workFlowAction.Preview,
                                    AutoAttachImages = workFlowAction.AutoAttachImages,
                                    EmailUrl = actionEmailUrl,
                                    Id = item.ActId,
                                    AttachmentFilter = workFlowAction.AttachmentFilter
                                });
                            }

                        }
                    }
                }
            }

            _applicationService.DetachAllEntities();
            var createActionWorkflows = workFlows.Where(wf => wf.ActionTypeId == (int)PatWorkflowActionType.CreateAction).Distinct().ToList();
            foreach (var item in createActionWorkflows)
            {
                await _applicationService.GenerateWorkflowAction(app.AppId, item.ActionValueId, DateTime.Now);
            }

            var closeActionWorkflows = workFlows.Where(wf => wf.ActionTypeId == (int)PatWorkflowActionType.CloseAction).Distinct().ToList();
            foreach (var item in closeActionWorkflows)
            {
                var actionDues = await _applicationService.CloseWorkflowAction(app.AppId, item.ActionValueId);
                if (actionDues.Any())
                {
                    foreach (var actionDue in actionDues)
                    {
                        await _actionDueService.Update(actionDue);
                    }
                }
            }

            var emailWorkflows = workFlows.Where(wf => wf.ActionTypeId == (int)PatWorkflowActionType.SendEmail)
                 .Select(wf => new WorkflowEmailViewModel
                 {
                     isAutoEmail = !wf.Preview,
                     qeSetupId = wf.ActionValueId,
                     autoAttachImages = wf.AutoAttachImages,
                     id = (int)wf.Id,
                     fileNames = new string[] { },
                     emailUrl = wf.EmailUrl,
                     attachmentFilter = wf.AttachmentFilter
                 }).Distinct().ToList();

            if (inventorRenEmailWorkflows.Any())
                emailWorkflows.AddRange(inventorRenEmailWorkflows);
            if (disclosureStatusChangeEmailWorkflows.Any())
                emailWorkflows.AddRange(disclosureStatusChangeEmailWorkflows);

            return emailWorkflows;

        }

        private static class RelatedBy
        {
            public const string CaseNumber = "C";
            public const string FamilyNumber = "F";
            public const string Keyword = "K";
            public const string RelatedCase = "R";
        }

        public async Task ApplyDetailPageTradeSecretPermission(DetailPageViewModel<CountryApplicationDetailViewModel> viewModel)
        {
            viewModel.IsTradeSecret = viewModel.Detail.IsTradeSecret ?? false;
            viewModel.ShowTradeSecretRequest = false;

            if (_user.CanAccessPatTradeSecret())
            {
                viewModel.TradeSecretLocator = _tradeSecretService.CreateLocator(TradeSecretScreen.Invention, viewModel.Detail.InvId);
                if (viewModel.IsTradeSecret)
                    viewModel.TradeSecretUserRequest = await _tradeSecretService.GetUserRequest(viewModel.TradeSecretLocator);

                var isTSCleared = viewModel.TradeSecretUserRequest?.IsCleared ?? false;
                viewModel.ShowTradeSecretSwitch = viewModel.IsTradeSecret ? isTSCleared && _user.IsPatTradeSecretAdmin() : _user.CanAccessPatTradeSecret();
                viewModel.CanDeleteTradeSecret = isTSCleared && _user.IsPatTradeSecretAdmin();
                viewModel.CanEditTradeSecret = isTSCleared && _user.CanEditPatTradeSecretFields();

                if (viewModel.Detail.InvId > 0 && viewModel.IsTradeSecret && viewModel.Detail.TradeSecret != null && viewModel.TradeSecretUserRequest != null && viewModel.TradeSecretUserRequest.IsCleared)
                {
                    viewModel.Detail.RestoreTradeSecret(viewModel.Detail.TradeSecret, true);
                    await _tradeSecretService.LogActivity(TradeSecretScreen.CountryApplication, TradeSecretScreen.CountryApplication, viewModel.Detail.AppId, TradeSecretActivityCode.View, viewModel.TradeSecretUserRequest.RequestId);
                }
                else if (viewModel.IsTradeSecret)
                {
                    // log redacted view
                    await _tradeSecretService.LogActivity(TradeSecretScreen.CountryApplication, TradeSecretScreen.CountryApplication, viewModel.Detail.AppId, TradeSecretActivityCode.RedactedView, 0);
                }
            }

            viewModel.CanCopyRecord = !viewModel.IsTradeSecret;
        }
    }
}
