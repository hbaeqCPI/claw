using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using R10.Core.Interfaces;
using R10.Web.Helpers;
using R10.Web.Models;
using R10.Web.Security;

namespace R10.Web.Areas.Shared.Controllers
{
    [Area("Shared"), Authorize(Policy = SharedAuthorizationPolicy.CanAccessLetters)]
    public class DOCXFieldListController : Microsoft.AspNetCore.Mvc.Controller
    {
        private readonly IDOCXService _docxService;
        private readonly ExportHelper _exportHelper;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public DOCXFieldListController(IDOCXService docxService, ExportHelper exportHelper,
            IStringLocalizer<SharedResource> localizer)
        {
            _docxService = docxService;
            _exportHelper = exportHelper;
            _localizer = localizer;
        }

        public async Task<IActionResult> GridRead([DataSourceRequest] DataSourceRequest request, int parentId)
        {
            string sortField = "";
            string sortDir = "";

            if (request.Sorts != null && request.Sorts.Any())
            {
                sortField = request.Sorts[0].Member;
                sortDir = request.Sorts[0].SortDirection.ToString();
            }
            var result = await _docxService.GetFieldList(parentId, sortField, sortDir);
            return Json(result.ToDataSourceResult(request));
        }

        public async Task<IActionResult> ExportToExcel(int docxId, string sortField, string sortDir)
        {
            var result = await _docxService.GetFieldList(docxId, sortField, sortDir);

            var fileStream = await _exportHelper.ListToExcelMemoryStream(result, "FieldList", _localizer);

            return File(fileStream.ToArray(), ImageHelper.GetContentType(".xlsx"), "DOCXFieldList.xlsx");

        }


    }
}
