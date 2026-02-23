using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Models
{
    public class GridOptions
    {
        public int ParentId { get; set; }
        public string Controller { get; set; }
        public string Area { get; set; }
        public DetailPagePermission Permission { get; set; }
        public string  ParentScreen { get; set; }
        public string? DriveItemId { get; set; }
        public string? DocLibrary { get; set; }
        //public string? DocLibraryFolder { get; set; }
        //public string? RecKey { get; set; }
    }
}
