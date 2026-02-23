using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static R10.Web.Helpers.ImageHelper;

namespace R10.Web.Services.DocumentStorage
{
    public class CPIFile
    {
        public string FileName { get; set; }
        public string OrigFileName { get; set; }
        public string ContentType { get; set; }
        public Stream Stream { get; set; }
    }
}
