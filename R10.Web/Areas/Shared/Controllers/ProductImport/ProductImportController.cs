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
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;
using R10.Web.Interfaces;
using R10.Core.Entities.Patent;

using R10.Web.Areas;

namespace R10.Web.Areas.Shared.Controllers
{
    [Area("Shared"), Authorize(Policy = SharedAuthorizationPolicy.CanAccessSystem)]
    public class ProductImportController : BaseController
    {
        private readonly IAuthorizationService _authService;
        private readonly IProductImportService _importService;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly IStringLocalizer<ProductImportTypeViewModel> _exportLocalizer;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly ILogger _logger;
        private readonly ExportHelper _exportHelper;
        private readonly ISystemSettings<PatSetting> _patSettings;
        private readonly IPatInventorRemunerationService _patInventorRemunerationService;

        private ProductImportHistory _importJob;

        private const int MaxRecordsAllowed = 30000;
        private const string ImportFolder = @"UserFiles\PrdProductImport";

        public ProductImportController(IProductImportService importService, IAuthorizationService authService,
                                    IStringLocalizer<SharedResource> localizer, ILogger<ProductImportController> logger,
                                    IHostingEnvironment hostingEnvironment, IStringLocalizer<ProductImportTypeViewModel> exportLocalizer,
                                    ExportHelper exportHelper, ISystemSettings<PatSetting> patSettings,
                                    IPatInventorRemunerationService patInventorRemunerationService)
        {
            _importService = importService;
            _authService = authService;
            _localizer = localizer;
            _exportLocalizer = exportLocalizer;
            _logger = logger;
            _hostingEnvironment = hostingEnvironment;
            _exportHelper = exportHelper;
            _patSettings = patSettings;
            _patInventorRemunerationService = patInventorRemunerationService;
        }

        [Authorize(Policy = PatentAuthorizationPolicy.CanAccessProducts)]
        public async Task<IActionResult> Patent()
        {
            return await Index(SystemTypeCode.Patent);
        }

        [Authorize(Policy = TrademarkAuthorizationPolicy.CanAccessProducts)]
        public async Task<IActionResult> Trademark()
        {
            return await Index(SystemTypeCode.Trademark);
        }

        [Authorize(Policy = GeneralMatterAuthorizationPolicy.CanAccessProducts)]
        public async Task<IActionResult> GeneralMatter()
        {
            return await Index(SystemTypeCode.GeneralMatter);
        }

        [Authorize(Policy = AMSAuthorizationPolicy.CanAccessProducts)]
        public async Task<IActionResult> AMS()
        {
            return await Index(SystemTypeCode.AMS);
        }

        public async Task<IActionResult> Index(string systemType = SystemTypeCode.Patent)
        {
            var authorized = false;

            if (systemType == SystemTypeCode.Trademark)
                authorized = (await _authService.AuthorizeAsync(User, TrademarkAuthorizationPolicy.CanAccessProducts)).Succeeded;
            else if (systemType == SystemTypeCode.GeneralMatter)
                authorized = (await _authService.AuthorizeAsync(User, GeneralMatterAuthorizationPolicy.CanAccessProducts)).Succeeded;
            else if (systemType == SystemTypeCode.AMS)
                authorized = (await _authService.AuthorizeAsync(User, AMSAuthorizationPolicy.CanAccessProducts)).Succeeded;
            else
                authorized = (await _authService.AuthorizeAsync(User, PatentAuthorizationPolicy.CanAccessProducts)).Succeeded;

            if (!authorized)
                return Forbid();

            ViewBag.CanAddRecord = await CanAddRecord();
            ViewBag.CanDeleteRecord = await CanDeleteRecord();
            ViewBag.SystemType = systemType;

            return View("Index");
        }

