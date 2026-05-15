using System.IO;

namespace LawPortal.Web.Services.DocumentStorage
{
    public class CPIFile
    {
        public string FileName { get; set; } = "";
        public string OrigFileName { get; set; } = "";
        public string ContentType { get; set; } = "";
        public Stream? Stream { get; set; }
    }
}
