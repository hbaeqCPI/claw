using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ActiveQueryBuilder.Web.Server.Models;
using Azure.Core;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using DocuSign.eSign.Client;
using DocuSign.eSign.Model;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Entities.GeneralMatter;
using R10.Core.Entities.Shared;
using R10.Core.Helpers;
using R10.Core.Identity;
using R10.Core.Interfaces;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Areas.Shared.ViewModels.SharePoint;
using R10.Web.Extensions;
using R10.Web.Helpers;
using R10.Web.Interfaces;
using R10.Web.Models;
using R10.Web.Security;
using R10.Web.Services;
using R10.Web.Services.DocumentStorage;
using R10.Web.Services.SharePoint;
using SmartFormat.Utilities;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace R10.Web.Areas.Shared.Controllers
{
    
    [Area("Shared"), Authorize]
    public class DocuSignController : Controller
    {
        private readonly IDocuSignService _docuSignService;
        private readonly DocuSignSettings _docuSignSettings;
        private readonly IStringLocalizer<SharedResource> _localizer;
        protected readonly IDocumentStorage _documentStorage;
        private readonly IDocumentService _docService;
        private readonly IDocumentsViewModelService _docViewModelService;
        private readonly ISharePointService _sharePointService;
        private readonly IApplicationDbContext _repository;
        private readonly GraphSettings _graphSettings;
        private readonly ISystemSettings<DefaultSetting> _settings;

        private static readonly string DocuSignFolder = @"Resources\DocuSign";

        public DocuSignController(IDocuSignService docuSignService, IOptions<DocuSignSettings> docuSignSettings,
                                  IStringLocalizer<SharedResource> localizer,
                                  IDocumentStorage documentStorage, IDocumentService docService, IDocumentsViewModelService docViewModelService,
                                  ISharePointService sharePointService, IApplicationDbContext repository, IOptions<GraphSettings> graphSettings,
                                  ISystemSettings<DefaultSetting> settings)
        {
            _docuSignService = docuSignService;
            _docuSignSettings = docuSignSettings.Value;
            _localizer = localizer;
            _documentStorage = documentStorage;
            _docService = docService;
            _docViewModelService = docViewModelService;
            _sharePointService = sharePointService;
            _repository = repository;
            _graphSettings = graphSettings.Value;
            _settings = settings;
        }


        [HttpPost]
        public async Task<IActionResult> SendEnvelopeFromFileUploadWorkflow(List<WorkflowSignatureViewModel> workflows)
        {

            var accessToken = _docuSignService.GetDocuSignAccessToken();
            if (accessToken.ContainsKey("AuthFailed")) {
                return BadRequest(accessToken.GetValueOrDefault("AuthFailed"));
            }
            if (accessToken.ContainsKey("ConsentRequired"))
            {
                return BadRequest(new {consentRequired=true,url= accessToken.GetValueOrDefault("ConsentRequired"), errorMessage = _localizer["DocuSign consent is required first, you can resend this to DocuSign after."].ToString() });
            }

            var success = true;
            foreach (var workflow in workflows) {
                var ok = false;
                if (!string.IsNullOrEmpty(workflow.SharePointDocLibrary))
                   ok = await SendEnvelopeFromSharePointFileUpload(workflow,accessToken);
                else
                    ok = await SendEnvelopeFromFileUpload(workflow, accessToken);

                if (!ok)
                    success = false;
            }

            if (success)
               return Ok(new { success = _localizer["Document has been submitted for signature successfully."].ToString() });
            
            return BadRequest(_localizer["eSignature failed, please make sure that recipient is properly setup and email subject is not empty."].ToString());
        }

        [HttpPost]
        public async Task<IActionResult> ResendEnvelopeFromFileUpload(WorkflowSignatureViewModel workflow)
        {

            var accessToken = _docuSignService.GetDocuSignAccessToken();
            if (accessToken.ContainsKey("AuthFailed"))
            {
                return BadRequest(accessToken.GetValueOrDefault("AuthFailed"));
            }
            if (accessToken.ContainsKey("ConsentRequired"))
            {
                return BadRequest(new { consentRequired = true, url = accessToken.GetValueOrDefault("ConsentRequired"), errorMessage = _localizer["DocuSign consent is required first, you can resend this to DocuSign after."].ToString() });
            }

            var ok = false;
            if (!string.IsNullOrEmpty(workflow.SharePointDocLibrary))
                ok = await SendEnvelopeFromSharePointFileUpload(workflow, accessToken);
            else
                ok = await SendEnvelopeFromFileUpload(workflow, accessToken);
            
            if (ok)
                return Ok(new { success = _localizer["Document has been submitted for signature successfully."].ToString() });

            return BadRequest(_localizer["eSignature failed, please make sure that recipient is properly setup and email subject is not empty."].ToString());
        }

        [HttpPost]
        public async Task<IActionResult> SendAllReviewedDocuments(List<DocReviewDTO> documents)
        {
            var docsToSend = documents.Where(d => d.SignatureReviewed && string.IsNullOrEmpty(d.EnvelopeId)).ToList();

            if (!docsToSend.Any())
                return Ok();

            var accessToken = _docuSignService.GetDocuSignAccessToken();
            if (accessToken.ContainsKey("AuthFailed"))
            {
                return BadRequest(accessToken.GetValueOrDefault("AuthFailed"));
            }
            if (accessToken.ContainsKey("ConsentRequired"))
            {
                return BadRequest(new { consentRequired = true, url = accessToken.GetValueOrDefault("ConsentRequired"), errorMessage = _localizer["DocuSign consent is required first, you can resend this to DocuSign after."].ToString() });
            }

            await ProcessSendingReviewedDocuments(docsToSend, accessToken);

            if (!docsToSend.Any(d=> !d.Successful))
                return Ok(new { success = _localizer["Documents have been submitted for signature successfully."].ToString() });

            return BadRequest(_localizer["Some documents have failed, please make sure that recipient is properly setup and email subject is not empty."].ToString());
        }


        private async Task ProcessSendingReviewedDocuments(List<DocReviewDTO> docsToSend, Dictionary<string, string> accessToken)
        {
            var uploadedFiles = docsToSend.Where(d => d.Source == "DU").ToList();
            var generatedLetters = docsToSend.Where(d => d.Source == "LG").ToList();
            var generatedEFS = docsToSend.Where(d => d.Source == "EFS").ToList();

            var setting = await _settings.GetSetting();

            if (uploadedFiles.Any())
            {
                if (setting.IsSharePointIntegrationOn)
                {
                    foreach (var item in uploadedFiles)
                    {
                        var workflow = new WorkflowSignatureViewModel
                        {
                            UserFile = new WorkflowSignatureDocViewModel
                            {
                                FileName = item.Name,
                                StrId = item.Id,
                                Name = item.Name
                            },
                            QESetupId = (int)item.QESetupId,
                            ParentId = item.ParentId,
                            ScreenCode = item.ScreenCode,
                            RoleLink = item.RoleLink,
                            SystemTypeCode = item.SystemTypeCode,
                            SharePointDocLibrary = item.DocLibrary
                        };
                        item.Successful = await SendEnvelopeFromSharePointFileUpload(workflow, accessToken);
                    }
                }
                else
                {
                    foreach (var item in uploadedFiles)
                    {
                        var workflow = new WorkflowSignatureViewModel
                        {
                            UserFile = new WorkflowSignatureDocViewModel
                            {
                                FileName = item.DocFileName,
                                FileId = item.FileId,
                                Name = item.DocName
                            },
                            QESetupId = (int)item.QESetupId,
                            ParentId = item.ParentId,
                            ScreenCode = item.ScreenCode,
                            RoleLink = item.RoleLink
                        };
                        item.Successful = await SendEnvelopeFromFileUpload(workflow, accessToken);
                    }
                }
            }

            foreach (var item in generatedLetters)
            {
                var doc = new DocsOutSignatureViewModel
                {
                    UserFile = new DocsOutSignatureDocViewModel
                    {
                        FileName = item.LogFile,
                        StrId = item.ItemId,
                        Name = item.Document
                    },

                    QESetupId = (int)item.SignatureQESetupId,
                    ParentId = item.RecKey,
                    ScreenCode = item.ScreenCode,
                    RoleLink = item.RoleLink,
                    SystemTypeCode = item.SystemType,
                    SharePointDocLibrary = setting.IsSharePointIntegrationOn ? SharePointDocLibrary.LetterLog : "",
                    DocLogId = (int)item.DocLogId,
                    DocumentCode = item.DocumentCode
                };
                item.Successful = await SendEnvelopeFromLetterLog(doc, accessToken);
            }

            foreach (var item in generatedEFS)
            {
                var doc = new DocsOutSignatureViewModel
                {
                    UserFile = new DocsOutSignatureDocViewModel
                    {
                        FileName = item.LogFile,
                        StrId = item.ItemId,
                        Name = item.Document
                    },
                    Signer = new DocuSignRecipientParam
                    {
                        Name = item.SignerName,
                        Email = item.SignerEmail,
                        AnchorCode = item.SignerAnchorCode
                    },
                    QESetupId = (int)item.SignatureQESetupId,
                    ParentId = item.RecKey,
                    ScreenCode = item.ScreenCode,
                    RoleLink = item.RoleLink,
                    SystemTypeCode = item.SystemType,
                    SharePointDocLibrary = setting.IsSharePointIntegrationOn ? SharePointDocLibrary.IPFormsLog : "",
                    DocLogId = (int)item.DocLogId,
                    DocumentCode = item.DocumentCode
                };

                if (doc.Signer != null && !string.IsNullOrEmpty(doc.Signer.Name) && !string.IsNullOrEmpty(doc.Signer.Email))
                {
                    var anchorCode = doc.Signer.AnchorCode;
                    if (string.IsNullOrEmpty(anchorCode))
                    {
                        anchorCode = "EFS-Default";
                        doc.Signer.AnchorCode = anchorCode;
                    }

                    var tabs = await _repository.DocuSignAnchorTabs.Where(t => t.DocuSignAnchor.AnchorCode == anchorCode).Include(t => t.DocuSignAnchor).ToListAsync();
                    doc.SignHereTabs = tabs.Where(t => t.AnchorType == "Sign").ToList();
                    doc.InitialHereTabs = tabs.Where(t => t.AnchorType == "Initial").ToList();
                    doc.DateSignedTabs = tabs.Where(t => t.AnchorType == "DateSigned").ToList();

                    item.Successful = await SendEnvelopeFromEFSLog(doc, accessToken);
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> SendEnvelopeFromDocsOutLog(DocsOutSignatureViewModel docsOut)
        {

            var accessToken = _docuSignService.GetDocuSignAccessToken();
            if (accessToken.ContainsKey("AuthFailed"))
            {
                return BadRequest(accessToken.GetValueOrDefault("AuthFailed"));
            }
            if (accessToken.ContainsKey("ConsentRequired"))
            {
                return BadRequest(new { consentRequired = true, url = accessToken.GetValueOrDefault("ConsentRequired"), errorMessage = _localizer["DocuSign consent is required first, you can resend this to DocuSign after."].ToString() });
            }

            var success = true;
            var ok = false;

            if (docsOut.DocumentCode.ToUpper() == "LET") {
                ok = await SendEnvelopeFromLetterLog(docsOut, accessToken);
            }
            else if (docsOut.DocumentCode.ToUpper() == "EFS") {
                if (docsOut.Signer != null && !string.IsNullOrEmpty(docsOut.Signer.Name) && !string.IsNullOrEmpty(docsOut.Signer.Email)) {
                    var anchorCode = docsOut.Signer.AnchorCode;
                    if (string.IsNullOrEmpty(anchorCode)) {
                        anchorCode = "EFS-Default";
                        docsOut.Signer.AnchorCode = anchorCode;
                    }

                    var tabs = await _repository.DocuSignAnchorTabs.Where(t => t.DocuSignAnchor.AnchorCode == anchorCode).Include(t => t.DocuSignAnchor).ToListAsync();
                    docsOut.SignHereTabs = tabs.Where(t => t.AnchorType == "Sign").ToList();
                    docsOut.InitialHereTabs = tabs.Where(t => t.AnchorType == "Initial").ToList();
                    docsOut.DateSignedTabs = tabs.Where(t => t.AnchorType == "DateSigned").ToList();

                    ok = await SendEnvelopeFromEFSLog(docsOut, accessToken);
                }
            }

            if (!ok)
               success = false;

            if (success)
                return Ok(new { success = _localizer["Document has been submitted for signature successfully."].ToString() });

            return BadRequest(_localizer["eSignature failed, please make sure that recipient is properly setup and email subject is not empty."].ToString());
        }


        [HttpPost]
        public async Task<IActionResult> GetAllSignedDocuments(List<DocReviewDTO> documents)
        {
            var docsToRetrieve = documents.Where(d=> !string.IsNullOrEmpty(d.EnvelopeId)).ToList();

            if (!docsToRetrieve.Any())
                return Ok();

            var accessToken = _docuSignService.GetDocuSignAccessToken();
            if (accessToken.ContainsKey("AuthFailed"))
            {
                return BadRequest(accessToken.GetValueOrDefault("AuthFailed"));
            }
            if (accessToken.ContainsKey("ConsentRequired"))
            {
                return BadRequest(new { consentRequired = true, url = accessToken.GetValueOrDefault("ConsentRequired"), errorMessage = _localizer["DocuSign consent is required first, you can resend this to DocuSign after."].ToString() });
            }
            await ProcessRetrievalSignedDocuments(docsToRetrieve, accessToken.GetValueOrDefault("AccessToken"));

            if (!docsToRetrieve.Any(d => !d.Successful))
                return Ok();

            return BadRequest(_localizer["Some documents are not completely signed yet."].ToString());
        }

        
        private async Task ProcessRetrievalSignedDocuments(List<DocReviewDTO> docsToRetrieve, string accessToken)
        {
            var uploadedFiles = docsToRetrieve.Where(d => d.Source == "DU").ToList();
            var docsOut = docsToRetrieve.Where(d => d.Source == "LG" || d.Source == "EFS").ToList();

            var setting = await _settings.GetSetting();
            if (uploadedFiles.Any())
            {
                if (setting.IsSharePointIntegrationOn)
                {
                    foreach (var item in uploadedFiles)
                    {
                        var doc = new DocDocumentListViewModel {
                            EnvelopeId = item.EnvelopeId,
                            DocLibrary= item.DocLibrary,
                            DocLibraryFolder = item.DocLibraryFolder,
                            ParentId = item.ParentId,
                            Id = item.Id,
                            DocName = item.Name,
                        };
                        item.Successful = await ProcessSignedDocumentsAndSaveToSharePoint(doc, accessToken);
                    }
                }
                else
                {
                    foreach (var item in uploadedFiles)
                    {
                        var doc = new DocDocumentListViewModel
                        {
                            DocName= item.DocName,
                            SystemType= item.SystemType,
                            ScreenCode= item.ScreenCode,
                            ParentId= item.ParentId,
                            EnvelopeId= item.EnvelopeId,
                            FileId= (int)item.FileId,
                            DataKey= item.DataKey,
                        };
                        item.Successful = await ProcessSignedDocumentsAndSave(doc, accessToken);
                    }
                }
            }

            foreach (var item in docsOut)
            {
                var doc = new DocsOutSignatureSignedViewModel
                {
                    DocLogId= (int)item.DocLogId,
                    ParentId= item.RecKey,
                    EnvelopeId= item.EnvelopeId,
                    LetFile= item.LogFile,
                    ScreenCode= item.ScreenCode,
                    SystemTypeCode= item.SystemType,
                    DocumentCode= item.DocumentCode
                };
                if (setting.IsSharePointIntegrationOn)
                    item.Successful = await ProcessSignedDocsOutAndSaveToSharePoint(doc, accessToken);
                else
                    item.Successful = await ProcessSignedDocsOutAndSave(doc, accessToken);
            }
            
        }

        [HttpPost]
        public async Task<IActionResult> GetSignedDocumentsAndSave(DocDocumentListViewModel viewModelParam)
        {
            var accessToken = _docuSignService.GetDocuSignAccessToken();
            if (accessToken.ContainsKey("AccessToken"))
            {
                var ok = await ProcessSignedDocumentsAndSave(viewModelParam, accessToken.GetValueOrDefault("AccessToken"));
                if (ok) return Ok();
                return BadRequest(_localizer["Document is not completely signed yet"].Value);
            }
            if (accessToken.ContainsKey("AuthFailed"))
            {
                return BadRequest(accessToken.GetValueOrDefault("AuthFailed"));
            }
            if (accessToken.ContainsKey("ConsentRequired"))
            {
                return BadRequest(new { consentRequired = true, url = accessToken.GetValueOrDefault("ConsentRequired"), errorMessage = _localizer["DocuSign consent is required first, you can resend this to DocuSign after."].ToString() });
            }
            return BadRequest();
        }

        private async Task<bool> ProcessSignedDocumentsAndSave(DocDocumentListViewModel viewModelParam,string accessToken)
        {
            var envelopeParam = new DocuSignEnvelopeGetParam()
            {
                AuthServer = _docuSignSettings.AuthServer,
                AccessToken = accessToken,
                EnvelopeId = viewModelParam.EnvelopeId
            };

            var envelope = await _docuSignService.GetEnvelopeData(envelopeParam);
            if (envelope.Status.ToLower() == "completed")
            {
                var stream = _docuSignService.GetSignedDocuments(envelopeParam);
                if (stream != null)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        stream.CopyTo(memoryStream);
                        memoryStream.Position = 0;

                        var viewModel = new DocDocumentViewModel
                        {
                            DocFileName = $"{viewModelParam.DocName}-Signed.pdf",
                            CreatedBy = User.GetUserName(),
                            UpdatedBy = User.GetUserName(),
                            DateCreated = DateTime.Now,
                            LastUpdate = DateTime.Now,
                            SystemType = viewModelParam.SystemType,
                            ScreenCode = viewModelParam.ScreenCode,
                            ParentId = viewModelParam.ParentId,
                            DataKey = viewModelParam.DataKey,
                            SignedDoc = true
                        };
                        await _docViewModelService.SaveDocumentFromStream(viewModel, memoryStream);
                        await _docService.MarkSignedDoc(viewModelParam.FileId, (int)viewModel.FileId);
                    }
                    return true;
                }
                
            }
            return false;
        }

        [HttpPost]
        public async Task<IActionResult> GetSignedDocumentsAndSaveToSharePoint(DocDocumentListViewModel viewModelParam)
        {
            var accessToken = _docuSignService.GetDocuSignAccessToken();
            if (accessToken.ContainsKey("AccessToken"))
            {
                var ok = await ProcessSignedDocumentsAndSaveToSharePoint(viewModelParam, accessToken.GetValueOrDefault("AccessToken"));
                if (ok) return Ok();
                return BadRequest(_localizer["Document is not completely signed yet"].Value);
            }
            if (accessToken.ContainsKey("AuthFailed"))
            {
                return BadRequest(accessToken.GetValueOrDefault("AuthFailed"));
            }
            if (accessToken.ContainsKey("ConsentRequired"))
            {
                return BadRequest(new { consentRequired = true, url = accessToken.GetValueOrDefault("ConsentRequired"), errorMessage = _localizer["DocuSign consent is required first, you can resend this to DocuSign after."].ToString() });
            }
            return BadRequest();
        }

        private async Task<bool> ProcessSignedDocumentsAndSaveToSharePoint(DocDocumentListViewModel viewModelParam, string accessToken)
        {
            var envelopeParam = new DocuSignEnvelopeGetParam()
            {
                AuthServer = _docuSignSettings.AuthServer,
                AccessToken = accessToken,
                EnvelopeId = viewModelParam.EnvelopeId
            };

            var envelope = await _docuSignService.GetEnvelopeData(envelopeParam);
            if (envelope.Status.ToLower() == "completed")
            {
                var stream = _docuSignService.GetSignedDocuments(envelopeParam);
                if (stream != null)
                {
                    var recKey = "";
                    switch (viewModelParam.DocLibraryFolder)
                    {
                        case SharePointDocLibraryFolder.Invention:
                            var inv = await _repository.Inventions.Where(r => r.InvId == viewModelParam.ParentId).FirstOrDefaultAsync();
                            if (inv != null)
                            {
                                recKey = inv.CaseNumber;
                            }
                            break;
                        case SharePointDocLibraryFolder.Application:
                            var ca = await _repository.CountryApplications.Where(r => r.AppId == viewModelParam.ParentId).FirstOrDefaultAsync();
                            if (ca != null)
                            {
                                recKey = SharePointViewModelService.BuildRecKey(ca.CaseNumber, ca.Country, ca.SubCase);
                            }
                            break;
                        case SharePointDocLibraryFolder.Trademark:
                            var tmk = await _repository.TmkTrademarks.Where(r => r.TmkId == viewModelParam.ParentId).FirstOrDefaultAsync();
                            if (tmk != null)
                            {
                                recKey = SharePointViewModelService.BuildRecKey(tmk.CaseNumber, tmk.Country, tmk.SubCase);
                            }
                            break;
                        case SharePointDocLibraryFolder.GeneralMatter:
                            var gm = await _repository.GMMatters.Where(r => r.MatId == viewModelParam.ParentId).FirstOrDefaultAsync();
                            if (gm != null)
                            {
                                recKey = SharePointViewModelService.BuildGMRecKey(gm.CaseNumber, gm.SubCase);
                            }
                            break;

                            //case SharePointDocLibraryFolder.Action:
                            //    break;
                            //case SharePointDocLibraryFolder.Cost:
                            //    break;
                    }

                    if (!string.IsNullOrEmpty(recKey))
                    {
                        var folders = SharePointViewModelService.GetDocumentFolders(viewModelParam.DocLibraryFolder, recKey);
                        var graphClient = _sharePointService.GetGraphClient();

                        var docName = viewModelParam.DocName.Split(".");
                        var fileName = $"{docName[0]}-Signed.pdf";
                        var result = await graphClient.UploadSiteFile(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, viewModelParam.DocLibrary, folders, stream, fileName);
                        if (!string.IsNullOrEmpty(result.DriveItemId))
                        {
                            await _docService.MarkSignedDocForSharePoint(viewModelParam.Id, result.DriveItemId, fileName);
                        }
                    }
                    return true;
                }
            }
            return false;
        }

        [HttpPost]
        public async Task<IActionResult> GetSignedDocsOutAndSaveToSharePoint(DocsOutSignatureSignedViewModel viewModelParam)
        {
            var accessToken = _docuSignService.GetDocuSignAccessToken();
            if (accessToken.ContainsKey("AccessToken"))
            {
                var ok = await ProcessSignedDocsOutAndSaveToSharePoint(viewModelParam, accessToken.GetValueOrDefault("AccessToken"));
                if (ok) return Ok();
                return BadRequest(_localizer["Document is not completely signed yet"].Value);
            }
            if (accessToken.ContainsKey("AuthFailed"))
            {
                return BadRequest(accessToken.GetValueOrDefault("AuthFailed"));
            }
            if (accessToken.ContainsKey("ConsentRequired"))
            {
                return BadRequest(new { consentRequired = true, url = accessToken.GetValueOrDefault("ConsentRequired"), errorMessage = _localizer["DocuSign consent is required first, you can resend this to DocuSign after."].ToString() });
            }
            return BadRequest();
        }


        private async Task<bool> ProcessSignedDocsOutAndSaveToSharePoint(DocsOutSignatureSignedViewModel viewModelParam, string accessToken)
        {
            var envelopeParam = new DocuSignEnvelopeGetParam()
            {
                AuthServer = _docuSignSettings.AuthServer,
                AccessToken = accessToken,
                EnvelopeId = viewModelParam.EnvelopeId
            };

            var envelope = await _docuSignService.GetEnvelopeData(envelopeParam);
            if (envelope.Status.ToLower() == "completed")
            {
                var stream = _docuSignService.GetSignedDocuments(envelopeParam);
                var docLibraryFolder = "";
                var systemFolder = "";
                if (stream != null)
                {
                    var recKey = "";
                    switch (viewModelParam.ScreenCode)
                    {
                        case ScreenCode.Invention:
                            systemFolder = "Patent";
                            docLibraryFolder = SharePointDocLibraryFolder.Invention;
                            var inv = await _repository.Inventions.Where(r => r.InvId == viewModelParam.ParentId).FirstOrDefaultAsync();
                            if (inv != null)
                            {
                                recKey = inv.CaseNumber;
                            }
                            break;
                        case ScreenCode.Application:
                            systemFolder = "Patent";
                            docLibraryFolder = SharePointDocLibraryFolder.Application;
                            var ca = await _repository.CountryApplications.Where(r => r.AppId == viewModelParam.ParentId).FirstOrDefaultAsync();
                            if (ca != null)
                            {
                                recKey = SharePointViewModelService.BuildRecKey(ca.CaseNumber, ca.Country, ca.SubCase);
                            }
                            break;
                        case ScreenCode.Trademark:
                            systemFolder = "Trademark";
                            docLibraryFolder = SharePointDocLibraryFolder.Trademark;
                            var tmk = await _repository.TmkTrademarks.Where(r => r.TmkId == viewModelParam.ParentId).FirstOrDefaultAsync();
                            if (tmk != null)
                            {
                                recKey = SharePointViewModelService.BuildRecKey(tmk.CaseNumber, tmk.Country, tmk.SubCase);
                            }
                            break;
                        case ScreenCode.GeneralMatter:
                            systemFolder = "General Matter";
                            docLibraryFolder = SharePointDocLibraryFolder.GeneralMatter;
                            var gm = await _repository.GMMatters.Where(r => r.MatId == viewModelParam.ParentId).FirstOrDefaultAsync();
                            if (gm != null)
                            {
                                recKey = SharePointViewModelService.BuildGMRecKey(gm.CaseNumber, gm.SubCase);
                            }
                            break;

                            //case ScreenCode.Action:
                            //    break;
                            //case ScreenCode.Cost:
                            //    break;
                    }

                    if (!string.IsNullOrEmpty(recKey))
                    {
                        var folders = new List<string> { systemFolder };
                        var subFolders = SharePointViewModelService.GetDocumentFolders(docLibraryFolder, recKey);
                        folders.AddRange(subFolders);
                        var graphClient = _sharePointService.GetGraphClient();

                        var docName = viewModelParam.LetFile.Split(".");
                        var fileName = $"{docName[0]}-Signed.pdf";
                        var docLibrary = viewModelParam.DocumentCode.ToUpper() == "LET" ? SharePointDocLibrary.LetterLog : SharePointDocLibrary.IPFormsLog;
                        var result = await graphClient.UploadSiteFile(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, docLibrary, folders, stream, fileName);
                        if (!string.IsNullOrEmpty(result.DriveItemId))
                        {
                            if (viewModelParam.DocumentCode.ToUpper() == "LET")
                            {
                                await _docService.MarkSignedLetter(viewModelParam.DocLogId, fileName, result.DriveItemId, User.GetUserName());
                            }
                            else if (viewModelParam.DocumentCode.ToUpper() == "EFS")
                            {
                                await _docService.MarkSignedEFSLog(viewModelParam.DocLogId, fileName, result.DriveItemId, User.GetUserName());
                            }
                        }
                    }
                }
                return true;
            }
            return false;
        }


        [HttpPost]
        public async Task<IActionResult> GetSignedDocsOutAndSave(DocsOutSignatureSignedViewModel viewModelParam)
        {
            var accessToken = _docuSignService.GetDocuSignAccessToken();
            if (accessToken.ContainsKey("AccessToken"))
            {
                var ok = await ProcessSignedDocsOutAndSave(viewModelParam, accessToken.GetValueOrDefault("AccessToken"));
                if (ok) return Ok();
                return BadRequest(_localizer["Document is not completely signed yet"].Value);
            }
            if (accessToken.ContainsKey("AuthFailed"))
            {
                return BadRequest(accessToken.GetValueOrDefault("AuthFailed"));
            }
            if (accessToken.ContainsKey("ConsentRequired"))
            {
                return BadRequest(new { consentRequired = true, url = accessToken.GetValueOrDefault("ConsentRequired"), errorMessage = _localizer["DocuSign consent is required first, you can resend this to DocuSign after."].ToString() });
            }
            return BadRequest();
        }

        private async Task<bool> ProcessSignedDocsOutAndSave(DocsOutSignatureSignedViewModel viewModelParam, string accessToken)
        {
            var envelopeParam = new DocuSignEnvelopeGetParam()
            {
                AuthServer = _docuSignSettings.AuthServer,
                AccessToken = accessToken,
                EnvelopeId = viewModelParam.EnvelopeId
            };

            var envelope = await _docuSignService.GetEnvelopeData(envelopeParam);
            if (envelope.Status.ToLower() == "completed")
            {
                var stream = _docuSignService.GetSignedDocuments(envelopeParam);
                if (stream != null)
                {
                    var fileName = $"{viewModelParam.DocumentCode}-{DateTime.Now:yy-MM-dd-hhmmsstt}-Signed.pdf";
                    using (var memoryStream = new MemoryStream())
                    {
                        stream.CopyTo(memoryStream);
                        memoryStream.Position = 0;

                        if (viewModelParam.DocumentCode.ToUpper() == "LET")
                        {
                            var outputFile = Path.Combine(_documentStorage.LetterLogFolder, fileName).Replace(@"\", "/");
                            await _documentStorage.SaveFile(memoryStream.ToArray(), outputFile, null);
                            await _docService.MarkSignedLetter(viewModelParam.DocLogId, fileName, "", User.GetUserName());
                        }
                        else if (viewModelParam.DocumentCode.ToUpper() == "EFS")
                        {
                            var outputFile = Path.Combine(_documentStorage.EFSLogFolder, fileName).Replace(@"\", "/");
                            await _documentStorage.SaveFile(memoryStream.ToArray(), outputFile, null);
                            await _docService.MarkSignedEFSLog(viewModelParam.DocLogId, fileName, "", User.GetUserName());
                        }

                    }
                }
                return true;
            }
            return false;
        }

        private async Task<bool> SendEnvelopeFromFileUpload(WorkflowSignatureViewModel workflow, Dictionary<string, string> accessToken)
        {
            var path = _documentStorage.BuildPath(_documentStorage.ImageRootFolder, "", workflow.UserFile.FileName);
            var file = await _documentStorage.GetFileStream(path);
            var fileInBase64String = Convert.ToBase64String(file.ToArray());
            var fileName = workflow.UserFile.Name;
            var extension = "";
            if (workflow.UserFile.FileName.Contains("."))
            {
                extension = Path.GetExtension(workflow.UserFile.FileName).Substring(1);
                fileName = workflow.UserFile.FileName.Split(".")[0];
            }

            var buildDataParam = new DocuSignEnvelopeBuildDataParam
            {
                QESetupId = workflow.QESetupId,
                ParentId = workflow.ParentId,
                ScreenCode = workflow.ScreenCode,
                SystemTypeCode=workflow.SystemTypeCode,
                RoleLink = workflow.RoleLink,
                DocToSignInBase64String = fileInBase64String,
                FileName = fileName,
                FileExtension = extension,
                ClaimsPrincipal = User
            };
            var envelopeParam = await _docuSignService.BuildEnvelopeData(buildDataParam);
            if (envelopeParam != null && envelopeParam.Signers.Any())
            {
                var signHereTabs = new List<DocuSignAnchorTab>();
                var initialHereTabs = new List<DocuSignAnchorTab>();
                var dateSignedTabs = new List<DocuSignAnchorTab>();

                foreach (var signer in envelopeParam.Signers)
                {
                    var anchorCode = signer.AnchorCode;
                    if (string.IsNullOrEmpty(anchorCode))
                    {
                        anchorCode = "Default";
                    }
                    var tabs = await _repository.DocuSignAnchorTabs.Where(t => t.DocuSignAnchor.AnchorCode == anchorCode).Include(t=> t.DocuSignAnchor).ToListAsync();
                    if (tabs.Count > 0)
                    {
                        signHereTabs.AddRange(tabs.Where(t => t.AnchorType == "Sign").ToList());
                        initialHereTabs.AddRange(tabs.Where(t => t.AnchorType == "Initial").ToList());
                        dateSignedTabs.AddRange(tabs.Where(t => t.AnchorType == "DateSigned").ToList());
                    }
                }

                envelopeParam.AccessToken = accessToken.GetValueOrDefault("AccessToken");
                envelopeParam.SignHereTabs = signHereTabs;
                envelopeParam.InitialHereTabs = initialHereTabs;
                envelopeParam.DateSignedTabs = dateSignedTabs;
                
                var envelopeId = _docuSignService.SendEnvelopeViaEmail(envelopeParam);
                await _docuSignService.AddRecipients(envelopeParam, envelopeId);
                if (workflow.UserFile.FileId > 0) {
                    await _docService.SetEnvelopeId((int)workflow.UserFile.FileId, envelopeId);
                    return true;
                }
            }
            return false;
        }

        private async Task<bool> SendEnvelopeFromSharePointFileUpload(WorkflowSignatureViewModel workflow, Dictionary<string, string> accessToken)
        {
            var graphClient = _sharePointService.GetGraphClient();
            var file = await graphClient.DownloadSiteDriveItem(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, workflow.SharePointDocLibrary, workflow.UserFile.StrId);
            
            using (var stream = new MemoryStream())
            {
                file.CopyTo(stream);
                var fileInBase64String = Convert.ToBase64String(stream.ToArray());
                var fileName = workflow.UserFile.Name;
                var extension = "";
                if (workflow.UserFile.FileName.Contains("."))
                {
                    extension = Path.GetExtension(workflow.UserFile.FileName).Substring(1);
                }

                var buildDataParam = new DocuSignEnvelopeBuildDataParam
                {
                    QESetupId = workflow.QESetupId,
                    ParentId = workflow.ParentId,
                    ScreenCode = workflow.ScreenCode,
                    SystemTypeCode = workflow.SystemTypeCode,
                    RoleLink = workflow.RoleLink,
                    DocToSignInBase64String = fileInBase64String,
                    FileName = fileName,
                    FileExtension = extension,
                    ClaimsPrincipal = User
                };
                var envelopeParam = await _docuSignService.BuildEnvelopeData(buildDataParam);
                if (envelopeParam != null && envelopeParam.Signers.Any())
                {
                    var signHereTabs = new List<DocuSignAnchorTab>();
                    var initialHereTabs = new List<DocuSignAnchorTab>();
                    var dateSignedTabs = new List<DocuSignAnchorTab>();

                    foreach (var signer in envelopeParam.Signers)
                    {
                        var anchorCode = signer.AnchorCode;
                        if (string.IsNullOrEmpty(anchorCode))
                        {
                            anchorCode = "Default";
                        }
                        var tabs = await _repository.DocuSignAnchorTabs.Where(t => t.DocuSignAnchor.AnchorCode == anchorCode).Include(t => t.DocuSignAnchor).ToListAsync();
                        if (tabs.Count > 0)
                        {
                            signHereTabs.AddRange(tabs.Where(t => t.AnchorType == "Sign").ToList());
                            initialHereTabs.AddRange(tabs.Where(t => t.AnchorType == "Initial").ToList());
                            dateSignedTabs.AddRange(tabs.Where(t => t.AnchorType == "DateSigned").ToList());
                        }
                    }

                    envelopeParam.AccessToken = accessToken.GetValueOrDefault("AccessToken");
                    envelopeParam.SignHereTabs = signHereTabs;
                    envelopeParam.InitialHereTabs = initialHereTabs;
                    envelopeParam.DateSignedTabs = dateSignedTabs;

                    var envelopeId = _docuSignService.SendEnvelopeViaEmail(envelopeParam);
                    await _docuSignService.AddRecipients(envelopeParam, envelopeId);
                    if (workflow.UserFile.FileId > 0)
                    {
                        await _docService.SetEnvelopeId((int)workflow.UserFile.FileId, envelopeId);
                        return true;
                    }
                    else if (!string.IsNullOrEmpty(workflow.UserFile.StrId))
                    {
                        await _docService.SetEnvelopeIdForSharePointFile(workflow.UserFile.StrId, envelopeId);
                        return true;
                    }
                }
            }

            
            return false;
        }


        private async Task<bool> SendEnvelopeFromLetterLog(DocsOutSignatureViewModel letter, Dictionary<string, string> accessToken)
        {
            Stream file;
            if (!string.IsNullOrEmpty(letter.SharePointDocLibrary))
            {
                var graphClient = _sharePointService.GetGraphClient();
                file = await graphClient.DownloadSiteDriveItem(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, letter.SharePointDocLibrary, letter.UserFile.StrId);
            }
            else {
                var path = _documentStorage.BuildPath(_documentStorage.LetterLogFolder, "", letter.UserFile.FileName);
                file = await _documentStorage.GetFileStream(path);
            }

            using (var stream = new MemoryStream())
            {
                file.Position = 0;
                file.CopyTo(stream);
                var fileInBase64String = Convert.ToBase64String(stream.ToArray());
                var fileName = letter.UserFile.Name;
                var extension = "";
                if (letter.UserFile.FileName.Contains("."))
                {
                    extension = Path.GetExtension(letter.UserFile.FileName).Substring(1);
                }

                var buildDataParam = new DocuSignEnvelopeBuildDataParam
                {
                    QESetupId = letter.QESetupId,
                    ParentId = letter.ParentId,
                    ScreenCode = letter.ScreenCode,
                    SystemTypeCode = letter.SystemTypeCode,
                    RoleLink = letter.RoleLink,
                    DocToSignInBase64String = fileInBase64String,
                    FileName = fileName,
                    FileExtension = extension,
                    ClaimsPrincipal = User
                };
                var envelopeParam = await _docuSignService.BuildEnvelopeData(buildDataParam);
                if (envelopeParam != null && envelopeParam.Signers.Any())
                {
                    foreach (var signer in envelopeParam.Signers)
                    {
                        var anchorCode = signer.AnchorCode;
                        if (string.IsNullOrEmpty(anchorCode)) {
                            anchorCode = "Default";
                        }
                        var tabs = await _repository.DocuSignAnchorTabs.Where(t => t.DocuSignAnchor.AnchorCode == anchorCode).Include(t => t.DocuSignAnchor).ToListAsync();
                        if (tabs.Count > 0) {
                            letter.SignHereTabs = new List<DocuSignAnchorTab>();
                            letter.InitialHereTabs = new List<DocuSignAnchorTab>();
                            letter.DateSignedTabs = new List<DocuSignAnchorTab>();

                            letter.SignHereTabs.AddRange(tabs.Where(t => t.AnchorType == "Sign").ToList());
                            letter.InitialHereTabs.AddRange(tabs.Where(t => t.AnchorType == "Initial").ToList());
                            letter.DateSignedTabs.AddRange(tabs.Where(t => t.AnchorType == "DateSigned").ToList());
                        }
                    }
                    envelopeParam.AccessToken = accessToken.GetValueOrDefault("AccessToken");
                    envelopeParam.SignHereTabs = letter.SignHereTabs;
                    envelopeParam.InitialHereTabs = letter.InitialHereTabs;
                    envelopeParam.DateSignedTabs = letter.DateSignedTabs;

                    var envelopeId = _docuSignService.SendEnvelopeViaEmail(envelopeParam);
                    await _docuSignService.AddRecipients(envelopeParam, envelopeId);
                    await _docService.SetEnvelopeIdForLetterFile(letter.DocLogId, envelopeId);
                    return true;
                }
            }


            return false;
        }

        
        private async Task<bool> SendEnvelopeFromEFSLog(DocsOutSignatureViewModel efs, Dictionary<string, string> accessToken)
        {
            Stream file;
            if (!string.IsNullOrEmpty(efs.SharePointDocLibrary))
            {
                var graphClient = _sharePointService.GetGraphClient();
                file = await graphClient.DownloadSiteDriveItem(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, efs.SharePointDocLibrary, efs.UserFile.StrId);
            }
            else
            {
                var path = _documentStorage.BuildPath(_documentStorage.EFSLogFolder, "", efs.UserFile.FileName);
                file = await _documentStorage.GetFileStream(path);
            }

            using (var stream = new MemoryStream())
            {
                file.Position = 0;
                file.CopyTo(stream);
                var fileInBase64String = Convert.ToBase64String(stream.ToArray());
                var fileName = efs.UserFile.Name;
                var extension = "";
                if (efs.UserFile.FileName.Contains("."))
                {
                    extension = Path.GetExtension(efs.UserFile.FileName).Substring(1);
                }

                var buildDataParam = new DocuSignEnvelopeBuildDataParam
                {
                    QESetupId = efs.QESetupId,
                    ParentId = efs.ParentId,
                    ScreenCode = efs.ScreenCode,
                    SystemTypeCode = efs.SystemTypeCode,
                    RoleLink = efs.RoleLink,
                    DocToSignInBase64String = fileInBase64String,
                    FileName = fileName,
                    FileExtension = extension,
                    ClaimsPrincipal = User,
                    Signers = new List<DocuSignRecipientParam> { efs.Signer } 
                };
                var envelopeParam = await _docuSignService.BuildEnvelopeData(buildDataParam);
                if (envelopeParam != null && envelopeParam.Signers.Any())
                {
                    envelopeParam.AccessToken = accessToken.GetValueOrDefault("AccessToken");
                    envelopeParam.SignHereTabs = efs.SignHereTabs;
                    envelopeParam.InitialHereTabs = efs.InitialHereTabs;
                    envelopeParam.DateSignedTabs = efs.DateSignedTabs;

                    var envelopeId = _docuSignService.SendEnvelopeViaEmail(envelopeParam);
                    await _docuSignService.AddRecipients(envelopeParam, envelopeId);
                    await _docService.SetEnvelopeIdForEFSFile(efs.DocLogId, envelopeId);
                    return true;
                }
            }
            return false;
        }

        //private Dictionary<string,string>  GetDocuSignAccessToken()
        //{
        //    var privateKeyPath = Path.Combine(_hostingEnvironment.ContentRootPath, DocuSignFolder, _docuSignSettings.PrivateKeyFile);
        //    var privateKey = System.IO.File.ReadAllBytes(privateKeyPath);

        //    try
        //    {
        //        var accessToken = _docuSignService.AuthenticateWithJWT("ESignature", _docuSignSettings.ClientId, _docuSignSettings.ImpersonatedUserId,
        //                                                _docuSignSettings.AuthServer, privateKey);

        //        if (accessToken != null)
        //        {
        //            return new Dictionary<string, string> { {"AccessToken", accessToken.access_token } };
        //        }
        //    }
        //    catch (ApiException apiException)
        //    {
        //        if (apiException.Message.Contains("consent_required"))
        //        {
        //            // build a URL to provide consent for this Integration Key and this userId
        //            string url = $"https://{_docuSignSettings.AuthServer}/oauth/auth?response_type=code&scope=impersonation%20signature" +
        //                         $"&client_id={_docuSignSettings.ClientId}&redirect_uri={_docuSignSettings.DeveloperServer}";
        //            return new Dictionary<string, string> { { "ConsentRequired", url } };
        //        }
        //    }
        //    return new Dictionary<string, string> { { "AuthFailed", _localizer["Authentication to DocuSign failed. Please check your DocuSign settings."].ToString() } };
        //}

        //private List<string> GetDocumentFolders(string docLibraryFolder, string recKey)
        //{
        //    recKey = recKey.Replace("/", "");
        //    var recKeys = recKey.Split(SharePointSeparator.Folder).ToList();
        //    var folders = new List<string> { docLibraryFolder };
        //    folders.AddRange(recKeys);

            
        //    return folders;
        //}
    }
}