        public async Task<IActionResult> ImportHistoryRead([DataSourceRequest] DataSourceRequest request)
        {
            var result = await _importService.ProductImportsHistory.ToDataSourceResultAsync(request);
            var data = (List<ProductImportHistory>)result.Data;
            data.ForEach(h=> h.TranslatedStatus= _localizer[h.Status]);
            return Json(result);
        }

        [Authorize(Policy = SharedAuthorizationPolicy.FullModify)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<Microsoft.AspNetCore.Mvc.ActionResult> UploadFile(ProductImportViewModel import)
        {
            var form = Request.Form;
            if (form.Files.Count > 0)
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

                        await InitializeImportJob(import.ImportId, formFile.FileName, rowsCount - 1);
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
            return Ok(import.ImportId);
        }
        
        public async Task<IActionResult> GetImportJob(int importId)
        {
            var importJob = await _importService.GetImportHistory(importId);
            return Json(importJob);
        }

        [Authorize(Policy = SharedAuthorizationPolicy.FullModify)]
        public async Task<IActionResult> LoadStep_FieldMapping(int importId)
        {
            var importJob = await _importService.GetImportHistory(importId);
            var columnsLookup = await _importService.GetDataImportTypeColumns();
            columnsLookup.Add(new ProductImportTypeColumn { ColumnName=""});

            ViewBag.TypeColumns = columnsLookup;
            return PartialView("_ImportStepFieldMapping", importJob);
        }

        [Authorize(Policy = SharedAuthorizationPolicy.FullModify)]
        public async Task<IActionResult> LoadStep_Review(int importId)
        {
            var importJob = await _importService.GetImportHistory(importId);
            if (importJob != null)
            {
                var dupeMappings = _importService.ProductImportMappings.Where(m=>m.ImportId==importId && !string.IsNullOrEmpty(m.CPIField)).GroupBy(m => m.CPIField)
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
                    return BadRequest(String.Format(message,field));
                }
                
                ViewBag.ImportOptions = string.IsNullOrEmpty(importJob.Options) ? new ProductImportOptionsViewModel() : JsonConvert.DeserializeObject<ProductImportOptionsViewModel>(importJob.Options);

                return PartialView("_ImportStepReview", importJob);
            }
            return BadRequest();
        }

        public async Task<IActionResult> GetImportStatus(int importId)
        {
            var importStatus = await _importService.GetImportStatus(importId);
            if (!(importStatus==ImportStatus.Failed || importStatus == ImportStatus.Imported || importStatus == ImportStatus.Processing)) {
                importStatus = importStatus == ImportStatus.ForMapping ? ImportStatus.Processing : ImportStatus.Failed;
            }
            return Content(importStatus);
        }

        public async Task<IActionResult> LoadStepImportResult_Success(int importId)
        {
            var importJob = await _importService.GetImportHistory(importId);
            return PartialView("_ImportStepResult_Success", importJob);
        }

        public IActionResult LoadStepImportResult_Fail(int importId)
        {
            ViewBag.ImportID = importId;
            return PartialView("_ImportStepResult_Fail");
        }

        public IActionResult LoadStepImportResult_Processing(int importId)
        {
            ViewBag.ImportId = importId;
            return PartialView("_ImportStepResult_Processing");
        }

        public IActionResult GetStructure()
        {
            //var dataType = await _importService.GetDataImportType(dataTypeId);
            //if (dataType != null)
            //{
            //    return PartialView("_DataTypeColumns", dataType);
            //}
            //return Ok();
            return PartialView("_DataTypeColumns");
        }

        public async Task<IActionResult> DownloadErrors(int importId)
        {
            var errors = await _importService.ProductImportErrors.Where(e => e.ImportId == importId).OrderBy(e => e.Row).Select(e=> new {e.Row,e.Error }).ToListAsync();
            var fileStream = await _exportHelper.ListToExcelMemoryStream(errors, "Import Errors", _localizer);
            return File(fileStream.ToArray(), ImageHelper.GetContentType(".xlsx"), "ImportErrors.xlsx");
        }

