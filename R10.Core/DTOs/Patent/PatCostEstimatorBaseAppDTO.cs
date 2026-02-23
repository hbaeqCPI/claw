using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Core.DTOs
{
    public class PatCostEstimatorBaseAppDTO
    {
        public string? UniqueId { get; set; }
        public int AppId { get; set; }
        public int InvId { get; set; }
        public string? Title { get; set; }
        public string? ApplicationStatus { get; set; }
        public string? CaseNumber { get; set; }
        public string? Country { get; set; }
        public string? Subcase { get; set; }
        public DateTime? ParentFilDate { get; set; }
        public string? CountrySCase { get; set; }
    }
}
