using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Patent.ViewModels
{
    public class PatActionDueInvDetailViewModel : PatActionDueInvDetail
    {

        [Display(Name = "Status")]
        public string? DisclosureStatus { get; set; }


        [Display(Name = "Responsible Attorney")]
        public string? ResponsibleCode { get; set; }

        [Display(Name = "Attorney Name")]
        public string? ResponsibleName { get; set; }

        public bool CanModifyAttorney { get; set; } = true;

        public string? RespOffice { get; set; }

        public string? CopyOptions { get; set; }
        public int InvId { get; set; }


        public bool CanVerifyAction { get; set; } = false;
        public bool ShowVerifyAction { get; set; } = false;

        public int? ActionTypeID { get; set; }
    }
}
