using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;

namespace R10.Web.Api.Models
{
    public class PaymentCriteria : ApiCriteria
    {
        public string? CPICode { get; set; }

        public override bool IsValid()
        {
            //required criteria
            if (string.IsNullOrEmpty(CPICode))
            {
                ErrorMessage = "CPICode is required.";
                return false;
            }

            return base.IsValid();
        }

        public override List<QueryFilterViewModel> ToQueryFilter()
        {
            var filters = new List<QueryFilterViewModel>();

            if (StartDate != null)
                filters.Add(new QueryFilterViewModel() { Property = "CPIPaymentDateFrom", Value = StartDate.FormatToSave() });

            if (EndDate != null)
                filters.Add(new QueryFilterViewModel() { Property = "CPIPaymentDateTo", Value = EndDate.FormatToSave() });

            if (!string.IsNullOrEmpty(CPICode))
                filters.Add(new QueryFilterViewModel() { Property = "AMSMain.CPIClientCode", Value = CPICode });

            return filters;
        }
    }
}
