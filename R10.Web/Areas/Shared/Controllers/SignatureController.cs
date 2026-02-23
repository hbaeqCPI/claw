using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using R10.Core.Entities.Shared;
using R10.Core.Helpers;
using R10.Core.Interfaces;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions.ActionResults;
using R10.Web.Extensions;
using R10.Web.Helpers;
using R10.Web.Interfaces;
using R10.Web.Security;
using R10.Web.Services;
using R10.Web.Services.DocumentStorage;
using R10.Web.Services.SharePoint;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using R10.Web.Models;
using Sustainsys.Saml2.Metadata;
using R10.Core.DTOs;
using R10.Core.Services.Shared;
using R10.Core.Services;
using ActiveQueryBuilder.View.DatabaseSchemaView;

using R10.Web.Areas;

namespace R10.Web.Areas.Shared.Controllers
{
    [Area("Shared")]
    [Authorize(Policy = SharedAuthorizationPolicy.CanAccessESignature)]
    public class SignatureController : BaseController
    {

        private readonly ISignatureService _service;
        private readonly IAuthorizationService _authService;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly ISystemSettings<DefaultSetting> _settings;
        private readonly ISharePointViewModelService _sharePointViewModelService;
        private readonly IDocuSignService _docuSignService;

        public SignatureController(
            IAuthorizationService authService,
            ISignatureService service,
            IStringLocalizer<SharedResource> localizer,
            ISystemSettings<DefaultSetting> settings,
            ISharePointViewModelService sharePointViewModelService,
            IDocuSignService docuSignService
            )
        {
            _authService = authService;
            _service = service;
            _localizer = localizer;
            _settings = settings;
            _sharePointViewModelService = sharePointViewModelService;
            _docuSignService = docuSignService;
        }

        public async Task<IActionResult> Patent()
        {
            ViewData["CanModify"] = (await _authService.AuthorizeAsync(User, PatentAuthorizationPolicy.FullModify)).Succeeded;
            ViewData["SystemType"] = SystemTypeCode.Patent;
            return View("Index");
        }

        public async Task<IActionResult> Trademark()
        {
            ViewData["CanModify"] = (await _authService.AuthorizeAsync(User, TrademarkAuthorizationPolicy.FullModify)).Succeeded;
            ViewData["SystemType"] = SystemTypeCode.Trademark;
            return View("Index");
        }

        public async Task<IActionResult> GeneralMatter()
        {
            ViewData["CanModify"] = (await _authService.AuthorizeAsync(User, GeneralMatterAuthorizationPolicy.FullModify)).Succeeded;
            ViewData["SystemType"] = SystemTypeCode.GeneralMatter;
            return View("Index");
        }

        public async Task<IActionResult> SignatureReviewRead([DataSourceRequest] DataSourceRequest request, string systemType, string displayType)
        {
            var result = new List<DocReviewDTO>();
            result = await _service.GetDocs(systemType, displayType);

            var settings = await _settings.GetSetting();
            result.Each(d => {
                d.Name = d.FileName;

                d.LogFile = d.FileName;
                d.ItemId = d.DriveItemId;
                d.Document = d.Name;
                d.SignatureQESetupId = d.QESetupId;
                d.RecKey = d.ParentId;
                d.SystemType = d.SystemTypeCode;
                d.DocName = d.FileName;
                d.SystemTypeName = QuickEmailHelper.GetSystem(d.SystemTypeCode);

                switch (d.Source)
                {
                    case "DU":
                        d.SourceDescription = _localizer["Document Upload"].Value; break;
                    case "LG":
                        d.SourceDescription = _localizer["Letter Generation"].Value; break;
                    case "EFS":
                        d.SourceDescription = _localizer["IP Forms"].Value; break;
                }

                if (settings.IsSharePointIntegrationOn)
                {
                    d.DocLibraryFolder = _sharePointViewModelService.GetDocLibraryFolderFromScreenCode(d.ScreenCode);

                    if (displayType == "C")  //completed
                    {
                        d.Id = d.SignedDocDriveItemId;
                        d.Name = d.SignedFileName;
                    }
                    else
                    {
                        d.Id = d.DriveItemId;
                    }
                }

            });
            return Json(result.ToDataSourceResult(request));
        }


