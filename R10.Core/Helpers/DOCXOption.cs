using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Helpers
{
    public enum DOCXOption
    {
        [Display(Name = "All")]
        All = 1,
        [Display(Name = "Specific")]
        Specific = 2,
        [Display(Name = "None")]
        None = 0
    }
}
