using Microsoft.Extensions.Localization;
using R10.Core.DTOs;
using R10.Web.Models;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class QECustomFieldPeriodViewModel
    {
        public static List<LookupDTO> BuildList(IStringLocalizer<SharedResource> localizer)
        {

            return new List<LookupDTO>
                {
                    new LookupDTO  { Value = "day", Text = localizer["Day"]},
                    new LookupDTO  { Value = "month", Text = localizer["Month"]},
                    new LookupDTO  { Value = "year", Text = localizer["Year"]}
                };
        }
    }
}
