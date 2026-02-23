using Microsoft.Extensions.Localization;
using R10.Web.Models;


namespace R10.Web.Areas.Shared.ViewModels
{
    public class DOCXOptionViewModel
    {
        public int GenAllDOCXes { get; set; }
        public string? Description { get; set; }

        public static List<DOCXOptionViewModel> BuildList(IStringLocalizer<SharedResource> localizer)
        {
            return new List<DOCXOptionViewModel>{
                new DOCXOptionViewModel {GenAllDOCXes=1,Description=localizer["All"] },
                new DOCXOptionViewModel { GenAllDOCXes = 2, Description = localizer["Specific"] },
                new DOCXOptionViewModel { GenAllDOCXes = 0, Description = localizer["None"] }
            };
        }

        public static DOCXOptionViewModel GetDefault(IStringLocalizer<SharedResource> localizer)
        {
            return new DOCXOptionViewModel { GenAllDOCXes = 1, Description = localizer["All"] };
        }
    }

    public class DOCXSendAsOptionViewModel
    {
        public string? DOCXSendAs { get; set; }
        public string? Description { get; set; }

        public static List<DOCXSendAsOptionViewModel> BuildList(IStringLocalizer<SharedResource> localizer)
        {
            return new List<DOCXSendAsOptionViewModel>{
                new DOCXSendAsOptionViewModel {DOCXSendAs="T",Description=localizer["To"] },
                new DOCXSendAsOptionViewModel { DOCXSendAs = "C", Description = localizer["Cc"] },

            };
        }

        public static DOCXSendAsOptionViewModel GetDefault(IStringLocalizer<SharedResource> localizer)
        {
            return new DOCXSendAsOptionViewModel { DOCXSendAs = "T", Description = localizer["To"] };
        }

    }
}
