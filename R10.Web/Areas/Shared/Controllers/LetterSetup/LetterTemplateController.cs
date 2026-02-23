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
using R10.Core.Entities.Patent;
using R10.Core.Exceptions;
using R10.Core.Interfaces;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;
using R10.Web.Extensions.ActionResults;
using R10.Web.Helpers;
using R10.Web.Models;
using R10.Web.Security;
using R10.Web.Services.DocumentStorage;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace R10.Web.Areas.Shared.Controllers
{
    [Area("Shared"), Authorize(Policy = SharedAuthorizationPolicy.CanAccessLetters)]
    public class LetterTemplateController : Microsoft.AspNetCore.Mvc.Controller
    {
        private readonly IAuthorizationService _authService;
        private readonly ILetterService _letterService;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IFileProvider _fileProvider;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly IDocumentStorage _documentStorage;
        private readonly ISystemSettings<PatSetting> _patSettings;

        public LetterTemplateController(
            IAuthorizationService authService,
            ILetterService letterService,
            IHostingEnvironment hostingEnvironment,
            IFileProvider fileProvider,
            IStringLocalizer<SharedResource> localizer,
            IDocumentStorage documentStorage,
            ISystemSettings<PatSetting> patSettings
            )
        {
            _authService = authService;
            _letterService = letterService;
            _fileProvider = fileProvider;
            _hostingEnvironment = hostingEnvironment;
            _localizer = localizer;
            _documentStorage = documentStorage;
            _patSettings = patSettings;
        }

        public async Task<IActionResult> GridRead([DataSourceRequest] DataSourceRequest request, string sys)
        {
            var canAccess = await LetterHelper.CanAccessLetter(sys, User, _authService);
            Guard.Against.NoRecordPermission(canAccess);
            
            var systemFolder = GetSystemName(sys);
            var templatePath = Path.Combine(_documentStorage.LetterTemplateFolder, systemFolder);
            var files = _documentStorage.GetListOfFiles(templatePath);
            
            files.ForEach(f => f.SystemType = sys);
            return Json(files.ToDataSourceResult(request));
        }

        public async Task<IActionResult> GridDelete ([Bind(Prefix = "deleted")] LetterTemplateViewModel deletedTemplate)
        {
            var canUpdate = await LetterHelper.CanUpdateLetter(deletedTemplate.SystemType, User, _authService);
            Guard.Against.NoRecordPermission(canUpdate);

            if (!ModelState.IsValid)
                return new JsonBadRequest(new { errors = ModelState.Errors() });

            var inUseByLetter = _letterService.LettersMain.Where(l => l.TemplateFile == deletedTemplate.TemplateFile && l.SystemScreen.SystemType == deletedTemplate.SystemType).Any();

            if (inUseByLetter)
                return new JsonBadRequest(_localizer["You cannot delete this template, it is used by one of the letters."].ToString());

            var systemFolder = GetSystemName(deletedTemplate.SystemType);
            var templatePath = Path.Combine(_documentStorage.LetterTemplateFolder, systemFolder, deletedTemplate.TemplateFile);
            await _documentStorage.DeleteFile(templatePath);
            return Ok(new { success = _localizer["The template file has been successfully deleted."].ToString() });

        }

        public async Task<Microsoft.AspNetCore.Mvc.ActionResult> GridDownload (string sys, string templateFile)
        {
            var canAccess = await LetterHelper.CanAccessLetter(sys, User, _authService);
            Guard.Against.NoRecordPermission(canAccess);

            var systemFolder = GetSystemName(sys);
            var templatePath = Path.Combine(_documentStorage.LetterTemplateFolder, systemFolder,templateFile);
            var stream = await _documentStorage.GetFileStream(templatePath);

            var letterHelper = new LetterGenerationHelper();
            var mimeType = letterHelper.MimeType(templateFile);
            return new FileStreamResult(stream, mimeType) { FileDownloadName = templateFile };
        }

        public async Task<IActionResult> SaveTemplateFile(IEnumerable<IFormFile> droppedFiles, string sys, bool overwriteExisting)
        {
            var canUpdate = await LetterHelper.CanUpdateLetter(sys, User, _authService);
            Guard.Against.NoRecordPermission(canUpdate);

            // return custom error messages via Content
            if (!ModelState.IsValid)
                return new JsonBadRequest(new { errors = ModelState.Errors() });

            var uploadedFile = droppedFiles.First();
            var fileName = Path.GetFileName(uploadedFile.FileName);

            var systemFolder = GetSystemName(sys);
            var templateFile = Path.Combine(_documentStorage.LetterTemplateFolder, systemFolder, fileName);

            if (await _documentStorage.IsFileExists(templateFile) && !overwriteExisting)
                return Content(_localizer["The template file exists. Please check 'Overwrite if existing' to overwrite previously uploaded template."].ToString());
            await _documentStorage.DeleteFile(templateFile);
            await _documentStorage.SaveFile(uploadedFile, templateFile, new DocumentStorageHeader());

            return Ok(new { success = _localizer["The template file has been uploaded."].ToString() });
        }


        // letter content view - template combo
        public async Task<IActionResult> GetTemplateList(string sys)
        {
            var canAccess = await LetterHelper.CanAccessLetter(sys, User, _authService);
            Guard.Against.NoRecordPermission(canAccess);

            var systemFolder = GetSystemName(sys);
            var templatePath = Path.Combine(_documentStorage.LetterTemplateFolder, systemFolder);
            var files = _documentStorage.GetListOfFiles(templatePath);

            var pickList = new List<LookupDTO>();
            files.Each(f => pickList.Add(new LookupDTO { Value = f.TemplateFile }));

            if (sys == "P")
            {
                if (!_patSettings.GetSetting().Result.IsInventorRemunerationOn)
                {
                    pickList.RemoveAll(c => c.Value.StartsWith("Pat-Remuneration"));
                }
                if (!_patSettings.GetSetting().Result.IsInventorFRRemunerationOn)
                {
                    pickList.RemoveAll(c => c.Value.StartsWith("Pat-FRRemuneration"));
                }
            }

            return Json(pickList);
        }

        private string GetSystemName(string systemType)
        {
            return systemType == "P" ? "Patent" : systemType == "T" ? "Trademark" : systemType == "G" ? "GeneralMatter" : "";
        }

    }
}
