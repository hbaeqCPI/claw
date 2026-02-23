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
using R10.Core.Helpers;
using R10.Core.Interfaces;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;
using R10.Web.Extensions.ActionResults;
using R10.Web.Interfaces;
using R10.Web.Models;
using R10.Web.Security;

namespace R10.Web.Areas.Shared.Controllers
{
    [Area("Shared"), Authorize(Policy = SharedAuthorizationPolicy.CanAccessSystem)]
    public class DocFolderController : Microsoft.AspNetCore.Mvc.Controller
    {
        private readonly IDocumentService _docService;
        private readonly IDocumentViewModelService _docViewModelService;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly IMapper _mapper;

        public DocFolderController(
                    IDocumentService docService,
                    IDocumentViewModelService docViewModelService,
                    IStringLocalizer<SharedResource> localizer,
                    IMapper mapper)
        {
            _docService = docService;
            _docViewModelService = docViewModelService;
            _localizer = localizer;
            _mapper = mapper;
        }

        [HttpPost, Authorize(Policy = SharedAuthorizationPolicy.FullModify)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveFolder([FromBody] DocFolderViewModel docFolder)
        {
            if (ModelState.IsValid)
            {
                await _docService.UpdateFolder(User.GetUserName(), _mapper.Map<DocFolder>(docFolder));
                return Json(new { newName = docFolder.FolderName });
            }
            else
            {
                return new JsonBadRequest(new { errors = ModelState.Errors() });
            }
        }

        #region Tree Events
        public async Task<Microsoft.AspNetCore.Mvc.ActionResult> AddFolder(string id, string folderName)
        {
            var folderNode = await _docService.AddTreeFolder(id, folderName, User.GetUserName());
            //string imageFile = "folder";
            //folderNode.imageUrl = Url.Content($"~/images/tv-{imageFile}.png");
            if (folderNode !=null && folderNode.id =="")
            {
                return new JsonBadRequest("Folder name exist. Please use a different name.");
            }
            return Json(folderNode);
        }

        public async Task<Microsoft.AspNetCore.Mvc.ActionResult> RenameFolderDoc(string id, string newName)
        {
            if (string.IsNullOrEmpty(newName))
                return BadRequest(_localizer["Folder name is empty"].ToString());

            await _docViewModelService.RenameTreeNode(id, newName, User.GetUserName());
            return Ok();
        }

        public async Task<Microsoft.AspNetCore.Mvc.ActionResult> DeleteFolderDoc(string id)
        {
            if (await _docViewModelService.DeleteTreeNode(id))
                return Ok();
            else
                return BadRequest(_localizer["Unable to delete the document. The file may be locked by another process. Please try again later."].ToString());

        }

        public async Task<Microsoft.AspNetCore.Mvc.ActionResult> DropFolderDoc(string sourceId, string destId)
        {
            await _docViewModelService.DropTreeNode(sourceId, destId, User.GetUserName());
            return Ok();
        }

        #endregion
    }
}
