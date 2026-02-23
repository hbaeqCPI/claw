using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Helpers
{
    public class NoExportAttribute : Attribute
    {
        public string Setting { get; set; }
    }
}
