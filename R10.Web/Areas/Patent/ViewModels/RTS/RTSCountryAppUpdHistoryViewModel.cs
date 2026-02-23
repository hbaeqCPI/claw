using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using R10.Core.DTOs;

namespace R10.Web.Areas.Patent.ViewModels
{
    public class RTSCountryAppUpdHistoryViewModel
    {
        public int AppId { get; set; }

        [Display(Name = "Batch ID")]
        public string? BatchId { get; set; }

        [Display(Name = "Updated By")]
        public string? UserId { get; set; }

        [Display(Name = "Changed On")]
        public DateTime? LastUpdate { get; set; }

        public List<RTSCountryAppUpdHistoryFieldsViewModel> UpdatedFields { get; set; }
    }

    public class RTSCountryAppUpdHistoryFieldsViewModel
    {
        [Display(Name = "Field Name")]
        public string? FieldName { get; set; }

        [Display(Name = "Old Value")]
        public string? OldValue { get; set; }

        [Display(Name = "New Value")]
        public string? NewValue { get; set; }

        [Display(Name = "Reverted?")]
        public bool Reverted { get; set; }


    }
}
