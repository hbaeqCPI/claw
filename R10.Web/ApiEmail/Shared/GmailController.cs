using Kendo.Mvc.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using OpenIddict.Validation.AspNetCore;
using R10.Core.DTOs;
using R10.Core.Entities.Documents;
using R10.Core.Entities.Shared;
using R10.Core.Helpers;
using R10.Core.Interfaces;
using R10.Web.ApiEmail.Models;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Areas.Shared.ViewModels.SharePoint;
using R10.Web.Interfaces;
using R10.Web.Security;
using R10.Web.Services;
using R10.Web.Services.SharePoint;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace R10.Web.ApiEmail.Shared
{
    [Route("~/emailapi/[controller]")]
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme, Policy = SharedAuthorizationPolicy.CanAccessSystem)]
    [EnableCors("EmailAddInCORSPolicy")]
    [ApiController]
    public class GmailController : Microsoft.AspNetCore.Mvc.Controller
    {
        private readonly IDocumentService _documentService;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly ISystemSettings<DefaultSetting> _settings;
        private readonly ISharePointService _sharePointService;
        private readonly GraphSettings _graphSettings;
        private readonly ISharePointViewModelService _sharePointViewModelService;

        public GmailController(IDocumentService documentService, 
            IHostingEnvironment hostingEnvironment, 
            ISystemSettings<DefaultSetting> settings,
            ISharePointService sharePointService,
            IOptions<GraphSettings> graphSettings,
            ISharePointViewModelService sharePointViewModelService
            )
        {
            _documentService = documentService;
            _hostingEnvironment = hostingEnvironment;
            _settings = settings;
            _sharePointService = sharePointService;
            _graphSettings = graphSettings.Value;
            _sharePointViewModelService = sharePointViewModelService;
        }


        [HttpPost("savetocpi")]
        public async Task<Microsoft.AspNetCore.Mvc.ActionResult> SaveToCPI(string userEmail, string systemType, string screenCode, string selectedCases, string gmailMsgId, string msgSubject, [FromBody] string encodedMsg)
        {
            try
            {
                if (string.IsNullOrEmpty(gmailMsgId))
                {
                    throw new ArgumentNullException("Email Id");
                }

                var settings = await _settings.GetSetting();
                var result = new OutlookLinkedCases();

                if (settings.IsSharePointIntegrationOn)
                {
                    result = await SaveGmailToSharePoint(userEmail, systemType, screenCode, selectedCases, gmailMsgId, msgSubject, encodedMsg);
                }
                else
                {
                    var contentRootPath = _hostingEnvironment.ContentRootPath;
                    result = await _documentService.SaveGmailEmail(contentRootPath, userEmail, systemType, screenCode, selectedCases, gmailMsgId, msgSubject, encodedMsg);  
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }

            
        }

        [HttpGet("getlinkedcases")]
        public async Task<ActionResult<CaseLogDTO[]>> GetLinkedCases(string gmailMsgId)
        {
            try
            {
                var results = await _documentService.GetGmailCaseLogByEmailId(gmailMsgId);
                return results;
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        private async Task<OutlookLinkedCases> SaveGmailToSharePoint(string userEmail, string systemType, string screenCode, string selectedCases, string gmailMsgId, string msgSubject, [FromBody] string encodedMsg)
        {
            /*
            SAMPLE 
            userEmail: tnguyencpi@gmail.com
            systemType: P
            gmailMsgId: 18d38997618b7c54
            msgSubject: Test Subject
            selectedCases: AppId|900223
            screenCode: CA
            encodedMsg: super long encoded string for email w/o attachments
            */
            var docLibrary = _sharePointViewModelService.GetDocLibraryFromSystemTypeCode(systemType);
            var docLibraryFolder = _sharePointViewModelService.GetDocLibraryFolderFromScreenCode(screenCode);

            var graphClient = _sharePointService.GetGraphClientByClientCredentials();
            
            var userName = User.GetUserName();            
            var settings = await _settings.GetSetting();

            var processedCases = new OutlookLinkedCases
            {
                FileId = 0,
                DataKeyValue = new List<OutlookProcessedCases>()
            };
            var spFileIdList = selectedCases.Split(",").Where(d => !string.IsNullOrEmpty(d)).ToList();

            foreach (var recordLink in spFileIdList)
            {
                var dataArr = recordLink.Split("|");
                var dataKey = dataArr[0];
                var dataKeyValueTemp = dataArr[1];
                int dataKeyValue;
                if (string.IsNullOrEmpty(dataKey) || string.IsNullOrEmpty(dataKeyValueTemp))
                    continue;

                if (int.TryParse(dataKeyValueTemp, out dataKeyValue))
                {                    
                    var recKey = await _sharePointViewModelService.GetRecKey(docLibrary, docLibraryFolder, dataKeyValue);
                    var folders = SharePointViewModelService.GetDocumentFolders(docLibraryFolder, recKey);

                    var fileName = msgSubject + ".eml";

                    if (settings.SharePointInvalidCharacters.Any(s => fileName.Contains(s)))
                    {
                        foreach (var invalid in settings.SharePointInvalidCharacters)
                        {
                            fileName = fileName.Replace(invalid.ToString(), string.Empty);
                        }
                    }

                    var addResult = new SharePointGraphDriveItemKeyViewModel();

                    var decodedBytes = Convert.FromBase64String(encodedMsg);
                    using (var stream = new MemoryStream())
                    {                        
                        stream.Write(decodedBytes, 0, decodedBytes.Length);
                        stream.Position = 0;

                        var existing = await graphClient.FileExists(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, docLibrary, fileName);
                        if (settings.IsSharePointIntegrationByMetadataOn)
                        {                            
                            if (!existing)
                                addResult = await graphClient.UploadSiteFile(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, docLibrary, new List<string>(), stream, fileName);
                        }
                        else
                        {
                            if (!existing)
                                addResult = await graphClient.UploadSiteFile(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, docLibrary, folders, stream, fileName);
                        }                            
                    }

                    if (addResult != null)
                    {
                        var site = graphClient.GetSiteWithLists(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName).Result;
                        var list = site.Lists.Where(l => l.Name == docLibrary).FirstOrDefault();
                        var driveItem = await graphClient.Drives[addResult.DriveId].Items[addResult.DriveItemId].Request().Expand("listItem").GetAsync();

                        if (list != null)
                        {
                            var sync = new SharePointSyncToDocViewModel
                            {
                                DocLibrary = docLibrary,
                                DocLibraryFolder = docLibraryFolder,
                                DriveItemId = driveItem.Id,
                                ParentId = dataKeyValue,
                                FileName = fileName,
                                CreatedBy = userName.Left(20),
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
                            await _sharePointViewModelService.SyncToDocumentTables(sync);

                            var docId = await _documentService.DocDocuments.Where(d => d.DocFile.DriveItemId == driveItem.Id).Select(d => d.DocId).FirstOrDefaultAsync();
                            if (docId > 0)
                            {
                                processedCases.DataKeyValue.Add(new OutlookProcessedCases { DataKey = dataKey, DataKeyValue = dataKeyValue, DocId = docId });
                            }
                        }

                        if (list != null)
                        {                            
                            if (settings.IsSharePointIntegrationByMetadataOn)
                            {
                                var requestBody = new FieldValueSet
                                {
                                    AdditionalData = new Dictionary<string, object>
                                    {
                                        {
                                            "CPIScreen" , docLibraryFolder
                                        },
                                        {
                                            "CPIRecordKey", recKey.Replace(SharePointSeparator.Folder, SharePointSeparator.Field)
                                        }
                                    }
                                };
                                var updateResult = await graphClient.Sites[site.Id].Lists[list.Id].Items[driveItem.ListItem.Id].Fields.Request().UpdateAsync(requestBody);
                            }                            
                        }
                    }
                }  
            }
            
            var logs = processedCases.DataKeyValue.Select(d => new DocGmailCaseLink()
            {
                EmailId = gmailMsgId,
                SystemType = systemType,
                DataKey = d.DataKey,
                DataKeyValue = d.DataKeyValue,
                DocId = d.DocId,
                CreatedBy = userName,
                DateCreated = DateTime.Now
            }).ToList();
            await _documentService.LogGmailEmail(logs);

            return processedCases;
        }

    }
}
