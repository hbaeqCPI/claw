using R10.Web.Helpers;
using AutoMapper.QueryableExtensions;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Helpers;
using R10.Core.Interfaces;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Models;
using R10.Web.Security;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using R10.Core.Entities.Patent;

namespace R10.Web.Areas.Shared.Controllers.Letter
{
    [Area("Shared"), Authorize(Policy = SharedAuthorizationPolicy.CanAccessLetters)]
    public class LetterDataSourceController : Microsoft.AspNetCore.Mvc.Controller
    {
        private readonly ILetterService _letterService;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly ISystemSettings<PatSetting> _patSettings;

        public LetterDataSourceController(
            ILetterService letterService,
            IStringLocalizer<SharedResource> localizer,
            ISystemSettings<PatSetting> patSettings
            )
        {
            _letterService = letterService;
            _localizer = localizer;
            _patSettings = patSettings;
        }

        public async Task<IActionResult> GridRead([DataSourceRequest] DataSourceRequest request, string systemType)
        {
            var result = await _letterService.FilteredLetterDataSources.Where(ds => ds.SystemType == systemType).ProjectTo<LetterDataSourceViewModel>().ToListAsync();

            return Json(result.ToDataSourceResult(request));
        }


        public async Task<Microsoft.AspNetCore.Mvc.ActionResult> GetFamilyTree(int letId, int? id)
        {
            var subTree = new List<FamilyTreeDTO>();
            if (letId <= 0 )        // add mode or no record mode
                subTree.Add(new FamilyTreeDTO { id = "0", hasChildren = false, text = "", expanded = false });
            else
                subTree = await _letterService.GetFamilyTree(letId, id);
            
            return Json(subTree);
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRecordSource(int letId, int dataSourceId, int parentRecSourceId)
        {
            var newRecord = new List<LetterRecordSource>();
            newRecord.Add(new LetterRecordSource() { LetId = letId, DataSourceId = dataSourceId, ParentRecSourceId = parentRecSourceId });

            await _letterService.RecordSourceUpdate(letId, User.GetUserName(), new List<LetterRecordSource>(), newRecord, new List<LetterRecordSource>());

            return Ok(new { success = _localizer["Letter Data Source has been saved successfully."].ToString() });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRecordSource(int letId, int dataSourceId, int parentRecSourceId)
        {
            //var updatedRecord = await _letterService.GetRecordSourceById(dataSourceId);
            //updatedRecord.ParentRecSourceId = parentRecSourceId;
            //updatedRecord.Add(new LetterRecordSource() { LetId = letId, DataSourceId = dataSourceId, ParentRecSourceId = parentRecSourceId });

            //fix for missplacing child node
            var updatedRecord = await _letterService.GetRecordSourceById(dataSourceId, letId);

            if (updatedRecord is null)
            {
                updatedRecord = await _letterService.GetRecordSourceById(dataSourceId);
            }

            // validate parentRecSourceId, sometimes GUI passes node dataSourceId  
            if (parentRecSourceId > 0)
            {
                var validParentRecord = await _letterService.ValidParentRecord(parentRecSourceId, letId);

                if (!validParentRecord)
                {
                    var parentRecSource = await _letterService.GetRecordSourceById(parentRecSourceId, letId);
                    parentRecSourceId = parentRecSource.RecSourceId;
                }
            }


            await _letterService.RecordSourceUpdate(letId, User.GetUserName() , new List<LetterRecordSource>() { updatedRecord }, 
                                                            new List<LetterRecordSource>(), new List<LetterRecordSource>());

            return Ok(new { success = _localizer["Letter Data Source has been saved successfully."].ToString() });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRecordSource(int letId, int recSourceId)
        {

            var deletedRecord = new List<LetterRecordSource>();
            deletedRecord.Add(await _letterService.GetRecordSourceById(recSourceId));

            await _letterService.RecordSourceUpdate(letId, User.GetUserName(), new List<LetterRecordSource>(), new List<LetterRecordSource>(), deletedRecord);

            return Ok(new { success = _localizer["Letter Data Source has been deleted successfully."].ToString() });

        }
    }
}
