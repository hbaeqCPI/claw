using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Models
{
    public class TLUpdatePageViewModel
    {
        public string UpdateType { get; set; }
        public string PageId { get; set; }
        public int GridPageSize { get; set; } = 10;

        public DetailPagePermission PagePermission { get; set; }
    }

    
}
