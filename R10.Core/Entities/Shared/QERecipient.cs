using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities
{
    public class QERecipient: BaseEntity
    {
        [Key]
        public int RecipientID { get; set; }

        [Required]
        public int QESetupID { get; set; }

        [Required]
        public int RoleSourceID { get; set; }
        
        [Display(Name = "Send As")]
        [StringLength(10)]
        public string?  SendAs { get; set; }


        [Display(Name = "Default?")]
        public bool IsDefault { get; set; }

        [Display(Name = "Order Of Entry")]
        public int OrderOfEntry { get; set; }

        [Display(Name = "Role")]
        public QERoleSource? QERoleSource { get; set; }

        [Display(Name = "Routing Order")]
        public int? RoutingOrder { get; set; }

        [Display(Name = "Anchor")]
        public string? AnchorCode { get; set; }

    }
}
