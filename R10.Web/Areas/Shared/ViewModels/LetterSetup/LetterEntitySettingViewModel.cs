using R10.Core.Entities;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class LetterEntitySettingViewModel:LetterEntitySetting
    {
        public string? LetterSendAs { get; set; }
        public string? LetterSendAsDescription { get; set; }
    }
}


