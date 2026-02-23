using R10.Core.Entities;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;

namespace R10.Web.Api.Models
{
    public class InventorCriteria : ApiCriteria
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Inventor { get; set; }
        public string? EMail { get; set; }

        public override List<QueryFilterViewModel> ToQueryFilter()
        {
            var filters = new List<QueryFilterViewModel>();

            if (StartDate != null)
                filters.Add(new QueryFilterViewModel() { Property = "LastUpdateFrom", Value = StartDate.FormatToSave() });

            if (EndDate != null)
                filters.Add(new QueryFilterViewModel() { Property = "LastUpdateTo", Value = EndDate.FormatToSave() });

            if (!string.IsNullOrEmpty(FirstName))
                filters.Add(new QueryFilterViewModel() { Property = "FirstName", Value = FirstName });

            if (!string.IsNullOrEmpty(LastName))
                filters.Add(new QueryFilterViewModel() { Property = "LastName", Value = LastName });

            if (!string.IsNullOrEmpty(Inventor))
                filters.Add(new QueryFilterViewModel() { Property = "Inventor", Value = Inventor });

            if (!string.IsNullOrEmpty(EMail))
                filters.Add(new QueryFilterViewModel() { Property = "EMail", Value = EMail });

            return filters;
        }
    }
}
