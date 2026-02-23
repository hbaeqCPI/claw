using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;

namespace R10.Web.Api.Models
{
    public class ActionDueCriteria : ApiCriteria
    {
        public string? ActionType { get; set; }
        public string? ActionDue { get; set; }
        public string? Indicator { get; set; }
        public string? Responsible { get; set; }
        public string? Attorney { get; set; }

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
                filters.Add(new QueryFilterViewModel() { Property = "DueDates.DueDateFrom", Value = StartDate.FormatToSave() });

            if (EndDate != null)
                filters.Add(new QueryFilterViewModel() { Property = "DueDates.DueDateTo", Value = EndDate.FormatToSave() });

            if (!string.IsNullOrEmpty(ActionType))
                filters.Add(new QueryFilterViewModel() { Property = "ActionType", Value = ActionType });

            if (!string.IsNullOrEmpty(Responsible))
                filters.Add(new QueryFilterViewModel() { Property = "MultiSelect_Responsible.AttorneyCode", Value = Responsible });

            if (!string.IsNullOrEmpty(ActionDue))
                filters.Add(new QueryFilterViewModel() { Property = "DueDates.ActionDue", Value = ActionDue });

            if (!string.IsNullOrEmpty(Indicator))
                filters.Add(new QueryFilterViewModel() { Property = "DueDates.Indicator", Value = Indicator });

            if (!string.IsNullOrEmpty(Attorney))
                filters.Add(new QueryFilterViewModel() { Property = "DueDates.Attorney", Value = Attorney });

            return filters;
        }
    }
}
