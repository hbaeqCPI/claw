using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class AbstractExport
    {
        [Display(Name = "Abstract")]
        public string Abstract { get; set; } = "";

        [Display(Name = "Language")]
        public string Language { get; set; } = "";

        public int OrderOfEntry { get; set; }
    }
}
