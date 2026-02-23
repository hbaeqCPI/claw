using AutoMapper;
using AutoMapper.QueryableExtensions;
using Kendo.Mvc;
using Kendo.Mvc.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using R10.Core.Entities.Patent;
using R10.Core.Helpers;
using R10.Core.Identity;
using R10.Core.Interfaces;
using R10.Core.Interfaces.Patent;
using R10.Web.Areas.Patent.ViewModels;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;
using R10.Web.Helpers;
using R10.Web.Interfaces;
using R10.Web.Models;
using R10.Web.Services.SharePoint;
using R10.Web.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using ActiveQueryBuilder.View;
using System.Linq.Expressions;
using R10.Core;
using Kendo.Mvc.Extensions;
using DocumentFormat.OpenXml.Wordprocessing;
using static R10.Web.Helpers.ExpressionHelper;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using R10.Web.Areas.Shared.ViewModels.SharePoint;
using R10.Core.Interfaces.Shared;
using R10.Core.Entities.Shared;

namespace R10.Web.Areas.Patent.Services
{
    public class InventionViewModelService : IInventionViewModelService
    {
        private readonly IInventionService _inventionService;
        private readonly IMapper _mapper;
        private readonly ISystemSettings<PatSetting> _settings;
        private readonly IDocumentService _docService;
        private readonly ISharePointService _sharePointService;
        private readonly GraphSettings _graphSettings;
        private readonly ITradeSecretService _tradeSecretService;
        private readonly ClaimsPrincipal _user;

        public InventionViewModelService(
            IInventionService inventionService,
            IMapper mapper,
            ISystemSettings<PatSetting> settings,
            IDocumentService docService,
            ISharePointService sharePointService, IOptions<GraphSettings> graphSettings,
            ITradeSecretService tradeSecretService, ClaimsPrincipal user
            )

        {
            _inventionService = inventionService;
            _mapper = mapper;
            _settings = settings;
            _docService = docService;
            _sharePointService = sharePointService;
            _graphSettings = graphSettings.Value;
            _tradeSecretService = tradeSecretService;
            _user = user;
        }

