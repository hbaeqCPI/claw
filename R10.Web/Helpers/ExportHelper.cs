using ClosedXML.Excel;

using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using Kendo.Mvc.Extensions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using R10.Core.Entities.Shared;
using R10.Core.DTOs;
using R10.Core.Interfaces;
using R10.Web.Models;
using R10.Web.Services;
using R10.Web.Areas.Shared.ViewModels;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using WordProcessing = DocumentFormat.OpenXml.Wordprocessing;
using R10.Web.Services.DocumentStorage;
using System;
using A = DocumentFormat.OpenXml.Drawing;
using DW = DocumentFormat.OpenXml.Drawing.Wordprocessing;
using PIC = DocumentFormat.OpenXml.Drawing.Pictures;
using R10.Core.Identity;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Presentation;
using P = DocumentFormat.OpenXml.Presentation;
using D = DocumentFormat.OpenXml.Drawing;
using R10.Web.Services.SharePoint;
using Microsoft.Extensions.Options;
using DocumentFormat.OpenXml.Wordprocessing;
using R10.Web.Services.iManage;
using R10.Web.Interfaces;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using R10.Web.Services.NetDocuments;

namespace R10.Web.Helpers
{
    public class ExportHelper
    {
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly ISystemSettings<DefaultSetting> _settings;
        private readonly IDocumentStorage _documentStorage;
        private readonly IUserSettingsService _userSettingsService;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly ISharePointService _sharePointService;
        private readonly GraphSettings _graphSettings;
        private readonly IiManageClientFactory _iManageClientFactory;
        private readonly INetDocumentsClientFactory _netDocsClientFactory;
        private readonly IDocumentsViewModelService _docViewModelService;
        protected readonly IUrlHelper _url;

        const string wordBorderColor = "silver";
        const string wordHeaderFillColor = "silver";
        const int excelMaxWorksheetName = 30;

        public ExportHelper(IStringLocalizer<SharedResource> localizer,
                            ISystemSettings<DefaultSetting> settings,
                            IConfiguration configuration,
                            IDocumentStorage documentStorage,
                            IUserSettingsService userSettingsService,
                            IHostingEnvironment hostingEnvironment,
                            ISharePointService sharePointService, IOptions<GraphSettings> graphSettings,
                            IiManageClientFactory iManageClientFactory,
                            INetDocumentsClientFactory netDocsClientFactory,
                            IDocumentsViewModelService docViewModelService,
                            IUrlHelper url)
        {
            _localizer = localizer;
            _settings = settings;
            _documentStorage = documentStorage;
            _userSettingsService = userSettingsService;
            _hostingEnvironment = hostingEnvironment;
            _sharePointService = sharePointService;
            _graphSettings = graphSettings.Value;
            _iManageClientFactory = iManageClientFactory;
            _netDocsClientFactory = netDocsClientFactory;
            _docViewModelService = docViewModelService;
            _url = url;
        }

        public ExportHelper() { }

        #region Excel
        public async Task<MemoryStream> DataTableToExcelMemoryStream(DataTable dt, string workSheetName, string imageColumnSuffix, string imageRootFolder, int imageSize)
        {
            var wb = await DataTableToExcelWorkbook(dt, workSheetName, imageColumnSuffix, imageRootFolder, imageSize);
            //do not use "using" to avoid returning the IDisposable
            MemoryStream stream = new MemoryStream();
            wb.SaveAs(stream);
            return stream;
        }

        private async Task<XLWorkbook> DataTableToExcelWorkbook(DataTable dt, string workSheetName, string imageColumnSuffix, string rootImageFolder, int imageSize = 50)
        {

            var setting = await _settings.GetSetting();
            var exportableImages = setting.DQExportableImages.Split("|");

            var wb = new XLWorkbook();
            workSheetName = workSheetName.Substring(0, Math.Min(workSheetName.Length, excelMaxWorksheetName));
            var ws = wb.Worksheets.Add(dt, workSheetName);                  // pass dt as parameter so CloseXML will apply nice formatting to the worksheet

            int nCols = dt.Columns.Count;
            int nRows = dt.Rows.Count;

            // save indices of image and date columns for special handling below
            List<int> imageColumns = new List<int>();
            List<int> webLinkColumns = new List<int>();
            List<int> dateColumns = new List<int>();

            imageColumnSuffix = imageColumnSuffix.ToLower();
            for (int col = 0; col < nCols; col++)
            {
                if (imageColumnSuffix.Length > 0 && dt.Columns[col].ColumnName.ToLower().EndsWith(imageColumnSuffix))
                    imageColumns.Add(col);
                if (dt.Columns[col].ColumnName.ToLower().EndsWith("weblink"))
                    webLinkColumns.Add(col);
                else if (dt.Columns[col].DataType.Name == "DateTime")
                    dateColumns.Add(col);
            }

            // format date columns
            for (int row = 0; row < nRows; row++)
            {
                foreach (int col in dateColumns)
                {
                    if (dt.Rows[row][col].ToString() != "")
                    {
                        ws.Cell(row + 2, col + 1).Style.NumberFormat.Format = "dd-MMM-yyyy";
                    }
                }

                foreach (int col in webLinkColumns)
                {
                    if (dt.Rows[row][col].ToString() != "" && dt.Rows[row][col].ToString().Contains("/") && dt.Rows[row][col].ToString().Count(c => c == '/') == 2)
                    {
                        var cell = ws.Cell(row + 2, col + 1);
                        var urlData = dt.Rows[row][col].ToString();
                        var urlParts = urlData.Split("/");
                        var weblink = _url.ActionLink("Detail", urlParts[1], new { area = urlParts[0], id = urlParts[2] });
                        var recordType = Regex.Replace(urlParts[1], "(?<!^)([A-Z])", " $1");
                        cell.Value = $"{recordType} Web Link";
                        cell.Hyperlink = new XLHyperlink(weblink) { Tooltip = $"Go to {recordType}" };
                        cell.Style.Font.Underline = XLFontUnderlineValues.Single;
                        cell.Style.Font.FontColor = XLColor.Blue;
                    }
                }
            }

            // spit out images
            if (imageColumns.Count > 0)
            {
                if (!rootImageFolder.EndsWith("/")) rootImageFolder += "/";
                for (int row = 0; row < nRows; row++)
                {
                    foreach (int col in imageColumns)
                    {
                        var imageFile = dt.Rows[row][col].ToString();
                        if (!string.IsNullOrEmpty(imageFile) && exportableImages.Any(x => imageFile.ToLower().EndsWith(x)))        // filter image export to avoid error
                        {
                            string imageFilePath = rootImageFolder + imageFile;
                            var fileStream = new MemoryStream();

                            if (setting.DocumentStorage == DocumentStorageOptions.iManage)
                            {
                                var docFile = await _docViewModelService.GetDocFileByDocFileName(imageFile);
                                if (docFile != null && !string.IsNullOrEmpty(docFile.DriveItemId))
                                {
                                    try
                                    {
                                        using (var client = await _iManageClientFactory.GetClient())
                                        {
                                            fileStream = await client.GetDocumentAsStream(docFile.DriveItemId);
                                        }
                                    }
                                    catch (Exception)
                                    {
                                        //ignore errors
                                    }
                                }
                            }
                            else
                            if (setting.DocumentStorage == DocumentStorageOptions.NetDocuments)
                            {
                                var docFile = await _docViewModelService.GetDocFileByDocFileName(imageFile);
                                if (docFile != null && !string.IsNullOrEmpty(docFile.DriveItemId))
                                {
                                    try
                                    {
                                        using (var client = await _netDocsClientFactory.GetClient())
                                        {
                                            fileStream = await client.GetDocumentAsStream(docFile.DriveItemId);
                                        }
                                    }
                                    catch (Exception)
                                    {
                                        //ignore errors
                                    }
                                }
                            }
                            else if (setting.DocumentStorage == DocumentStorageOptions.SharePoint)
                            {
                                var docFile = await _docViewModelService.GetDocFileByDocFileName(imageFile);
                                if (docFile != null && !string.IsNullOrEmpty(docFile.DriveItemId))
                                {
                                    var graphClient = _sharePointService.GetGraphClient();
                                    var content = await graphClient.Drives[docFile.DriveItemId].Items[imageFile].Content.Request().GetAsync();

                                    content.CopyTo(fileStream);
                                    fileStream.Position = 0;
                                }
                            }
                            else
                            {
                                fileStream = await _documentStorage.GetFileStream(imageFilePath);
                            }

                            if (fileStream?.Length > 0)
                            {
                                ws.Cell(row + 2, col + 1).Value = "";           // clear image name on cell
                                var image = ws.AddPicture(fileStream).MoveTo(ws.Cell(row + 2, col + 1));
                                image.Width = imageSize;
                                image.Height = imageSize;
                            }
                        }
                    }
                }
                ws.RowHeight = imageSize;       // adjust height to fit images
            }
            else
            {
                ws.Rows().AdjustToContents();   // if no images, adjust row height to content
            }

            ws.Columns().AdjustToContents();

            return wb;
        }

