using Org.BouncyCastle.Asn1.X509;
using R10.Core.Entities;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;

namespace R10.Web.Api.Models
{
    public class AttorneyCriteria : ApiCriteria
    {
        public int AttorneyID { get; set; }
        public string? AttorneyCode { get; set; }
        public string? AttorneyName { get; set; }

        public override List<QueryFilterViewModel> ToQueryFilter()
        {
            var filters = new List<QueryFilterViewModel>();

            if (StartDate != null)
                filters.Add(new QueryFilterViewModel() { Property = "LastUpdateFrom", Value = StartDate.FormatToSave() });

            if (EndDate != null)
                filters.Add(new QueryFilterViewModel() { Property = "LastUpdateTo", Value = EndDate.FormatToSave() });

            if (AttorneyID > 0)
                filters.Add(new QueryFilterViewModel() { Property = "AttorneyID", Value = AttorneyID.ToString() });

            if (!string.IsNullOrEmpty(AttorneyCode))
                filters.Add(new QueryFilterViewModel() { Property = "AttorneyCode", Value = AttorneyCode });

            if (!string.IsNullOrEmpty(AttorneyName))
                filters.Add(new QueryFilterViewModel() { Property = "AttorneyName", Value = AttorneyName });

            return filters;
        }
    }
}
