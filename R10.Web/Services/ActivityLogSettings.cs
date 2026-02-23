using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Services
{
    public class ActivityLogSettings
    {
        public bool Enabled { get; set; }
        public string[] ExcludePaths { get; set; }
    }
}
