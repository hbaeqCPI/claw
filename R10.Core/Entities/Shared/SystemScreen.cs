using R10.Core.Entities.Patent;
using R10.Core.Entities.Trademark;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities 
{
    public class SystemScreen : BaseEntity
    {
        [Key]
        public int ScreenId { get; set; }

        [StringLength(5)]
        [Display(Name = "System")]
        public string?  SystemType { get; set; }

        [StringLength(5)]
        [Display(Name = "Feature")]
        public string?  FeatureType { get; set; }

        [StringLength(20)]
        public string?  ScreenCode { get; set; }

        [StringLength(50)]
        public string?  ScreenName { get; set; }

        public bool? eSignature { get; set; }

        public List<PatWorkflow>? PatWorkflows { get; set; }
        public List<TmkWorkflow>? TmkWorkflows { get; set; }
    }
}
