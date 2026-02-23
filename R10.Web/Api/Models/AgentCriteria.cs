using DocumentFormat.OpenXml.Bibliography;
using Microsoft.IdentityModel.Tokens;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;

namespace R10.Web.Api.Models
{
    public class AgentCriteria : ApiCriteria
    {
        public int AgentID { get; set; }
        public string? AgentCode { get; set; }
        public string? AgentName { get; set; }

        public override List<QueryFilterViewModel> ToQueryFilter()
        {
            var filters = new List<QueryFilterViewModel>();

            if (StartDate != null)
                filters.Add(new QueryFilterViewModel() { Property = "LastUpdateFrom", Value = StartDate.FormatToSave() });

            if (EndDate != null)
                filters.Add(new QueryFilterViewModel() { Property = "LastUpdateTo", Value = EndDate.FormatToSave() });

            if (AgentID > 0)
                filters.Add(new QueryFilterViewModel() { Property = "AgentID", Value = AgentID.ToString() });

            if (!string.IsNullOrEmpty(AgentCode))
                filters.Add(new QueryFilterViewModel() { Property = "AgentCode", Value = AgentCode });

            if (!string.IsNullOrEmpty(AgentName))
                filters.Add(new QueryFilterViewModel() { Property = "AgentName", Value = AgentName });

            return filters;
        }
    }
}
