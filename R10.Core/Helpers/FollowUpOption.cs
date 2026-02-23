using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Helpers
{
    public enum FollowUpOption
    {
        [Display(Name = "Don't Generate")]
        DontGenerate = 0,
        [Display(Name = "Base Date")]
        BaseDate = 1,
        [Display(Name = "Response Date")]
        ResponseSentDate = -1
    }
}
