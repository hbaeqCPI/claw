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
using System.Drawing.Imaging;

namespace R10.Web.Helpers
{
    public class LetterGenerationHelper
    {

        public readonly string LetterTemplateBaseFolder = @"UserFiles\Letters\Templates";
        public readonly string LetterLogBaseFolder = @"UserFiles\Logs\Letters";

        //private readonly string[] _imageTypes = { "bmp", "gif", "jpeg", "jpg", "png", "tiff" };            // SVG excluded because of resize issue: https://github.com/vvvv/SVG/issues/173#event-386302206
        private readonly string _imageBaseFolder = @"UserFiles\Images";
        private string _imagePath;


        ////    templateFileName = Path.Combine(rootPath, LetterTemplateBaseFolder, systemFolder, templateFileName);
        ///
        //public MemoryStream MergeLetters(string letterTemplateFolder, string systemType, string templateFileName, DataSet sourceDataSet, bool hasImage = false)
        //{
        //    string systemFolder = GetSystemName(systemType);
        //    templateFileName = Path.Combine(letterTemplateFolder, systemFolder, templateFileName);

        //    return MergeLetterDataSet(templateFileName, sourceDataSet, hasImage, imageFolder);
        //}

        public MemoryStream MergeLetterDataSet(MemoryStream ms, DataSet sourceDataSet, bool hasImage, string templateFileExtension)
        {

            if (hasImage)
            {
                var factory = Configuration.Builder                                         // use this construct to allow for plugin
                   .Include(ImageLoader)
                   .Include(ImageMaxSize)
                   .Build();
                using (var doc = factory.Open(ms, templateFileExtension))
                {
                    doc.Process(sourceDataSet);
                }
            }
            else
            {
                var factory = Configuration.Factory;
                using (var doc = factory.Open(ms, templateFileExtension))
                {
                    doc.Process(sourceDataSet);
                }
            }

            ms.Position = 0;
            return ms;
        }

        //public MemoryStream MergeLetters(string rootPath, string systemType, string templateFileName, DataSet sourceDataSet, bool hasImage = false)
        //{
        //    string systemFolder = GetSystemName(systemType);
        //    string imageFolder = hasImage ? Path.Combine(rootPath, _imageBaseFolder, systemFolder) : "";

        //    templateFileName = Path.Combine(rootPath, LetterTemplateBaseFolder, systemFolder, templateFileName);

        //    return MergeLetterDataSet(templateFileName, sourceDataSet, hasImage, imageFolder);
        //}

        //public MemoryStream MergeLetterDataSet(string templateFileName, DataSet sourceDataSet, bool hasImage = false, string imageFolder = "")
        //{
        //    var templateFile = new FileInfo(templateFileName);

        //    var ms = new System.IO.MemoryStream();
        //    var bytes = System.IO.File.ReadAllBytes(templateFile.FullName);
        //    ms.Write(bytes, 0, bytes.Length);
        //    ms.Position = 0;

        //    if (hasImage)
        //    {
        //        //_imagePath = imageFolder + (imageFolder.EndsWith("/") ? "" : "/");          // _imagePath will be used by ImageLoader plugin
        //        var factory = Configuration.Builder                                         // use this construct to allow for plugin
        //           .Include(ImageLoader)
        //           .Include(ImageMaxSize)
        //           .Build();
        //        using (var doc = factory.Open(ms, templateFile.Extension))
        //        {
        //            doc.Process(sourceDataSet);
        //        }
        //    }
        //    else
        //    {
        //        var factory = Configuration.Factory;
        //        using (var doc = factory.Open(ms, templateFile.Extension))
        //        {
        //            doc.Process(sourceDataSet);
        //        }
        //    }

        //    ms.Position = 0;
        //    return ms;
        //}

