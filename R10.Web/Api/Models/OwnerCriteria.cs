using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;

namespace R10.Web.Api.Models
{
    public class OwnerCriteria : ApiCriteria
    {
        public int OwnerID { get; set; }
        public string? OwnerCode { get; set; }
        public string? OwnerName { get; set; }

        public override List<QueryFilterViewModel> ToQueryFilter()
        {
            var filters = new List<QueryFilterViewModel>();

            if (StartDate != null)
                filters.Add(new QueryFilterViewModel() { Property = "LastUpdateFrom", Value = StartDate.FormatToSave() });

            if (EndDate != null)
                filters.Add(new QueryFilterViewModel() { Property = "LastUpdateTo", Value = EndDate.FormatToSave() });

            if (OwnerID > 0)
                filters.Add(new QueryFilterViewModel() { Property = "OwnerID", Value = OwnerID.ToString() });

            if (!string.IsNullOrEmpty(OwnerCode))
                filters.Add(new QueryFilterViewModel() { Property = "OwnerCode", Value = OwnerCode });

            if (!string.IsNullOrEmpty(OwnerName))
                filters.Add(new QueryFilterViewModel() { Property = "OwnerName", Value = OwnerName });

            return filters;
        }
    }
}