        public async Task<MemoryStream> ListToExcelMemoryStream<T, T1>(List<T> list, string workSheetName, IStringLocalizer<T1> exportLocalizer, bool adjustCol = true,
                                                                    string imageColumnSuffix = "", string imageRootFolder = "", int imageSize = 50, bool showTime = false,
                                                                    List<string> excludeColumns = null, List<ExportSettingViewModel> customLabels = null)
        {
            var wb = await ListToExcelWorkbook(list, workSheetName, exportLocalizer, adjustCol, imageColumnSuffix, imageRootFolder, imageSize, showTime, excludeColumns, null, customLabels);
            MemoryStream stream = new MemoryStream();
            wb.SaveAs(stream);
            return stream;
        }

        public async Task AddWorksheet<T, T1>(List<T> list, string workSheetName, IStringLocalizer<T1> exportLocalizer, XLWorkbook wb)
        {
            await ListToExcelWorkbook(list, workSheetName, exportLocalizer, true, "", "", 50, false, null, wb);
        }


        private async Task<XLWorkbook> ListToExcelWorkbook<T, T1>(List<T> list, string workSheetName, IStringLocalizer<T1> exportLocalizer, bool adjustCol = true,
                                                              string imageColumnSuffix = "", string imageRootFolder = "", int imageSize = 50, bool showTime = false,
                                                              List<string> excludeColumns = null, XLWorkbook wb = null, List<ExportSettingViewModel> customLabels = null)
        {
            var setting = await _settings.GetSetting();
            var exportableImages = setting.DQExportableImages.Split("|");

            if (wb == null)
                wb = new XLWorkbook();

            workSheetName = workSheetName.Substring(0, Math.Min(workSheetName.Length, excelMaxWorksheetName));
            var ws = wb.Worksheets.Add(workSheetName);


            List<int> imageColumns = new List<int>();
            imageColumnSuffix = imageColumnSuffix.ToLower();

            var columns = typeof(T).GetProperties().ToList();
            for (int col = 1; col <= columns.Count; col++)
            {
                var column = columns[col - 1];

                var noExport = column.GetCustomAttribute(typeof(NoExportAttribute)) as NoExportAttribute;
                if (noExport != null || (excludeColumns?.Any(c => c.ToLower() == column.Name.ToLower()) ?? false))
                {
                    ws.Column(col).Hide();

                    //important
                    if (imageColumnSuffix.Length > 0 && column.Name.ToLower().EndsWith(imageColumnSuffix) && !excludeColumns.Any(x => x.ToLower().EndsWith(imageColumnSuffix)))
                    {
                        imageColumns.Add(col);
                    }
                }
                else
                {
                    // format date columns
                    if (column.PropertyType.ToString().Contains("DateTime"))
                    {
                        if (!showTime)
                            ws.Column(col).Style.NumberFormat.Format = "dd-MMM-yyyy";
                        else
                            ws.Column(col).Style.NumberFormat.Format = "dd-MMM-yyyy hh:mm AM/PM";
                    }

                    else if (imageColumnSuffix.Length > 0 && column.Name.ToLower().EndsWith(imageColumnSuffix))
                    {
                        imageColumns.Add(col);
                    }
                }
            }

            var properties = columns.Select(p => {
                var dd = p.GetCustomAttribute(typeof(DisplayAttribute)) as DisplayAttribute;
                if (dd != null)
                    return dd.Name.Replace("{{;}}", setting.ReportExcelRecordDelimiter).Replace("{{,}}", setting.ReportExcelFieldDelimiter);
                else
                {
                    if (p.Name.StartsWith("CustomField") && customLabels != null)
                    {
                        var customLabel = customLabels.FirstOrDefault(cf => cf.PropertyName == p.Name);
                        if (customLabel != null)
                            return customLabel.Label;
                    }
                    return p.Name;
                }
            }).ToArray();

            if (exportLocalizer != null)
            {
                for (var i = 0; i <= properties.Length - 1; i++)
                {
                    if (properties[i].StartsWith("Label"))
                        properties[i] = await GetLocalizedLabel(properties[i], setting);
                    else
                        properties[i] = exportLocalizer[properties[i]];
                }
            }

            var titles = new List<string[]>();
            titles.Add(properties);

            var rangeTitle = ws.Cell(1, 1).InsertData(titles);
            rangeTitle.AddToNamed("titles");
            var titlesStyle = wb.Style;
            titlesStyle.Font.Bold = true;
            wb.NamedRanges.NamedRange("titles").Ranges.Style = titlesStyle;
            ws.Cell(2, 1).InsertData(list);

            var imageRowsAndSize = new Dictionary<int, int>();

            // spit out images
            if (imageColumns.Count > 0)
            {
                if (!imageRootFolder.EndsWith("/")) imageRootFolder += "/";

                var row = 0;
                if (setting.DocumentStorage == DocumentStorageOptions.SharePoint)
                {
                    var graphClient = _sharePointService.GetGraphClient();
                    var col = imageColumns.Last();
                    var driveId = imageColumns.First();

                    foreach (var item in list)
                    {
                        var prop = columns[col - 1];
                        var imageFile = prop.GetValue(item);
                        if (imageFile != null)
                        {
                            var driveIdProp = columns[driveId - 1];
                            var driveIdVal = driveIdProp.GetValue(item);

                            var stream = await graphClient.Drives[Convert.ToString(driveIdVal)].Items[Convert.ToString(imageFile)].Content.Request().GetAsync();
                            if (stream != null)
                            {
                                using (var memoryStream = new MemoryStream())
                                {
                                    stream.CopyTo(memoryStream);
                                    memoryStream.Position = 0;

                                    //ws.Row(row + 2).Height = imageSize;
                                    ws.Cell(row + 2, col).Clear();
                                    var image = ws.AddPicture(memoryStream).MoveTo(ws.Cell(row + 2, col));
                                    image.Width = imageSize;
                                    image.Height = imageSize;
                                }

                            }
                        }
                        row++;
                    }
                }
                else if (setting.DocumentStorage == DocumentStorageOptions.iManage ||
                         setting.DocumentStorage == DocumentStorageOptions.NetDocuments)
                {
                    var iManageClient = setting.DocumentStorage == DocumentStorageOptions.iManage? await _iManageClientFactory.GetClient() : null;
                    var netDocsClient = setting.DocumentStorage == DocumentStorageOptions.NetDocuments ? await _netDocsClientFactory.GetClient() : null;
                    var col = imageColumns.Last();

                    foreach (var item in list)
                    {
                        var prop = columns[col - 1];
                        var imageFile = prop.GetValue(item)?.ToString();
                        if (imageFile != null)
                        {
                            var docFile = await _docViewModelService.GetDocFileByDocFileName(imageFile);
                            if (docFile != null && !string.IsNullOrEmpty(docFile.DriveItemId))
                            {
                                try
                                {
                                    var memoryStream = new MemoryStream();

                                    if (iManageClient != null) memoryStream = await iManageClient.GetDocumentAsStream(docFile.DriveItemId);
                                    else if (netDocsClient != null) memoryStream = await netDocsClient.GetDocumentAsStream(docFile.DriveItemId);

                                    if (memoryStream != null)
                                    {
                                        memoryStream.Position = 0;

                                        //ws.Row(row + 2).Height = imageSize;
                                        ws.Cell(row + 2, col).Clear();
                                        var image = ws.AddPicture(memoryStream).MoveTo(ws.Cell(row + 2, col));
                                        image.Width = imageSize;
                                        image.Height = imageSize;

                                        memoryStream.Dispose();
                                        memoryStream.Close();
                                    }
                                }
                                catch (Exception)
                                {
                                    //ignore errors
                                }
                            }
                        }
                        row++;
                    }
                }
                else
                {
                    foreach (var item in list)
                    {
                        foreach (int col in imageColumns)
                        {
                            var prop = columns[col - 1];
                            var imageFile = prop.GetValue(item);
                            if (imageFile != null)
                            {
                                var strImageFile = imageFile.ToString();
                                if (exportableImages.Any(x => strImageFile.ToLower().EndsWith(x)))       // filter image export to avoid error
                                {
                                    string imageFilePath = imageRootFolder + strImageFile;
                                    var stream = await _documentStorage.GetFileStream(imageFilePath);
                                    if (stream != null)
                                    {
                                        //ws.Row(row + 2).Height = imageSize;
                                        ws.Cell(row + 2, col).Clear();
                                        var image = ws.AddPicture(stream).MoveTo(ws.Cell(row + 2, col));
                                        image.Width = imageSize;
                                        image.Height = imageSize;

                                        imageRowsAndSize.Add(row + 2, imageSize);
                                    }
                                }
                            }
                        }
                        row++;
                    }
                }


            }

            if (adjustCol)
            {
                ws.Columns().AdjustToContents();
                ws.Rows().AdjustToContents();

                //restore image row size
                if (imageRowsAndSize.Any())
                {
                    var rows = ws.Rows();

                    foreach (var row in rows)
                    {
                        row.Height = row.Height + 10;
                        var rowNo = row.RowNumber();
                        int imageRowHeight;
                        if (imageRowsAndSize.TryGetValue(rowNo, out imageRowHeight) && row.Height < imageRowHeight)
                        {
                            row.Height = imageRowHeight;
                        }
                    }
                }

            }

            foreach (var column in ws.Columns())
            {
                if (column.IsHidden)
                    column.Delete();
            }

            return wb;
        }


