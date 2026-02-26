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
// using R10.Core.Entities.GeneralMatter; // Removed during deep clean
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
    public class RSPrintOptionController : BaseController
    {
        private readonly IRSMainService _rSMainService;
        private readonly IRSPrintOptionService _rSPrintOptionService;
        private readonly IMapper _mapper;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly ISystemSettings<DefaultSetting> _defaultSettings;
        private readonly ISystemSettings<PatSetting> _PatentSettings;
        private readonly ISystemSettings<TmkSetting> _TrademarkSettings;

        public RSPrintOptionController(IRSMainService rSMainService,
            IRSPrintOptionService rSPrintOptionService,
             IMapper mapper,
             IStringLocalizer<SharedResource> localizer,
             ISystemSettings<DefaultSetting> defaultSettings,
             ISystemSettings<PatSetting> PatentSettings,
             ISystemSettings<TmkSetting> TrademarkSettings)
        {
            _rSMainService = rSMainService;
            _rSPrintOptionService = rSPrintOptionService;
            _mapper = mapper;
            _localizer = localizer;
            _defaultSettings = defaultSettings;
            _PatentSettings = PatentSettings;
            _TrademarkSettings = TrademarkSettings;
        }

        [Authorize(Policy = SharedAuthorizationPolicy.FullModify)]
        public async Task<IActionResult> GridUpdate(int parentId,
[Bind(Prefix = "updated")]IEnumerable<RSPrintOptionViewModel> updated,
[Bind(Prefix = "new")]IEnumerable<RSPrintOptionViewModel> added,
[Bind(Prefix = "deleted")]IEnumerable<RSPrintOptionViewModel> deleted)
        {
            if (updated.Any() || added.Any() || deleted.Any())
            {
                if (!ModelState.IsValid)
                    return new JsonBadRequest(new { errors = ModelState.Errors() });

                await _rSPrintOptionService.Update(parentId, User.GetUserName(),
                    _mapper.Map<List<RSPrintOption>>(updated),
                    _mapper.Map<List<RSPrintOption>>(added),
                    _mapper.Map<List<RSPrintOption>>(deleted)
                    );
                var success = deleted.Count() + updated.Count() + added.Count() == 1 ?
                    _localizer["Print Option has been saved successfully."].ToString() :
                    _localizer["Print Option have been saved successfully"].ToString();
                return Ok(new { success = success });
            }
            return Ok();
        }

        public async Task<IActionResult> GridRead(
            [DataSourceRequest] DataSourceRequest request, int parentId
            )
        {
            var rSPrintOptions = _rSPrintOptionService.GetRSPrintOptions(parentId);
            int reportId = _rSMainService.GetReportId(parentId);

            var result = rSPrintOptions.Select(c => new
            RSPrintOptionViewModel
            {
                TaskId = c.TaskId,
                parentId = c.TaskId,
                SchedParamId = c.SchedParamId,
                OptionName = c.OptionName,
                OptionAlias = _rSMainService.RSPrintOptionControls.FirstOrDefault(a=>a.ReportId==reportId&&a.OptionName==c.OptionName).OptionAlias,
                OptionValue = c.OptionValue,
                CreatedBy = c.CreatedBy,
                UpdatedBy = c.UpdatedBy,
                DateCreated = c.DateCreated,
                LastUpdate = c.LastUpdate,
                tStamp = c.tStamp,
            }).OrderBy(c => c.OptionAlias).ToList();

            if (reportId == 2)//due date list
            {
                if (!(await _defaultSettings.GetSetting()).IsDeDocketOn)
                    result.RemoveAll(c => c.OptionName == "DeDocketInstructionOnly");
                if (!User.IsInSystem(SystemType.Patent))
                    result.RemoveAll(c => c.OptionName == "PrintInventors");
            }
            else if (reportId == 1)//patent list
            {
                if(!User.IsInSystem(SystemType.GeneralMatter))
                    result.RemoveAll(c => c.OptionName == "PrintRelatedMatter");
                if (!(await _PatentSettings.GetSetting()).IsProductsOn)
                    result.RemoveAll(c => c.OptionName == "PrintProducts");
                if (!(await _PatentSettings.GetSetting()).IsSubjectMattersOn)
                    result.RemoveAll(c => c.OptionName == "PrintSubjectMatters");
                if (!(await _PatentSettings.GetSetting()).IsCorporation || !(await _PatentSettings.GetSetting()).IsPatentScoreOn)
                    result.RemoveAll(c => c.OptionName == "PrintPatentScore");
            }
            else if (reportId == 5)//trademark list
            {
                if (!User.IsInSystem(SystemType.GeneralMatter))
                    result.RemoveAll(c => c.OptionName == "PrintRelatedMatter");
                if (!(await _TrademarkSettings.GetSetting()).IsProductsOn)
                    result.RemoveAll(c => c.OptionName == "PrintProducts");
            }

            return Json(result.ToDataSourceResult(request));
        }
    }
}
