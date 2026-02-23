using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using R10.Core.Interfaces;
using R10.Web.Security;

using R10.Web.Areas;

namespace R10.Web.Areas.Shared.Controllers
{
    //contact person users, etc. have no shared role
    //[Area("Shared"), Authorize(Policy = SharedAuthorizationPolicy.CanAccessSystem)]
    [Area("Shared"), Authorize]
    public class DocTypeController : BaseController
    {
        private readonly IDocumentService _docService;

        public DocTypeController(
                IDocumentService docService
                )
        {
            _docService = docService;
        }

        public async Task<IActionResult> GetPicklistData(string property, string text, FilterType filterType)
        {
            var list = await _docService.DocTypes.Select(d => new { DocTypeId = d.DocTypeId, DocTypeName = d.DocTypeName }).OrderBy(d => d.DocTypeName).ToListAsync();
            return Json(list);

        }

    }
}