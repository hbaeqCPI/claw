using ActiveQueryBuilder.View;
using ActiveQueryBuilder.Web.Server.Models;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using DocumentFormat.OpenXml.Bibliography;
using DocuSign.eSign.Model;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.SharePoint.Client;
using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Entities.Documents;
using R10.Core.Entities.GlobalSearch;
using R10.Core.Entities.Shared;
using R10.Core.Helpers;
using R10.Core.Identity;
using R10.Core.Interfaces;
using R10.Core.Services.Shared;
using R10.Infrastructure.Data;
using R10.Web.Areas;
using R10.Web.Areas.Shared.Services;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Areas.Shared.ViewModels.SharePoint;
using R10.Web.Extensions;
using R10.Web.Helpers;
using R10.Web.Interfaces;
using R10.Web.Models;
using R10.Web.Services.SharePoint;
using R10.Web.ViewComponents;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace R10.Web.Services
{
    public class SharePointViewModelService : ISharePointViewModelService
    {
        private readonly IApplicationDbContext _repository;
        private readonly ISharePointService _sharePointService;
        private readonly GraphSettings _graphSettings;
        private readonly IMemoryCache _cache;
        private readonly ISystemSettings<DefaultSetting> _settings;
        private readonly ICPiUserSettingManager _userSettingManager;
        private readonly ClaimsPrincipal _user;
        private readonly IMapper _mapper;
        private readonly IGraphAuthProvider _authProvider;
        private readonly ISharePointRepository _sharePointRepository;
        private readonly IDocumentsAIViewModelService _documentsAIViewModelService;
        private readonly IDocumentService _documentService;

        private List<SharePointSyncDTO> _sharePointSyncItems;

        //set to true if recordkey is not represented as multiple folders in Sharepoint (ex. ABC-US folder instead of ABC folder with US subfolder)
        public const bool IsSharePointRecKeySingleNodeOnly = false;

        //set to true if integration is only for main screens (No documents for action, cost, or invention)
        public const bool IsSharePointIntegrationMainScreenOnly = false;

        //set to true if integration is using document set 
        public const bool IsSharePointIntegrationUsingDocumentSet = false;

        //set to true if integration will auto add the document set
        public const bool IsSharePointIntegrationAutoAddDocumentSet = true;

        public const bool IsSharePointIntegrationCanAddFolder = true;

        //set to true if integration has the field CPISyncCompleted
        public const bool IsSharePointIntegrationHasSyncField = false;


        public SharePointViewModelService(IApplicationDbContext repository,
            ISharePointService sharePointService,
            IOptions<GraphSettings> graphSettings,
            IMemoryCache cache, ISystemSettings<DefaultSetting> settings, ICPiUserSettingManager userSettingManager,
            ClaimsPrincipal user, IMapper mapper, IGraphAuthProvider authProvider,
            ISharePointRepository sharePointRepository, IDocumentsAIViewModelService documentsAIViewModelService, IDocumentService documentService)
        {
            _repository = repository;
            _sharePointService = sharePointService;
            _graphSettings = graphSettings.Value;
            _cache = cache;
            _settings = settings;
            _userSettingManager = userSettingManager;
            _user = user;
            _mapper = mapper;
            _authProvider = authProvider;
            _sharePointRepository = sharePointRepository;
            _documentsAIViewModelService = documentsAIViewModelService;
            _documentService = documentService;
        }

        public async Task<bool> IsAuthenticatedToSharePoint(ClaimsPrincipal user)
        {
            return await _authProvider.IsAuthenticated(_graphSettings.SharePoint, _graphSettings.Site.GetAuthenticationFlow(user), user.GetUserIdentifier());
        }

        public async Task<SharePointFolderViewModel> GetFolders(string screenCode, int recordId, string systemType, string? subScreenCode = "")
        {
            var sharePointFolder = "";
            var recKey = "";

            switch (screenCode)
            {
                case ScreenCode.Invention:
                    sharePointFolder = SharePointDocLibraryFolder.Invention;
                    var invention = await _repository.Inventions.Where(r => r.InvId == recordId).FirstOrDefaultAsync();
                    if (invention != null)
                    {
                        recKey = invention.CaseNumber;
                    }
                    break;

                case ScreenCode.Application:
                    sharePointFolder = SharePointDocLibraryFolder.Application;
                    var application = await _repository.CountryApplications.Where(r => r.AppId == recordId).FirstOrDefaultAsync();
                    if (application != null)
                    {
                        recKey = BuildRecKey(application.CaseNumber, application.Country, application.SubCase);
                    }
                    break;

                case ScreenCode.Trademark:
                    sharePointFolder = SharePointDocLibraryFolder.Trademark;
                    var trademark = await _repository.TmkTrademarks.Where(r => r.TmkId == recordId).FirstOrDefaultAsync();
                    if (trademark != null)
                    {
                        recKey = BuildRecKey(trademark.CaseNumber, trademark.Country, trademark.SubCase);
                    }

                    break;

                case ScreenCode.Action:
                    sharePointFolder = SharePointDocLibraryFolder.Action;
                    switch (systemType)
                    {
                        case SystemTypeCode.Patent:
                            if (string.IsNullOrEmpty(subScreenCode))
                            {
                                var patAction = await _repository.PatActionDues.Where(a => a.ActId == recordId).FirstOrDefaultAsync();
                                if (patAction != null)
                                {
                                    recKey = BuildActionRecKey(patAction.CaseNumber, patAction.Country, patAction.SubCase, patAction.ActionType, patAction.BaseDate);
                                }
                            }
                            else if (subScreenCode == ScreenCode.ActionDueDate)
                            {
                                var dueDate = await _repository.PatDueDates.Where(a => a.DDId == recordId).Include(a => a.PatActionDue).FirstOrDefaultAsync();
                                if (dueDate != null)
                                {
                                    recKey = BuildActionDueDateRecKey(dueDate.PatActionDue.CaseNumber, dueDate.PatActionDue.Country, dueDate.PatActionDue.SubCase, dueDate.PatActionDue.ActionType, dueDate.PatActionDue.BaseDate, dueDate.ActionDue);
                                }
                            }
                            break;

                        case SystemTypeCode.Trademark:

                            if (string.IsNullOrEmpty(subScreenCode))
                            {
                                var tmkAction = await _repository.TmkActionDues.Where(a => a.ActId == recordId).FirstOrDefaultAsync();
                                if (tmkAction != null)
                                {
                                    recKey = BuildActionRecKey(tmkAction.CaseNumber, tmkAction.Country, tmkAction.SubCase, tmkAction.ActionType, tmkAction.BaseDate);
                                }
                            }
                            else if (subScreenCode == ScreenCode.ActionDueDate)
                            {
                                var dueDate = await _repository.TmkDueDates.Where(a => a.DDId == recordId).Include(a => a.TmkActionDue).FirstOrDefaultAsync();
                                if (dueDate != null)
                                {
                                    recKey = BuildActionDueDateRecKey(dueDate.TmkActionDue.CaseNumber, dueDate.TmkActionDue.Country, dueDate.TmkActionDue.SubCase, dueDate.TmkActionDue.ActionType, dueDate.TmkActionDue.BaseDate, dueDate.ActionDue);
                                }
                            }
                            break;

                    }
                    break;

                case ScreenCode.CostTracking:
                    sharePointFolder = SharePointDocLibraryFolder.Cost;
                    switch (systemType)
                    {
                        case SystemTypeCode.Patent:
                            var patCost = await _repository.PatCostTracks.Where(a => a.CostTrackId == recordId).FirstOrDefaultAsync();
                            if (patCost != null)
                            {
                                recKey = BuildCostTrackingRecKey(patCost.CaseNumber, patCost.Country, patCost.SubCase, patCost.CostType, patCost.InvoiceNumber, patCost.InvoiceDate);
                            }
                            break;

                        case SystemTypeCode.Trademark:
                            var tmkCost = await _repository.TmkCostTracks.Where(a => a.CostTrackId == recordId).FirstOrDefaultAsync();
                            if (tmkCost != null)
                            {
                                recKey = BuildCostTrackingRecKey(tmkCost.CaseNumber, tmkCost.Country, tmkCost.SubCase, tmkCost.CostType, tmkCost.InvoiceNumber, tmkCost.InvoiceDate);
                            }
                            break;

                    }
                    break;

                case ScreenCode.TmkConflict:
                    sharePointFolder = SharePointDocLibraryFolder.Conflict;
                    var conflict = await _repository.TmkConflicts.Where(r => r.ConflictId == recordId).FirstOrDefaultAsync();
                    if (conflict != null)
                    {
                        recKey = BuildTmkConflictRecKey(conflict.CaseNumber, conflict.Country, conflict.SubCase, conflict.ConflictOppNumber);
                    }
                    break;

                case ScreenCode.ActionInv:
                    sharePointFolder = SharePointDocLibraryFolder.InventionAction;
                    var invAction = await _repository.PatActionDueInvs.Where(r => r.ActId == recordId).FirstOrDefaultAsync();
                    if (invAction != null)
                    {
                        recKey = BuildInventionActionRecKey(invAction.CaseNumber, invAction.ActionType, invAction.BaseDate);
                    }
                    break;

                case ScreenCode.CostInv:
                    sharePointFolder = SharePointDocLibraryFolder.InventionCostTracking;
                    var invCostTracking = await _repository.PatCostTrackInvs.Where(r => r.CostTrackInvId == recordId).FirstOrDefaultAsync();
                    if (invCostTracking != null)
                    {
                        recKey = BuildInventionCostTrackingRecKey(invCostTracking.CaseNumber, invCostTracking.CostType, invCostTracking.InvoiceNumber, invCostTracking.InvoiceDate);
                    }
                    break;
            }

            if (!string.IsNullOrEmpty(recKey))
            {
                return new SharePointFolderViewModel { Folder = sharePointFolder, RecKey = recKey };
            }
            return null;
        }

        public async Task<List<string>> GetFolders(string system, string module, int recordId)
        {
            if (system == SystemTypeCode.Patent)
            {
                if (module == "Act")
                {
                    var action = await _repository.PatActionDues.AsNoTracking().Where(c => c.ActId == recordId).FirstOrDefaultAsync();
                    if (action == null)
                        return new List<string>();
                    var app = await _repository.CountryApplications.AsNoTracking().Where(c => c.AppId == action.AppId).FirstOrDefaultAsync();
                    if (app != null)
                    {
                        return new List<string>() { app.CaseNumber, app.Country + (string.IsNullOrEmpty(app.SubCase) ? "" : SharePointSeparator.Field + app.SubCase) + SharePointSeparator.Field + action.ActionType + SharePointSeparator.Field + ((DateTime?)action.BaseDate).FormatToDisplay() };
                    }
                }
                else if (module == "App")
                {
                    var app = await _repository.CountryApplications.AsNoTracking().Where(c => c.AppId == recordId).FirstOrDefaultAsync();
                    if (app != null)
                    {
                        return new List<string>() { app.CaseNumber, app.Country + (string.IsNullOrEmpty(app.SubCase) ? "" : SharePointSeparator.Field + app.SubCase) };
                    }
                }
                else if (module == "Cst")
                {
                    var cost = await _repository.PatCostTracks.AsNoTracking().Where(c => c.CostTrackId == recordId).FirstOrDefaultAsync();
                    if (cost == null)
                        return new List<string>();
                    var app = await _repository.CountryApplications.AsNoTracking().Where(c => c.AppId == cost.AppId).FirstOrDefaultAsync();
                    if (app != null)
                    {
                        return new List<string>() { app.CaseNumber, app.Country + (string.IsNullOrEmpty(app.SubCase) ? "" : SharePointSeparator.Field + app.SubCase) + SharePointSeparator.Field + cost.CostType + SharePointSeparator.Field + cost.InvoiceNumber + SharePointSeparator.Field + ((DateTime?)cost.InvoiceDate).FormatToDisplay() };
                    }
                }
                else if (module == "Inv")
                {
                    var inv = await _repository.Inventions.AsNoTracking().Where(c => c.InvId == recordId).FirstOrDefaultAsync();
                    if (inv != null)
                    {
                        return new List<string>() { inv.CaseNumber };
                    }
                }
            }
            else if (system == SystemTypeCode.Trademark)
            {
                if (module == "Act")
                {
                    var action = await _repository.TmkActionDues.AsNoTracking().Where(c => c.ActId == recordId).FirstOrDefaultAsync();
                    if (action == null)
                        return new List<string>();
                    var tmk = await _repository.TmkTrademarks.AsNoTracking().Where(c => c.TmkId == action.TmkId).FirstOrDefaultAsync();
                    if (tmk != null)
                    {
                        return new List<string>() { tmk.CaseNumber, tmk.Country + (string.IsNullOrEmpty(tmk.SubCase) ? "" : SharePointSeparator.Field + tmk.SubCase) + SharePointSeparator.Field + action.ActionType + SharePointSeparator.Field + ((DateTime?)action.BaseDate).FormatToDisplay() };
                    }
                }
                else if (module == "Cst")
                {
                    var cost = await _repository.TmkCostTracks.AsNoTracking().Where(c => c.CostTrackId == recordId).FirstOrDefaultAsync();
                    if (cost == null)
                        return new List<string>();
                    var tmk = await _repository.TmkTrademarks.AsNoTracking().Where(c => c.TmkId == cost.TmkId).FirstOrDefaultAsync();
                    if (tmk != null)
                    {
                        return new List<string>() { tmk.CaseNumber, tmk.Country + (string.IsNullOrEmpty(tmk.SubCase) ? "" : SharePointSeparator.Field + tmk.SubCase) + SharePointSeparator.Field + cost.CostType + SharePointSeparator.Field + cost.InvoiceNumber + SharePointSeparator.Field + ((DateTime?)cost.InvoiceDate).FormatToDisplay() };
                    }
                }
                else if (module == "Tmk")
                {
                    var tmk = await _repository.TmkTrademarks.AsNoTracking().Where(c => c.TmkId == recordId).FirstOrDefaultAsync();
                    if (tmk != null)
                    {
                        return new List<string>() { tmk.CaseNumber, tmk.Country + (string.IsNullOrEmpty(tmk.SubCase) ? "" : SharePointSeparator.Field + tmk.SubCase) };
                    }
                }
            }

            return new List<string>();
        }

        public async Task<List<string>> GetFolders(string system, int recordId)
        {
            if (system == SystemTypeCode.Patent)
            {
                var app = await _repository.CountryApplications.AsNoTracking().Where(c => c.AppId == recordId).FirstOrDefaultAsync();
                if (app != null)
                {
                    if (IsSharePointRecKeySingleNodeOnly) {
                        var recKey = BuildRecKey(app.CaseNumber, app.Country, app.SubCase);
                        return new List<string>() { recKey };
                    }
                    return new List<string>() { app.CaseNumber, app.Country + (string.IsNullOrEmpty(app.SubCase) ? "" : SharePointSeparator.Field + app.SubCase) };
                }

            }
            else if (system == SystemTypeCode.Trademark)
            {
                var tmk = await _repository.TmkTrademarks.AsNoTracking().Where(c => c.TmkId == recordId).FirstOrDefaultAsync();
                if (tmk != null)
                {
                    if (IsSharePointRecKeySingleNodeOnly)
                    {
                        var recKey = BuildRecKey(tmk.CaseNumber, tmk.Country, tmk.SubCase);
                        return new List<string>() { recKey };
                    }
                    return new List<string>() { tmk.CaseNumber, tmk.Country + (string.IsNullOrEmpty(tmk.SubCase) ? "" : SharePointSeparator.Field + tmk.SubCase) };
                }
            }
            return new List<string>();
        }

        public async Task<List<string>> GetIdFromFolders(string folders)
        {
            var foldersArray = folders.Split("/");
            for (int i = 0; i < foldersArray.Length; i++)
            {
                foldersArray[i] = Uri.UnescapeDataString(foldersArray[i]);//System.Net.WebUtility.HtmlDecode(foldersArray[i]);//foldersArray[i];
            }
            var result = new List<string>();

            if (foldersArray[0] == "Patent")
            {
                result.Add("P");
                if (foldersArray[1] == "Action")
                {
                    result.Add("Act");

                    var casesArray = foldersArray[3].Split(SharePointSeparator.Field);
                    int currentIndex = 0;
                    var country = casesArray[currentIndex++];
                    var subCase = casesArray.Length == 4 ? casesArray[currentIndex++] : "";
                    var actionType = casesArray[currentIndex++];
                    var baseDate = DateTime.Parse(casesArray[currentIndex++]);

                    var action = await _repository.PatActionDues.AsNoTracking().Where(c => c.CountryApplication.CaseNumber == foldersArray[2]
                    && c.CountryApplication.Country == country
                    && (c.CountryApplication.SubCase == null || c.CountryApplication.SubCase == subCase)
                    && c.ActionType == actionType
                    && c.BaseDate == baseDate).FirstOrDefaultAsync();

                    if (action == null)
                        return new List<string>();
                    result.Add(action.ActId.ToString());
                    result.Add(foldersArray[4]);
                    return result;
                }
                else if (foldersArray[1] == "Application")
                {
                    result.Add("App");

                    var casesArray = foldersArray[3].Split(SharePointSeparator.Field);
                    int currentIndex = 0;
                    var country = casesArray[currentIndex++];
                    var subCase = casesArray.Length == 2 ? casesArray[currentIndex++] : "";

                    var app = await _repository.CountryApplications.AsNoTracking().Where(c => c.CaseNumber == foldersArray[2]
                    && c.Country == country
                    && (c.SubCase == null || c.SubCase == subCase)).FirstOrDefaultAsync();
                    if (app == null)
                        return new List<string>();
                    result.Add(app.AppId.ToString());
                    result.Add(foldersArray[4]);
                    return result;
                }
                else if (foldersArray[1] == "Cost")
                {
                    result.Add("Cst");

                    var casesArray = foldersArray[3].Split(SharePointSeparator.Field);
                    int currentIndex = 0;
                    var country = casesArray[currentIndex++];
                    var subCase = casesArray.Length == 5 ? casesArray[currentIndex++] : "";
                    var costType = casesArray[currentIndex++];
                    var invoiceNumber = casesArray[currentIndex++];
                    var invoiceDate = DateTime.Parse(casesArray[currentIndex++]);

                    var cost = await _repository.PatCostTracks.AsNoTracking().Where(c => c.CountryApplication.CaseNumber == foldersArray[2]
                    && c.CountryApplication.Country == country
                    && (c.CountryApplication.SubCase == null || c.CountryApplication.SubCase == subCase)
                    && c.CostType == costType
                    && c.InvoiceNumber == invoiceNumber
                    && c.InvoiceDate == invoiceDate).FirstOrDefaultAsync();
                    if (cost == null)
                        return new List<string>();
                    result.Add(cost.CostTrackId.ToString());
                    result.Add(foldersArray[4]);
                    return result;
                }
                else if (foldersArray[1] == "Invention")
                {
                    result.Add("Inv");
                    var inv = await _repository.Inventions.AsNoTracking().Where(c => c.CaseNumber == foldersArray[2]).FirstOrDefaultAsync();
                    if (inv == null)
                        return new List<string>();
                    result.Add(inv.InvId.ToString());
                    result.Add(foldersArray[3]);
                    return result;
                }
            }
            else if (foldersArray[0] == "Trademark")
            {
                result.Add("T");
                if (foldersArray[1] == "Action")
                {
                    result.Add("Act");

                    var casesArray = foldersArray[3].Split(SharePointSeparator.Field);
                    int currentIndex = 0;
                    var country = casesArray[currentIndex++];
                    var subCase = casesArray.Length == 4 ? casesArray[currentIndex++] : "";
                    var actionType = casesArray[currentIndex++];
                    var baseDate = DateTime.Parse(casesArray[currentIndex++]);

                    var action = await _repository.TmkActionDues.AsNoTracking().Where(c => c.TmkTrademark.CaseNumber == foldersArray[2]
                    && c.TmkTrademark.Country == country
                    && (c.TmkTrademark.SubCase == null || c.TmkTrademark.SubCase == subCase)
                    && c.ActionType == actionType
                    && c.BaseDate == baseDate).FirstOrDefaultAsync();
                    if (action == null)
                        return new List<string>();
                    result.Add(action.ActId.ToString());
                    result.Add(foldersArray[4]);
                    return result;
                }
                else if (foldersArray[1] == "Cost")
                {
                    result.Add("Cst");

                    var casesArray = foldersArray[3].Split(SharePointSeparator.Field);
                    int currentIndex = 0;
                    var country = casesArray[currentIndex++];
                    var subCase = casesArray.Length == 5 ? casesArray[currentIndex++] : "";
                    var costType = casesArray[currentIndex++];
                    var invoiceNumber = casesArray[currentIndex++];
                    var invoiceDate = DateTime.Parse(casesArray[currentIndex++]);

                    var cost = await _repository.TmkCostTracks.AsNoTracking().Where(c => c.TmkTrademark.CaseNumber == foldersArray[2]
                    && c.TmkTrademark.Country == country
                    && (c.TmkTrademark.SubCase == null || c.TmkTrademark.SubCase == subCase)
                    && c.CostType == costType
                    && c.InvoiceNumber == invoiceNumber
                    && c.InvoiceDate == invoiceDate).FirstOrDefaultAsync();
                    if (cost == null)
                        return new List<string>();
                    result.Add(cost.CostTrackId.ToString());
                    result.Add(foldersArray[4]);
                    return result;
                }
                else if (foldersArray[1] == "Trademark")
                {
                    result.Add("Tmk");

                    var casesArray = foldersArray[3].Split(SharePointSeparator.Field);
                    int currentIndex = 0;
                    var country = casesArray[currentIndex++];
                    var subCase = casesArray.Length == 2 ? casesArray[currentIndex++] : "";

                    var tmk = await _repository.TmkTrademarks.AsNoTracking().Where(c => c.CaseNumber == foldersArray[2]
                    && c.Country == country
                    && (c.SubCase == null || c.SubCase == subCase)).FirstOrDefaultAsync();
                    if (tmk == null)
                        return new List<string>();
                    result.Add(tmk.TmkId.ToString());
                    result.Add(foldersArray[4]);
                    return result;
                }
            }

            return new List<string>();
        }



        public string GetSharePointSystemFolder(string systemType)
        {
            return systemType == "P" ? "Patent" : systemType == "T" ? "Trademark" : systemType == "G" ? "General Matter" : "";
        }

        public async Task<string> GetActionDueRecKey(string systemType, int actId)
        {
            var recKey = "";
            switch (systemType)
            {
                case SystemTypeCode.Patent:
                    var patActionDue = await _repository.PatActionDues.Where(r => r.ActId == actId).FirstOrDefaultAsync();
                    if (patActionDue != null)
                    {
                        recKey = patActionDue.CaseNumber + SharePointSeparator.Folder + patActionDue.Country + (string.IsNullOrEmpty(patActionDue.SubCase) ? "" : SharePointSeparator.Field + patActionDue.SubCase) + SharePointSeparator.Field + patActionDue.ActionType + SharePointSeparator.Field + ((DateTime?)patActionDue.BaseDate).FormatToDisplay();
                    }

                    break;

                case SystemTypeCode.Trademark:
                    var tmkActionDue = await _repository.TmkActionDues.Where(r => r.ActId == actId).FirstOrDefaultAsync();
                    if (tmkActionDue != null)
                    {
                        recKey = tmkActionDue.CaseNumber + SharePointSeparator.Folder + tmkActionDue.Country + (string.IsNullOrEmpty(tmkActionDue.SubCase) ? "" : SharePointSeparator.Field + tmkActionDue.SubCase) + SharePointSeparator.Field + tmkActionDue.ActionType + SharePointSeparator.Field + ((DateTime?)tmkActionDue.BaseDate).FormatToDisplay();
                    }
                    break;


            }
            return recKey;
        }

        public string GetDocLibraryFromSystemTypeCode(string systemType)
        {
            return systemType == SystemTypeCode.Patent ? SharePointDocLibrary.Patent : systemType == SystemTypeCode.Trademark ? SharePointDocLibrary.Trademark : systemType == SystemTypeCode.GeneralMatter ? SharePointDocLibrary.GeneralMatter :
                   systemType == SystemTypeCode.AMS ? SharePointDocLibrary.Patent : string.IsNullOrEmpty(systemType) ? SharePointDocLibrary.Orphanage :
                   systemType == SystemTypeCode.PatClearance ? SharePointDocLibrary.PatClearance : systemType == SystemTypeCode.Clearance ? SharePointDocLibrary.TmkRequest : "";
        }

        public string GetDocLibraryFromDocumentCode(string documentCode)
        {
            return documentCode == "QE" ? SharePointDocLibrary.QELog : documentCode == "Let" ? SharePointDocLibrary.LetterLog : documentCode == "EFS" ? SharePointDocLibrary.IPFormsLog : "";
        }

        public string GetDocLibraryFolderFromScreenCode(string screenCode)
        {
            switch (screenCode)
            {
                case ScreenCode.Invention:
                    return SharePointDocLibraryFolder.Invention;
                case ScreenCode.Application:
                    return SharePointDocLibraryFolder.Application;
                case ScreenCode.Trademark:
                    return SharePointDocLibraryFolder.Trademark;
                case ScreenCode.GeneralMatter:
                    return SharePointDocLibraryFolder.GeneralMatter;
                case ScreenCode.DMS:
                    return SharePointDocLibraryFolder.DMS;
                case ScreenCode.PatClearance:
                    return SharePointDocLibraryFolder.PatClearance;
                case ScreenCode.Clearance:
                    return SharePointDocLibraryFolder.TmkRequest;
                case ScreenCode.Action:
                    return SharePointDocLibraryFolder.Action;
                case ScreenCode.CostTracking:
                    return SharePointDocLibraryFolder.Cost;
            }
            return string.Empty;
        }

        //intentionally written without using SharePointDocLibraryFolder constants (hardcoded in stored proc)
        public static string GetSyncDocLibraryFolder(string docLibrary) {
            switch (docLibrary)
            {
                case SharePointDocLibrary.Patent:
                    return "Application";

                case SharePointDocLibrary.Trademark:
                    return "Trademark";
                    
                case SharePointDocLibrary.GeneralMatter:
                    return "General Matter";
                    
                case SharePointDocLibrary.DMS:
                    return "DMS";

                case SharePointDocLibrary.PatClearance:
                    return "Patent Clearance";

                case SharePointDocLibrary.TmkRequest:
                    return "Trademark Request";

                default:
                    return "";
            }
        }

        //intentionally written without using constants (hardcoded in stored proc)
        public string GetSyncDocLibrary(string docLibrary)
        {
            switch (docLibrary)
            {
                case SharePointDocLibrary.Patent:
                    return "Patent";

                case SharePointDocLibrary.Trademark:
                    return "Trademark";

                case SharePointDocLibrary.GeneralMatter:
                    return "General Matter";

                case SharePointDocLibrary.DMS:
                    return "DMS";

                case SharePointDocLibrary.PatClearance:
                    return "Patent Clearance";

                case SharePointDocLibrary.TmkRequest:
                    return "Trademark Request";

                default:
                    return "";
            }
        }



        private async Task<bool> IsUserRestrictedFromPrivateDocuments()
        {
            var settings = await _settings.GetSetting();
            if (!settings.IsRestrictPrivateDocAccessOn)
                return false;

            var userAccountSettings = await _userSettingManager.GetUserSetting<UserAccountSettings>(_user.GetUserIdentifier());
            if (userAccountSettings?.RestrictPrivateDocuments == null)
                return false;
            return userAccountSettings.RestrictPrivateDocuments;
        }

        public async Task<List<SharePointReportImage>> GetReportImages(string data)
        {
            //https://localhost:44371/Shared/SharePointGraph/GetReportImages?data=P_Inv_320063_P_Inv_100_T_Tmk_100
            var criteria = data.Split('_');
            var images = new List<SharePointReportImage>();
            for (int i = 0; i <= (criteria.Length / 3 - 1); i++)
            {
                var image = new SharePointReportImage
                {
                    System = criteria[3 * i],
                    Module = criteria[3 * i + 1],
                    Id = int.Parse(criteria[3 * i + 2])
                };
                images.Add(image);
            }
            //var imagesResult = await GetSharePointImages();
            //var result = new List<SharePointReportImage>();
            //foreach (var image in images)
            //{
            //    var order = 0;
            //    var tempImages = imagesResult.Where(c => c.System == image.System && c.Module == image.Module && c.Id == image.Id);
            //    foreach(var tempImage in tempImages)
            //    {
            //        tempImage.OrderOfEntry = order++;
            //        result.Add(tempImage);
            //    }
            //}
            //return result;

            //var imagesResult = await GetReportImagesHelper(images);
            //imagesResult = imagesResult.Where(c => c.System == system && c.Module == moduleCode && data.Contains("|" + c.Id + "|")).ToList();
            var result = await GetReportImagesHelper(images);
            return result.Where(c => c.IsPrintOnReport).ToList();
        }

        private async Task<List<SharePointReportImage>> GetSharePointImages(string currentSystems = "|P|T|G|D|E|C|")
        {
            if (_cache.TryGetValue("SharePointDocList" + currentSystems, out var cacheResult))
            {
                return (List<SharePointReportImage>)cacheResult;
            }
            var graphClient = _sharePointService.GetGraphClientByClientCredentials();
            var site = graphClient.GetSiteWithLists(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName).Result;
            var systems = currentSystems.Trim('|').Split("|");//new string[] { "P", "T", "G", "D", "E", "C"};
            var result = new List<SharePointReportImage>();
            foreach (string sys in systems)
            {
                var list = site.Lists.Where(l => l.Name == GetDocLibrary(sys)).FirstOrDefault();

                var output = new List<string>();
                var spResult = await graphClient.Sites[site.Id].Lists[list.Id].Items.Request().Expand("DriveItem($expand=Permissions),fields($select=IsDefault,IsPrintOnReport)").GetAsync();

                if (spResult.CurrentPage.Count > 0)
                {
                    var spResultRem = spResult.Where(c => c.ContentType.Name != "Folder");
                    foreach (var item in spResultRem)
                    {
                        var fields = item.Fields;
                        //fields.AdditionalData.TryGetValue("IsDefault", out var objIsDefault);
                        //fields.AdditionalData.TryGetValue("IsPrintOnReport", out var objIsPrintOnReport);
                        var objIsDefault = fields.AdditionalData.ToDictionary(kvp => kvp.Key, kvp => kvp.Value).GetValueOrDefault("IsDefault");
                        var objIsPrintOnReport = fields.AdditionalData.ToDictionary(kvp => kvp.Key, kvp => kvp.Value).GetValueOrDefault("IsPrintOnReport");
                        if (objIsDefault != null || objIsPrintOnReport != null)
                        {
                            var isDefault = false;
                            var isPrintOnReport = false;
                            if (objIsDefault != null)
                                isDefault = Convert.ToBoolean(objIsDefault.ToString());//JsonSerializer.Deserialize<bool>(objIsDefault.ToString());
                            if (objIsPrintOnReport != null)
                                isPrintOnReport = Convert.ToBoolean(objIsPrintOnReport.ToString()); //JsonSerializer.Deserialize<bool>(objIsPrintOnReport.ToString());

                            var filePath = item.WebUrl.Substring("https://".Length + _graphSettings.Site.HostName.Length + _graphSettings.Site.RelativePath.Length + 1);
                            var pathData = (await GetIdFromFolders(filePath)).ToArray();

                            if (isDefault || isPrintOnReport)
                            {
                                if (pathData.Length == 4)
                                {
                                    var imageItem = new SharePointReportImage()
                                    {
                                        System = pathData[0],
                                        Module = pathData[1],
                                        Id = int.Parse(pathData[2]),
                                        IsDefault = isDefault,
                                        IsPrintOnReport = isPrintOnReport,
                                        FileName = pathData[3],
                                        ItemId = item.DriveItem.File.MimeType.Contains("image") ? item.DriveItem.Id : GetImageFileThumbnailUrl(item.DriveItem.Name),
                                        //OrderOfEntry = orderorder
                                    };
                                    result.Add(imageItem);
                                }
                            }
                        }
                    }
                }
            }

            _cache.Set("SharePointDocList" + currentSystems, result, new MemoryCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(2),
                SlidingExpiration = TimeSpan.FromMinutes(1)
            }.SetSize(1));

            return result;
        }

        private async Task<List<SharePointReportImage>> GetReportImagesHelper(List<SharePointReportImage> images)
        {
            var result = new List<SharePointReportImage>();
            var graphClient = _sharePointService.GetGraphClientByClientCredentials();
            List<Task> tasks = new List<Task>();

            foreach (var image in images)
            {
                var docLibrary = GetDocLibrary(image.System);
                var folders = await GetFolders(image);

                var spDocs = await graphClient.GetSiteDocuments(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, docLibrary, folders);

                var canViewPublicOnly = await IsUserRestrictedFromPrivateDocuments();
                if (canViewPublicOnly)
                {
                    spDocs = spDocs.Where(d => !(d.DriveItem.ListItem.Fields.AdditionalData != null &&
                                                     d.DriveItem.ListItem.Fields.AdditionalData.ContainsKey("IsPrivate") &&
                                                     Convert.ToBoolean(d.DriveItem.ListItem.Fields.AdditionalData["IsPrivate"].ToString())) ||
                                 d.DriveItem.CreatedBy.User.AdditionalData["email"] == _user.GetEmail().ToLower()
                             ).ToList();
                }

                if (spDocs.Count != 0)
                {
                    var PrintedspDocs = spDocs.Where(c =>
                    c.DriveItem.ListItem.Fields.AdditionalData.ToDictionary(kvp => kvp.Key, kvp => kvp.Value).GetValueOrDefault("IsDefault") == null || c.DriveItem.ListItem.Fields.AdditionalData.ToDictionary(kvp => kvp.Key, kvp => kvp.Value).GetValueOrDefault("IsPrintOnReport") == null
                    ? false :
                    Convert.ToBoolean(c.DriveItem.ListItem.Fields.AdditionalData.ToDictionary(kvp => kvp.Key, kvp => kvp.Value).GetValueOrDefault("IsDefault").ToString()) || Convert.ToBoolean(c.DriveItem.ListItem.Fields.AdditionalData.ToDictionary(kvp => kvp.Key, kvp => kvp.Value).GetValueOrDefault("IsPrintOnReport").ToString()));
                    var order = 0;
                    foreach (var printedspDoc in PrintedspDocs)
                    {
                        //var fields = spDoc.DriveItem.ListItem.Fields.AdditionalData.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                        //var isDefault = fields.GetValueOrDefault("IsDefault").ToString();
                        //var isPrintOnReport = fields.GetValueOrDefault("IsPrintOnReport").ToString();
                        //if(true)//if(isDefault || isPrintOnReport)
                        //{
                        var imageItem = new SharePointReportImage()
                        {
                            System = image.System,
                            Module = image.Module,
                            Id = image.Id,
                            IsDefault = Convert.ToBoolean(printedspDoc.DriveItem.ListItem.Fields.AdditionalData.ToDictionary(kvp => kvp.Key, kvp => kvp.Value).GetValueOrDefault("IsDefault").ToString()),//true,//isDefault,
                            IsPrintOnReport = Convert.ToBoolean(printedspDoc.DriveItem.ListItem.Fields.AdditionalData.ToDictionary(kvp => kvp.Key, kvp => kvp.Value).GetValueOrDefault("IsPrintOnReport").ToString()),
                            FileName = printedspDoc.DriveItem.Name,
                            //ImageUrl = printedspDoc.DriveItem.File.MimeType.Contains("image") ? printedspDoc.DriveItem.WebUrl : GetImageFileThumbnailUrl(printedspDoc.DriveItem.Name),
                            ItemId = printedspDoc.DriveItem.File.MimeType.Contains("image") ? printedspDoc.DriveItem.Id : GetImageFileThumbnailUrl(printedspDoc.DriveItem.Name),
                            OrderOfEntry = order
                        };
                        result.Add(imageItem);
                        order++;
                        //}
                    }
                }
            }

            return result;
        }

        private string GetImageFileThumbnailUrl(string fileName)
        {
            return ImageHelper.GetThumbnailIcon(fileName);
        }

        private async Task<List<string>> GetFolders(SharePointReportImage image)
        {
            var result = new List<string>();
            if (IsSharePointIntegrationMainScreenOnly)
            {
                result.AddRange(await GetFolders(image.System, image.Id));
            }
            else {
                result.Add(GetDocLibraryFolder(image.Module));
                result.AddRange(await GetFolders(image.System, image.Module, image.Id));
            }
            
            return result;
        }
        public string GetDocLibrary(string systemTypeCode)
        {
            return systemTypeCode == "P" ? "Patent" : systemTypeCode == "T" ? "Trademark" : systemTypeCode == "G" ? "General Matter" : systemTypeCode == "D" ? "DMS" : systemTypeCode == "E" ? "Patent Clearance" : systemTypeCode == "C" ? "Trademark Request" : "";
        }

        public string GetDocLibraryFolder(string ModuleCode)
        {
            return ModuleCode == "Act" ? "Action" : ModuleCode == "App" ? "Application" : ModuleCode == "Cst" ? "Cost" : ModuleCode == "Inv" ? "Invention" : ModuleCode == "Tmk" ? "Trademark" : ModuleCode == "Mat" ? "General Matter" : ModuleCode == "Dms" ? "DMS" : ModuleCode == "Clr" ? "Clearance" : ModuleCode == "Trq" ? "Trademark Request" : "";
        }

        public async Task<SharePointReportImageViewModel> GetReportImageFile(string system, string itemId, string fileName)
        {
            var file = new SharePointReportImageViewModel();
            if (itemId.StartsWith("logo_"))
            {
                var filePath = "wwwroot/images/" + itemId;
                using (FileStream fs = System.IO.File.OpenRead(filePath))
                {
                    using (BinaryReader br = new BinaryReader(fs))
                    {
                        file.Bytes = br.ReadBytes((int)fs.Length);
                    }
                }

                file.ContentType = ImageHelper.GetContentType(itemId.Substring(itemId.LastIndexOf("."), itemId.Length - itemId.LastIndexOf(".")));
                file.FileName = itemId;
            }
            else
            {
                var graphClient = _sharePointService.GetGraphClientByClientCredentials();
                var docLibrary = GetDocLibrary(system);

                var selectionList = new List<SharePointDocumentDownloadViewModel>
                {
                    new SharePointDocumentDownloadViewModel()
                    {
                        DriveItemId = itemId,
                        Name = fileName
                    }
                };

                await graphClient.DownloadSiteDriveItems(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, docLibrary, selectionList);

                file.Bytes = selectionList.First().FileBytes;
                file.ContentType = ImageHelper.GetContentType(fileName.Substring(fileName.LastIndexOf("."), fileName.Length - fileName.LastIndexOf(".")));
                file.FileName = fileName;
            }
            return file;
        }

        public async Task<List<SharePointReportImage>> GetReportDefaultImagesForPrintScreen(string system, string moduleCode, string data)
        {
            var Ids = data.Trim('|');

            var criteria = Ids.Split('|');
            var images = new List<SharePointReportImage>();
            for (int i = 0; i <= (criteria.Length - 1); i++)
            {
                var image = new SharePointReportImage
                {
                    System = system,
                    Module = moduleCode,
                    Id = int.Parse(criteria[i])
                };
                images.Add(image);
            }
            var imagesResult = await GetReportImagesHelper(images);
            return imagesResult.Where(c => c.IsDefault).ToList();

            //var imagesResult = await GetSharePointImages();
            //imagesResult = imagesResult.Where(c => c.IsDefault && c.System == system && c.Module == moduleCode && data.Contains("|" + c.Id + "|")).ToList();
            //return imagesResult;
        }

        public async Task<List<SharePointImageList>> GetReportImagesListForPrintScreen(string system, string moduleCode, string data)
        {
            var Ids = data.Trim('|');

            var criteria = Ids.Split('|');
            var images = new List<SharePointReportImage>();
            for (int i = 0; i <= (criteria.Length - 1); i++)
            {
                var image = new SharePointReportImage
                {
                    System = system,
                    Module = moduleCode,
                    Id = int.Parse(criteria[i])
                };
                images.Add(image);
            }
            var imagesResult = await GetReportImagesHelper(images);
            imagesResult = imagesResult.Where(c => c.IsPrintOnReport).ToList();

            var result = imagesResult.GroupBy(c => new { c.System, c.Module, c.Id }).Select(c => new SharePointImageList { System = c.First().System, Module = c.First().Module, Id = c.First().Id, ImageNames = string.Join(", ", c.Select(d => d.FileName)) }).ToList();

            return result;
            //var imagesResult = await GetSharePointImages();
            //imagesResult = imagesResult.Where(c => c.IsDefault && c.System == system && c.Module == moduleCode && data.Contains("|" + c.Id + "|")).ToList();
            //return imagesResult;
        }

        public static List<string> GetDocumentFolders(string? docLibraryFolder = "", string? recKey = "")
        {
            var folders = new List<string>();

            if (!string.IsNullOrEmpty(docLibraryFolder) && !IsSharePointIntegrationMainScreenOnly) folders.Add(docLibraryFolder);

            if (!string.IsNullOrEmpty(recKey))
            {
                recKey = recKey.Replace("/", "");
                if (IsSharePointRecKeySingleNodeOnly)
                {
                    folders.Add(recKey);
                }
                else {
                    var recKeys = recKey.Split(SharePointSeparator.Folder).Where(d => !string.IsNullOrEmpty(d)).ToList();
                    if (recKeys.Count > 0)
                        folders.AddRange(recKeys);
                }
                
            }
            return folders;
        }

        public static string BuildFolders(params string[] list)
        {
            var folders = "";
            for (int i = 0; i < list.Length; i++)
            {
                if (!string.IsNullOrEmpty(folders))
                    folders += SharePointSeparator.Folder;

                folders = folders + list[i];
            }
            return folders;
        }

        public static string BuildRecKey(string caseNumber, string country, string? subCase)
        {
            var recKey = caseNumber + SharePointSeparator.Folder + country + (string.IsNullOrEmpty(subCase) ? "" : SharePointSeparator.Field + subCase);
            return recKey;
        }

        public static string BuildFieldsRecKey(string field1, string? field2, string? field3="", string? field4 = "")
        {
            var recKey = field1 + (string.IsNullOrEmpty(field2) ? "" : SharePointSeparator.Field + field2) + (string.IsNullOrEmpty(field3) ? "" : SharePointSeparator.Field + field3) + (string.IsNullOrEmpty(field4) ? "" : SharePointSeparator.Field + field4);
            return recKey;
        }

        public static string BuildGMRecKey(string caseNumber, string? subCase)
        {
            var recKey = caseNumber + (string.IsNullOrEmpty(subCase) ? "" : SharePointSeparator.Field + subCase);
            return recKey;
        }

        public static string BuildGMOPTRecKey(string caseNumber, string? subCase, string? gmoptId)
        {
            var recKey = caseNumber + (string.IsNullOrEmpty(subCase) ? "" : SharePointSeparator.Field + subCase) + SharePointSeparator.Field + gmoptId;
            return recKey;
        }

        public static string BuildActionRecKey(string caseNumber, string country, string? subCase, string? actionType, DateTime? baseDate)
        {
            var recKey = caseNumber + SharePointSeparator.Folder + country + (string.IsNullOrEmpty(subCase) ? "" : SharePointSeparator.Field + subCase) + SharePointSeparator.Field + actionType + SharePointSeparator.Field + baseDate.FormatToDisplay();
            return recKey;
        }

        public static string BuildInventionActionRecKey(string caseNumber, string? actionType, DateTime? baseDate)
        {
            var recKey = caseNumber + SharePointSeparator.Folder + actionType + SharePointSeparator.Field + baseDate.FormatToDisplay();
            return recKey;
        }

        public static string BuildActionCopyRecKey(string country, string? subCase, string? actionType, DateTime? baseDate)
        {
            var recKey = country + (string.IsNullOrEmpty(subCase) ? "" : SharePointSeparator.Field + subCase) + SharePointSeparator.Field + actionType + SharePointSeparator.Field + baseDate.FormatToDisplay();
            return recKey;
        }

        public static string BuildActionDueDateRecKey(string caseNumber, string country, string? subCase, string? actionType, DateTime? baseDate, string? actionDue)
        {
            var recKey = caseNumber + SharePointSeparator.Folder + country + (string.IsNullOrEmpty(subCase) ? "" : SharePointSeparator.Field + subCase) + SharePointSeparator.Field + actionType + SharePointSeparator.Field + baseDate.FormatToDisplay() + SharePointSeparator.Folder + actionDue;
            return recKey;
        }

        public static string BuildGMActionRecKey(string caseNumber, string? subCase, string? actionType, DateTime? baseDate)
        {
            var recKey = caseNumber + (string.IsNullOrEmpty(subCase) ? "" : SharePointSeparator.Field + subCase) + SharePointSeparator.Field + actionType + SharePointSeparator.Field + baseDate.FormatToDisplay();
            return recKey;
        }

        public static string BuildGMActionDueDateRecKey(string caseNumber, string? subCase, string? actionType, DateTime? baseDate, string? actionDue)
        {
            var recKey = caseNumber + (string.IsNullOrEmpty(subCase) ? "" : SharePointSeparator.Field + subCase) + SharePointSeparator.Field + actionType + SharePointSeparator.Field + baseDate.FormatToDisplay() + SharePointSeparator.Folder + actionDue;
            return recKey;
        }

        public static string BuildDMSActionRecKey(string caseNumber, string? actionType, DateTime? baseDate)
        {
            var recKey = caseNumber + SharePointSeparator.Field + actionType + SharePointSeparator.Field + baseDate.FormatToDisplay();
            return recKey;
        }

        public static string BuildDMSActionDueDateRecKey(string caseNumber, string? actionType, DateTime? baseDate, string? actionDue)
        {
            var recKey = caseNumber + SharePointSeparator.Field + actionType + SharePointSeparator.Field + baseDate.FormatToDisplay() + SharePointSeparator.Folder + actionDue;
            return recKey;
        }

        public static string BuildCostTrackingRecKey(string caseNumber, string country, string? subCase, string? costType, string? invoiceNumber, DateTime? invoiceDate)
        {
            var recKey = caseNumber + SharePointSeparator.Folder + country + (string.IsNullOrEmpty(subCase) ? "" : SharePointSeparator.Field + subCase) + SharePointSeparator.Field + costType + SharePointSeparator.Field + invoiceNumber + SharePointSeparator.Field + invoiceDate.FormatToDisplay();
            return recKey;
        }

        public static string BuildInventionCostTrackingRecKey(string caseNumber, string? costType, string? invoiceNumber, DateTime? invoiceDate)
        {
            var recKey = caseNumber + SharePointSeparator.Folder + costType + SharePointSeparator.Field + invoiceNumber + SharePointSeparator.Field + invoiceDate.FormatToDisplay();
            return recKey;
        }

        public static string BuildCostTrackingCopyRecKey(string country, string? subCase, string? costType, string? invoiceNumber, DateTime? invoiceDate)
        {
            var recKey = country + (string.IsNullOrEmpty(subCase) ? "" : SharePointSeparator.Field + subCase) + SharePointSeparator.Field + costType + SharePointSeparator.Field + invoiceNumber + SharePointSeparator.Field + invoiceDate.FormatToDisplay();
            return recKey;
        }

        public static string BuildGMCostTrackingRecKey(string caseNumber, string? subCase, string? costType, string? invoiceNumber, DateTime? invoiceDate)
        {
            var recKey = caseNumber + (string.IsNullOrEmpty(subCase) ? "" : SharePointSeparator.Field + subCase) + SharePointSeparator.Field + costType + SharePointSeparator.Field + invoiceNumber + SharePointSeparator.Field + invoiceDate.FormatToDisplay();
            return recKey;
        }

        public static string BuildTmkConflictRecKey(string caseNumber, string country, string? subCase, string? conflictOppNumber)
        {
            var recKey = caseNumber + SharePointSeparator.Folder + country + (string.IsNullOrEmpty(subCase) ? "" : SharePointSeparator.Field + subCase) + SharePointSeparator.Field + conflictOppNumber;
            return recKey;
        }

        public static string BuildDesignationRecKey(string country, string? subCase)
        {
            var recKey = country + (string.IsNullOrEmpty(subCase) ? "" : SharePointSeparator.Field + subCase);
            return recKey;
        }

        public static string BuildDesignationRecKey(string caseNumber, string country, string? subCase)
        {
            if (IsSharePointRecKeySingleNodeOnly) {
                return BuildRecKey(caseNumber, country, subCase);
            }
            var recKey = country + (string.IsNullOrEmpty(subCase) ? "" : SharePointSeparator.Field + subCase);
            return recKey;
        }

        public async Task<string> GetRecKey(string docLibrary, string docLibraryFolder, int id)
        {
            string recKey = "";

            if (docLibrary == SharePointDocLibrary.Patent)
            {
                if (docLibraryFolder == SharePointDocLibraryFolder.Application)
                {
                    var app = await _repository.CountryApplications.AsNoTracking().FirstOrDefaultAsync(d => d.AppId == id);
                    if (app != null)
                    {
                        recKey = BuildRecKey(app.CaseNumber, app.Country, app.SubCase);
                    }
                }
            }
            else if (docLibrary == SharePointDocLibrary.Trademark)
            {
                if (docLibraryFolder == SharePointDocLibraryFolder.Trademark)
                {
                    var tmk = await _repository.TmkTrademarks.AsNoTracking().FirstOrDefaultAsync(d => d.TmkId == id);
                    if (tmk != null)
                    {
                        recKey = BuildRecKey(tmk.CaseNumber, tmk.Country, tmk.SubCase);
                    }
                }
            }

            return recKey;
        }


        public async Task<List<LookupDTO>> GetChildrenRecKeys(string docLibrary, int parentId)
        {
            var result = new List<LookupDTO>();
            if (docLibrary == SharePointDocLibrary.Patent)
            {
                var countryApp = await _repository.CountryApplications.Where(ca => ca.AppId == parentId).FirstOrDefaultAsync();
                result.Add(new LookupDTO { Value = SharePointDocLibraryFolder.Invention, Text = countryApp.CaseNumber });

                var actions = await _repository.PatActionDues.AsNoTracking().Where(d => d.AppId == parentId).ToListAsync();
                foreach (var item in actions)
                {
                    result.Add(new LookupDTO { Value = SharePointDocLibraryFolder.Action, Text = BuildActionRecKey(item.CaseNumber, item.Country, item.SubCase, item.ActionType, item.BaseDate) });
                }
                var costs = await _repository.PatCostTracks.AsNoTracking().Where(d => d.AppId == parentId).ToListAsync();
                foreach (var item in costs)
                {
                    result.Add(new LookupDTO { Value = SharePointDocLibraryFolder.Cost, Text = BuildCostTrackingRecKey(item.CaseNumber, item.Country, item.SubCase, item.CostType, item.InvoiceNumber, item.InvoiceDate) });
                }
            }

            else if (docLibrary == SharePointDocLibrary.Trademark)
            {
                var actions = await _repository.TmkActionDues.AsNoTracking().Where(d => d.TmkId == parentId).ToListAsync();
                foreach (var item in actions)
                {
                    result.Add(new LookupDTO { Value = SharePointDocLibraryFolder.Action, Text = BuildActionRecKey(item.CaseNumber, item.Country, item.SubCase, item.ActionType, item.BaseDate) });
                }

                var costs = await _repository.TmkCostTracks.AsNoTracking().Where(d => d.TmkId == parentId).ToListAsync();
                foreach (var item in costs)
                {
                    result.Add(new LookupDTO { Value = SharePointDocLibraryFolder.Cost, Text = BuildCostTrackingRecKey(item.CaseNumber, item.Country, item.SubCase, item.CostType, item.InvoiceNumber, item.InvoiceDate) });
                }
            }

            return result;
        }

        public async Task SyncToDocumentTables(ClaimsPrincipal user)
        {
            var systems = user.GetSystems();
            var docLibraries = new List<string>();

            if (systems.Any(s => s == "Patent")) docLibraries.Add(SharePointDocLibrary.Patent);
            if (systems.Any(s => s == "Trademark")) docLibraries.Add(SharePointDocLibrary.Trademark);
            if (systems.Any(s => s == "GeneralMatter")) docLibraries.Add(SharePointDocLibrary.GeneralMatter);
            if (systems.Any(s => s == "DMS")) docLibraries.Add(SharePointDocLibrary.DMS);
            if (systems.Any(s => s == "PatClearance")) docLibraries.Add(SharePointDocLibrary.PatClearance);
            if (systems.Any(s => s == "SearchRequest")) docLibraries.Add(SharePointDocLibrary.TmkRequest);

            var setting = await _settings.GetSetting();
            if (setting.IsSharePointIntegrationByMetadataOn)
            {
                await SyncToDocumentTablesByMetadata(docLibraries);
            }
            else
            {
                await SyncToDocumentTables(docLibraries);
            }
        }

        public async Task ClearSyncFlagToDocumentTables(ClaimsPrincipal user)
        {
            var systems = user.GetSystems();
            var docLibraries = new List<string>();

            if (systems.Any(s => s == "Patent")) docLibraries.Add(SharePointDocLibrary.Patent);
            if (systems.Any(s => s == "Trademark")) docLibraries.Add(SharePointDocLibrary.Trademark);
            if (systems.Any(s => s == "GeneralMatter")) docLibraries.Add(SharePointDocLibrary.GeneralMatter);
            if (systems.Any(s => s == "DMS")) docLibraries.Add(SharePointDocLibrary.DMS);
            if (systems.Any(s => s == "PatClearance")) docLibraries.Add(SharePointDocLibrary.PatClearance);
            if (systems.Any(s => s == "SearchRequest")) docLibraries.Add(SharePointDocLibrary.TmkRequest);

            await ClearSyncFlagToDocumentTables(docLibraries);
        }

        public async Task SyncToDocumentTables(List<string> docLibraries)
        {
            var graphClient = _sharePointService.GetGraphClientByClientCredentials();
            var site = graphClient.GetSiteWithLists(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName).Result;

            var settings = await _settings.GetSetting();

            _sharePointSyncItems = new List<SharePointSyncDTO>();
            foreach (var docLibrary in docLibraries)
            {
                _sharePointSyncItems.Clear();
                var list = site.Lists.Where(l => l.Name == docLibrary).FirstOrDefault();
                if (list == null)
                    continue;

                var driveItems = new List<DriveItem>();
                var docLibraryForSync = GetSyncDocLibrary(docLibrary);

                try
                {
                    IListItemsCollectionPage result;

                    if (SharePointViewModelService.IsSharePointIntegrationHasSyncField)
                        result = await graphClient.Sites[site.Id].Lists[list.Id].Items.Request().Filter($"fields/CPISyncCompleted ne 1").Expand("driveItem").GetAsync();
                    else {
                        var lastModifiedDate = await _sharePointRepository.GetDocLibraryLastSync(docLibraryForSync);
                        if (lastModifiedDate == null)
                            lastModifiedDate = "1900-01-01";
                        else
                            lastModifiedDate = $"{lastModifiedDate}Z";

                        if (SharePointViewModelService.IsSharePointRecKeySingleNodeOnly)
                           result = await graphClient.Sites[site.Id].Lists[list.Id].Items.Request().Filter($"fields/Modified ge '{lastModifiedDate}'").Expand("driveItem").GetAsync();
                        else
                           result = await graphClient.Sites[site.Id].Lists[list.Id].Items.Request().Header("Prefer", "allowthrottleablequeries").Filter($"fields/ContentType eq 'Folder' or fields/Modified ge '{lastModifiedDate}'").Expand("driveItem").GetAsync();
                     
                    }

                    if (result.CurrentPage.Count > 0)
                    {
                        var process = true;
                        var items = result.CurrentPage;

                        while (process)
                        {
                            foreach (var item in items)
                            {
                                var driveItem = item.DriveItem;
                                driveItem.ListItem = item;
                                driveItems.Add(driveItem);
                            }

                            process = false;
                            if (result.NextPageRequest != null)
                            {
                                var page = await result.NextPageRequest.GetAsync();
                                if (page.CurrentPage.Count > 0)
                                {
                                    result = page;
                                    items = page.CurrentPage;
                                    process = true;
                                }
                            }

                        }

                        foreach (var item in driveItems)
                        {
                            var parentId = String.Empty;
                            var parentFolder = String.Empty;
                            var type = "File";
                            int level = -1;

                            
                            if (item.ParentReference != null)
                            {
                                parentId = item.ParentReference.Id;
                                var parent = item.ParentReference;

                                if (SharePointViewModelService.IsSharePointIntegrationMainScreenOnly)
                                {
                                    parentFolder = parent.Name;
                                    if (SharePointViewModelService.IsSharePointRecKeySingleNodeOnly) {
                                        if (item.ListItem.ContentType.Name == "Document Set" || parent.Path.EndsWith($"/root:"))
                                           level = 0;
                                    }
                                    //not using  DoclibraryFolder but has multiple nodes (multiple folders ex. 0001 then US)
                                    else if (parent.Path.Contains($"/root:/{parentFolder}"))
                                    {
                                        level = 0;
                                        item.Name = parentFolder + SharePointSeparator.Folder + item.Name;
                                    }
                                }
                                else {
                                    //parent is the doclibrary
                                    if (parent.Path.EndsWith($"/root:")) {
                                        level = 0;
                                    }
                                    else parentFolder = parent.Name;
                                }
                            }
                            else level = 0;

                            if (item.Folder != null)
                            {
                                type = "Folder";
                            }

                            var author = "";
                            if (item.CreatedBy.User.AdditionalData != null && item.CreatedBy.User.AdditionalData["email"] != null)
                            {
                                //author = item.CreatedBy.User.AdditionalData["email"].ToString().Left(20);
                                author = item.CreatedBy.User.AdditionalData["email"].ToString();
                            }
                            else
                            {
                                author = item.CreatedBy.User.DisplayName;
                            }

                            var newSync = new SharePointSyncDTO
                            {
                                Id = item.Id,
                                Name = item.Name,
                                ParentId = parentId,
                                ParentFolder = parentFolder,
                                Type = type,
                                Level = level,
                                DocLibrary = docLibraryForSync,
                                IsImage = item.Image != null,
                                Author = author,
                                ModifiedDate = item.LastModifiedDateTime != null ? item.LastModifiedDateTime.Value.DateTime : null
                            };

                            if (!settings.IsSharePointIntegrationKeyFieldsOnly) {
                                GetListItemValues(newSync, item.ListItem);
                            }
                            _sharePointSyncItems.Add(newSync);
                        }

                        var topFolders = _sharePointSyncItems.Where(l => l.Level == 0).ToList();
                        if (IsSharePointIntegrationMainScreenOnly)
                        {
                            var docLibraryFolderForSync = GetSyncDocLibraryFolder(docLibrary);
                            foreach (var topFolder in topFolders)
                            {
                                topFolder.DocLibraryFolder = docLibraryFolderForSync;
                                GetChildrenDriveItems(topFolder, topFolder.Name);
                            }
                        }

                        else {
                            topFolders.Each(f => f.DocLibraryFolder = f.Name);
                            foreach (var topFolder in topFolders)
                            {
                                var keys = new List<SharePointSyncDTO>();
                                if (topFolder.DocLibraryFolder == SharePointDocLibraryFolder.Invention || topFolder.DocLibraryFolder == SharePointDocLibraryFolder.DMS ||
                                    //topFolder.DocLibraryFolder == SharePointDocLibraryFolder.GeneralMatter ||
                                    docLibrary == SharePointDocLibrary.GeneralMatter ||
                                    topFolder.DocLibraryFolder == SharePointDocLibraryFolder.PatClearance || topFolder.DocLibraryFolder == SharePointDocLibraryFolder.TmkRequest)
                                {
                                    keys = _sharePointSyncItems.Where(l => topFolder.Id == l.ParentId).ToList();
                                    foreach (var item in keys)
                                    {
                                        item.Key = item.Name;
                                        item.DocLibraryFolder = topFolder.DocLibraryFolder;
                                    }
                                }
                                else
                                {
                                    var caseNumbers = _sharePointSyncItems.Where(l => topFolder.Id == l.ParentId).ToList();
                                    keys = _sharePointSyncItems.Where(l => caseNumbers.Any(c => c.Id == l.ParentId)).ToList();

                                    foreach (var item in caseNumbers)
                                    {
                                        var children = keys.Where(c => c.ParentId == item.Id).ToList();
                                        foreach (var child in children)
                                        {
                                            child.Key = item.Name + SharePointSeparator.Folder + child.Name;
                                            child.DocLibraryFolder = topFolder.DocLibraryFolder;
                                        }
                                    }
                                }

                                foreach (var item in keys)
                                {
                                    GetChildrenDriveItems(item, item.Key);
                                }
                            }
                        }

                        var syncList = _sharePointSyncItems.Where(z => z.Type != "Folder" && z.Key != null).ToList();
                        if (syncList.Any())
                        {
                            var lastModifiedDateTime = (syncList.OrderByDescending(i => i.ModifiedDate).FirstOrDefault()).ModifiedDate;
                            var savedList = await _sharePointRepository.SyncToDocumentTablesSave(_user.GetUserName(), lastModifiedDateTime,syncList,false,IsSharePointIntegrationMainScreenOnly,IsSharePointRecKeySingleNodeOnly);
                            if (savedList.Any() && IsSharePointIntegrationHasSyncField)
                            {
                                var drive = (await graphClient.GetSiteByPath(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName)).Drives.FirstOrDefault(d => d.Name == docLibrary);
                                if (drive != null)
                                {
                                    foreach (var item in savedList)
                                    {
                                        var driveItem = await graphClient.Drives[drive.Id].Items[item].Request().Expand("listItem").GetAsync();
                                        if (driveItem != null)
                                        {
                                            var requestBody = new FieldValueSet
                                            {
                                                AdditionalData = new Dictionary<string, object>
                                                    {
                                                        {
                                                            "CPISyncCompleted" , true
                                                        }
                                                    }
                                            };

                                            try
                                            {
                                                await graphClient.Sites[site.Id].Lists[list.Id].Items[driveItem.ListItem.Id].Fields.Request().UpdateAsync(requestBody);
                                            }
                                            catch (Exception ex)
                                            {

                                                //throw;
                                            }

                                        }
                                    }
                                }


                            }
                        }

                    }
                }
                catch (Exception ex)
                {

                    throw;
                }


            }
        }

        public void GetListItemValues(SharePointSyncDTO newSync, Microsoft.Graph.ListItem item) {
            var fields = item.Fields.AdditionalData.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            if (fields.ContainsKey("IsDefault"))
            {
                newSync.IsDefault = Convert.ToBoolean(fields.GetValueOrDefault("IsDefault").ToString());

            }
            if (fields.ContainsKey("IsPrintOnReport"))
            {
                newSync.IsPrintOnReport = Convert.ToBoolean(fields.GetValueOrDefault("IsPrintOnReport").ToString());
            }
            if (fields.ContainsKey("IsVerified"))
            {
                newSync.IsVerified = Convert.ToBoolean(fields.GetValueOrDefault("IsVerified").ToString());
            }
            if (fields.ContainsKey("IncludeInWorkflow"))
            {
                newSync.IncludeInWorkflow = Convert.ToBoolean(fields.GetValueOrDefault("IncludeInWorkflow").ToString());
            }
            if (fields.ContainsKey("IsPrivate"))
            {
                newSync.IsPrivate = Convert.ToBoolean(fields.GetValueOrDefault("IsPrivate").ToString());
            }
            if (fields.ContainsKey("Remarks"))
            {
                newSync.Remarks = fields.GetValueOrDefault("Remarks").ToString();
            }
            if (fields.ContainsKey("CPiTags"))
            {
                newSync.Tags = fields.GetValueOrDefault("CPiTags").ToString();
            }
            if (fields.ContainsKey("IsActRequired"))
            {
                newSync.IsActRequired = Convert.ToBoolean(fields.GetValueOrDefault("IsActRequired").ToString());
            }
            if (fields.ContainsKey("CheckAct"))
            {
                newSync.CheckAct = Convert.ToBoolean(fields.GetValueOrDefault("CheckAct").ToString());
            }
            if (fields.ContainsKey("SendToClient"))
            {
                newSync.SendToClient = Convert.ToBoolean(fields.GetValueOrDefault("SendToClient").ToString());
            }
            if (fields.ContainsKey("Source"))
            {
                newSync.Source = fields.GetValueOrDefault("Source").ToString();
            }
        }
        
        public async Task SyncToDocumentTablesByMetadata(List<string> docLibraries)
        {
            var graphClient = _sharePointService.GetGraphClientByClientCredentials();
            var site = graphClient.GetSiteWithLists(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName).Result;

            var settings = await _settings.GetSetting();

            _sharePointSyncItems = new List<SharePointSyncDTO>();
            foreach (var docLibrary in docLibraries)
            {
                _sharePointSyncItems.Clear();
                var list = site.Lists.Where(l => l.Name == docLibrary).FirstOrDefault();
                if (list == null)
                    continue;

                var driveItems = new List<DriveItem>();
                var docLibraryForSync = GetSyncDocLibrary(docLibrary);

                try
                {
                    var result = await graphClient.Sites[site.Id].Lists[list.Id].Items.Request().Filter($"fields/CPISyncCompleted ne 1").Expand("driveItem").GetAsync();
                    if (result.CurrentPage.Count > 0)
                    {
                        var process = true;
                        var items = result.CurrentPage;

                        while (process)
                        {
                            foreach (var item in items)
                            {
                                var driveItem = item.DriveItem;
                                driveItem.ListItem = item;
                                driveItems.Add(driveItem);
                            }

                            process = false;
                            if (result.NextPageRequest != null)
                            {
                                var page = await result.NextPageRequest.GetAsync();
                                if (page.CurrentPage.Count > 0)
                                {
                                    result = page;
                                    items = page.CurrentPage;
                                    process = true;
                                }
                            }
                        }

                        foreach (var item in driveItems)
                        {
                            var author = "";
                            if (item.CreatedBy.User.AdditionalData != null && item.CreatedBy.User.AdditionalData["email"] != null)
                            {
                                //author = item.CreatedBy.User.AdditionalData["email"].ToString().Left(20);
                                author = item.CreatedBy.User.AdditionalData["email"].ToString();
                            }
                            else
                            {
                                author = item.CreatedBy.User.DisplayName;
                            }

                            var newSync = new SharePointSyncDTO
                            {
                                Id = item.Id,
                                Name = item.Name,
                                DocLibrary = docLibraryForSync,
                                IsImage = item.Image != null,
                                Author = author,
                                ModifiedDate = item.LastModifiedDateTime != null ? item.LastModifiedDateTime.Value.DateTime : null
                            };

                            var fields = item.ListItem.Fields.AdditionalData.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                            if (fields.ContainsKey("CPIScreen"))
                            {
                                newSync.DocLibraryFolder = fields.GetValueOrDefault("CPIScreen").ToString();
                            }

                            if (fields.ContainsKey("CPIRecordKey"))
                            {
                                newSync.Key = fields.GetValueOrDefault("CPIRecordKey").ToString();
                            }

                            if (!settings.IsSharePointIntegrationKeyFieldsOnly) {

                                GetListItemValues(newSync, item.ListItem);
                            }
                            _sharePointSyncItems.Add(newSync);
                        }

                        if (_sharePointSyncItems.Any())
                        {
                            var lastModifiedDateTime = (_sharePointSyncItems.OrderByDescending(i => i.ModifiedDate).FirstOrDefault()).ModifiedDate;
                            var savedList = await _sharePointRepository.SyncToDocumentTablesSave(_user.GetUserName(), lastModifiedDateTime, _sharePointSyncItems);
                            if (savedList.Any() && IsSharePointIntegrationHasSyncField)
                            {
                                var drive = (await graphClient.GetSiteByPath(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName)).Drives.FirstOrDefault(d => d.Name == docLibrary);
                                if (drive != null)
                                {
                                    foreach (var item in savedList)
                                    {
                                        var driveItem = await graphClient.Drives[drive.Id].Items[item].Request().Expand("listItem").GetAsync();
                                        if (driveItem != null)
                                        {
                                            var requestBody = new FieldValueSet
                                            {
                                                AdditionalData = new Dictionary<string, object>
                                                    {
                                                        {
                                                            "CPISyncCompleted" , true
                                                        }
                                                    }
                                            };

                                            try
                                            {
                                                await graphClient.Sites[site.Id].Lists[list.Id].Items[driveItem.ListItem.Id].Fields.Request().UpdateAsync(requestBody);
                                            }
                                            catch (Exception ex)
                                            {

                                                //throw;
                                            }

                                        }
                                    }
                                }


                            }
                        }

                    }
                }
                catch (Exception ex)
                {

                    throw;
                }


            }
        }


        public async Task ClearSyncFlagToDocumentTables(List<string> docLibraries)
        {

            var graphClient = _sharePointService.GetGraphClientByClientCredentials();
            var site = graphClient.GetSiteWithLists(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName).Result;

            _sharePointSyncItems = new List<SharePointSyncDTO>();
            foreach (var docLibrary in docLibraries)
            {
                _sharePointSyncItems.Clear();
                var list = site.Lists.Where(l => l.Name == docLibrary).FirstOrDefault();
                if (list == null)
                    continue;

                var driveItems = new List<DriveItem>();

                try
                {
                    var result = await graphClient.Sites[site.Id].Lists[list.Id].Items.Request().Filter($"fields/CPISyncCompleted eq 1").GetAsync();
                    if (result.CurrentPage.Count > 0)
                    {
                        var process = true;
                        var items = result.CurrentPage;

                        while (process)
                        {
                            foreach (var item in items)
                            {
                                var requestBody = new FieldValueSet
                                {
                                    AdditionalData = new Dictionary<string, object>
                                                    {
                                                        {
                                                            "CPISyncCompleted" , false
                                                        }
                                                    }
                                };

                                try
                                {
                                    await graphClient.Sites[site.Id].Lists[list.Id].Items[item.Id].Fields.Request().UpdateAsync(requestBody);
                                }
                                catch (Exception ex)
                                {

                                    //throw;
                                }

                            }

                            process = false;
                            if (result.NextPageRequest != null)
                            {
                                var page = await result.NextPageRequest.GetAsync();
                                if (page.CurrentPage.Count > 0)
                                {
                                    result = page;
                                    items = page.CurrentPage;
                                    process = true;
                                }
                            }

                        }
                    }
                }
                catch (Exception ex)
                {

                    throw;
                }
            }
        }


        private void GetChildrenDriveItems(SharePointSyncDTO parent, string? key)
        {
            var children = _sharePointSyncItems.Where(c => c.ParentId == parent.Id).ToList();
            foreach (var child in children)
            {
                child.Key = key;
                child.DocLibraryFolder = parent.DocLibraryFolder;
                var children2 = _sharePointSyncItems.Where(c => c.ParentId == child.Id).ToList();
                if (children2.Any())
                {
                    foreach (var child2 in children2)
                    {
                        child2.DocLibraryFolder = parent.DocLibraryFolder;
                        GetChildrenDriveItems(child2, key);
                    }
                }
            }
            if (string.IsNullOrEmpty(parent.Key))
                parent.Key = key;
        }


        public async Task SyncToDocumentTablesDelete(string docLibrary, string driveItemId)
        {
            var systemType = GetSystemCodeFromDocLibrary(docLibrary);

            var existingDocFiles = await _repository.DocFiles.Where(d => d.DriveItemId == driveItemId).Include(d => d.DocDocument).ThenInclude(d => d.DocFolder)
                                                    .Where(d => d.DocDocument != null && d.DocDocument.DocFolder.SystemType == systemType).AsNoTracking().ToListAsync();

            foreach (var item in existingDocFiles)
            {
                await _repository.Database.ExecuteSqlAsync($"Delete from tblDocDocument Where FileId={item.FileId}");
                await _repository.Database.ExecuteSqlAsync($"Delete from tblDocFile Where FileId={item.FileId}");
            }


        }

        public async Task SyncToDocumentTablesUpdateDelete(ClaimsPrincipal user)
        {
            var systems = user.GetSystems();
            var docLibraries = new List<string>();

            if (systems.Any(s => s == "Patent")) docLibraries.Add(SharePointDocLibrary.Patent);
            if (systems.Any(s => s == "Trademark")) docLibraries.Add(SharePointDocLibrary.Trademark);
            if (systems.Any(s => s == "GeneralMatter")) docLibraries.Add(SharePointDocLibrary.GeneralMatter);
            if (systems.Any(s => s == "DMS")) docLibraries.Add(SharePointDocLibrary.DMS);
            if (systems.Any(s => s == "PatClearance")) docLibraries.Add(SharePointDocLibrary.PatClearance);
            if (systems.Any(s => s == "SearchRequest")) docLibraries.Add(SharePointDocLibrary.TmkRequest);

            await SyncToDocumentTablesUpdateDelete(docLibraries);
        }

        public async Task SyncToDocumentTablesUpdateDelete(List<string> docLibraries)
        {
            var graphClient = _sharePointService.GetGraphClientByClientCredentials();
            var site = graphClient.GetSiteWithLists(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName).Result;

            _sharePointSyncItems = new List<SharePointSyncDTO>();
            var settings = await _settings.GetSetting();

            foreach (var docLibrary in docLibraries)
            {
                _sharePointSyncItems.Clear();
                var list = site.Lists.Where(l => l.Name == docLibrary).FirstOrDefault();
                if (list == null)
                    continue;

                var systemType = GetSystemCodeFromDocLibrary(docLibrary);
                var docLibraryForSync = GetSyncDocLibrary(docLibrary);

                try
                {
                    IListItemsCollectionPage result;
                    if (SharePointViewModelService.IsSharePointIntegrationHasSyncField)
                        result = await graphClient.Sites[site.Id].Lists[list.Id].Items.Request().Filter($"fields/CPISyncCompleted eq 1").Expand("driveItem").GetAsync();
                    else
                        result = await graphClient.Sites[site.Id].Lists[list.Id].Items.Request().Expand("driveItem").GetAsync();
               
                    if (result.CurrentPage.Count > 0)
                    {
                        var process = true;
                        var items = result.CurrentPage;

                        while (process)
                        {
                            foreach (var item in items)
                            {
                                if (item.DriveItem.Folder == null) {
                                    var newSync = new SharePointSyncDTO
                                    {
                                        Id = item.DriveItem.Id,
                                        Name = item.DriveItem.Name,
                                        DocLibrary = docLibraryForSync,
                                    };
                                    if (!settings.IsSharePointIntegrationKeyFieldsOnly)
                                    {
                                        GetListItemValues(newSync, item);
                                    }
                                    _sharePointSyncItems.Add(newSync);

                                }
                            }

                            process = false;
                            if (result.NextPageRequest != null)
                            {
                                var page = await result.NextPageRequest.GetAsync();
                                if (page.CurrentPage.Count > 0)
                                {
                                    result = page;
                                    items = page.CurrentPage;
                                    process = true;
                                }
                            }

                        }

                        if (_sharePointSyncItems.Any())
                        {
                            await _sharePointRepository.SyncToDocumentTablesUpdateDelete(docLibrary, systemType, _sharePointSyncItems);
                        }

                    }
                }
                catch (Exception ex)
                {

                    throw;
                }


            }
        }

        public async Task SyncToDocumentTables(SharePointSyncToDocViewModel sync)
        {
            var docType = 0;
            var newDoc = false;

            var systemType = GetSystemCodeFromDocLibrary(sync.DocLibrary);
            var dataKey = GetDataKeyFromDocLibraryFolder(sync.DocLibraryFolder);

            //clear existing defaults
            if (sync.IsDefault)
            {
                await _documentService.DocDocuments.Where(d => d.DocFolder.SystemType == systemType && d.DocFolder.DataKey == dataKey && d.DocFolder.DataKeyValue == sync.ParentId)
                                                   .ExecuteUpdateAsync(f => f.SetProperty(d => d.IsDefault, false)
                                                   .SetProperty(d => d.UpdatedBy, sync.CreatedBy)
                                                   .SetProperty(d => d.LastUpdate, DateTime.Now)
                                                   );
            }

            var fileDocType = await _repository.DocTypes.Where(t => t.DocTypeName == "File").FirstOrDefaultAsync();
            if (fileDocType != null)
            {
                docType = fileDocType.DocTypeId;
            }

            var fileId = 0;
            var existingDocFile = await _repository.DocFiles.Where(d => d.DriveItemId == sync.DriveItemId).FirstOrDefaultAsync();
            if (existingDocFile != null)
            {
                existingDocFile.FileExt = Path.GetExtension(sync.FileName).Substring(1);
                existingDocFile.UserFileName = sync.FileName;
                existingDocFile.IsImage = sync.IsImage;
                existingDocFile.UpdatedBy = sync.CreatedBy;
                _repository.DocFiles.Update(existingDocFile);
                fileId = existingDocFile.FileId;
            }
            else
            {
                var docFile = new DocFile
                {
                    FileExt = Path.GetExtension(sync.FileName).Substring(1),
                    UserFileName = sync.FileName,
                    FileSize = 0,
                    IsImage = sync.IsImage,
                    DriveItemId = sync.DriveItemId,
                    CreatedBy = sync.CreatedBy,
                    UpdatedBy = sync.CreatedBy,
                    DateCreated = DateTime.Now,
                    LastUpdate = DateTime.Now
                };
                _repository.DocFiles.Add(docFile);
                await _repository.SaveChangesAsync();
                fileId = docFile.FileId;
                newDoc = true;
            }

            var folderId = 0;
            var existingDocFolder = await _repository.DocFolders.Where(d => d.SystemType == systemType && d.DataKey == dataKey && d.DataKeyValue == sync.ParentId && d.FolderName == "Documents").AsNoTracking().FirstOrDefaultAsync();
            if (existingDocFolder != null)
            {
                folderId = existingDocFolder.FolderId;
            }
            else
            {
                var docFolder = new DocFolder
                {
                    Author = sync.Author,
                    SystemType = systemType,
                    DataKey = dataKey,
                    DataKeyValue = sync.ParentId,
                    FolderName = "Documents",
                    ParentFolderId = 0,
                    IsPrivate = false,
                    ScreenCode = GetScreenCodeFromDocLibraryFolder(sync.DocLibraryFolder),
                    CreatedBy = sync.CreatedBy,
                    UpdatedBy = sync.CreatedBy,
                    DateCreated = DateTime.Now,
                    LastUpdate = DateTime.Now
                };
                _repository.DocFolders.Add(docFolder);
                await _repository.SaveChangesAsync();
                folderId = docFolder.FolderId;
            }

            var docDocument = new DocDocument();
            var existingDocDocument = await _repository.DocDocuments.Where(d => d.FileId == fileId && d.FolderId == folderId).FirstOrDefaultAsync();
            if (existingDocDocument != null)
            {
                existingDocDocument.DocName = sync.FileName;
                existingDocDocument.Remarks = sync.Remarks;
                existingDocDocument.IsPrivate = sync.IsPrivate;
                existingDocDocument.IsDefault = sync.IsDefault;
                existingDocDocument.IsPrintOnReport = sync.IsPrintOnReport;
                existingDocDocument.UpdatedBy = sync.CreatedBy;
                existingDocDocument.LastUpdate = DateTime.Now;
                existingDocDocument.Tags = sync.Tags;
                existingDocDocument.IsVerified = sync.IsVerified;
                existingDocDocument.IncludeInWorkflow = sync.IncludeInWorkflow;
                existingDocDocument.IsActRequired = sync.IsActRequired;
                existingDocDocument.CheckAct = sync.CheckAct;
                existingDocDocument.SendToClient = sync.SendToClient;
                existingDocDocument.Source = sync.Source;
                _repository.DocDocuments.Update(existingDocDocument);
            }
            else
            {
                docDocument.Author = sync.Author;
                docDocument.DocName = sync.FileName;
                docDocument.FolderId = folderId;
                docDocument.FileId = fileId;
                docDocument.Remarks = sync.Remarks;
                docDocument.IsPrivate = sync.IsPrivate;
                docDocument.IsDefault = sync.IsDefault;
                docDocument.IsPrintOnReport = sync.IsPrintOnReport;
                docDocument.CreatedBy = sync.CreatedBy;
                docDocument.UpdatedBy = sync.CreatedBy;
                docDocument.DateCreated = DateTime.Now;
                docDocument.LastUpdate = DateTime.Now;
                docDocument.Tags = sync.Tags;
                docDocument.IsVerified = sync.IsVerified;
                docDocument.IncludeInWorkflow = sync.IncludeInWorkflow;
                docDocument.IsActRequired = sync.IsActRequired;
                docDocument.CheckAct = sync.CheckAct;
                docDocument.SendToClient = sync.SendToClient;
                docDocument.Source = sync.Source;
                docDocument.DocTypeId = docType;

                _repository.DocDocuments.Add(docDocument);
            }
            await _repository.SaveChangesAsync();

            if (newDoc && dataKey == DataKey.Application && sync.ProcessAI)
            {
                var settings = await _settings.GetSetting();
                if (settings.IsDocumentUploadAIOn)
                {
                    _repository.Entry(docDocument).State = EntityState.Detached;

                    docDocument.DocFolder = new DocFolder { DataKeyValue = sync.ParentId };
                    docDocument.DocFile = new DocFile { DocFileName = sync.FileName, DriveItemId = sync.DriveItemId };

                    await _documentsAIViewModelService.ProcessUploadedDocuments(new List<DocDocument> { docDocument });
                }
            }

            //return file id
            sync.FileId = fileId;
        }


        public string GetSystemCodeFromDocLibrary(string docLibrary)
        {
            switch (docLibrary)
            {
                case SharePointDocLibrary.Patent:
                    return SystemTypeCode.Patent;

                case SharePointDocLibrary.Trademark:
                    return SystemTypeCode.Trademark;

                case SharePointDocLibrary.GeneralMatter:
                    return SystemTypeCode.GeneralMatter;

                case SharePointDocLibrary.DMS:
                    return SystemTypeCode.DMS;

                case SharePointDocLibrary.PatClearance:
                    return SystemTypeCode.PatClearance;

                case SharePointDocLibrary.TmkRequest:
                    return SystemTypeCode.Clearance;

            }
            return string.Empty;
        }

        public string GetDataKeyFromDocLibraryFolder(string docLibraryFolder)
        {
            switch (docLibraryFolder)
            {
                case SharePointDocLibraryFolder.Invention:
                    return DataKey.Invention;

                case SharePointDocLibraryFolder.Application:
                    return DataKey.Application;

                case SharePointDocLibraryFolder.Action:
                    return DataKey.Action;

                case SharePointDocLibraryFolder.Cost:
                    return DataKey.CostTracking;

                case SharePointDocLibraryFolder.Trademark:
                    return DataKey.Trademark;

                case SharePointDocLibraryFolder.GeneralMatter:
                    return DataKey.GeneralMatter;

                case SharePointDocLibraryFolder.DMS:
                    return DataKey.DMS;

                case SharePointDocLibraryFolder.PatClearance:
                    return DataKey.PatClearance;

                case SharePointDocLibraryFolder.TmkRequest:
                    return DataKey.Clearance;

                case SharePointDocLibraryFolder.InventionAction:
                    return DataKey.ActionInv;
                case SharePointDocLibraryFolder.InventionCostTracking:
                    return DataKey.CostTrackingInv;

            }
            return string.Empty;
        }

        public string GetScreenCodeFromDocLibraryFolder(string docLibraryFolder)
        {
            switch (docLibraryFolder)
            {
                case SharePointDocLibraryFolder.Invention:
                    return ScreenCode.Invention;
                case SharePointDocLibraryFolder.Application:
                    return ScreenCode.Application;
                case SharePointDocLibraryFolder.Trademark:
                    return ScreenCode.Trademark;
                case SharePointDocLibraryFolder.GeneralMatter:
                    return ScreenCode.GeneralMatter;
                case SharePointDocLibraryFolder.Action:
                    return ScreenCode.Action;
                case SharePointDocLibraryFolder.Cost:
                    return ScreenCode.CostTracking;
                case SharePointDocLibraryFolder.InventionAction:
                    return ScreenCode.ActionInv;
                case SharePointDocLibraryFolder.InventionCostTracking:
                    return ScreenCode.CostInv;
                case SharePointDocLibraryFolder.PatClearance:
                    return ScreenCode.PatClearance;
                case SharePointDocLibraryFolder.TmkRequest:
                    return ScreenCode.Clearance;


            }
            return string.Empty;
        }

        public async Task GetIsPrivateDocumentInfoFromDocTable(List<SharePointDocumentViewModel> documents)
        {
            var docsInfo = await _documentService.GetDocumentInfoFromDriveItemIds(documents.Select(d => d.Id).ToList());
            if (docsInfo.Any())
            {
                documents.Each(d =>
                {
                    var info = docsInfo.FirstOrDefault(i => i.DriveItemId == d.Id);
                    if (info != null)
                    {
                        d.IsPrivate = info.IsPrivate;
                    }
                });
            }
        }

        public async Task GetDocumentInfoFromDocTable(SharePointDocumentEntryViewModel viewModel)
        {
            var docsInfo = await _documentService.GetDocumentInfoFromDriveItemIds(new List<string> { viewModel.DriveItemId });
            if (docsInfo.Any())
            {
                var docInfo = docsInfo.FirstOrDefault();

                viewModel.IsDefault = docInfo.IsDefault;
                viewModel.IsDefaultPrev = docInfo.IsDefault;
                viewModel.IsPrintOnReport = docInfo.IsPrintOnReport;
                viewModel.IsVerified = docInfo.IsVerified;
                viewModel.IncludeInWorkflow = docInfo.IncludeInWorkflow;
                viewModel.IsPrivate = docInfo.IsPrivate;
                viewModel.Remarks = docInfo.Remarks;
                viewModel.IsActRequired = docInfo.IsActRequired;
                viewModel.CheckAct = docInfo.CheckAct;  
                viewModel.SendToClient = docInfo.SendToClient;
                viewModel.Source = docInfo.Source;
            }
        }

        public async Task SyncToDocumentTablesInit(SharePointDocumentEntryViewModel viewModel)
        {
            var systemType = GetSystemCodeFromDocLibrary(viewModel.DocLibrary);
            var dataKey = GetDataKeyFromDocLibraryFolder(viewModel.DocLibraryFolder);

            var existingDocument = await _repository.DocDocuments.Where(d => d.DocFile.DriveItemId == viewModel.DriveItemId && d.DocFolder.SystemType == systemType && d.DocFolder.DataKey == dataKey && d.DocFolder.DataKeyValue == viewModel.ParentId).AsNoTracking().FirstOrDefaultAsync();
            if (existingDocument != null)
            {
                viewModel.IsDefault = existingDocument.IsDefault;
                viewModel.IsDefaultPrev = existingDocument.IsDefault;
                viewModel.IsPrintOnReport = existingDocument.IsPrintOnReport;
                viewModel.IsVerified = existingDocument.IsVerified;
                viewModel.IncludeInWorkflow = existingDocument.IncludeInWorkflow;
                viewModel.IsPrivate = existingDocument.IsPrivate;
                viewModel.Remarks = existingDocument.Remarks;
                viewModel.IsActRequired = existingDocument.IsActRequired;
                viewModel.CheckAct = existingDocument.CheckAct;
                viewModel.SendToClient = existingDocument.SendToClient;
                viewModel.Source = existingDocument.Source;
                viewModel.DocId = existingDocument.DocId;
            }
            else {
                var existingDocFile = await _repository.DocFiles.Where(d => d.DriveItemId == viewModel.DriveItemId).FirstOrDefaultAsync();
                if (existingDocFile == null)
                {
                    var folderId = 0;
                    var existingDocFolder = await _repository.DocFolders.Where(d => d.SystemType == systemType && d.DataKey == dataKey && d.DataKeyValue == viewModel.ParentId && d.FolderName == "Documents").AsNoTracking().FirstOrDefaultAsync();
                    if (existingDocFolder != null)
                    {
                        folderId = existingDocFolder.FolderId;
                    }
                    else
                    {
                        var docFolder = new DocFolder
                        {
                            Author = viewModel.Author,
                            SystemType = systemType,
                            DataKey = dataKey,
                            DataKeyValue = viewModel.ParentId,
                            FolderName = "Documents",
                            ParentFolderId = 0,
                            IsPrivate = false,
                            ScreenCode = GetScreenCodeFromDocLibraryFolder(viewModel.DocLibraryFolder),
                            CreatedBy = viewModel.CreatedBy,
                            UpdatedBy = viewModel.CreatedBy,
                            DateCreated = DateTime.Now,
                            LastUpdate = DateTime.Now
                        };
                        _repository.DocFolders.Add(docFolder);
                        await _repository.SaveChangesAsync();
                        folderId = docFolder.FolderId;
                    }
                    var fileId = 0;

                    var docFile = new DocFile
                    {
                        FileExt = Path.GetExtension(viewModel.FileName).Substring(1),
                        UserFileName = viewModel.FileName,
                        FileSize = 0,
                        IsImage = viewModel.IsImage,
                        DriveItemId = viewModel.DriveItemId,
                        CreatedBy = viewModel.CreatedBy,
                        UpdatedBy = viewModel.CreatedBy,
                        DateCreated = DateTime.Now,
                        LastUpdate = DateTime.Now
                    };

                    var docType = 0;
                    var fileDocType = await _repository.DocTypes.Where(t => t.DocTypeName == "File").FirstOrDefaultAsync();
                    if (fileDocType != null)
                    {
                        docType = fileDocType.DocTypeId;
                    }

                    var docDocument = new DocDocument();
                    docDocument.Author = viewModel.Author;
                    docDocument.DocName = viewModel.FileName;
                    docDocument.FolderId = folderId;

                    docDocument.DocFile = docFile;
                    docDocument.CreatedBy = viewModel.CreatedBy;
                    docDocument.UpdatedBy = viewModel.CreatedBy;
                    docDocument.DateCreated = DateTime.Now;
                    docDocument.LastUpdate = DateTime.Now;
                    docDocument.DocTypeId = docType;
                    _repository.DocDocuments.Add(docDocument);
                    await _repository.SaveChangesAsync();

                    viewModel.DocId = docDocument.DocId;
                }
            }
            
        }

        public async Task<DefaultImageViewModel> GetDefaultImageDocumentInfoFromDocTable(string docLibrary,string docLibraryFolder, int dataKeyValue)
        {
            var systemType = GetSystemCodeFromDocLibrary(docLibrary);
            var dataKey = GetDataKeyFromDocLibraryFolder(docLibraryFolder);

            var document = await _documentService.GetDefaultImage(systemType, dataKey, dataKeyValue);
            if (document != null) {
                return new DefaultImageViewModel
                {
                    SharePointDriveItemId = document.DocFile.DriveItemId,
                    SharePointDocLibrary = docLibrary,
                    ImageFile = document.DocName,
                    IsPublic = !document.IsPrivate,
                    ImageTitle = document.DocName
                };
            }
            return null;
        }

        public async Task<DefaultImageViewModel> GetDefaultImageDocumentInfoByRecKey(string docLibrary, string docLibraryFolder, string recKey)
        {
            var settings = await _settings.GetSetting();
            var defaultImageViewModel = new DefaultImageViewModel();

            if (_graphSettings.Site != null)
            {
                var graphClient = _sharePointService.GetGraphClient();

                if (settings.IsSharePointIntegrationByMetadataOn)
                {
                    defaultImageViewModel = await graphClient.GetDefaultImageByMetadata(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, docLibrary, docLibraryFolder, recKey.Replace(SharePointSeparator.Folder, SharePointSeparator.Field));
                }
                else
                {
                    var folderList = SharePointViewModelService.GetDocumentFolders(docLibraryFolder, recKey);
                    defaultImageViewModel = await graphClient.GetDefaultImage(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, docLibrary, folderList);
                }
            }

            return defaultImageViewModel;
        }

        public async Task SyncToDocumentTablesCopy(string author, List<SharePointSyncCopyDTO> sharePointSyncItems) {
            //so you don't have to change the stored proc if names are different
            foreach (var item in sharePointSyncItems)
            {
                item.DocLibrary = GetSyncDocLibrary(item.DocLibrary);
            }
            await _sharePointRepository.SyncToDocumentTablesCopy(author, sharePointSyncItems);
        }

        public async Task SyncToDocumentTablesCopy(string author, List<SharePointSyncDTO> driveItems)
        {
            var docLibrary = driveItems.First().DocLibrary;

            //so you don't have to change the stored proc if names are different
            foreach (var item in driveItems) {
                item.DocLibrary = GetSyncDocLibrary(docLibrary);
                item.DocLibraryFolder = GetSyncDocLibraryFolder(docLibrary);
            }

            var savedList = await _sharePointRepository.SyncToDocumentTablesSave(author, null,driveItems, true);
            if (savedList.Any() && IsSharePointIntegrationHasSyncField)
            {
                var graphClient = _sharePointService.GetGraphClient();

                var drive = (await graphClient.GetSiteByPath(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName)).Drives.FirstOrDefault(d => d.Name == docLibrary);
                if (drive != null)
                {
                    var site = graphClient.GetSiteWithLists(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName).Result;
                    var list = site.Lists.Where(l => l.Name == docLibrary).FirstOrDefault();

                    foreach (var item in savedList)
                    {
                        var driveItem = await graphClient.Drives[drive.Id].Items[item].Request().Expand("listItem").GetAsync();
                        if (driveItem != null)
                        {
                            var requestBody = new FieldValueSet
                            {
                                AdditionalData = new Dictionary<string, object>
                                                    {
                                                        {
                                                            "CPISyncCompleted" , true
                                                        }
                                                    }
                            };

                            try
                            {
                                await graphClient.Sites[site.Id].Lists[list.Id].Items[driveItem.ListItem.Id].Fields.Request().UpdateAsync(requestBody);
                            }
                            catch (Exception ex)
                            {

                                //throw;
                            }

                        }
                    }
                }
            }
        }

        #region Rename Record Keys
        public async Task RenameRecordKey(string docLibrary, string docLibraryFolder, string oldRecordKey, string newRecordKey)
        {
            var graphClient = _sharePointService.GetGraphClient();
            var settings = await _settings.GetSetting();
            if (!settings.IsSharePointCascadeKeyChanges)
                return;

            var oldKeys = oldRecordKey.Split(SharePointSeparator.Folder);
            var newKeys = newRecordKey.Split(SharePointSeparator.Folder);
            var mainKeyModified = docLibraryFolder == SharePointDocLibraryFolder.Invention || docLibraryFolder == SharePointDocLibraryFolder.DMS ||
                docLibraryFolder == SharePointDocLibraryFolder.PatClearance ||
                docLibraryFolder == SharePointDocLibraryFolder.TmkRequest ||
                (oldKeys[0].ToLower() != newKeys[0].ToLower() && docLibraryFolder != SharePointDocLibraryFolder.GeneralMatter);

            if (mainKeyModified || settings.IsSharePointIntegrationByMetadataOn) { 
                if (settings.IsSharePointIntegrationByMetadataOn)
                    await graphClient.RenameRecordKeyByMetadata(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, docLibrary, docLibraryFolder, oldRecordKey.Replace(SharePointSeparator.Folder, SharePointSeparator.Field), newRecordKey.Replace(SharePointSeparator.Folder, SharePointSeparator.Field));
                else
                    await graphClient.RenameMainRecordKey(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, docLibrary, docLibraryFolder, oldRecordKey, newKeys[0]);

            }

            if (docLibraryFolder == SharePointDocLibraryFolder.GeneralMatter || (oldKeys.Length > 1 && newKeys.Length > 1 && oldKeys[1].ToLower() != newKeys[1].ToLower())) {
                if (!settings.IsSharePointIntegrationByMetadataOn)
                {
                    var newKey = docLibraryFolder == SharePointDocLibraryFolder.GeneralMatter ? newRecordKey : newKeys[1];
                    await graphClient.RenameRecordKey(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, docLibrary, docLibraryFolder, oldRecordKey, newKey);
                }
            }
            
        }
        #endregion  Rename Record  

        public async Task SaveImportedDocument(IFormFile formFile, string fileName, int parentId, string docLibrary, string docLibraryFolder, string recKey)
        {
            var folders = SharePointViewModelService.GetDocumentFolders(docLibraryFolder, recKey);
            var result = new SharePointGraphDriveItemKeyViewModel();
            var graphClient = _sharePointService.GetGraphClient();
            using (var stream = new MemoryStream())
            {
                formFile.CopyTo(stream);
                stream.Position = 0;
                result = await graphClient.UploadSiteFile(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, docLibrary, folders, stream, fileName);
            }

            if (!string.IsNullOrEmpty(result.DriveItemId))
            {
                var driveItem = await graphClient.Drives[result.DriveId].Items[result.DriveItemId].Request().Expand("listItem").GetAsync();
                var sync = new SharePointSyncToDocViewModel
                {
                    DocLibrary = docLibrary,
                    DocLibraryFolder = docLibraryFolder,
                    DriveItemId = result.DriveItemId,
                    ParentId = parentId,
                    FileName = fileName,
                    CreatedBy = _user.GetUserName().Left(20),
                    Remarks = "",
                    Tags = "",
                    IsImage = driveItem.Image != null,
                    IsPrivate = false,
                    IsDefault = false,
                    IsPrintOnReport = false,
                    IsVerified = false,
                    IncludeInWorkflow = false,
                    IsActRequired = false,
                    CheckAct = false,
                    SendToClient = false,
                    Source = DocumentSourceType.Manual
                };
                await SyncToDocumentTables(sync);
            }
        }

        public async Task<Stream?> GetDocumentAsStream(string docLibrary, string driveItemId)
        {
            var graphClient = _sharePointService.GetGraphClient();
            return await graphClient.DownloadSiteDriveItem(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, docLibrary, driveItemId);
        }
    }

}
