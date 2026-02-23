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

namespace R10.Web.Areas.Shared.Controllers.DOCX
{
    [Area("Shared"), Authorize(Policy = SharedAuthorizationPolicy.CanAccessLetters)]
    public class DOCXDataSourceController : Microsoft.AspNetCore.Mvc.Controller
    {
        private readonly IDOCXService _docxService;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly ISystemSettings<PatSetting> _patSettings;

        public DOCXDataSourceController(
            IDOCXService docxService,
            IStringLocalizer<SharedResource> localizer,
            ISystemSettings<PatSetting> patSettings
            )
        {
            _docxService = docxService;
            _localizer = localizer;
            _patSettings = patSettings;
        }

        public async Task<IActionResult> GridRead([DataSourceRequest] DataSourceRequest request, string systemType)
        {
            var result = await _docxService.DOCXDataSources.Where(ds => ds.SystemType == systemType).ProjectTo<DOCXDataSourceViewModel>().ToListAsync();
            if (systemType == "P")
            {
                if (!User.IsInSystem(SystemType.IDS))
                {
                    result.RemoveAll(c => c.DataSourceDescMain.StartsWith("IDS"));
                }

            }
            return Json(result.ToDataSourceResult(request));
        }


        public async Task<Microsoft.AspNetCore.Mvc.ActionResult> GetFamilyTree(int docxId, int? id)
        {
            var subTree = new List<FamilyTreeDTO>();
            if (docxId <= 0 )        // add mode or no record mode
                subTree.Add(new FamilyTreeDTO { id = "0", hasChildren = false, text = "", expanded = false });
            else
                subTree = await _docxService.GetFamilyTree(docxId, id);
            
            return Json(subTree);
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRecordSource(int docxId, int dataSourceId, int parentRecSourceId)
        {
            var newRecord = new List<DOCXRecordSource>();
            newRecord.Add(new DOCXRecordSource() { DOCXId = docxId, DataSourceId = dataSourceId, ParentRecSourceId = parentRecSourceId });

            await _docxService.RecordSourceUpdate(docxId, User.GetUserName(), new List<DOCXRecordSource>(), newRecord, new List<DOCXRecordSource>());

            return Ok(new { success = _localizer["DOCX Data Source has been saved successfully."].ToString() });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRecordSource(int docxId, int dataSourceId, int parentRecSourceId)
        {
            var updatedRecord = await _docxService.GetRecordSourceById(dataSourceId, docxId);

            if (updatedRecord is null)
            {
                updatedRecord = await _docxService.GetRecordSourceById(dataSourceId);
            }

            // validate parentRecSourceId, sometimes GUI passes node dataSourceId  
            if (parentRecSourceId > 0) 
            {
                var validParentRecord = await _docxService.ValidParentRecord(parentRecSourceId, docxId);

                if (!validParentRecord)
                {
                    var parentRecSource = await _docxService.GetRecordSourceById(parentRecSourceId, docxId);
                    parentRecSourceId = parentRecSource.RecSourceId;
                }
            }            

            updatedRecord.ParentRecSourceId = parentRecSourceId;
            //updatedRecord.Add(new DOCXRecordSource() { DOCXId = docxId, DataSourceId = dataSourceId, ParentRecSourceId = parentRecSourceId });

            await _docxService.RecordSourceUpdate(docxId, User.GetUserName() , new List<DOCXRecordSource>() { updatedRecord }, 
                                                            new List<DOCXRecordSource>(), new List<DOCXRecordSource>());

            return Ok(new { success = _localizer["DOCX Data Source has been saved successfully."].ToString() });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRecordSource(int docxId, int recSourceId)
        {

            var deletedRecord = new List<DOCXRecordSource>();
            deletedRecord.Add(await _docxService.GetRecordSourceById(recSourceId));

            await _docxService.RecordSourceUpdate(docxId, User.GetUserName(), new List<DOCXRecordSource>(), new List<DOCXRecordSource>(), deletedRecord);

            return Ok(new { success = _localizer["DOCX Data Source has been deleted successfully."].ToString() });

        }
    }
}
