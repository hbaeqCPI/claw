using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;

namespace R10.Web.Api.Models
{
    public class InventionCriteria : ApiCriteria
    {
        public string? CaseNumber { get; set; }

        public override bool IsValid()
        {
            //required criteria
            if (StartDate == null && EndDate == null && string.IsNullOrEmpty(CaseNumber))
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

            if (!string.IsNullOrEmpty(CaseNumber))
                filters.Add(new QueryFilterViewModel() { Property = "CaseNumber", Value = CaseNumber });

            return filters;
        }
    }
}
