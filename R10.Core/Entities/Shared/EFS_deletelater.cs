using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Entities.Shared
{
    public class EFS_deletelater: BaseEntity
    {

        [Display(Name="System Type")]
        public string? SystemType { get; set; }
        [Display(Name="Group")]
        public string? GroupDesc { get; set; }
        [Display(Name ="Description")]
        public string? DocDesc { get; set;}
        [Display(Name ="Country")]
        public string? Country { get; set; }
        [Display(Name ="For eSignature?")]
        public bool ForSignature { get; set; }
        [Display(Name ="Email Body")]
        public int?  SignatureQESetupId { get; set; }
        [Display(Name ="Anchor Code")]
        public string? AnchorCode { get; set; }

    }
}
