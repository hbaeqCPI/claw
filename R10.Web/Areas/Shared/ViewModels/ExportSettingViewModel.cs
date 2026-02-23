using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class ExportSettingViewModel
    {
        public string? PropertyName { get; set; }
        public string? Label { get; set; }
        public bool Include { get; set; }
    }


}
