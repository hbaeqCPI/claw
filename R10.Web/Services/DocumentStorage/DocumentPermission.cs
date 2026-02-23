using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using R10.Core.Entities;
using R10.Core.Entities.DMS;
using R10.Core.Entities.GeneralMatter;
using R10.Core.Entities.Patent;
using R10.Core.Entities.Trademark;
using R10.Core.Interfaces;
using R10.Core.Interfaces.AMS;
using R10.Core.Interfaces.DMS;
using R10.Core.Interfaces.Patent;
using R10.Web.Helpers;
using R10.Web.Security;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using static R10.Web.Helpers.ImageHelper;

namespace R10.Web.Services.DocumentStorage
{
    public class DocumentPermission: IDocumentPermission
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IAsyncRepository<QELog> _qeLogRepository;
        private readonly IAsyncRepository<EFSLog> _efsLogRepository;
        private readonly IInventionService _inventionService;
        private readonly ICountryApplicationService _applicationService;
        private readonly ITmkTrademarkService _trademarkService;
        private readonly IGMMatterService _gmMatterService;
        private readonly IDisclosureService _disclosureService;
        private readonly ITmcClearanceService _clearanceService;
        private readonly IPacClearanceService _patClearanceService;
        private readonly IActionDueService<PatActionDue, PatDueDate> _patActionDueService;
        private readonly IActionDueService<PatActionDueInv, PatDueDateInv> _patActionDueInvService;
        private readonly IActionDueService<TmkActionDue, TmkDueDate> _tmkActionDueService;
        private readonly IActionDueService<GMActionDue, GMDueDate> _gmActionDueService;
        private readonly IActionDueService<DMSActionDue, DMSDueDate> _dmsActionDueService;
        private readonly ICostTrackingService<PatCostTrack> _patCostTrackingService;
        private readonly ICostTrackingService<PatCostTrackInv> _patCostTrackingInvService;
        private readonly ICostTrackingService<TmkCostTrack> _tmkCostTrackingService;
        private readonly ICostTrackingService<GMCostTrack> _gmCostTrackingService;
        private readonly ILetterService _letterService;
        private readonly IPatIDSService _idsService;
        private readonly ITLInfoService _tlInfoService;
        private readonly IRTSService _rtsService;
        private readonly IAMSMainService _amsMainService;
        private readonly IQuickEmailService _qeService;
        private readonly IDocumentService _docService;
        private readonly IProductService _productService;
        private readonly IChildEntityService<GMMatter, GMMatterOtherPartyTrademark> _matterOtherPartyTrademarkService;
        private readonly IDocketRequestService _docketRequestService;

        public DocumentPermission(IAuthorizationService authorizationService, 
                            IInventionService inventionService, ICountryApplicationService applicationService, IDisclosureService disclosureService,
                            ITmkTrademarkService trademarkService, IGMMatterService gmMatterService, IActionDueService<PatActionDue, PatDueDate> patActionDueService, 
                            IActionDueService<TmkActionDue, TmkDueDate> tmkActionDueService, IActionDueService<GMActionDue, GMDueDate> gmActionDueService,
                            IActionDueService<DMSActionDue, DMSDueDate> dmsActionDueService, ICostTrackingService<PatCostTrack> patCostTrackingService,
                            ICostTrackingService<TmkCostTrack> tmkCostTrackingService, ICostTrackingService<GMCostTrack> gmCostTrackingService,
                            IAsyncRepository<QELog> qeLogRepository, ILetterService letterService, IAsyncRepository<EFSLog> efsLogRepository,
                            IPatIDSService idsService, ITLInfoService tlInfoService, IRTSService rtsService, IAMSMainService amsMainService,
                            IQuickEmailService qeService, ITmcClearanceService clearanceService, IPacClearanceService patClearanceService, IDocumentService docService,
                            IProductService productService, ICostTrackingService<PatCostTrackInv> patCostTrackingInvService, IActionDueService<PatActionDueInv, PatDueDateInv> patActionDueInvService,
                            IChildEntityService<GMMatter, GMMatterOtherPartyTrademark> matterOtherPartyTrademarkService,
                            IDocketRequestService docketRequestService)
        {
            _authorizationService = authorizationService;
            _inventionService = inventionService;
            _applicationService = applicationService;
            _disclosureService = disclosureService;
            _trademarkService = trademarkService;
            _gmMatterService = gmMatterService;
            _patActionDueService = patActionDueService;
            _patActionDueInvService = patActionDueInvService;
            _tmkActionDueService = tmkActionDueService;
            _gmActionDueService = gmActionDueService;
            _dmsActionDueService = dmsActionDueService;
            _patCostTrackingService = patCostTrackingService;
            _tmkCostTrackingService = tmkCostTrackingService;
            _gmCostTrackingService = gmCostTrackingService;
            _qeLogRepository = qeLogRepository;
            _letterService = letterService;
            _efsLogRepository = efsLogRepository;
            _idsService = idsService;
            _tlInfoService = tlInfoService;
            _rtsService = rtsService;
            _amsMainService = amsMainService;
            _qeService = qeService;
            _clearanceService = clearanceService;
            _patClearanceService = patClearanceService;
            _docService = docService;
            _productService = productService;
            _patCostTrackingInvService = patCostTrackingInvService;
            _matterOtherPartyTrademarkService = matterOtherPartyTrademarkService;
            _docketRequestService = docketRequestService;
        }

