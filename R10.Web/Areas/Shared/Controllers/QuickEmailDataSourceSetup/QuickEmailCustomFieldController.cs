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
    [Area("Shared"), Authorize(Policy = SharedAuthorizationPolicy.CanAccessSystem)]
    public class QECustomFieldController : BaseController
    {
        private readonly IAuthorizationService _authService;
        private readonly IQuickEmailService _qeService;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly IMapper _mapper;

        public QECustomFieldController(
            IAuthorizationService authService,
            IQuickEmailService qeService,
            IStringLocalizer<SharedResource> localizer,
            IMapper mapper)
        {
            _authService = authService;
            _qeService = qeService;
            _localizer = localizer;
            _mapper = mapper;
        }

        public async Task<IActionResult> GridRead([DataSourceRequest] DataSourceRequest request, int parentId, string sys)
        {
            //var canAccess = await QEHelper.CanAccessQE(sys, User, _authService);
            //Guard.Against.NoRecordPermission(canAccess);

            var result = await _qeService.QECustomFields.Where(rs => rs.DataSourceID == parentId).ProjectTo<QECustomFieldViewModel>().ToListAsync();
            return Json(result.ToDataSourceResult(request));
        }

        public async Task<IActionResult> GridUpdate(
            int parentId, string sys,
            [Bind(Prefix = "updated")] IEnumerable<QECustomFieldViewModel> updated,
            [Bind(Prefix = "new")] IEnumerable<QECustomFieldViewModel> added,
            [Bind(Prefix = "deleted")] IEnumerable<QECustomFieldViewModel> deleted)
        {
            //var canUpdate = await QEHelper.CanUpdateQE(sys, User, _authService);
            //Guard.Against.NoRecordPermission(canUpdate);

            if (deleted.Any() || updated.Any() || added.Any())
            {
                if (!ModelState.IsValid)
                    return new JsonBadRequest(new { errors = ModelState.Errors() });

                if (updated.Any() || added.Any())
                {
                    var dataSourceFieldList = await _qeService.GetDataSourceFieldList(parentId);
                    var existingfieldList = dataSourceFieldList.Select(l => l.ColumnName.Trim('[', ']').ToLower()).ToList();

                    var newfieldList = added.Select(u => u.CustomFieldName.ToLower()).ToList();
                    newfieldList.AddRange(updated.Select(u => u.CustomFieldName.ToLower()).ToList());

                    if (existingfieldList.Intersect(newfieldList).Any())
                        return new JsonBadRequest(_localizer["You cannot create this Custom Merged Field, field name is already in the data source."].ToString());
                }

                await _qeService.QECustomFieldUpdate(parentId, User.GetUserName(), User.GetEmail(),
                    _mapper.Map<List<QECustomField>>(updated),
                    _mapper.Map<List<QECustomField>>(added),
                    _mapper.Map<List<QECustomField>>(deleted));

                var success = deleted.Count() + updated.Count() + added.Count() == 1 ?
                    _localizer[$"Custom Merged Field has been saved successfully."].ToString() :
                    _localizer[$"Custom Merged Fields have been saved successfully"].ToString();
                return Ok(new { success = success });

            }
            return Ok();
        }

        public async Task<IActionResult> GridDelete(string sys, [Bind(Prefix = "deleted")] QECustomFieldViewModel deleted)
        {
            //var canUpdate = await QEHelper.CanUpdateQE(sys, User, _authService);
            //Guard.Against.NoRecordPermission(canUpdate);

            if (!ModelState.IsValid)
                return new JsonBadRequest(new { errors = ModelState.Errors() });

            if (deleted.CFId > 0)
            {
                await _qeService.QECustomFieldUpdate(0, User.GetUserName(), "",
                        new List<QECustomField>(), new List<QECustomField>(),
                        new List<QECustomField>() { _mapper.Map<QECustomField>(deleted) });
                return Ok(new { success = _localizer["Custom Merged Field has been deleted successfully."].ToString() });
            }

            return Ok();
        }

        public async Task<IActionResult> GetFilterFieldsList(int dataSourceID)
        {
            var picklist = await _qeService.GetDataSourceDateFieldList(dataSourceID);
            return Json(picklist);
        }
    }
}