        public async Task<IActionResult> DataTypeColumnsRead([DataSourceRequest] DataSourceRequest request)
        {
            var columns = (await _importService.GetDataImportTypeColumns())
              .Select(c => BuildUserFriendlyColumn(c)).ToDataSourceResult(request);
            return Json(columns);
        }

        public async Task<IActionResult> ImportErrorsRead([DataSourceRequest] DataSourceRequest request, int importId)
        {
            var errors = await _importService.ProductImportErrors.Where(e => e.ImportId == importId).ToDataSourceResultAsync(request);
            return Json(errors);
        }

        [Authorize(Policy = SharedAuthorizationPolicy.FullModify)]
        [HttpPost]
        public async Task<IActionResult> Import(int importId, string options)
        {
            var importJob = await _importService.GetImportHistory(importId);
            var fileName = importJob.FileName;
            var fullPath = Path.Combine(_hostingEnvironment.ContentRootPath, ImportFolder, fileName);
            var dtFromExcel = ImportExceltoDataTable(fullPath);

            var columns = await _importService.ProductImportMappings.Where(m => m.ImportId == importId).ToListAsync();
            var errors = new List<ProductImportError>();

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

            var dtToImport = _importService.GetStructure();

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
                await _importService.UpdateErrors(importId, errors);

                if (errors.Count > 0) 
                    return BadRequest();
                
                else {
                    await _importService.Import(dtToImport, importId, options, User.GetUserName());
                    var patSettings = await _patSettings.GetSetting();
                    if (patSettings.IsInventorRemunerationOn && (patSettings.InventorRemunerationPayOption.ToLower() == "both" || patSettings.InventorRemunerationPayOption.ToLower() == "yearly") && patSettings.IsInventorRemunerationUsingProductSalesOn)
                    {
                        await _patInventorRemunerationService.ProcessImportedProductSales(User.GetUserName());
                    }
                    return Ok(new { success = _localizer["Records have been imported successfully"].ToString() });
                }
            }
            return BadRequest();

        }

        //[HttpPost]
        public async Task<Microsoft.AspNetCore.Mvc.ActionResult> ExportFile()
        {
            var columns = (await _importService.GetDataImportTypeColumns())
                         .Select(c => BuildUserFriendlyColumn(c)).ToList();
            var fileStream = await _exportHelper.ListToExcelMemoryStream(columns, "Details", _exportLocalizer);
            return File(fileStream.ToArray(), ImageHelper.GetContentType(".xlsx"), "Product Import Structure.xlsx");
        }

        #region FieldMapping
        public async Task<IActionResult> FieldMappingRead([DataSourceRequest] DataSourceRequest request, int importId)
        {
            var result = await _importService.ProductImportMappings.Where(m => m.ImportId == importId).ToDataSourceResultAsync(request);
            return Json(result);
        }