        public async Task<bool> HasPermission(ClaimsPrincipal user, string system, string screenCode, int key, string fileName, CPiSavedFileType fileType)
        {
            var authorized = await _authorizationService.AuthorizeAsync(user, GetPolicy(system, screenCode));
            if (!authorized.Succeeded)
                return false;

            switch (screenCode.ToLower())
            {
                case "inv":
                    return await CanAccessInventionFile(key, fileName, fileType);

                case "ca":
                    return await CanAccessApplicationFile(key, fileName, fileType);

                case "ids":
                    return await CanAccessApplicationFile(key, fileName, CPiSavedFileType.IDSReferences);

                case "tmk":
                    return await CanAccessTrademarkFile(key, fileName, fileType);

                case "rms":
                    return await CanAccessTrademarkFile(key, fileName, fileType);

                case "tl":
                    return await CanAccessTL(key, fileName, fileType);

                case "rts":
                    return await CanAccessRTS(key, fileName, fileType);

                case "gm":
                    return await CanAccessGeneralMatterFile(key, fileName, fileType);
                case "gmt":
                    return await CanAccessGeneralMatterTrademarkFile(key, fileName, fileType);

                case "dms":
                    return await CanAccessDMSFile(key, fileName, fileType);

                case "ams":
                    return await CanAccessAMSFile(key, fileName, fileType);

                case "act":
                    switch (system)
                    {
                        case SystemType.Patent:
                            return await CanAccessPatentActionFile(key, fileName, fileType);

                        case SystemType.Trademark:
                            return await CanAccessTrademarkActionFile(key, fileName, fileType);

                        case SystemType.GeneralMatter:
                            return await CanAccessGMActionFile(key, fileName, fileType);

                        case SystemType.DMS:
                            return await CanAccessDMSActionFile(key, fileName, fileType);

                        default:
                            return false;
                    }

                case "actinv":
                            return await CanAccessPatentActionInvFile(key, fileName, fileType);

                case "cost":
                    switch (system)
                    {
                        case SystemType.Patent:
                            return await CanAccessPatentCostFile(key, fileName, fileType);

                        case SystemType.Trademark:
                            return await CanAccessTrademarkCostFile(key, fileName, fileType);

                        case SystemType.GeneralMatter:
                            return await CanAccessGMCostFile(key, fileName, fileType);

                        default:
                            return false;
                    }

                case "costinv":
                    return await CanAccessPatentCostInvFile(key, fileName, fileType);

                case "tmc":
                    return await CanAccessClearanceFile(key, fileName, fileType);

                case "pac":
                    return await CanAccessPatentClearanceFile(key, fileName, fileType);

                case "prd":
                    return await CanAccessProductFile(key, fileName, fileType);

                case "asgmt":
                    switch (system)
                    {
                        case SystemType.Patent:
                            return await CanAccessPatentAsgmtFile(key, fileName, fileType);

                        case SystemType.Trademark:
                            return await CanAccessTrademarkAsgmtFile(key, fileName, fileType);
                        
                        default:
                            return false;
                    }

                case "lce":
                    switch (system)
                    {
                        case SystemType.Patent:
                            return await CanAccessPatentLicenseeFile(key, fileName, fileType);

                        case SystemType.Trademark:
                            return await CanAccessTrademarkLicenseeFile(key, fileName, fileType);

                        default:
                            return false;
                    }

                case "req":
                    switch (system)
                    {
                        case SystemType.Patent:
                            return await CanAccessApplicationDocketRequestFile(key, fileName, fileType);

                        case SystemType.Trademark:
                            return await CanAccessTrademarkDocketRequestFile(key, fileName, fileType);

                        case SystemType.GeneralMatter:
                            return await CanAccessGMDocketRequestFile(key, fileName, fileType);

                        default:
                            return false;
                    }
                
                default:
                    return false;

            }
        }

        private async Task<bool> CanAccessInventionFile(int invid, string fileName, CPiSavedFileType fileType)
        {
            if (fileType == CPiSavedFileType.DocMgt || fileType == CPiSavedFileType.DocMgtThumbnail)
            {
                fileName = fileName.Replace("_thumb", "");
                return await _inventionService.QueryableList.AnyAsync(i => i.InvId == invid && _docService.DocDocuments.Any(f => f.DocFolder.DataKey == "InvId" && f.DocFolder.DataKeyValue == invid && f.DocFile.DocFileName == fileName));
            }

            if (fileType == CPiSavedFileType.QELoggedImage || fileType == CPiSavedFileType.QELoggedImageThumbnail)
            {
                fileName = fileName.Replace("_thumb", "");
                var hasAccess = _inventionService.QueryableList.Any(i => i.InvId == invid && _qeLogRepository.QueryableList.Any(l => l.DataKey == "InvId" && l.DataKeyValue == invid && l.Attachments.Contains(fileName)));
                return hasAccess;
            }
            if (fileType == CPiSavedFileType.Letter)
            {
                return await _inventionService.QueryableList.AnyAsync(i => i.InvId == invid && _letterService.LetterLogs.Any(l => l.DataKey == "InvId" && l.LetFile == fileName && l.LetterLogDetails.Any(d => d.DataKeyValue == invid)));
            }
            if (fileType == CPiSavedFileType.QE)
            {
                return await _inventionService.QueryableList.AnyAsync(i => i.InvId == invid && _qeService.QELogs.Any(l => l.DataKey == "InvId" && l.DataKeyValue == invid && l.QEFile == fileName));
            }
            return false;
        }

        private async Task<bool> CanAccessApplicationFile(int appid, string fileName, CPiSavedFileType fileType)
        {
            if (fileType == CPiSavedFileType.DocMgt || fileType == CPiSavedFileType.DocMgtThumbnail)
            {
                fileName = fileName.Replace("_thumb", "");
                return await _applicationService.CountryApplications.AnyAsync(i => i.AppId == appid && _docService.DocDocuments.Any(f => f.DocFolder.DataKey == "AppId" && f.DocFolder.DataKeyValue == appid && f.DocFile.DocFileName == fileName));
            }

            if (fileType == CPiSavedFileType.QELoggedImage || fileType == CPiSavedFileType.QELoggedImageThumbnail)
            {
                fileName = fileName.Replace("_thumb", "");
                var hasAccess = _applicationService.CountryApplications.Any(a => a.AppId == appid && _qeLogRepository.QueryableList.Any(l => l.DataKey == "AppId" && l.DataKeyValue == appid && l.Attachments.Contains(fileName)));
                return hasAccess;
            }

            if (fileType == CPiSavedFileType.IDSReferences)
            {
                return await _applicationService.CountryApplications.AnyAsync(a => a.AppId == appid && (_idsService.IDSRelatedCases.Any(r => r.AppId == appid && r.DocFilePath == fileName) || _idsService.IDSNonPatLiteratures.Any(r => r.AppId == appid && r.DocFilePath == fileName)));
            }

            if (fileType == CPiSavedFileType.Letter)
            {
                return await _applicationService.CountryApplications.AnyAsync(a => a.AppId == appid && _letterService.LetterLogs.Any(l => l.DataKey == "AppId" && l.LetFile == fileName && l.LetterLogDetails.Any(d => d.DataKeyValue == appid)));
            }

            if (fileType == CPiSavedFileType.EFS)
            {
                return await _applicationService.CountryApplications.AnyAsync(a => a.AppId == appid && _efsLogRepository.QueryableList.Any(l => l.DataKey == "AppId" && l.EfsFile == fileName && l.DataKeyValue == appid));
            }
            if (fileType == CPiSavedFileType.QE)
            {
                return await _applicationService.CountryApplications.AnyAsync(i => i.AppId == appid && _qeService.QELogs.Any(l => l.DataKey == "AppId" && l.DataKeyValue == appid && l.QEFile == fileName));
            }
            return false;
        }