        public IQueryable<Invention> AddCriteria(List<QueryFilterViewModel> mainSearchFilters, IQueryable<Invention> inventions)
        {
            if (mainSearchFilters.Count > 0)
            {                
                var disclosureStatusOp = mainSearchFilters.GetFilterOperator("DisclosureStatusOp");
                var disclosureStatus = mainSearchFilters.FirstOrDefault(f => f.Property == "DisclosureStatus");
                if (disclosureStatus != null)
                {
                    disclosureStatus.Operator = disclosureStatusOp;
                    var disclosureStatuses = disclosureStatus.GetValueList();

                    if (disclosureStatuses.Count > 0)
                    {
                        if (disclosureStatus.Operator == "eq")
                            inventions = inventions.Where(w => disclosureStatuses.Contains(w.DisclosureStatus));
                        else
                            inventions = inventions.Where(w => !disclosureStatuses.Contains(w.DisclosureStatus));

                        mainSearchFilters.Remove(disclosureStatus);
                    }
                }

                //bool IsMultipleOwners = _settings.GetSetting().Result.IsMultipleOwnerOn;

                var ownerCode = mainSearchFilters.FirstOrDefault(f => f.Property == "OwnerCode");
                if (ownerCode != null)
                {
                    var owCodes = ownerCode.GetValueListForLoop();
                    if (owCodes.Count > 0)
                    {                        
                        Expression<Func<PatOwnerInv, bool>> ownerPredicate = (item) => false;
                        foreach (var val in owCodes)
                        {
                            ownerPredicate = ownerPredicate.Or(o => EF.Functions.Like(o.Owner.OwnerCode, val));
                        }
                        var predicate = R10.Core.ExpressionHelper.BuildAnyPredicate<Invention>("Owners", ownerPredicate);
                        inventions = inventions.Where(predicate);
                    }
                    //inventions = inventions.Where(w => w.Owners.Any(a => EF.Functions.Like(a.Owner.OwnerCode, ownerCode.Value)));
                    mainSearchFilters.Remove(ownerCode);
                }

                var ownerName = mainSearchFilters.FirstOrDefault(f => f.Property == "OwnerName");
                if (ownerName != null)
                {
                    var owNames = ownerName.GetValueListForLoop();
                    if (owNames.Count > 0)
                    {                        
                        Expression<Func<PatOwnerInv, bool>> ownerPredicate = (item) => false;
                        foreach (var val in owNames)
                        {
                            ownerPredicate = ownerPredicate.Or(o => EF.Functions.Like(o.Owner.OwnerName, val));
                        }
                        var predicate = R10.Core.ExpressionHelper.BuildAnyPredicate<Invention>("Owners", ownerPredicate);
                        inventions = inventions.Where(predicate);
                    }
                    //inventions = inventions.Where(w => w.Owners.Any(a => EF.Functions.Like(a.Owner.OwnerName, ownerName.Value)));
                    mainSearchFilters.Remove(ownerName);
                }

                var attorneyCode = mainSearchFilters.FirstOrDefault(f => f.Property == "AttorneyCode");
                if (attorneyCode != null)
                {
                    var attyCodes = attorneyCode.GetValueListForLoop();
                    if (attyCodes.Count > 0)
                    {
                        Expression<Func<Invention, bool>> predicate = (item) => false;
                        foreach (var atty in attyCodes)
                        {
                            predicate = predicate.Or(w => EF.Functions.Like(w.Attorney1.AttorneyCode, atty) ||
                                                          EF.Functions.Like(w.Attorney2.AttorneyCode, atty) ||
                                                          EF.Functions.Like(w.Attorney3.AttorneyCode, atty) ||
                                                          EF.Functions.Like(w.Attorney4.AttorneyCode, atty) ||
                                                          EF.Functions.Like(w.Attorney5.AttorneyCode, atty));
                        }                      
                        inventions = inventions.Where(predicate);
                    }
                    //inventions = inventions.Where(w => EF.Functions.Like(w.Attorney1.AttorneyCode, attorneyCode.Value) ||
                    //                                   EF.Functions.Like(w.Attorney2.AttorneyCode, attorneyCode.Value) ||
                    //                                   EF.Functions.Like(w.Attorney3.AttorneyCode, attorneyCode.Value) ||
                    //                                   EF.Functions.Like(w.Attorney4.AttorneyCode, attorneyCode.Value) ||
                    //                                   EF.Functions.Like(w.Attorney5.AttorneyCode, attorneyCode.Value));
                    mainSearchFilters.Remove(attorneyCode);
                }

                var attorneyName = mainSearchFilters.FirstOrDefault(f => f.Property == "AttorneyName");
                if (attorneyName != null)
                {
                    var attyNames = attorneyName.GetValueListForLoop();
                    if (attyNames.Count > 0)
                    {
                        Expression<Func<Invention, bool>> predicate = (item) => false;
                        foreach (var attyName in attyNames)
                        {
                            predicate = predicate.Or(w => EF.Functions.Like(w.Attorney1.AttorneyName, attyName) ||
                                                          EF.Functions.Like(w.Attorney2.AttorneyName, attyName) ||
                                                          EF.Functions.Like(w.Attorney3.AttorneyName, attyName) ||
                                                          EF.Functions.Like(w.Attorney4.AttorneyName, attyName) ||
                                                          EF.Functions.Like(w.Attorney5.AttorneyName, attyName));
                        }
                        inventions = inventions.Where(predicate);
                    }

                    //inventions = inventions.Where(w => EF.Functions.Like(w.Attorney1.AttorneyName, attorneyName.Value) ||
                    //                                    EF.Functions.Like(w.Attorney2.AttorneyName, attorneyName.Value) ||
                    //                                    EF.Functions.Like(w.Attorney3.AttorneyName, attorneyName.Value));
                    mainSearchFilters.Remove(attorneyName);
                }

                var inventor = mainSearchFilters.FirstOrDefault(f => f.Property == "Inventor");
                if (inventor != null)
                {
                    var inventors = inventor.GetValueListForLoop();
                    if (inventors.Count > 0)
                    {
                        Expression<Func<Invention, bool>> predicate = (item) => false;
                        foreach (var invt in inventors)
                        {
                            predicate = predicate.Or(w => w.Inventors.Any(a => EF.Functions.Like(a.InventorInvInventor.Inventor, invt)));
                        }
                        inventions = inventions.Where(predicate);                       
                    }
                    //inventions = inventions.Where(w => w.Inventors.Any(a => EF.Functions.Like(a.InventorInvInventor.Inventor, inventor.Value)));
                    mainSearchFilters.Remove(inventor);
                }

                var priorityCaseTypeOp = mainSearchFilters.GetFilterOperator("PriorityCaseTypeOp");
                var priorityCountryOp = mainSearchFilters.GetFilterOperator("PriorityCountryOp");
                if (mainSearchFilters.FirstOrDefault(f => f.Property.StartsWith("Priorities.")) != null)
                {
                    var priorityCountries = new List<string>();
                    var priorityCaseTypes = new List<string>();

                    var priorityCountry = mainSearchFilters.FirstOrDefault(f => f.Property == "Priorities.Country");
                    if (priorityCountry != null)
                    {
                        priorityCountry.Operator = priorityCountryOp;
                        priorityCountries = priorityCountry.GetValueList();
                        if (priorityCountries.Count == 0) priorityCountries.Add(priorityCountry.Value);
                    }

                    var priorityCaseType = mainSearchFilters.FirstOrDefault(f => f.Property == "Priorities.CaseType");
                    if (priorityCaseType != null)
                    {
                        priorityCaseType.Operator = priorityCaseTypeOp;
                        priorityCaseTypes = priorityCaseType.GetValueList();
                        if (priorityCaseTypes.Count == 0) priorityCaseTypes.Add(priorityCaseType.Value);
                    }
                    var priorityCountryName = mainSearchFilters.FirstOrDefault(f => f.Property == "Priorities.CountryName");
                    var priorityAppNo = mainSearchFilters.FirstOrDefault(f => f.Property == "Priorities.AppNumber");
                    var priorityFilDateFrom = mainSearchFilters.FirstOrDefault(f => f.Property == "Priorities.FilDateFrom");
                    var priorityFilDateTo = mainSearchFilters.FirstOrDefault(f => f.Property == "Priorities.FilDateTo");

                    var priorityAppNoSearch = "";
                    if (priorityAppNo != null)
                    {
                        priorityAppNoSearch = QueryHelper.ExtractSignificantNumbers(priorityAppNo.Value);
                    }

                    inventions = inventions.Where(w => w.Priorities.Any(c => (priorityCountry == null || (priorityCountryOp == "eq" && priorityCountries.Contains(c.Country)) || (priorityCountryOp != "eq" && !priorityCountries.Contains(c.Country))) &&
                                 (priorityCaseType == null || (priorityCaseTypeOp == "eq" && priorityCaseTypes.Contains(c.CaseType)) || (priorityCaseTypeOp != "eq" && !priorityCaseTypes.Contains(c.CaseType))) &&
                                 (priorityCountryName == null || EF.Functions.Like(c.PriorityCountry.CountryName, priorityCountryName.Value)) &&
                                  (priorityAppNo == null || EF.Functions.Like(c.AppNumber, priorityAppNo.Value) || EF.Functions.Like(c.AppNumberSearch, priorityAppNoSearch)) &&
                               (priorityFilDateFrom == null || c.FilDate >= Convert.ToDateTime(priorityFilDateFrom.Value)) &&
                               (priorityFilDateTo == null || c.FilDate <= Convert.ToDateTime(priorityFilDateTo.Value))
                    ));
                    mainSearchFilters.Remove(priorityCountry);
                    mainSearchFilters.Remove(priorityCaseType);
                    mainSearchFilters.Remove(priorityCountryName);
                    mainSearchFilters.Remove(priorityAppNo);
                    mainSearchFilters.Remove(priorityFilDateFrom);
                    mainSearchFilters.Remove(priorityFilDateTo);
                }

                var applicationStatusOp = mainSearchFilters.GetFilterOperator("ApplicationStatusOp");
                if (mainSearchFilters.FirstOrDefault(f => f.Property.StartsWith("CountryApplications.")) != null)
                {                    
                    Expression<Func<CountryApplication, bool>> appPredicate = (item) => false;
                    Expression<Func<CountryApplication, bool>> appDummyPredicate = (item) => false;

                    var applicationCountry = mainSearchFilters.FirstOrDefault(f => f.Property == "CountryApplications.Country");                 
                    if (applicationCountry != null)
                    {
                        var countries = applicationCountry.GetValueListForLoop();                        
                        if (countries.Count > 0)
                        {
                            var countryPredicate = R10.Web.Helpers.ExpressionHelper.BuildLoopNestedPredicate<CountryApplication>("Country", countries, false);
                            if (appPredicate.ToString() == appDummyPredicate.ToString())
                                appPredicate = appPredicate.Or(countryPredicate);
                            else
                                appPredicate = appPredicate.And(countryPredicate);
                        }                        
                    }

                    var applicationCountryName = mainSearchFilters.FirstOrDefault(f => f.Property == "CountryApplications.CountryName");
                    if (applicationCountryName != null)
                    {
                        var countryNames = applicationCountryName.GetValueListForLoop();
                        if (countryNames.Count > 0)
                        {
                            var countryNamePredicate = R10.Web.Helpers.ExpressionHelper.BuildLoopNestedPredicate<CountryApplication>("PatCountry.CountryName", countryNames, false);
                            if (appPredicate.ToString() == appDummyPredicate.ToString())
                                appPredicate = appPredicate.Or(countryNamePredicate);
                            else
                                appPredicate = appPredicate.And(countryNamePredicate);
                        }                        
                    }

                    var applicationStatus = mainSearchFilters.FirstOrDefault(f => f.Property == "CountryApplications.ApplicationStatus");
                    if (applicationStatus != null)
                    {
                        applicationStatus.Operator = applicationStatusOp;
                        var appStatuses = applicationStatus.GetValueListForLoop();
                        if (appStatuses.Count > 0)
                        {
                            Expression<Func<CountryApplication, bool>> appStatusPredicate = app => ((applicationStatus.Operator == "eq" && appStatuses.Contains(app.ApplicationStatus)) 
                                                                                                || (applicationStatus.Operator != "eq" && !appStatuses.Contains(app.ApplicationStatus)));
                            if (appPredicate.ToString() == appDummyPredicate.ToString())
                                appPredicate = appPredicate.Or(appStatusPredicate);
                            else
                                appPredicate = appPredicate.And(appStatusPredicate);
                        }                       
                    }

                    var applicationSubCase = mainSearchFilters.FirstOrDefault(f => f.Property == "CountryApplications.SubCase");
                    var applicationAppNo = mainSearchFilters.FirstOrDefault(f => f.Property == "CountryApplications.AppNumber");
                    var applicationFilDateFrom = mainSearchFilters.FirstOrDefault(f => f.Property == "CountryApplications.FilDateFrom");
                    var applicationFilDateTo = mainSearchFilters.FirstOrDefault(f => f.Property == "CountryApplications.FilDateTo");
                    var applicationPatNo = mainSearchFilters.FirstOrDefault(f => f.Property == "CountryApplications.PatNumber");
                    var applicationIssDateFrom = mainSearchFilters.FirstOrDefault(f => f.Property == "CountryApplications.IssDateFrom");
                    var applicationIssDateTo = mainSearchFilters.FirstOrDefault(f => f.Property == "CountryApplications.IssDateTo");

                    var appNumberSearch = "";
                    if (applicationAppNo != null)
                    {
                        appNumberSearch = QueryHelper.ExtractSignificantNumbers(applicationAppNo.Value);
                    }
                    var patNumberSearch = "";
                    if (applicationPatNo != null)
                    {
                        patNumberSearch = QueryHelper.ExtractSignificantNumbers(applicationPatNo.Value);
                    }

                    Expression<Func<CountryApplication, bool>> appCombinedPredicate = a => (                                                                
                                                                (applicationSubCase == null || EF.Functions.Like(a.SubCase, applicationSubCase.Value)) &&                                                               
                                                                (applicationAppNo == null || EF.Functions.Like(a.AppNumber, applicationAppNo.Value) || EF.Functions.Like(a.AppNumberSearch, appNumberSearch)) &&
                                                                (applicationPatNo == null || EF.Functions.Like(a.PatNumber, applicationPatNo.Value) || EF.Functions.Like(a.PatNumberSearch, patNumberSearch)) &&
                                                                (applicationFilDateFrom == null || a.FilDate >= Convert.ToDateTime(applicationFilDateFrom.Value)) &&
                                                                (applicationFilDateTo == null || a.FilDate <= Convert.ToDateTime(applicationFilDateTo.Value)) &&
                                                                (applicationIssDateFrom == null || a.IssDate >= Convert.ToDateTime(applicationIssDateFrom.Value)) &&
                                                                (applicationIssDateTo == null || a.IssDate <= Convert.ToDateTime(applicationIssDateTo.Value))
                                                            );
                    if (appPredicate.ToString() == appDummyPredicate.ToString())
                        appPredicate = appPredicate.Or(appCombinedPredicate);
                    else
                        appPredicate = appPredicate.And(appCombinedPredicate);


                    var appAnyPredicate = R10.Core.ExpressionHelper.BuildAnyPredicate<Invention>("CountryApplications", appPredicate);
                    inventions = inventions.Where(appAnyPredicate);

                    mainSearchFilters.Remove(applicationCountry);
                    mainSearchFilters.Remove(applicationCountryName);
                    mainSearchFilters.Remove(applicationSubCase);                    
                    mainSearchFilters.Remove(applicationStatus);
                    mainSearchFilters.Remove(applicationAppNo);
                    mainSearchFilters.Remove(applicationFilDateFrom);
                    mainSearchFilters.Remove(applicationFilDateTo);
                    mainSearchFilters.Remove(applicationPatNo);
                    mainSearchFilters.Remove(applicationIssDateFrom);
                    mainSearchFilters.Remove(applicationIssDateTo);
                }

                if (mainSearchFilters.FirstOrDefault(f => f.Property.StartsWith("InventionRelatedDisclosures.")) != null)
                {
                    var relatedDisclosureNumber = mainSearchFilters.FirstOrDefault(f => f.Property == "InventionRelatedDisclosures.DisclosureNumber");
                    var relatedDisclosureRecommendation = mainSearchFilters.FirstOrDefault(f => f.Property == "InventionRelatedDisclosures.Recommendation");
                    var relatedDisclosureTitle = mainSearchFilters.FirstOrDefault(f => f.Property == "InventionRelatedDisclosures.DiscTitle");
                    var relatedDisclosureClient = mainSearchFilters.FirstOrDefault(f => f.Property == "InventionRelatedDisclosures.ClientCode");
                    var relatedDisclosureClientName = mainSearchFilters.FirstOrDefault(f => f.Property == "InventionRelatedDisclosures.ClientName");
                    var relatedDisclosureDisclosureDateFrom = mainSearchFilters.FirstOrDefault(f => f.Property == "InventionRelatedDisclosures.DisclosureDateFrom");
                    var relatedDisclosureDisclosureDateTo = mainSearchFilters.FirstOrDefault(f => f.Property == "InventionRelatedDisclosures.DisclosureDateTo");

                    inventions = inventions.Where(w => w.InventionRelatedDisclosures.Any(a => (relatedDisclosureNumber == null || EF.Functions.Like(a.InventionDisclosure.DisclosureNumber, relatedDisclosureNumber.Value)) &&
                                                (relatedDisclosureRecommendation == null || EF.Functions.Like(a.InventionDisclosure.Recommendation, relatedDisclosureRecommendation.Value)) &&
                                                (relatedDisclosureTitle == null || EF.Functions.Like(a.InventionDisclosure.DisclosureTitle, relatedDisclosureTitle.Value)) &&
                                                (relatedDisclosureClient == null || EF.Functions.Like(a.InventionDisclosure.Client.ClientCode, relatedDisclosureClient.Value)) &&
                                                (relatedDisclosureClientName == null || EF.Functions.Like(a.InventionDisclosure.Client.ClientName, relatedDisclosureClientName.Value)) &&
                                                (relatedDisclosureDisclosureDateFrom == null || a.InventionDisclosure.DisclosureDate >= Convert.ToDateTime(relatedDisclosureDisclosureDateFrom.Value)) &&
                                                (relatedDisclosureDisclosureDateTo == null || a.InventionDisclosure.DisclosureDate <= Convert.ToDateTime(relatedDisclosureDisclosureDateTo.Value))

                    ));

                    mainSearchFilters.Remove(relatedDisclosureNumber);
                    mainSearchFilters.Remove(relatedDisclosureRecommendation);
                    mainSearchFilters.Remove(relatedDisclosureTitle);
                    mainSearchFilters.Remove(relatedDisclosureClient);
                    mainSearchFilters.Remove(relatedDisclosureClientName);
                    mainSearchFilters.Remove(relatedDisclosureDisclosureDateFrom);
                    mainSearchFilters.Remove(relatedDisclosureDisclosureDateTo);
                }


                if (mainSearchFilters.FirstOrDefault(f => f.Property.StartsWith("InventionRelatedInventions.")) != null)
                {
                    var relatedInvCaseNumber = mainSearchFilters.FirstOrDefault(f => f.Property == "InventionRelatedInventions.RelatedCaseNumber");
                    var relatedInvInvTitle = mainSearchFilters.FirstOrDefault(f => f.Property == "InventionRelatedInventions.InvTitle");
                    var relatedInvDisclosureStatus = mainSearchFilters.FirstOrDefault(f => f.Property == "InventionRelatedInventions.DisclosureStatus");
                    var relatedInvDisclosureDateFrom = mainSearchFilters.FirstOrDefault(f => f.Property == "InventionRelatedInventions.DisclosureDateFrom");
                    var relatedInvDisclosureDateTo = mainSearchFilters.FirstOrDefault(f => f.Property == "InventionRelatedInventions.DisclosureDateTo");

                    inventions = inventions.Where(w => w.InventionRelatedInventions.Any(a => (relatedInvCaseNumber == null || EF.Functions.Like(a.RelatedInvention.CaseNumber, relatedInvCaseNumber.Value)) &&
                                    (relatedInvInvTitle == null || EF.Functions.Like(a.RelatedInvention.InvTitle, relatedInvInvTitle.Value)) &&
                                    (relatedInvDisclosureStatus == null || EF.Functions.Like(a.RelatedInvention.DisclosureStatus, relatedInvDisclosureStatus.Value)) &&
                                    (relatedInvDisclosureDateFrom == null || a.RelatedInvention.DisclosureDate >= Convert.ToDateTime(relatedInvDisclosureDateFrom.Value)) &&
                                    (relatedInvDisclosureDateTo == null || a.RelatedInvention.DisclosureDate <= Convert.ToDateTime(relatedInvDisclosureDateTo.Value))
                                  ) ||
                                  w.InventionRelateds.Any(a => (relatedInvCaseNumber == null || EF.Functions.Like(a.Invention.CaseNumber, relatedInvCaseNumber.Value)) &&
                                    (relatedInvInvTitle == null || EF.Functions.Like(a.Invention.InvTitle, relatedInvInvTitle.Value)) &&
                                    (relatedInvDisclosureStatus == null || EF.Functions.Like(a.Invention.DisclosureStatus, relatedInvDisclosureStatus.Value)) &&
                                    (relatedInvDisclosureDateFrom == null || a.Invention.DisclosureDate >= Convert.ToDateTime(relatedInvDisclosureDateFrom.Value)) &&
                                    (relatedInvDisclosureDateTo == null || a.Invention.DisclosureDate <= Convert.ToDateTime(relatedInvDisclosureDateTo.Value))
                                  )
                    );

                    mainSearchFilters.Remove(relatedInvCaseNumber);
                    mainSearchFilters.Remove(relatedInvInvTitle);
                    mainSearchFilters.Remove(relatedInvDisclosureStatus);
                    mainSearchFilters.Remove(relatedInvDisclosureDateFrom);
                    mainSearchFilters.Remove(relatedInvDisclosureDateTo);
                }

                if (mainSearchFilters.FirstOrDefault(f => f.Property.StartsWith("Images.")) != null)
                {
                    var docName = mainSearchFilters.FirstOrDefault(f => f.Property == "Images.DocName");
                    var tag = mainSearchFilters.FirstOrDefault(f => f.Property == "Images.Tag");
                    var dateCreatedFrom = mainSearchFilters.FirstOrDefault(f => f.Property == "Images.DateCreatedFrom");
                    var dateCreatedTo = mainSearchFilters.FirstOrDefault(f => f.Property == "Images.DateCreatedTo");

                    var settings = _settings.GetSetting().GetAwaiter().GetResult();
                    if (settings.IsSharePointIntegrationOn && settings.IsSharePointListRealTime)
                    {
                        var graphClient = _sharePointService.GetGraphClient();
                        var docs = new List<SharePointGraphDocPicklistViewModel>();

                        if (settings.IsSharePointIntegrationByMetadataOn)
                            docs =  graphClient.GetSiteDocumentNamesByMetadata(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, SharePointDocLibrary.Patent, SharePointDocLibraryFolder.Invention, docName != null ? docName.Value : "").GetAwaiter().GetResult();
                        else
                            docs = graphClient.GetSiteDocumentNames(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, SharePointDocLibrary.Patent, new List<string> { SharePointDocLibraryFolder.Invention }, docName != null ? docName.Value : "").GetAwaiter().GetResult();
                        

                        if (dateCreatedFrom != null) {
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
                                if (!settings.IsSharePointIntegrationByMetadataOn) {
                                    var recKey = d.Folder.Split("/")[1];
                                    d.RecKey = recKey;
                                }
                            });

                            var recKeys = docs.Select(d => d.RecKey).ToList();
                            inventions = inventions.Where(i => recKeys.Contains(i.CaseNumber));
                        }
                        else {
                            inventions = inventions.Where(i => false);
                        }
                    }
                    else {
                        inventions = inventions.Where(a =>
                          _docService.DocDocuments.Any(d =>
                              (d.DocFolder.SystemType == SystemTypeCode.Patent && d.DocFolder.DataKey == "InvId" && d.DocFolder.DataKeyValue == a.InvId) &&
                              (docName == null || EF.Functions.Like(d.DocName, docName.Value)) &&
                              (tag == null || d.DocDocumentTags.Any(t=> EF.Functions.Like(t.Tag, tag.Value))) &&
                              (dateCreatedFrom == null || d.DateCreated >= Convert.ToDateTime(dateCreatedFrom.Value)) &&
                              (dateCreatedTo == null || d.DateCreated <= Convert.ToDateTime(dateCreatedTo.Value))
                          )
                    );
                    }

                    mainSearchFilters.Remove(docName);
                    mainSearchFilters.Remove(tag);
                    mainSearchFilters.Remove(dateCreatedFrom);
                    mainSearchFilters.Remove(dateCreatedTo);
                }