        [Authorize(Policy = SharedAuthorizationPolicy.FullModify)]
        public async Task<IActionResult> FieldMappingUpdate([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "updated")]IList<ProductImportMapping> updated,
                   [Bind(Prefix = "new")]IList<ProductImportMapping> added, [Bind(Prefix = "deleted")]IList<ProductImportMapping> deleted)
        {
            if (!ModelState.IsValid)
                return new JsonBadRequest(new { errors = ModelState.Errors() });

            await _importService.UpdateMappings(updated);
            return Ok(new { success = _localizer["Field mappings have been saved successfully"].ToString() });
        }

        public async Task<IActionResult> FieldMappingMappedRead([DataSourceRequest] DataSourceRequest request, int importId)
        {
            var result = await _importService.ProductImportMappings.Where(m => m.ImportId == importId && !string.IsNullOrEmpty(m.CPIField)).ToDataSourceResultAsync(request);
            return Json(result);
        }

        public async Task<IActionResult> FieldMappingUnmappedRead([DataSourceRequest] DataSourceRequest request, int importId)
        {
            var result = await _importService.ProductImportMappings.Where(m => m.ImportId == importId && string.IsNullOrEmpty(m.CPIField)).ToDataSourceResultAsync(request);
            return Json(result);
        }

        #endregion

        #region Helpers
        private async Task InitializeImportJob(int importId, string fileName, int rowCount)
        {
            _importJob = new ProductImportHistory
            {
                ImportId = importId,
                ImportDate = DateTime.Now,
                OrigFileName = fileName,                
                NoOfRecords = rowCount,
                Status = ImportStatus.Uploaded,
                ImportedBy = User.GetUserName()               
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
                if (!string.IsNullOrEmpty(cell.Value.ToString()))
                {
                    fields.Add(cell.Value.ToString());
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
            var fullPath = Path.Combine(_hostingEnvironment.ContentRootPath, ImportFolder, newFileName);
            return fullPath;
        }

        private DataTable ImportExceltoDataTable(string filePath)
        {
            using (XLWorkbook workBook = new XLWorkbook(filePath))
            {
                IXLWorksheet workSheet = workBook.Worksheet(1);

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
                                    dt.Columns.Add(cell.Value.ToString());
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

                            firstRow = false;
                        }
                        else
                        {
                            int i = 0;
                            DataRow toInsert = dt.NewRow();
                            foreach (IXLCell cell in row.Cells(1, dt.Columns.Count))
                            {
                                //Skip if column is cpi_rowno
                                if (toInsert.Table.Columns.Contains("cpi_rowno") && i == toInsert.Table.Columns["cpi_rowno"].Ordinal)
                                    continue;

                                toInsert[i] = cell.Value.ToString();
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

        private ProductImportTypeViewModel BuildUserFriendlyColumn(ProductImportTypeColumn c)
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

            var model = new ProductImportTypeViewModel
            {
                ColumnName = c.ColumnName,
                Required = !c.IsNullable,
                DataType = colDataType,
                MaxLength = colDataType == "string" ? length.ToString() : "",
                Description=c.Description
            };
            return model;
        }

        private ProductImportError BuildUserFriendlyException(Exception ex, DataRow toInsert)
        {
            string message = ex.Message;
            int errorType=0;

            if (message.Contains("The value violates the MaxLength"))
            {
                var field = Regex.Match(message, "Cannot set column '(.*?)'").Groups[1].ToString();

                if (!string.IsNullOrEmpty(field))
                {
                    var value = toInsert[field];

                    var maxLength = toInsert.Table.Columns[field].MaxLength;
                    message = _localizer["{0} - The value {1} violates the maximum length ({2}) limit of this column."];
                    message = string.Format(message, field, value, maxLength);
                    errorType = 1;
                }
            }

            if (message.Contains("does not allow nulls"))
            {
                var field = Regex.Match(message, "Column '(.*?)' does").Groups[1].ToString();

                if (!string.IsNullOrEmpty(field))
                {
                    message = _localizer["{0} - does not allow empty values."];
                    message = string.Format(message, field);
                    errorType = 2;
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
                errorType = 3;
            }
            return new ProductImportError { ErrorType = errorType, Error = message };
        }

        private async Task<bool> CanAddRecord()
        {
            return (await _authService.AuthorizeAsync(User, SharedAuthorizationPolicy.FullModify)).Succeeded;
        }

        private async Task<bool> CanDeleteRecord()
        {
            return (await _authService.AuthorizeAsync(User, SharedAuthorizationPolicy.CanDelete)).Succeeded;
        }

        protected enum ErrorType
        {
            MaxLength = 1,
            Required = 2,
            DataType = 3,
            Dupe=4
        }

        protected class ImportStatus
        {
            public const string Uploaded = "File Uploaded";
            public const string ForMapping = "For Mapping Review";
            public const string Failed = "Import Failed";
            public const string Imported = "Imported";
            public const string Processing = "Processing";
        }
        #endregion
    }



}