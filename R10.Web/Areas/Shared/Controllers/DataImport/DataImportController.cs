using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using R10.Core.Interfaces;
using R10.Web.Security;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System;
using Microsoft.Extensions.Localization;
using R10.Web.Models;
using ClosedXML.Excel;
using Microsoft.Extensions.Logging;
using R10.Core.Entities;
using R10.Core.Helpers;
using Microsoft.AspNetCore.Hosting;
using System.Collections.Generic;
using R10.Web.Extensions.ActionResults;
using R10.Web.Extensions;
using R10.Web.Areas.Shared.ViewModels;
using System.Data;
using System.Text.RegularExpressions;
using R10.Web.Helpers;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Localization;
using R10.Core.Entities.Patent;
using R10.Core.Entities.Shared;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;
using ClosedXML.Excel.Drawings;
using R10.Web.Services.DocumentStorage;
using R10.Core.Entities.Documents;
using R10.Web.Interfaces;
using R10.Web.Services;
using Microsoft.Extensions.Options;
using R10.Web.Filters;
using Microsoft.Graph;
using R10.Web.Services.SharePoint;
using R10.Core.Services.Shared;
using R10.Web.Areas.Shared.ViewModels.SharePoint;
using R10.Core.Identity;

using R10.Web.Areas;

namespace R10.Web.Areas.Shared.Controllers
{
    //[Area("Shared"), Authorize(Policy = SharedAuthorizationPolicy.CanAccessPortfolioOnboarding)]
    [Area("Shared"), Authorize()]
    public class DataImportController : BaseController
    {
        private readonly IAuthorizationService _authService;
        private readonly IDataImportService _importService;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly IStringLocalizer<DataImportTypeViewModel> _exportLocalizer;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly ILogger _logger;
        private readonly ExportHelper _exportHelper;
        private readonly ISystemSettings<PatSetting> _patSettings;
        private readonly ISystemSettings<DefaultSetting> _settings;

        private readonly ITmkTrademarkService _trademarkService;
        private readonly IDocumentImportService _docImportService;

        private readonly IDocumentStorage _documentStorage;
        private readonly IDocumentService _docService;
        private readonly IDocumentsViewModelService _docViewModelService;
        private readonly ISharePointService _sharePointService;
        private readonly ISharePointViewModelService _sharePointViewModelService;
        private readonly GraphSettings _graphSettings;

        private DataImportHistory _importJob;

        private const int MaxRecordsAllowed = 30000;
        private const string ImportFolder = @"UserFiles\DataImport";

        public DataImportController(IDataImportService importService, IAuthorizationService authService,
                                    IStringLocalizer<SharedResource> localizer, ILogger<DataImportController> logger,
                                    IHostingEnvironment hostingEnvironment, IStringLocalizer<DataImportTypeViewModel> exportLocalizer,
                                    ISystemSettings<PatSetting> patSettings, ExportHelper exportHelper,
                                    ISystemSettings<DefaultSetting> settings,
                                    ITmkTrademarkService trademarkService,
                                    IDocumentImportService docImportService,
                                    IDocumentStorage documentStorage,
                                    IDocumentService docService,
                                    IDocumentsViewModelService docViewModelService,
                                    ISharePointService sharePointService,
                                    ISharePointViewModelService sharePointViewModelService,
                                    IOptions<GraphSettings> graphSettings)
        {
            _importService = importService;
            _authService = authService;
            _localizer = localizer;
            _exportLocalizer = exportLocalizer;
            _logger = logger;
            _hostingEnvironment = hostingEnvironment;
            _exportHelper = exportHelper;
            _patSettings = patSettings;
            _settings = settings;

            _trademarkService = trademarkService;
            _docImportService = docImportService;

            _documentStorage = documentStorage;
            _docService = docService;
            _docViewModelService = docViewModelService;
            _sharePointService = sharePointService;
            _sharePointViewModelService = sharePointViewModelService;
            _graphSettings = graphSettings.Value;
        }

        [Authorize(Policy = PatentAuthorizationPolicy.CanAccessPortfolioOnboarding)]
        public async Task<IActionResult> Patent(bool isUpdate = false)
        {
            return await Index(SystemTypeCode.Patent, isUpdate);
        }

        [Authorize(Policy = TrademarkAuthorizationPolicy.CanAccessPortfolioOnboarding)]
        public async Task<IActionResult> Trademark(bool isUpdate = false)
        {
            return await Index(SystemTypeCode.Trademark, isUpdate);
        }

        [Authorize(Policy = GeneralMatterAuthorizationPolicy.CanAccessPortfolioOnboarding)]
        public async Task<IActionResult> GeneralMatter(bool isUpdate = false)
        {
            return await Index(SystemTypeCode.GeneralMatter, isUpdate);
        }


        [Authorize(Policy = IDSAuthorizationPolicy.CanAccessIDSImport)]
        public async Task<IActionResult> IDS()
        {
            return await Index(SystemTypeCode.IDS);
        }

        [Authorize(Policy = AMSAuthorizationPolicy.FullModify)]
        public async Task<IActionResult> AMS()
        {
            return await Index(SystemTypeCode.AMS);
        }

