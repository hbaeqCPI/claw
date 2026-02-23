using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Presentation;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using R10.Core;
using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Entities.DMS;
using R10.Core.Entities.GeneralMatter;
using R10.Core.Entities.Patent;
using R10.Core.Entities.ReportScheduler;
using R10.Core.Entities.Trademark;
using R10.Core.Helpers;
using R10.Core.Identity;
using R10.Core.Interfaces;
using R10.Core.Interfaces.DMS;
using R10.Core.Interfaces.Patent;
using R10.Web.Areas.Patent.ViewModels;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;
using R10.Web.Extensions.ActionResults;
using R10.Web.Helpers;
using R10.Web.Interfaces;
using R10.Web.Models;
using R10.Web.Models.DashboardViewModels;
using R10.Web.Models.PageViewModels;
using R10.Web.Security;
using R10.Web.Services.DocumentSearch;
using R10.Web.Services.DocumentStorage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace R10.Web.Areas.Shared.Controllers 
{
    [Area("Shared"), Authorize(Policy = SharedAuthorizationPolicy.CanAccessGlobalSearch)]
    public class GlobalSearchController : Microsoft.AspNetCore.Mvc.Controller
    {
        private readonly AzureSearch _azureSearch;
        private readonly IDocumentStorage _documentStorageService;
        private readonly IGlobalSearchService _globalSearchService;
        private readonly IGlobalSearchViewModelService _globalSearchViewModelService;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly IAuthorizationService _authService;
        private readonly IInventionService _inventionService;
        private readonly ICountryApplicationService _countryApplicationService;
        private readonly ITmkTrademarkService _trademarkService;
        private readonly IGMMatterService _gmMatterService;
        private readonly IDisclosureService _disclosureService;

        private readonly string pageTitle = "Global Search";
        private const string BASICSEARCH = "b";

        public GlobalSearchController(
                AzureSearch azureSearch,
                IDocumentStorage documentStorageService,
                IGlobalSearchService globalSearchService,
                IGlobalSearchViewModelService globalSearchViewModelService,
                IStringLocalizer<SharedResource> localizer,
                IAuthorizationService authService,
                IInventionService inventionService,
                ICountryApplicationService countryApplicationService,
                ITmkTrademarkService trademarkService,
                IGMMatterService gmMatterService,
                IDisclosureService disclosureService
            )
        {
            _azureSearch = azureSearch;
            _documentStorageService = documentStorageService;
            _globalSearchService = globalSearchService;
            _globalSearchViewModelService = globalSearchViewModelService;
            _localizer = localizer;
            _authService = authService;

            _inventionService = inventionService;
            _countryApplicationService = countryApplicationService;
            _trademarkService = trademarkService;
            _gmMatterService = gmMatterService;
            _disclosureService = disclosureService;
        }

        public async Task<IActionResult> Index()
        {
            return await Startup(string.Empty);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(string SearchStr )
        {
            return await Startup(SearchStr, true);
        }

        private async Task<IActionResult> Startup(string searchString, bool fromSrchBox = false)
        {
            //var systemTypes = User.GetSystemTypes();
            var systemTypes = await GetUserSystemTypes();

            var userType = User.GetUserType();
            if (userType == CPiUserType.Inventor || userType == CPiUserType.ContactPerson)
            {
                systemTypes.RemoveAll(d => d.SystemId == SystemType.Shared);

                if (!((await _authService.AuthorizeAsync(User, PatentAuthorizationPolicy.CanAccessSystem)).Succeeded))
                    systemTypes.RemoveAll(d => d.SystemId == SystemType.Patent);
            }

            var model = new GlobalSearchPageViewModel()
            {
                Title = _localizer[pageTitle],
                SystemScreens = await _globalSearchViewModelService.GetSystemScreens(systemTypes),
                DataFilterFieldList = await _globalSearchViewModelService.GetFieldList(systemTypes, false),
                DocFilterFieldList = await _globalSearchViewModelService.GetFieldList(systemTypes, true),
                LogicalOperators = GetLogicalOperators(),
                DocSearchMode = GetDocSearchMode(),
                DocQueryType = GetDocQueryType()
            };

            //Coming from GS search box on top nav bar
            var defaultDataFilterFieldlist = await _globalSearchViewModelService.GetFieldList(systemTypes, false, true);
            if (fromSrchBox && !string.IsNullOrEmpty(searchString) && defaultDataFilterFieldlist != null && defaultDataFilterFieldlist.Count > 0)
            {
                var validScreens = new List<string>() { 
                    ScreenCode.Invention.ToLower(), 
                    ScreenCode.Application.ToLower(), 
                    ScreenCode.Trademark.ToLower(), 
                    ScreenCode.GeneralMatter.ToLower(), 
                    //ScreenCode.DMS.ToLower() 
                };

                if (userType == CPiUserType.Inventor || (systemTypes.Count == 1 && systemTypes.Any(d => d.SystemId == SystemType.DMS)))
                    validScreens.Add(ScreenCode.DMS.ToLower());

                var screenList = model.SystemScreens.SelectMany(d => d.Screens)
                                        .Where(s => !string.IsNullOrEmpty(s.Value) && validScreens.Contains(s.Value.ToLower()))
                                        .Select(d => !string.IsNullOrEmpty(d.Value) ? d.Value.ToLower() : "").ToList();
                
                var trimmedStr = searchString.Trim();
                var recordFound = new List<CaseListViewModel>();                                
                var fieldIds = defaultDataFilterFieldlist.Select(d => d.FieldId).Distinct().ToList();
                var searchFields = await _globalSearchService.GSFields.AsNoTracking().Where(d => fieldIds.Contains(d.FieldId))
                                                .Select(d => new { d.FieldName, d.GSScreen.ScreenCode }).Distinct().ToListAsync();

                foreach (var screen in screenList)
                {   
                    if (screen == ScreenCode.Invention.ToLower())
                    {
                        var filteredFields = searchFields.Where(d => typeof(Invention).GetProperty(d.FieldName) != null && d.ScreenCode.ToLower() == screen)
                                                .Select(d => new QueryFilterViewModel()
                                                {
                                                    Property = d.FieldName,
                                                    Value = "%" + trimmedStr + "%",
                                                    Operator = "like"
                                                })
                                                .ToList();

                        if (filteredFields == null || filteredFields.Count == 0) continue;                        

                        recordFound.AddRange(await QueryHelper.BuildOrCriteria<Invention>(_inventionService.QueryableList, filteredFields)
                                                    .Select(d => new CaseListViewModel()
                                                    {
                                                        Id = d.InvId,
                                                        IdType = "InvId",
                                                        System = ScreenCode.Invention,
                                                        Action = Url.Action("Detail", "Invention", new { area = "Patent", id = d.InvId })
                                                    }).Distinct().ToListAsync());
                    }
                    else if (screen == ScreenCode.Application.ToLower())
                    {
                        var filteredFields = searchFields.Where(d => typeof(CountryApplication).GetProperty(d.FieldName) != null && d.ScreenCode.ToLower() == screen)
                                                .Select(d => new QueryFilterViewModel()
                                                {
                                                    Property = d.FieldName,
                                                    Value = "%" + trimmedStr + "%",
                                                    Operator = "like"
                                                })
                                                .ToList();

                        if (filteredFields == null || filteredFields.Count == 0) continue;

                        recordFound.AddRange(await QueryHelper.BuildOrCriteria<CountryApplication>(_countryApplicationService.CountryApplications, filteredFields)
                                                    .Select(d => new CaseListViewModel()
                                                    {
                                                        Id = d.AppId,
                                                        IdType = "AppId",
                                                        System = ScreenCode.Application,
                                                        Action = Url.Action("Detail", "CountryApplication", new { area = "Patent", id = d.AppId }),
                                                        IsNew = d.InvId
                                                    }).Distinct().ToListAsync());                        
                    }                        
                    else if (screen == ScreenCode.Trademark.ToLower())
                    {
                        var filteredFields = searchFields.Where(d => typeof(TmkTrademark).GetProperty(d.FieldName) != null && d.ScreenCode.ToLower() == screen)
                                                .Select(d => new QueryFilterViewModel()
                                                {
                                                    Property = d.FieldName,
                                                    Value = "%" + trimmedStr + "%",
                                                    Operator = "like"
                                                })
                                                .ToList();

                        if (filteredFields == null || filteredFields.Count == 0) continue;

                        recordFound.AddRange(await QueryHelper.BuildOrCriteria<TmkTrademark>(_trademarkService.TmkTrademarks, filteredFields)
                                                .Select(d => new CaseListViewModel()
                                                {
                                                    Id = d.TmkId,
                                                    IdType = "TmkId",
                                                    System = ScreenCode.Trademark,
                                                    Action = Url.Action("Detail", "TmkTrademark", new { area = "Trademark", id = d.TmkId })
                                                }).Distinct().ToListAsync());
                    }                        
                    else if (screen == ScreenCode.GeneralMatter.ToLower())
                    {
                        var filteredFields = searchFields.Where(d => typeof(GMMatter).GetProperty(d.FieldName) != null && d.ScreenCode.ToLower() == screen)
                                                .Select(d => new QueryFilterViewModel()
                                                {
                                                    Property = d.FieldName,
                                                    Value = "%" + trimmedStr + "%",
                                                    Operator = "like"
                                                })
                                                .ToList();

                        if (filteredFields == null || filteredFields.Count == 0) continue;

                        recordFound.AddRange(await QueryHelper.BuildOrCriteria<GMMatter>(_gmMatterService.QueryableList, filteredFields)
                                                .Select(d => new CaseListViewModel()
                                                {
                                                    Id = d.MatId,
                                                    IdType = "MatId",
                                                    System = ScreenCode.GeneralMatter,
                                                    Action = Url.Action("Detail", "Matter", new { area = "GeneralMatter", id = d.MatId })
                                                }).Distinct().ToListAsync());
                    }
                    else if (screen == ScreenCode.DMS.ToLower())
                    {
                        var filteredFields = searchFields.Where(d => typeof(Disclosure).GetProperty(d.FieldName) != null && d.ScreenCode.ToLower() == screen)
                                                .Select(d => new QueryFilterViewModel()
                                                {
                                                    Property = d.FieldName,
                                                    Value = "%" + trimmedStr + "%",
                                                    Operator = "like"
                                                })
                                                .ToList();

                        if (filteredFields == null || filteredFields.Count == 0) continue;

                        recordFound.AddRange(await QueryHelper.BuildOrCriteria<Disclosure>(_disclosureService.QueryableList, filteredFields)
                                                .Select(d => new CaseListViewModel()
                                                {
                                                    Id = d.DMSId,
                                                    IdType = "DMSId",
                                                    System = ScreenCode.DMS,
                                                    Action = Url.Action("Detail", "Disclosure", new { area = "DMS", id = d.DMSId })
                                                }).Distinct().ToListAsync());
                    }
                }

                if (recordFound != null)
                {
                    if (recordFound.Count > 1)
                    {
                        //Multiple records found
                        var screensFound = recordFound.Select(d => d.System).Distinct().ToList();

                        if (screensFound.Count > 1)
                        {
                            //Special handling for 1 invention with 1 or multiple ctry app
                            if (screensFound.Count == 2 
                                && screensFound.Contains(ScreenCode.Invention) 
                                && screensFound.Contains(ScreenCode.Application) 
                                && recordFound.Count(d => d.System == ScreenCode.Invention) == 1)
                            {
                                var invIdFound = recordFound.Where(d => d.System == ScreenCode.Invention).Select(d => d.Id).FirstOrDefault();
                                //Check to make sure there is no CtryApp with different InvId
                                if (!recordFound.Any(d => d.System == ScreenCode.Application && d.IsNew != invIdFound))
                                {
                                    //Go to CtryApp search screen or detail screen
                                    var ctryAppList = recordFound.Where(d => d.System == ScreenCode.Application).ToList();
                                    if (ctryAppList.Count == 1)
                                    {
                                        //Go to CtryApp detail screen
                                        var ctryApp = ctryAppList.FirstOrDefault();
                                        if (ctryApp != null && !string.IsNullOrEmpty(ctryApp.Action))
                                            return Redirect(ctryApp.Action);
                                    }
                                    else if (ctryAppList.Count > 1)
                                    {
                                        //Go to CtryApp search result screen
                                        var redirectUrl = Url.Action("Search", "CountryApplication", new { area = "Patent" });
                                        var queryFilters = searchFields.Where(d => d.ScreenCode.ToLower() == ScreenCode.Application.ToLower() && typeof(CountryApplication).GetProperty(d.FieldName) != null).Select(d => d.FieldName).ToList();
                                        if (!string.IsNullOrEmpty(redirectUrl))
                                        {
                                            HttpContext.Session.SetString("GSSearchStr", trimmedStr + "," + string.Join("|", queryFilters));
                                            HttpContext.Session.SetString("DoNotLoadSavedCriteria", "1");
                                            return Redirect(redirectUrl);
                                        }
                                    }
                                }
                            }

                            //Use global search if found in more than 1 system                            
                            defaultDataFilterFieldlist.ForEach(d => { d.FieldValue = searchString; });
                            ViewData["defaultDataFieldList"] = defaultDataFilterFieldlist;
                        }
                        else if (screensFound.Count == 1)
                        {
                            //Redirect to search result screen if found in only 1 system
                            var screenFound = screensFound.FirstOrDefault();
                            if (!string.IsNullOrEmpty(screenFound))
                            {
                                var redirectUrl = string.Empty;    
                                var queryFilters = new List<string>();
                                switch(screenFound)
                                {
                                    case ScreenCode.Invention:                                        
                                        redirectUrl = Url.Action("Search", "Invention", new { area = "Patent" });
                                        queryFilters = searchFields.Where(d => d.ScreenCode.ToLower() == screenFound.ToLower() && typeof(Invention).GetProperty(d.FieldName) != null).Select(d => d.FieldName).ToList();
                                        break;
                                    case ScreenCode.Application:                                        
                                        redirectUrl = Url.Action("Search", "CountryApplication", new { area = "Patent" });
                                        queryFilters = searchFields.Where(d => d.ScreenCode.ToLower() == screenFound.ToLower() && typeof(CountryApplication).GetProperty(d.FieldName) != null).Select(d => d.FieldName).ToList();
                                        break;
                                    case ScreenCode.Trademark:                                        
                                        redirectUrl = Url.Action("Search", "TmkTrademark", new { area = "Trademark" });
                                        queryFilters = searchFields.Where(d => d.ScreenCode.ToLower() == screenFound.ToLower() && typeof(TmkTrademark).GetProperty(d.FieldName) != null).Select(d => d.FieldName).ToList();
                                        break;
                                    case ScreenCode.GeneralMatter:                                        
                                        redirectUrl = Url.Action("Search", "Matter", new { area = "GeneralMatter" });
                                        queryFilters = searchFields.Where(d => d.ScreenCode.ToLower() == screenFound.ToLower() && typeof(GMMatter).GetProperty(d.FieldName) != null).Select(d => d.FieldName).ToList();
                                        break;
                                    case ScreenCode.DMS:
                                        redirectUrl = Url.Action("Search", "Disclosure", new { area = "DMS" });
                                        queryFilters = searchFields.Where(d => d.ScreenCode.ToLower() == screenFound.ToLower() && typeof(Disclosure).GetProperty(d.FieldName) != null).Select(d => d.FieldName).ToList();
                                        break;
                                    default:
                                        break;
                                }                               

                                if (!string.IsNullOrEmpty(redirectUrl))
                                {
                                    HttpContext.Session.SetString("GSSearchStr", trimmedStr + "," + string.Join("|", queryFilters));
                                    HttpContext.Session.SetString("DoNotLoadSavedCriteria", "1");
                                    return Redirect(redirectUrl);
                                }
                            }
                        }
                    }
                    else if (recordFound.Count == 1)
                    {
                        //1 record found - redirect to detail screen
                        var foundRecord = recordFound.FirstOrDefault();
                        if (foundRecord != null && !string.IsNullOrEmpty(foundRecord.Action))
                            return Redirect(foundRecord.Action);
                            
                    }
                }
            }

            ViewData["SearchString"] = searchString;

            if (Request.IsAjax())
                return PartialView("Index", model);

            return View(model);
        }
               
        public IActionResult OpenHelp()
        {
            return PartialView("_GlobalSearchHelp");
        }

        private List<LookupDTO> GetLogicalOperators()
        {
            var logOpList = new List<LookupDTO>
            {
                new LookupDTO  { Value = "NOT", Text = "NOT" },
                new LookupDTO  { Value = "AND", Text = "AND" },
                new LookupDTO  { Value = "OR", Text ="OR" },
                new LookupDTO  { Value = "NOT AND", Text = "NOT AND" },
                new LookupDTO  { Value = "NOT OR", Text = "NOT OR"}
            };
            return logOpList;
        }

        private List<LookupDTO> GetDocSearchMode()
        {
            var searchModeList = new List<LookupDTO>
            {
                new LookupDTO  { Value = "Any", Text = "Any" },
                new LookupDTO  { Value = "All", Text = "All" }
            };
            return searchModeList;
        }

        private List<LookupDTO> GetDocQueryType()
        {
            var searchModeList = new List<LookupDTO>
            {
                new LookupDTO  { Value = "Simple", Text = "Simple" },
                new LookupDTO  { Value = "Full", Text = "Full" }
            };
            return searchModeList;
        }

        public async Task<IActionResult> GridDataFilterRead([DataSourceRequest] DataSourceRequest request, bool isLoadAll = false)
        {
            var result = new List<GSDataCriteriaViewModel>();
            if (isLoadAll)
            {
                //var systemTypes = User.GetSystemTypes();
                var systemTypes = await GetUserSystemTypes();
                var userType = User.GetUserType();
                if (userType == CPiUserType.Inventor || userType == CPiUserType.ContactPerson)
                {
                    systemTypes.RemoveAll(d => d.SystemId == SystemType.Shared);

                    if (!((await _authService.AuthorizeAsync(User, PatentAuthorizationPolicy.CanAccessSystem)).Succeeded))
                        systemTypes.RemoveAll(d => d.SystemId == SystemType.Patent);
                }
                result = await _globalSearchViewModelService.GetFieldDataSource(systemTypes, isLoadAll);
            }                

            return Json(result.ToDataSourceResult(request));
        }

        public async Task<IActionResult> GridDocFilterRead([DataSourceRequest] DataSourceRequest request, bool isLoadAll = false)
        {
            var result = new List<GSDocCriteriaViewModel>();
            if (isLoadAll)
            {
                //var systemTypes = User.GetSystemTypes();
                var systemTypes = await GetUserSystemTypes();
                var userType = User.GetUserType();
                if (userType == CPiUserType.Inventor || userType == CPiUserType.ContactPerson)
                {
                    systemTypes.RemoveAll(d => d.SystemId == SystemType.Shared);

                    if (!((await _authService.AuthorizeAsync(User, PatentAuthorizationPolicy.CanAccessSystem)).Succeeded))
                        systemTypes.RemoveAll(d => d.SystemId == SystemType.Patent);
                }
                result = await _globalSearchViewModelService.GetDocDataSource(systemTypes, isLoadAll);
            }            

            return Json(result.ToDataSourceResult(request));
        }

        private async Task CheckCustomQueryParam(GSParamDTO objParam)
        {
            var validOp = new List<string>() { "or", "and", "and not", "or not" };
            var errorMsg = _localizer["Query String - Invalid data: "].ToString();
            var customSQLFilter = objParam.MoreFilters.Where(d => d.FieldName.ToLower() == "customsql").FirstOrDefault();
            //Get available GSFields
            var availableFields = await _globalSearchService.GSFields.Where(d => d.IsEnabled)
                                    .OrderBy(o => o.EntryOrder)
                                    .Select(d => new
                                    {
                                        d.FieldId,
                                        d.FieldName,
                                        d.FieldLabelLong,
                                        TablesAlias = d.GSTable.TableAlias,
                                        d.ScreenId,
                                        ScreenName = d.GSScreen.ScreenName,
                                        d.SQLWhere
                                    })
                                    .ToListAsync();

            var sqlFilterList = new List<GSMoreFilter>();

            //Each Screen/Table/Group must be wrapped inside curly brackets
            //Pattern: {([Test Field1]: "Value1") OR ([Test Field2]: "Value2")}

            //Must be wrapped inside parenthesis
            //Pattern: ([Test Field1]: "Value1")
            //Regex: \([^(^)]*\)

            //Pattern: [Test Field1]: "Value1"
            //Regex: \[([^"]+)\]+:+([\s]|)+\"([^"]+)\"

            //Get matching groups
            var groupList = Regex.Matches(customSQLFilter.FieldValue, @"\{[^{^}]*\}").Select(d => d.Value).ToList();

            if (!groupList.Any()) throw new Exception(_localizer["Query String - Missing curly bracket(s)"].ToString());

            int screenId;
            var fieldIndx = 0;
            var groupSQL = "";
            var oldScreenName = "";
            var tempGroupSQL = string.Empty;
            foreach (var group in groupList)
            {
                //Checking logical operators
                //Pattern: ([Test Field1]: "Value1") or ([Test Field1]: "Value1")
                //Regex: \)[^{(^)}]*\(
                var operatorList = Regex.Matches(group, @"\)[^{(^)}]*\(").Select(d => d.Value).ToList();
                foreach (var op in operatorList)
                {                    
                    var logicalOp = op.Replace(")", "").Replace("(", "").Trim();
                    if (string.IsNullOrEmpty(logicalOp)) throw new Exception(_localizer["Query String - Missing logical operator in group: "].ToString() + group);
                    if (!validOp.Contains(logicalOp.ToLower())) throw new Exception(_localizer["Query String - Invalid logical operator: "].ToString() + logicalOp);
                }

                groupSQL = group;
                tempGroupSQL = groupSQL;
                screenId = 0;
                //Get matching filters in group               
                var filterList = Regex.Matches(group, @"\([^(^)]*\)").Select(d => d.Value).ToList();
                //Loop through matched list
                foreach (var matched in filterList)
                {
                    try
                    {
                        fieldIndx = filterList.IndexOf(matched);
                        //Pattern: [Test Field1]:
                        //Regex: \[([^"]+)\]+:                        
                        var fieldFilter = Regex.Matches(matched, @"\[([^""]+)\]+:").Select(d => d.Value).FirstOrDefault();
                        if (string.IsNullOrEmpty(fieldFilter))
                        {
                            errorMsg += matched;
                            throw new Exception(errorMsg);
                        }
                        else
                        {
                            //Check FieldName is valid                            
                            var matchedField = availableFields
                                                .Where(d => d.FieldLabelLong.ToLower() == fieldFilter.Replace(":", "").Replace("[", "").Replace("]", "").ToLower())
                                                .FirstOrDefault();

                            if (matchedField == null)
                            {
                                errorMsg += fieldFilter.Replace(":", "");
                                throw new Exception(errorMsg);
                            }
                            else
                            {
                                if (fieldIndx == 0)
                                {
                                    screenId = matchedField.ScreenId;
                                    oldScreenName = matchedField.ScreenName;
                                }
                                else
                                {
                                    if (screenId != matchedField.ScreenId)
                                    {
                                        throw new Exception(_localizer["Query String - Invalid field in group "].ToString() + oldScreenName + ": " + matched);
                                    }
                                }

                                //Check if criteria is valid - 2 double quotes
                                var criteria = matched.Substring(1, matched.Length - 2).Replace(fieldFilter, "").Trim();
                                if (!string.IsNullOrEmpty(criteria) && criteria.StartsWith('"') && criteria.EndsWith('"') && criteria.Length >= 4)
                                {
                                    var parsedCriteria = criteria.Substring(1, criteria.Length - 2);
                                    var itemParam = objParam.DataFilters.Where(d => d.FieldId == matchedField.FieldId && (d.Criteria.Contains(parsedCriteria) || string.IsNullOrEmpty(d.Criteria))).FirstOrDefault();
                                    var itemEntryOrder = 0;
                                    if (itemParam != null)
                                    {
                                        itemParam.Criteria = parsedCriteria;
                                        itemParam.LogicalOperator = "OR";
                                        itemEntryOrder = itemParam.OrderEntry;
                                    }
                                    else
                                    {
                                        itemEntryOrder = objParam.DataFilters.Count() + 1;
                                        var tempDataFilter = new GSDataFilterBase()
                                        {
                                            FieldId = matchedField.FieldId,
                                            Criteria = parsedCriteria,
                                            OrderEntry = itemEntryOrder,
                                            LogicalOperator = "OR",
                                            LeftParen = "",
                                            RightParen = ""
                                        };
                                        var tempDataFilterList = objParam.DataFilters.Where(d => !string.IsNullOrEmpty(d.LogicalOperator)).ToList();
                                        tempDataFilterList.Add(tempDataFilter);
                                        objParam.DataFilters = tempDataFilterList;
                                    }
                                    var parsedFieldFilter = matched.Replace(fieldFilter, matchedField.TablesAlias + "." + matchedField.FieldName + " LIKE ").Replace(criteria, "@Param_" + matchedField.FieldId + "_" + itemEntryOrder);

                                    if(!string.IsNullOrEmpty(matchedField.SQLWhere))
                                    {
                                        parsedFieldFilter = "(" + matchedField.SQLWhere.Replace("@Param2000", "@Param_" + matchedField.FieldId + "_" + itemEntryOrder) + ")";
                                    }

                                    groupSQL = groupSQL.Replace(matched, parsedFieldFilter);
                                    tempGroupSQL = tempGroupSQL.Replace(matched, "");
                                }
                                else
                                {
                                    errorMsg += criteria;
                                    throw new Exception(errorMsg);
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        throw new Exception(e.Message);
                    }
                }

                tempGroupSQL = tempGroupSQL.Substring(1, tempGroupSQL.Length - 2).ToLower();
                foreach (var op in GetLogicalOperators()) { 
                    tempGroupSQL = tempGroupSQL.Replace(op.Text.ToLower(), ""); 
                    if (string.IsNullOrEmpty(tempGroupSQL.Trim())) break;                     
                }
                                
                if (!string.IsNullOrEmpty(tempGroupSQL.Trim()))
                {
                    errorMsg += tempGroupSQL;
                    throw new Exception(errorMsg);
                }

                sqlFilterList.Add(new GSMoreFilter() { FieldName = "CustomSQL_" + screenId.ToString(), FieldValue = groupSQL.Substring(1, groupSQL.Length - 2) });
            }

            sqlFilterList.AddRange(objParam.MoreFilters.Where(d => d.FieldName.ToLower() != "customsql").ToList());
            objParam.MoreFilters = sqlFilterList;
            var tempDataFilters = objParam.DataFilters.Where(d => !string.IsNullOrEmpty(d.LogicalOperator)).ToList();
            objParam.DataFilters = tempDataFilters;
        }

        public async Task<IActionResult> SearchReadData([DataSourceRequest] DataSourceRequest request, string globalSearchParams)
        {
            var objParam = JsonConvert.DeserializeObject<GSParamDTO>(globalSearchParams);

            //Custom SQL - replace SearchTerm/Criteria in DataFilters with matching values
            if (objParam != null && objParam.MoreFilters.Any(d => !string.IsNullOrEmpty(d.FieldName) && d.FieldName.ToLower() == "customsql" && !string.IsNullOrEmpty(d.FieldValue)))
            {
                try
                {
                    await CheckCustomQueryParam(objParam);
                }
                catch (Exception ex)
                {
                    return new JsonBadRequest(ex.Message);
                }
            }

            try
            {
                var result = await _globalSearchService.RunGlobalSearchDB(User.GetEmail(), User.HasRespOfficeFilter(), User.HasEntityFilter(), objParam);
                var list = result.ToDataSourceResult(request);
                return Json(list);
            }
            catch (Exception ex)
            {                
                return BadRequest(ex.Message);
            }            
        }

        public async Task<IActionResult> SearchReadDoc([DataSourceRequest] DataSourceRequest request, string globalSearchParams)
        {
            await _globalSearchService.LogGlobalSearch(User.GetEmail(), globalSearchParams);

            var objParam = JsonConvert.DeserializeObject<GSParamDTO>(globalSearchParams);
            var docParam = await GetDocParam(objParam);

            var docResults = _azureSearch.SearchDocument(docParam);
            
            var result = await _globalSearchService.RunGlobalSearchDoc(User.GetUserName(), User.HasRespOfficeFilter(), User.HasEntityFilter(), docResults, objParam.MoreFilters);
            var list = result.ToDataSourceResult(request);

            return Json(list);
        }
       
        private async Task<List<AzureSearchDocList>> GetDocParam(GSParamDTO param)
        {
            var docParam = new List<AzureSearchDocList>();
            if (param.SearchMode == BASICSEARCH)
            {
                docParam.Add(new AzureSearchDocList()
                {
                    SystemScreens = param.SystemScreens,
                    DocumentTypes = param.DocumentTypes,
                    Criteria = param.BasicSearchTerm, 
                    DocSearchMode = param.DocSearchMode.ToLower(),
                    DocQueryType = param.DocQueryType.ToLower()
                });
            }
            else
            {
                string fieldIds = "|";
                param.DocFilters.Each(df => fieldIds = fieldIds + df.FieldId.ToString() + "|");

                //var list = await _globalSearchService.GSFields.Where(fld => param.DocFilters.Any(fltr => fltr.FieldId == fld.FieldId))
                var list = await _globalSearchService.GSFields.Where(fld =>  fieldIds.Contains("|" + fld.FieldId.ToString() + "|"))
                                    .Select(fld => new AzureSearchDocList
                                    {
                                        FieldId = fld.FieldId,
                                        SystemScreens = "|" + fld.GSScreen.GSSystem.SystemType + "-" + fld.GSScreen.ScreenCode + "|",
                                        DocumentTypes = "|" + fld.FieldName + "|",
                                    }).ToListAsync();

                var filters = param.DocFilters.ToList();

                docParam = list.Join(filters, lst => lst.FieldId, fltr => fltr.FieldId,
                            (lst, fltr) => new AzureSearchDocList
                            {
                                SystemScreens = lst.SystemScreens, DocumentTypes = lst.DocumentTypes, Criteria = fltr.Criteria, 
                                DocSearchMode = string.IsNullOrEmpty(fltr.DocSearchMode) ? "any" : fltr.DocSearchMode.ToLower(),
                                DocQueryType = string.IsNullOrEmpty(fltr.DocQueryType) ? "simple" : fltr.DocQueryType.ToLower()
                            }).ToList();
            }

            return docParam;
        }

        [HttpPost]
        public async Task<IActionResult> DownloadDoc(string selection)
        {
            var docList = GetDownloadParam(selection);
            var docFileList = await _globalSearchService.GetDownloadDocInfo(docList);
            var compressFile = _documentStorageService.CompressGSDocuments(docFileList.ToList());
            var stream = new MemoryStream(compressFile);
            return new FileStreamResult(stream, "application/zip") { FileDownloadName = "GlobalSearchDoc.zip" };
        }

        private List<GSDownloadParamDTO> GetDownloadParam(string selection)
        {
            var docList = new List<GSDownloadParamDTO>();
            var i = 0;
            var docs = selection.Split(",");
            foreach (string doc in docs)
            {
                var items = doc.Split("|");
                var param = new GSDownloadParamDTO()
                {
                    RecordId = ++i,
                    SystemType = items[0],
                    ScreenCode = items[1],
                    DocumentType = items[2],
                    ParentId = Int32.Parse(items[3]),
                    LogId = Int32.Parse(items[4]),
                    DocFileName = items[5]
                };
                docList.Add(param);
            };
            return docList;
        }

        public async Task<IActionResult> BuildAdvancedSearchCriteria(List<GSDataFilterBase> advancedCriteria)
        {
            var searchStr = String.Empty;
            var grpSearchStr = String.Empty;

            var availableFields = await _globalSearchService.GSFields
                                .Where(f => f.IsEnabled).OrderBy(f => f.EntryOrder)                                
                                .ToListAsync();

            var grpData = availableFields.Where(d => advancedCriteria.Any(a => a.FieldId == d.FieldId))
                                            .Select(d => new { d.ScreenId, d.FieldId, d.FieldLabelLong })
                                            .OrderBy(o => o.ScreenId).ThenBy(t => t.FieldLabelLong)
                                            .GroupBy(grp => grp.ScreenId)
                                            .Select(d => new
                                            {
                                                ScreenId = d.Key,
                                                Fields = d.Select(s => new { s.FieldId, s.FieldLabelLong }).ToList()
                                            })
                                            .ToList();

            foreach (var item in grpData)
            {
                if (item.Fields.Any())
                {                    
                    var filters = advancedCriteria.Where(d => item.Fields.Any(i => i.FieldId == d.FieldId))
                                                .Select(d => new
                                                {
                                                    d.FieldId,
                                                    d.Criteria,
                                                    d.OrderEntry,
                                                    FieldLabelLong = item.Fields.Where(i => i.FieldId == d.FieldId).Select(i => i.FieldLabelLong).FirstOrDefault()
                                                })
                                                .OrderBy(o => o.OrderEntry).ToList();

                    foreach (var filter in filters)
                    {
                        if (!string.IsNullOrEmpty(grpSearchStr))
                            //grpSearchStr = grpSearchStr + " OR ";
                            grpSearchStr = grpSearchStr + " AND "; //Carlo requested on 9/2/2024 to use AND instead of OR

                        grpSearchStr = grpSearchStr + $"([{filter.FieldLabelLong}]: \"{filter.Criteria}\")";
                    }
                    grpSearchStr = "{" + grpSearchStr + "}";
                    searchStr += grpSearchStr;
                    grpSearchStr = String.Empty;
                }
            }

            return Content(searchStr);
        }

        private async Task<List<SystemType>> GetUserSystemTypes()
        {
            var systemTypes = new List<SystemType>();

            var userSystems = User.GetSystems();
            if (userSystems != null && userSystems.Count > 0)
            {
                systemTypes = await _globalSearchService.CPiSystems.Where(d => userSystems.Any() && userSystems.Contains(d.Id))
                                        .Select(d => new SystemType()
                                        {
                                            TypeId = d.SystemType,
                                            SystemId = d.Id
                                        })
                                        .ToListAsync();
            }

            return systemTypes;
        }        
    }    
}