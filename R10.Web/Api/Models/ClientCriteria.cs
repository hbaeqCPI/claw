using R10.Core.Entities;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;

namespace R10.Web.Api.Models
{
    public class ClientCriteria : ApiCriteria
    {
        public int ClientID { get; set; }
        public string? ClientCode { get; set; }
        public string? ClientName { get; set; }

        public override List<QueryFilterViewModel> ToQueryFilter()
        {
            var filters = new List<QueryFilterViewModel>();

            if (StartDate != null)
                filters.Add(new QueryFilterViewModel() { Property = "LastUpdateFrom", Value = StartDate.FormatToSave() });

            if (EndDate != null)
                filters.Add(new QueryFilterViewModel() { Property = "LastUpdateTo", Value = EndDate.FormatToSave() });

            if (ClientID > 0)
                filters.Add(new QueryFilterViewModel() { Property = "ClientID", Value = ClientID.ToString() });

            if (!string.IsNullOrEmpty(ClientCode))
                filters.Add(new QueryFilterViewModel() { Property = "ClientCode", Value = ClientCode });

            if (!string.IsNullOrEmpty(ClientName))
                filters.Add(new QueryFilterViewModel() { Property = "ClientName", Value = ClientName });

            return filters;
        }
    }
}