        private async Task<IActionResult> Index(string systemType = SystemTypeCode.Patent, bool isUpdate = false)
        {
            var authorized = false;
            ViewBag.Title = _localizer["Portfolio Onboarding"];

            if (systemType == SystemTypeCode.Trademark)
                authorized = (await _authService.AuthorizeAsync(User, TrademarkAuthorizationPolicy.CanAccessPortfolioOnboarding)).Succeeded;
            else if (systemType == SystemTypeCode.GeneralMatter)
                authorized = (await _authService.AuthorizeAsync(User, GeneralMatterAuthorizationPolicy.CanAccessPortfolioOnboarding)).Succeeded;
            else if (systemType == SystemTypeCode.IDS)
            {
                authorized = (await _authService.AuthorizeAsync(User, IDSAuthorizationPolicy.CanAccessIDSImport)).Succeeded;
                ViewBag.Title = _localizer["IDS Import"];
            }
            else if (systemType == SystemTypeCode.AMS)
                authorized = (await _authService.AuthorizeAsync(User, AMSAuthorizationPolicy.FullModify)).Succeeded;
            else
                authorized = (await _authService.AuthorizeAsync(User, PatentAuthorizationPolicy.CanAccessPortfolioOnboarding)).Succeeded;

            if (!authorized)
                return Forbid();

            ViewBag.ActiveTab = "ptoImport-tab";
            if (isUpdate)
                ViewBag.ActiveTab = "ptoUpdate-tab";

            ViewBag.CanAddRecord = await CanAddRecord(systemType);
            ViewBag.CanDeleteRecord = await CanDeleteRecord(systemType);
            ViewBag.SystemType = systemType;

            // Check Entity Filter and remove
            var dataImportTypes = await _importService.GetDataImportTypes(systemType);
            var dataUpdateTypes = await _importService.GetDataImportTypes(systemType, true);

            var userEntityFilterType = User.GetEntityFilterType();
            if (userEntityFilterType != CPiEntityType.None)
            {
                dataImportTypes.RemoveAll(d => !string.IsNullOrEmpty(d.DataGroup) && d.DataGroup.Equals("auxiliary", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(d.DataType) && d.DataType.Equals(userEntityFilterType.ToString(), StringComparison.OrdinalIgnoreCase));
                dataUpdateTypes.RemoveAll(d => !string.IsNullOrEmpty(d.DataGroup) && d.DataGroup.Equals("auxiliary", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(d.DataType) && d.DataType.Equals(userEntityFilterType.ToString(), StringComparison.OrdinalIgnoreCase));
            }

            ViewBag.DataImportTypes = dataImportTypes;
            ViewBag.DataUpdateTypes = dataUpdateTypes;


            return View("Index");
        }

        public async Task<IActionResult> ImportHistoryRead([DataSourceRequest] DataSourceRequest request, string systemType, bool isUpdate = false)
        {
            var result = await _importService.DataImportsHistory
                                .Where(h => h.SystemType == systemType)
                                .Include(h => h.DataType)
                                .Where(h => (isUpdate && h.DataType.IsUpdate) || (!isUpdate && !h.DataType.IsUpdate))
                                .ToDataSourceResultAsync(request);
            var data = (List<DataImportHistory>)result.Data;
            data.ForEach(h => h.TranslatedStatus = _localizer[h.Status]);
            return Json(result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UploadFile(DataImportViewModel import)
        {

            var authorized = false;
            if (import.SystemType == SystemTypeCode.Trademark)
                authorized = (await _authService.AuthorizeAsync(User, TrademarkAuthorizationPolicy.FullModify)).Succeeded;
            else if (import.SystemType == SystemTypeCode.GeneralMatter)
                authorized = (await _authService.AuthorizeAsync(User, GeneralMatterAuthorizationPolicy.FullModify)).Succeeded;
            else if (import.SystemType == SystemTypeCode.IDS)
                authorized = (await _authService.AuthorizeAsync(User, IDSAuthorizationPolicy.FullModify)).Succeeded;
            else if (import.SystemType == SystemTypeCode.AMS)
                authorized = (await _authService.AuthorizeAsync(User, AMSAuthorizationPolicy.InternalFullModify)).Succeeded;
            else
                authorized = (await _authService.AuthorizeAsync(User, PatentAuthorizationPolicy.FullModify)).Succeeded;

            if (!authorized)
                return Forbid();

            var form = Request.Form;
            if (form.Files.Count > 0)
            {
                if (import.DataType > 0)
                {

                    var formFile = form.Files[0];
                    var stream = new MemoryStream();

                    await formFile.CopyToAsync(stream);
                    try
                    {
                        using (var workBook = new XLWorkbook(stream))
                        {
                            var workSheet = workBook.Worksheet(1);

                            //var rowsCount = workSheet.Rows().Count();
                            var rowsCount = 0;
                            foreach (IXLRow row in workSheet.Rows())
                            {
                                if (!row.IsEmpty())
                                    rowsCount++;
                            }

                            if (rowsCount - 1 > MaxRecordsAllowed)
                                return BadRequest(_localizer["The file imported has exceeded the maximum number of records allowed."].ToString());

                            var fields = GetColumnNames(workSheet);
                            if (fields.Count == 0)
                                return BadRequest(_localizer["Please make sure that the first row of the file contains the column names."].ToString());

                            if (rowsCount - 1 == 0)
                                return BadRequest(_localizer["The file imported has no data row, only column headers."].ToString());

                            await InitializeImportJob(import.ImportId, import.DataType, import.SystemType, formFile.FileName, rowsCount - 1);
                            await _importService.AddImportColumnNames(_importJob.ImportId, fields);
                        }

                        var filePath = GetNewFileName(formFile.FileName, _importJob.ImportId);
                        if (System.IO.File.Exists(filePath))
                        {
                            System.IO.File.Delete(filePath);
                        }

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            formFile.CopyTo(fileStream);
                        }
                        var fileName = Path.GetFileName(filePath);
                        await FlagForMappingReview(fileName);
                        return Ok(_importJob.ImportId);
                    }

                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "XLWorkbook load error");
                        return BadRequest(_localizer["An error was encountered, please check if the file has a valid format."].ToString());
                    }
                }
            }
            return Ok(import.ImportId);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UploadUpdateFile(DataUpdateViewModel import)
        {
            var authorized = false;
            if (import.SystemType == SystemTypeCode.Trademark)
                authorized = (await _authService.AuthorizeAsync(User, TrademarkAuthorizationPolicy.FullModify)).Succeeded;
            else if (import.SystemType == SystemTypeCode.GeneralMatter)
                authorized = (await _authService.AuthorizeAsync(User, GeneralMatterAuthorizationPolicy.FullModify)).Succeeded;
            else if (import.SystemType == SystemTypeCode.IDS)
                authorized = (await _authService.AuthorizeAsync(User, IDSAuthorizationPolicy.FullModify)).Succeeded;
            else
                authorized = (await _authService.AuthorizeAsync(User, PatentAuthorizationPolicy.FullModify)).Succeeded;

            if (!authorized)
                return Forbid();

            var form = Request.Form;
            if (form.Files.Count > 0)
            {
                if (import.DataType > 0)
                {

                    var formFile = form.Files[0];
                    var stream = new MemoryStream();

                    await formFile.CopyToAsync(stream);
                    try
                    {
                        using (var workBook = new XLWorkbook(stream))
                        {
                            var workSheet = workBook.Worksheet(1);

                            //var rowsCount = workSheet.Rows().Count();
                            var rowsCount = 0;
                            foreach (IXLRow row in workSheet.Rows())
                            {
                                if (!row.IsEmpty())
                                    rowsCount++;
                            }

                            if (rowsCount - 1 > MaxRecordsAllowed)
                                return BadRequest(_localizer["The file uploaded has exceeded the maximum number of records allowed."].ToString());

                            var fields = GetColumnNames(workSheet);
                            if (fields.Count == 0)
                                return BadRequest(_localizer["Please make sure that the first row of the file contains the column names."].ToString());

                            if (rowsCount - 1 == 0)
                                return BadRequest(_localizer["The file uploaded has no data row, only column headers."].ToString());

                            await InitializeImportJob(import.UpdateId, import.DataType, import.SystemType, formFile.FileName, rowsCount - 1);
                            await _importService.AddImportColumnNames(_importJob.ImportId, fields);
                        }

                        var filePath = GetNewFileName(formFile.FileName, _importJob.ImportId);
                        if (System.IO.File.Exists(filePath))
                        {
                            System.IO.File.Delete(filePath);
                        }

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            formFile.CopyTo(fileStream);
                        }
                        var fileName = Path.GetFileName(filePath);
                        await FlagForMappingReview(fileName);
                        return Ok(_importJob.ImportId);
                    }

                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "XLWorkbook load error");
                        return BadRequest(_localizer["An error was encountered, please check if the file has a valid format."].ToString());
                    }
                }
            }
            return Ok(import.UpdateId);
        }

