using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Drawing.Charts;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using R10.Core.DTOs;
using R10.Core.Interfaces;
using R10.Web.Helpers;
using R10.Web.Models;
using R10.Web.Security;

namespace R10.Web.Areas.Shared.Controllers
{
    [Area("Shared"), Authorize(Policy = SharedAuthorizationPolicy.CanAccessLetters)]
    public class DOCXHeaderKeywordController : Microsoft.AspNetCore.Mvc.Controller
    {
        private readonly IDOCXService _docxService;
        private readonly ExportHelper _exportHelper;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public DOCXHeaderKeywordController(IDOCXService docxService, ExportHelper exportHelper,
            IStringLocalizer<SharedResource> localizer)
        {
            _docxService = docxService;
            _exportHelper = exportHelper;
            _localizer = localizer;
        }

        public async Task<IActionResult> GridRead([DataSourceRequest] DataSourceRequest request)
        {
            var result = await _docxService.GetUSPTOHeaderKeywordList();
            return Json(result.ToDataSourceResult(request));
        }

        public async Task<IActionResult> ExportToExcel()
        {
            var result = await _docxService.GetUSPTOHeaderKeywordExcelList();

            var fileStream = await _exportHelper.ListToExcelMemoryStream(result, "USPTO Header Keyword List", _localizer);

            return File(fileStream.ToArray(), ImageHelper.GetContentType(".xlsx"), "USPTO DOCX Keywords For Section Headers.xlsx");
        }
    }
}
