using Kendo.Mvc.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using OpenIddict.Validation.AspNetCore;
using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using R10.Core.Entities.Trademark;
using R10.Core.Interfaces;
using R10.Web.Helpers;
using R10.Web.Models;
using R10.Web.Security;
using R10.Web.Services;
using R10.Web.Services.DocumentSearch;
using R10.Web.Services.DocumentStorage;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Controllers
{
    [Authorize(AuthenticationSchemes = AuthSchemes)]
    public class AzureUtilityController : Controller
    {
        private readonly AzureStorage _azureStorage;
        private readonly IApplicationDbContext _repository;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly IConfiguration _configuration;
        private readonly IDocumentHelper _documentHelper;

        private const string AuthSchemes = "Identity.Application" + "," + OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
        private bool _metadataOnly = false;

        public AzureUtilityController(AzureStorage azureStorage,
                                      IApplicationDbContext repository,
                                      IStringLocalizer<SharedResource> localizer,
                                      IConfiguration configuration,
                                      IDocumentHelper documentHelper)
        {
            _azureStorage = azureStorage;
            _repository = repository;
            _localizer = localizer;
            _configuration = configuration;
            _documentHelper = documentHelper;
        }

        [Authorize(Policy = CPiAuthorizationPolicy.Administrator)]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Policy = CPiAuthorizationPolicy.Administrator)]
        public async Task<IActionResult> TransferThumbnails(bool metadataonly = false)
        {
            _metadataOnly = metadataonly;

            await DoThumbnails();
            return Content(_localizer["Thumbnails have been imported."]);
        }

        [HttpPost]
        [Authorize(Policy = CPiAuthorizationPolicy.Administrator)]
        public async Task<IActionResult> TransferLogs()
        {
            _metadataOnly = true;
            await DoLetterLogs();
            await DoQELogs();
            await DoEFSLogs();
            //return Content(_localizer["Logs have been imported."]);
            return Content(_localizer["Logs metadata have been updated."]);
        }

        [HttpPost]
        [Authorize(Policy = CPiAuthorizationPolicy.Administrator)]
        public async Task<IActionResult> TransferImages()
        {
            _metadataOnly = true;

            await DoDocMgt();
            await DoIDSImages();
            //return Content(_localizer["Images have been imported."]);
            return Content(_localizer["Images metadata have been updated."]);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult> AzureDocumentsSync()
        {
            var settings = _configuration.GetSection("DocumentStorage").Get<DocumentStorageSettings>();
            if (!settings.UseAzureStorage)
                return BadRequest();

            // RTS and TL modules have been removed; transfer methods are no longer available.
            return Ok();
        }

        [Authorize]
        public async Task<ActionResult> GetDocumentStorageInfo()
        {
            var settings = _configuration.GetSection("DocumentStorage").Get<DocumentStorageSettings>();
            return Json(settings);
        }

        #region EFS
        private async Task DoEFSLogs()
        {
            var logs = await _repository.EFSLogs.Where(l => !string.IsNullOrEmpty(l.EfsFile) && l.MetaUpdate == null).ToListAsync();

            // fsn (19-nov-2021) - don't upload anymore, just update the metadata
            //await UploadEFSLogs(logs, DocumentLogType.EFSLog);
            await UpdateMetadataEFSLogs(logs, DocumentLogType.EFSLog);
        }


        #endregion

        #region Quick Email

        private async Task DoQELogs()
        {
            var logs = await _repository.QELogs.Where(l => l.DataKeyValue > 0 && !string.IsNullOrEmpty(l.QEFile) && l.MetaUpdate == null).ToListAsync();
            // fsn (19-nov-2021) - don't upload anymore, just update the metadata
            //await UploadQELogs(logs, DocumentLogType.EmailLog);
            await UpdateMetadataQELogs(logs, DocumentLogType.EmailLog);
        }


        #endregion

        #region Letters
        private async Task DoLetterLogs()
        {
            var logs = await _repository.LetterLogs.Where(l => !string.IsNullOrEmpty(l.LetFile) && l.MetaUpdate == null).Include(l => l.LetterLogDetails).ToListAsync();

            // fsn (19-nov-2021) - don't upload anymore, just update the metadata
            //await UploadLetterLogs(logs, DocumentLogType.LetterLog);
            await UpdateMetadataLetterLogs(logs, DocumentLogType.LetterLog);
        }


        #endregion

        #region Document Management
        private async Task DoDocMgt()
        {
            var docs = await _repository.DocFiles.Where(f => !string.IsNullOrEmpty(f.DocDocument.DocFolder.SystemType))
                            .Select(f => new DocTransferDTO
                            {
                                FileId = f.FileId,
                                SystemType = f.DocDocument.DocFolder.SystemType,
                                ScreenCode = f.DocDocument.DocFolder.ScreenCode,
                                ParentId = f.DocDocument.DocFolder.DataKeyValue,
                                DocFileName = f.DocFileName,
                                ThumbFileName = f.ThumbFileName
                            })
                            .ToListAsync();

            // fsn (19-nov-2021) - don't upload anymore, just update the metadata
            //await UploadDocMgt(docs, DocumentLogType.DocMgt);
            //var thumbnails = docs.Where(doc => doc.ThumbFileName.Length > 0).ToList();
            //await UploadDocMgtThumbnails(thumbnails);

            await UpdateMetadataDocMgt(docs, DocumentLogType.DocMgt);
        }


        //private async Task UploadDocMgt(List<DocTransferDTO> docs, string docType)
        //{
        //    var storageFiles = new List<DocumentStorageFile>();
        //    var directory = Path.Combine(Directory.GetCurrentDirectory(), @"UserFiles\Documents\");

        //    foreach (var doc in docs)
        //    {
        //        var sourceFile = directory + doc.DocFileName;
        //        if (System.IO.File.Exists(sourceFile))
        //        {
        //            var azureFile = _azureStorage.BuildPath(_azureStorage.DocumentRootFolder, string.Empty, doc.DocFileName);

        //            byte[] buffer = null;
        //            if (!_metadataOnly)
        //            {
        //                using (var memoryStream = new MemoryStream())
        //                {
        //                    using (var source = System.IO.File.Open(sourceFile, FileMode.Open, FileAccess.Read))
        //                    {
        //                        source.CopyTo(memoryStream);
        //                    }
        //                    buffer = memoryStream.ToArray();
        //                }
        //            }

        //            var header = new DocumentStorageHeader
        //            {
        //                SystemType = doc.SystemType.ToUpper(),                              // global search consistency
        //                ScreenCode = doc.ScreenCode,                                        // global search consistency
        //                DocumentType = docType,
        //                ParentId = doc.ParentId.ToString(),
        //                FileName = doc.DocFileName
        //            };
        //            var storageFile = new DocumentStorageFile
        //            {
        //                Buffer = buffer,
        //                FileName = azureFile,
        //                Header = header
        //            };
        //            storageFiles.Add(storageFile);

        //            //process per 50
        //            if (storageFiles.Count == 50)
        //            {
        //                await _azureStorage.SaveFiles(storageFiles);
        //                storageFiles.Clear();
        //            }
        //        }
        //    }
        //    if (storageFiles.Count > 0)
        //    {
        //        await _azureStorage.SaveFiles(storageFiles);
        //    }
        //}

        //private async Task UploadDocMgtThumbnails(List<DocTransferDTO> thumbnails)
        //{
        //    var storageFiles = new List<DocumentStorageFile>();
        //    var directory = Path.Combine(Directory.GetCurrentDirectory(), @"UserFiles\Documents\Thumbnails");

        //    foreach (var doc in thumbnails)
        //    {
        //        var sourceFile = directory + doc.DocFileName;
        //        if (System.IO.File.Exists(sourceFile))
        //        {
        //            var azureFile = _azureStorage.BuildPath(_azureStorage.DocumentThumbnailFolder, string.Empty, doc.ThumbFileName);

        //            byte[] buffer = null;
        //            if (!_metadataOnly)
        //            {
        //                using (var memoryStream = new MemoryStream())
        //                {
        //                    using (var source = System.IO.File.Open(sourceFile, FileMode.Open, FileAccess.Read))
        //                    {
        //                        source.CopyTo(memoryStream);
        //                    }
        //                    buffer = memoryStream.ToArray();
        //                }
        //            }

        //            var storageFile = new DocumentStorageFile
        //            {
        //                Buffer = buffer,
        //                FileName = azureFile,
        //            };
        //            storageFiles.Add(storageFile);

        //            //process per 50
        //            if (storageFiles.Count == 50)
        //            {
        //                await _azureStorage.SaveFiles(storageFiles);
        //                storageFiles.Clear();
        //            }

        //        }
        //    }
        //    if (storageFiles.Count > 0)
        //    {
        //        await _azureStorage.SaveFiles(storageFiles);
        //    }
        //}
        #endregion

        #region Images


        private async Task DoIDSImages()
        {
            // fsn (4-feb-2022) separate the updates so we can update/stamp the proper table 
            var relatedCases = await _repository.PatIDSRelatedCases.Where(i => !string.IsNullOrEmpty(i.DocFilePath) && i.MetaUpdate == null)
                                                    .Select(i => new ImageViewModel { ImageId = i.RelatedCasesId, ParentId = i.AppId, ImageFile = i.DocFilePath, ImageTitle = "Patent", ImageSource = "IDS" }).ToListAsync();
            await UpdateMetadataImages(relatedCases, DocumentLogType.IDSDoc, "RC");

            var nonPat = await _repository.PatIDSNonPatLiteratures.Where(i => !string.IsNullOrEmpty(i.DocFilePath) && i.MetaUpdate == null)
                                                    .Select(i => new ImageViewModel { ImageId = i.NonPatLiteratureId, ParentId = (int)i.AppId, ImageFile = i.DocFilePath, ImageTitle = "Patent", ImageSource = "IDS" }).ToListAsync();
            //relatedCases.AddRange(nonPat);
            await UpdateMetadataImages(nonPat, DocumentLogType.IDSDoc, "NPL");

            // fsn (19-nov-2021) - don't upload anymore, just update the metadata
            //await UploadImages(relatedCases, DocumentLogType.IDSDoc);
            //await UpdateMetadataImages(relatedCases, DocumentLogType.IDSDoc);
        }

        //private async Task DoTrademarkImages()
        //{

        //    var images = await _repository.TmkImages.Where(i => i.FileID != null && i.MetaUpdate == null)
        //                                    .Select(i => new ImageEntity { ImageId = i.ImageId, ParentId = i.ParentId, ImageFile = i.ImageFile, ImageTitle = "Trademark", ImageSource = "Tmk" }).ToListAsync();

        //    var actions = await _repository.TmkImageActs.Where(i => i.FileID != null && i.MetaUpdate == null)
        //                                    .Select(i => new ImageEntity { ImageId = i.ImageId, ParentId = i.ParentId, ImageFile = i.ImageFile, ImageTitle = "Trademark", ImageSource = "Act" }).ToListAsync();
        //    images.AddRange(actions);

        //    var costs = await _repository.TmkImageCosts.Where(i => i.FileID != null && i.MetaUpdate == null)
        //                                    .Select(i => new ImageEntity { ImageId = i.ImageId, ParentId = i.ParentId, ImageFile = i.ImageFile, ImageTitle = "Trademark", ImageSource = "Cost" }).ToListAsync();
        //    images.AddRange(costs);

        //    // fsn (19-nov-2021) - don't upload anymore, just update the metadata
        //    //await UploadImages(images, DocumentLogType.ImageDoc);
        //    await UpdateMetadataImages(images, DocumentLogType.ImageDoc);
        //}

        //private async Task DoGMImages()
        //{
        //    var images = await _repository.GMMatterImages.Where(i => i.FileID != null && i.MetaUpdate == null)
        //                                    .Select(i => new ImageEntity { ImageId = i.ImageId, ParentId = i.ParentId, ImageFile = i.ImageFile, ImageTitle = "GeneralMatter", ImageSource = "GM" }).ToListAsync();

        //    var actions = await _repository.GMMatterActImages.Where(i => i.FileID != null && i.MetaUpdate == null)
        //                                    .Select(i => new ImageEntity { ImageId = i.ImageId, ParentId = i.ParentId, ImageFile = i.ImageFile, ImageTitle = "GeneralMatter", ImageSource = "Act" }).ToListAsync();
        //    images.AddRange(actions);

        //    var costs = await _repository.GMMatterImageCosts.Where(i => i.FileID != null && i.MetaUpdate == null)
        //                                    .Select(i => new ImageEntity { ImageId = i.ImageId, ParentId = i.ParentId, ImageFile = i.ImageFile, ImageTitle = "GeneralMatter", ImageSource = "Cost" }).ToListAsync();
        //    images.AddRange(costs);

        //    // fsn (19-nov-2021) - don't upload anymore, just update the metadata
        //    //await UploadImages(images, DocumentLogType.ImageDoc);
        //    await UpdateMetadataImages(images, DocumentLogType.ImageDoc);
        //}

        //private async Task DoDMSImages()
        //{
        //    var images = await _repository.DMSImages.Where(i => i.FileID != null && i.MetaUpdate == null)
        //                                    .Select(i => new ImageEntity { ImageId = i.ImageId, ParentId = i.ParentId, ImageFile = i.ImageFile, ImageTitle = "DMS", ImageSource = "DMS" }).ToListAsync();

        //    // fsn (19-nov-2021) - don't upload anymore, just update the metadata
        //    //await UploadImages(images, DocumentLogType.ImageDoc);
        //    await UpdateMetadataImages(images, DocumentLogType.ImageDoc);
        //}

        #region DON'T delete
        //private async Task UploadImages(List<ImageEntity> images, string docType)
        //{
        //    var storageFiles = new List<DocumentStorageFile>();
        //    var directory = Path.Combine(Directory.GetCurrentDirectory(), @"UserFiles\Images\");

        //    foreach (var file in images)
        //    {
        //        var imageFile = (docType == DocumentLogType.IDSDoc ? "References/" : "") + file.ImageFile;
        //        var sourceFile = directory + $@"{file.ImageTitle}\{imageFile.Replace("/",@"\")}";
        //        if (System.IO.File.Exists(sourceFile))
        //        {
        //            var azureFile = _azureStorage.BuildPath(_azureStorage.ImageRootFolder, file.ImageTitle, imageFile);

        //            byte[] buffer = null;
        //            if (!_metadataOnly) {
        //                using (var memoryStream = new MemoryStream())
        //                {
        //                    using (var source = System.IO.File.Open(sourceFile, FileMode.Open, FileAccess.Read))
        //                    {
        //                        source.CopyTo(memoryStream);
        //                    }
        //                    buffer = memoryStream.ToArray();
        //                }
        //            }

        //            var header = new DocumentStorageHeader
        //            {
        //                SystemType = file.ImageTitle.Substring(0, 1).ToUpper(),         // global search consistency
        //                ScreenCode = file.ImageSource,
        //                ParentId = file.ParentId.ToString(),
        //                DocumentType = docType,
        //                FileName = file.ImageFile
        //            };
        //            var storageFile = new DocumentStorageFile
        //            {
        //                Buffer = buffer,
        //                FileName = azureFile,
        //                Header = header
        //            };
        //            storageFiles.Add(storageFile);

        //            //process per 50
        //            if (storageFiles.Count == 50) {
        //                await _azureStorage.SaveFiles(storageFiles);
        //                storageFiles.Clear();
        //            } 
        //        }
        //    }

        //    //remaining
        //    if (storageFiles.Count > 0) {
        //        await _azureStorage.SaveFiles(storageFiles);
        //    }

        //}
        #endregion

        private async Task DoThumbnails()
        {
            var directory = Path.Combine(Directory.GetCurrentDirectory(), @"UserFiles\Images\Thumbnails");
            var files = Directory.GetFiles(directory);

            if (files.Length > 0)
            {
                var noPerGroup = 20; //group by 20 files 
                var groups = Math.Ceiling((double)files.Length / noPerGroup);
                var counter = 1;
                for (var i = 1; i <= groups; i++)
                {
                    var filesToProcess = new List<string>();
                    for (var y = 1; y <= noPerGroup; y++)
                    {
                        if (counter <= files.Length)
                        {
                            var file = files[counter - 1];
                            filesToProcess.Add(file);
                        }
                        else
                        {
                            break;
                        }
                        counter++;
                    }

                    if (filesToProcess.Count > 0)
                    {
                        var storageFiles = new List<DocumentStorageFile>();
                        foreach (var file in filesToProcess)
                        {
                            if (System.IO.File.Exists(file))
                            {
                                var paths = file.Split(@"\");
                                var fileName = paths[paths.Length - 1];
                                var thumbnailFile = _azureStorage.BuildPath(_azureStorage.ImageThumbnailRootFolder, "", fileName);

                                byte[] buffer = null;
                                if (!_metadataOnly)
                                {
                                    using (var memoryStream = new MemoryStream())
                                    {
                                        using (var source = System.IO.File.Open(file, FileMode.Open, FileAccess.Read))
                                        {
                                            source.CopyTo(memoryStream);
                                        }
                                        buffer = memoryStream.ToArray();
                                    }
                                }


                                var storageFile = new DocumentStorageFile
                                {
                                    Buffer = buffer,
                                    FileName = thumbnailFile,
                                };
                                storageFiles.Add(storageFile);

                                //process per 20
                                if (storageFiles.Count == 20)
                                {
                                    await _azureStorage.SaveFiles(storageFiles);
                                    storageFiles.Clear();
                                }
                            }
                        }
                        if (storageFiles.Count > 0)
                        {
                            await _azureStorage.SaveFiles(storageFiles);
                        }

                    }
                }
            }

        }

        #endregion

        // TransferIFW, TransferTLDocs, TransferTLImages methods removed - RTS and TL modules deleted

        #region Update Metadata
        private async Task UpdateMetadataImages(List<ImageViewModel> images, string docType, string moreInfo = "")
        {
            // update/save metadata info of file in Azure blob storage file
            var storageFiles = new List<DocumentStorageFile>();
            var dbUpdateList = new List<DocumentDBUpdateInfo>();

            foreach (var file in images)
            {
                var imageFile = (docType == DocumentLogType.IDSDoc ? "References/" : "") + file.ImageFile;
                var azureFile = _azureStorage.BuildPath(_azureStorage.ImageRootFolder, file.ImageTitle, imageFile);

                var header = new DocumentStorageHeader
                {
                    SystemType = file.ImageTitle.Substring(0, 1).ToUpper(),         // global search consistency
                    ScreenCode = file.ImageSource,
                    ParentId = file.ParentId.ToString(),
                    DocumentType = docType,
                    FileName = file.ImageFile
                };
                var storageFile = new DocumentStorageFile
                {
                    Buffer = null,
                    FileName = azureFile,
                    Header = header
                };
                storageFiles.Add(storageFile);

                dbUpdateList.Add(new DocumentDBUpdateInfo
                {
                    SystemType = header.SystemType,
                    ScreenCode = header.ScreenCode,
                    RecordId = file.ImageId
                });

                //process per 20
                if (storageFiles.Count == 20)
                {
                    await _azureStorage.UpdateFileMetadata(storageFiles);
                    storageFiles.Clear();

                    // update metadata update date in the images file
                    await UpdateImageDBMetadata(dbUpdateList, moreInfo);
                    dbUpdateList.Clear();
                }
            }

            //remaining
            if (storageFiles.Count > 0)
            {
                await _azureStorage.UpdateFileMetadata(storageFiles);
                // update metadata update date in the images file
                await UpdateImageDBMetadata(dbUpdateList, moreInfo);
                dbUpdateList.Clear();
            }
        }

        private async Task UpdateMetadataDocMgt(List<DocTransferDTO> docs, string docType)
        {
            var storageFiles = new List<DocumentStorageFile>();
            var dbUpdateList = new List<DocumentDBUpdateInfo>();

            foreach (var doc in docs)
            {
                var azureFile = _azureStorage.BuildPath(_azureStorage.DocumentRootFolder, string.Empty, doc.DocFileName);

                var header = new DocumentStorageHeader
                {
                    SystemType = doc.SystemType.ToUpper(),                              // global search consistency
                    ScreenCode = doc.ScreenCode,                                        // global search consistency
                    DocumentType = docType,
                    ParentId = doc.ParentId.ToString(),
                    LogId = doc.FileId.ToString(),
                    FileName = doc.DocFileName
                };
                var storageFile = new DocumentStorageFile
                {
                    Buffer = null,
                    FileName = azureFile,
                    Header = header
                };
                storageFiles.Add(storageFile);
                
                dbUpdateList.Add(new DocumentDBUpdateInfo
                {
                    SystemType = header.SystemType,
                    ScreenCode = header.ScreenCode,
                    RecordId = Int32.Parse(header.LogId)
                });

                dbUpdateList.Add(new DocumentDBUpdateInfo
                {
                    SystemType = header.SystemType,
                    ScreenCode = header.ScreenCode,
                    RecordId = Int32.Parse(header.LogId)
                });

                //process per 20
                if (storageFiles.Count == 20)
                {
                    await _azureStorage.UpdateFileMetadata(storageFiles);
                    storageFiles.Clear();

                    // update metadata update date in the images file
                    await UpdateImageDBMetadata(dbUpdateList, "");
                    dbUpdateList.Clear();
                }

            }
            if (storageFiles.Count > 0)
            {
                await _azureStorage.UpdateFileMetadata(storageFiles);
                // update metadata update date in the images file
                await UpdateImageDBMetadata(dbUpdateList, "");
            }
        }

        private async Task UpdateMetadataLetterLogs(List<LetterLog> logs, string docType)
        {
            var storageFiles = new List<DocumentStorageFile>();
            var dbUpdateList = new List<DocumentDBUpdateInfo>();

            foreach (var log in logs)
            {
                var systemName = ImageHelper.GetSystemName(log.SystemType);
                var azureFile = _azureStorage.BuildPath(_azureStorage.LetterLogFolder, systemName, log.LetFile);

                if (log.LetterLogDetails != null)
                {
                    var child = log.LetterLogDetails.FirstOrDefault();
                    if (child != null)
                    {
                        var header = new DocumentStorageHeader
                        {
                            SystemType = log.SystemType.ToUpper(),                              // global search consistency
                            ScreenCode = _documentHelper.DataKeyToScreenCode(log.DataKey),      // global search consistency
                            DocumentType = docType,
                            LogId = log.LetLogId.ToString(),
                            FileName = log.LetFile
                        };
                        var storageFile = new DocumentStorageFile
                        {
                            Buffer = null,
                            FileName = azureFile,
                            Header = header
                        };
                        storageFiles.Add(storageFile);
                        dbUpdateList.Add(new DocumentDBUpdateInfo
                        {
                            SystemType = header.SystemType,
                            ScreenCode = header.ScreenCode,
                            RecordId = Int32.Parse(header.LogId)
                        });
                    }
                }

                //process per 20
                if (storageFiles.Count == 20)
                {
                    await _azureStorage.UpdateFileMetadata(storageFiles);
                    storageFiles.Clear();

                    // update metadata update date in the images file
                    await UpdateImageDBMetadata(dbUpdateList, "LTR");
                    dbUpdateList.Clear();
                }
            }
            if (storageFiles.Count > 0)
            {
                await _azureStorage.UpdateFileMetadata(storageFiles);
                // update metadata update date in the images file
                await UpdateImageDBMetadata(dbUpdateList, "LTR");
                dbUpdateList.Clear();
            }
        }

        private async Task UpdateMetadataQELogs(List<QELog> logs, string docType)
        {
            var storageFiles = new List<DocumentStorageFile>();
            var dbUpdateList = new List<DocumentDBUpdateInfo>();

            foreach (var log in logs)
            {
                var systemName = ImageHelper.GetSystemName(log.SystemType);
                var azureFile = _azureStorage.BuildPath(_azureStorage.EmailLogFolder, systemName, log.QEFile);

                var header = new DocumentStorageHeader
                {
                    SystemType = log.SystemType.ToUpper(),                              // global search consistency
                    ScreenCode = _documentHelper.DataKeyToScreenCode(log.DataKey),      // global search consistency
                    ParentId = log.DataKeyValue.ToString(),
                    DocumentType = docType,
                    FileName = log.QEFile,
                };
                var storageFile = new DocumentStorageFile
                {
                    Buffer = null,
                    FileName = azureFile,
                    Header = header
                };
                storageFiles.Add(storageFile);
                dbUpdateList.Add(new DocumentDBUpdateInfo
                {
                    SystemType = header.SystemType,
                    ScreenCode = header.ScreenCode,
                    RecordId = log.LogID
                });

                if (!string.IsNullOrEmpty(log.Attachments))
                {
                    var images = JsonConvert.DeserializeObject<List<AttachedFileDTO>>(log.Attachments);

                    foreach (var image in images)
                    {
                        var azureImageFile = _azureStorage.BuildPath(_azureStorage.EmailLogFolder, systemName, image.FileName);

                        var imageHeader = new DocumentStorageHeader
                        {
                            SystemType = log.SystemType.ToUpper(),                                  // global search consistency
                            ScreenCode = _documentHelper.DataKeyToScreenCode(log.DataKey),          // global search consistency
                            ParentId = log.DataKeyValue.ToString(),
                            DocumentType = DocumentLogType.EmailLogAttachment,
                            FileName = image.FileName,
                        };
                        var imageStorageFile = new DocumentStorageFile
                        {
                            Buffer = null,
                            FileName = azureImageFile,
                            Header = imageHeader
                        };
                        storageFiles.Add(imageStorageFile);
                        dbUpdateList.Add(new DocumentDBUpdateInfo
                        {
                            SystemType = header.SystemType,
                            ScreenCode = header.ScreenCode,
                            RecordId = log.LogID
                        });
                    }
                }

                //process per 20
                if (storageFiles.Count == 20)
                {
                    await _azureStorage.UpdateFileMetadata(storageFiles);
                    storageFiles.Clear();
                    // update metadata update date in the images file
                    await UpdateImageDBMetadata(dbUpdateList, "QE");
                    dbUpdateList.Clear();
                }
            }
            if (storageFiles.Count > 0)
            {
                await _azureStorage.UpdateFileMetadata(storageFiles);
                // update metadata update date in the images file
                await UpdateImageDBMetadata(dbUpdateList, "QE");
                dbUpdateList.Clear();
            }
        }

        private async Task UpdateMetadataEFSLogs(List<EFSLog> logs, string docType)
        {
            var storageFiles = new List<DocumentStorageFile>();
            var dbUpdateList = new List<DocumentDBUpdateInfo>();

            foreach (var log in logs)
            {
                //var systemName = GetSystemName(log.SystemType);                         
                var systemName = ImageHelper.GetSystemName(log.SystemType);
                var azureFile = _azureStorage.BuildPath(_azureStorage.EFSLogFolder, systemName, log.EfsFile);

                var header = new DocumentStorageHeader
                {
                    SystemType = log.SystemType.ToUpper(),                                  // global search consistency
                    ScreenCode = _documentHelper.DataKeyToScreenCode(log.DataKey),          // global search consistency
                    ParentId = log.DataKeyValue.ToString(),
                    DocumentType = docType,
                    FileName = log.EfsFile,
                };
                var storageFile = new DocumentStorageFile
                {
                    Buffer = null,
                    FileName = azureFile,
                    Header = header
                };
                storageFiles.Add(storageFile);
                dbUpdateList.Add(new DocumentDBUpdateInfo
                {
                    SystemType = header.SystemType,
                    ScreenCode = header.ScreenCode,
                    RecordId = log.EfsLogId
                });

                //process per 20
                if (storageFiles.Count == 20)
                {
                    await _azureStorage.UpdateFileMetadata(storageFiles);
                    storageFiles.Clear();
                    // update metadata update date in the images file
                    await UpdateImageDBMetadata(dbUpdateList, "EFS");
                    dbUpdateList.Clear();
                }
            }
            if (storageFiles.Count > 0)
            {
                await _azureStorage.UpdateFileMetadata(storageFiles);
                // update metadata update date in the images file
                await UpdateImageDBMetadata(dbUpdateList, "EFS");
                dbUpdateList.Clear();
            }
        }
        #endregion

        #region Update DB Metadata Date
        private async Task UpdateImageDBMetadata(List<DocumentDBUpdateInfo> imageList, string moreInfo)
        {
            // process by systemtype, screen for efficiency
            imageList.Add(new DocumentDBUpdateInfo { SystemType = "x", ScreenCode = "x", RecordId = -1 });      // sentinel, add this so the loop processes all real records
            string svSystemType = "";
            string svScreenCode = "";
            string idConcat = "";
            string columnName = GetColumnIdName(moreInfo);

            foreach (var row in imageList)
            {
                if (!(svSystemType == row.SystemType && svScreenCode == row.ScreenCode))
                {
                    if (idConcat.Length > 0)
                    {
                        // formulate update 
                        var table = GetImageTable(svSystemType, svScreenCode, moreInfo);
                        if (table.Length > 0)
                        {

                            var sql = "Update " + table + " Set MetaUpdate = getdate() Where " + columnName + " In (" + idConcat.Substring(1) + ")";
                            await _repository.Database.ExecuteSqlRawAsync(sql);
                        }
                    }

                    svSystemType = row.SystemType;
                    svScreenCode = row.ScreenCode;
                    idConcat = "";
                }
                idConcat += "," + row.RecordId;
            }
        }

        private string GetColumnIdName(string moreInfo)
        {
            switch (moreInfo)
            {
                case "RC": return "RelatedCasesId";
                case "NPL": return "NonPatLiteratureId";
                case "LTR": return "LetLogId";
                case "EFS": return "EfsLogId";
                case "QE": return "LogId";
                default: return "FileId";
            }
        }
        private string GetImageTable(string systemType, string screenCode, string moreInfo)
        {
            switch (moreInfo)
            {
                case "LTR": return "tblLetLog";
                case "EFS": return "tblEFS_Log";
                case "QE": return "tblQELog";
                default: break;
            }

            switch (systemType)
            {
                case "P":
                    switch (screenCode)
                    {
                        case "IDS":
                            if (moreInfo == "RC")
                                return "tblPatIDSRelatedCases";
                            else
                                return "tblPatAppNonPatLiterature";
                        default: return "tblDocFile";
                    }
                //case "T":
                //    return "tblDocFile";
                //case "G":
                //    return "tblDocFile";
                //case "D":
                //    return "tblDocFile";
                default:
                    return "tblDocFile";
            }
        }
        #endregion

        #region Commented out, no longer called
        //private async Task UploadLetterLogs(List<LetterLog> logs, string docType)
        //{
        //    var storageFiles = new List<DocumentStorageFile>();
        //    var directory = Path.Combine(Directory.GetCurrentDirectory(), @"UserFiles\Logs\Letters\");

        //    foreach (var log in logs)
        //    {
        //        //var systemName = GetSystemName(log.SystemType);
        //        var systemName = ImageHelper.GetSystemName(log.SystemType);                         
        //        var sourceFile = directory + $@"{systemName}\{log.LetFile}";
        //        if (System.IO.File.Exists(sourceFile))
        //        {
        //            var azureFile = _azureStorage.BuildPath(_azureStorage.LetterLogFolder, systemName, log.LetFile);

        //            byte[] buffer = null;
        //            if (!_metadataOnly)
        //            {
        //                using (var memoryStream = new MemoryStream())
        //                {
        //                    using (var source = System.IO.File.Open(sourceFile, FileMode.Open, FileAccess.Read))
        //                    {
        //                        source.CopyTo(memoryStream);
        //                    }
        //                    buffer = memoryStream.ToArray();
        //                }
        //            }

        //            if (log.LetterLogDetails != null)
        //            {
        //                var child = log.LetterLogDetails.FirstOrDefault();

        //                if (child != null)
        //                {
        //                    var header = new DocumentStorageHeader
        //                    {
        //                        SystemType = log.SystemType.ToUpper(),                              // global search consistency
        //                        ScreenCode = _documentHelper.DataKeyToScreenCode(log.DataKey),      // global search consistency
        //                        DocumentType = docType,
        //                        LogId = log.LetLogId.ToString(),
        //                        FileName = log.LetFile
        //                    };
        //                    var storageFile = new DocumentStorageFile
        //                    {
        //                        Buffer = buffer,
        //                        FileName = azureFile,
        //                        Header = header
        //                    };
        //                    storageFiles.Add(storageFile);
        //                }
        //            }

        //            //process per 50
        //            if (storageFiles.Count == 50)
        //            {
        //                await _azureStorage.SaveFiles(storageFiles);
        //                storageFiles.Clear();
        //            }
        //        }
        //    }
        //    if (storageFiles.Count > 0) {
        //        await _azureStorage.SaveFiles(storageFiles);
        //    }

        //}

        //private async Task UploadQELogs(List<QELog> logs, string docType)
        //{
        //    var storageFiles = new List<DocumentStorageFile>();
        //    var directory = Path.Combine(Directory.GetCurrentDirectory(), @"UserFiles\Logs\QuickEmails\");

        //    foreach (var log in logs)
        //    {
        //        //var systemName = GetSystemName(log.SystemType);
        //        var systemName = ImageHelper.GetSystemName(log.SystemType);
        //        var sourceFile = directory + $@"{systemName}\{log.QEFile}";
        //        if (System.IO.File.Exists(sourceFile))
        //        {
        //            var azureFile = _azureStorage.BuildPath(_azureStorage.EmailLogFolder, systemName, log.QEFile);
        //            byte[] buffer = null;
        //            if (!_metadataOnly)
        //            {
        //                using (var memoryStream = new MemoryStream())
        //                {
        //                    using (var source = System.IO.File.Open(sourceFile, FileMode.Open, FileAccess.Read))
        //                    {
        //                        source.CopyTo(memoryStream);
        //                    }
        //                    buffer = memoryStream.ToArray();
        //                }
        //            }

        //            var header = new DocumentStorageHeader
        //            {
        //                SystemType = log.SystemType.ToUpper(),                              // global search consistency
        //                ScreenCode = _documentHelper.DataKeyToScreenCode(log.DataKey),      // global search consistency
        //                ParentId = log.DataKeyValue.ToString(),
        //                DocumentType = docType,
        //                FileName = log.QEFile,
        //                //DataKey=log.DataKey                                               // replaced with ScreenCode
        //            };
        //            var storageFile = new DocumentStorageFile
        //            {
        //                Buffer = buffer,
        //                FileName = azureFile,
        //                Header = header
        //            };
        //            storageFiles.Add(storageFile);

        //            if (!string.IsNullOrEmpty(log.Attachments))
        //            {
        //                var images = JsonConvert.DeserializeObject<List<AttachedFileDTO>>(log.Attachments);
        //                foreach (var image in images)
        //                {

        //                    var sourceImageFile = directory + $@"{systemName}\{image.FileName}";
        //                    if (System.IO.File.Exists(sourceImageFile))
        //                    {
        //                        var azureImageFile = _azureStorage.BuildPath(_azureStorage.EmailLogFolder, systemName, image.FileName);

        //                        byte[] imageBuffer = null;
        //                        if (!_metadataOnly)
        //                        {
        //                            using (var memoryStream = new MemoryStream())
        //                            {
        //                                using (var imageSource = System.IO.File.Open(sourceImageFile, FileMode.Open, FileAccess.Read))
        //                                {
        //                                    imageSource.CopyTo(memoryStream);
        //                                }
        //                                imageBuffer = memoryStream.ToArray();
        //                            }
        //                        }

        //                        var imageHeader = new DocumentStorageHeader
        //                        {
        //                            SystemType = log.SystemType.ToUpper(),                                  // global search consistency
        //                            ScreenCode = _documentHelper.DataKeyToScreenCode(log.DataKey),          // global search consistency
        //                            ParentId = log.DataKeyValue.ToString(),
        //                            DocumentType = DocumentLogType.EmailLogAttachment,
        //                            FileName = image.FileName,
        //                            //DataKey = log.DataKey                                                 // replaced with ScreenCode
        //                        };
        //                        var imageStorageFile = new DocumentStorageFile
        //                        {
        //                            Buffer = imageBuffer,
        //                            FileName = azureImageFile,
        //                            Header = imageHeader
        //                        };
        //                        storageFiles.Add(imageStorageFile);
        //                    }

        //                }
        //            }

        //            //process per 50
        //            if (storageFiles.Count == 50)
        //            {
        //                await _azureStorage.SaveFiles(storageFiles);
        //                storageFiles.Clear();
        //            }
        //        }
        //    }
        //    if (storageFiles.Count > 0)
        //    {
        //        await _azureStorage.SaveFiles(storageFiles);
        //    }
        //}

        //private async Task UploadEFSLogs(List<EFSLog> logs, string docType)
        //{
        //    var storageFiles = new List<DocumentStorageFile>();
        //    var directory = Path.Combine(Directory.GetCurrentDirectory(), @"UserFiles\Logs\EFS\");

        //    foreach (var log in logs)
        //    {
        //        //var systemName = GetSystemName(log.SystemType);                         
        //        var systemName = ImageHelper.GetSystemName(log.SystemType);
        //        var sourceFile = directory + $@"{systemName}\{log.EfsFile}";
        //        if (System.IO.File.Exists(sourceFile))
        //        {
        //            var azureFile = _azureStorage.BuildPath(_azureStorage.EFSLogFolder, systemName, log.EfsFile);

        //            byte[] buffer = null;
        //            if (!_metadataOnly)
        //            {
        //                using (var memoryStream = new MemoryStream())
        //                {
        //                    using (var source = System.IO.File.Open(sourceFile, FileMode.Open, FileAccess.Read))
        //                    {
        //                        source.CopyTo(memoryStream);
        //                    }
        //                    buffer = memoryStream.ToArray();
        //                }
        //            }


        //            var header = new DocumentStorageHeader
        //            {
        //                SystemType = log.SystemType.ToUpper(),                                  // global search consistency
        //                ScreenCode = _documentHelper.DataKeyToScreenCode(log.DataKey),          // global search consistency
        //                ParentId = log.DataKeyValue.ToString(),
        //                DocumentType = docType,
        //                FileName = log.EfsFile,
        //                //DataKey = log.DataKey                                                 // replaced with ScreenCode
        //            };
        //            var storageFile = new DocumentStorageFile
        //            {
        //                Buffer = buffer,
        //                FileName = azureFile,
        //                Header = header
        //            };
        //            storageFiles.Add(storageFile);

        //            //process per 50
        //            if (storageFiles.Count == 50)
        //            {
        //                await _azureStorage.SaveFiles(storageFiles);
        //                storageFiles.Clear();
        //            }
        //        }
        //    }
        //    if (storageFiles.Count > 0)
        //    {
        //        await _azureStorage.SaveFiles(storageFiles);
        //    }
        //}

        #endregion
    }

}