        public async Task<IActionResult> GetImportJob(int importId)
        {
            var importJob = await _importService.GetImportHistory(importId);
            return Json(importJob);
        }

        //[Authorize(Policy = SharedAuthorizationPolicy.FullModify)]
        public async Task<IActionResult> LoadStep_FieldMapping(int importId, bool isUpdate = false)
        {
            var importJob = await _importService.GetImportHistory(importId);
            var columnsLookup = await _importService.GetDataImportTypeColumns(importJob.DataType.TableType);
            columnsLookup.Add(new DataImportTypeColumn { ColumnName = "" });

            ViewBag.TypeColumns = columnsLookup;

            if (isUpdate)
                return PartialView("_UpdateStepFieldMapping", importJob);

            return PartialView("_ImportStepFieldMapping", importJob);
        }

        //[Authorize(Policy = SharedAuthorizationPolicy.FullModify)]
        public async Task<IActionResult> LoadStep_Review(int importId, bool isUpdate = false)
        {
            var importJob = await _importService.GetImportHistory(importId);
            if (importJob != null)
            {
                var dupeMappings = _importService.DataImportMappings.Where(m => m.ImportId == importId && !string.IsNullOrEmpty(m.CPIField)).GroupBy(m => m.CPIField)
                                                 .Where(g => g.Count() > 1).Select(y => y.Key).ToList();
                if (dupeMappings.Count > 0)
                {
                    var message = "";
                    var field = "";
                    if (dupeMappings.Count == 1)
                    {
                        message = _localizer["CPI Field [{0}] has duplicate mappings."];
                        field = dupeMappings.First();
                    }
                    else
                    {
                        message = _localizer["CPI Fields [{0}] have duplicate mappings."];
                        field = String.Join(", ", dupeMappings);
                    }
                    return BadRequest(String.Format(message, field));
                }

                if (isUpdate)
                {
                    var hasRemarksUpdate = await _importService.DataImportMappings.AsNoTracking().AnyAsync(d => d.ImportId == importId && !string.IsNullOrEmpty(d.CPIField) && d.CPIField.ToLower() == "remarks");
                    ViewBag.HasRemarksUpdate = hasRemarksUpdate;

                    var portfolioDataType = new List<string>() { "Country Application", "Invention", "Trademark", "General Matter" };
                    var actionDataType = new List<string>() { "Patent Actions", "Trademark Actions", "Matter Actions" };
                    if (portfolioDataType.Contains(importJob.DataType.DataType))
                        ViewBag.UpdateOptions = string.IsNullOrEmpty(importJob.Options) ? new DataUpdateOptionsPortfolioViewModel() : JsonConvert.DeserializeObject<DataUpdateOptionsPortfolioViewModel>(importJob.Options);
                    else if (actionDataType.Contains(importJob.DataType.DataType))
                        ViewBag.UpdateOptions = string.IsNullOrEmpty(importJob.Options) ? new DataUpdateOptionsActionViewModel() : JsonConvert.DeserializeObject<DataUpdateOptionsActionViewModel>(importJob.Options);
                    else
                        ViewBag.UpdateOptions = string.IsNullOrEmpty(importJob.Options) ? new DataUpdateOptionsViewModel() : JsonConvert.DeserializeObject<DataUpdateOptionsViewModel>(importJob.Options);

                    return PartialView("_UpdateStepReview", importJob);
                }
                else
                {
                    if (importJob.DataType.DataType == "Patent Portfolio" || importJob.DataType.DataType == "Trademark Portfolio")
                        ViewBag.ImportOptions = string.IsNullOrEmpty(importJob.Options) ? new DataImportOptionsPortfolioViewModel() : JsonConvert.DeserializeObject<DataImportOptionsPortfolioViewModel>(importJob.Options);
                    else if (importJob.DataType.DataType == "Trademark Images")
                        ViewBag.ImportOptions = string.IsNullOrEmpty(importJob.Options) ? new DataImportOptionsImageViewModel() : JsonConvert.DeserializeObject<DataImportOptionsImageViewModel>(importJob.Options);
                    else if (importJob.DataType.DataType == "References")
                        ViewBag.ImportOptions = string.IsNullOrEmpty(importJob.Options) ? new DataImportOptionsIDSViewModel() : JsonConvert.DeserializeObject<DataImportOptionsIDSViewModel>(importJob.Options);
                    else
                        ViewBag.ImportOptions = string.IsNullOrEmpty(importJob.Options) ? new DataImportOptionsViewModel() : JsonConvert.DeserializeObject<DataImportOptionsViewModel>(importJob.Options);

                    return PartialView("_ImportStepReview", importJob);
                }
            }
            return BadRequest();
        }

        public async Task<IActionResult> GetImportStatus(int importId)
        {
            var importStatus = await _importService.GetImportStatus(importId);

            if (!(importStatus == ImportStatus.Failed
                    || importStatus == ImportStatus.Imported
                    || importStatus == ImportStatus.Processing
                    || importStatus == ImportStatus.Updated
                    || importStatus == ImportStatus.UpdateFailed
                ))
            {
                importStatus = importStatus == ImportStatus.ForMapping ? ImportStatus.Processing : ImportStatus.Failed;
            }

            return Content(importStatus);
        }

