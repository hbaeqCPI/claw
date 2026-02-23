using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using R10.Web.Areas.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Patent.ViewModels
{
    public class TerminalDisclaimerTreeViewModel
    {
        [Display(Name = "Terminal Disclaimer")]
        public string? CaseNumberCtrySub { get; set; }

        public string? AppTitle { get; set; }

        public int? AppId { get; set; }

        public List<TerminalDisclaimerRelated>? RelatedRecords { get; set; }
    }
    public class TerminalDisclaimerRelated
    {
        public string? CaseNumberCtrySub { get; set; }
        public string? AppTitle { get; set; }
        public int? AppId { get; set; }

    }

}
