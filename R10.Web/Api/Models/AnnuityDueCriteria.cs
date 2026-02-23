using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Api.Models
{
    public class AnnuityDueCriteria : ApiCriteria
    {
        public string? CPICode { get; set; }
        public string? CaseNumber { get; set; }
        public string? Country { get; set; }
        public string? SubCase { get; set; }

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
                filters.Add(new QueryFilterViewModel() { Property = "AnnuityDueDateFrom", Value = StartDate.FormatToSave() });

            if (EndDate != null)
                filters.Add(new QueryFilterViewModel() { Property = "AnnuityDueDateTo", Value = EndDate.FormatToSave() });

            if (!string.IsNullOrEmpty(CPICode))
                filters.Add(new QueryFilterViewModel() { Property = "AMSMain.CPIClientCode", Value = CPICode });

            if (!string.IsNullOrEmpty(CaseNumber))
                filters.Add(new QueryFilterViewModel() { Property = "AMSMain.CaseNumber", Value = CaseNumber });

            if (!string.IsNullOrEmpty(Country))
                filters.Add(new QueryFilterViewModel() { Property = "AMSMain.Country", Value = Country });

            if (!string.IsNullOrEmpty(SubCase))
                filters.Add(new QueryFilterViewModel() { Property = "AMSMain.SubCase", Value = SubCase });

            filters.Add(new QueryFilterViewModel() { Property = "IncludeInstructed", Value = "true" });
            filters.Add(new QueryFilterViewModel() { Property = "IncludeHiddenInstructionTypes", Value = "true" });

            return filters;
        }
    }
}
