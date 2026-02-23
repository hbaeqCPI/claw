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
    public class LetterUserDataController : BaseController
    {
        private readonly IAuthorizationService _authService;
        private readonly ILetterService _letterService;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly IMapper _mapper;

        public LetterUserDataController(
            IAuthorizationService authService,
            ILetterService letterService,
            IStringLocalizer<SharedResource> localizer,
            IMapper mapper)
        {
            _authService = authService;
            _letterService = letterService;
            _localizer = localizer;
            _mapper = mapper;
        }

        public async Task<IActionResult> GridRead([DataSourceRequest] DataSourceRequest request, int parentId, string sys)
        {
            var canAccess = await LetterHelper.CanAccessLetter(sys, User, _authService);
            Guard.Against.NoRecordPermission(canAccess);

            var result = await _letterService.LetterUserData.Where(ud => ud.LetId == parentId).ToListAsync();
            return Json(result.ToDataSourceResult(request));
        }

        public async Task<IActionResult> GridUpdate(
            int parentId, string sys,
            [Bind(Prefix = "updated")]IEnumerable<LetterUserDataViewModel> updated, 
            [Bind(Prefix = "new")]IEnumerable<LetterUserDataViewModel> added, 
            [Bind(Prefix = "deleted")]IEnumerable<LetterUserDataViewModel> deleted)
        {
            var canUpdate = await LetterHelper.CanUpdateLetter(sys, User, _authService);
            Guard.Against.NoRecordPermission(canUpdate);

            if (deleted.Any() || updated.Any() || added.Any())
            {
                if (!ModelState.IsValid)
                    return new JsonBadRequest(new { errors = ModelState.Errors() });

                await _letterService.UserDataUpdate(parentId, User.GetUserName(),
                    _mapper.Map<List<LetterUserData>>(updated),
                    _mapper.Map<List<LetterUserData>>(added),
                    _mapper.Map<List<LetterUserData>>(deleted));

                var success = deleted.Count() + updated.Count() + added.Count() == 1 ?
                    _localizer[$"Letter User Data has been saved successfully."].ToString() :
                    _localizer[$"Letter User Data have been saved successfully"].ToString();
                return Ok(new { success = success });

            }
            return Ok();
        }

        public async Task<IActionResult> GridDelete(string sys, [Bind(Prefix = "deleted")] LetterUserDataViewModel deleted)
        {
            var canUpdate = await LetterHelper.CanUpdateLetter(sys, User, _authService);
            Guard.Against.NoRecordPermission(canUpdate);

            if (!ModelState.IsValid)
                return new JsonBadRequest(new { errors = ModelState.Errors() });

            if (deleted.LetDataId> 0)
            {
                await _letterService.UserDataUpdate(deleted.LetId, User.GetUserName(), new List<LetterUserData>(), new List<LetterUserData>(),
                                    new List<LetterUserData>() { _mapper.Map<LetterUserData>(deleted) });
                return Ok(new { success = _localizer["Letter User Data has been deleted successfully."].ToString() });
            }

            return Ok();
        }

       
    }
}