                if (mainSearchFilters.FirstOrDefault(f => f.Property.StartsWith("Product.")) != null)
                {
                    bool isInventionProductOn = _settings.GetSetting().Result.IsInventionProductOn;
                    if (isInventionProductOn)
                    {
                        Expression<Func<PatProductInv, bool>> prodPredicate = (item) => false;
                        Expression<Func<PatProductInv, bool>> prodDummyPredicate = (item) => false;

                        var productName = mainSearchFilters.FirstOrDefault(f => f.Property == "Product.ProductName");
                        if (productName != null)
                        {
                            var productNames = productName.GetValueListForLoop();
                            if (productNames.Count > 0)
                            {
                                var productNamePredicate = R10.Web.Helpers.ExpressionHelper.BuildLoopNestedPredicate<PatProductInv>("Product.ProductName", productNames, false);
                                if (prodPredicate.ToString() == prodDummyPredicate.ToString())
                                    prodPredicate = prodPredicate.Or(productNamePredicate);
                                else
                                    prodPredicate = prodPredicate.And(productNamePredicate);
                            }
                        }

                        var productCategory = mainSearchFilters.FirstOrDefault(f => f.Property == "Product.ProductCategory");
                        if (productCategory != null)
                        {
                            var productCategories = productCategory.GetValueListForLoop();
                            if (productCategories.Count > 0)
                            {
                                var productCategoryPredicate = R10.Web.Helpers.ExpressionHelper.BuildLoopNestedPredicate<PatProductInv>("Product.ProductCategory", productCategories, false);
                                if (prodPredicate.ToString() == prodDummyPredicate.ToString())
                                    prodPredicate = prodPredicate.Or(productCategoryPredicate);
                                else
                                    prodPredicate = prodPredicate.And(productCategoryPredicate);
                            }
                        }

                        var prodAnyPredicate = R10.Core.ExpressionHelper.BuildAnyPredicate<Invention>("Products", prodPredicate);

                        inventions = inventions.Where(prodAnyPredicate);

                        //inventions = inventions.Where(a => 
                        //                (productName == null || a.CountryApplications.Any(ctryApp => ctryApp.Products.Any(ap => EF.Functions.Like(ap.Product.ProductName, productName.Value)))) 
                        //             && (productCategory == null || a.CountryApplications.Any(ctryApp => ctryApp.Products.Any(ap => EF.Functions.Like(ap.Product.ProductCategory, productCategory.Value))))
                        //        );

                        mainSearchFilters.Remove(productName);
                        mainSearchFilters.Remove(productCategory);
                    }
                    else
                    {
                        Expression<Func<PatProduct, bool>> prodPredicate = (item) => false;
                        Expression<Func<PatProduct, bool>> prodDummyPredicate = (item) => false;

                        var productName = mainSearchFilters.FirstOrDefault(f => f.Property == "Product.ProductName");
                        if (productName != null)
                        {
                            var productNames = productName.GetValueListForLoop();
                            if (productNames.Count > 0)
                            {
                                var productNamePredicate = R10.Web.Helpers.ExpressionHelper.BuildLoopNestedPredicate<PatProduct>("Product.ProductName", productNames, false);
                                if (prodPredicate.ToString() == prodDummyPredicate.ToString())
                                    prodPredicate = prodPredicate.Or(productNamePredicate);
                                else
                                    prodPredicate = prodPredicate.And(productNamePredicate);
                            }
                        }

                        var productCategory = mainSearchFilters.FirstOrDefault(f => f.Property == "Product.ProductCategory");
                        if (productCategory != null)
                        {
                            var productCategories = productCategory.GetValueListForLoop();
                            if (productCategories.Count > 0)
                            {
                                var productCategoryPredicate = R10.Web.Helpers.ExpressionHelper.BuildLoopNestedPredicate<PatProduct>("Product.ProductCategory", productCategories, false);
                                if (prodPredicate.ToString() == prodDummyPredicate.ToString())
                                    prodPredicate = prodPredicate.Or(productCategoryPredicate);
                                else
                                    prodPredicate = prodPredicate.And(productCategoryPredicate);
                            }
                        }

                        var prodAnyPredicate = R10.Core.ExpressionHelper.BuildAnyPredicate<CountryApplication>("Products", prodPredicate);
                        var appAnyPredicate = R10.Core.ExpressionHelper.BuildAnyPredicate<Invention>("CountryApplications", prodAnyPredicate);

                        inventions = inventions.Where(appAnyPredicate);

                        //inventions = inventions.Where(a => 
                        //                (productName == null || a.CountryApplications.Any(ctryApp => ctryApp.Products.Any(ap => EF.Functions.Like(ap.Product.ProductName, productName.Value)))) 
                        //             && (productCategory == null || a.CountryApplications.Any(ctryApp => ctryApp.Products.Any(ap => EF.Functions.Like(ap.Product.ProductCategory, productCategory.Value))))
                        //        );

                        mainSearchFilters.Remove(productName);
                        mainSearchFilters.Remove(productCategory);
                    }
                    
                }

