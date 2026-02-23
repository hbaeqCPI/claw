using Microsoft.Extensions.Configuration;
using Azure;
using Azure.AI.FormRecognizer;
using Azure.AI.FormRecognizer.Models;
using R10.Core.Entities.Shared;
using R10.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using R10.Core.DTOs;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using Newtonsoft.Json;
using DocumentFormat.OpenXml.Drawing.Charts;
using System.Linq;

namespace R10.Web.Services.FormExtract
{
    public class AzureFormRecognizer
    {
        protected readonly ISystemSettings<DefaultSetting> _settings;
        protected readonly IConfiguration _configuration;
        const string apiKeySection = "FormRecognizer:ApiKey";

        public AzureFormRecognizer(
                    ISystemSettings<DefaultSetting> settings,
                   IConfiguration configuration)
        {
            _settings = settings;
            _configuration = configuration;
        }

        public async Task<List<FormExtractDTO>> AnalyzeFormFile(string modelId, string filePath, List<string> pages)
        {
            var extractedFields = new List<FormExtractDTO>();

            try
            {
                var settings = _settings.GetSetting().GetAwaiter().GetResult();

                var apiUrl = string.Format(settings.FormRecognizerUrl, settings.FormRecognizerServiceName);
                var apiKey = _configuration.GetSection(apiKeySection).Get<string>();
                var credential = new AzureKeyCredential(apiKey);
                var client = new DocumentAnalysisClient(new Uri(apiUrl), credential);
                var options = new AnalyzeDocumentOptions();
                pages.ForEach(p => options.Pages.Add(p));


                var stream = new FileStream(filePath, FileMode.Open);
                var operation = await client.AnalyzeDocumentAsync(WaitUntil.Completed, modelId, stream, options);
                AnalyzeResult result = operation.Value;

                foreach (var doc in result.Documents)
                {
                    foreach (var field in doc.Fields)
                    {
                        var row = new FormExtractDTO() { FieldName = field.Key };
                        if (field.Value != null) {
                            
                            if (field.Value.FieldType == DocumentFieldType.Dictionary) {
                                var fieldValue = field.Value.Value;
                                var fieldValueDict = fieldValue.AsDictionary();

                                var fieldValueList = new List<string>();
                                foreach (var item in fieldValueDict) {
                                    var fieldValueFinal = "";

                                    if (item.Value.FieldType == DocumentFieldType.Dictionary)
                                    {
                                        var fieldValueDict2 = item.Value.Value.AsDictionary();
                                        foreach (var item2 in fieldValueDict2) {
                                            if (item2.Value.FieldType == DocumentFieldType.String) {
                                                var data = $"\"{item2.Key}\":\"{item2.Value.Content.Replace("\"","~")}\"";
                                                fieldValueFinal += (string.IsNullOrEmpty(fieldValueFinal) ? "" : ",") + data;
                                            }
                                        }
                                        fieldValueFinal = $"{{{fieldValueFinal}}}";
                                    }
                                    else
                                        fieldValueFinal = item.Value.Content;

                                    if (fieldValueFinal !="{}")
                                       fieldValueList.Add(fieldValueFinal);
                                }
                                var jsonList = $"{{\"Data\":[{string.Join(",", fieldValueList)}]}}";
                                if (!fieldValueList.Any())
                                    jsonList = "";

                                row.FieldData = jsonList;
                            }
                            
                            else if (field.Value.FieldType == DocumentFieldType.List)
                            {
                                var fieldValue = field.Value.Value;
                                var fieldValueDocList = fieldValue.AsList();

                                var fieldValueList = new List<string>();
                                foreach (var item in fieldValueDocList)
                                {
                                    var fieldValueFinal = "";

                                    if (item.FieldType == DocumentFieldType.Dictionary)
                                    {
                                        var fieldValueDict2 = item.Value.AsDictionary();
                                        foreach (var item2 in fieldValueDict2)
                                        {
                                            if (item2.Value.FieldType == DocumentFieldType.String || item2.Value.FieldType == DocumentFieldType.Date)
                                            {
                                                var data = $"\"{item2.Key}\":\"{item2.Value.Content.Replace("\"", "~")}\"";
                                                fieldValueFinal += (string.IsNullOrEmpty(fieldValueFinal) ? "" : ",") + data;
                                            }
                                            
                                        }
                                        fieldValueFinal = $"{{{fieldValueFinal}}}";
                                    }
                                    else
                                        fieldValueFinal = item.Content;

                                    if (fieldValueFinal != "{}")
                                        fieldValueList.Add(fieldValueFinal);
                                }
                                var jsonList = $"{{\"Data\":[{string.Join(",", fieldValueList)}]}}";
                                if (!fieldValueList.Any())
                                    jsonList = "";

                                row.FieldData = jsonList;
                            }

                            else
                              row.FieldData = field.Value.Content;

                            if (field.Value.Confidence !=null)
                               row.Confidence = (double)field.Value.Confidence;
                            extractedFields.Add(row);
                        }
                    }
                }
                return extractedFields;
            }
            catch (Exception ex)
            {

                //throw;
                return extractedFields;
            }
            
        }

        //public async Task<List<FormExtractDTO>> AnalyzeFormFile(string modelId, string filePath, List<string> pages )
        //{
        //    var settings = _settings.GetSetting().GetAwaiter().GetResult();

        //    var apiUrl = string.Format(settings.FormRecognizerUrl, settings.FormRecognizerServiceName);
        //    var apiKey = _configuration.GetSection(apiKeySection).Get<string>();
        //    var credential = new AzureKeyCredential(apiKey);
        //    var client = new FormRecognizerClient(new Uri(apiUrl), credential);
        //    var options = new RecognizeCustomFormsOptions() { IncludeFieldElements = false, ContentType = FormContentType.Pdf };
        //    pages.ForEach(p => options.Pages.Add(p));

        //    //Uri formUri = new Uri(filePath);
        //    //RecognizeCustomFormsOperation operation = await client.StartRecognizeCustomFormsFromUriAsync(modelId, formUri, options);

        //    var stream = new FileStream(filePath, FileMode.Open);
        //    RecognizeCustomFormsOperation operation = await client.StartRecognizeCustomFormsAsync(modelId, stream, options);
        //    Response<RecognizedFormCollection> operationResponse = await operation.WaitForCompletionAsync();
        //    RecognizedFormCollection forms = operationResponse.Value;

        //    var extractedFields = new List<FormExtractDTO>();
        //    foreach (RecognizedForm form in forms)
        //    { 
        //        foreach(FormField field in form.Fields.Values)
        //        {
        //            var row = new FormExtractDTO() { FieldName = field.Name };
        //            if (field.ValueData != null)
        //                row.FieldData = field.ValueData;
        //            row.Confidence = (double)field.Confidence;
        //            extractedFields.Add(row);
        //        }
        //    }

        //    return extractedFields;
        //}
    }
}