        #endregion

        #region Word 

        public async Task<MemoryStream> DataTableToWordMemoryStream(DataTable data)
        {
            var setting = await _settings.GetSetting();

            MemoryStream stream = new MemoryStream();
            using (WordprocessingDocument doc = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document, true))
            {
                MainDocumentPart mainDocPart = doc.AddMainDocumentPart();
                mainDocPart.Document = new WordProcessing.Document();
                WordProcessing.Body body = new WordProcessing.Body();
                mainDocPart.Document.Append(body);

                WordProcessing.Table table = new WordProcessing.Table();


                // set borders
                WordProcessing.TableBorders tblBorders = new WordProcessing.TableBorders();

                var topBorder = new WordProcessing.TopBorder();
                topBorder.Val = new EnumValue<WordProcessing.BorderValues>(WordProcessing.BorderValues.Thick);
                topBorder.Color = wordBorderColor;
                //topBorder.Size = 24;
                tblBorders.AppendChild(topBorder);

                var bottomBorder = new WordProcessing.BottomBorder();
                bottomBorder.Val = new EnumValue<WordProcessing.BorderValues>(WordProcessing.BorderValues.Thick);
                bottomBorder.Color = wordBorderColor;
                tblBorders.AppendChild(bottomBorder);

                var leftBorder = new WordProcessing.LeftBorder();
                leftBorder.Val = new EnumValue<WordProcessing.BorderValues>(WordProcessing.BorderValues.Thick);
                leftBorder.Color = wordBorderColor;
                tblBorders.AppendChild(leftBorder);

                var rightBorder = new WordProcessing.RightBorder();
                rightBorder.Val = new EnumValue<WordProcessing.BorderValues>(WordProcessing.BorderValues.Thick);
                rightBorder.Color = wordBorderColor;
                tblBorders.AppendChild(rightBorder);

                var insideHBorder = new WordProcessing.InsideHorizontalBorder();
                insideHBorder.Val = new EnumValue<WordProcessing.BorderValues>(WordProcessing.BorderValues.Thick);
                insideHBorder.Color = wordBorderColor;
                tblBorders.AppendChild(insideHBorder);

                var insideVBorder = new WordProcessing.InsideVerticalBorder();
                insideVBorder.Val = new EnumValue<WordProcessing.BorderValues>(WordProcessing.BorderValues.Thick);
                insideVBorder.Color = wordBorderColor;
                tblBorders.AppendChild(insideVBorder);

                // add the table borders to the table properties
                WordProcessing.TableProperties tblProperties = new WordProcessing.TableProperties();
                tblProperties.AppendChild(tblBorders);

                // add table properties to the table
                table.AppendChild(tblProperties);

                // add column names row for header
                WordProcessing.TableRow headerRow = new WordProcessing.TableRow();
                for (int j = 0; j < data.Columns.Count; j++)
                {
                    WordProcessing.TableCell cell = new WordProcessing.TableCell();
                    var columnLabel = await GetLocalizedLabel("Label" + data.Columns[j].ColumnName, setting);
                    if (columnLabel.StartsWith("Label"))
                        columnLabel = columnLabel.Replace("Label", "");
                    cell.Append(new WordProcessing.Paragraph(new WordProcessing.Run(new WordProcessing.Text(columnLabel))));

                    // set background color
                    WordProcessing.TableCellProperties cellProps = new WordProcessing.TableCellProperties(
                            //new WordProcessing.TableCellWidth { Type = WordProcessing.TableWidthUnitValues.Dxa, Width = "1500" });
                            new WordProcessing.TableCellWidth { Type = WordProcessing.TableWidthUnitValues.Auto });
                    WordProcessing.Shading shading = new WordProcessing.Shading() { Color = "auto", Fill = wordHeaderFillColor, Val = WordProcessing.ShadingPatternValues.Clear };
                    cellProps.Append(shading);
                    cell.Append(cellProps);

                    headerRow.Append(cell);
                }
                table.Append(headerRow);

                // ADD DATA
                for (int i = 0; i < data.Rows.Count; ++i)
                {
                    var row = new WordProcessing.TableRow();

                    for (int j = 0; j < data.Columns.Count; j++)
                    {
                        WordProcessing.TableCell cell = new WordProcessing.TableCell();
                        cell.Append(new WordProcessing.Paragraph(new WordProcessing.Run(new WordProcessing.Text(data.Rows[i][j].ToString()))));
                        cell.Append(new WordProcessing.TableCellProperties(
                                new WordProcessing.TableCellWidth { Type = WordProcessing.TableWidthUnitValues.Dxa, Width = "1500" }));
                        row.Append(cell);
                    }
                    table.Append(row);
                }

                // specify header
                var firstRow = table.GetFirstChild<WordProcessing.TableRow>();
                if (firstRow.TableRowProperties == null)
                    firstRow.TableRowProperties = new WordProcessing.TableRowProperties();
                firstRow.TableRowProperties.AppendChild(new WordProcessing.TableHeader());

                body.Append(table);
                doc.MainDocumentPart.Document.Save();
            }
            stream.Seek(0, SeekOrigin.Begin);

