using System;
using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Patent.ViewModels
{
    public class IDSMassUpdateViewModel
    {
        public int AppId { get; set; }

        [Display(Name = "Update")]
        public string? FilDateType { get; set; }
        public string? RecordType { get; set; }

        [Display(Name = "Date Filed")]
        public DateTime? FilDate { get; set; }

        public DateTime? SpecificFilDate { get; set; }

        public bool ConsideredByExaminer { get; set; }
    }

    public class IDSMassUpdateExaminerViewModel
    {
        public int AppId { get; set; }

        public string? FilDateType { get; set; }
        public string? RecordType { get; set; }

        public DateTime? FilDateFrom { get; set; }
        public DateTime? FilDateTo { get; set; }
        public DateTime? SpecificFilDate { get; set; }

    }

}
