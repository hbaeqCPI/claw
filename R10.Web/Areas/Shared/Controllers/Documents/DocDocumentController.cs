using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using R10.Core.Entities.Documents;
using R10.Core.Entities.Shared;
using R10.Core.Helpers;
using R10.Core.Interfaces;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;
using R10.Web.Extensions.ActionResults;
using R10.Web.Interfaces;
using R10.Web.Models;
using R10.Web.Security;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

using R10.Web.Areas;

namespace R10.Web.Areas.Shared.Controllers
{
    [Area("Shared"), Authorize(Policy = SharedAuthorizationPolicy.CanAccessSystem)]
    public class DocDocumentController : BaseController
    {
        private readonly IDocumentService _docService;
        private readonly IDocumentViewModelService _docViewModelService;
        private readonly IStringLocalizer<SharedResource> _localizer;
        //private readonly ISystemSettings<DefaultSetting> _settings;
        private readonly IMapper _mapper;
        private readonly IHostingEnvironment _hostingEnvironment;

        public DocDocumentController(
                    IDocumentService docService,
                    IDocumentViewModelService docViewModelService,
                    IStringLocalizer<SharedResource> localizer,
                    ISystemSettings<DefaultSetting> settings,
                    IMapper mapper,
                    IHostingEnvironment hostingEnvironment
            )
        {
            _docService = docService;
            _docViewModelService = docViewModelService;
            _localizer = localizer;
            _mapper = mapper;
            _hostingEnvironment = hostingEnvironment;
        }

        public async Task<IActionResult> GridRead([DataSourceRequest] DataSourceRequest request, int folderId)
        {
            var result = await _docViewModelService.GetDocumentsByFolderId(folderId);
            return Json(result.ToDataSourceResult(request));
        }

        [Authorize(Policy = SharedAuthorizationPolicy.FullModify)]
        public async Task<IActionResult> GridAdd([DataSourceRequest] DataSourceRequest request, int folderId)
        {
            var model = await _docViewModelService.CreateDocumentEditorViewModel(folderId, 0);
            model.Author = User.GetEmail();
            return PartialView("../Documents/_DocumentEditor", model);
        }

        [Authorize(Policy = SharedAuthorizationPolicy.FullModify)]
        public async Task<IActionResult> GridUpdate([DataSourceRequest] DataSourceRequest request, int id, string docViewer)
        {
            var model = await _docViewModelService.CreateDocumentEditorViewModel(0, id);

            var lockedBy = await _docService.IsLocked(id, User.GetUserName());
            if (!string.IsNullOrEmpty(lockedBy))
            {
                return BadRequest(_localizer["Document is currently checked out by: "] + lockedBy);
            }

            model.DocViewer = docViewer;
            return PartialView("../Documents/_DocumentEditor", model);
        }

        [Authorize(Policy = SharedAuthorizationPolicy.FullModify)]
        public async Task<IActionResult> GridDelete([Bind(Prefix = "deleted")] DocDocumentListViewModel deleted)
        {
            if (deleted.DocId > 0)
            {
                await _docService.UpdateDocuments(User.GetUserName(), new List<DocDocument>(), new List<DocDocument>(), new List<DocDocument>() { _mapper.Map<DocDocument>(deleted) });
                return Ok(new { success = _localizer["Document has been deleted successfully."].ToString() });
            }
            return Ok();
        }

        [Authorize(Policy = SharedAuthorizationPolicy.FullModify)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveUploadedDocument(IEnumerable<IFormFile> droppedFiles, int folderId)
        {
            // return custom error messages via Content
            if (!ModelState.IsValid)
                return new JsonBadRequest(new { errors = ModelState.Errors() });

            var document = new DocDocument { FolderId = folderId, Author = User.GetEmail() };
            UpdateEntityStamps(document, 0);

            var uploadedFile = droppedFiles.First();
            if (await _docViewModelService.SaveUploadedDocument(document, uploadedFile, _hostingEnvironment.ContentRootPath))
                return Ok(new { success = _localizer["The file has been uploaded."].ToString() });
            else
                return Content(_localizer["An error was encountered when uploading the document file."].ToString());
        }

        [HttpPost, Authorize(Policy = SharedAuthorizationPolicy.FullModify)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveDocument(DocDocumentViewModel viewModel)
        {
            // this is called from pop-up window DocDocument/_DocumentEditor, and from _DetailUserDocument
            // note: file extension validation done at browser/client-side
            if (!ModelState.IsValid)
                return new JsonBadRequest(new { errors = ModelState.Errors() });

            var lockedBy = await _docService.IsLocked(viewModel.DocId, User.GetUserName());
            if (!string.IsNullOrEmpty(lockedBy))
            {
                return BadRequest(_localizer["Document is currently checked out by: "] + lockedBy);
            }

            UpdateEntityStamps(viewModel, viewModel.DocId);

            await _docViewModelService.SaveDocumentPopup(viewModel, _hostingEnvironment.ContentRootPath);
            return Json(new { newName = viewModel.DocName });
        }


        public async Task<IActionResult> SaveDocuments(IEnumerable<IFormFile> droppedDocs, int folderId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (droppedDocs.Count() <= 0)
            {
                return Content("No Document To Upload");
            }

            var viewModels = new List<DocDocumentViewModel>();
            foreach (var file in droppedDocs)
            {
                var viewModel = new DocDocumentViewModel();
                viewModel.ParentId = folderId;
                viewModel.UploadedFile = file;
                viewModel.Author = User.GetEmail();
                viewModel.CreatedBy = User.GetUserName(); //need to pass these to add in tblDocFile
                viewModel.UpdatedBy = User.GetUserName();
                viewModel.LastUpdate = DateTime.Now;
                viewModel.DateCreated = DateTime.Now;
                viewModel.UserFileName = file.FileName;
                viewModel.FolderId = folderId;

                viewModels.Add(viewModel);
            }
            //need to update entity stamps, check to see how to do so when batch uploading
            await _docViewModelService.SaveUploadedDocuments(viewModels);
            return Content("");
        }



    }
}