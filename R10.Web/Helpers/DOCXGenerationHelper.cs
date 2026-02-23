using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NGS.Templater;
using System.Drawing;
using Kendo.Mvc.UI;
using DocumentFormat.OpenXml.ExtendedProperties;
using DocumentFormat.OpenXml.Bibliography;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace R10.Web.Helpers
{
    public class DOCXGenerationHelper
    {
        //private readonly JsonSerializer Newtonsoft;                                             // used for JSON data processing (we are not using this type of data source)
        //private readonly IDocumentFactory Factory = Configuration.Factory;

        public readonly string DOCXTemplateBaseFolder = @"UserFiles\DOCXes\Templates";
        public readonly string DOCXLogBaseFolder = @"UserFiles\Logs\DOCXes";

        //private readonly string[] _imageTypes = { "bmp", "gif", "jpeg", "jpg", "png", "tiff" };            // SVG excluded because of resize issue: https://github.com/vvvv/SVG/issues/173#event-386302206
        private readonly string _imageBaseFolder = @"UserFiles\Images";
        private string _imagePath;

        /*
        public DOCXGenerationHelper()
        {
            // below used for JSON data processing (we are not using this type of data source)
            //Newtonsoft = new JsonSerializer();
            //Newtonsoft.Culture = System.Globalization.CultureInfo.InvariantCulture;
            //Newtonsoft.TypeNameHandling = TypeNameHandling.None;
            //Newtonsoft.Converters.Add(new DOCXDictionaryConverter());
        }
        */

        public MemoryStream MergeDOCXes(string rootPath, string systemType, string templateFileName, DataSet sourceDataSet, bool hasImage = false)
        {
            string systemFolder = GetSystemName(systemType);
            string imageFolder = hasImage ? Path.Combine(rootPath, _imageBaseFolder, systemFolder) : "";

            templateFileName = Path.Combine(rootPath, DOCXTemplateBaseFolder, systemFolder, templateFileName);

            return MergeDOCXDataSet(templateFileName, sourceDataSet, hasImage, imageFolder);
        }

        public MemoryStream MergeDOCXDataSet(string templateFileName, DataSet sourceDataSet, bool hasImage = false, string imageFolder = "")
        {
            var templateFile = new FileInfo(templateFileName);
            
            var ms = new System.IO.MemoryStream();
            var bytes = System.IO.File.ReadAllBytes(templateFile.FullName);
            ms.Write(bytes, 0, bytes.Length);
            ms.Position = 0;

            if (hasImage)
            {
                _imagePath = imageFolder + (imageFolder.EndsWith("/") ? "" : "/");          // _imagePath will be used by ImageLoader plugin
                var factory = Configuration.Builder                                         // use this construct to allow for plugin
                   .Include(ImageLoader)
                   .Include(ImageMaxSize)
                   .Build();
                using (var doc = factory.Open(ms, templateFile.Extension))
                {
                    doc.Process(sourceDataSet);
                }
            }
            else
            {
                var factory = Configuration.Factory;
                using (var doc = factory.Open(ms, templateFile.Extension))
                {
                    doc.Process(sourceDataSet);
                }
            }
           
            ms.Position = 0;
            return ms;
        }

        public MemoryStream ProcessWithImage<T>(string templateFileName, List<T> sourceData, string imageFolder)
        {
            var templateFile = new FileInfo(templateFileName);
            _imagePath = imageFolder + (imageFolder.EndsWith("/") ? "" : "/") ;

            var ms = new System.IO.MemoryStream();
            var bytes = System.IO.File.ReadAllBytes(templateFile.FullName);
            ms.Write(bytes, 0, bytes.Length);
            ms.Position = 0;

            var factory = Configuration.Builder
                   .Include(ImageLoader)
                   .Include(ImageMaxSize)
                   .Build();
            using (var doc = factory.Open(ms, templateFile.Extension))
            {
                doc.Process(sourceData);
            }
            ms.Position = 0;
            return ms;
        }
        
        public string MimeType(string fileName)
        {
            var ext = Path.GetExtension(fileName).ToLower();
            string mimeType;

            switch(ext)
            {
                case ".docx" : 
                    mimeType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                    break;
                case ".xlsx":
                    mimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    break;
                default:
                    mimeType = "";
                    break;
            }
            return mimeType;    
        }

        public string GetFileName (string systemType, string templateFile, string userName)
        {
            return $"{systemType}-{DateTime.Now:yyyy-MM-dd-hhmmsstt}-{userName}{Path.GetExtension(templateFile)}";
        }

        public string GetLogFolder(string rootPath, string systemType)
        {
            return Path.Combine(rootPath, DOCXLogBaseFolder, GetSystemName(systemType));

        }

        private string GetSystemName (string systemType)
        {
            return systemType == "P" ? "Patent" : systemType == "T" ? "Trademark" : systemType == "G" ? "GeneralMatter" : "";
        }

        public string GetTemplateFolder(string rootPath, string systemType)
        {
            var templateFolder = Path.Combine(rootPath, DOCXTemplateBaseFolder, GetSystemName(systemType));
            return templateFolder;
        }

        public string GetTemplateFolderRelative(string rootPath, string systemType)
        {
            var templateFolder = DOCXTemplateBaseFolder.Replace("\\", "/") + "/" + GetSystemName(systemType);
            return templateFolder;
        }

        public string GetTemplateFilePath (string rootPath, string systemType, string docxFileName)
        {
            var templateFilePath = Path.Combine(rootPath, DOCXTemplateBaseFolder, GetSystemName(systemType), docxFileName);
            return templateFilePath;
        }

        public bool ExistTemplateFile(string filePath)
        {
            return File.Exists(filePath);
        }
        public async Task<bool> UploadTemplateFile(string filePath, IFormFile uploadedFile)
        {
            if (ExistTemplateFile(filePath))
                File.Delete(filePath);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await uploadedFile.CopyToAsync(fileStream);
            }
            return true;
        }

        public bool DeleteTemplateFile(string filePath)
        {
            if (ExistTemplateFile(filePath))
            {
                File.Delete(filePath);
                return true;
            }
            return false;
        }

        #region Image Plugin
        //private object ImageLoader(object value, string metadata)
        //{
        //    // this plugin can be used to convert string into an Image type which Templater recognizes
        //    // from-resource should be added as metadata in the template document image field
        //    // for example, suppose file of image is in field "ImageFile", and maxSize is 2, this is the merge field in the template file: [[ImageFile]:from-resource:maxSize(2)]
        //    if (metadata == "from-resource" && value is string)
        //    {
        //        var strValue = (string)value;
        //        if (_imageTypes.Any(x => strValue.ToLower().EndsWith(x)))
        //            return Image.FromFile(_imagePath + value);
        //        else
        //            return string.Empty;
        //    }

        //    return value;
        //}

        private object ImageLoader(object value, string metadata)
        {
            // this plugin can be used to convert string into an Image type which Templater recognizes
            // from-resource should be added as metadata in the template document image field
            // for example, suppose file of image is in field "ImageFile", and maxSize is 2, this is the merge field in the template file: [[ImageFile]:from-resource:maxSize(2)]
            if (metadata == "from-resource" && value.GetType() == typeof(byte[]))
            {
                byte[] bytes = (byte[])value;
                return Image.FromStream(new MemoryStream(bytes));
            }
            return value;
        }

        private object ImageMaxSize(object value, string metadata)
        {
            // for example, suppose file name of image is in field "ImageFile", and maxSize is 2, this is the merge field in the template file: [[ImageFile]:from-resource:maxSize(2)]
            var bmp = value as Bitmap;
            if (metadata.StartsWith("maxSize(") && bmp != null)
            {
                var parts = metadata.Substring(8, metadata.Length - 9).Split(',');
                var maxWidth = int.Parse(parts[0].Trim()) * 28;
                var maxHeight = int.Parse(parts[parts.Length - 1].Trim()) * 28;
                if (bmp.Width > 0 && maxWidth > 0 && bmp.Width > maxWidth || bmp.Height > 0 && maxHeight > 0 && bmp.Height > maxHeight)
                {
                    var widthScale = 1f * bmp.Width / maxWidth;
                    var heightScale = 1f * bmp.Height / maxHeight;
                    var scale = Math.Max(widthScale, heightScale);
                    //Before passing image for processing it can be manipulated via Templater plugins
                    bmp.SetResolution(bmp.HorizontalResolution * scale, bmp.VerticalResolution * scale);
                }
            }
            return value;
        }

        #endregion

        // below for json data
        //public static System.IO.MemoryStream Process(System.IO.FileInfo templateFile, string docxData)
        //{
        //    var ms = new System.IO.MemoryStream();
        //    var bytes = System.IO.File.ReadAllBytes(templateFile.FullName);
        //    ms.Write(bytes, 0, bytes.Length);
        //    ms.Position = 0;
        //    var factory = Configuration.Factory;
        //    using (var doc = factory.Open(ms, templateFile.Extension))
        //    {
        //        if (docxData.TrimStart().StartsWith("["))
        //            doc.Process(Newtonsoft.Deserialize<IDictionary<string, object>[]>(new JsonTextReader(new StringReader(docxData))));
        //        else
        //            doc.Process(Newtonsoft.Deserialize<IDictionary<string, object>>(new JsonTextReader(new StringReader(docxData))));
        //    }
        //    ms.Position = 0;
        //    return ms;
        //}
    }
}
