using System;
using Microsoft.AspNetCore.Http;
using LawPortal.Core.DTOs;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.StaticFiles;
using LawPortal.Core.Entities;

namespace LawPortal.Web.Helpers
{
    public static class ImageHelper
    {
        private const string imageRootUrl = "~/UserFiles/Searchable/Documents";
        private const string imageRootFolder = @"UserFiles\Searchable\Documents";
        private const string imageThumbnailRootFolder = @"UserFiles\Thumbnails";

        private const string qeLogFolder = @"UserFiles\Searchable\Logs\QuickEmails";
        private const string qeLogAttachmentFolder = @"UserFiles\Logs\QuickEmails";
        private const string letterLogFolder = @"UserFiles\Searchable\Logs\Letters";
        private const string efsLogFolder = @"UserFiles\Searchable\Logs\EFS";
        private const string _documentRootFolder = @"UserFiles\Searchable\Documents";
        private const string _documentThumbnailFolder = @"UserFiles\Thumbnails";
        private const string _calendarFolder = @"UserFiles\Calendar";
        private const string _reportLogFolder = @"UserFiles\ReportLogs";


        public enum CPiSavedFileType { 
            Image,              // 0
            Thumbnail,
            QELoggedImage,
            IDSReferences,
            Letter,
            EFS,
            QE,
            DocMgt,             // 7
            DocMgtThumbnail,
            QELoggedImageThumbnail,
            Assignment,
            Licensees,
            DeDocket,
            Calendar,
            ReportFile
        };

        public static bool IsImageFile(string fileName)
        {
            var extension = GetFileType(fileName).ToLower();
            return extension.Contains("bmp") || extension.Contains("jpeg") || extension.Contains("jpg") ||
                   extension.StartsWith("png") || extension.Contains("gif") || extension.Contains("tiff");
        }

        public static bool IsUrl(string fileName)
        {
            var extension = GetFileType(fileName).ToLower();
            return extension == "www" || extension == "http" || extension == "ftp" || extension == "file";
        }

        public static string GetFileType(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return string.Empty;

            if (fileName.StartsWith("http")) return "http";
            if (fileName.StartsWith("www")) return "www";
            if (fileName.StartsWith("ftp")) return "ftp";
            if (fileName.StartsWith("file")) return "file";
            var extension = Path.GetExtension(fileName);
            var trimChar = ".";
            return extension.TrimStart(trimChar.ToCharArray());
        }

        /// Gets the size, in bytes, of an existing file.
        public static long GetSavedFileSize(string fileName)
        {
            FileInfo file = new FileInfo(fileName);
            return file.Length;
        }

        /// Create a thumbnail in the specified path
        public static void CreateAndSaveThumbnail(string path, string filename)
        {
            var width = 80; //128;
            var height = 80; //128;

            // will cause an error if image source is not accessible
            using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
            using (var image = new Bitmap(fileStream))
            {
                var resized = new Bitmap(width, height);
                using (var graphics = Graphics.FromImage(resized))
                {
                    graphics.CompositingQuality = CompositingQuality.HighSpeed;
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.CompositingMode = CompositingMode.SourceCopy;
                    graphics.DrawImage(image, 0, 0, width, height);
                    resized.Save(filename, GetThumbnailImageFormat(filename));
                }
            }
        }

        private static ImageFormat GetThumbnailImageFormat(string filename)
        {
            var extension = Path.GetExtension(filename).Replace(".", "").ToLower();
            switch(extension)
            {
                case "bmp": return ImageFormat.Bmp;
                case "jpeg": return ImageFormat.Jpeg;
                case "jpg": return ImageFormat.Jpeg;
                default: 
                    return ImageFormat.Png;
            }
        }

