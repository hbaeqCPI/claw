using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using R10.Core.Entities.ReportScheduler;
using R10.Core.Helpers;
using R10.Core.Interfaces;
using R10.Web.Areas.Shared.ViewModels.ReportScheduler;
using R10.Web.Extensions;
using R10.Web.Extensions.ActionResults;
using R10.Web.Models;
using R10.Web.Security;

using R10.Web.Areas;

namespace R10.Web.Areas.Shared.Controllers.ReportScheduler
{
    [Area("Shared"), Authorize(Policy = SharedAuthorizationPolicy.CanAccessSystem)]
    public class RSActionController : BaseController
    {
        //private readonly IChildEntityService<RSMain, RSAction> _childService;
        private readonly IRSActionService _rSActionService;
        private readonly IRSMainService _rSMainService;
        private readonly IMapper _mapper;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public RSActionController(//IChildEntityService<RSMain, RSAction> childService,
            IRSActionService rSActionService
            , IRSMainService rSMainService,
                                    IMapper mapper,
                                    IStringLocalizer<SharedResource> localizer)
        {
            //_childService = childService;
            _rSMainService = rSMainService;
            _rSActionService = rSActionService;
            _mapper = mapper;
            _localizer = localizer;
        }

        [Authorize(Policy = PatentAuthorizationPolicy.FullModify)]
        public async Task<IActionResult> GridUpdate(int parentId,
            [Bind(Prefix = "updated")]IEnumerable<RSActionViewModel> updated,
            [Bind(Prefix = "new")]IEnumerable<RSActionViewModel> added,
            [Bind(Prefix = "deleted")]IEnumerable<RSActionViewModel> deleted)
        {
            if (updated.Any() || added.Any() || deleted.Any())
            {
                if (!ModelState.IsValid)
                    return new JsonBadRequest(new { errors = ModelState.Errors() });

                await _rSActionService.Update(parentId, User.GetUserName(),
                    _mapper.Map<List<RSAction>>(updated),
                    _mapper.Map<List<RSAction>>(added),
                    _mapper.Map<List<RSAction>>(deleted)
                    );
                var success = deleted.Count() + updated.Count() + added.Count() == 1 ?
                    _localizer["Keyword has been saved successfully."].ToString() :
                    _localizer["Keywords have been saved successfully"].ToString();
                return Ok(new { success = success });
            }
            return Ok();
        }

        [Authorize(Policy = PatentAuthorizationPolicy.FullModify)]
        public async Task<IActionResult> GridDelete([Bind(Prefix = "deleted")] RSActionViewModel deleted)
        {
            if (deleted.ActionId > 0)
            {
                await _rSActionService.Update(deleted.ParentId, User.GetUserName(), new List<RSAction>(), new List<RSAction>(), new List<RSAction>() { _mapper.Map<RSAction>(deleted) });
                return Ok(new { success = _localizer["Keyword has been deleted successfully."].ToString() });
            }
            return Ok();
        }

        public IActionResult ActionAdd(int taskId)
        {
            int reportId = _rSMainService.RSMains.FirstOrDefault(c=>c.TaskId==taskId).ReportId;
            string defaultOrderBy = _rSActionService.RSOrderByControls.FirstOrDefault(c => c.ReportId == reportId && c.OrderBy == 1).OrderByName;
            ViewBag.ReportId = reportId;
            return PartialView("_ActionEntry", new RSAction { TaskId = taskId, ActionTypeId=1,IsEnabled=true, OutputFormat="PDF", SortOrder= defaultOrderBy });
        }

        public IActionResult ActionEdit(int ActionId)
        {
            var action = _rSActionService.GetRSActionById(ActionId);
            int reportId = _rSMainService.RSMains.FirstOrDefault(c => c.TaskId == action.TaskId).ReportId;
            ViewBag.ReportId = reportId;
            return PartialView("_ActionEntry", action);
        }

