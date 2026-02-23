using AutoMapper;
using AutoMapper.QueryableExtensions;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using R10.Core;
using R10.Core.Entities;
using R10.Core.Exceptions;
using R10.Core.Helpers;
using R10.Core.Interfaces;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;
using R10.Web.Extensions.ActionResults;
using R10.Web.Helpers;
using R10.Web.Models;
using R10.Web.Security;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using R10.Web.Areas;

namespace R10.Web.Areas.Shared.Controllers
{
    [Area("Shared"), Authorize(Policy = SharedAuthorizationPolicy.CanAccessLetters)]
    public class DOCXUserDataController : BaseController
    {
        private readonly IAuthorizationService _authService;
        private readonly IDOCXService _docxService;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly IMapper _mapper;

        public DOCXUserDataController(
            IAuthorizationService authService,
            IDOCXService docxService,
            IStringLocalizer<SharedResource> localizer,
            IMapper mapper)
        {
            _authService = authService;
            _docxService = docxService;
            _localizer = localizer;
            _mapper = mapper;
        }

        public async Task<IActionResult> GridRead([DataSourceRequest] DataSourceRequest request, int parentId, string sys)
        {
            var canAccess = await DOCXHelper.CanAccessDOCX(sys, User, _authService);
            Guard.Against.NoRecordPermission(canAccess);

            var result = await _docxService.DOCXUserData.Where(ud => ud.DOCXId == parentId).ToListAsync();
            return Json(result.ToDataSourceResult(request));
        }

        public async Task<IActionResult> GridUpdate(
            int parentId, string sys,
            [Bind(Prefix = "updated")]IEnumerable<DOCXUserDataViewModel> updated, 
            [Bind(Prefix = "new")]IEnumerable<DOCXUserDataViewModel> added, 
            [Bind(Prefix = "deleted")]IEnumerable<DOCXUserDataViewModel> deleted)
        {
            var canUpdate = await DOCXHelper.CanUpdateDOCX(sys, User, _authService);
            Guard.Against.NoRecordPermission(canUpdate);

            if (deleted.Any() || updated.Any() || added.Any())
            {
                if (!ModelState.IsValid)
                    return new JsonBadRequest(new { errors = ModelState.Errors() });

                await _docxService.UserDataUpdate(parentId, User.GetUserName(),
                    _mapper.Map<List<DOCXUserData>>(updated),
                    _mapper.Map<List<DOCXUserData>>(added),
                    _mapper.Map<List<DOCXUserData>>(deleted));

                var success = deleted.Count() + updated.Count() + added.Count() == 1 ?
                    _localizer[$"DOCX User Data has been saved successfully."].ToString() :
                    _localizer[$"DOCX User Data have been saved successfully"].ToString();
                return Ok(new { success = success });

            }
            return Ok();
        }

        public async Task<IActionResult> GridDelete(string sys, [Bind(Prefix = "deleted")] DOCXUserDataViewModel deleted)
        {
            var canUpdate = await DOCXHelper.CanUpdateDOCX(sys, User, _authService);
            Guard.Against.NoRecordPermission(canUpdate);

            if (!ModelState.IsValid)
                return new JsonBadRequest(new { errors = ModelState.Errors() });

            if (deleted.DOCXDataId> 0)
            {
                await _docxService.UserDataUpdate(deleted.DOCXId, User.GetUserName(), new List<DOCXUserData>(), new List<DOCXUserData>(),
                                    new List<DOCXUserData>() { _mapper.Map<DOCXUserData>(deleted) });
                return Ok(new { success = _localizer["DOCX User Data has been deleted successfully."].ToString() });
            }

            return Ok();
        }

       
    }
}