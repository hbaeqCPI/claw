using Microsoft.Extensions.Localization;
using R10.Web.Models;


namespace R10.Web.Areas.Shared.ViewModels
{
    public class LetterOptionViewModel
    {
        public int GenAllLetters { get; set; }
        public string? Description { get; set; }

        public static List<LetterOptionViewModel> BuildList(IStringLocalizer<SharedResource> localizer)
        {
            return new List<LetterOptionViewModel>{
                new LetterOptionViewModel {GenAllLetters=1,Description=localizer["All"] },
                new LetterOptionViewModel { GenAllLetters = 2, Description = localizer["Specific"] },
                new LetterOptionViewModel { GenAllLetters = 0, Description = localizer["None"] }
            };
        }

        public static LetterOptionViewModel GetDefault(IStringLocalizer<SharedResource> localizer)
        {
            return new LetterOptionViewModel { GenAllLetters = 1, Description = localizer["All"] };
        }
    }

    public class SendAsOptionViewModel
    {
        public string? LetterSendAs { get; set; }
        public string? Description { get; set; }

        public static List<SendAsOptionViewModel> BuildList(IStringLocalizer<SharedResource> localizer)
        {
            return new List<SendAsOptionViewModel>{
                new SendAsOptionViewModel {LetterSendAs="T",Description=localizer["To"] },
                new SendAsOptionViewModel { LetterSendAs = "C", Description = localizer["Cc"] },

            };
        }

        public static SendAsOptionViewModel GetDefault(IStringLocalizer<SharedResource> localizer)
        {
            return new SendAsOptionViewModel { LetterSendAs = "T", Description = localizer["To"] };
        }

    }
}