        [Authorize(Policy = SharedAuthorizationPolicy.FullModify)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActionSave([FromBody]RSAction rSAction)
        {
            if (ModelState.IsValid)
            {
                UpdateEntityStamps(rSAction, rSAction.ActionId);
                await _rSActionService.ActionUpdate(rSAction);
                return Ok();
            }
            return BadRequest(ModelState);
        }


        #region Action
        public IActionResult GridRead(
            [DataSourceRequest] DataSourceRequest request, int parentId
            )
        {
            var rSActions = _rSActionService.GetRSActions(parentId);
            var rSActionTypes = _rSActionService.RSActionTypes.ToList();
            var rSOrderByControls = _rSActionService.RSOrderByControls.ToList();

            var result =rSActions.Select(c => new
            {
                TaskId = c.TaskId,
                parentId = c.TaskId,
                ActionId = c.ActionId,
                ActionTypeId = c.ActionTypeId,
                IsEnabled = c.IsEnabled,
                OutputFormat = c.OutputFormat,
                SortOrder = c.SortOrder,
                EmailTo = c.EmailTo,
                EmailCopyTo = c.EmailCopyTo,
                EmailSubject = c.EmailSubject,
                EmailBody = c.EmailBody,
                FTPAddress = c.FTPAddress,
                FTPUserID = c.FTPUserID,
                FTPPassword = c.FTPPassword,
                CreatedBy = c.CreatedBy,
                UpdatedBy = c.UpdatedBy,
                DateCreated = c.DateCreated,
                LastUpdate = c.LastUpdate,
                tStamp=c.tStamp,
                rSActionType = _rSActionService.RSActionTypes.FirstOrDefault(a=>a.ActionTypeId== c.ActionTypeId),
                Status = c.IsEnabled?"Enabled":"Disabled"
            }).ToList();
            return Json(result.ToDataSourceResult(request));
        }

        public IActionResult GetActionTypeList(string property, string text, FilterType filterType)
        {
            var list = _rSActionService.RSActionTypes.Select(a => new { Name = a.Name, ActionTypeId = a.ActionTypeId }).OrderBy(a => a.Name).ToList();
            return Json(list);
        }

        public string GetActionTypeName(int actionTypeId, List<RSActionType> rSActionTypes)
        {
            return rSActionTypes.FirstOrDefault(c => c.ActionTypeId == actionTypeId).Name;
        }

        public IActionResult GetFileFormatList(string property, string text, FilterType filterType)
        {
            var list = Enum.GetValues(typeof(FileFormat)).Cast<FileFormat>().Select(a => new { FileFormat = a.ToString() }).ToList();

            return Json(list);
        }

        public IActionResult GetEnabledOptionList(string property, string text, FilterType filterType)
        {
            var list = Enum.GetValues(typeof(EnabledOption)).Cast<EnabledOption>().Select(a => new { Option = a.ToString() }).ToList();

            return Json(list);
        }

        public IActionResult GetSortOrderList(string property, string text, FilterType filterType, int taskId=0)
        {
            int reportId;
            if (taskId != 0)
                reportId = _rSMainService.RSMains.FirstOrDefault(c => c.TaskId == taskId).ReportId;
            else
                reportId = 2;
            var list = _rSActionService.RSOrderByControls.Where(c => c.ReportId == reportId).Select(a => new { SortOrder = a.OrderByName }).OrderBy(a => a.SortOrder).ToList();
            return Json(list);
        }

        private enum FileFormat
        {
            PDF,
            Word,
            Excel,
        }

        private enum EnabledOption
        {
            Enabled,
            Disabled
        }

        //public IActionResult ActionGridUpdate(int parentId,
        //    [Bind(Prefix = "updated")]IEnumerable<RSActionViewModel> updated,
        //    [Bind(Prefix = "new")]IEnumerable<RSActionViewModel> added,
        //    [Bind(Prefix = "deleted")]IEnumerable<RSActionViewModel> deleted)
        //{
        //    if (updated.Any() || added.Any() || deleted.Any())
        //    {
        //        if (!ModelState.IsValid)
        //            return new JsonBadRequest(new { errors = ModelState.Errors() });

        //        foreach (RSActionViewModel updatedAction in updated)
        //        {
        //            _rSActionService.UpdateRSAction(ConvertActionViewModelToAction(updatedAction));
        //        }
        //        foreach (RSActionViewModel addedAction in added)
        //        {
        //            _rSActionService.AddRSAction(ConvertActionViewModelToAction(addedAction));
        //        }
        //        foreach (RSActionViewModel deletedAction in deleted)
        //        {
        //            _rSActionService.DeleteRSActionById(deletedAction.ActionId);
        //        }

        //        var success = deleted.Count() + updated.Count() + added.Count() == 1 ?
        //            _localizer["Action has been saved successfully."].ToString() :
        //            _localizer["Actions have been saved successfully"].ToString();
        //        return Ok(new { success = success });
        //    }
        //    return Ok();
        //}

        //public RSAction ConvertActionViewModelToAction(RSActionViewModel RSActionViewModel)
        //{
        //    RSAction rSAction = RSActionViewModel.ActionId == 0 ? new RSAction() : _rSActionService.GetRSActionById(RSActionViewModel.ActionId);

        //    rSAction.ActionTypeId = _rSActionService.RSActionTypes.FirstOrDefault(c => c.Name == RSActionViewModel.Action).ActionTypeId;
        //    rSAction.IsEnabled = RSActionViewModel.Status.IsCaseInsensitiveEqual("Enabled");
        //    rSAction.OutputFormat = RSActionViewModel.FileFormat;
        //    rSAction.SortOrder = RSActionViewModel.SortOrder;
        //    rSAction.EmailTo = RSActionViewModel.To;
        //    rSAction.EmailCopyTo = RSActionViewModel.CC;
        //    rSAction.EmailSubject = RSActionViewModel.Subject;
        //    rSAction.EmailBody = RSActionViewModel.Body;
        //    rSAction.FTPAddress = RSActionViewModel.Host;
        //    rSAction.FTPUserID = RSActionViewModel.User;
        //    rSAction.FTPPassword = RSActionViewModel.Password;
        //    rSAction.UpdatedBy = User.GetUserName();
        //    rSAction.LastUpdate = DateTime.Now;
        //    if (RSActionViewModel.ActionId == 0)
        //    {
        //        rSAction.CreatedBy = User.GetUserName();
        //        rSAction.DateCreated = DateTime.Now;
        //    }

        //    return rSAction;
        //}


        #endregion Action
    }
}