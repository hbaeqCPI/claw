using System.ComponentModel.DataAnnotations;

namespace LawPortal.Core.Helpers
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
