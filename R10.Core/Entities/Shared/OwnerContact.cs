using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities
{  
    public partial class OwnerContact : BaseEntity
    {
        [Key]
        public int OwnerContactID { get; set; }

        [Display(Name = "Default?")]
        public bool Default { get; set; }

        [Display(Name = "Send Letters?")]
        [UIHint("LetterOptions")]
        public int GenAllLetters { get; set; }

        [StringLength(1)]
        [Display(Name = "Send As")]
        [UIHint("SendAsOptions")]
        public string?  LetterSendAs { get; set; }

        public int OwnerID { get; set; }
        public Owner? Owner { get; set; }

        [Required]
        [Display(Name = "Contact")]
        public int ContactID { get; set; }

        [Display(Name = "Contact")]
        public ContactPerson? Contact { get; set; }


        [Display(Name = "Pat Contact")]
        public bool? IsPatentContact { get; set; }

        [Display(Name = "Tmk Contact")]
        public bool? IsTrademarkContact { get; set; }

        [Display(Name = "GM Contact")]
        public bool? IsGeneralMatterContact { get; set; }
    }
}