        public async Task<IActionResult> SignatureReviewUpdate(
            [Bind(Prefix = "updated")] List<DocReviewDTO> updated,
            [Bind(Prefix = "new")] IList<DocReviewDTO> added,
            [Bind(Prefix = "deleted")] IList<DocReviewDTO> deleted, string systemType)
        {
            if (updated.Any())
            {
                var canModify = false;
                if (systemType == SystemTypeCode.Patent)
                    canModify = (await _authService.AuthorizeAsync(User, PatentAuthorizationPolicy.FullModify)).Succeeded;

                else if (systemType == SystemTypeCode.Trademark)
                    canModify = (await _authService.AuthorizeAsync(User, TrademarkAuthorizationPolicy.FullModify)).Succeeded;

                else if (systemType == SystemTypeCode.GeneralMatter)
                    canModify = (await _authService.AuthorizeAsync(User, GeneralMatterAuthorizationPolicy.FullModify)).Succeeded;

                if (canModify)
                {
                    await _service.MarkReviewed(updated, User.GetUserName());
                    var success = _localizer["Changes have been saved successfully"].ToString();
                    return Ok(new { success = success });
                }
                return Unauthorized();
            }
            return Ok();
        }

        public async Task<IActionResult> SignatureMarkAllReviewed(List<DocReviewDTO> updated, string systemType)
        {
            if (updated.Any())
            {
                var canModify = false;
                if (systemType == SystemTypeCode.Patent)
                    canModify = (await _authService.AuthorizeAsync(User, PatentAuthorizationPolicy.FullModify)).Succeeded;

                else if (systemType == SystemTypeCode.Trademark)
                    canModify = (await _authService.AuthorizeAsync(User, TrademarkAuthorizationPolicy.FullModify)).Succeeded;

                else if (systemType == SystemTypeCode.GeneralMatter)
                    canModify = (await _authService.AuthorizeAsync(User, GeneralMatterAuthorizationPolicy.FullModify)).Succeeded;

                if (canModify)
                {
                    updated.ForEach(u => u.SignatureReviewed = true);
                    await _service.MarkReviewed(updated, User.GetUserName());
                    var success = _localizer["Changes have been saved successfully"].ToString();
                    return Ok(new { success = success });
                }
                return Unauthorized();
            }
            return Ok();
        }

        public async Task<IActionResult> DisplayRecipients(DocuSignRecipientViewModel viewModel)
        {
            return PartialView("_Recipients", viewModel);
        }


        public async Task<IActionResult> RecipientsRead([DataSourceRequest] DataSourceRequest request, DocuSignRecipientViewModel viewModel)
        {
            List<DocuSignRecipientViewModel> result;
            if (viewModel.Source == "EFS")
                result = new List<DocuSignRecipientViewModel> {viewModel};
            else
                result = await _docuSignService.GetRecipientsForDisplay(viewModel.QESetupId, viewModel.RoleLink, "To");

            var envelopeId = viewModel.envelopeId;
            if (!string.IsNullOrEmpty(envelopeId))
            {
                var docuSignRecipients = await _docuSignService.GetDocuSignRecipients(envelopeId);
                foreach (var rep in docuSignRecipients)
                {
                    if (!string.IsNullOrEmpty(rep.Email))
                    {
                        var filtered = result.Where(d => !string.IsNullOrEmpty(d.Email) && d.Email.ToLower() == rep.Email.ToLower()
                                            && d.Name.ToLower() == rep.RecipientName.ToLower())
                                            .FirstOrDefault();
                        if (filtered != null)
                        {
                            filtered.sentDateTime = rep.SentDate;
                            filtered.signedDateTime = rep.SignedDate;
                        }
                    }                    
                }
            }

            return Json(result.ToDataSourceResult(request));
        }

    }

}