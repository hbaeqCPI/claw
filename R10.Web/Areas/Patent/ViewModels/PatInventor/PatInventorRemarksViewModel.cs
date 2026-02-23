using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Patent.ViewModels
{
    public class PatInventorRemarksViewModel
    {
        public int InventorID { get; set; }
        public string? Remarks { get; set; }
        public byte[] tStamp { get; set; }
    }
}
