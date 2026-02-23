using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;

namespace R10.Web.Api.Models
{
    public class StandardGoodsCriteria : ApiCriteria
    {
        public string? Class { get; set; }
        public string? ClassType { get; set; }
        public string? StandardGoods { get; set; }

        public override List<QueryFilterViewModel> ToQueryFilter()
        {
            var filters = new List<QueryFilterViewModel>();

            if (StartDate != null)
                filters.Add(new QueryFilterViewModel() { Property = "LastUpdateFrom", Value = StartDate.FormatToSave() });

            if (EndDate != null)
                filters.Add(new QueryFilterViewModel() { Property = "LastUpdateTo", Value = EndDate.FormatToSave() });

            if (!string.IsNullOrEmpty(Class))
                filters.Add(new QueryFilterViewModel() { Property = "Class", Value = Class });

            if (!string.IsNullOrEmpty(ClassType))
                filters.Add(new QueryFilterViewModel() { Property = "ClassType", Value = ClassType });

            if (!string.IsNullOrEmpty(StandardGoods))
                filters.Add(new QueryFilterViewModel() { Property = "StandardGoods", Value = StandardGoods });

            return filters;
        }
    }
}
