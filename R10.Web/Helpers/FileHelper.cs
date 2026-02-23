using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace R10.Web.Helpers
{
    public static class FileHelper
    {
        private static string AMSLogFolder => @"UserFiles\Searchable\Logs\AMS";
        private static string RMSLogFolder => @"UserFiles\Searchable\Logs\RMS";
        private static string FFLogFolder => @"UserFiles\Searchable\Logs\FF";
        private static string QuickEmailLogFolder => @"UserFiles\Searchable\Logs\QuickEmails";
        private static string QuickEmailAttachmentLogFolder => @"UserFiles\Searchable\Logs\QuickEmails";
        private static string DocMgtFolder => @"UserFiles\Searchable\Documents";
        private static string TemporaryFolder => @"UserFiles\Temporary Folder";

        public static string GetTemporaryFolder(this IHostingEnvironment hostingEnvironment, string userName)
        {
            var tempFolder = $"{TemporaryFolder}\\{userName}";
            return hostingEnvironment.GetFolder(tempFolder);
        }

        public static string GetAMSLogFolder(this IHostingEnvironment hostingEnvironment)
        {
            return hostingEnvironment.GetFolder(AMSLogFolder);
        }

        public static string GetRMSLogFolder(this IHostingEnvironment hostingEnvironment)
        {
            return hostingEnvironment.GetFolder(RMSLogFolder);
        }

        public static string GetFFLogFolder(this IHostingEnvironment hostingEnvironment)
        {
            return hostingEnvironment.GetFolder(FFLogFolder);
        }

        public static string GetQuickEmailLogFolder(this IHostingEnvironment hostingEnvironment, string system)
        {
            //return hostingEnvironment.GetFolder(Path.Combine(QuickEmailLogFolder, system));
            return hostingEnvironment.GetFolder(QuickEmailAttachmentLogFolder);
        }

        public static string GetQuickEmailLogPath(this IHostingEnvironment hostingEnvironment, string system, string fileName)
        {
            return Path.Combine(hostingEnvironment.GetQuickEmailLogFolder(system), Path.GetFileName(fileName));
        }

        public static string GetDocMgtPath(this IHostingEnvironment hostingEnvironment, string fileName)
        {
            return Path.Combine(hostingEnvironment.GetFolder(DocMgtFolder), Path.GetFileName(fileName));
        }


        public static string GetFolder(this IHostingEnvironment hostingEnvironment, string folderName)
        {
            var dir = new DirectoryInfo(Path.Combine(hostingEnvironment.ContentRootPath, folderName));
            if (!dir.Exists)
                dir.Create();

            return dir.FullName;
        }

        public static string AppendTimeStamp(this string fileName)
        {
            return string.Concat(
                Path.GetFileNameWithoutExtension(fileName),
                "_",
                DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                Path.GetExtension(fileName)
                );
        }

        public static async Task<bool> SaveFileUpload(string filePath, IFormFile uploadedFile)
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await uploadedFile.CopyToAsync(fileStream);
            }
            return true;
        }

        public static bool DeleteFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            return true;
        }

        public static bool CreateAndSaveThumbnail (string imageFilePath, string thumbFilePath, int thumbWidth = 80, int thumbHeight = 80)
        {
            try
            {
                FileStream imageStream = new FileStream(imageFilePath, FileMode.Open, FileAccess.Read);
                Image image = Image.FromStream(imageStream);
                Image thumb = image.GetThumbnailImage(thumbWidth, thumbHeight, () => false, IntPtr.Zero);
                thumb.Save(thumbFilePath);

                imageStream.Dispose();
                image.Dispose();
                thumb.Dispose();

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        /// <summary>
        /// Deletes all files and subdirectories within a specified folder.
        /// If the folder does not exist, the method does nothing.
        /// Exceptions during deletion (e.g., file in use) are caught and ignored.
        /// </summary>
        /// <param name="folderPath">The path to the folder from which to delete files and subdirectories.</param>
        public static void DeleteFiles(string folderPath)
        {
            try
            {
                var dirInfo = new DirectoryInfo(folderPath);
                if (System.IO.Directory.Exists(folderPath))
                {
                    foreach (FileInfo file in dirInfo.GetFiles())
                    {
                        file.Delete();
                    }
                    foreach (DirectoryInfo dir in dirInfo.GetDirectories())
                    {
                        dir.Delete(true);
                    }
                }
            }

            //just ignore, possible that file is still in use
            catch (Exception ex) { }
        }

        /// <summary>
        /// Create or return path to temporary folder or file
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GetTemporaryFolder(string userName, string fileName = "")
        {
            var path = Path.Combine(System.IO.Directory.GetCurrentDirectory(), TemporaryFolder, userName);

            var newFileName = fileName;
            if (newFileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                newFileName = GetValidFileName(fileName);
            }
            var fullPath = Path.Combine(path, newFileName);
            var file = new FileInfo(fullPath);

            try
            {
                if (!System.IO.Directory.Exists(path))
                {
                    System.IO.Directory.CreateDirectory(path);
                }
                else if (file.Exists && System.IO.File.GetCreationTime(fullPath) < DateTime.Now.AddSeconds(-30))
                {
                    System.IO.File.Delete(fullPath);
                }
                else if (file.Exists && System.IO.File.GetCreationTime(fullPath) > DateTime.Now.AddSeconds(-30))
                {
                    if (System.IO.Directory.Exists(path))
                    {
                        //unable to delete file, use different filename
                        newFileName = GetNextFileName(fullPath);
                        fullPath = Path.Combine(path, newFileName);
                    }
                }
            }
            catch (Exception e)
            {
                if (System.IO.Directory.Exists(path))
                {
                    //unable to delete file, use different filename
                    newFileName = GetNextFileName(fullPath);
                    fullPath = Path.Combine(path, newFileName);
                }
            }

            return fullPath;
        }

        /// <summary>
        /// Sanitizes a file name by replacing invalid path characters with a specified fill character.
        /// </summary>
        /// <param name="fileName">The original file name to validate.</param>
        /// <param name="fillChar">The character to use for replacing invalid characters (default is '_').</param>
        /// <returns>A valid file name string without invalid path characters.</returns>
        private static string GetValidFileName(string fileName, string fillChar = "_")
        {
            const string pattern = @"[\\\/:\*\?""'<>|]";
            var regex = new System.Text.RegularExpressions.Regex(pattern);
            return regex.Replace(fileName, fillChar);
        }

        /// <summary>
        /// Generates a unique, non-existent file path by appending a sequential number to the original file name
        /// until a non-existent path is found. Attempts to delete any existing file at the generated path before returning.
        /// </summary>
        /// <param name="fullPath">The original full path including file name and extension.</param>
        /// <returns>A new, unique full path that does not currently exist on the file system.</returns>
        private static string GetNextFileName(string fullPath)
        {
            var count = 1;
            var fileNameOnly = Path.GetFileNameWithoutExtension(fullPath);
            var extension = Path.GetExtension(fullPath);
            var path = System.IO.Path.GetDirectoryName(fullPath);
            var newFullPath = fullPath;
            do
            {
                var tempFileName = fileNameOnly + " " + count.ToString();
                newFullPath = Path.Combine(path, tempFileName + extension);
                count++;

                try
                {
                    System.IO.File.Delete(newFullPath);
                }
                catch (Exception ex)
                {
                }

            } while (System.IO.File.Exists(newFullPath));

            return newFullPath;
        }
    }
}
