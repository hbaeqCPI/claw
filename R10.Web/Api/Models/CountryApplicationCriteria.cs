using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Api.Models
{
    public class CountryApplicationCriteria : ApiCriteria
    {
        public string? AppNumber { get; set; }
        public string? PatNumber { get; set; }
        public string? CaseNumber { get; set; }
        public string? Country { get; set; }

        public override bool IsValid()
        {
            //required criteria
            if (StartDate == null && EndDate == null && string.IsNullOrEmpty(AppNumber) && string.IsNullOrEmpty(PatNumber) && string.IsNullOrEmpty(CaseNumber) && string.IsNullOrEmpty(Country))
            {
                ErrorMessage = "Required criteria not found.";
                return false;
            }

            //country must be used with appNumber or patNumber
            //if (string.IsNullOrEmpty(AppNumber) && string.IsNullOrEmpty(PatNumber) && !string.IsNullOrEmpty(Country))
            //{
            //    ErrorMessage = "Application number or patent number is required.";
            //    return false;
            //}

            return base.IsValid();
        }

        public override List<QueryFilterViewModel> ToQueryFilter()
        {
            var filters = new List<QueryFilterViewModel>();

            if (StartDate != null)
                filters.Add(new QueryFilterViewModel() { Property = "LastUpdateFrom", Value = StartDate.FormatToSave() });

            if (EndDate != null)
                filters.Add(new QueryFilterViewModel() { Property = "LastUpdateTo", Value = EndDate.FormatToSave() });

            if (!string.IsNullOrEmpty(AppNumber))
                filters.Add(new QueryFilterViewModel() { Property = "AppNumber", Value = AppNumber });

            if (!string.IsNullOrEmpty(PatNumber))
                filters.Add(new QueryFilterViewModel() { Property = "PatNumber", Value = PatNumber });

            if (!string.IsNullOrEmpty(Country))
                filters.Add(new QueryFilterViewModel() { Property = "Country", Value = Country });

            if (!string.IsNullOrEmpty(CaseNumber))
                filters.Add(new QueryFilterViewModel() { Property = "CaseNumber", Value = CaseNumber });

            return filters;
        }
    }
}
