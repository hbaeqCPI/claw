using R10.Core.Entities.Patent;
using R10.Core.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.DMS
{
    public class DMSPreview : DMSPreviewDetail
    {
        public Disclosure? Disclosure { get; set; }

        //PREVIEWER TYPES
        public ContactPerson? Contact { get; set; }
        public PatInventor? Inventor { get; set; }
    }
    public class DMSPreviewDetail : BaseEntity
    {
        [Key]
        public int DMSPreviewId { get; set; }
        public int DMSId { get; set; }

        public CPiEntityType PreviewerType { get; set; }
        public int? PreviewerId { get; set; }       
        public string? UserId { get; set; }

        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }        
    }
}