            return stream;
        }

        public async Task<MemoryStream> DataTableToWordMemoryStream(DataTable data, string imageColumnSuffix, string rootImageFolder, int imageSize)
        {
            var setting = await _settings.GetSetting();

            MemoryStream stream = new MemoryStream();
            using (WordprocessingDocument doc = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document, true))
            {
                MainDocumentPart mainDocPart = doc.AddMainDocumentPart();
                mainDocPart.Document = new WordProcessing.Document();
                WordProcessing.Body body = new WordProcessing.Body();
                mainDocPart.Document.Append(body);

                WordProcessing.Table table = new WordProcessing.Table();


                // set borders
                WordProcessing.TableBorders tblBorders = new WordProcessing.TableBorders();

                var topBorder = new WordProcessing.TopBorder();
                topBorder.Val = new EnumValue<WordProcessing.BorderValues>(WordProcessing.BorderValues.Thick);
                topBorder.Color = wordBorderColor;
                //topBorder.Size = 24;
                tblBorders.AppendChild(topBorder);

                var bottomBorder = new WordProcessing.BottomBorder();
                bottomBorder.Val = new EnumValue<WordProcessing.BorderValues>(WordProcessing.BorderValues.Thick);
                bottomBorder.Color = wordBorderColor;
                tblBorders.AppendChild(bottomBorder);

                var leftBorder = new WordProcessing.LeftBorder();
                leftBorder.Val = new EnumValue<WordProcessing.BorderValues>(WordProcessing.BorderValues.Thick);
                leftBorder.Color = wordBorderColor;
                tblBorders.AppendChild(leftBorder);

                var rightBorder = new WordProcessing.RightBorder();
                rightBorder.Val = new EnumValue<WordProcessing.BorderValues>(WordProcessing.BorderValues.Thick);
                rightBorder.Color = wordBorderColor;
                tblBorders.AppendChild(rightBorder);

                var insideHBorder = new WordProcessing.InsideHorizontalBorder();
                insideHBorder.Val = new EnumValue<WordProcessing.BorderValues>(WordProcessing.BorderValues.Thick);
                insideHBorder.Color = wordBorderColor;
                tblBorders.AppendChild(insideHBorder);

                var insideVBorder = new WordProcessing.InsideVerticalBorder();
                insideVBorder.Val = new EnumValue<WordProcessing.BorderValues>(WordProcessing.BorderValues.Thick);
                insideVBorder.Color = wordBorderColor;
                tblBorders.AppendChild(insideVBorder);

                // add the table borders to the table properties
                WordProcessing.TableProperties tblProperties = new WordProcessing.TableProperties();
                tblProperties.AppendChild(tblBorders);

                // add table properties to the table
                table.AppendChild(tblProperties);

                // add column names row for header
                WordProcessing.TableRow headerRow = new WordProcessing.TableRow();
                for (int j = 0; j < data.Columns.Count; j++)
                {
                    WordProcessing.TableCell cell = new WordProcessing.TableCell();
                    var columnLabel = await GetLocalizedLabel("Label" + data.Columns[j].ColumnName, setting);
                    if (columnLabel.StartsWith("Label"))
                        columnLabel = columnLabel.Replace("Label", "");
                    cell.Append(new WordProcessing.Paragraph(new WordProcessing.Run(new WordProcessing.Text(columnLabel))));

                    // set background color
                    WordProcessing.TableCellProperties cellProps = new WordProcessing.TableCellProperties(
                            //new WordProcessing.TableCellWidth { Type = WordProcessing.TableWidthUnitValues.Dxa, Width = "1500" });
                            new WordProcessing.TableCellWidth { Type = WordProcessing.TableWidthUnitValues.Auto });
                    WordProcessing.Shading shading = new WordProcessing.Shading() { Color = "auto", Fill = wordHeaderFillColor, Val = WordProcessing.ShadingPatternValues.Clear };
                    cellProps.Append(shading);
                    cell.Append(cellProps);

                    headerRow.Append(cell);
                }
                table.Append(headerRow);

                imageColumnSuffix = imageColumnSuffix.ToLower();

                if (!rootImageFolder.EndsWith("/")) rootImageFolder += "/";
                var exportableImages = setting.DQExportableImages.Split("|");

                // ADD DATA
                for (int i = 0; i < data.Rows.Count; ++i)
                {
                    var row = new WordProcessing.TableRow();

                    for (int j = 0; j < data.Columns.Count; j++)
                    {
                        WordProcessing.TableCell cell = new WordProcessing.TableCell();
                        var imageFile = data.Rows[i][j].ToString();
                        if (imageColumnSuffix.Length > 0 && data.Columns[j].ColumnName.ToLower().EndsWith(imageColumnSuffix) && (!String.IsNullOrEmpty(imageFile)) && exportableImages.Any(x => imageFile.ToLower().EndsWith(x)))
                        {
                            string imageFilePath = rootImageFolder + imageFile;
                            var fileStream = new MemoryStream();

                            if (setting.DocumentStorage == DocumentStorageOptions.iManage)
                            {
                                var docFile = await _docViewModelService.GetDocFileByDocFileName(imageFile);
                                if (docFile != null && !string.IsNullOrEmpty(docFile.DriveItemId))
                                {
                                    try
                                    {
                                        using (var client = await _iManageClientFactory.GetClient())
                                        {
                                            fileStream = await client.GetDocumentAsStream(docFile.DriveItemId);
                                        }
                                    }
                                    catch (Exception)
                                    {
                                        //ignore errors
                                    }
                                }
                            }
                            else if (setting.DocumentStorage == DocumentStorageOptions.NetDocuments)
                            {
                                var docFile = await _docViewModelService.GetDocFileByDocFileName(imageFile);
                                if (docFile != null && !string.IsNullOrEmpty(docFile.DriveItemId))
                                {
                                    try
                                    {
                                        using (var client = await _netDocsClientFactory.GetClient())
                                        {
                                            fileStream = await client.GetDocumentAsStream(docFile.DriveItemId);
                                        }
                                    }
                                    catch (Exception)
                                    {
                                        //ignore errors
                                    }
                                }
                            }
                            else if (setting.DocumentStorage == DocumentStorageOptions.SharePoint)
                            {
                                var docFile = await _docViewModelService.GetDocFileByDocFileName(imageFile);
                                if (docFile != null && !string.IsNullOrEmpty(docFile.DriveItemId))
                                {
                                    var graphClient = _sharePointService.GetGraphClient();
                                    var content = await graphClient.Drives[docFile.DriveItemId].Items[imageFile].Content.Request().GetAsync();

                                    content.CopyTo(fileStream);
                                    fileStream.Position = 0;
                                }
                            }
                            else
                            {
                                fileStream = await _documentStorage.GetFileStream(imageFilePath);
                            }

                            if (fileStream?.Length > 0)
                            {
                                fileStream.Position = 0;
                                ImagePart imagePart = mainDocPart.AddImagePart(ImagePartType.Jpeg);
                                doc.MainDocumentPart.Document.Save();
                                imagePart.FeedData(fileStream);
                                cell.Append(new WordProcessing.Paragraph(new WordProcessing.Run(GetDrawing(mainDocPart.GetIdOfPart(imagePart)))));
                                cell.Append(new WordProcessing.TableCellProperties(
                                        new WordProcessing.TableCellWidth { Type = WordProcessing.TableWidthUnitValues.Dxa, Width = "1800" }));

                            }
                            else
                            {
                                cell.Append(new WordProcessing.Paragraph(new WordProcessing.Run(new WordProcessing.Text(imageFile))));
                                cell.Append(new WordProcessing.TableCellProperties(
                                        new WordProcessing.TableCellWidth { Type = WordProcessing.TableWidthUnitValues.Dxa, Width = "1500" }));
                            }
                        }
                        else
                        {
                            cell.Append(new WordProcessing.Paragraph(new WordProcessing.Run(new WordProcessing.Text(imageFile))));
                            cell.Append(new WordProcessing.TableCellProperties(
                                    new WordProcessing.TableCellWidth { Type = WordProcessing.TableWidthUnitValues.Dxa, Width = "1500" }));
                        }

                        row.Append(cell);
                    }
                    table.Append(row);
                }

                // specify header
                var firstRow = table.GetFirstChild<WordProcessing.TableRow>();
                if (firstRow.TableRowProperties == null)
                    firstRow.TableRowProperties = new WordProcessing.TableRowProperties();
                firstRow.TableRowProperties.AppendChild(new WordProcessing.TableHeader());

                body.Append(table);

                //Change Orientation to Landscape
                //body.Append(
                //    new WordProcessing.Paragraph(
                //        new WordProcessing.ParagraphProperties(
                //            new WordProcessing.SectionProperties(
                //                new WordProcessing.PageSize() { Width = (UInt32Value)15840U, Height = (UInt32Value)12240U, Orient = WordProcessing.PageOrientationValues.Landscape },
                //                new WordProcessing.PageMargin() { Top = 720, Right = Convert.ToUInt32(1440.0), Bottom = 360, Left = Convert.ToUInt32(1440.0), Header = (UInt32Value)450U, Footer = (UInt32Value)720U, Gutter = (UInt32Value)0U }))));

                doc.MainDocumentPart.Document.Save();
            }
            stream.Seek(0, SeekOrigin.Begin);

