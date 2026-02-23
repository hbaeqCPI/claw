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
    public class DOCXFilterController : BaseController
    {
        private readonly IAuthorizationService _authService;
        private readonly IDOCXService _docxService;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly IMapper _mapper;
        private readonly string userFilter = "U";

        public DOCXFilterController(
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

        #region Main Filter
        public async Task<IActionResult> GridRead([DataSourceRequest] DataSourceRequest request, int parentId, string sys)
        {
            var canAccess = await DOCXHelper.CanAccessDOCX(sys, User, _authService); //TO DO: permission
            Guard.Against.NoRecordPermission(canAccess);

            var result = await _docxService.DOCXRecordSourceFilters.Where(rs => rs.DOCXRecordSource.DOCXId == parentId).ProjectTo<DOCXRecordSourceFilterViewModel>().ToListAsync();
            return Json(result.ToDataSourceResult(request));
        }

        public async Task<IActionResult> GridUpdate(
            string sys,
            [Bind(Prefix = "updated")] IEnumerable<DOCXRecordSourceFilterViewModel> updated,
            [Bind(Prefix = "new")] IEnumerable<DOCXRecordSourceFilterViewModel> added,
            [Bind(Prefix = "deleted")] IEnumerable<DOCXRecordSourceFilterViewModel> deleted)
        {
            var canUpdate = await DOCXHelper.CanUpdateDOCX(sys, User, _authService); //TO DO: permission
            Guard.Against.NoRecordPermission(canUpdate);

            if (deleted.Any() || updated.Any() || added.Any())
            {
                if (!ModelState.IsValid)
                    return new JsonBadRequest(new { errors = ModelState.Errors() });

                await _docxService.DOCXRecordSourceFilterUpdate(User.GetUserName(),
                    _mapper.Map<List<DOCXRecordSourceFilter>>(updated),
                    _mapper.Map<List<DOCXRecordSourceFilter>>(added),
                    _mapper.Map<List<DOCXRecordSourceFilter>>(deleted));

                var success = deleted.Count() + updated.Count() + added.Count() == 1 ?
                    _localizer[$"DOCX Filter has been saved successfully."].ToString() :
                    _localizer[$"DOCX Filters have been saved successfully"].ToString();
                return Ok(new { success = success });

            }
            return Ok();
        }
         
        public async Task<IActionResult> GridDelete(string sys, [Bind(Prefix = "deleted")] DOCXRecordSourceFilterViewModel deleted)
        {
            var canUpdate = await DOCXHelper.CanUpdateDOCX(sys, User, _authService);
            Guard.Against.NoRecordPermission(canUpdate);

            if (!ModelState.IsValid)
                return new JsonBadRequest(new { errors = ModelState.Errors() });

            if (deleted.DOCXFilterId > 0)
            {
                await _docxService.DOCXRecordSourceFilterUpdate(User.GetUserName(),
                        new List<DOCXRecordSourceFilter>(), new List<DOCXRecordSourceFilter>(),
                        new List<DOCXRecordSourceFilter>() { _mapper.Map<DOCXRecordSourceFilter>(deleted) });
                return Ok(new { success = _localizer["DOCX Filter has been deleted successfully."].ToString() });
            }

            return Ok();
        }

        #endregion

        #region User Filter
        public async Task<IActionResult> GridUserFilterRead([DataSourceRequest] DataSourceRequest request, int parentId, string sys)
        {
            var canAccess = await DOCXHelper.CanAccessDOCX(sys, User, _authService);
            Guard.Against.NoRecordPermission(canAccess);

            var result = await _docxService.DOCXRecordSourceFiltersUser.Where(rs => rs.DOCXRecordSource.DOCXId == parentId && rs.FilterSource == userFilter && rs.UserEmail == User.GetEmail())
                                .ProjectTo<DOCXRecordSourceFilterUserViewModel>().ToListAsync();
            return Json(result.ToDataSourceResult(request));
        }

        public async Task<IActionResult> GridUserFilterUpdate(
           int parentId, string sys,
           [Bind(Prefix = "updated")] IEnumerable<DOCXRecordSourceFilterUserViewModel> updated,
           [Bind(Prefix = "new")] IEnumerable<DOCXRecordSourceFilterUserViewModel> added,
           [Bind(Prefix = "deleted")] IEnumerable<DOCXRecordSourceFilterUserViewModel> deleted)
        {
            var canUpdate = await DOCXHelper.CanUpdateDOCX(sys, User, _authService);
            Guard.Against.NoRecordPermission(canUpdate);

            if (deleted.Any() || updated.Any() || added.Any())
            {
                if (!ModelState.IsValid)
                    return new JsonBadRequest(new { errors = ModelState.Errors() });

                try {
                    await _docxService.DOCXRecordSourceFilterUserUpdate(parentId, User.GetUserName(), User.GetEmail(),
                    _mapper.Map<List<DOCXRecordSourceFilterUser>>(updated),
                    _mapper.Map<List<DOCXRecordSourceFilterUser>>(added),
                    _mapper.Map<List<DOCXRecordSourceFilterUser>>(deleted));

                    var success = deleted.Count() + updated.Count() + added.Count() == 1 ?
                    _localizer[$"User DOCX Filter has been saved successfully."].ToString() :
                    _localizer[$"User DOCX Filters have been saved successfully"].ToString();
                    return Ok(new { success = success });
                }
                catch(Exception ex)
                {
                    var message = ex.Message;
                    return Ok();
                }             
                               

            }
            return Ok();
        }

        
        public async Task<IActionResult> GridUserFilterDelete(string sys, [Bind(Prefix = "deleted")] DOCXRecordSourceFilterUserViewModel deleted)
        {
            var canUpdate = await DOCXHelper.CanUpdateDOCX(sys, User, _authService);
            Guard.Against.NoRecordPermission(canUpdate);

            if (!ModelState.IsValid)
                return new JsonBadRequest(new { errors = ModelState.Errors() });

            if (deleted.UserFilterId > 0)
            {
                await _docxService.DOCXRecordSourceFilterUserUpdate(0, User.GetUserName(), "",
                        new List<DOCXRecordSourceFilterUser>(), new List<DOCXRecordSourceFilterUser>(),
                        new List<DOCXRecordSourceFilterUser>() { _mapper.Map<DOCXRecordSourceFilterUser>(deleted) });
                return Ok(new { success = _localizer["User DOCX Filter has been deleted successfully."].ToString() });
            }

            return Ok();
        }

        #endregion

        #region DOCX Popup Screen
        public async Task<IActionResult> GridFilterListRead([DataSourceRequest] DataSourceRequest request, int docxId)
        {
            var result = await _docxService.DOCXRecordSourceFilters.Where(rsf => rsf.DOCXRecordSource.DOCXId == docxId)
                                .ProjectTo<DOCXFilterListViewModel>().ToListAsync();
            return Json(result.ToDataSourceResult(request));
        }
        #endregion

        #region Pick List
        public async Task<IActionResult> GetFilterFieldsList(int recSourceId)
        {
            var picklist = await _docxService.GetFilterFieldsList(recSourceId);
            return Json(picklist);
        }

        public async Task<IActionResult> GetFilterDataList([DataSourceRequest] DataSourceRequest request, int recSourceId, string fieldName)
        {
            string text = request.Filters?.Count > 0 ? ((FilterDescriptor) request.Filters[0]).Value as string : "";

            var list = await _docxService.GetFilterDataList(recSourceId, fieldName, request.Page, request.PageSize, text);
            var result = list.Data.ToDataSourceResult(request);
            result.Total = list.RecordCount;
            return Json(result);
        }

        public async Task<IActionResult> GetFilterDataListToo(int recSourceId, string fieldName)
        {
          
            var list = await _docxService.GetFilterDataList(recSourceId, fieldName, 1, int.MaxValue, "");
            return Json(list.Data);
        }

        public async Task<Microsoft.AspNetCore.Mvc.ActionResult> DataValueMapper(int recSourceId, string fieldName, string value)
        {
            if (value == null) value = "";
            var result = await _docxService.FilterDataValueMapper(recSourceId, fieldName, value);
            return Json(result);
        }
        #endregion

    }
}