        private async Task<bool> CanAccessPatentActionFile(int actId, string fileName, CPiSavedFileType fileType)
        {
            if (fileType == CPiSavedFileType.DocMgt || fileType == CPiSavedFileType.DocMgtThumbnail)
            {
                fileName = fileName.Replace("_thumb", "");
                return await _patActionDueService.QueryableList.AnyAsync(a => a.ActId == actId && _docService.DocDocuments.Any(f => f.DocFolder.SystemType == "P" && f.DocFolder.DataKey == "ActId" && f.DocFolder.DataKeyValue == actId && f.DocFile.DocFileName == fileName));
            }
            if (fileType == CPiSavedFileType.QELoggedImage || fileType == CPiSavedFileType.QELoggedImageThumbnail)
            {
                fileName = fileName.Replace("_thumb", "");
                var hasAccess = _patActionDueService.QueryableList.Any(a => a.ActId == actId && _qeLogRepository.QueryableList.Any(l => l.SystemType == "P" && l.DataKey == "ActId" && l.DataKeyValue == actId && l.Attachments.Contains(fileName)));
                return hasAccess;
            }
            if (fileType == CPiSavedFileType.Letter)
            {
                var hasAccess = await _patActionDueService.QueryableList.AnyAsync(a => a.ActId == actId && _letterService.LetterLogs.Any(l => l.DataKey == "ActId" && l.LetFile == fileName && l.LetterLogDetails.Any(d => d.DataKeyValue == actId)));
                if (!hasAccess)
                    hasAccess = await _patActionDueService.QueryableList.AnyAsync(a => a.DueDates.Any(dd => dd.DDId == actId) && _letterService.LetterLogs.Any(l => l.DataKey == "DdId" && l.LetFile == fileName && l.LetterLogDetails.Any(d => d.DataKeyValue == actId)));
                return hasAccess;
            }
            if (fileType == CPiSavedFileType.QE)
            {
                var hasAccess =  await _patActionDueService.QueryableList.AnyAsync(a => a.ActId == actId && _qeService.QELogs.Any(l => l.DataKey == "ActId" && l.DataKeyValue == actId && l.QEFile == fileName));
                if (!hasAccess)
                    hasAccess =  await _patActionDueService.QueryableList.AnyAsync(a => a.DueDates.Any(dd => dd.DDId == actId) && _qeService.QELogs.Any(l => l.DataKey == "DdId" && l.DataKeyValue == actId && l.QEFile == fileName));

                if (!hasAccess)
                    hasAccess = await _patActionDueService.QueryableList.AnyAsync(a => a.DueDates.Any(dd => dd.Delegations.Any(ddd=> ddd.DelegationId== actId)) && _qeService.QELogs.Any(l => l.DataKey == "DelegationId" && l.DataKeyValue == actId && l.QEFile == fileName));
                
                if (!hasAccess)
                    hasAccess = await _patActionDueService.QueryableList.AnyAsync(a => a.DueDates.Any(dd => dd.DueDateDeDockets.Any(ddd => ddd.DeDocketId == actId)) && _qeService.QELogs.Any(l => l.DataKey == "DeDocketId" && l.DataKeyValue == actId && l.QEFile == fileName));

                return hasAccess;
            }
            if (fileType == CPiSavedFileType.DeDocket)
            {
                return await _patActionDueService.QueryableList.AnyAsync(a => a.ActId == actId && a.DueDates.Any(dd=> dd.DueDateDeDockets.Any(ddd=> ddd.DocFile== fileName)));
            }
            return false;
        }

        private async Task<bool> CanAccessPatentActionInvFile(int actId, string fileName, CPiSavedFileType fileType)
        {
            if (fileType == CPiSavedFileType.DocMgt || fileType == CPiSavedFileType.DocMgtThumbnail)
            {
                fileName = fileName.Replace("_thumb", "");
                return await _patActionDueInvService.QueryableList.AnyAsync(a => a.ActId == actId && _docService.DocDocuments.Any(f => f.DocFolder.SystemType == "P" && f.DocFolder.DataKey == "ActInvId" && f.DocFolder.DataKeyValue == actId && f.DocFile.DocFileName == fileName));
            }
            if (fileType == CPiSavedFileType.QELoggedImage || fileType == CPiSavedFileType.QELoggedImageThumbnail)
            {
                fileName = fileName.Replace("_thumb", "");
                var hasAccess = _patActionDueInvService.QueryableList.Any(a => a.ActId == actId && _qeLogRepository.QueryableList.Any(l => l.SystemType == "P" && l.DataKey == "ActInvId" && l.DataKeyValue == actId && l.Attachments.Contains(fileName)));
                return hasAccess;
            }
            if (fileType == CPiSavedFileType.Letter)
            {
                var hasAccess = await _patActionDueInvService.QueryableList.AnyAsync(a => a.ActId == actId && _letterService.LetterLogs.Any(l => l.DataKey == "ActInvId" && l.LetFile == fileName && l.LetterLogDetails.Any(d => d.DataKeyValue == actId)));
                if (!hasAccess)
                    hasAccess = await _patActionDueInvService.QueryableList.AnyAsync(a => a.DueDateInvs.Any(dd => dd.DDId == actId) && _letterService.LetterLogs.Any(l => l.DataKey == "DdInvId" && l.LetFile == fileName && l.LetterLogDetails.Any(d => d.DataKeyValue == actId)));
                return hasAccess;
            }
            if (fileType == CPiSavedFileType.QE)
            {
                var hasAccess = await _patActionDueInvService.QueryableList.AnyAsync(a => a.ActId == actId && _qeService.QELogs.Any(l => l.DataKey == "ActInvId" && l.DataKeyValue == actId && l.QEFile == fileName));
                if (!hasAccess)
                    hasAccess = await _patActionDueInvService.QueryableList.AnyAsync(a => a.DueDateInvs.Any(dd => dd.DDId == actId) && _qeService.QELogs.Any(l => l.DataKey == "DdInvId" && l.DataKeyValue == actId && l.QEFile == fileName));

                if (!hasAccess)
                    hasAccess = await _patActionDueInvService.QueryableList.AnyAsync(a => a.DueDateInvs.Any(dd => dd.Delegations.Any(ddd => ddd.DelegationId == actId)) && _qeService.QELogs.Any(l => l.DataKey == "DelegationInvId" && l.DataKeyValue == actId && l.QEFile == fileName));

                if (!hasAccess)
                    hasAccess = await _patActionDueInvService.QueryableList.AnyAsync(a => a.DueDateInvs.Any(dd => dd.DueDateDeDockets.Any(ddd => ddd.DeDocketId == actId)) && _qeService.QELogs.Any(l => l.DataKey == "DeDocketInvId" && l.DataKeyValue == actId && l.QEFile == fileName));

                return hasAccess;
            }
            if (fileType == CPiSavedFileType.DeDocket)
            {
                return await _patActionDueInvService.QueryableList.AnyAsync(a => a.ActId == actId && a.DueDateInvs.Any(dd => dd.DueDateDeDockets.Any(ddd => ddd.DocFile == fileName)));
            }
            return false;
        }

