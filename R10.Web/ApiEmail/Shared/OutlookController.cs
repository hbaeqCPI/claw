using Kendo.Mvc.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OpenIddict.Validation.AspNetCore;
using R10.Core.DTOs;
using R10.Core.Entities.Documents;
using R10.Core.Interfaces;
using R10.Web.Helpers;
using R10.Web.Interfaces;
using R10.Web.Models.EmailAddInModels;
using R10.Web.Security;
using R10.Web.Services.DocumentStorage;
using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Threading.Tasks;

namespace R10.Web.ApiEmail.Shared
{
    [Route("~/emailapi/[controller]")]
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme, Policy = SharedAuthorizationPolicy.CanAccessSystem)]
    [EnableCors("EmailAddInCORSPolicy")]
    [ApiController]
    public class OutlookController : Microsoft.AspNetCore.Mvc.Controller
    {
        private readonly IDocumentService _documentService;
        private readonly IOutlookService _outlookService;
        private readonly IDocumentHelper _documentHelper;

        public OutlookController(IOutlookService outlookService, IDocumentService documentService, IDocumentHelper documentHelper)
        {
            _outlookService = outlookService;
            _documentService = documentService;
            _documentHelper = documentHelper;
        }

        [HttpPost("savetocpi")]
        public async Task<Microsoft.AspNetCore.Mvc.ActionResult> SaveToCPI([FromBody] OutlookSaveParam param)
        {
            try
            {
                if (string.IsNullOrEmpty(param.olItemId))
                {
                    throw new ArgumentNullException("Email Id");
                }

                // get outlook message components from Microsoft/Outlook server using API
                var outlookMessage = await _outlookService.GetEmailMessage(param.olItemId, param.accessToken);
                var inlineAttachments = await _outlookService.GetEmailAttachments(param.olItemId, param.inlineAttachments, param.accessToken);
                var regularAttachments = await _outlookService.GetEmailAttachments(param.olItemId, param.regularAttachments, param.accessToken);
                var droppedAttachmentsList = new List<OutlookAttachment>();
                // transform outlook components to MSG file model for saving to .msg file
                if (param.droppedAttachments != null && param.droppedAttachments.Length > 0)
                {
                    foreach (object item in param.droppedAttachments)

                    {
                        droppedAttachmentsList.Add(JsonConvert.DeserializeObject<OutlookAttachment>(item.ToString()));
                    }
                }

                var msgModel = CopyOutlookToMsgModel(outlookMessage, inlineAttachments, regularAttachments, droppedAttachmentsList);

                // get a document file name (auto-generated/computed)
                var userEmail = param.userEmail;
                var userName = userEmail.Split("@")[0];
                var docFile = new DocFile { 
                    FileExt = "msg", 
                    UserFileName = GetDocumentName(userName, msgModel.Subject),
                    FileSize = 0, 
                    IsImage = false,
                    CreatedBy = userName, 
                    DateCreated = DateTime.Now, 
                    UpdatedBy = userName, 
                    LastUpdate = DateTime.Now 
                };

                // save docFile so a file name can be generated
                var newFile = await _documentService.AddDocFile(docFile);

                var documentHeader = new DocumentStorageHeader
                {
                    SystemType = param.systemType.ToUpper(),                                // global search consistency
                    ScreenCode = param.screenCode,            // global search consistency
                    DocumentType = DocumentLogType.DocMgt
                    //parentid
                };

                // use file name to save to an .msg file
                newFile.FileSize = _documentHelper.SaveOutlookEmailToMsgFile(msgModel, newFile.DocFileName, documentHeader);

                // transform email model to docOutlook for logging; better to keep email structure in the web project
                var docOutlook = CreateOutlookLog(param.olItemId, param.cpiEmailId, msgModel);

                // log
                var result = await _documentService.LogOutlookEmail(userEmail, param.systemType, param.screenCode, newFile, docOutlook, param.selectedCases, param.selectedCasesPaths);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        private string GetDocumentName(string userName, string subject)
        {
            userName = " (" + userName.Substring(0, Math.Min(userName.Length, 50)) + ")";
            var docName = subject.Substring(0, Math.Min(subject.Length, (255 - userName.Length))) + userName + ".msg";
            return docName;
        }

        private DocOutlook CreateOutlookLog(string olItemId, int? cpiEmailId, MsgEmailModel message)
        {
            var docOutlook = new DocOutlook()
            {
                OLItemId = olItemId,                                            // this changes when Outlook messages are moved between folders
                CPiEmailId = (int) (cpiEmailId ?? 0),                                   // this is assigned by CPi
                OLSender = JsonConvert.SerializeObject(message.Sender),
                OLFrom = JsonConvert.SerializeObject(message.From),
                OLTo = JsonConvert.SerializeObject(message.To),
                OLCc = JsonConvert.SerializeObject(message.Cc),
                OLBcc = JsonConvert.SerializeObject(message.Bcc),
                OLReplyTo = JsonConvert.SerializeObject(message.ReplyTo),
                OLSubject = message.Subject,
                OLBodyPreview = message.BodyPreview,
                OLImportance = message.Importance,
                OLSent = message.SentDate,
                OLReceived = message.ReceiptDate,
                OLModified = message.LastModified,
                OLSavedAttachments = JsonConvert.SerializeObject(message.ByteAttachments),
                //FileId = processedCases.FileId,           // assign later
                //CreatedBy = userName,                     // assign later
            };
            return docOutlook;
        }

        private MsgEmailModel CopyOutlookToMsgModel(OutlookEmail olMessage, List<OutlookAttachment> inlineAttachments, List<OutlookAttachment> regularAttachments, List<OutlookAttachment> droppedAttachments)
        {
            var model = new MsgEmailModel();

            model.IsSent = false;           // this is a read email;
            model.Sender = new MailAddress(olMessage.Sender["EmailAddress"].Address, olMessage.Sender["EmailAddress"].Name);
            model.Subject = olMessage.Subject;
            model.IsDraft = olMessage.IsDraft;
            model.IsReadReceiptRequested = olMessage.IsReadReceiptRequested;
            model.SentDate = olMessage.SentDateTime;
            model.ReceiptDate = olMessage.ReceivedDateTime;
            model.LastModified = olMessage.LastModifiedDateTime;

            model.From = new MailAddress(olMessage.From["EmailAddress"].Address, olMessage.Sender["EmailAddress"].Name);

            model.ReplyTo = new List<MailAddress>();
            olMessage.ReplyTo.Each(item => {
                var recipient = item["EmailAddress"];
                model.ReplyTo.Add(new MailAddress(recipient.Address, recipient.Name));
            });

            model.To = new List<MailAddress>();
            olMessage.ToRecipients.Each(item => {
                var recipient = item["EmailAddress"];
                model.To.Add(new MailAddress(recipient.Address, recipient.Name));
            });

            model.Cc = new List<MailAddress>();
            olMessage.CcRecipients.Each(item => {
                var recipient = item["EmailAddress"];
                model.Cc.Add(new MailAddress(recipient.Address, recipient.Name));
            });

            model.Bcc = new List<MailAddress>();
            olMessage.BccRecipients.Each(item => {
                var recipient = item["EmailAddress"];
                model.Bcc.Add(new MailAddress(recipient.Address, recipient.Name));
            });

            model.IsHtml = olMessage.Body.ContentType == "HTML";
            model.Body = olMessage.Body.Content;

            model.Importance = olMessage.Importance;

            inlineAttachments.Each(item => {
                var attachment = new ByteAttachment(item.Name, item.IsInline, item.ContentId, item.ContentBytes);
                model.ByteAttachments.Add(attachment);
            });

            regularAttachments.Each(item => {
                var attachment = new ByteAttachment(item.Name, item.IsInline, item.ContentId, item.ContentBytes);
                model.ByteAttachments.Add(attachment);
            });

            droppedAttachments.Each(item => {
                var attachment = new ByteAttachment(item.Name, item.IsInline, item.ContentId, item.ContentBytes);
                model.ByteAttachments.Add(attachment);
            });
            return model;
        }

        [HttpGet("getlinkedcases/{cpiEmailId}")]
        public async Task<ActionResult<CaseLogDTO[]>> GetLinkedCases(int? cpiEmailId)
        {
            try
            {
                var results = await _documentService.GetOulookCaseLogByEmailId(cpiEmailId);
                return results;
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

    }
}
