using AutoMapper;
using AutoMapper.QueryableExtensions;
using Kendo.Mvc;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.SharePoint.Client;
using R10.Core;
using R10.Core.DTOs;
using R10.Core.Entities.GeneralMatter;
using R10.Core.Entities.Patent;
using R10.Core.Entities.Trademark;
using R10.Core.Interfaces;
using R10.Core.Services.GeneralMatter;
using R10.Core.Services.Trademark;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Areas.Shared.ViewModels.SharePoint;
using R10.Web.Areas.Trademark.ViewModels;
using R10.Web.Extensions;
using R10.Web.Helpers;
using R10.Web.Interfaces;
using R10.Web.Services;
using R10.Web.Services.SharePoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace R10.Web.Areas.Trademark.Services
{
    public class TmkTrademarkViewModelService : ITmkTrademarkViewModelService
    {
        private readonly ITmkTrademarkService _trademarkService;
        private readonly IMapper _mapper;
        private readonly IDocumentService _docService;
        private readonly INotificationSettingManager _userSettingManager;
        private readonly ISystemSettings<TmkSetting> _settings;
        private readonly ISharePointService _sharePointService;
        private readonly GraphSettings _graphSettings;
        private readonly IWorkflowViewModelService _workflowViewModelService;
        private readonly IActionDueDeDocketService<TmkActionDue, TmkDueDate> _actionDueService;

        public TmkTrademarkViewModelService(
               ITmkTrademarkService trademarkService,
               IMapper mapper,
               IDocumentService docService,
               INotificationSettingManager userSettingManager, ISystemSettings<TmkSetting> settings,
               ISharePointService sharePointService, IOptions<GraphSettings> graphSettings,
               IWorkflowViewModelService workflowViewModelService,
               IActionDueDeDocketService<TmkActionDue, TmkDueDate> actionDueService
            )
        {
            _trademarkService = trademarkService;
            _mapper = mapper;
            _docService = docService;
            _userSettingManager = userSettingManager;
            _settings = settings;
            _sharePointService = sharePointService;
            _graphSettings = graphSettings.Value;
            _workflowViewModelService = workflowViewModelService;
            _actionDueService = actionDueService;
        }

        public IQueryable<TmkTrademark> AddCriteria(List<QueryFilterViewModel> mainSearchFilters, IQueryable<TmkTrademark> trademarks)
        {
            if (mainSearchFilters.Count > 0)
            {
                var settings = _settings.GetSetting().GetAwaiter().GetResult();

                var countryOp = mainSearchFilters.GetFilterOperator("CountryOp");
                var country = mainSearchFilters.FirstOrDefault(f => f.Property == "Country");
                var includeDesignatedCountries = mainSearchFilters.FirstOrDefault(f => f.Property == "IncludeDesignatedCountries");
                if (country != null)
                {
                    country.Operator = countryOp;
                    var countries = country.GetValueListForLoop();

                    if (countries.Count > 0)
                    {
                        if (country.Operator == "eq")
                        {
                            trademarks = trademarks.Where(m => countries.Contains(m.Country) ||
                            (includeDesignatedCountries != null && (m.DesignatedCountries.Any(d => countries.Contains(d.DesCountry)))));
                        }
                        else
                        {
                            trademarks = trademarks.Where(m => !countries.Contains(m.Country));
                        }
                        mainSearchFilters.Remove(country);
                    }
                }

                var countryName = mainSearchFilters.FirstOrDefault(f => f.Property == "MultiSelect_TmkCountry.CountryName");
                if (countryName != null)
                {
                    var countryNames = countryName.GetValueListForLoop();

                    trademarks = trademarks.Where(m => countryNames.Any(cn => EF.Functions.Like(m.TmkCountry.CountryName, cn) ||
                    (includeDesignatedCountries != null && (m.DesignatedCountries.Any(d => EF.Functions.Like(d.Country.CountryName, cn))))));
                    mainSearchFilters.Remove(countryName);
                }

                if (includeDesignatedCountries != null)
                {
                    mainSearchFilters.Remove(includeDesignatedCountries);
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
                            trademarks = trademarks.Where(m => caseTypes.Contains(m.CaseType));
                        else
                            trademarks = trademarks.Where(m => !caseTypes.Contains(m.CaseType));

                        mainSearchFilters.Remove(caseType);
                    }
                }

                var trademarkStatusOp = mainSearchFilters.GetFilterOperator("TrademarkStatusOp");
                var trademarkStatus = mainSearchFilters.FirstOrDefault(f => f.Property == "TrademarkStatus");
                if (trademarkStatus != null)
                {
                    trademarkStatus.Operator = trademarkStatusOp;
                    var statuses = trademarkStatus.GetValueList();

                    if (statuses.Count > 0)
                    {
                        if (trademarkStatus.Operator == "eq")
                            trademarks = trademarks.Where(m => statuses.Contains(m.TrademarkStatus));
                        else
                            trademarks = trademarks.Where(m => !statuses.Contains(m.TrademarkStatus));

                        mainSearchFilters.Remove(trademarkStatus);
                    }
                }

                var area = mainSearchFilters.FirstOrDefault(f => f.Property == "Area");
                if (area != null)
                {
                    var areas = area.GetValueListForLoop();
                    if (areas.Count > 0)
                    {
                        //Doesn't work because of the same name for member Area.Area
                        ////var areaPredicate = R10.Web.Helpers.ExpressionHelper.BuildLoopNestedPredicate<TmkAreaCountry>("Area.Area", areas, false);  

                        Expression<Func<TmkAreaCountry, bool>> areCountrypredicate = (item) => false;
                        foreach (var val in areas)
                        {
                            areCountrypredicate = areCountrypredicate.Or(a => EF.Functions.Like(a.Area.Area, val));
                        }
                        var countryPredicate = R10.Core.ExpressionHelper.BuildAnyPredicate<TmkCountry>("TmkCountryAreas", areCountrypredicate);

                        ParameterExpression parameterExpression = Expression.Parameter(typeof(TmkTrademark));
                        MemberExpression me = Expression.PropertyOrField(parameterExpression, "TmkCountry");
                        var invoke = Expression.Invoke(countryPredicate, me);
                        var lambda = Expression.Lambda<Func<TmkTrademark, bool>>(invoke, parameterExpression);
                        trademarks = trademarks.Where(lambda);
                    }
                    //trademarks = trademarks.Where(t => t.TmkCountry.TmkCountryAreas.Any(ca => ca.Area.Area == area.Value));

                    mainSearchFilters.Remove(area);
                }

                var indicatorOp = mainSearchFilters.GetFilterOperator("IndicatorOp");
                if (mainSearchFilters.FirstOrDefault(f => f.Property.StartsWith("DueDate.")) != null)
                {
                    var actionDue = mainSearchFilters.FirstOrDefault(f => f.Property == "DueDate.ActionDue");
                    var indicator = mainSearchFilters.FirstOrDefault(f => f.Property == "DueDate.Indicator");
                    var dueDateFrom = mainSearchFilters.FirstOrDefault(f => f.Property == "DueDate.DueDateFrom");
                    var dueDateTo = mainSearchFilters.FirstOrDefault(f => f.Property == "DueDate.DueDateTo");
                    var dateTakenFrom = mainSearchFilters.FirstOrDefault(f => f.Property == "DueDate.DateTakenFrom");
                    var dateTakenTo = mainSearchFilters.FirstOrDefault(f => f.Property == "DueDate.DateTakenTo");
                    var outstandingOnly = mainSearchFilters.FirstOrDefault(f => f.Property == "DueDate.ShowOutstandingActionsOnly");

                    Expression<Func<TmkDueDate, bool>> dueDatePredicate = (item) => false;
                    Expression<Func<TmkDueDate, bool>> dueDateDummyPredicate = (item) => false;

                    if (actionDue != null)
                    {
                        var actionDues = actionDue.GetValueListForLoop();
                        if (actionDues.Count > 0)
                        {
                            var actionDuePredicate = R10.Web.Helpers.ExpressionHelper.BuildLoopNestedPredicate<TmkDueDate>("ActionDue", actionDues, false);
                            if (dueDatePredicate.ToString() == dueDateDummyPredicate.ToString())
                                dueDatePredicate = dueDatePredicate.Or(actionDuePredicate);
                            else
                                dueDatePredicate = dueDatePredicate.And(actionDuePredicate);
                        }
                    }
                    if (indicator != null)
                    {
                        indicator.Operator = indicatorOp;
                        var indicators = indicator.GetValueListForLoop();
                        if (indicators.Count > 0)
                        {
                            Expression<Func<TmkDueDate, bool>> indicatorPredicate = dd => ((indicator.Operator == "eq" && indicators.Contains(dd.Indicator))
                                                                                                || (indicator.Operator != "eq" && !indicators.Contains(dd.Indicator)));
                            if (dueDatePredicate.ToString() == dueDateDummyPredicate.ToString())
                                dueDatePredicate = dueDatePredicate.Or(indicatorPredicate);
                            else
                                dueDatePredicate = dueDatePredicate.And(indicatorPredicate);
                        }
                    }

                    Expression<Func<TmkDueDate, bool>> dueDateCombinedPredicate = d => (
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

                    var ddAnyPredicate = R10.Core.ExpressionHelper.BuildAnyPredicate<TmkActionDue>("DueDates", dueDatePredicate);
                    var actionAnyPredicate = R10.Core.ExpressionHelper.BuildAnyPredicate<TmkTrademark>("ActionDues", ddAnyPredicate);

                    trademarks = trademarks.Where(actionAnyPredicate);

                    mainSearchFilters.Remove(actionDue);
                    mainSearchFilters.Remove(indicator);
                    mainSearchFilters.Remove(dueDateFrom);
                    mainSearchFilters.Remove(dueDateTo);
                    mainSearchFilters.Remove(dateTakenFrom);
                    mainSearchFilters.Remove(dateTakenTo);
                    mainSearchFilters.Remove(outstandingOnly);

                }

                if (mainSearchFilters.FirstOrDefault(f => f.Property.StartsWith("TrademarkClasses")) != null)
                {
                    var classId = mainSearchFilters.FirstOrDefault(f => f.Property == "TrademarkClasses.ClassId");
                    List<int> classIds = new List<int>();
                    if (classId != null)
                    {
                        var classList = classId.GetValueList();
                        if (classList.Count > 0)
                        {
                            classList.Each(c => classIds.Add(Int32.Parse(c)));
                        }
                        else
                        {
                            classIds.Add(Int32.Parse(classId.Value));
                        }
                        mainSearchFilters.Remove(classId);
                    }

                    var goods = mainSearchFilters.FirstOrDefault(f => f.Property == "TrademarkClasses.Goods");
                    var firstUseDateFrom = mainSearchFilters.FirstOrDefault(f => f.Property == "TrademarkClasses.FirstUseDateFrom");
                    var firstUseDateTo = mainSearchFilters.FirstOrDefault(f => f.Property == "TrademarkClasses.FirstUseDateTo");
                    var firstUseCommerceFrom = mainSearchFilters.FirstOrDefault(f => f.Property == "TrademarkClasses.FirstUseInCommerceFrom");
                    var firstUseCommerceTo = mainSearchFilters.FirstOrDefault(f => f.Property == "TrademarkClasses.FirstUseInCommerceTo");

                    trademarks = trademarks.Where(t => t.TrademarkClasses
                                        .Any(c =>
                                                    (classId == null || classIds.Contains(c.ClassId)) &&
                                                    (goods == null || EF.Functions.Like(c.Goods, goods.Value)) &&
                                                    (firstUseDateFrom == null || c.FirstUseDate >= Convert.ToDateTime(firstUseDateFrom.Value)) &&
                                                    (firstUseDateTo == null || c.FirstUseDate <= Convert.ToDateTime(firstUseDateTo.Value)) &&
                                                    (firstUseCommerceFrom == null || c.FirstUseInCommerce >= Convert.ToDateTime(firstUseCommerceFrom.Value)) &&
                                                    (firstUseCommerceTo == null || c.FirstUseInCommerce <= Convert.ToDateTime(firstUseCommerceTo.Value))));

                    mainSearchFilters.Remove(classId);
                    mainSearchFilters.Remove(goods);
                    mainSearchFilters.Remove(firstUseDateFrom);
                    mainSearchFilters.Remove(firstUseDateTo);
                    mainSearchFilters.Remove(firstUseCommerceFrom);
                    mainSearchFilters.Remove(firstUseCommerceTo);
                }

                if (mainSearchFilters.FirstOrDefault(f => f.Property.StartsWith("AssignmentsHistory.")) != null)
                {
                    var assignFrom = mainSearchFilters.FirstOrDefault(f => f.Property == "AssignmentsHistory.AssignmentFrom");
                    var assignTo = mainSearchFilters.FirstOrDefault(f => f.Property == "AssignmentsHistory.AssignmentTo");
                    var assignDateFrom = mainSearchFilters.FirstOrDefault(f => f.Property == "AssignmentsHistory.AssignmentDateFrom");
                    var assignDateTo = mainSearchFilters.FirstOrDefault(f => f.Property == "AssignmentsHistory.AssignmentDateTo");
                    var assignStatus = mainSearchFilters.FirstOrDefault(f => f.Property == "AssignmentsHistory.AssignmentStatus");
                    var reel = mainSearchFilters.FirstOrDefault(f => f.Property == "AssignmentsHistory.Reel");
                    var frame = mainSearchFilters.FirstOrDefault(f => f.Property == "AssignmentsHistory.Frame");

                    Expression<Func<TmkAssignmentHistory, bool>> assignmentPredicate = (item) => false;
                    Expression<Func<TmkAssignmentHistory, bool>> assignmentDummyPredicate = (item) => false;

                    if (assignFrom != null)
                    {
                        var assignFroms = assignFrom.GetValueListForLoop();
                        if (assignFroms.Count > 0)
                        {
                            var assignFromPredicate = R10.Web.Helpers.ExpressionHelper.BuildLoopNestedPredicate<TmkAssignmentHistory>("AssignmentFrom", assignFroms, false);
                            if (assignmentPredicate.ToString() == assignmentDummyPredicate.ToString())
                                assignmentPredicate = assignmentPredicate.Or(assignFromPredicate);
                            else
                                assignmentPredicate = assignmentPredicate.And(assignFromPredicate);
                        }
                    }

                    if (assignTo != null)
                    {
                        var assignTos = assignTo.GetValueListForLoop();
                        if (assignTos.Count > 0)
                        {
                            var assignToPredicate = R10.Web.Helpers.ExpressionHelper.BuildLoopNestedPredicate<TmkAssignmentHistory>("AssignmentTo", assignTos, false);
                            if (assignmentPredicate.ToString() == assignmentDummyPredicate.ToString())
                                assignmentPredicate = assignmentPredicate.Or(assignToPredicate);
                            else
                                assignmentPredicate = assignmentPredicate.And(assignToPredicate);
                        }
                    }

                    if (assignStatus != null)
                    {
                        var assignStatuses = assignStatus.GetValueListForLoop();
                        if (assignStatuses.Count > 0)
                        {
                            var assignStatusPredicate = R10.Web.Helpers.ExpressionHelper.BuildLoopNestedPredicate<TmkAssignmentHistory>("AssignmentStatus", assignStatuses, false);
                            if (assignmentPredicate.ToString() == assignmentDummyPredicate.ToString())
                                assignmentPredicate = assignmentPredicate.Or(assignStatusPredicate);
                            else
                                assignmentPredicate = assignmentPredicate.And(assignStatusPredicate);
                        }
                    }

                    Expression<Func<TmkAssignmentHistory, bool>> assignCombinedPredicate = a => (
                                                        (assignDateFrom == null || a.AssignmentDate >= Convert.ToDateTime(assignDateFrom.Value)) &&
                                                        (assignDateTo == null || a.AssignmentDate <= Convert.ToDateTime(assignDateTo.Value).AddDays(1).AddSeconds(-1)) &&
                                                        (reel == null || EF.Functions.Like(a.Reel, reel.Value)) &&
                                                        (frame == null || EF.Functions.Like(a.Frame, frame.Value))
                                                    );

                    if (assignmentPredicate.ToString() == assignmentDummyPredicate.ToString())
                        assignmentPredicate = assignmentPredicate.Or(assignCombinedPredicate);
                    else
                        assignmentPredicate = assignmentPredicate.And(assignCombinedPredicate);

                    var assignmentAnyPredicate = R10.Core.ExpressionHelper.BuildAnyPredicate<TmkTrademark>("AssignmentsHistory", assignmentPredicate);
                    trademarks = trademarks.Where(assignmentAnyPredicate);

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
                    var licensor = mainSearchFilters.FirstOrDefault(f => f.Property == "Licensees.Licensor");
                    var licensee = mainSearchFilters.FirstOrDefault(f => f.Property == "Licensees.Licensee");
                    var licenseNo = mainSearchFilters.FirstOrDefault(f => f.Property == "Licensees.LicenseNo");
                    var licenseStartFrom = mainSearchFilters.FirstOrDefault(f => f.Property == "Licensees.LicenseStartFrom");
                    var licenseStartTo = mainSearchFilters.FirstOrDefault(f => f.Property == "Licensees.LicenseStartTo");
                    var licenseExpireFrom = mainSearchFilters.FirstOrDefault(f => f.Property == "Licensees.LicenseExpireFrom");
                    var licenseExpireTo = mainSearchFilters.FirstOrDefault(f => f.Property == "Licensees.LicenseExpireTo");

                    Expression<Func<TmkLicensee, bool>> licanseePredicate = (item) => false;
                    Expression<Func<TmkLicensee, bool>> licanseeDummyPredicate = (item) => false;

                    if (licensor != null)
                    {
                        var licensors = licensor.GetValueListForLoop();
                        if (licensors.Count > 0)
                        {
                            var licensorPredicate = R10.Web.Helpers.ExpressionHelper.BuildLoopNestedPredicate<TmkLicensee>("Licensor", licensors, false);
                            if (licanseePredicate.ToString() == licanseeDummyPredicate.ToString())
                                licanseePredicate = licanseePredicate.Or(licensorPredicate);
                            else
                                licanseePredicate = licanseePredicate.And(licensorPredicate);
                        }
                    }
                    if (licensee != null)
                    {
                        var licensees = licensee.GetValueListForLoop();
                        if (licensees.Count > 0)
                        {
                            var licenseePredicate = R10.Web.Helpers.ExpressionHelper.BuildLoopNestedPredicate<TmkLicensee>("Licensee", licensees, false);
                            if (licanseePredicate.ToString() == licanseeDummyPredicate.ToString())
                                licanseePredicate = licanseePredicate.Or(licenseePredicate);
                            else
                                licanseePredicate = licanseePredicate.And(licenseePredicate);
                        }
                    }

                    Expression<Func<TmkLicensee, bool>> licenseeCombinedPredicate = l => (
                                                        (licenseNo == null || EF.Functions.Like(l.LicenseNo, licenseNo.Value)) &&
                                                        (licenseStartFrom == null || l.LicenseStart >= Convert.ToDateTime(licenseStartFrom.Value)) &&
                                                        (licenseStartTo == null || l.LicenseStart <= Convert.ToDateTime(licenseStartTo.Value)) &&
                                                        (licenseExpireFrom == null || l.LicenseExpire >= Convert.ToDateTime(licenseExpireFrom.Value)) &&
                                                        (licenseExpireTo == null || l.LicenseExpire <= Convert.ToDateTime(licenseExpireTo.Value))
                                                    );

                    if (licanseePredicate.ToString() == licanseeDummyPredicate.ToString())
                        licanseePredicate = licanseePredicate.Or(licenseeCombinedPredicate);
                    else
                        licanseePredicate = licanseePredicate.And(licenseeCombinedPredicate);

                    var licenseeAnyPredicate = R10.Core.ExpressionHelper.BuildAnyPredicate<TmkTrademark>("Licensees", licanseePredicate);
                    trademarks = trademarks.Where(licenseeAnyPredicate);

                    mainSearchFilters.Remove(licensor);
                    mainSearchFilters.Remove(licensee);
                    mainSearchFilters.Remove(licenseNo);
                    mainSearchFilters.Remove(licenseStartFrom);
                    mainSearchFilters.Remove(licenseStartTo);
                    mainSearchFilters.Remove(licenseExpireFrom);
                    mainSearchFilters.Remove(licenseExpireTo);
                }

                var costTypeOp = mainSearchFilters.GetFilterOperator("CostTypeOp");
                if (mainSearchFilters.FirstOrDefault(f => f.Property.StartsWith("CostTrackings.")) != null)
                {
                    var costType = mainSearchFilters.FirstOrDefault(f => f.Property == "CostTrackings.CostType");
                    var invoiceNumber = mainSearchFilters.FirstOrDefault(f => f.Property == "CostTrackings.InvoiceNumber");
                    var invoiceDateFrom = mainSearchFilters.FirstOrDefault(f => f.Property == "CostTrackings.InvoiceDateFrom");
                    var invoiceDateTo = mainSearchFilters.FirstOrDefault(f => f.Property == "CostTrackings.InvoiceDateTo");
                    var paymentDateFrom = mainSearchFilters.FirstOrDefault(f => f.Property == "CostTrackings.PayDateFrom");
                    var paymentDateTo = mainSearchFilters.FirstOrDefault(f => f.Property == "CostTrackings.PayDateTo");

                    Expression<Func<TmkCostTrack, bool>> costTrackPredicate = (item) => false;
                    Expression<Func<TmkCostTrack, bool>> costTrackDummyPredicate = (item) => false;

                    if (costType != null)
                    {
                        costType.Operator = costTypeOp;
                        var costTypes = costType.GetValueListForLoop();
                        if (costTypes.Count > 0)
                        {
                            Expression<Func<TmkCostTrack, bool>> predicate = (item) => false;
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

                    Expression<Func<TmkCostTrack, bool>> costTrackCombinedPredicate = c => (
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

                    var costTrackAnyPredicate = R10.Core.ExpressionHelper.BuildAnyPredicate<TmkTrademark>("CostTrackings", costTrackPredicate);
                    trademarks = trademarks.Where(costTrackAnyPredicate);

                    mainSearchFilters.Remove(costType);
                    mainSearchFilters.Remove(invoiceNumber);
                    mainSearchFilters.Remove(invoiceDateFrom);
                    mainSearchFilters.Remove(invoiceDateTo);
                    mainSearchFilters.Remove(paymentDateFrom);
                    mainSearchFilters.Remove(paymentDateTo);
                }

                if (mainSearchFilters.FirstOrDefault(f => f.Property.StartsWith("TmkConflicts.")) != null)
                {
                    var otherParty = mainSearchFilters.FirstOrDefault(f => f.Property == "TmkConflicts.OtherParty");
                    var otherPartyMark = mainSearchFilters.FirstOrDefault(f => f.Property == "TmkConflicts.OtherPartyMark");

                    Expression<Func<TmkConflict, bool>> conflictPredicate = (item) => false;
                    Expression<Func<TmkConflict, bool>> conflictDummyPredicate = (item) => false;

                    if (otherParty != null)
                    {
                        var otherPartys = otherParty.GetValueListForLoop();
                        if (otherPartys.Count > 0)
                        {
                            var otherPartyPredicate = R10.Web.Helpers.ExpressionHelper.BuildLoopNestedPredicate<TmkConflict>("OtherParty", otherPartys, false);
                            if (conflictPredicate.ToString() == conflictDummyPredicate.ToString())
                                conflictPredicate = conflictPredicate.Or(otherPartyPredicate);
                            else
                                conflictPredicate = conflictPredicate.And(otherPartyPredicate);
                        }
                    }
                    if (otherPartyMark != null)
                    {
                        var otherPartyMarks = otherPartyMark.GetValueListForLoop();
                        if (otherPartyMarks.Count > 0)
                        {
                            var otherPartyMarkPredicate = R10.Web.Helpers.ExpressionHelper.BuildLoopNestedPredicate<TmkConflict>("OtherPartyMark", otherPartyMarks, false);
                            if (conflictPredicate.ToString() == conflictDummyPredicate.ToString())
                                conflictPredicate = conflictPredicate.Or(otherPartyMarkPredicate);
                            else
                                conflictPredicate = conflictPredicate.And(otherPartyMarkPredicate);
                        }
                    }

                    var conflictAnyPredicate = R10.Core.ExpressionHelper.BuildAnyPredicate<TmkTrademark>("TmkConflicts", conflictPredicate);
                    trademarks = trademarks.Where(conflictAnyPredicate);

                    mainSearchFilters.Remove(otherParty);
                    mainSearchFilters.Remove(otherPartyMark);
                }

                if (mainSearchFilters.FirstOrDefault(f => f.Property.StartsWith("Client.")) != null)
                {
                    var clientCode = mainSearchFilters.FirstOrDefault(f => f.Property == "Client.ClientCode");
                    var clientName = mainSearchFilters.FirstOrDefault(f => f.Property == "Client.ClientName");

                    trademarks = trademarks.Where(t =>
                        (clientCode == null || EF.Functions.Like(t.Client.ClientCode, clientCode.Value)) &&
                        (clientName == null || EF.Functions.Like(t.Client.ClientName, clientName.Value))
                    );

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
                            Expression<Func<TmkTrademark, bool>> agentCodePredicate = (item) => false;
                            foreach (var val in agentCodes)
                            {
                                agentCodePredicate = agentCodePredicate.Or(tmk => EF.Functions.Like(tmk.Agent.AgentCode, val));
                            }
                            trademarks = trademarks.Where(agentCodePredicate);
                        }
                    }

                    if (agentName != null)
                    {
                        var agentNames = agentName.GetValueListForLoop();
                        if (agentNames.Count > 0)
                        {
                            Expression<Func<TmkTrademark, bool>> agentNamePredicate = (item) => false;
                            foreach (var val in agentNames)
                            {
                                agentNamePredicate = agentNamePredicate.Or(tmk => EF.Functions.Like(tmk.Agent.AgentName, val));
                            }
                            trademarks = trademarks.Where(agentNamePredicate);
                        }
                    }

                    mainSearchFilters.Remove(agentCode);
                    mainSearchFilters.Remove(agentName);
                }

                if (mainSearchFilters.FirstOrDefault(f => f.Property.StartsWith("Owner.")) != null)
                {
                    var ownerCode = mainSearchFilters.FirstOrDefault(f => f.Property == "Owner.OwnerCode");
                    var ownerName = mainSearchFilters.FirstOrDefault(f => f.Property == "Owner.OwnerName");

                    if (ownerCode != null)
                    {
                        var owCodes = ownerCode.GetValueListForLoop();
                        if (owCodes.Count > 0)
                        {
                            Expression<Func<TmkOwner, bool>> ownerPredicate = (item) => false;
                            foreach (var val in owCodes)
                            {
                                ownerPredicate = ownerPredicate.Or(o => EF.Functions.Like(o.Owner.OwnerCode, val));
                            }
                            var predicate = R10.Core.ExpressionHelper.BuildAnyPredicate<TmkTrademark>("Owners", ownerPredicate);
                            trademarks = trademarks.Where(predicate);
                        }
                    }
                    if (ownerName != null)
                    {
                        var owNames = ownerName.GetValueListForLoop();
                        if (owNames.Count > 0)
                        {
                            Expression<Func<TmkOwner, bool>> ownerPredicate = (item) => false;
                            foreach (var val in owNames)
                            {
                                ownerPredicate = ownerPredicate.Or(o => EF.Functions.Like(o.Owner.OwnerName, val));
                            }
                            var predicate = R10.Core.ExpressionHelper.BuildAnyPredicate<TmkTrademark>("Owners", ownerPredicate);
                            trademarks = trademarks.Where(predicate);
                        }
                    }

                    mainSearchFilters.Remove(ownerCode);
                    mainSearchFilters.Remove(ownerName);
                }

                if (mainSearchFilters.FirstOrDefault(f => f.Property.StartsWith("Product.")) != null)
                {
                    var productName = mainSearchFilters.FirstOrDefault(f => f.Property == "Product.ProductName");
                    if (productName != null)
                    {
                        var productNames = productName.GetValueListForLoop();
                        if (productNames.Count > 0)
                        {
                            var productNamePredicate = R10.Web.Helpers.ExpressionHelper.BuildLoopNestedPredicate<TmkProduct>("Product.ProductName", productNames, false);
                            var productAnyPredicate = R10.Core.ExpressionHelper.BuildAnyPredicate<TmkTrademark>("TmkProducts", productNamePredicate);
                            trademarks = trademarks.Where(productAnyPredicate);
                        }
                    }
                    mainSearchFilters.Remove(productName);
                }

                if (mainSearchFilters.FirstOrDefault(f => f.Property.StartsWith("Attorney.")) != null)
                {
                    var attorneyCode = mainSearchFilters.FirstOrDefault(f => f.Property == "Attorney.AttorneyCode");
                    var attorneyName = mainSearchFilters.FirstOrDefault(f => f.Property == "Attorney.AttorneyName");
                    if (attorneyCode != null)
                    {
                        var attyCodes = attorneyCode.GetValueListForLoop();
                        if (attyCodes.Count > 0)
                        {
                            Expression<Func<TmkTrademark, bool>> predicate = (item) => false;
                            foreach (var atty in attyCodes)
                            {
                                predicate = predicate.Or(w => EF.Functions.Like(w.Attorney1.AttorneyCode, atty) ||
                                                              EF.Functions.Like(w.Attorney2.AttorneyCode, atty) ||
                                                              EF.Functions.Like(w.Attorney3.AttorneyCode, atty) ||
                                                              EF.Functions.Like(w.Attorney4.AttorneyCode, atty) ||
                                                              EF.Functions.Like(w.Attorney5.AttorneyCode, atty));
                            }
                            trademarks = trademarks.Where(predicate);
                        }
                    }

                    if (attorneyName != null)
                    {
                        var attyNames = attorneyName.GetValueListForLoop();
                        if (attyNames.Count > 0)
                        {
                            Expression<Func<TmkTrademark, bool>> predicate = (item) => false;
                            foreach (var attyName in attyNames)
                            {
                                predicate = predicate.Or(w => EF.Functions.Like(w.Attorney1.AttorneyName, attyName) ||
                                                              EF.Functions.Like(w.Attorney2.AttorneyName, attyName) ||
                                                              EF.Functions.Like(w.Attorney3.AttorneyName, attyName) ||
                                                              EF.Functions.Like(w.Attorney4.AttorneyName, attyName) ||
                                                              EF.Functions.Like(w.Attorney5.AttorneyName, attyName));
                            }
                            trademarks = trademarks.Where(predicate);
                        }
                    }

                    mainSearchFilters.Remove(attorneyCode);
                    mainSearchFilters.Remove(attorneyName);
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
                            docs = graphClient.GetSiteDocumentNamesByMetadata(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, SharePointDocLibrary.Trademark, SharePointDocLibraryFolder.Trademark, docName != null ? docName.Value : "").GetAwaiter().GetResult();
                        else
                            docs = graphClient.GetSiteDocumentNames(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, SharePointDocLibrary.Trademark, new List<string> { SharePointDocLibraryFolder.Trademark }, docName != null ? docName.Value : "").GetAwaiter().GetResult();

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
                                var trademark = _trademarkService.TmkTrademarks.Where(a => a.CaseNumber == caseNumber && a.Country == country && a.SubCase == subCase).FirstOrDefaultAsync().GetAwaiter().GetResult();
                                if (trademark != null)
                                    d.ParentId = trademark.TmkId;
                            });
                            var recKeys = docs.Select(d => d.ParentId).ToList();
                            trademarks = trademarks.Where(a => recKeys.Contains(a.TmkId));
                        }
                        else
                        {
                            trademarks = trademarks.Where(a => false);
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

                        trademarks = trademarks.Where(a =>
                          _docService.DocDocuments.Any(d =>
                              (d.DocFolder.SystemType == SystemTypeCode.Trademark && d.DocFolder.DataKey == "TmkId" && d.DocFolder.DataKeyValue == a.TmkId) &&
                              (docName == null || EF.Functions.Like(d.DocName, docName.Value)) &&
                              //(tag == null || d.DocDocumentTags.Any(t => EF.Functions.Like(t.Tag, tag.Value))) &&
                              (tag == null || (string.IsNullOrEmpty(tagsList) && d.DocDocumentTags.Any(t => EF.Functions.Like(t.Tag, tag.Value))) || (!string.IsNullOrEmpty(tagsList) && d.DocDocumentTags.Any(t => EF.Functions.Like(tagsList, '%' + t.Tag + '%')))) &&
                              (isVerified == null || d.IsVerified == Convert.ToBoolean(isVerified.Value)) &&
                              (dateCreatedFrom == null || d.DateCreated >= Convert.ToDateTime(dateCreatedFrom.Value)) &&
                              (dateCreatedTo == null || d.DateCreated <= Convert.ToDateTime(dateCreatedTo.Value))
                          )
                        );
                    }

                    mainSearchFilters.Remove(docName);
                    mainSearchFilters.Remove(tag);
                    mainSearchFilters.Remove(dateCreatedFrom);
                    mainSearchFilters.Remove(dateCreatedTo);
                    mainSearchFilters.Remove(isVerified);
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

                    trademarks = trademarks.Where(a => a.ActionDues.Any(ad => (verifiedBy == null || EF.Functions.Like(ad.VerifiedBy, verifiedBy.Value))
                                                        && (verifiedDateFrom == null || ad.DateVerified >= Convert.ToDateTime(verifiedDateFrom.Value))
                                                        && (verifiedDateTo == null || ad.DateVerified <= Convert.ToDateTime(verifiedDateTo.Value))));

                    if (!string.IsNullOrEmpty(idType) && idValue > 0)
                    {
                        if (settings.IsSharePointIntegrationOn && settings.IsSharePointListRealTime)
                        {
                            var graphClient = _sharePointService.GetGraphClient();
                            var docs = new List<SharePointGraphDocPicklistViewModel>();

                            if (settings.IsSharePointIntegrationByMetadataOn)
                                docs = graphClient.GetSiteDocumentNamesByMetadata(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, SharePointDocLibrary.Trademark, SharePointDocLibraryFolder.Trademark, "").GetAwaiter().GetResult();
                            else
                                docs = graphClient.GetSiteDocumentNames(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, SharePointDocLibrary.Trademark, new List<string> { SharePointDocLibraryFolder.Trademark }, "").GetAwaiter().GetResult();

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

                                    var trademark = _trademarkService.TmkTrademarks.Where(a => a.CaseNumber == caseNumber && a.Country == country && a.SubCase == subCase).FirstOrDefaultAsync().GetAwaiter().GetResult();
                                    if (trademark != null)
                                        d.ParentId = trademark.TmkId;
                                });
                                var docIds = docs.Where(d => d.ParentId > 0).Select(d => new { DriveItemId = d.Id, TmkId = d.ParentId }).ToList();
                                var verifications = _docService.DocVerifications.Where(dv => docIds.Any(d => d.DriveItemId == dv.DocDocument.DocFile.DriveItemId)
                                                                && ((idType == "actid" && dv.ActId == idValue) || (idType == "actiontypeid" && dv.ActionTypeID == idValue))
                                                            )
                                                            .Select(d => d.DocDocument.DocFile.DriveItemId).ToListAsync().GetAwaiter().GetResult();

                                var tmkIds = docIds.Where(d => verifications.Contains(d.DriveItemId)).Select(d => d.TmkId).ToList();

                                trademarks = trademarks.Where(a => tmkIds.Contains(a.TmkId));
                            }
                            else
                            {
                                trademarks = trademarks.Where(a => false);
                            }
                        }
                        else
                        {
                            trademarks = trademarks.Where(a =>
                                          _docService.DocDocuments.Any(d => d.DocFolder != null
                                                && (d.DocFolder.SystemType ?? "").ToLower() == SystemTypeCode.Trademark.ToLower()
                                                && (d.DocFolder.ScreenCode ?? "").ToLower() == ScreenCode.Trademark.ToLower()
                                                && (d.DocFolder.DataKey ?? "").ToLower() == "tmkid"
                                                && d.DocFolder.DataKeyValue == a.TmkId
                                                && d.DocVerifications != null
                                                && d.DocVerifications.Any(dv => ((idType == "actid" && dv.ActId == idValue) || (idType == "actiontypeid" && dv.ActionTypeID == idValue)))
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
                        trademarks = trademarks.Where(i => i.Owners != null && i.Owners.Count == 1);
                    }
                    else
                    {
                        trademarks = trademarks.Where(i => i.Owners != null && i.Owners.Count > 1);
                    }
                    mainSearchFilters.Remove(noofOwners);
                }

                //number search
                var appNumber = mainSearchFilters.FirstOrDefault(f => f.Property == "AppNumber");
                if (appNumber != null)
                {
                    var appNumberSearch = QueryHelper.ExtractSignificantNumbers(appNumber.Value);
                    trademarks = trademarks.Where(a => (EF.Functions.Like(a.AppNumber, appNumber.Value) || EF.Functions.Like(a.AppNumberSearch, appNumberSearch)));
                    mainSearchFilters.Remove(appNumber);
                }

                var pubNumber = mainSearchFilters.FirstOrDefault(f => f.Property == "PubNumber");
                if (pubNumber != null)
                {
                    var pubNumberSearch = QueryHelper.ExtractSignificantNumbers(pubNumber.Value);
                    trademarks = trademarks.Where(a => (EF.Functions.Like(a.PubNumber, pubNumber.Value) || EF.Functions.Like(a.PubNumberSearch, pubNumberSearch)));
                    mainSearchFilters.Remove(pubNumber);
                }

                var regNumber = mainSearchFilters.FirstOrDefault(f => f.Property == "RegNumber");
                if (regNumber != null)
                {
                    var regNumberSearch = QueryHelper.ExtractSignificantNumbers(regNumber.Value);
                    trademarks = trademarks.Where(a => (EF.Functions.Like(a.RegNumber, regNumber.Value) || EF.Functions.Like(a.RegNumberSearch, regNumberSearch)));
                    mainSearchFilters.Remove(regNumber);
                }

                var priNumber = mainSearchFilters.FirstOrDefault(f => f.Property == "PriNumber");
                if (priNumber != null)
                {
                    var priNumberSearch = QueryHelper.ExtractSignificantNumbers(priNumber.Value);
                    trademarks = trademarks.Where(a => (EF.Functions.Like(a.PriNumber, priNumber.Value) || EF.Functions.Like(a.PriNumberSearch, priNumberSearch)));
                    mainSearchFilters.Remove(priNumber);
                }

                if (mainSearchFilters.Any())
                    trademarks = QueryHelper.BuildCriteria<TmkTrademark>(trademarks, mainSearchFilters);
            }
            return trademarks;
        }

        public async Task<CPiDataSourceResult> CreateViewModelForGrid(DataSourceRequest request, IQueryable<TmkTrademark> trademarks)
        {
            IQueryable<TmkTrademarkSearchResultViewModel> model;

            var settings = await _settings.GetSetting();

            if (settings.IsSharePointIntegrationOn)
                model = trademarks.ProjectTo<TmkTrademarkSearchResultSharePointViewModel>();
            else
                model = trademarks.ProjectTo<TmkTrademarkSearchResultViewModel>();

            if (request.Sorts != null && request.Sorts.Any())
                model = model.ApplySorting(request.Sorts);
            else
                model = model.OrderBy(tmk => tmk.CaseNumber).ThenBy(tmk => tmk.Country).ThenBy(tmk => tmk.SubCase);

            var ids = await model.Select(tmk => tmk.TmkId).ToArrayAsync();
            var list = await model.ApplyPaging(request.Page, request.PageSize).ToListAsync();
            //await GetThumbnails(list);

            return new CPiDataSourceResult()
            {
                Data = list,
                Total = ids.Length,
                Ids = ids
            };
        }

        //protected async Task GetThumbnails(List<TmkTrademarkSearchResultViewModel> trademarks) {
        //    var selectedIds = trademarks.Select(t => t.TmkId).ToList();
        //    var documents = await _trademarkService.Documents.Where(d => d.IsDefault && selectedIds.Any(i => i == d.DocFolder.DataKeyValue)).Include(d=> d.DocFolder).Include(d=> d.DocFile).ToListAsync();
        //    foreach (var item in documents) {
        //        if (!string.IsNullOrEmpty(item.DocFile.ThumbFileName)) {
        //            var trademark = trademarks.First(t => t.TmkId == item.DocFolder.DataKeyValue);
        //            trademark.ThumbnailFile = item.DocFile.ThumbFileName;
        //        }
        //    }
        //}

        public TmkTrademark ConvertViewModelToTmkTrademark(TmkTrademarkDetailViewModel viewModel)
        {
            var trademark = _mapper.Map<TmkTrademark>(viewModel);
            if (trademark.SubCase == null) trademark.SubCase = "";
            return trademark;
        }

        public async Task<TmkTrademarkDetailViewModel> CreateViewModelForDetailScreen(int id)
        {
            var viewModel = new TmkTrademarkDetailViewModel();


            if (id > 0)
                viewModel = await _trademarkService.TmkTrademarks
                                .ProjectTo<TmkTrademarkDetailViewModel>().FirstOrDefaultAsync(t => t.TmkId == id);

            if (viewModel != null)
            {
                viewModel.CanModifyAttorney1 = await _trademarkService.CanModifyAttorney(viewModel.Attorney1ID ?? 0);
                viewModel.CanModifyAttorney2 = await _trademarkService.CanModifyAttorney(viewModel.Attorney2ID ?? 0);
                viewModel.CanModifyAttorney3 = await _trademarkService.CanModifyAttorney(viewModel.Attorney3ID ?? 0);
                viewModel.CanModifyAttorney4 = await _trademarkService.CanModifyAttorney(viewModel.Attorney4ID ?? 0);
                viewModel.CanModifyAttorney5 = await _trademarkService.CanModifyAttorney(viewModel.Attorney5ID ?? 0);
                //if (viewModel.ImageFile != null)
                //{
                //    viewModel.ImageFile = ImageHelper.GetImageUrlPath("Trademark") + viewModel.ImageFile;
                //}

                if ((viewModel.ParentTmkId ?? 0) != 0)
                {
                    var parentCaseInfo = await _trademarkService.TmkTrademarks.Where(p => p.TmkId == viewModel.ParentTmkId)
                                            .Select(p => new { ParentCaseInfo = p.CaseNumber + "/" + p.Country + (p.SubCase.Length == 0 ? "" : "/" + p.SubCase) + "/" + p.CaseType })
                                            .FirstOrDefaultAsync();
                    if (parentCaseInfo != null)
                        viewModel.ParentCase = parentCaseInfo.ParentCaseInfo;
                }

            }

            return viewModel;
        }

        //public IQueryable<TmkTrademark> GetFamilyReferenceList()
        //{
        //    return _trademarkService.TmkTrademarks; 
        //}

        public async Task<FamilyTreeDiagram> GetFamilyTreeDiagram(string paramType, string paramValue)
        {
            var setting = await _settings.GetSetting();

            var trademarks = _trademarkService.TmkTrademarks.ProjectTo<TmkTrademarkFamilyTreeViewModel>();

            FamilyTreeDiagram graph = new FamilyTreeDiagram();
            graph.Header.LabelCaseNumber = setting.IsClientMatterOn ? setting.LabelClientMatter : setting.LabelCaseNumber;
            graph.Header.LabelClient = setting.LabelClient;
            graph.Header.LabelTitle = "Trademark";

            FamilyTreeDiagramDTO node = new FamilyTreeDiagramDTO();

            if (paramType == "F")  // Family level node
            {
                node.Text = paramValue;
                node.Type = "F";
                node.Client = "";
                node.Title = "";
                graph.Nodes.Add(node);
                graph.Header.CaseNumber = paramValue;
                GetFamilyTreeDiagramFamilyChildren(graph, node, trademarks);
            }
            else if (paramType == "C")
            {
                var trademark = trademarks.First(c => c.TmkId == int.Parse(paramValue));
                if (!string.IsNullOrEmpty(trademark.TrademarkName))     // Family level node
                {
                    node.Text = trademark.TrademarkName;
                    node.Type = "F";
                    node.Client = "";
                    node.Title = "";
                    graph.Nodes.Add(node);

                    graph.Header.CaseNumber = trademark.CaseNumber;
                    graph.Header.Title.Add(trademark.TrademarkName ?? "");
                    graph.Header.Client.Add(trademark.ClientName ?? "");
                    GetFamilyTreeDiagramFamilyChildren(graph, node, trademarks);
                }
                else    // Trademark level node
                {
                    // I don't think it should reach here.
                    GetFamilyTreeDiagramFamilyChildren(graph, node, trademarks); 
                }
            }

            return graph;
        }

        /// <summary>
        /// Family level to Trademark level
        /// </summary>
        private void GetFamilyTreeDiagramFamilyChildren(FamilyTreeDiagram graph, FamilyTreeDiagramDTO familyLevelNode,
            IQueryable<TmkTrademarkFamilyTreeViewModel> trademarks)
        {

            var stats = new FamilyTreeDiagramStats();

            var tmks = trademarks.Where(c => c.TrademarkName == familyLevelNode.Text && (c.ParentTmkId == null || c.ParentTmkId == 0));
            foreach (var tmk in tmks)
            {
                FamilyTreeDiagramDTO tmkLevelNode = new FamilyTreeDiagramDTO
                {
                    Text = tmk.CaseNumber + "/" + tmk.Country + (string.IsNullOrEmpty(tmk.SubCase) ? "" : "-" + tmk.SubCase) + "/" + tmk.TrademarkStatus,
                    CaseType = tmk.CaseTypeDescription,
                    AppNumber = tmk.AppNumber,
                    PubNumber = tmk.PubNumber,
                    PubDate = tmk.PubDate,
                    FilDate = tmk.FilDate,
                    RegDate = tmk.RegDate,
                    RegNumber = tmk.RegNumber,
                    LastRenewalDate = tmk.LastRenewalDate,
                    NextRenewalDate = tmk.NextRenewalDate,
                    Classes = String.Join("; ", tmk.TrademarkClasses.OrderBy(c => c.Class).Select(c => $"{c.Class} {c.ClassType}").ToArray()),
                    Type = "T",
                    Client = tmk.ClientName ?? "",
                    Country = tmk.Country,
                    Title = tmk.TrademarkName ?? "",
                    Id = tmk.TmkId.ToString(),
                    KeyId = tmk.TmkId,
                    ParentAppNumber = tmk.ParentAppNumber,
                    ParentFilDate = tmk.ParentFilDate,
                    Active = tmk.IsActive,
                };

                if ((tmkLevelNode.FilDate ?? DateTime.Today) < (graph.Stats.FilDate ?? DateTime.Today))
                {
                    graph.Stats.FilDate = tmkLevelNode.FilDate;
                    graph.Stats.Classes = tmkLevelNode.Classes;
                    graph.Stats.MarkType = tmk.MarkType; // add?
                }

                if (tmkLevelNode.Active)
                    graph.Stats.ActiveCount += 1;
                else
                    graph.Stats.InactiveCount += 1;

                if (!graph.Stats.MadridProtocol &&
                        ((tmk.Country == "WP" && tmk.CaseType == "ORD" && tmk.FilDate != null) || (tmk.Country == "WO" && tmk.CaseType == "ORD" && tmk.FilDate != null)))
                    graph.Stats.MadridProtocol = true;

                if (!graph.Stats.EuropeanUnion && (tmk.Country == "EM" && new[] { "ORD", "DIV", "PRI", "SUB" }.Contains(tmk.CaseType) && tmk.FilDate != null))
                    graph.Stats.EuropeanUnion = true;

                graph.Nodes.Add(tmkLevelNode);
                graph.Edges.Add(new FamilyTreeDiagramEdge { StartId = familyLevelNode.Id, EndId = tmkLevelNode.Id });
                GetFamilyTreeDiagramTrademarkChildren(graph, tmkLevelNode, trademarks, 0);
            }

        }

        /// <summary>
        /// Trademark level to child trademarks
        /// </summary>
        private void GetFamilyTreeDiagramTrademarkChildren(FamilyTreeDiagram graph, FamilyTreeDiagramDTO tmkLevelNode,
            IQueryable<TmkTrademarkFamilyTreeViewModel> trademarks, int index)
        {
            if (index >= 10)
                return;
            index++; // to avoid infinite loop

            var tmks = trademarks.Where(c => c.ParentTmkId != null && c.ParentTmkId.ToString() == tmkLevelNode.Id);
            foreach (var tmk in tmks)
            {
                FamilyTreeDiagramDTO tmkChildNode = new FamilyTreeDiagramDTO
                {
                    Text = tmk.CaseNumber + "/" + tmk.Country + (string.IsNullOrEmpty(tmk.SubCase) ? "" : "-" + tmk.SubCase) + "/" + tmk.TrademarkStatus,
                    CaseType = tmk.CaseTypeDescription,
                    AppNumber = tmk.AppNumber,
                    PubNumber = tmk.PubNumber,
                    PubDate = tmk.PubDate,
                    FilDate = tmk.FilDate,
                    RegDate = tmk.RegDate,
                    RegNumber = tmk.RegNumber,
                    LastRenewalDate = tmk.LastRenewalDate,
                    NextRenewalDate = tmk.NextRenewalDate,
                    Classes = String.Join("; ", tmk.TrademarkClasses.OrderBy(c => c.Class).Select(c => $"{c.Class} {c.ClassType}").ToArray()),
                    Type = "T",
                    Client = tmk.ClientName ?? "",
                    Country = tmk.Country,
                    Title = tmk.TrademarkName ?? "",
                    Id = tmk.TmkId.ToString(),
                    KeyId = tmk.TmkId,
                    ParentAppNumber = tmk.ParentAppNumber,
                    ParentFilDate = tmk.ParentFilDate,
                    Active = tmk.IsActive,
                };

                if ((tmkLevelNode.FilDate ?? DateTime.Today) < (graph.Stats.FilDate ?? DateTime.Today))
                {
                    graph.Stats.FilDate = tmkLevelNode.FilDate;
                    graph.Stats.Classes = tmkLevelNode.Classes;
                    graph.Stats.MarkType = tmk.MarkType; // add?
                }

                if (tmkLevelNode.Active)
                    graph.Stats.ActiveCount += 1;
                else
                    graph.Stats.InactiveCount += 1;

                if (!graph.Stats.MadridProtocol &&
                        ((tmk.Country == "WP" && tmk.CaseType == "ORD" && tmk.FilDate != null) || (tmk.Country == "WO" && tmk.CaseType == "ORD" && tmk.FilDate != null)))
                    graph.Stats.MadridProtocol = true;

                if (!graph.Stats.EuropeanUnion && (tmk.Country == "EM" && new[] { "ORD", "DIV", "PRI", "SUB" }.Contains(tmk.CaseType) && tmk.FilDate != null))
                    graph.Stats.EuropeanUnion = true;

                graph.Nodes.Add(tmkChildNode);
                graph.Edges.Add(new FamilyTreeDiagramEdge { StartId = tmkLevelNode.Id, EndId = tmkChildNode.Id, Label = tmk.CaseType });
                GetFamilyTreeDiagramTrademarkChildren(graph, tmkChildNode, trademarks, index);
            }
        }

        public IQueryable<CaseNumberLookupViewModel> GetCaseNumbersList(IQueryable<TmkTrademark> trademarks, DataSourceRequest request, string textProperty, string text, FilterType filterType)
        {
            if (request.Filters?.Count > 0)
            {
                text = ((FilterDescriptor)request.Filters[0]).Value as string;
            }

            trademarks = QueryHelper.BuildCriteria(trademarks, textProperty, text, filterType);
            var result = trademarks.Select(t => new CaseNumberLookupViewModel { Id = t.TmkId, CaseNumber = t.CaseNumber }).OrderBy(t => t.CaseNumber).Distinct();
            return result;
        }

        public async Task<CaseNumberLookupViewModel> CaseNumberSearchValueMapper(IQueryable<TmkTrademark> trademarks, string value)
        {
            var result = await trademarks.Where(t => t.CaseNumber == value)
                .Select(t => new CaseNumberLookupViewModel { Id = t.TmkId, CaseNumber = t.CaseNumber }).FirstOrDefaultAsync();
            return result;
        }

        public async Task<TrademarkNameLookupViewModel> TrademarkNameValueMapper(IQueryable<TmkTrademark> trademarks, string value)
        {
            var result = await trademarks.Where(t => t.CaseNumber == value)
                .Select(t => new TrademarkNameLookupViewModel { TrademarkName = t.TrademarkName }).FirstOrDefaultAsync();
            return result;
        }

        public async Task<List<WorkflowEmailViewModel>> ProcessSaveWorkflow(TmkTrademark trademark, bool checkStatusChangeWorkFlow, bool checkAttyChangeWorkFlow, string? oldTrademarkStatus,
                                              string? emailUrl, string? attyEmailUrl, string? userName, int delegationId, string? delegationEmailUrl, string? actionEmailUrl)
        {
            var workFlows = new List<WorkflowViewModel>();
            var emailWorkflows = new List<WorkflowEmailViewModel>();
            var dateCreated = DateTime.Now;

            if (checkStatusChangeWorkFlow)
            {
                var workflowActions = await _workflowViewModelService.GetTrademarkWorkflowActions(trademark, TmkWorkflowTriggerType.StatusChanged, false);
                if (workflowActions.Any())
                {
                    var newTrademarkStatus = await _trademarkService.TmkTrademarks.Where(c => c.TmkId == trademark.TmkId).Select(t => t.TmkTrademarkStatus).FirstOrDefaultAsync();
                    if (newTrademarkStatus.TrademarkStatus != oldTrademarkStatus)
                    {
                        var newTrademarkStatusId = newTrademarkStatus.TrademarkStatusId;
                        var workFlowActions = workflowActions.Where(a => a.Workflow.TriggerValueId == newTrademarkStatusId || a.Workflow.TriggerValueId == 0 || (a.Workflow.TriggerValueId == -1 && newTrademarkStatus.ActiveSwitch) || (a.Workflow.TriggerValueId == -2 && !newTrademarkStatus.ActiveSwitch)).ToList();
                        workFlowActions = _workflowViewModelService.ClearTmkBaseWorkflowActions(workFlowActions);

                        foreach (var item in workFlowActions)
                        {
                            workFlows.Add(new WorkflowViewModel
                            {
                                ActionTypeId = item.ActionTypeId,
                                ActionValueId = item.ActionValueId,
                                Preview = item.Preview,
                                AutoAttachImages = item.IncludeAttachments,
                                EmailUrl = emailUrl,
                                Id = trademark.TmkId,
                                AttachmentFilter = item.AttachmentFilter
                            });
                        }
                    }
                }
            }

            //country law actions (new actions)
            var workflowCLActionsMain = await _workflowViewModelService.GetTrademarkWorkflowActions(trademark, TmkWorkflowTriggerType.NewAction, false);
            workflowCLActionsMain = workflowCLActionsMain.Where(w => w.Workflow.TriggerValueId <= 0).ToList();
            workflowCLActionsMain = _workflowViewModelService.ClearTmkBaseWorkflowActions(workflowCLActionsMain);

            var workflowCLActions = workflowCLActionsMain.Select(w =>
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
                Expression<Func<TmkCountryDue, bool>> predicate = (item) => false;
                foreach (var item in workflowCLActions)
                {
                    predicate = predicate.Or(cd => cd.CDueId == item.TriggerValueId);
                }

                var baseActionTypes = await _trademarkService.TmkCountryDues.Where(predicate).Select(cd => cd.ActionType).ToListAsync();
                var actionTypes = await _trademarkService.TmkCountryDues.Where(cd => baseActionTypes.Any(at => at == cd.ActionType)).ToListAsync();
                if (actionTypes.Any() || workflowCLActions.Any(wf => wf.TriggerValueId == 0))
                {
                    //dateCreated = dateCreated.AddTicks(-(dateCreated.Ticks % TimeSpan.TicksPerSecond)); //remove the ms
                    dateCreated = dateCreated.AddSeconds(-25); //remove 25 secs

                    var newCLActions = await _trademarkService.QueryableChildList<TmkActionDue>().Where(a => a.TmkId == trademark.TmkId && a.ComputerGenerated && a.CreatedBy == userName && a.DateCreated >= dateCreated).Include(ad => ad.TmkTrademark).ToListAsync();
                    if (workflowCLActions.Any(wf => wf.TriggerValueId > 0))
                    {
                        newCLActions = newCLActions.Where(a => actionTypes.Any(at => at.ActionType == a.ActionType)).ToList();
                    }

                    //trigger is specific action
                    foreach (var item in newCLActions)
                    {
                        var actionType = actionTypes.FirstOrDefault(at => at.Country == item.Country && at.CaseType == item.TmkTrademark.CaseType && at.ActionType == item.ActionType);
                        if (actionType != null)
                        {
                            var workFlowAction = workflowCLActions.FirstOrDefault(a => actionTypes.Any(at => at.ActionType == item.ActionType && at.CDueId == a.TriggerValueId));
                            if (workFlowAction != null && workFlowAction.ActionValueId != 0)
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
                        if (workFlowAction != null && workFlowAction.ActionValueId != 0 && !workFlows.Any(w => w.ActionTypeId == workFlowAction.ActionTypeId && w.ActionValueId == workFlowAction.ActionValueId))
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

            //Attorney changed
            if (checkAttyChangeWorkFlow)
            {
                var workflowActions = await _workflowViewModelService.GetTrademarkWorkflowActions(trademark, TmkWorkflowTriggerType.AttorneyModified, true);
                if (workflowActions.Any())
                {
                    foreach (var item in workflowActions)
                    {
                        var workFlow = new WorkflowViewModel
                        {
                            ActionTypeId = item.ActionTypeId,
                            ActionValueId = item.ActionValueId,
                            Preview = item.Preview,
                            AutoAttachImages = item.IncludeAttachments,
                            EmailUrl = attyEmailUrl,
                            Id = trademark.TmkId,
                            AttachmentFilter = item.AttachmentFilter
                        };
                        workFlows.Add(workFlow);
                    }
                }
            }

            _trademarkService.DetachAllEntities();

            //Note: for Renewals, we don't close them (responsedate), if workflow is needed, maybe attached on the new action (new renewal)
            var createActionWorkflows = workFlows.Where(wf => wf.ActionTypeId == (int)TmkWorkflowActionType.CreateAction).Distinct().ToList();
            foreach (var item in createActionWorkflows)
            {
                await _trademarkService.GenerateWorkflowAction(trademark.TmkId, item.ActionValueId);
            }

            var closeActionWorkflows = workFlows.Where(wf => wf.ActionTypeId == (int)TmkWorkflowActionType.CloseAction).Distinct().ToList();
            foreach (var item in closeActionWorkflows)
            {
                var actionDuesToClose = await _trademarkService.CloseWorkflowAction(trademark.TmkId, item.ActionValueId);
                if (actionDuesToClose.Any())
                {
                    foreach (var actionDueToClose in actionDuesToClose)
                    {
                        await _actionDueService.Update(actionDueToClose);
                    }
                }
            }

            emailWorkflows = workFlows.Where(wf => wf.ActionTypeId == (int)TmkWorkflowActionType.SendEmail)
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

            //previous renewal closed
            if (delegationId > 0)
            {
                var wfs = await _workflowViewModelService.GetTrademarkWorkflowActions(trademark, TmkWorkflowTriggerType.ActionDelegatedCompleted, false);
                wfs = wfs.Where(w => w.ActionTypeId == (int)TmkWorkflowActionType.SendEmail).ToList();
                wfs = _workflowViewModelService.ClearTmkBaseWorkflowActions(wfs);

                var doNotEmailList = await _userSettingManager.GetDoNotSendQuickEmailList(QuickEmailOptOutSetting.ActionCompleted, Convert.ToChar(SystemTypeCode.Trademark));
                if (wfs.Any())
                {
                    foreach (var wf in wfs)
                    {
                        var emails = await _trademarkService.GetDelegationEmails(delegationId);
                        var emailString = "";
                        foreach (var email in emails)
                        {
                            if (!doNotEmailList.Any(e => e.IsCaseInsensitiveEqual(email.AssignedTo)))
                            {
                                emailString = emailString + email.AssignedTo + ";";
                            }
                        }

                        emailWorkflows.Add(new WorkflowEmailViewModel
                        {
                            isAutoEmail = !wf.Preview,
                            qeSetupId = wf.ActionValueId,
                            autoAttachImages = wf.IncludeAttachments,
                            id = delegationId,
                            fileNames = new string[] { },
                            emailUrl = delegationEmailUrl,
                            emailTo = emailString,
                            attachmentFilter = wf.AttachmentFilter
                        });
                    }
                }

            }

            return emailWorkflows;
        }

    }
}
