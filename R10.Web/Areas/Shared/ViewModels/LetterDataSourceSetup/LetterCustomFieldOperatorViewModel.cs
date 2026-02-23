using Microsoft.Extensions.Localization;
using R10.Core.DTOs;
using R10.Web.Models;

namespace R10.Web.Areas.Shared.ViewModels 
{
    public class LetterCustomFieldOperatorViewModel
    {
        public static List<LookupDTO> BuildList(IStringLocalizer<SharedResource> localizer)
        {

            return new List<LookupDTO>
                {
                    new LookupDTO  { Value = "+", Text = localizer["+"]},
                    new LookupDTO  { Value = "-", Text = localizer["-"]}
                };
        }
    }
}
