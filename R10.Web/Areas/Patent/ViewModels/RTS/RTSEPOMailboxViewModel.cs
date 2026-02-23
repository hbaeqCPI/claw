using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Patent.ViewModels
{
    public class RTSEPOMailboxViewModel
    {
        public int AppId { get; set; }
        public string? CaseNumber { get; set; }
        public string? Country { get; set; }
        public string? SubCase { get; set; }

        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Mail Room Date")]
        public DateTime? DispatchDate { get; set; }


        public string? DocFileName { get; set; }        
        public int DocId { get; set; }
        public string? DriveItemId { get; set; }
    }
}
