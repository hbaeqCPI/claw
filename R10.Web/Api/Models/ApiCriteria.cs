using R10.Web.Areas.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Api.Models
{
    public abstract class ApiCriteria
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public string? ErrorMessage { get; set; }

        public virtual bool IsValid()
        {
            //invalid date range
            if (StartDate != null && EndDate != null && StartDate > EndDate)
            {
                ErrorMessage = "Invalid date range.";
                return false;
            }

            //both start/end dates are required
            if ((StartDate != null && EndDate == null) || (StartDate == null && EndDate != null))
            {
                ErrorMessage = "Both start date and end date are required.";
                return false;
            }

            ErrorMessage = "";
            return true;
        }

        public abstract List<QueryFilterViewModel> ToQueryFilter();
    }
}