        public async Task<IActionResult> LoadStepImportResult_Success(int importId, bool isUpdate = false)
        {
            var importJob = await _importService.GetImportHistory(importId);

            if (isUpdate)
                return PartialView("_UpdateStepResult_Success", importJob);

            return PartialView("_ImportStepResult_Success", importJob);
        }

        public IActionResult LoadStepImportResult_Fail(int importId, bool isUpdate = false)
        {
            ViewBag.ImportID = importId;

            if (isUpdate)
                return PartialView("_UpdateStepResult_Fail");

            return PartialView("_ImportStepResult_Fail");
        }

        public IActionResult LoadStepImportResult_Processing(int importId, bool isUpdate = false)
        {
            ViewBag.ImportId = importId;

            if (isUpdate)
                return PartialView("_UpdateStepResult_Processing");

            return PartialView("_ImportStepResult_Processing");
        }

        //public async Task<IActionResult> LoadImportStatus(int importId)
        //{
        //    var importStatus = await _importService.GetImportStatus(importId);
        //    switch (importStatus) {
        //        case ImportStatus.Failed:
        //            return LoadStepImportResult_Fail(importId);
        //        case ImportStatus.Imported:
        //            return await LoadStepImportResult_Success(importId);
        //        case ImportStatus.Processing:
        //            return LoadStepImportResult_Processing(importId);
        //    }
        //    return Ok();
        //}

        public async Task<IActionResult> GetStructure(int dataTypeId)
        {
            var dataType = await _importService.GetDataImportType(dataTypeId);
            if (dataType != null)
            {
                return PartialView("_DataTypeColumns", dataType);
            }
            return Ok();
        }

        public async Task<IActionResult> DownloadErrors(int importId, bool isUpdate = false, bool ignoredOnly = false)
        {
            var errors = await _importService.DataImportErrors.Where(e => e.ImportId == importId && (ignoredOnly == false || (ignoredOnly && e.ErrorType == DataImportErrorType.Ignored)))
                .OrderBy(e => e.Row).Select(e => new { e.Row, e.Error }).ToListAsync();

            var worksheetName = _localizer["Import Errors"];
            var fileName = _localizer["ImportErrors"];
            
            if (isUpdate)
            {
                worksheetName = _localizer["Update Errors"];
                fileName = _localizer["UpdateErrors"];
            }
            else if (ignoredOnly)
            {
                worksheetName = _localizer["Import Skipped"];
                fileName = _localizer["ImportSkipped"];                
            }

            MemoryStream fileStream;

            if (!ignoredOnly)
                fileStream = await _exportHelper.ListToExcelMemoryStream(errors, worksheetName, _localizer);
            else 
                fileStream = await _exportHelper.ListToExcelMemoryStream(errors.Select(d => new { Row = d.Row, Data = d.Error }).ToList(), worksheetName, _localizer);

            return File(fileStream.ToArray(), ImageHelper.GetContentType(".xlsx"), fileName + ".xlsx");
        }

        public async Task<IActionResult> DataTypeColumnsRead([DataSourceRequest] DataSourceRequest request, string tableType)
        {
            var columns = (await _importService.GetDataImportTypeColumns(tableType))
              .Select(c => BuildUserFriendlyColumn(c)).ToDataSourceResult(request);
            return Json(columns);
        }

        public async Task<IActionResult> ImportErrorsRead([DataSourceRequest] DataSourceRequest request, int importId, bool ignoredOnly = false)
        {
            var errors = await _importService.DataImportErrors.Where(e => e.ImportId == importId && (ignoredOnly == false || (ignoredOnly && e.ErrorType == DataImportErrorType.Ignored))).ToDataSourceResultAsync(request);
            return Json(errors);
        }

        //[Authorize(Policy = SharedAuthorizationPolicy.FullModify)]
        [HttpPost]
        public async Task<IActionResult> Import(int importId, string options, bool isUpdate = false)
        {
            var importJob = await _importService.GetImportHistory(importId);
            var fileName = importJob.FileName;
            var fullPath = Path.Combine(_hostingEnvironment.ContentRootPath, ImportFolder, fileName);

            var imageDataType = new List<string>() { "Trademark Images" };
            var isImageImport = imageDataType.Any(d => d == importJob.DataType.DataType);

            var columns = await _importService.DataImportMappings.Where(m => m.ImportId == importId).ToListAsync();
            var imageColumn = string.Empty;

            if (isImageImport) imageColumn = columns.Where(d => d.CPIField == "Image").Select(d => d.YourField).FirstOrDefault();

            var dtFromExcel = ImportExceltoDataTable(fullPath, isImageImport, imageColumn);

            var errors = new List<DataImportError>();

            //remove unmapped columns
            foreach (var c in columns)
            {
                if (string.IsNullOrEmpty(c.CPIField))
                {
                    dtFromExcel.Columns.Remove(c.YourField);
                }
            }

            //rename the mapped columns
            foreach (DataColumn c in dtFromExcel.Columns)
            {
                var column = columns.FirstOrDefault(mc => mc.YourField == c.ColumnName);
                if (column != null)
                {
                    dtFromExcel.Columns[c.ColumnName].ColumnName = column.CPIField;
                }
            }

            var dtToImport = _importService.GetStructure(importJob.DataType);

            if (isImageImport) dtToImport.Columns.Add("CPI_FileExt", typeof(string));

            //transfer data from Excel to server structure 
            if (dtFromExcel.Columns.Count > 0)
            {
                var row = 0;
                foreach (DataRow dr in dtFromExcel.Rows)
                {
                    row++;
                    DataRow toInsert = dtToImport.NewRow();

                    try
                    {
                        foreach (DataColumn c in dtFromExcel.Columns)
                        {
                            //Skip if column doesn't exist in TVP
                            if (!dtToImport.Columns.Contains(c.ColumnName))
                                continue;

                            var value = dr[c.ColumnName];
                            if (value.ToString() == string.Empty)
                                value = DBNull.Value;

                            if (toInsert.Table.Columns[c.ColumnName].DataType == typeof(System.Boolean))
                            {
                                toInsert[c.ColumnName] = value.ToString() == "1";
                            }
                            else
                            {
                                toInsert[c.ColumnName] = value;
                            }
                        }
                        dtToImport.Rows.Add(toInsert);
                    }
                    catch (Exception ex)
                    {
                        var error = BuildUserFriendlyException(ex, toInsert);
                        error.ImportId = importId;
                        error.Row = row + 1;
                        errors.Add(error);
                    }

                }

                // var distinctErrors = errors.GroupBy(e => e.Error).Select(e => e.FirstOrDefault());
                await _importService.UpdateErrors(importId, errors, isUpdate);

                if (errors.Count > 0)
                    return BadRequest();

                else
                {
                    if (isImageImport)
                    {
                        if (importJob.DataType.DataType == "Trademark Images")
                        {
                            var isOk = await ImportTrademarkImage(dtToImport, importId, options, User.GetUserName());
                            if (!isOk) return BadRequest();
                        }
                    }
                    else
                    {
                        await _importService.Import(importJob.DataType, dtToImport, importId, options, User.Identity.Name); //User.GetUserName()
                    }

                    if (isUpdate)
                        return Ok(new { success = _localizer["Records have been updated successfully"].ToString() });

                    return Ok(new { success = _localizer["Records have been imported successfully"].ToString() });
                }
            }
            return BadRequest();

        }

