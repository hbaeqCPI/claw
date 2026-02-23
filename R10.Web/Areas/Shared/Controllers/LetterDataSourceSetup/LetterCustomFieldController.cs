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
    public class LetterCustomFieldController : BaseController
    {
        private readonly IAuthorizationService _authService;
        private readonly ILetterService _letterService;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly IMapper _mapper;

        public LetterCustomFieldController(
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

            var result = await _letterService.LetterCustomFields.Where(rs => rs.DataSourceId == parentId).ProjectTo<LetterCustomFieldViewModel>().ToListAsync();
            return Json(result.ToDataSourceResult(request));
        }

        public async Task<IActionResult> GridUpdate(
            int parentId, string sys,
            [Bind(Prefix = "updated")] IEnumerable<LetterCustomFieldViewModel> updated,
            [Bind(Prefix = "new")] IEnumerable<LetterCustomFieldViewModel> added,
            [Bind(Prefix = "deleted")] IEnumerable<LetterCustomFieldViewModel> deleted)
        {
            var canUpdate = await LetterHelper.CanUpdateLetter(sys, User, _authService);
            Guard.Against.NoRecordPermission(canUpdate);

            if (deleted.Any() || updated.Any() || added.Any())
            {
                if (!ModelState.IsValid)
                    return new JsonBadRequest(new { errors = ModelState.Errors() });

                if (updated.Any() || added.Any())
                {
                    var dataSourceFieldList = await _letterService.GetDataSourceFieldList(parentId);
                    var existingfieldList = dataSourceFieldList.Select(l => l.ColumnName.Trim('[', ']').ToLower()).ToList();

                    var newfieldList = added.Select(u => u.CustomFieldName.ToLower()).ToList();
                    newfieldList.AddRange(updated.Select(u => u.CustomFieldName.ToLower()).ToList());

                    if (existingfieldList.Intersect(newfieldList).Any())
                        return new JsonBadRequest(_localizer["You cannot create this Custom Merged Field, field name is already in the data source."].ToString());
                }

                await _letterService.LetterCustomFieldUpdate(parentId, User.GetUserName(), User.GetEmail(),
                    _mapper.Map<List<LetterCustomField>>(updated),
                    _mapper.Map<List<LetterCustomField>>(added),
                    _mapper.Map<List<LetterCustomField>>(deleted));

                var success = deleted.Count() + updated.Count() + added.Count() == 1 ?
                    _localizer[$"Custom Merged Field has been saved successfully."].ToString() :
                    _localizer[$"Custom Merged Fields have been saved successfully"].ToString();
                return Ok(new { success = success });

            }
            return Ok();
        }

        public async Task<IActionResult> GridDelete(string sys, [Bind(Prefix = "deleted")] LetterCustomFieldViewModel deleted)
        {
            var canUpdate = await LetterHelper.CanUpdateLetter(sys, User, _authService);
            Guard.Against.NoRecordPermission(canUpdate);

            if (!ModelState.IsValid)
                return new JsonBadRequest(new { errors = ModelState.Errors() });

            if (deleted.CFId > 0)
            {
                await _letterService.LetterCustomFieldUpdate(0, User.GetUserName(), "", 
                        new List<LetterCustomField>(), new List<LetterCustomField>(),
                        new List<LetterCustomField>() { _mapper.Map<LetterCustomField>(deleted) });
                return Ok(new { success = _localizer["Custom Merged Field has been deleted successfully."].ToString() });
            }

            return Ok();
        }

        public async Task<IActionResult> GetFilterFieldsList(int dataSourceId)
        {
            var picklist = await _letterService.GetDataSourceDateFieldList(dataSourceId);
            return Json(picklist);
        }
    }
}