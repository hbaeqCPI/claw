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
using R10.Core.Entities;
using R10.Core.Entities.GeneralMatter;
using R10.Core.Entities.Patent;
using R10.Core.Entities.ReportScheduler;
using R10.Core.Entities.Shared;
using R10.Core.Entities.Trademark;
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
    public class RSCriteriaController : BaseController
    {
        //private readonly IChildEntityService<RSMain, RSCriteria> _childService;
        private readonly IRSMainService _rSMainService;
        private readonly IRSCriteriaService _rSCriteriaService;
        private readonly IMapper _mapper;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly ISystemSettings<DefaultSetting> _defaultSettings;
        private readonly ISystemSettings<PatSetting> _PatentSettings;
        private readonly ISystemSettings<TmkSetting> _TrademarkSettings;
        private readonly ISystemSettings<GMSetting> _GMSettings;

        public RSCriteriaController(//IChildEntityService<RSMain, RSCriteria> childService,
            IRSMainService rSMainService,
            IRSCriteriaService rSCriteriaService,
            IMapper mapper,
            IStringLocalizer<SharedResource> localizer, 
            ISystemSettings<DefaultSetting> defaultSettings,
             ISystemSettings<PatSetting> PatentSettings,
             ISystemSettings<TmkSetting> TrademarkSettings,
             ISystemSettings<GMSetting> GMSettings)
        {
            //_childService = childService;
            _rSMainService = rSMainService;
            _rSCriteriaService = rSCriteriaService;
            _mapper = mapper;
            _localizer = localizer;
            _defaultSettings = defaultSettings;
            _PatentSettings = PatentSettings;
            _TrademarkSettings = TrademarkSettings;
            _GMSettings = GMSettings;
        }

        [Authorize(Policy = SharedAuthorizationPolicy.FullModify)]
        public async Task<IActionResult> GridUpdate(int parentId,
    [Bind(Prefix = "updated")]IEnumerable<RSCriteriaViewModel> updated,
    [Bind(Prefix = "new")]IEnumerable<RSCriteriaViewModel> added,
    [Bind(Prefix = "deleted")]IEnumerable<RSCriteriaViewModel> deleted)
        {
            if (updated.Any() || added.Any() || deleted.Any())
            {
                int reportId = _rSMainService.RSMains.FirstOrDefault(m => m.TaskId == parentId).ReportId;
                updated.Each(c => c.FieldName = _rSMainService.RSCriteriaControls.FirstOrDefault(a => a.ReportId == reportId && a.FieldAlias == c.FieldAlias).FieldName);
                added.Each(c => c.FieldName = _rSMainService.RSCriteriaControls.FirstOrDefault(a => a.ReportId == reportId && a.FieldAlias == c.FieldAlias).FieldName);
                deleted.Each(c => c.FieldName = _rSMainService.RSCriteriaControls.FirstOrDefault(a => a.ReportId == reportId && a.FieldAlias == c.FieldAlias).FieldName);
                updated.Each(c => c.FieldValue = _rSMainService.RSCriteriaControls.Any(a => a.ReportId == reportId && a.FieldAlias == c.FieldAlias && a.IsMultiple)? RemoveExtraSpaces(c.FieldValue??"") : c.FieldValue);
                added.Each(c => c.FieldValue = _rSMainService.RSCriteriaControls.Any(a => a.ReportId == reportId && a.FieldAlias == c.FieldAlias && a.IsMultiple) ? RemoveExtraSpaces(c.FieldValue ?? "") : c.FieldValue);

                //if (!ModelState.IsValid)
                //    return new JsonBadRequest(new { errors = ModelState.Errors() });
                //check included systems
                if (updated.Any())
                {
                    var printSystems = updated.FirstOrDefault(c => c.FieldName.IsCaseInsensitiveEqual("PrintSystems"));
                    if (printSystems != null)
                    {
                        await removeExtraSystems(printSystems);
                    }   
                }
                if (added.Any())
                {
                    var printSystems = added.FirstOrDefault(c => c.FieldName.IsCaseInsensitiveEqual("PrintSystems"));
                    if (printSystems != null)
                    {
                        await removeExtraSystems(printSystems);
                    }
                }

                await _rSCriteriaService.Update(parentId, User.GetUserName(),
                    _mapper.Map<List<RSCriteria>>(updated),
                    _mapper.Map<List<RSCriteria>>(added),
                    _mapper.Map<List<RSCriteria>>(deleted)
                    );
                var success = deleted.Count() + updated.Count() + added.Count() == 1 ?
                    _localizer["Criteria have been saved successfully."].ToString() :
                    _localizer["Criteria have been saved successfully"].ToString();
                return Ok(new { success = success });
            }
            return Ok();
        }

        private string RemoveExtraSpaces(string oldValue)
        {
            string result = oldValue;
            if (result.Contains(","))
            {
                var values = result.Split(",");
                for(int i = 0; i < values.Length;i++)
                {
                    values[i] = values[i].Trim();
                }
                result = String.Join(',', values);
            }
            if (result.Contains("|"))
            {
                var values = result.Split("|");
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = values[i].Trim();
                }
                result = String.Join('|', values);
            }
            return result;
        }

        private async Task removeExtraSystems(RSCriteriaViewModel printSystemField)
        {
            string[] printSystemsArray = printSystemField.FieldValue.Split(',');
            string includedSystems = (await _defaultSettings.GetSetting()).RSIncludedSystems;
            string[] includedSystemsArray = includedSystems.Split(',');
            List<string> inBoth = new List<string>();
            foreach (string si in includedSystemsArray)
            {
                foreach(string sp in printSystemsArray)
                {
                    if (si.Trim().IsCaseInsensitiveEqual(sp.Trim()))
                    {
                        inBoth.Add(si.Trim().ToUpper());
                    }
                }
            }
            printSystemField.FieldValue = String.Join(',',inBoth);
        }

        [Authorize(Policy = SharedAuthorizationPolicy.FullModify)]
        public async Task<IActionResult> GridDelete([Bind(Prefix = "deleted")] RSCriteriaViewModel deleted)
        {
            if (deleted.SchedCritId > 0)
            {
                await _rSCriteriaService.Update(deleted.parentId, User.GetUserName(), new List<RSCriteria>(), new List<RSCriteria>(), new List<RSCriteria>() { _mapper.Map<RSCriteria>(deleted) });
                return Ok(new { success = _localizer["Criteria has been deleted successfully."].ToString() });
            }
            return Ok();
        }

        public async Task<IActionResult> GridRead(
            [DataSourceRequest] DataSourceRequest request, int parentId
            )
        {
            var rSCriterias = _rSCriteriaService.GetRSCriterias(parentId);
            int reportId = _rSMainService.GetReportId(parentId);

            var result = rSCriterias.Select(c => new RSCriteriaViewModel
            {
                TaskId = c.TaskId,
                parentId = c.TaskId,
                SchedCritId = c.SchedCritId,
                FieldName = c.FieldName,
                FieldAlias = _rSMainService.RSCriteriaControls.FirstOrDefault(a => a.ReportId == reportId && a.FieldName == c.FieldName).FieldAlias,
                FieldValue = c.FieldValue,
                CreatedBy = c.CreatedBy,
                UpdatedBy = c.UpdatedBy,
                DateCreated = c.DateCreated,
                LastUpdate = c.LastUpdate,
                tStamp = c.tStamp,
                Condition = c.Condition,
                Special = c.Special,
                ParamOrder = c.ParamOrder,
            }).ToList();

            if (reportId == 2)//due date list
            {
                if (!User.IsInSystem(SystemType.Trademark))
                    result.RemoveAll(c => c.FieldName == "PrintGoods" || c.FieldName == "ClassesOp" || c.FieldName == "Classes" || c.FieldName == "Class");
                if (!(User.IsRespOfficeOn(SystemType.Patent) || User.IsRespOfficeOn(SystemType.Trademark) || User.IsRespOfficeOn(SystemType.GeneralMatter) || User.IsRespOfficeOn(SystemType.AMS)))
                    result.RemoveAll(c => c.FieldName == "RespOffice" || c.FieldName== "RespOffices");
                if (!(await _defaultSettings.GetSetting()).IsDeDocketOn)
                    result.RemoveAll(c => c.FieldName == "InstructedBy");
            }
            else if (reportId == 1)//patent list
            {
                if (!(await _PatentSettings.GetSetting()).IsProductsOn)
                    result.RemoveAll(c => c.FieldName == "Products" || c.FieldName == "Product");
                if (!(await _PatentSettings.GetSetting()).IsSubjectMattersOn)
                    result.RemoveAll(c => c.FieldName == "SubjectMatters" || c.FieldName == "SubjectMatter");
                if (!(User.IsRespOfficeOn(SystemType.Patent)))
                    result.RemoveAll(c => c.FieldName == "RespOffice" || c.FieldName == "RespOffices");
            }
            else if (reportId == 5)//trademark list
            {
                if (!(await _TrademarkSettings.GetSetting()).IsProductsOn)
                    result.RemoveAll(c => c.FieldName == "Products" || c.FieldName == "Product");
                if (!(User.IsRespOfficeOn(SystemType.Trademark)))
                    result.RemoveAll(c => c.FieldName == "RespOffice" || c.FieldName == "RespOffices");
            }
            else if (reportId == 6)//matter list
            {
                if (!(User.IsRespOfficeOn(SystemType.GeneralMatter)))
                    result.RemoveAll(c => c.FieldName == "RespOffice" || c.FieldName == "RespOffices");
            }

            return Json(result.ToDataSourceResult(request));
        }

        public async Task<IActionResult> GetFieldNameList(string property, string text, FilterType filterType, int taskId)
        {
            var criteriaList = _rSCriteriaService.RSCriterias.Where(c => c.TaskId == taskId);
            var controlList = _rSCriteriaService.RSCriteriaControls.Where(c => c.ReportId == _rSMainService.GetRSMainById(taskId).ReportId);
            foreach (RSCriteria rSCriteria in criteriaList)
            {
                controlList = controlList.Where(c => c.FieldName != rSCriteria.FieldName);
            }

            var list = controlList.OrderBy(c => c.FieldAlias).Select(c => new { FieldName = c.FieldName, FieldAlias = c.FieldAlias }).ToList();
            int reportId = _rSMainService.GetReportId(taskId);

            if (reportId == 2)//due date list
            {
                if (!User.IsInSystem(SystemType.Trademark))
                    list.RemoveAll(c => c.FieldName == "PrintGoods" || c.FieldName == "ClassesOp" || c.FieldName == "Classes" || c.FieldName == "Class");
                if (!(User.IsRespOfficeOn(SystemType.Patent) || User.IsRespOfficeOn(SystemType.Trademark) || User.IsRespOfficeOn(SystemType.GeneralMatter) || User.IsRespOfficeOn(SystemType.AMS)))
                    list.RemoveAll(c => c.FieldName == "RespOffice" || c.FieldName == "RespOffices");
                if (!(await _defaultSettings.GetSetting()).IsDeDocketOn)
                    list.RemoveAll(c => c.FieldName == "InstructedBy");
            }
            else if (reportId == 1)//patent list
            {
                if (!(await _PatentSettings.GetSetting()).IsProductsOn)
                    list.RemoveAll(c => c.FieldName == "Products" || c.FieldName == "Product");
                if (!(await _PatentSettings.GetSetting()).IsSubjectMattersOn)
                    list.RemoveAll(c => c.FieldName == "SubjectMatters" || c.FieldName == "SubjectMatter");
                if (!(User.IsRespOfficeOn(SystemType.Patent)))
                    list.RemoveAll(c => c.FieldName == "RespOffice" || c.FieldName == "RespOffices");
            }
            else if (reportId == 5)//trademark list
            {
                if (!(await _TrademarkSettings.GetSetting()).IsProductsOn)
                    list.RemoveAll(c => c.FieldName == "Products" || c.FieldName == "Product");
                if (!(User.IsRespOfficeOn(SystemType.Trademark)))
                    list.RemoveAll(c => c.FieldName == "RespOffice" || c.FieldName == "RespOffices");
            }
            else if (reportId == 6)//matter list
            {
                if (!(User.IsRespOfficeOn(SystemType.GeneralMatter)))
                    list.RemoveAll(c => c.FieldName == "RespOffice" || c.FieldName == "RespOffices");
            }

            return Json(list);
        }
    }
}