            return stream;
        }

        private static WordProcessing.Drawing GetDrawing(string relationshipId)
        {
            // Define the reference of the image.
            var element =
                 new WordProcessing.Drawing(
                     new DW.Inline(
                         new DW.Extent() { Cx = 792000L, Cy = 663600L },
                         new DW.EffectExtent()
                         {
                             LeftEdge = 0L,
                             TopEdge = 0L,
                             RightEdge = 0L,
                             BottomEdge = 0L
                         },
                         new DW.DocProperties()
                         {
                             Id = (UInt32Value)1U,
                             Name = "Picture 1"
                         },
                         new DW.NonVisualGraphicFrameDrawingProperties(
                             new A.GraphicFrameLocks() { NoChangeAspect = true }),
                         new A.Graphic(
                             new A.GraphicData(
                                 new PIC.Picture(
                                     new PIC.NonVisualPictureProperties(
                                         new PIC.NonVisualDrawingProperties()
                                         {
                                             Id = (UInt32Value)0U,
                                             Name = "New Bitmap Image.jpg"
                                         },
                                         new PIC.NonVisualPictureDrawingProperties()),
                                     new PIC.BlipFill(
                                         new A.Blip(
                                             new A.BlipExtensionList(
                                                 new A.BlipExtension()
                                                 {
                                                     Uri =
                                                       "{28A0092B-C50C-407E-A947-70E740481C1C}"
                                                 })
                                         )
                                         {
                                             Embed = relationshipId,
                                             CompressionState =
                                             A.BlipCompressionValues.Print
                                         },
                                         new A.Stretch(
                                             new A.FillRectangle())),
                                     new PIC.ShapeProperties(
                                         new A.Transform2D(
                                             new A.Offset() { X = 0L, Y = 0L },
                                             new A.Extents() { Cx = 792000L, Cy = 663600L }),
                                         new A.PresetGeometry(
                                             new A.AdjustValueList()
                                         )
                                         { Preset = A.ShapeTypeValues.Rectangle }))
                             )
                             { Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture" })
                     )
                     {
                         DistanceFromTop = (UInt32Value)0U,
                         DistanceFromBottom = (UInt32Value)0U,
                         DistanceFromLeft = (UInt32Value)0U,
                         DistanceFromRight = (UInt32Value)0U,
                         EditId = "50D07946"
                     });

            // Append the reference to body, the element should be in a Run.
            return element;
        }
        #endregion

        #region XML
        public MemoryStream DataTableToXMLMemoryStream(DataTable dt, string tableName)
        {

            dt.TableName = tableName.Replace(" ", "");
            //do not use "using" to avoid returning the IDisposable
            MemoryStream stream = new MemoryStream();
            dt.WriteXml(stream);
            stream.Position = 0;
            //var result = new XPathDocument(stream);
            return stream;
        }
        #endregion

        #region JSON

        public MemoryStream DataTableToJSONMemoryStream(DataTable dt)
        {
            var JSonString = DataTableToJSONString(dt);
            return GenerateStreamFromString(JSonString);
        }

        private string DataTableToJSONString(DataTable dt)
        {
            var JSONString = JsonConvert.SerializeObject(dt);
            return JSONString;
        }
        #endregion

        #region ExportHelpers
        private readonly string _exportScreenHasDefaultSettingName = "ExportToExcelHasScreenDefaults";
        public async Task<List<ExportSettingViewModel>> GetExportPropertyNames<T>(Type type, IStringLocalizer<T> exportLocalizer)
        {
            var columns = type.GetProperties().ToList();
            var properties = new Dictionary<string, string>();

            columns.ForEach(p =>
            {
                var dd = p.GetCustomAttribute(typeof(DisplayAttribute)) as DisplayAttribute;
                if (dd != null)
                    properties.Add(p.Name, dd.Name);
                else
                    properties.Add(p.Name, p.Name);
            });

            if (exportLocalizer != null)
            {
                var setting = await _settings.GetSetting();
                foreach (KeyValuePair<string, string> entry in properties)
                {
                    if (entry.Key != entry.Value && entry.Value.StartsWith("Label"))
                        properties[entry.Key] = await GetLocalizedLabel(entry.Value, setting);
                    else
                        properties[entry.Key] = exportLocalizer[entry.Value];
                }
            }

            var result = new List<ExportSettingViewModel>();
            for (int col = 0; col < columns.Count; col++)
            {
                var column = columns[col];
                var noExport = column.GetCustomAttribute(typeof(NoExportAttribute)) as NoExportAttribute;
                if (noExport == null)
                {
                    result.Add(new ExportSettingViewModel { PropertyName = column.Name, Label = properties[column.Name] });
                }
            }
            return result;
        }


        public async Task<List<ExportSettingViewModel>> GetDisplayPropertyNames<T>(Type type, IStringLocalizer<T> exportLocalizer, string systemCode)
        {
            var result = new List<ExportSettingViewModel>();
            var systemEnum = Enum.TryParse<TargetedSystem>(systemCode, out var parsedSystem) ? parsedSystem : (TargetedSystem?)null;

            var columns = type.GetProperties()
                .Where(p => p.GetCustomAttribute<DisplayAttribute>() != null &&
                    p.GetCustomAttributes<TargetedSystemAttribute>().Any(attr => attr.Systems.Contains(systemEnum.Value)))
                .OrderBy(p => p.Name)
                .ToList();

            if (exportLocalizer != null)
            {
                var setting = await _settings.GetSetting();
                foreach (var p in columns)
                {
                    var displayAttribute = p.GetCustomAttribute<DisplayAttribute>();
                    var label = displayAttribute?.Name ?? p.Name;

                    if (p.Name != label && label.StartsWith("Label"))
                    {
                        label = await GetLocalizedLabel(label, setting);
                        result.Add(new ExportSettingViewModel { PropertyName = p.Name, Label = label });
                    }
                    else
                    {
                        result.Add(new ExportSettingViewModel { PropertyName = p.Name, Label = exportLocalizer[label] });
                    }
                }

            }
            else
            {
                columns.ForEach(p =>
                {
                    result.Add(new ExportSettingViewModel { PropertyName = p.Name, Label = (p.GetCustomAttribute(typeof(DisplayAttribute)) as DisplayAttribute).Name });
                });
            }

            return result;
        }

        public async Task<bool> SettingHasUserDefault(string settingName, string screenCode, string userId)
        {
            var defaults = new List<string>();
            CPiUserSetting userSettings;

            var cpiSetting = await _userSettingsService.GetSettingByNameAsync(settingName);
            if (cpiSetting == null)
            {
                cpiSetting = new CPiSetting { Name = settingName, Policy = "*" };
                await _userSettingsService.AddSettingAsync(cpiSetting);
            }
            userSettings = await _userSettingsService.GetUserSettingsAsync(userId, cpiSetting.Id);
            if (userSettings != null)
            {
                defaults = JsonConvert.DeserializeObject<List<string>>(userSettings.Settings);
                return defaults.Any(d => screenCode == d);
            }
            return false;
        }

        public async Task UpdateUserDefaultSetting(string settingName, string screenCode, bool isDefault, string userId)
        {
            var defaults = new List<string>();
            CPiUserSetting userSettings;

            var cpiSetting = await _userSettingsService.GetSettingByNameAsync(settingName);
            if (cpiSetting != null)
            {
                userSettings = await _userSettingsService.GetUserSettingsAsync(userId, cpiSetting.Id);
                if (userSettings != null)
                {
                    defaults = JsonConvert.DeserializeObject<List<string>>(userSettings.Settings);
                    if (isDefault)
                    {
                        if (!defaults.Any(d => screenCode == d))
                        {
                            defaults.Add(screenCode);
                        }
                    }
                    else
                    {
                        defaults.Remove(screenCode);
                    }
                    userSettings.Settings = JsonConvert.SerializeObject(defaults);
                    await _userSettingsService.UpdateUserSettingsAsync(userSettings);
                }
                else
                {
                    defaults.Add(screenCode);
                    userSettings = new CPiUserSetting();
                    userSettings.UserId = userId;
                    userSettings.SettingId = cpiSetting.Id;
                    userSettings.Settings = JsonConvert.SerializeObject(defaults);
                    await _userSettingsService.AddUserSettingsAsync(userSettings);
                }
            }
        }


        public async Task<bool> ExportScreenHasUserDefault(string screenCode, string userId)
        {
            var defaults = new List<string>();
            CPiUserSetting userSettings;

            var cpiSetting = await _userSettingsService.GetSettingByNameAsync(_exportScreenHasDefaultSettingName);
            if (cpiSetting == null)
            {
                cpiSetting = new CPiSetting { Name = _exportScreenHasDefaultSettingName, Policy = "*" };
                await _userSettingsService.AddSettingAsync(cpiSetting);
            }
            userSettings = await _userSettingsService.GetUserSettingsAsync(userId, cpiSetting.Id);
            if (userSettings != null)
            {
                defaults = JsonConvert.DeserializeObject<List<string>>(userSettings.Settings);
                return defaults.Any(d => screenCode == d);
            }
            return false;
        }

        public async Task ExportScreenUpdateUserDefaultSetting(string screenCode, bool isDefault, string userId)
        {
            var defaults = new List<string>();
            CPiUserSetting userSettings;

            var cpiSetting = await _userSettingsService.GetSettingByNameAsync(_exportScreenHasDefaultSettingName);
            if (cpiSetting != null)
            {
                userSettings = await _userSettingsService.GetUserSettingsAsync(userId, cpiSetting.Id);
                if (userSettings != null)
                {
                    defaults = JsonConvert.DeserializeObject<List<string>>(userSettings.Settings);
                    if (isDefault)
                    {
                        if (!defaults.Any(d => screenCode == d))
                        {
                            defaults.Add(screenCode);
                        }
                    }
                    else
                    {
                        defaults.Remove(screenCode);
                    }
                    userSettings.Settings = JsonConvert.SerializeObject(defaults);
                    await _userSettingsService.UpdateUserSettingsAsync(userSettings);
                }
                else
                {
                    defaults.Add(screenCode);
                    userSettings = new CPiUserSetting();
                    userSettings.UserId = userId;
                    userSettings.SettingId = cpiSetting.Id;
                    userSettings.Settings = JsonConvert.SerializeObject(defaults);
                    await _userSettingsService.AddUserSettingsAsync(userSettings);
                }
            }
        }

        public async Task<List<string>> GetUserExportDefaultProperties(string exportSettingName, List<string> defaults, string userId)
        {
            CPiUserSetting userSettings;
            var cpiSetting = await _userSettingsService.GetSettingByNameAsync(exportSettingName);
            if (cpiSetting == null)
            {
                cpiSetting = new CPiSetting { Name = exportSettingName, Policy = "*" };
                await _userSettingsService.AddSettingAsync(cpiSetting);

            }
            if (cpiSetting.Id > 0)
            {
                userSettings = await _userSettingsService.GetUserSettingsAsync(userId, cpiSetting.Id);
                if (userSettings != null)
                {
                    if (!string.IsNullOrEmpty(userSettings.Settings))
                    {
                        defaults = JsonConvert.DeserializeObject<List<string>>(userSettings.Settings);
                    }
                }
                else
                {
                    userSettings = new CPiUserSetting();
                    userSettings.UserId = userId;
                    userSettings.SettingId = cpiSetting.Id;
                    userSettings.Settings = JsonConvert.SerializeObject(defaults);
                    await _userSettingsService.AddUserSettingsAsync(userSettings);
                }
            }
            return defaults;
        }

        public async Task UpdateUserExportSetting(string exportSettingName, ExportSettingViewModel exportSetting, string userId)
        {
            var defaults = new List<string>();
            CPiUserSetting userSettings;

            var cpiSetting = await _userSettingsService.GetSettingByNameAsync(exportSettingName);
            if (cpiSetting.Id > 0)
            {
                userSettings = await _userSettingsService.GetUserSettingsAsync(userId, cpiSetting.Id);
                if (userSettings != null)
                {
                    if (!string.IsNullOrEmpty(userSettings.Settings))
                    {
                        defaults = JsonConvert.DeserializeObject<List<string>>(userSettings.Settings);
                        if (exportSetting.Include && !defaults.Any(d => d == exportSetting.PropertyName))
                        {
                            defaults.Add(exportSetting.PropertyName);
                        }
                        else if (!exportSetting.Include)
                        {
                            defaults.Remove(exportSetting.PropertyName);
                        }
                        userSettings.Settings = JsonConvert.SerializeObject(defaults);
                        await _userSettingsService.UpdateUserSettingsAsync(userSettings);
                    }
                }
            }
        }

        public async Task UpdateUserExportSettings(string exportSettingName, List<ExportSettingViewModel> exportSettings, string userId)
        {
            var defaults = new List<string>();
            CPiUserSetting userSettings;

            var cpiSetting = await _userSettingsService.GetSettingByNameAsync(exportSettingName);
            if (cpiSetting.Id > 0)
            {
                userSettings = await _userSettingsService.GetUserSettingsAsync(userId, cpiSetting.Id);
                if (userSettings != null)
                {
                    if (!string.IsNullOrEmpty(userSettings.Settings))
                    {
                        defaults = JsonConvert.DeserializeObject<List<string>>(userSettings.Settings);
                        foreach (var exportSetting in exportSettings)
                        {
                            if (exportSetting.Include && !defaults.Any(d => d == exportSetting.PropertyName))
                            {
                                defaults.Add(exportSetting.PropertyName);
                            }
                            else if (!exportSetting.Include)
                            {
                                defaults.Remove(exportSetting.PropertyName);
                            }
                        }
                        userSettings.Settings = JsonConvert.SerializeObject(defaults);
                        await _userSettingsService.UpdateUserSettingsAsync(userSettings);
                    }
                }
            }
        }


        private async Task<string> GetLocalizedLabel(string label, DefaultSetting setting)
        {
            var optLabel = await _settings.GetValue<string>("General", label);
            return _localizer[optLabel ?? label];

            //switch (label)
            //{
            //    case "LabelCaseNumber":
            //        return _localizer[setting.LabelCaseNumber];
            //    case "LabelClientMatter":
            //        return _localizer[setting.LabelClientMatter];
            //    case "LabelAgent":
            //        return _localizer[setting.LabelAgent];
            //    case "LabelClient":
            //        return _localizer[setting.LabelClient];
            //    case "LabelOwner":
            //        return _localizer[setting.LabelOwner];
            //    case "LabelClientRef":
            //        return _localizer[setting.LabelClientRef];
            //    default:
            //        return label;
            //}
        }

        private MemoryStream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
        #endregion

        #region PowerPoint
        public async Task<MemoryStream> DataToPowerPointMemoryStream(string contentType, string base64, string fileName)
        {
            var templatePath = System.IO.Path.Combine(_hostingEnvironment.ContentRootPath, @"Resources", "WidgetTemplate.pptx");

            using (var templateFile = System.IO.File.Open(templatePath, FileMode.Open, FileAccess.ReadWrite))
            {
                //do not use "using" to avoid returning the IDisposable
                var stream = new MemoryStream();
                templateFile.CopyTo(stream);

                var imgBytes = Convert.FromBase64String(base64);
                var widgetTitle = fileName.Split("|")[0];
                var widgetType = fileName.Split("|")[1];

                using (PresentationDocument myPres = PresentationDocument.Open(stream, true))
                {
                    // Get the first slide, using the GetFirstSlide method.
                    SlidePart slidePart1 = GetFirstSlide(myPres);

                    UpdateSlideTitle(ref slidePart1, widgetTitle);
                    AddImagePart(ref slidePart1, ref imgBytes, widgetType);

                    myPres.PresentationPart.Presentation.Save();
                }

                stream.Seek(0, SeekOrigin.Begin);//scroll to stream start point

                //return File(stream.ToArray(), ImageHelper.GetContentType(".pptx"), widgetTitle + ".pptx");
                return stream;
            }
        }

        public static SlidePart GetFirstSlide(PresentationDocument presentationDocument)
        {
            // Get relationship ID of the first slide
            PresentationPart part = presentationDocument.PresentationPart;
            SlideId slideId = part.Presentation.SlideIdList.GetFirstChild<SlideId>();
            string relId = slideId.RelationshipId;

            // Get the slide part by the relationship ID.
            SlidePart slidePart = (SlidePart)part.GetPartById(relId);

            return slidePart;
        }

        public static void UpdateSlideTitle(ref SlidePart slidePart, string widgetTitle)
        {
            if (slidePart.Slide != null)
            {
                foreach (var paragraph in slidePart.Slide.Descendants<DocumentFormat.OpenXml.Drawing.Paragraph>())
                {
                    foreach (var text in paragraph.Descendants<DocumentFormat.OpenXml.Drawing.Text>())
                    {
                        if (text.Text == "[[Title]]") text.Text = widgetTitle;
                    }
                }
            }
        }

        public static void AddImagePart(ref SlidePart slidePart, ref byte[] imageBytes, string widgetType)
        {
            long oX = 2325000, oY = 1180000, eX = 4500000, eY = 4500000;
            if (widgetType == "map") { oX = 0; oY = 1180000; eX = 9144000L; eY = 4858000L; }

            ImagePart ip = slidePart.AddImagePart(ImagePartType.Jpeg, "rId2");
            Stream imgStream = new MemoryStream(imageBytes);
            imgStream.Position = 0;
            ip.FeedData(imgStream);

            P.Picture picture = new P.Picture
                        (
                            new P.NonVisualPictureProperties
                            (
                                new P.NonVisualDrawingProperties() { Id = (UInt32Value)1026U, Name = "Photo", Description = "" },
                                new P.NonVisualPictureDrawingProperties
                                (
                                    new D.PictureLocks() { NoChangeAspect = true }
                                ),
                                new ApplicationNonVisualDrawingProperties()
                            ),
                            new P.BlipFill
                            (
                                new D.Blip
                                    (
                                        new D.NonVisualPicturePropertiesExtensionList()
                                    )
                                { Embed = "rId2" },
                                new D.Stretch
                                (
                                    new D.FillRectangle()
                                )
                            ),
                            new P.ShapeProperties
                            (
                                new D.Transform2D
                                (
                                    new D.Offset() { X = oX, Y = oY },
                                    new D.Extents() { Cx = eX, Cy = eY }
                                ),
                                new D.PresetGeometry
                                (
                                    new D.AdjustValueList()
                                )
                                { Preset = D.ShapeTypeValues.Rectangle }
                            )
                        );

            slidePart.Slide.CommonSlideData.ShapeTree.AppendChild<P.Picture>(picture);
        }
        #endregion
    }
}