        private async Task<bool> CanAccessPatentCostFile(int costTrackId, string fileName, CPiSavedFileType fileType)
        {
            if (fileType == CPiSavedFileType.DocMgt || fileType == CPiSavedFileType.DocMgtThumbnail)
            {
                fileName = fileName.Replace("_thumb", "");
                return await _patCostTrackingService.QueryableList.AnyAsync(c => c.CostTrackId == costTrackId && _docService.DocDocuments.Any(f => f.DocFolder.SystemType == "P" && f.DocFolder.DataKey == "CostTrackId" && f.DocFolder.DataKeyValue == costTrackId && f.DocFile.DocFileName == fileName));
            }

            if (fileType == CPiSavedFileType.QELoggedImage || fileType == CPiSavedFileType.QELoggedImageThumbnail)
            {
                fileName = fileName.Replace("_thumb", "");
                var hasAccess = _patCostTrackingService.QueryableList.Any(c => c.CostTrackId == costTrackId && _qeLogRepository.QueryableList.Any(l => l.SystemType == "P" && l.DataKey == "CostTrackId" && l.DataKeyValue == costTrackId && l.Attachments.Contains(fileName)));
                return hasAccess;
            }
            if (fileType == CPiSavedFileType.Letter)
            {
                return await _patCostTrackingService.QueryableList.AnyAsync(a => a.CostTrackId == costTrackId && _letterService.LetterLogs.Any(l => l.DataKey == "CostTrackId" && l.LetFile == fileName && l.LetterLogDetails.Any(d => d.DataKeyValue == costTrackId)));
            }

            if (fileType == CPiSavedFileType.QE)
            {
                return await _patCostTrackingService.QueryableList.AnyAsync(c => c.CostTrackId == costTrackId && _qeService.QELogs.Any(l => l.DataKey == "CostTrackId" && l.DataKeyValue == costTrackId && l.QEFile == fileName));
            }
            return false;
        }

        private async Task<bool> CanAccessPatentCostInvFile(int costTrackInvId, string fileName, CPiSavedFileType fileType)
        {
            if (fileType == CPiSavedFileType.DocMgt || fileType == CPiSavedFileType.DocMgtThumbnail)
            {
                fileName = fileName.Replace("_thumb", "");
                return await _patCostTrackingInvService.QueryableList.AnyAsync(c => c.CostTrackInvId == costTrackInvId && _docService.DocDocuments.Any(f => f.DocFolder.SystemType == "P" && f.DocFolder.DataKey == "CostTrackInvId" && f.DocFolder.DataKeyValue == costTrackInvId && f.DocFile.DocFileName == fileName));
            }

            if (fileType == CPiSavedFileType.QELoggedImage || fileType == CPiSavedFileType.QELoggedImageThumbnail)
            {
                fileName = fileName.Replace("_thumb", "");
                var hasAccess = _patCostTrackingInvService.QueryableList.Any(c => c.CostTrackInvId == costTrackInvId && _qeLogRepository.QueryableList.Any(l => l.SystemType == "P" && l.DataKey == "CostTrackInvId" && l.DataKeyValue == costTrackInvId && l.Attachments.Contains(fileName)));
                return hasAccess;
            }
            if (fileType == CPiSavedFileType.Letter)
            {
                return await _patCostTrackingInvService.QueryableList.AnyAsync(a => a.CostTrackInvId == costTrackInvId && _letterService.LetterLogs.Any(l => l.DataKey == "CostTrackInvId" && l.LetFile == fileName && l.LetterLogDetails.Any(d => d.DataKeyValue == costTrackInvId)));
            }

            if (fileType == CPiSavedFileType.QE)
            {
                return await _patCostTrackingInvService.QueryableList.AnyAsync(c => c.CostTrackInvId == costTrackInvId && _qeService.QELogs.Any(l => l.DataKey == "CostTrackInvId" && l.DataKeyValue == costTrackInvId && l.QEFile == fileName));
            }
            return false;
        }

        //still necessary, otherwise RTS can be use as entry point
        private async Task<bool> CanAccessRTS(int plAppId, string fileName, CPiSavedFileType fileType)
        {
            if (fileType == CPiSavedFileType.Image || fileType == CPiSavedFileType.Thumbnail || fileType == CPiSavedFileType.QELoggedImage)
            {
                return await _applicationService.CountryApplications.AnyAsync(ca=> _rtsService.RTSSearchRecords.Any(pl => pl.PLAppId == plAppId && pl.PMSAppId==ca.AppId && pl.RTSSearchUSIFWs.Any(im => im.FileName == fileName)));
            }
            return false;
        }

        private async Task<bool> CanAccessTrademarkFile(int tmkid, string fileName, CPiSavedFileType fileType)
        {
            if (fileType == CPiSavedFileType.DocMgt || fileType == CPiSavedFileType.DocMgtThumbnail)
            {
                fileName = fileName.Replace("_thumb", "");
                return await _trademarkService.TmkTrademarks.AnyAsync(t => t.TmkId == tmkid && _docService.DocDocuments.Any(f => f.DocFolder.DataKey == "TmkId" && f.DocFolder.DataKeyValue == tmkid && f.DocFile.DocFileName == fileName));
            }
            if (fileType == CPiSavedFileType.QELoggedImage || fileType == CPiSavedFileType.QELoggedImageThumbnail)
            {
                fileName = fileName.Replace("_thumb", "");
                var hasAccess = _trademarkService.TmkTrademarks.Any(t => t.TmkId == tmkid && _qeLogRepository.QueryableList.Any(l => l.DataKey == "TmkId" && l.DataKeyValue == tmkid && l.Attachments.Contains(fileName)));
                return hasAccess;
            }
            if (fileType == CPiSavedFileType.Letter)
            {
                return await _trademarkService.TmkTrademarks.AnyAsync(t => t.TmkId == tmkid && _letterService.LetterLogs.Any(l => l.DataKey == "TmkId" && l.LetFile == fileName && l.LetterLogDetails.Any(d => d.DataKeyValue == tmkid)));
            }
            if (fileType == CPiSavedFileType.QE)
            {
                return await _trademarkService.TmkTrademarks.AnyAsync(t => t.TmkId == tmkid && _qeService.QELogs.Any(l => l.DataKey == "TmkId" && l.DataKeyValue == tmkid && l.QEFile == fileName));
            }
            return false;
        }