                var noofOwners = mainSearchFilters.FirstOrDefault(f => f.Property == "NoOfOwners");
                if (noofOwners != null)
                {
                    if (noofOwners.Value == "s")
                    {
                        inventions = inventions.Where(i => i.Owners != null && i.Owners.Count == 1);
                    }
                    else
                    {
                        inventions = inventions.Where(i => i.Owners != null && i.Owners.Count > 1);
                    }
                    mainSearchFilters.Remove(noofOwners);
                }

                //Using IsTradeSecret criteria name will cause "label for" conflict with detail screen IsTradeSecret control
                var tradeSecretOnly = mainSearchFilters.FirstOrDefault(f => f.Property == "TradeSecretOnly");
                if (tradeSecretOnly != null)
                {
                    inventions = inventions.Where(i => (i.IsTradeSecret ?? false));
                    mainSearchFilters.Remove(tradeSecretOnly);
                }

                if (mainSearchFilters.Any())
                    inventions = QueryHelper.BuildCriteria<Invention>(inventions, mainSearchFilters);

            }
            return inventions;
        }

        public async Task<CPiDataSourceResult> CreateViewModelForGrid(DataSourceRequest request, IQueryable<Invention> inventions)
        {
            int[]? ids;
            var data = new List<InventionSearchResultViewModel>();
            var model = inventions.ProjectTo<InventionSearchResultViewModel>();

            if (request.Sorts != null && request.Sorts.Any())
                model = model.ApplySorting(request.Sorts);
            else
                model = model.OrderBy(i => i.CaseNumber);

            ids = await model.Select(i => i.InvId).ToArrayAsync();
            model = model.ApplyPaging(request.Page, request.PageSize);
            data = await model.ToListAsync();

            var settings = await _settings.GetSetting();
            if (settings.IsSharePointIntegrationOn) {
                data.ForEach(i=> { i.SharePointRecKey = i.CaseNumber; i.ThumbnailFile = "x"; });
            }

            // check trade secret
            if (data.Any(i => i.IsTradeSecret ?? false))
            {
                var tsInvIds = data.Where(ca => ca.IsTradeSecret ?? false).Select(ca => ca.InvId).ToList();
                var tsInventions = await _inventionService.Inventions.Include(i => i.TradeSecretRequests).Where(i => (i.IsTradeSecret ?? false) && tsInvIds.Contains(i.InvId)).ToListAsync();
                foreach (var item in data.Where(i => (i.IsTradeSecret ?? false)))
                {
                    var tsInvention = tsInventions.SingleOrDefault(ts => ts.InvId == item.InvId);
                    var tsRequest = tsInvention?.TradeSecretRequests?.Where(ts => ts.UserId == _user.GetUserIdentifier()).OrderByDescending(ts => ts.RequestDate).FirstOrDefault();
                    if (tsRequest != null && tsRequest.IsCleared && item.TradeSecret != null)
                    {
                        // show trade secret if last request is cleared
                        item.RestoreTradeSecret(item.TradeSecret, true);
                        await _tradeSecretService.LogActivity(TradeSecretScreen.InventionSearch, TradeSecretScreen.Invention, item.InvId, TradeSecretActivityCode.View, tsRequest.RequestId);
                    }
                    else
                    {
                        // hide image if not cleared
                        item.ImageFile = null;
                        item.ThumbnailFile = null;
                        item.SharePointRecKey = null;
                        item.ThumbnailUrl = null;

                        // log redacted view
                        await _tradeSecretService.LogActivity(TradeSecretScreen.InventionSearch, TradeSecretScreen.Invention, item.InvId, TradeSecretActivityCode.RedactedView, 0);
                    }
                }
            }

            return new CPiDataSourceResult()
            {
                Data = data,
                Total = ids.Length,
                Ids = ids
            };
        }

        public Invention ConvertViewModelToInvention(InventionDetailViewModel viewModel)
        {
            var invention = _mapper.Map<Invention>(viewModel);
            return invention;
        }

        public async Task<InventionDetailViewModel> CreateViewModelForDetailScreen(int id)
        {
            var viewModel = new InventionDetailViewModel();

            if (id > 0)
            {
                viewModel = await _inventionService.QueryableList.ProjectTo<InventionDetailViewModel>()
                    .SingleOrDefaultAsync(i => i.InvId == id);

                if (viewModel == null)
                    return viewModel;

                viewModel.CanModifyAttorney1 = await _inventionService.CanModifyAttorney(viewModel.Attorney1ID ?? 0);
                viewModel.CanModifyAttorney2 = await _inventionService.CanModifyAttorney(viewModel.Attorney2ID ?? 0);
                viewModel.CanModifyAttorney3 = await _inventionService.CanModifyAttorney(viewModel.Attorney3ID ?? 0);
                viewModel.CanModifyAttorney4 = await _inventionService.CanModifyAttorney(viewModel.Attorney4ID ?? 0);
                viewModel.CanModifyAttorney5 = await _inventionService.CanModifyAttorney(viewModel.Attorney5ID ?? 0);

                bool IsInventorRemunerationOn = _settings.GetSetting().Result.IsInventorRemunerationOn;
                bool IsInventorFRRemunerationOn = _settings.GetSetting().Result.IsInventorFRRemunerationOn;
                if (IsInventorRemunerationOn && IsInventorFRRemunerationOn)
                {
                    viewModel.Remuneration = "N/A";
                    if (viewModel.UseInventorRemuneration) viewModel.Remuneration = "German";
                    if (viewModel.UseInventorFRRemuneration) viewModel.Remuneration = "French";
                }
            }

            viewModel.IsOwnerRequired = _inventionService.IsOwnerRequired;
            viewModel.IsInventorRequired = _inventionService.IsInventorRequired;

            return viewModel;
        }

        public IQueryable<CaseNumberLookupViewModel> GetCaseNumbersList(IQueryable<Invention> inventions, DataSourceRequest request, string textProperty, string text, FilterType filterType)
        {
            if (request.Filters?.Count > 0)
            {
                text = ((FilterDescriptor)request.Filters[0]).Value as string;
            }

            inventions = QueryHelper.BuildCriteria(inventions, textProperty, text, filterType);
            var result = inventions.Select(i => new CaseNumberLookupViewModel { Id = i.InvId, CaseNumber = i.CaseNumber }).OrderBy(i => i.CaseNumber);
            return result;
        }

        public async Task<CaseNumberLookupViewModel> CaseNumberSearchValueMapper(IQueryable<Invention> inventions, string value)
        {
            var result = await inventions.Where(i => i.CaseNumber == value)
                .Select(i => new CaseNumberLookupViewModel { Id = i.InvId, CaseNumber = i.CaseNumber }).FirstOrDefaultAsync();
            return result;
        }

        public async Task<TitleLookupViewModel> TitleSearchValueMapper(IQueryable<Invention> inventions, string value)
        {
            var result = await inventions.Where(i => i.InvTitle == value)
                .Select(i => new TitleLookupViewModel { Id = i.InvId, InvTitle = i.InvTitle }).FirstOrDefaultAsync();
            return result;
        }

        public async Task ApplyDetailPageTradeSecretPermission(DetailPageViewModel<InventionDetailViewModel> viewModel)
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
                    await _tradeSecretService.LogActivity(TradeSecretScreen.Invention, TradeSecretScreen.Invention, viewModel.Detail.InvId, TradeSecretActivityCode.View, viewModel.TradeSecretUserRequest.RequestId);
                }
                else if (viewModel.IsTradeSecret)
                {
                    // log redacted view
                    await _tradeSecretService.LogActivity(TradeSecretScreen.Invention, TradeSecretScreen.Invention, viewModel.Detail.InvId, TradeSecretActivityCode.RedactedView, 0);
                }

                viewModel.ShowTradeSecretRequest = viewModel.IsTradeSecret;
            }

            viewModel.CanCopyRecord = !viewModel.IsTradeSecret;
        }
    }
}
