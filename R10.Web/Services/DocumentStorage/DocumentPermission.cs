using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using R10.Core.Entities.Trademark;
using R10.Core.Interfaces;
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
        private readonly IActionDueService<PatActionDue, PatDueDate> _patActionDueService;
        private readonly IActionDueService<PatActionDueInv, PatDueDateInv> _patActionDueInvService;
        private readonly IActionDueService<TmkActionDue, TmkDueDate> _tmkActionDueService;
        private readonly ICostTrackingService<PatCostTrack> _patCostTrackingService;
        private readonly ICostTrackingService<PatCostTrackInv> _patCostTrackingInvService;
        private readonly ICostTrackingService<TmkCostTrack> _tmkCostTrackingService;
        private readonly ILetterService _letterService;
        private readonly IPatIDSService _idsService;
        private readonly IQuickEmailService _qeService;
        private readonly IDocumentService _docService;
        private readonly IProductService _productService;
        private readonly IDocketRequestService _docketRequestService;

        public DocumentPermission(IAuthorizationService authorizationService,
                            IInventionService inventionService, ICountryApplicationService applicationService,
                            ITmkTrademarkService trademarkService, IActionDueService<PatActionDue, PatDueDate> patActionDueService,
                            IActionDueService<PatActionDueInv, PatDueDateInv> patActionDueInvService,
                            IActionDueService<TmkActionDue, TmkDueDate> tmkActionDueService,
                            ICostTrackingService<PatCostTrack> patCostTrackingService,
                            ICostTrackingService<PatCostTrackInv> patCostTrackingInvService,
                            ICostTrackingService<TmkCostTrack> tmkCostTrackingService,
                            IAsyncRepository<QELog> qeLogRepository, ILetterService letterService, IAsyncRepository<EFSLog> efsLogRepository,
                            IPatIDSService idsService,
                            IQuickEmailService qeService, IDocumentService docService,
                            IProductService productService,
                            IDocketRequestService docketRequestService)
        {
            _authorizationService = authorizationService;
            _inventionService = inventionService;
            _applicationService = applicationService;
            _trademarkService = trademarkService;
            _patActionDueService = patActionDueService;
            _patActionDueInvService = patActionDueInvService;
            _tmkActionDueService = tmkActionDueService;
            _patCostTrackingService = patCostTrackingService;
            _patCostTrackingInvService = patCostTrackingInvService;
            _tmkCostTrackingService = tmkCostTrackingService;
            _qeLogRepository = qeLogRepository;
            _letterService = letterService;
            _efsLogRepository = efsLogRepository;
            _idsService = idsService;
            _qeService = qeService;
            _docService = docService;
            _productService = productService;
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

                case "act":
                    switch (system)
                    {
                        case SystemType.Patent:
                            return await CanAccessPatentActionFile(key, fileName, fileType);

                        case SystemType.Trademark:
                            return await CanAccessTrademarkActionFile(key, fileName, fileType);

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

                        default:
                            return false;
                    }

                case "costinv":
                    return await CanAccessPatentCostInvFile(key, fileName, fileType);

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

        private string GetPolicy(string system, string screenCode)
        {
            switch (system.ToLower())
            {
                case "patent":
                    return PatentAuthorizationPolicy.CanAccessSystem;

                case "trademark":
                    return TrademarkAuthorizationPolicy.CanAccessSystem;

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