        private async Task<bool> CanAccessTrademarkActionFile(int actId, string fileName, CPiSavedFileType fileType)
        {
            if (fileType == CPiSavedFileType.DocMgt || fileType == CPiSavedFileType.DocMgtThumbnail)
            {
                fileName = fileName.Replace("_thumb", "");
                return await _tmkActionDueService.QueryableList.AnyAsync(a => a.ActId == actId && _docService.DocDocuments.Any(f => f.DocFolder.SystemType == "T" && f.DocFolder.DataKey == "ActId" && f.DocFolder.DataKeyValue == actId && f.DocFile.DocFileName == fileName));
            }
            if (fileType == CPiSavedFileType.QELoggedImage || fileType == CPiSavedFileType.QELoggedImageThumbnail)
            {
                fileName = fileName.Replace("_thumb", "");
                var hasAccess = _tmkActionDueService.QueryableList.Any(a => a.ActId == actId && _qeLogRepository.QueryableList.Any(l => l.SystemType == "T" && l.DataKey == "ActId" && l.DataKeyValue == actId && l.Attachments.Contains(fileName)));
                return hasAccess;
            }
            if (fileType == CPiSavedFileType.Letter)
            {
                var hasAccess = await _tmkActionDueService.QueryableList.AnyAsync(a => a.ActId == actId && _letterService.LetterLogs.Any(l => l.DataKey == "ActId" && l.LetFile == fileName && l.LetterLogDetails.Any(d => d.DataKeyValue == actId)));
                if (!hasAccess)
                    hasAccess = await _tmkActionDueService.QueryableList.AnyAsync(a => a.DueDates.Any(dd => dd.DDId == actId) && _letterService.LetterLogs.Any(l => l.DataKey == "DdId" && l.LetFile == fileName && l.LetterLogDetails.Any(d => d.DataKeyValue == actId)));
                return hasAccess;
            }
            if (fileType == CPiSavedFileType.QE)
            {
                var hasAccess = await _tmkActionDueService.QueryableList.AnyAsync(a => a.ActId == actId && _qeService.QELogs.Any(l => l.DataKey == "ActId" && l.DataKeyValue == actId && l.QEFile == fileName));
                if (!hasAccess)
                    hasAccess =  await _tmkActionDueService.QueryableList.AnyAsync(a => a.DueDates.Any(dd => dd.DDId == actId) && _qeService.QELogs.Any(l => l.DataKey == "DdId" && l.DataKeyValue == actId && l.QEFile == fileName));

                if (!hasAccess)
                    hasAccess = await _tmkActionDueService.QueryableList.AnyAsync(a => a.DueDates.Any(dd => dd.Delegations.Any(ddd => ddd.DelegationId == actId)) && _qeService.QELogs.Any(l => l.DataKey == "DelegationId" && l.DataKeyValue == actId && l.QEFile == fileName));

                if (!hasAccess)
                    hasAccess = await _tmkActionDueService.QueryableList.AnyAsync(a => a.DueDates.Any(dd => dd.DueDateDeDockets.Any(ddd => ddd.DeDocketId == actId)) && _qeService.QELogs.Any(l => l.DataKey == "DeDocketId" && l.DataKeyValue == actId && l.QEFile == fileName));

                return hasAccess;
            }
            if (fileType == CPiSavedFileType.DeDocket)
            {
                return await _tmkActionDueService.QueryableList.AnyAsync(a => a.ActId == actId && a.DueDates.Any(dd => dd.DueDateDeDockets.Any(ddd => ddd.DocFile == fileName)));
            }
            return false;
        }

        private async Task<bool> CanAccessTrademarkCostFile(int costTrackId, string fileName, CPiSavedFileType fileType)
        {
            if (fileType == CPiSavedFileType.DocMgt || fileType == CPiSavedFileType.DocMgtThumbnail)
            {
                fileName = fileName.Replace("_thumb", "");
                return await _tmkCostTrackingService.QueryableList.AnyAsync(c => c.CostTrackId == costTrackId && _docService.DocDocuments.Any(f => f.DocFolder.SystemType == "T" && f.DocFolder.DataKey == "CostTrackId" && f.DocFolder.DataKeyValue == costTrackId && f.DocFile.DocFileName == fileName));
            }
            if (fileType == CPiSavedFileType.QELoggedImage || fileType == CPiSavedFileType.QELoggedImageThumbnail)
            {
                fileName = fileName.Replace("_thumb", "");
                var hasAccess = _tmkCostTrackingService.QueryableList.Any(c => c.CostTrackId == costTrackId && _qeLogRepository.QueryableList.Any(l => l.SystemType == "T" && l.DataKey == "CostTrackId" && l.DataKeyValue == costTrackId && l.Attachments.Contains(fileName)));
                return hasAccess;
            }
            if (fileType == CPiSavedFileType.Letter)
            {
                return await _tmkCostTrackingService.QueryableList.AnyAsync(a => a.CostTrackId == costTrackId && _letterService.LetterLogs.Any(l => l.DataKey == "CostTrackId" && l.LetFile == fileName && l.LetterLogDetails.Any(d => d.DataKeyValue == costTrackId)));
            }

            if (fileType == CPiSavedFileType.QE)
            {
                return await _tmkCostTrackingService.QueryableList.AnyAsync(c => c.CostTrackId == costTrackId && _qeService.QELogs.Any(l => l.DataKey == "CostTrackId" && l.DataKeyValue == costTrackId && l.QEFile == fileName));
            }
            return false;
        }

        private async Task<bool> CanAccessGeneralMatterFile(int matId, string fileName, CPiSavedFileType fileType)
        {
            if (fileType == CPiSavedFileType.DocMgt || fileType == CPiSavedFileType.DocMgtThumbnail)
            {
                fileName = fileName.Replace("_thumb", "");
                return await _gmMatterService.QueryableList.AnyAsync(g => g.MatId == matId && _docService.DocDocuments.Any(f => f.DocFolder.DataKey == "MatId" && f.DocFolder.DataKeyValue == matId && f.DocFile.DocFileName == fileName));
            }
            if (fileType == CPiSavedFileType.QELoggedImage || fileType == CPiSavedFileType.QELoggedImageThumbnail)
            {
                fileName = fileName.Replace("_thumb", "");
                var hasAccess = _gmMatterService.QueryableList.Any(g => g.MatId == matId && _qeLogRepository.QueryableList.Any(l => l.DataKey == "MatId" && l.DataKeyValue == matId && l.Attachments.Contains(fileName)));
                return hasAccess;
            }
            if (fileType == CPiSavedFileType.Letter)
            {
                return await _gmMatterService.QueryableList.AnyAsync(g => g.MatId == matId && _letterService.LetterLogs.Any(l => l.DataKey == "MatId" && l.LetFile == fileName && l.LetterLogDetails.Any(d => d.DataKeyValue == matId)));
            }
            if (fileType == CPiSavedFileType.QE)
            {
                return await _gmMatterService.QueryableList.AnyAsync(g => g.MatId == matId && _qeService.QELogs.Any(l => l.DataKey == "MatId" && l.DataKeyValue == matId && l.QEFile == fileName));
            }
            return false;
        }

