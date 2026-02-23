using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R9.Web.Areas.Patent.ViewModels
{
    public class InventionRemarksViewModel
    {
        public int InvId { get; set; }
        public string Remarks { get; set; }
        public byte[] tStamp { get; set; }
    }
}
