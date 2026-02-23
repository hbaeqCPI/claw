using DocuSign.eSign.Model;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;

namespace R10.Web.Api.Models
{
    public class DisclosureStatusCriteria : ApiCriteria
    {
        public string? DisclosureStatus { get; set; }
        public string? Description { get; set; }

        public override List<QueryFilterViewModel> ToQueryFilter()
        {
            var filters = new List<QueryFilterViewModel>();

            if (StartDate != null)
                filters.Add(new QueryFilterViewModel() { Property = "LastUpdateFrom", Value = StartDate.FormatToSave() });

            if (EndDate != null)
                filters.Add(new QueryFilterViewModel() { Property = "LastUpdateTo", Value = EndDate.FormatToSave() });

            if (!string.IsNullOrEmpty(DisclosureStatus))
                filters.Add(new QueryFilterViewModel() { Property = "DisclosureStatus", Value = DisclosureStatus });

            if (!string.IsNullOrEmpty(Description))
                filters.Add(new QueryFilterViewModel() { Property = "Description", Value = Description });

            return filters;
        }
    }
}
