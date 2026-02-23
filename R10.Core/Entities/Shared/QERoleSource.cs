using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities
{
    public class QERoleSource : BaseEntity
    {
        [Key]
        public int RoleSourceID { get; set; }

        //[Required]
        //[StringLength(1)]
        public string?  SystemType { get; set; }

        [Required]
        [StringLength(2)]
        [Display(Name = "Type")]
        public string?  RoleType { get; set; }
        
        [StringLength(150)]
        [Display(Name = "Role")]
        public string?  RoleName { get; set; }

        [Display(Name = "Source SQL")]
        [StringLength(3000)]
        public string?  SourceSQL { get; set; }

        [Display(Name = "Source Description")]
        [StringLength(50)]
        public string?  Description { get; set; }

        [Display(Name = "Order Of Entry")]
        public int OrderOfEntry { get; set; }

        public List<QERecipient>? QERoleSourceRecipients { get; set; }
    }
}
