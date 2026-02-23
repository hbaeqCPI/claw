using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;

namespace R10.Web.Api.Models
{
    public class CostTrackingCriteria : ApiCriteria
    {
        public string? CostType { get; set; }

        public override bool IsValid()
        {
            //required criteria
            if (StartDate == null && EndDate == null)
            {
                ErrorMessage = "Required criteria not found.";
                return false;
            }

            return base.IsValid();
        }

        public override List<QueryFilterViewModel> ToQueryFilter()
        {
            var filters = new List<QueryFilterViewModel>();

            if (StartDate != null)
                filters.Add(new QueryFilterViewModel() { Property = "LastUpdateFrom", Value = StartDate.FormatToSave() });

            if (EndDate != null)
                filters.Add(new QueryFilterViewModel() { Property = "LastUpdateTo", Value = EndDate.FormatToSave() });

            if (!string.IsNullOrEmpty(CostType))
                filters.Add(new QueryFilterViewModel() { Property = "CostType", Value = CostType });

            return filters;
        }
    }
}