        private async Task<bool> CanAccessGeneralMatterTrademarkFile(int matId, string fileName, CPiSavedFileType fileType)
        {
            if (fileType == CPiSavedFileType.Image || fileType == CPiSavedFileType.Thumbnail || fileType == CPiSavedFileType.DocMgt)
            {
                fileName = fileName.Replace("_thumb", "");
                return await _gmMatterService.QueryableList.AnyAsync(g => g.MatId == matId && _matterOtherPartyTrademarkService.QueryableList.Any(t => t.MatId == matId && t.DocFilePath == fileName));
            }
            return false;
        }

        private async Task<bool> CanAccessGMActionFile(int actId, string fileName, CPiSavedFileType fileType)
        {
            if (fileType == CPiSavedFileType.DocMgt || fileType == CPiSavedFileType.DocMgtThumbnail)
            {
                fileName = fileName.Replace("_thumb", "");
                return await _gmActionDueService.QueryableList.AnyAsync(a => a.ActId == actId && _docService.DocDocuments.Any(f => f.DocFolder.SystemType == "G" && f.DocFolder.DataKey == "ActId" && f.DocFolder.DataKeyValue == actId && f.DocFile.DocFileName == fileName));
            }

            if (fileType == CPiSavedFileType.QELoggedImage || fileType == CPiSavedFileType.QELoggedImageThumbnail)
            {
                fileName = fileName.Replace("_thumb", "");
                var hasAccess = _gmActionDueService.QueryableList.Any(a => a.ActId == actId && _qeLogRepository.QueryableList.Any(l => l.SystemType == "G" && l.DataKey == "ActId" && l.DataKeyValue == actId && l.Attachments.Contains(fileName)));
                return hasAccess;
            }
            if (fileType == CPiSavedFileType.Letter)
            {
                var hasAccess = await _gmActionDueService.QueryableList.AnyAsync(a => a.ActId == actId && _letterService.LetterLogs.Any(l => l.DataKey == "ActId" && l.LetFile == fileName && l.LetterLogDetails.Any(d => d.DataKeyValue == actId)));
                if (!hasAccess)
                    hasAccess = await _gmActionDueService.QueryableList.AnyAsync(a => a.DueDates.Any(dd => dd.DDId == actId) && _letterService.LetterLogs.Any(l => l.DataKey == "DdId" && l.LetFile == fileName && l.LetterLogDetails.Any(d => d.DataKeyValue == actId)));

                return hasAccess;
            }
            if (fileType == CPiSavedFileType.QE)
            {
                var hasAccess = await _gmActionDueService.QueryableList.AnyAsync(a => a.ActId == actId && _qeService.QELogs.Any(l => l.DataKey == "ActId" && l.DataKeyValue == actId && l.QEFile == fileName));
                if (!hasAccess)
                    hasAccess =  await _gmActionDueService.QueryableList.AnyAsync(a => a.DueDates.Any(dd => dd.DDId == actId) && _qeService.QELogs.Any(l => l.DataKey == "DdId" && l.DataKeyValue == actId && l.QEFile == fileName));

                if (!hasAccess)
                    hasAccess = await _gmActionDueService.QueryableList.AnyAsync(a => a.DueDates.Any(dd => dd.Delegations.Any(ddd => ddd.DelegationId == actId)) && _qeService.QELogs.Any(l => l.DataKey == "DelegationId" && l.DataKeyValue == actId && l.QEFile == fileName));

                if (!hasAccess)
                    hasAccess = await _gmActionDueService.QueryableList.AnyAsync(a => a.DueDates.Any(dd => dd.DueDateDeDockets.Any(ddd => ddd.DeDocketId == actId)) && _qeService.QELogs.Any(l => l.DataKey == "DeDocketId" && l.DataKeyValue == actId && l.QEFile == fileName));

                return hasAccess;
            }
            if (fileType == CPiSavedFileType.DeDocket)
            {
                return await _gmActionDueService.QueryableList.AnyAsync(a => a.ActId == actId && a.DueDates.Any(dd => dd.DueDateDeDockets.Any(ddd => ddd.DocFile == fileName)));
            }
            return false;
        }

        private async Task<bool> CanAccessGMCostFile(int costTrackId, string fileName, CPiSavedFileType fileType)
        {
            if (fileType == CPiSavedFileType.DocMgt || fileType == CPiSavedFileType.DocMgtThumbnail)
            {
                fileName = fileName.Replace("_thumb", "");
                return await _gmCostTrackingService.QueryableList.AnyAsync(c => c.CostTrackId == costTrackId && _docService.DocDocuments.Any(f => f.DocFolder.SystemType == "G" && f.DocFolder.DataKey == "CostTrackId" && f.DocFolder.DataKeyValue == costTrackId && f.DocFile.DocFileName == fileName));
            }
            if (fileType == CPiSavedFileType.QELoggedImage || fileType == CPiSavedFileType.QELoggedImageThumbnail)
            {
                fileName = fileName.Replace("_thumb", "");
                var hasAccess = _gmCostTrackingService.QueryableList.Any(c => c.CostTrackId == costTrackId && _qeLogRepository.QueryableList.Any(l => l.SystemType == "G" && l.DataKey == "CostTrackId" && l.DataKeyValue == costTrackId && l.Attachments.Contains(fileName)));
                return hasAccess;
            }
            if (fileType == CPiSavedFileType.Letter)
            {
                return await _gmCostTrackingService.QueryableList.AnyAsync(a => a.CostTrackId == costTrackId && _letterService.LetterLogs.Any(l => l.DataKey == "CostTrackId" && l.LetFile == fileName && l.LetterLogDetails.Any(d => d.DataKeyValue == costTrackId)));
            }
            if (fileType == CPiSavedFileType.QE)
            {
                return await _gmCostTrackingService.QueryableList.AnyAsync(c => c.CostTrackId == costTrackId && _qeService.QELogs.Any(l => l.DataKey == "CostTrackId" && l.DataKeyValue == costTrackId && l.QEFile == fileName));
            }
            return false;
        }

