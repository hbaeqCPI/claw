using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;

namespace R10.Web.Api.Models
{
    public class InstructionCriteria : ApiCriteria
    {
        public override List<QueryFilterViewModel> ToQueryFilter()
        {
            var filters = new List<QueryFilterViewModel>();

            if (StartDate != null)
                filters.Add(new QueryFilterViewModel() { Property = "AMSInstrxCPiLog.SendDateFrom", Value = StartDate.FormatToSave() });

            if (EndDate != null)
                filters.Add(new QueryFilterViewModel() { Property = "AMSInstrxCPiLog.SendDateTo", Value = EndDate.FormatToSave() });

            return filters;
        }
    }
}
