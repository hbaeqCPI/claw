using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Api.Models
{
    public class LicenseeData
    {
        public string Licensee { get; set; }
        public string Licensor { get; set; }
        public string LicenseNo { get; set; }
        public DateTime? LicenseStart { get; set; }
        public DateTime? LicenseExpire { get; set; }
        public string LicenseRemarks { get; set; }
    }
}