        private async Task<bool> CanAccessDMSActionFile(int actId, string fileName, CPiSavedFileType fileType)
        {
            if (fileType == CPiSavedFileType.DocMgt || fileType == CPiSavedFileType.DocMgtThumbnail)
            {
                fileName = fileName.Replace("_thumb", "");
                return await _gmActionDueService.QueryableList.AnyAsync(a => a.ActId == actId && _docService.DocDocuments.Any(f => f.DocFolder.SystemType == "D" && f.DocFolder.DataKey == "ActId" && f.DocFolder.DataKeyValue == actId && f.DocFile.DocFileName == fileName));
            }

            if (fileType == CPiSavedFileType.QE)
            {
                var hasAccess = await _dmsActionDueService.QueryableList.AnyAsync(a => a.ActId == actId && _qeService.QELogs.Any(l => l.DataKey == "ActId" && l.DataKeyValue == actId && l.QEFile == fileName));
                if (!hasAccess)
                    hasAccess = await _dmsActionDueService.QueryableList.AnyAsync(a => a.DueDates.Any(dd => dd.DDId == actId) && _qeService.QELogs.Any(l => l.DataKey == "DdId" && l.DataKeyValue == actId && l.QEFile == fileName));
                return hasAccess;
            }
            if (fileType == CPiSavedFileType.QELoggedImage || fileType == CPiSavedFileType.QELoggedImageThumbnail)
            {
                fileName = fileName.Replace("_thumb", "");
                var hasAccess = _dmsActionDueService.QueryableList.Any(a => a.ActId == actId && _qeLogRepository.QueryableList.Any(l => l.SystemType == "D" && l.DataKey == "ActId" && l.DataKeyValue == actId && l.Attachments.Contains(fileName)));
                return hasAccess;
            }
            if (fileType == CPiSavedFileType.Letter)
            {
                return await _dmsActionDueService.QueryableList.AnyAsync(a => a.ActId == actId && _letterService.LetterLogs.Any(l => l.DataKey == "ActId" && l.LetFile == fileName && l.LetterLogDetails.Any(d => d.DataKeyValue == actId)));
            }
            return false;
        }

        //still necessary, otherwise TL can be use as entry point
        private async Task<bool> CanAccessTL(int tlTmkId, string fileName, CPiSavedFileType fileType)
        {
            return await _tlInfoService.TLSearchRecords.AnyAsync(t => t.TLTmkId == tlTmkId && (t.TLSearchImages.Any(im => im.OrigFileName == fileName) || t.TLSearchDocuments.Any(im => im.FileName == fileName)));

            //if (fileType == CPiSavedFileType.Image || fileType == CPiSavedFileType.Thumbnail || fileType == CPiSavedFileType.QELoggedImage)
            //{
            //    return await _tlInfoService.TLSearchRecords.AnyAsync(t => t.TLTmkId == tlTmkId && (t.TLSearchImages.Any(im => im.OrigFileName == fileName) || t.TLSearchDocuments.Any(im => im.FileName == fileName)));
            //}
            //return false;
        }

        private async Task<bool> CanAccessDMSFile(int dmsId, string fileName, CPiSavedFileType fileType)
        {
            if (fileType == CPiSavedFileType.DocMgt || fileType == CPiSavedFileType.DocMgtThumbnail)
            {
                fileName = fileName.Replace("_thumb", "");
                return await _disclosureService.QueryableList.AnyAsync(d=> d.DMSId == dmsId && _docService.DocDocuments.Any(f => f.DocFolder.DataKey == "DMSId" && f.DocFolder.DataKeyValue == dmsId && f.DocFile.DocFileName == fileName));
            }

            if (fileType == CPiSavedFileType.QELoggedImage || fileType == CPiSavedFileType.QELoggedImageThumbnail)
            {
                fileName = fileName.Replace("_thumb", "");
                var hasAccess = _disclosureService.QueryableList.Any(d => d.DMSId == dmsId && _qeLogRepository.QueryableList.Any(l => l.DataKey == "DMSId" && l.DataKeyValue == dmsId && l.Attachments.Contains(fileName)));
                return hasAccess;
            }
            if (fileType == CPiSavedFileType.Letter)
            {
                return await _disclosureService.QueryableList.AnyAsync(d => d.DMSId == dmsId && _letterService.LetterLogs.Any(l => l.DataKey == "DMSId" && l.LetFile == fileName && l.LetterLogDetails.Any(dd => dd.DataKeyValue == dmsId)));
            }
            if (fileType == CPiSavedFileType.QE)
            {
                return await _disclosureService.QueryableList.AnyAsync(d => d.DMSId == dmsId && _qeService.QELogs.Any(l => l.DataKey == "DMSId" && l.DataKeyValue == dmsId && l.QEFile == fileName));
            }
            return false;
        }

        private async Task<bool> CanAccessAMSFile(int annId, string fileName, CPiSavedFileType fileType)
        {
            if (fileType == CPiSavedFileType.QE)
            {
                return await _amsMainService.QueryableList.AnyAsync(a => a.AnnID == annId && _qeService.QELogs.Any(l => l.DataKey == "AnnId" && l.DataKeyValue == annId && l.QEFile==fileName ));
            }
            return false;
        }

        private async Task<bool> CanAccessClearanceFile(int tmcId, string fileName, CPiSavedFileType fileType)
        {
            if (fileType == CPiSavedFileType.DocMgt || fileType == CPiSavedFileType.DocMgtThumbnail)
            {
                fileName = fileName.Replace("_thumb", "");
                return await _clearanceService.QueryableList.AnyAsync(c => c.TmcId == tmcId && _docService.DocDocuments.Any(f => f.DocFolder.DataKey == "TmcId" && f.DocFolder.DataKeyValue == tmcId && f.DocFile.DocFileName == fileName));
            }

            if (fileType == CPiSavedFileType.QELoggedImage || fileType == CPiSavedFileType.QELoggedImageThumbnail)
            {
                fileName = fileName.Replace("_thumb", "");
                var hasAccess = _clearanceService.QueryableList.Any(d => d.TmcId == tmcId && _qeLogRepository.QueryableList.Any(l => l.DataKey == "TmcId" && l.DataKeyValue == tmcId && l.Attachments.Contains(fileName)));
                return hasAccess;
            }
            if (fileType == CPiSavedFileType.Letter)
            {
                return await _clearanceService.QueryableList.AnyAsync(d => d.TmcId == tmcId && _letterService.LetterLogs.Any(l => l.DataKey == "TmcId" && l.LetFile == fileName && l.LetterLogDetails.Any(dd => dd.DataKeyValue == tmcId)));
            }
            if (fileType == CPiSavedFileType.QE)
            {
                return await _clearanceService.QueryableList.AnyAsync(d => d.TmcId == tmcId && _qeService.QELogs.Any(l => l.DataKey == "TmcId" && l.DataKeyValue == tmcId && l.QEFile == fileName));
            }
            return false;
        }

