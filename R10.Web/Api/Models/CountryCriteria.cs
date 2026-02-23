using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;

namespace R10.Web.Api.Models
{
    public class CountryCriteria : ApiCriteria
    {
        public string? Country { get; set; }
        public string? CountryName { get; set; }

        public override List<QueryFilterViewModel> ToQueryFilter()
        {
            var filters = new List<QueryFilterViewModel>();

            if (StartDate != null)
                filters.Add(new QueryFilterViewModel() { Property = "LastUpdateFrom", Value = StartDate.FormatToSave() });

            if (EndDate != null)
                filters.Add(new QueryFilterViewModel() { Property = "LastUpdateTo", Value = EndDate.FormatToSave() });

            if (!string.IsNullOrEmpty(Country))
                filters.Add(new QueryFilterViewModel() { Property = "Country", Value = Country });

            if (!string.IsNullOrEmpty(CountryName))
                filters.Add(new QueryFilterViewModel() { Property = "CountryName", Value = CountryName });

            return filters;
        }
    }
}
