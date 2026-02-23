using AutoMapper;
using AutoMapper.QueryableExtensions;
using Kendo.Mvc;
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
    public class LetterFilterController : BaseController
    {
        private readonly IAuthorizationService _authService;
        private readonly ILetterService _letterService;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly IMapper _mapper;
        private readonly string userFilter = "U";

        public LetterFilterController(
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

        #region Main Filter
        public async Task<IActionResult> GridRead([DataSourceRequest] DataSourceRequest request, int parentId, string sys)
        {
            var canAccess = await LetterHelper.CanAccessLetter(sys, User, _authService);
            Guard.Against.NoRecordPermission(canAccess);

            var result = await _letterService.LetterRecordSourceFilters.Where(rs => rs.LetterRecordSource.LetId == parentId).ProjectTo<LetterRecordSourceFilterViewModel>().ToListAsync();
            return Json(result.ToDataSourceResult(request));
        }

        public async Task<IActionResult> GridUpdate(
            string sys,
            [Bind(Prefix = "updated")] IEnumerable<LetterRecordSourceFilterViewModel> updated,
            [Bind(Prefix = "new")] IEnumerable<LetterRecordSourceFilterViewModel> added,
            [Bind(Prefix = "deleted")] IEnumerable<LetterRecordSourceFilterViewModel> deleted)
        {
            var canUpdate = await LetterHelper.CanUpdateLetter(sys, User, _authService);
            Guard.Against.NoRecordPermission(canUpdate);

            if (deleted.Any() || updated.Any() || added.Any())
            {
                if (!ModelState.IsValid)
                    return new JsonBadRequest(new { errors = ModelState.Errors() });

                await _letterService.LetterRecordSourceFilterUpdate(User.GetUserName(),
                    _mapper.Map<List<LetterRecordSourceFilter>>(updated),
                    _mapper.Map<List<LetterRecordSourceFilter>>(added),
                    _mapper.Map<List<LetterRecordSourceFilter>>(deleted));

                var success = deleted.Count() + updated.Count() + added.Count() == 1 ?
                    _localizer[$"Letter Filter has been saved successfully."].ToString() :
                    _localizer[$"Letter Filters have been saved successfully"].ToString();
                return Ok(new { success = success });

            }
            return Ok();
        }

        public async Task<IActionResult> GridDelete(string sys, [Bind(Prefix = "deleted")] LetterRecordSourceFilterViewModel deleted)
        {
            var canUpdate = await LetterHelper.CanUpdateLetter(sys, User, _authService);
            Guard.Against.NoRecordPermission(canUpdate);

            if (!ModelState.IsValid)
                return new JsonBadRequest(new { errors = ModelState.Errors() });

            if (deleted.LetFilterId > 0)
            {
                await _letterService.LetterRecordSourceFilterUpdate(User.GetUserName(),
                        new List<LetterRecordSourceFilter>(), new List<LetterRecordSourceFilter>(),
                        new List<LetterRecordSourceFilter>() { _mapper.Map<LetterRecordSourceFilter>(deleted) });
                return Ok(new { success = _localizer["Letter Filter has been deleted successfully."].ToString() });
            }

            return Ok();
        }

        #endregion

        #region User Filter
        public async Task<IActionResult> GridUserFilterRead([DataSourceRequest] DataSourceRequest request, int parentId, string sys)
        {
            var canAccess = await LetterHelper.CanAccessLetter(sys, User, _authService);
            Guard.Against.NoRecordPermission(canAccess);

            var result = await _letterService.LetterRecordSourceFiltersUser.Where(rs => rs.LetterRecordSource.LetId == parentId && rs.FilterSource == userFilter && rs.UserEmail == User.GetEmail())
                                .ProjectTo<LetterRecordSourceFilterUserViewModel>().ToListAsync();
            return Json(result.ToDataSourceResult(request));
        }

        public async Task<IActionResult> GridUserFilterUpdate(
           int parentId, string sys,
           [Bind(Prefix = "updated")] IEnumerable<LetterRecordSourceFilterUserViewModel> updated,
           [Bind(Prefix = "new")] IEnumerable<LetterRecordSourceFilterUserViewModel> added,
           [Bind(Prefix = "deleted")] IEnumerable<LetterRecordSourceFilterUserViewModel> deleted)
        {
            var canUpdate = await LetterHelper.CanUpdateLetter(sys, User, _authService);
            Guard.Against.NoRecordPermission(canUpdate);

            if (deleted.Any() || updated.Any() || added.Any())
            {
                if (!ModelState.IsValid)
                    return new JsonBadRequest(new { errors = ModelState.Errors() });
               
                await _letterService.LetterRecordSourceFilterUserUpdate(parentId, User.GetUserName(), User.GetEmail(),
                    _mapper.Map<List<LetterRecordSourceFilterUser>>(updated),
                    _mapper.Map<List<LetterRecordSourceFilterUser>>(added),
                    _mapper.Map<List<LetterRecordSourceFilterUser>>(deleted));

                var success = deleted.Count() + updated.Count() + added.Count() == 1 ?
                    _localizer[$"User Letter Filter has been saved successfully."].ToString() :
                    _localizer[$"User Letter Filters have been saved successfully"].ToString();
                return Ok(new { success = success });

            }
            return Ok();
        }

        
        public async Task<IActionResult> GridUserFilterDelete(string sys, [Bind(Prefix = "deleted")] LetterRecordSourceFilterUserViewModel deleted)
        {
            var canUpdate = await LetterHelper.CanUpdateLetter(sys, User, _authService);
            Guard.Against.NoRecordPermission(canUpdate);

            if (!ModelState.IsValid)
                return new JsonBadRequest(new { errors = ModelState.Errors() });

            if (deleted.UserFilterId > 0)
            {
                await _letterService.LetterRecordSourceFilterUserUpdate(0, User.GetUserName(), "",
                        new List<LetterRecordSourceFilterUser>(), new List<LetterRecordSourceFilterUser>(),
                        new List<LetterRecordSourceFilterUser>() { _mapper.Map<LetterRecordSourceFilterUser>(deleted) });
                return Ok(new { success = _localizer["User Letter Filter has been deleted successfully."].ToString() });
            }

            return Ok();
        }

        #endregion

        #region Letter Popup Screen
        public async Task<IActionResult> GridFilterListRead([DataSourceRequest] DataSourceRequest request, int letId)
        {
            var result = await _letterService.LetterRecordSourceFilters.Where(rsf => rsf.LetterRecordSource.LetId == letId)
                                .ProjectTo<LetterFilterListViewModel>().ToListAsync();
            return Json(result.ToDataSourceResult(request));
        }
        #endregion

        #region Pick List
        public async Task<IActionResult> GetFilterFieldsList(int recSourceId)
        {
            var picklist = await _letterService.GetFilterFieldsList(recSourceId);
            return Json(picklist);
        }

        public async Task<IActionResult> GetFilterDataList([DataSourceRequest] DataSourceRequest request, int recSourceId, string fieldName)
        {
            string text = request.Filters?.Count > 0 ? ((FilterDescriptor) request.Filters[0]).Value as string : "";

            var list = await _letterService.GetFilterDataList(recSourceId, fieldName, request.Page, request.PageSize, text);
            var result = list.Data.ToDataSourceResult(request);
            result.Total = list.RecordCount;
            return Json(result);
        }

        public async Task<IActionResult> GetFilterDataListToo(int recSourceId, string fieldName)
        {
          
            var list = await _letterService.GetFilterDataList(recSourceId, fieldName, 1, int.MaxValue, "");
            return Json(list.Data);
        }

        public async Task<Microsoft.AspNetCore.Mvc.ActionResult> DataValueMapper(int recSourceId, string fieldName, string value)
        {
            if (value == null) value = "";
            var result = await _letterService.FilterDataValueMapper(recSourceId, fieldName, value);
            return Json(result);
        }
        #endregion

    }
}