/*

 public static async System.Threading.Tasks.Task RecogStreamAsync() {
           
            var credential = new AzureKeyCredential(apiKey);
            var client = new FormRecognizerClient(new Uri(endpoint), credential);

            var stream = new FileStream(filePath, FileMode.Open);
            var pages = new List<string>() { "1", "2" };

            var options = new RecognizeCustomFormsOptions() { IncludeFieldElements = false, ContentType = FormContentType.Pdf, Pages = { "1", "2" } };

            RecognizeCustomFormsOperation operation = await client.StartRecognizeCustomFormsAsync(modelId, stream, options);
            Response<RecognizedFormCollection> operationResponse = await operation.WaitForCompletionAsync();
            RecognizedFormCollection forms = operationResponse.Value;

            foreach (RecognizedForm form in forms)
            {
                Console.WriteLine($"Form of type: {form.FormType}");
                Console.WriteLine($"Form was analyzed with model with ID: {form.ModelId}");
                foreach (FormField field in form.Fields.Values)
                {
                    Console.WriteLine($"Field '{field.Name}': ");

                    if (field.LabelData != null)
                    {
                        Console.WriteLine($"  Label: '{field.LabelData.Text}'");
                    }

                    if (field.ValueData != null)
                    {
                        Console.WriteLine($"  Value: '{field.ValueData.Text}'");
                    }
                    Console.WriteLine($"  Confidence: '{field.Confidence}'");
                }

                // Iterate over tables, lines, and selection marks on each page
                foreach (var page in form.Pages)
                {
                    for (int i = 0; i < page.Tables.Count; i++)
                    {
                        Console.WriteLine($"Table {i + 1} on page {page.Tables[i].PageNumber}");
                        foreach (var cell in page.Tables[i].Cells)
                        {
                            Console.WriteLine($"  Cell[{cell.RowIndex}][{cell.ColumnIndex}] has text '{cell.Text}' with confidence {cell.Confidence}");
                        }
                    }
                    Console.WriteLine($"Lines found on page {page.PageNumber}");
                    foreach (var line in page.Lines)
                    {
                        Console.WriteLine($"  Line {line.Text}");
                    }

                    if (page.SelectionMarks.Count != 0)
                    {
                        Console.WriteLine($"Selection marks found on page {page.PageNumber}");
                        foreach (var selectionMark in page.SelectionMarks)
                        {
                            Console.WriteLine($"  Selection mark is '{selectionMark.State}' with confidence {selectionMark.Confidence}  left pos: {selectionMark.BoundingBox[0].X}, {selectionMark.BoundingBox[0].Y} ");
                        }
                    }
                }
            }

        } 

*/
