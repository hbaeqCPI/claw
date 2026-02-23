using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Localization;
using R10.Core;
using R10.Core.DTOs;
using R10.Core.Exceptions;
using R10.Core.Interfaces;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;
using R10.Web.Extensions.ActionResults;
using R10.Web.Helpers;
using R10.Web.Models;
using R10.Web.Security;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace R10.Web.Areas.Shared.Controllers
{
    [Area("Shared"), Authorize(Policy = SharedAuthorizationPolicy.CanAccessLetters)]
    public class DOCXTemplateController : Microsoft.AspNetCore.Mvc.Controller
    {
        private readonly IAuthorizationService _authService;
        private readonly IDOCXService _docxService;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IFileProvider _fileProvider;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public DOCXTemplateController(
            IAuthorizationService authService,
            IDOCXService docxService,
            IHostingEnvironment hostingEnvironment,
            IFileProvider fileProvider,
            IStringLocalizer<SharedResource> localizer
            )
        {
            _authService = authService;
            _docxService = docxService;
            _fileProvider = fileProvider;
            _hostingEnvironment = hostingEnvironment;
            _localizer = localizer;
        }

        public async Task<IActionResult> GridRead([DataSourceRequest] DataSourceRequest request,  string sys)
        {
            var canAccess = await DOCXHelper.CanAccessDOCX(sys, User, _authService);
            Guard.Against.NoRecordPermission(canAccess);

            var docxHelper = new DOCXGenerationHelper();
            var templateFolder = docxHelper.GetTemplateFolderRelative(_hostingEnvironment.ContentRootPath, sys);

            IDirectoryContents contents = _fileProvider.GetDirectoryContents(templateFolder);
            var result = contents.ToList().OrderBy(f => f.Name).Select(f => new DOCXTemplateViewModel { SystemType = sys, TemplateFile = f.Name, FileSize = f.Length });

            return Json(result.ToDataSourceResult(request));
        }

        public async Task<IActionResult> GridDelete ([Bind(Prefix = "deleted")] DOCXTemplateViewModel deletedTemplate)
        {
            var canUpdate = await DOCXHelper.CanUpdateDOCX(deletedTemplate.SystemType, User, _authService);
            Guard.Against.NoRecordPermission(canUpdate);

            if (!ModelState.IsValid)
                return new JsonBadRequest(new { errors = ModelState.Errors() });

            var inUseByDOCX = _docxService.DOCXesMain.Where(l => l.TemplateFile == deletedTemplate.TemplateFile && l.SystemScreen.SystemType == deletedTemplate.SystemType).Any();

            if (inUseByDOCX)
                return new JsonBadRequest(_localizer["You cannot delete this template, it is used by one of the DOCXes."].ToString());

            var docxHelper = new DOCXGenerationHelper();
            var templateFilePath = docxHelper.GetTemplateFilePath(_hostingEnvironment.ContentRootPath, deletedTemplate.SystemType, deletedTemplate.TemplateFile);
            if (docxHelper.DeleteTemplateFile(templateFilePath))
                return Ok(new { success = _localizer["The template file has been successfully deleted."].ToString() });
            else
                return new JsonBadRequest(_localizer["The template file does not exists or you do not have permission to delete it."].ToString());
        }

        public async Task<Microsoft.AspNetCore.Mvc.ActionResult> GridDownload (string sys, string templateFile)
        {
            var canAccess = await DOCXHelper.CanAccessDOCX(sys, User, _authService);
            Guard.Against.NoRecordPermission(canAccess);

            var docxHelper = new DOCXGenerationHelper();
            var templateFilePath = docxHelper.GetTemplateFilePath(_hostingEnvironment.ContentRootPath, sys, templateFile);
            
            var mimeType = docxHelper.MimeType(templateFile);
            byte[] fileBytes = System.IO.File.ReadAllBytes(templateFilePath);

            return File(fileBytes, mimeType, templateFile);
        }



        public async Task<IActionResult> SaveTemplateFile(IEnumerable<IFormFile> droppedFiles, string sys, bool overwriteExisting)
        {
            var canUpdate = await DOCXHelper.CanUpdateDOCX(sys, User, _authService);
            Guard.Against.NoRecordPermission(canUpdate);

            // return custom error messages via Content
            if (!ModelState.IsValid)
                return new JsonBadRequest(new { errors = ModelState.Errors() });

            var uploadedFile = droppedFiles.First();
            var fileName = Path.GetFileName(uploadedFile.FileName);

            var docxHelper = new DOCXGenerationHelper();
            var templateFilePath = docxHelper.GetTemplateFilePath(_hostingEnvironment.ContentRootPath, sys, fileName);

            if (docxHelper.ExistTemplateFile(templateFilePath) && !overwriteExisting)
                return Content(_localizer["The template file exists. Please check 'Overwrite if existing' to overwrite previously uploaded template."].ToString());


            if (await docxHelper.UploadTemplateFile(templateFilePath, uploadedFile))
                return Ok(new { success = _localizer["The template file has been uploaded."].ToString() });
            else
                return Content(_localizer["An error was encountered uploading the template file."].ToString());
        }

        
        // docx content view - template combo
        public async Task<IActionResult> GetTemplateList(string sys)
        {
            var canAccess = await DOCXHelper.CanAccessDOCX(sys, User, _authService);
            Guard.Against.NoRecordPermission(canAccess);

            var docxHelper = new DOCXGenerationHelper();
            var templateFolder = docxHelper.GetTemplateFolder(_hostingEnvironment.ContentRootPath, sys);

            string[] templateFiles = Directory.GetFiles(templateFolder, "*.docx");
            var pickList = new List<LookupDTO>();
            templateFiles.Each(f => pickList.Add(new LookupDTO { Value = Path.GetFileName(f) }));

            return Json(pickList);
        }


        
    }
}