        private async Task<bool> CanAccessPatentClearanceFile(int tmcId, string fileName, CPiSavedFileType fileType)
        {
            if (fileType == CPiSavedFileType.DocMgt || fileType == CPiSavedFileType.DocMgtThumbnail)
            {
                fileName = fileName.Replace("_thumb", "");
                return await _patClearanceService.QueryableList.AnyAsync(c => c.PacId == tmcId && _docService.DocDocuments.Any(f => f.DocFolder.DataKey == "PacId" && f.DocFolder.DataKeyValue == tmcId && f.DocFile.DocFileName == fileName));
            }

            if (fileType == CPiSavedFileType.QELoggedImage || fileType == CPiSavedFileType.QELoggedImageThumbnail)
            {
                fileName = fileName.Replace("_thumb", "");
                var hasAccess = _patClearanceService.QueryableList.Any(d => d.PacId == tmcId && _qeLogRepository.QueryableList.Any(l => l.DataKey == "PacId" && l.DataKeyValue == tmcId && l.Attachments.Contains(fileName)));
                return hasAccess;
            }
            if (fileType == CPiSavedFileType.Letter)
            {
                return await _patClearanceService.QueryableList.AnyAsync(d => d.PacId == tmcId && _letterService.LetterLogs.Any(l => l.DataKey == "PacId" && l.LetFile == fileName && l.LetterLogDetails.Any(dd => dd.DataKeyValue == tmcId)));
            }
            if (fileType == CPiSavedFileType.QE)
            {
                return await _patClearanceService.QueryableList.AnyAsync(d => d.PacId == tmcId && _qeService.QELogs.Any(l => l.DataKey == "PacId" && l.DataKeyValue == tmcId && l.QEFile == fileName));
            }
            return false;
        }

        private async Task<bool> CanAccessProductFile(int productId, string fileName, CPiSavedFileType fileType)
        {
            if (fileType == CPiSavedFileType.DocMgt || fileType == CPiSavedFileType.DocMgtThumbnail)
            {
                fileName = fileName.Replace("_thumb", "");
                return await _productService.QueryableList.AnyAsync(p => p.ProductId==productId && _docService.DocDocuments.Any(f => f.DocFolder.DataKey == "ProductId" && f.DocFolder.DataKeyValue == productId && f.DocFile.DocFileName == fileName));
            }
            return false;
        }
        
        private async Task<bool> CanAccessPatentAsgmtFile(int appId, string fileName, CPiSavedFileType fileType)
        {
            return await _applicationService.CountryApplications.AnyAsync(a => a.AppId == appId && a.AssignmentsHistory.Any(ah=> ah.DocFilePath== fileName));
        }

        private async Task<bool> CanAccessTrademarkAsgmtFile(int tmkId, string fileName, CPiSavedFileType fileType)
        {
            return await _trademarkService.TmkTrademarks.AnyAsync(t => t.TmkId==tmkId && t.AssignmentsHistory.Any(ah => ah.DocFilePath == fileName));
        }

        private async Task<bool> CanAccessPatentLicenseeFile(int appId, string fileName, CPiSavedFileType fileType)
        {
            return await _applicationService.CountryApplications.AnyAsync(a => a.AppId == appId && a.Licensees.Any(ah => ah.DocFilePath == fileName));
        }

        private async Task<bool> CanAccessTrademarkLicenseeFile(int tmkId, string fileName, CPiSavedFileType fileType)
        {
            return await _trademarkService.TmkTrademarks.AnyAsync(t => t.TmkId == tmkId && t.Licensees.Any(ah => ah.DocFilePath == fileName));
        }

        private async Task<bool> CanAccessApplicationDocketRequestFile(int reqId, string fileName, CPiSavedFileType fileType)
        {            
            if (fileType == CPiSavedFileType.QE)
            {
                return await _docketRequestService.PatDocketRequests.AnyAsync(c => c.ReqId == reqId && _qeService.QELogs.Any(l => l.DataKey == "ReqId" && l.DataKeyValue == reqId && l.QEFile == fileName));
            }
            return false;
        }

        private async Task<bool> CanAccessInventionDocketRequestFile(int reqId, string fileName, CPiSavedFileType fileType)
        {            
            if (fileType == CPiSavedFileType.QE)
            {
                return await _docketRequestService.PatDocketInvRequests.AnyAsync(c => c.ReqId == reqId && _qeService.QELogs.Any(l => l.DataKey == "ReqId" && l.DataKeyValue == reqId && l.QEFile == fileName));
            }
            return false;
        }

        private async Task<bool> CanAccessTrademarkDocketRequestFile(int reqId, string fileName, CPiSavedFileType fileType)
        {            
            if (fileType == CPiSavedFileType.QE)
            {
                return await _docketRequestService.TmkDocketRequests.AnyAsync(c => c.ReqId == reqId && _qeService.QELogs.Any(l => l.DataKey == "ReqId" && l.DataKeyValue == reqId && l.QEFile == fileName));
            }
            return false;
        }

        private async Task<bool> CanAccessGMDocketRequestFile(int reqId, string fileName, CPiSavedFileType fileType)
        {            
            if (fileType == CPiSavedFileType.QE)
            {
                return await _docketRequestService.GMDocketRequests.AnyAsync(c => c.ReqId == reqId && _qeService.QELogs.Any(l => l.DataKey == "ReqId" && l.DataKeyValue == reqId && l.QEFile == fileName));
            }
            return false;
        }

        private string GetPolicy(string system, string screenCode)
        {

            switch (screenCode.ToLower())
            {
                case "rms":
                    return RMSAuthorizationPolicy.CanAccessSystem;
            }

            switch (system.ToLower())
            {
                case "patent":
                    return PatentAuthorizationPolicy.CanAccessSystem;

                case "trademark":
                    return TrademarkAuthorizationPolicy.CanAccessSystem;

                case "generalmatter":
                    return GeneralMatterAuthorizationPolicy.CanAccessSystem;

                case "dms":
                    return DMSAuthorizationPolicy.CanAccessSystem;

                case "clearance":
                    return SearchRequestAuthorizationPolicy.CanAccessSystem;

                case "patclearance":
                    return PatentClearanceAuthorizationPolicy.CanAccessSystem;

                default:
                    return SharedAuthorizationPolicy.CanAccessSystem;
            }
        }

    }

    public interface IDocumentPermission
    {
        Task<bool> HasPermission(ClaimsPrincipal user, string system, string screenCode, int key, string fileName, CPiSavedFileType fileType);
    }
}