        public MemoryStream ProcessWithImage<T>(string templateFileName, List<T> sourceData, string imageFolder)
        {
            var templateFile = new FileInfo(templateFileName);
            _imagePath = imageFolder + (imageFolder.EndsWith("/") ? "" : "/");

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

            switch (ext)
            {
                case ".docx":
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

        public string GetFileName(string systemType, string templateFile, string userName)
        {
            return $"{systemType}-{DateTime.Now:yyyy-MM-dd-hhmmsstt}-{userName}{Path.GetExtension(templateFile)}";
        }

        public string BuildSharePointLogFileName(string templateFile)
        {
            var file = templateFile.Split(".");
            var fileName = $"{file[0]}-{DateTime.Now:yyyy-MM-dd-hhmmsstt}{Path.GetExtension(templateFile)}";
            return fileName;
        }

        public string GetLogFolder(string rootPath, string systemType)
        {
            return Path.Combine(rootPath, LetterLogBaseFolder, GetSystemName(systemType));

        }

        private string GetSystemName(string systemType)
        {
            return systemType == "P" ? "Patent" : systemType == "T" ? "Trademark" : systemType == "G" ? "GeneralMatter" : "";
        }

        public string GetTemplateFolder(string rootPath, string systemType)
        {
            var templateFolder = Path.Combine(rootPath, LetterTemplateBaseFolder, GetSystemName(systemType));
            return templateFolder;
        }

        public string GetTemplateFolderRelative(string rootPath, string systemType)
        {
            var templateFolder = LetterTemplateBaseFolder.Replace("\\", "/") + "/" + GetSystemName(systemType);
            return templateFolder;
        }

        public string GetTemplateFilePath(string rootPath, string systemType, string letterFileName)
        {
            var templateFilePath = Path.Combine(rootPath, LetterTemplateBaseFolder, GetSystemName(systemType), letterFileName);
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

        public static bool IsBmpFile(byte[] data)
        {
            // BMP files start with the ASCII characters "BM" (42 4D in hexadecimal)
            return data != null && data.Length > 2 && data[0] == 0x42 && data[1] == 0x4D;
        }

        public static byte[] ConvertBmpToPng(byte[] bmpData)
        {
            if (!IsBmpFile(bmpData))
            {
                throw new InvalidOperationException("The provided data is not a BMP file.");
            }

            using (MemoryStream bmpStream = new MemoryStream(bmpData))
            using (Image bmpImage = Image.FromStream(bmpStream))
            using (MemoryStream pngStream = new MemoryStream())
            {
                // Save the image as PNG
                bmpImage.Save(pngStream, ImageFormat.Png);
                return pngStream.ToArray();
            }
        }

        private object ImageLoader(object value, string metadata)
        {
            // this plugin can be used to convert string into an Image type which Templater recognizes
            // from-resource should be added as metadata in the template document image field
            // for example, suppose file of image is in field "ImageFile", and maxSize is 2, this is the merge field in the template file: [[ImageFile]:from-resource:maxSize(2)]
            //if (metadata == "from-resource" && value.GetType() == typeof(byte[]))
            if (value.GetType() == typeof(byte[]))
            {
                byte[] bytes = (byte[])value;
                if (IsBmpFile(bytes))
                    bytes = ConvertBmpToPng(bytes);
                var image = Image.FromStream(new MemoryStream(bytes));
                return image;
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
        //public static System.IO.MemoryStream Process(System.IO.FileInfo templateFile, string letterData)
        //{
        //    var ms = new System.IO.MemoryStream();
        //    var bytes = System.IO.File.ReadAllBytes(templateFile.FullName);
        //    ms.Write(bytes, 0, bytes.Length);
        //    ms.Position = 0;
        //    var factory = Configuration.Factory;
        //    using (var doc = factory.Open(ms, templateFile.Extension))
        //    {
        //        if (letterData.TrimStart().StartsWith("["))
        //            doc.Process(Newtonsoft.Deserialize<IDictionary<string, object>[]>(new JsonTextReader(new StringReader(letterData))));
        //        else
        //            doc.Process(Newtonsoft.Deserialize<IDictionary<string, object>>(new JsonTextReader(new StringReader(letterData))));
        //    }
        //    ms.Position = 0;
        //    return ms;
        //}
    }
}