        /// Creates a file in the specified path.
        public static async Task SaveFile(IFormFile file, string physicalPath)
        {
            using (var fileStream = new FileStream(physicalPath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }
        }

        /// Copies an existing file to a new file. Delete DestFile if exist. 
        public static void CopyFile(string sourceFile, string destFile)
        {
            if (!File.Exists(sourceFile))
                return;

            if (File.Exists(destFile))
                File.Delete(destFile);

            File.Copy(sourceFile, destFile);
        }

        /// Copies an existing file asynchronously to a new file. Delete DestFile if exist.
        public static async Task CopyFileAsync(string sourceFile, string destFile)
        {
            if (!File.Exists(sourceFile))
                return;

            if (File.Exists(destFile))
                File.Delete(destFile);

            using (Stream source = File.Open(sourceFile, FileMode.Open))
            {
                using (Stream destination = File.Create(destFile))
                {
                    await source.CopyToAsync(destination);
                }
            }
           
        }

        /// Deletes the specified file.
        public static void DeleteFile(string physicalPath)
        {
            if (physicalPath != null)
            {
                if (File.Exists(physicalPath))
                {
                    File.Delete(physicalPath);
                }
            }
        }

        /// Determines whether the specified file exists.
        public static bool IsFileExists(string physicalPath)
        {
            if (physicalPath != null)
                return File.Exists(physicalPath);
            return false;
        }

        /// Determines whether the specified file exists.
        public static bool IsFileExists(string system, string fileName)
        {
            return IsFileExists(GetPhysicalFilePath(system, fileName, CPiSavedFileType.Image));
        }

        /// Returns content type base on the file extension of the given path
        public static string GetContentType(string fileName)
        {

            string contentType;
            var _contentTypeProvider = new FileExtensionContentTypeProvider();
            if (!_contentTypeProvider.TryGetContentType(fileName, out contentType))
            {
                var ext = Path.GetExtension(fileName).Replace(".", "").ToLowerInvariant();
                switch (ext)
                {
                    case "msg":
                        contentType = "application/vnd.ms-outlook";
                        break;
                    default:
                       contentType = "application/octet-stream";
                        break;
                }
            }
            return contentType;

            // below causes error (KeyNotFoundException) when extension not one of the files below
            //var types = new Dictionary<string, string>
            //{
            //    {".txt", "text/plain"},
            //    {".pdf", "application/pdf"},
            //    {".doc", "application/vnd.ms-word"},
            //    {".docx", "application/vnd.ms-word"},
            //    //{".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document"},
            //    {".xls", "application/vnd.ms-excel"},
            //    {".xlsx", "application/vnd.openxmlformatsofficedocument.spreadsheetml.sheet"},
            //    {".png", "image/png"},
            //    {".jpg", "image/jpeg"},
            //    {".jpeg", "image/jpeg"},
            //    {".gif", "image/gif"},
            //    {".csv", "text/csv"},
            //    {".xml", "application/xml"},
            //    {".json", "application/json"}
            //};

            //var ext = Path.GetExtension(fileName).ToLowerInvariant();
            //return types[ext];
        }

        /// Contructs and returns the actual physical file + path (C:\...)
        public static string GetPhysicalFilePath(string system, string fileName, CPiSavedFileType savedFileType)
        {
            var rootFolder = "";
            system = "";
            switch (savedFileType)
            {
                case CPiSavedFileType.Image:
                    rootFolder = imageRootFolder;
                    break;

                case CPiSavedFileType.Thumbnail:
                    rootFolder = imageThumbnailRootFolder;
                    break;

                case CPiSavedFileType.QE:
                    rootFolder = qeLogFolder;
                    break;

                case CPiSavedFileType.QELoggedImage:
                    //rootFolder = qeLogFolder;
                    rootFolder = qeLogAttachmentFolder;
                    break;
               
                case CPiSavedFileType.IDSReferences:
                    rootFolder = imageRootFolder;
                    break;

                case CPiSavedFileType.Letter:
                    rootFolder = letterLogFolder;
                    break;

                case CPiSavedFileType.EFS:
                    rootFolder = efsLogFolder;
                    break;

                case CPiSavedFileType.DocMgt:
                    rootFolder = _documentRootFolder;
                    break;

                case CPiSavedFileType.DocMgtThumbnail:
                    rootFolder = _documentThumbnailFolder;
                    break;

                case CPiSavedFileType.QELoggedImageThumbnail:
                    rootFolder = imageThumbnailRootFolder;
                    break;

                case CPiSavedFileType.Calendar:
                    rootFolder = _calendarFolder;
                    break;

                case CPiSavedFileType.ReportFile:
                    rootFolder = _reportLogFolder;
                    break;

                default:
                    rootFolder = imageRootFolder;
                    break;
            }

            if (string.IsNullOrEmpty(rootFolder))
                return string.Empty;

            var folder = Path.Combine(Directory.GetCurrentDirectory(), rootFolder, system);
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            fileName = Path.GetFileName(fileName);
            return Path.Combine(folder, fileName);
        }

        //todo: use url helper
        public static string GetImageUrlPath(string system)
        {
            return imageRootUrl.Replace("~", "") + "/" + system + "/";
        }

        public static string ImageRootFolder()
        {
            return imageRootUrl;
        }

        public static string GetIFWRootPath()
        {
            //return Path.Combine(Directory.GetCurrentDirectory(), imageRootFolder, "Patent");
            return Path.Combine(Directory.GetCurrentDirectory(), imageRootFolder);
        }

        public static string GetTLRootPath()
        {
            //return Path.Combine(Directory.GetCurrentDirectory(), imageRootFolder, "Trademark");
            return Path.Combine(Directory.GetCurrentDirectory(), imageRootFolder);
        }

        public static string GetThumbnailIcon(string? imageFile, string? thumbnailFile)
        {
            if (!string.IsNullOrEmpty(thumbnailFile) && !thumbnailFile.StartsWith("logo"))
                return thumbnailFile;
            else if (imageFile != null && imageFile.Contains("."))
            {
                var extension = imageFile.Split('.')[1].ToLower();
                var icon = GetDocumentIcon(extension);
                if (string.IsNullOrEmpty(icon) && !string.IsNullOrEmpty(thumbnailFile))
                    icon = thumbnailFile;

                return icon;
            }
            else 
                return null;
        }

        public static string GetThumbnailIcon(string fileName)
        {
            if (!string.IsNullOrEmpty(fileName)) {
                var extension = Path.GetExtension(fileName).Substring(1).ToLower();
                var documentIcon = GetDocumentIcon(extension);
                if (string.IsNullOrEmpty(documentIcon))
                    return fileName;
                else
                    return documentIcon;
            }
            return string.Empty;
        }

        //----------------------- below for review ------------------


        //MOVED TO FileHelper
        /// Returns the actual path of Quick Email Log
        //public static string GetQuickEmailLogPath(string system, string fileName)
        //{
        //    fileName = Path.GetFileName(fileName);
        //    return Path.Combine(Directory.GetCurrentDirectory(), quickEmailLogFolder, system, fileName);
        //}


        /// Creates a thumbnail filename out of the given filename.
        public static string ComposeThumbnailFileName(string originalFileName)
        {
            var nameOnly = Path.GetFileNameWithoutExtension(originalFileName);
            var extensionOnly = GetFileType(originalFileName);
            return $"{nameOnly}_thumb.{extensionOnly}";
        }

        /// Gets the name part of the filename and uses it as the File ID.
        public static int ExtractFileId(string fileName)
        {
            fileName = fileName.Replace("QELog_", "");
            var fileId = Path.GetFileNameWithoutExtension(fileName);
            if (IsNumeric(fileId))
                return Convert.ToInt32(fileId);
            else
                return 0;
        }

        public static string GetDocumentIcon(string extension)
        {
            if (extension == "pdf")
                return "logo_pdf.png";

            else if (extension.StartsWith("xls"))
                return "logo_excel.png";

            else if (extension.StartsWith("doc"))
                //return "logo_msword.jpg";
                return "logo_word.png";
            else if (extension.StartsWith("ppt"))
                return "logo_ppt.jpg";
            else if (extension.StartsWith("msg") || extension.StartsWith("eml"))
                return "logo_email.png";
            else if (extension.StartsWith("zip"))
                return "logo_zip.png";
            else
                return "logo_file.jpg";
        }


        /// Returns a Boolean value indicating whether an expression can be evaluated as a number.
        public static bool IsNumeric(string value)
        {
            return value.All(char.IsNumber);
        }

        public static string GetScreenCode(string parentKey)
        {
            switch (parentKey.ToLower())
            {
                case "invid":
                    return "Inv";

                case "appid":
                    return "CA";

                case "tmkid":
                    return "Tmk";

                case "matid":
                    return "GM";

                case "dmsid":
                    return "DMS";

                case "annid":
                    return "AMS";

                case "actid":
                    return "Act";

                case "costtrackid":
                    return "Cost";

                default:
                    return "";
            }
        }

        /// <summary>
        /// Returns a system out of a given system type
        /// </summary>
        /// <param name="systemType"></param>
        /// <returns></returns>
        public static string GetSystemName(string systemType)              
        {
            var systemName = "";
            switch (systemType.ToLower())
            {
                case "p":
                    systemName = "Patent";
                    break;

                case "t":
                    systemName = "Trademark";
                    break;

                case "g":
                    systemName = "GeneralMatter";
                    break;

                case "a":
                    systemName = "AMS";
                    break;

                case "d":
                    systemName = "DMS";
                    break;
            }
            return systemName;
        }

        //public static void CombineImages(List<ImageViewModel> main, List<ImageViewModel> source)
        //{
        //    foreach (var item in source)
        //    {
        //        if (string.IsNullOrEmpty(item.UserFileName))
        //            item.UserFileName = item.ImageFile;

        //        if (main.Any(i => i.UserFileName == item.UserFileName))
        //            item.UserFileName = item.ImageFile;
        //        main.Add(item);
        //    }
        //}

        //----------------------------- below to be deleted -------------------------

        /// <summary>
        /// Returns a system out of a given imageSystemModule. See enum ImageSystemModule
        /// </summary>
        /// <param name="imageSystemModule"></param>
        /// <returns></returns>
        //public static string GetSystem(ImageSystemModule imageSystemModule)
        //{
        //    switch (imageSystemModule)
        //    {
        //        case ImageSystemModule.PatentInvention:
        //        case ImageSystemModule.PatentApplication:
        //        case ImageSystemModule.PatentActionDue:
        //        case ImageSystemModule.PatentCostTracking:
        //            return "Patent";

        //        case ImageSystemModule.Trademark:
        //        case ImageSystemModule.TrademarkActionDue:
        //        case ImageSystemModule.TrademarkCostTracking:
        //            return "Trademark";

        //        case ImageSystemModule.GeneralMatter:
        //        case ImageSystemModule.GeneralMatterAction:
        //        case ImageSystemModule.GeneralMatterCostTracking:
        //            return "GeneralMatter";

        //        case ImageSystemModule.Disclosure:
        //        case ImageSystemModule.DisclosureActionDue:
        //            return "DMS";

        //        default:
        //            return "NotSet";
        //    }
        //}




        /// <summary>
        ///  Determines whether the given path is a location of a network file (i.e. file:///drive/folder/filename.ext).
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        //public static bool IsLocalFile(string path)
        //{
        //    return GetFileType(path).StartsWith("file:");
        //}

        // returns the image files URL for viewing on main screens (such as default image on trademark)
        //public static string GetImageUrl(ImageSystemModule imageSystemModule, string fileName)
        //{
        //    var system = ImageHelper.GetSystem(imageSystemModule);
        //    return imageRootUrl + "/" + system + "/" + fileName;
        //}


    }

}