        [HttpPost]
        public async Task<Microsoft.AspNetCore.Mvc.ActionResult> ExportFile(int dataType)
        {
            var type = await _importService.GetDataImportType(dataType);
            var columns = (await _importService.GetDataImportTypeColumns(type.TableType))
                         .Select(c => BuildUserFriendlyColumn(c)).ToList();
            var fileName = $"{await GetDataTypeLabel(type.DataType)}.xlsx";
            var fileStream = await _exportHelper.ListToExcelMemoryStream(columns, "Details", _exportLocalizer);
            return File(fileStream.ToArray(), ImageHelper.GetContentType(".xlsx"), fileName);
        }

        #region FieldMapping
        public async Task<IActionResult> FieldMappingRead([DataSourceRequest] DataSourceRequest request, int importId)
        {
            var result = await _importService.DataImportMappings.Where(m => m.ImportId == importId).ToDataSourceResultAsync(request);
            return Json(result);
        }

        //[Authorize(Policy = SharedAuthorizationPolicy.FullModify)]
        public async Task<IActionResult> FieldMappingUpdate([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "updated")] IList<DataImportMapping> updated,
                   [Bind(Prefix = "new")] IList<DataImportMapping> added, [Bind(Prefix = "deleted")] IList<DataImportMapping> deleted)
        {
            if (!ModelState.IsValid)
                return new JsonBadRequest(new { errors = ModelState.Errors() });

            await _importService.UpdateMappings(updated);
            return Ok(new { success = _localizer["Field mappings have been saved successfully"].ToString() });
        }

        public async Task<IActionResult> FieldMappingMappedRead([DataSourceRequest] DataSourceRequest request, int importId)
        {
            var result = await _importService.DataImportMappings.Where(m => m.ImportId == importId && !string.IsNullOrEmpty(m.CPIField)).ToDataSourceResultAsync(request);
            return Json(result);
        }

        public async Task<IActionResult> FieldMappingUnmappedRead([DataSourceRequest] DataSourceRequest request, int importId)
        {
            var result = await _importService.DataImportMappings.Where(m => m.ImportId == importId && string.IsNullOrEmpty(m.CPIField)).ToDataSourceResultAsync(request);
            return Json(result);
        }

        #endregion

        #region Helpers
        private async Task InitializeImportJob(int importId, int dataType, string systemType, string fileName, int rowCount)
        {
            _importJob = new DataImportHistory
            {
                ImportId = importId,
                ImportDate = DateTime.Now,
                OrigFileName = fileName,
                DataTypeId = dataType,
                NoOfRecords = rowCount,
                Status = ImportStatus.Uploaded,
                ImportedBy = User.GetUserName(),
                SystemType = systemType
            };
            await _importService.InitializeImportJob(_importJob);
        }

        private async Task FlagForMappingReview(string newFileName)
        {
            _importJob.FileName = newFileName;
            _importJob.Status = ImportStatus.ForMapping;
            await _importService.SaveChanges();
        }

        private List<string> GetColumnNames(IXLWorksheet workSheet)
        {

            IXLRow row = workSheet.Row(1);
            var fields = new List<string>();
            foreach (IXLCell cell in row.Cells())
            {
                var cellValue = cell.Value.ToString();
                if (cellValue != null && !string.IsNullOrEmpty(cellValue.Trim()))
                {
                    fields.Add(cellValue);
                }
                else
                {
                    break;
                }
            }
            return fields;
        }

        private string GetNewFileName(string oldFileName, int importId)
        {
            var newFileName = importId.ToString() + Path.GetExtension(oldFileName);

            var folderExists = System.IO.Directory.Exists(Path.Combine(_hostingEnvironment.ContentRootPath, ImportFolder));
            if (!folderExists)
                System.IO.Directory.CreateDirectory(Path.Combine(_hostingEnvironment.ContentRootPath, ImportFolder));

            var fullPath = Path.Combine(_hostingEnvironment.ContentRootPath, ImportFolder, newFileName);
            return fullPath;
        }

        private DataTable ImportExceltoDataTable(string filePath, bool isImageImport = false, string? imageColumnName = null)
        {
            var checkDateFrom = new DateTime(1899, 12, 31);
            var checkDateTo = new DateTime(1900, 2, 27);

            var imageColumnLetter = string.Empty;

            using (XLWorkbook workBook = new XLWorkbook(filePath))
            {
                IXLWorksheet workSheet = workBook.Worksheet(1);
                IXLPictures wsPictures = workSheet.Pictures;

                DataTable dt = new DataTable();

                bool firstRow = true;
                foreach (IXLRow row in workSheet.Rows())
                {
                    if (!row.IsEmpty())
                    {
                        if (firstRow)
                        {
                            foreach (IXLCell cell in row.Cells())
                            {
                                if (!string.IsNullOrEmpty(cell.Value.ToString()))
                                {
                                    if (isImageImport && cell.Value.ToString() == imageColumnName)
                                    {
                                        dt.Columns.Add(cell.Value.ToString(), typeof(byte[]));
                                        imageColumnLetter = cell.Address.ColumnLetter.ToString();
                                    }
                                    else
                                    {
                                        dt.Columns.Add(cell.Value.ToString());
                                    }
                                }
                                else
                                {
                                    break;
                                }
                            }

                            //Add cpi_rowno at the end of columns to keep data sorted
                            if (!dt.Columns.Contains("cpi_rowno"))
                            {
                                dt.Columns.Add("cpi_rowno", typeof(System.Int32));
                            }

                            //If importing images, add another column to keep file extension
                            if (isImageImport && !dt.Columns.Contains("CPI_FileExt")) dt.Columns.Add("CPI_FileExt");

                            firstRow = false;
                        }
                        else
                        {
                            int i = 0;
                            DataRow toInsert = dt.NewRow();
                            foreach (IXLCell cell in row.Cells(1, dt.Columns.Count))
                            {
                                //Skip if column is cpi_rowno
                                if (toInsert.Table.Columns.Contains("cpi_rowno") && toInsert.Table.Columns["cpi_rowno"] != null && i == toInsert.Table.Columns["cpi_rowno"]?.Ordinal)
                                    continue;

                                //note: dates before 1/1/1900 - excel stores this as text
                                //dates between 1/1/1900-2/28/1900 - 1 day is taken so we have to add it back (maybe an api problem?)
                                if (cell.DataType == XLDataType.DateTime && cell.Value != null)
                                {
                                    var dateValue = Convert.ToDateTime(cell.Value);
                                    if (dateValue >= checkDateFrom && dateValue <= checkDateTo)
                                    {
                                        dateValue = dateValue.AddDays(1);
                                        cell.Value = dateValue;
                                    }
                                }

                                if (isImageImport && !string.IsNullOrEmpty(imageColumnLetter) && cell.Address.ColumnLetter == imageColumnLetter)
                                {
                                    var tempImage = wsPictures.Where(d => d.TopLeftCell == cell).FirstOrDefault();
                                    if (tempImage != null)
                                    {
                                        toInsert[i] = tempImage.ImageStream.ToArray();
                                        toInsert["CPI_FileExt"] = tempImage.Format.ToString().ToLower();
                                    }
                                }
                                else
                                {
                                    if (dt.Columns[i].ColumnName != "CPI_FileExt")
                                    {
                                        if (cell.HasFormula)
                                            toInsert[i] = cell.RichText.ToString();
                                        else if (cell.Value != null)
                                            toInsert[i] = cell.Value.ToString();
                                    }                                    
                                }

                                i++;
                            }

                            //Add cpi_rowno by using excel row index
                            if (dt.Columns.Contains("cpi_rowno"))
                            {
                                toInsert["cpi_rowno"] = row.RowNumber() - 1;
                            }

                            dt.Rows.Add(toInsert);
                        }
                    }
                }
                return dt;
            }
        }

        private DataImportTypeViewModel BuildUserFriendlyColumn(DataImportTypeColumn c)
        {
            var length = c.CharMaxLength;
            if (c.DataType.Contains("nvarchar"))
                length = (short)(c.CharMaxLength / 2);

            var colDataType = "string";
            if (c.DataType.Contains("int"))
                colDataType = "numeric";

            else if (c.DataType.Contains("date"))
                colDataType = "date";

            else if (c.DataType == "bit")
                colDataType = "1/0";

            var model = new DataImportTypeViewModel
            {
                ColumnName = c.ColumnName,
                Required = !c.IsNullable,
                DataType = colDataType,
                MaxLength = colDataType == "string" ? (length <= 0 ? "max" : length.ToString()) : "",
                Description = c.Description
            };
            return model;
        }

        private DataImportError BuildUserFriendlyException(Exception ex, DataRow toInsert)
        {

            string message = ex.Message;
            DataImportErrorType errorType = DataImportErrorType.Invalid;

            if (message.Contains("The value violates the MaxLength"))
            {
                var field = Regex.Match(message, "Cannot set column '(.*?)'").Groups[1].ToString();

                if (!string.IsNullOrEmpty(field))
                {
                    var value = toInsert[field];

                    var maxLength = toInsert.Table.Columns[field].MaxLength;
                    message = _localizer["{0} - The value {1} violates the maximum length ({2}) limit of this column."];
                    message = string.Format(message, field, value, maxLength);
                    errorType = DataImportErrorType.MaxLength;
                }
            }

            if (message.Contains("does not allow nulls"))
            {
                var field = Regex.Match(message, "Column '(.*?)' does").Groups[1].ToString();

                if (!string.IsNullOrEmpty(field))
                {
                    message = _localizer["{0} - does not allow empty values."];
                    message = string.Format(message, field);
                    errorType = DataImportErrorType.Null;
                }
            }

            var pos = message.IndexOf("Couldn't store");
            if (pos > 0)
            {
                message = message.Substring(pos);
                var value = Regex.Match(message, "Couldn't store <(.*?)> in").Groups[1].ToString();
                message = message.Replace($"<{value}>", "<{0}>");
                message = _localizer[message];
                message = string.Format(message, value);
                errorType = DataImportErrorType.DataType;
            }
            return new DataImportError { ErrorType = errorType, Error = message };
        }

        private async Task<bool> CanAddRecord(string systemType)
        {
            switch (systemType)
            {
                case SystemTypeCode.Trademark:
                    return (await _authService.AuthorizeAsync(User, TrademarkAuthorizationPolicy.FullModify)).Succeeded;

                case SystemTypeCode.GeneralMatter:
                    return (await _authService.AuthorizeAsync(User, GeneralMatterAuthorizationPolicy.FullModify)).Succeeded;

                case SystemTypeCode.IDS:
                    return (await _authService.AuthorizeAsync(User, IDSAuthorizationPolicy.FullModify)).Succeeded;

                case SystemTypeCode.AMS:
                    return (await _authService.AuthorizeAsync(User, AMSAuthorizationPolicy.InternalFullModify)).Succeeded;

                default:
                    return (await _authService.AuthorizeAsync(User, PatentAuthorizationPolicy.FullModify)).Succeeded;
            }
        }

        private async Task<bool> CanDeleteRecord(string systemType)
        {

            switch (systemType)
            {
                case SystemTypeCode.Trademark:
                    return (await _authService.AuthorizeAsync(User, TrademarkAuthorizationPolicy.CanDelete)).Succeeded;

                case SystemTypeCode.GeneralMatter:
                    return (await _authService.AuthorizeAsync(User, GeneralMatterAuthorizationPolicy.CanDelete)).Succeeded;

                case SystemTypeCode.IDS:
                    return (await _authService.AuthorizeAsync(User, IDSAuthorizationPolicy.FullModify)).Succeeded;

                case SystemTypeCode.AMS:
                    return (await _authService.AuthorizeAsync(User, AMSAuthorizationPolicy.InternalFullModify)).Succeeded;

                default:
                    return (await _authService.AuthorizeAsync(User, PatentAuthorizationPolicy.CanDelete)).Succeeded;
            }
        }

        private async Task<string> GetDataTypeLabel(string dataType)
        {
            var settings = await _settings.GetSetting();
            switch (dataType.ToLower())
            {
                case "client":
                    return settings.LabelClient;
                case "agent":
                    return settings.LabelAgent;
                case "owner":
                    return settings.LabelOwner;
                default:
                    return dataType;
            }
        }

        protected enum ErrorType
        {
            MaxLength = 1,
            Required = 2,
            DataType = 3,
            Dupe = 4
        }

        protected class ImportStatus
        {
            public const string Uploaded = "File Uploaded";
            public const string ForMapping = "For Mapping Review";
            public const string Failed = "Import Failed";
            public const string Imported = "Imported";
            public const string Processing = "Processing";
            public const string Updated = "Updated";
            public const string UpdateFailed = "Update Failed";
        }
        #endregion

        #region Import Images
        private async Task<bool> ImportTrademarkImage(DataTable table, int importId, string options, string userName)
        {
            var settings = await _settings.GetSetting();
            var extList = settings.ValidImageFileExtensions;

            var importOptions = string.IsNullOrEmpty(options) ? new DataImportOptionsImageViewModel() : JsonConvert.DeserializeObject<DataImportOptionsImageViewModel>(options);
            var errors = new List<DataImportError>();
            var ignores = new List<DataImportError>();
            var row = 0;
            var importList = new List<(int TmkId, string RecKey, bool isDefault, string ImageName, string FileExt, byte[] Image)>();
            var tmkId = 0;
            var caseNumber = string.Empty;
            var country = string.Empty;
            var subCase = string.Empty;
            var fileName = string.Empty;
            var fileExt = string.Empty;
            var recKey = string.Empty;
            var isDefaultImage = false;

            var importHistory = await _importService.DataImportsHistory.FirstOrDefaultAsync(d => d.ImportId == importId);
            if (importHistory != null)
                importHistory.Options = options;

            foreach (DataRow dr in table.Rows)
            {
                row++;
                try
                {
                    //Check for missing image data/byte[]                    
                    var imageData = dr.Field<byte[]>("Image");
                    if (imageData == null)
                    {
                        throw new Exception(_localizer["Missing Image"].ToString());
                    }

                    caseNumber = dr.Field<string>("CaseNumber");
                    country = dr.Field<string>("Country");
                    subCase = (dr.Field<string>("SubCase") == null ? "" : dr.Field<string>("SubCase"));
                    isDefaultImage = dr.Field<bool?>("Default") == null ? false : dr.Field<bool>("Default");;

                    recKey = caseNumber + SharePointSeparator.Folder + country + (!string.IsNullOrEmpty(subCase) ? SharePointSeparator.Field + subCase : "");

                    var trademark = await _trademarkService.TmkTrademarks.Where(d => d.CaseNumber == caseNumber && d.Country == country && d.SubCase == subCase)
                                                                .Select(d => new { d.TmkId, d.TrademarkName }).FirstOrDefaultAsync();

                    fileName = dr.Field<string>("ImageName");
                    fileExt = dr.Field<string>("CPI_FileExt");

                    if (trademark != null)
                    {
                        tmkId = trademark.TmkId;
                        if (importOptions != null && importOptions.UseTrademarkName)
                        {
                            fileName = trademark.TrademarkName?.Trim();
                        }
                        else if (string.IsNullOrEmpty(fileName))
                        {
                            throw new Exception(_localizer["Missing image name for: "].ToString() + recKey);
                        }
                    }

                    if (importList.Any(d => d.ImageName.Contains(fileName ?? "") && d.FileExt == fileExt && d.RecKey.Contains(caseNumber + SharePointSeparator.Folder + country)))
                    {
                        var lastNum = importList.Where(d => d.ImageName.Contains(fileName ?? "") && d.FileExt == fileExt && d.RecKey.Contains(caseNumber + SharePointSeparator.Folder + country))
                                                .Select(d => d.ImageName.Replace(fileName ?? "", "").Replace("-", ""))
                                                .Where(d => !string.IsNullOrEmpty(d)).OrderByDescending(d => d).FirstOrDefault();

                        int intLastNum;
                        if (!string.IsNullOrEmpty(lastNum) && int.TryParse(lastNum, out intLastNum))
                        {
                            fileName += "-" + (intLastNum + 1).ToString();
                        }
                        else if (string.IsNullOrEmpty(lastNum))
                        {
                            fileName += "-1";
                        }
                    }

                    //Check for invalid file extensions
                    if (!extList.Contains(fileExt))
                    {
                        throw new Exception(_localizer["Invalid image extension: "].ToString() + fileName + "." + fileExt ?? "");
                    }

                    //Check for not existing records
                    if (importOptions != null)
                    {
                        if (!importOptions.IgnoreOrphans && tmkId <= 0)
                            throw new Exception(_localizer["Entry doesn't exist: "].ToString() + recKey);
                        else if (importOptions.IgnoreOrphans && tmkId <= 0)
                        {
                            var ignoreRecord = new DataImportError()
                            {
                                ImportId = importId,
                                ErrorType = DataImportErrorType.Ignored,
                                Row = row + 1,
                                Error = _localizer["Entry: "].ToString() + recKey
                            };
                            ignores.Add(ignoreRecord);
                        }
                    }

                    if (tmkId > 0)
                    {
                        var byteData = dr.Field<byte[]>("Image");
                        if (byteData != null && byteData.Length > 0)
                            importList.Add((tmkId, recKey, isDefaultImage, fileName?.Trim() ?? "", fileExt ?? "", byteData));
                    }

                    caseNumber = string.Empty;
                    country = string.Empty;
                    subCase = string.Empty;
                    tmkId = 0;
                    fileName = string.Empty;
                    fileExt = string.Empty;
                    recKey = string.Empty;
                    isDefaultImage = false;
                }
                catch (Exception ex)
                {
                    var error = BuildUserFriendlyException(ex, dr);
                    error.ImportId = importId;
                    error.Row = row + 1;
                    errors.Add(error);
                }
            }

            await _importService.UpdateErrors(importId, errors, false);

            if (errors.Count > 0)
            {
                if (importHistory != null)
                {
                    importHistory.Status = ImportStatus.Failed;
                    await _importService.UpdateImportHistory(importHistory);
                }                    

                return false;
            }
            else
            {                                
                var importStatus = false;

                if (importList.Count > 0)
                {
                    try
                    {
                        row = 0;
                        foreach (var item in importList)
                        {
                            var fullFileName = item.ImageName + "." + item.FileExt;

                            var ms = new MemoryStream(item.Image);
                            IFormFile formFile = new FormFile(ms, 0, item.Image.Length, item.ImageName, fullFileName)
                            {
                                Headers = new HeaderDictionary(),
                                ContentType = "image/" + item.FileExt
                            };

                            var documentLink = $"{SystemTypeCode.Trademark}|{ScreenCode.Trademark}|TmkId|{item.TmkId}";
                            await _docImportService.ImportDocument(formFile, documentLink, SharePointDocLibrary.Trademark, SharePointDocLibraryFolder.Trademark, item.RecKey);

                            ////Check duplicate file name
                            //if (await _docService.DocDocuments.AnyAsync(d => d.DocFolder != null && d.DocFolder.DataKey == "TmkId" && d.DocFolder.ScreenCode == ScreenCode.Trademark && d.DocFolder.DataKeyValue == item.TmkId && d.DocName == fullFileName))
                            //{
                            //    //Use current date time to avoid copy duplicate
                            //    fullFileName = item.ImageName + "_copy-" + DateTime.Now.ToString("dd-MMM-yyyy-HH-mm-ss") + "." + item.FileExt;
                            //}

                            //if (settings.DocumentStorage == DocumentStorageOptions.SharePoint)
                            //{
                            //    await SaveSharePointImportedDocument(formFile, fullFileName, item.TmkId, SharePointDocLibrary.Trademark, SharePointDocLibraryFolder.Trademark, item.RecKey, item.isDefault);
                            //}
                            //else
                            //{
                            //    await SaveImportedDocument(formFile, fullFileName, item.TmkId, userName, SystemTypeCode.Trademark, ScreenCode.Trademark, "TmkId", item.isDefault);
                            //}

                            row++;
                        }

                        importStatus = true;
                        if (importHistory != null)
                        {
                            importHistory.NoOfRecordsImported = row;
                            importHistory.Status = ImportStatus.Imported;
                            importHistory.Error = string.Empty;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, _localizer["Save image error"].ToString());
                        if (importHistory != null)
                        {
                            importHistory.Status = ImportStatus.Failed;
                            importHistory.Error = ex.Message;
                        }
                    }
                }

                if (importHistory != null) 
                    await _importService.UpdateImportHistory(importHistory);

                if (ignores != null && ignores.Count > 0)
                    await _importService.AddErrors(ignores);

                return importStatus;
            }
        }

        protected async Task SaveImportedDocument(IFormFile formFile, string fileName, int id, string userName, string systemType, string screenCode, string dataKey, bool isDeault)
        {
            var documentLink = string.Join("|", systemType, screenCode, dataKey, id.ToString());
            var folder = await _docViewModelService.GetOrAddDefaultFolder(documentLink);

            var parentId = folder.DataKeyValue;
            var viewModels = new List<DocDocumentViewModel>
            {
                new DocDocumentViewModel()
                {
                    ParentId = parentId,
                    UploadedFile = formFile,
                    Author = User.GetEmail(),
                    CreatedBy = userName,
                    UpdatedBy = userName,
                    LastUpdate = DateTime.Now,
                    DateCreated = DateTime.Now,
                    UserFileName = fileName,
                    FolderId = folder.FolderId,
                    DocumentLink = documentLink,
                    DocFolder = folder,
                    Source = DocumentSourceType.Manual,
                    IsActRequired = false,
                    IsVerified = false,
                    IsDefault = isDeault
                }
            };
            await _docViewModelService.SaveUploadedDocuments(viewModels);
        }

        protected async Task SaveSharePointImportedDocument(IFormFile formFile, string fileName, int parentId, string docLibrary, string docLibraryFolder, string recKey, bool isDeault)
        {
            var settings = await _settings.GetSetting();

            var folders = SharePointViewModelService.GetDocumentFolders(docLibraryFolder, recKey);
            var result = new SharePointGraphDriveItemKeyViewModel();
            var graphClient = _sharePointService.GetGraphClient();
            using (var stream = new MemoryStream())
            {
                formFile.CopyTo(stream);
                stream.Position = 0;
                result = await graphClient.UploadSiteFile(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, docLibrary, folders, stream, fileName);
            }

            if (!string.IsNullOrEmpty(result.DriveItemId))
            {
                var driveItem = await graphClient.Drives[result.DriveId].Items[result.DriveItemId].Request().Expand("listItem").GetAsync();

                if (isDeault)
                {   
                    var site = graphClient.GetSiteWithLists(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName).Result;
                    var list = site.Lists.Where(l => l.Name == docLibrary).FirstOrDefault();

                    //we need to unmark other image as default
                    if (!settings.IsSharePointIntegrationKeyFieldsOnly)
                    {
                        if (settings.IsSharePointIntegrationByMetadataOn)
                        {
                            await graphClient.UnmarkDefaultImageByMetadata(site.Id, list.Id, docLibraryFolder, recKey);
                        }
                        else
                        {                            
                            await graphClient.UnmarkDefaultImage(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, docLibrary, folders, site.Id, list.Id);
                        }
                    }

                    if (list != null)
                    {
                        var requestBody = new FieldValueSet
                        {
                            AdditionalData = new Dictionary<string, object>
                            {
                                {
                                    "IsDefault" , isDeault
                                }
                            }
                        };                        
                        var updateResult = await graphClient.Sites[site.Id].Lists[list.Id].Items[driveItem.ListItem.Id].Fields.Request().UpdateAsync(requestBody);
                    }
                }

                var sync = new SharePointSyncToDocViewModel
                {
                    DocLibrary = docLibrary,
                    DocLibraryFolder = docLibraryFolder,
                    DriveItemId = result.DriveItemId,
                    ParentId = parentId,
                    FileName = fileName,
                    CreatedBy = User.GetUserName().Left(20),
                    Author = User.GetEmail(),
                    Remarks = "",
                    Tags = "",
                    IsImage = driveItem.Image != null,
                    IsPrivate = false,
                    IsDefault = isDeault,
                    IsPrintOnReport = false,
                    IsVerified = false,
                    IncludeInWorkflow = false,
                    IsActRequired = false,
                    CheckAct = false,
                    Source = DocumentSourceType.Manual
                };
                await _sharePointViewModelService.SyncToDocumentTables(sync);
            }
        }
        #endregion
    }
}