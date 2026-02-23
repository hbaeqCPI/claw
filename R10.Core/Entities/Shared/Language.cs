using R10.Core.Entities.DMS;
using R10.Core.Entities.GeneralMatter;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace R10.Core.Entities
{

    public partial class Language : BaseEntity
    {
        public int KeyID { get; set; }

        [Key]
        [StringLength(10)]
        [Required]
        [Display(Name = "Language")]
        public string LanguageName { get; set; }

        [StringLength(10)]
        [Required]
        [Display(Name = "Language Culture")]
        public string? LanguageCulture { get; set; }

        [StringLength(255)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        public List<PatAbstract>? LanguagePatAbstracts { get; set; }
        public List<DMSAbstract>? LanguageDMSAbstracts { get; set; }
        public List<QEMain>? LanguageQEMains { get; set; }
        public List<Agent>? LanguageAgents { get; set; }
        public List<Client>? LanguageClients { get; set; }
        public List<Attorney>? LanguageAttorneys { get; set; }
        public List<Owner>? LanguageOwners { get; set; }
        public List<ContactPerson>? LanguageContacts { get; set; }
        public List<GMOtherParty>? LanguageOtherParties { get; set; }
        public List<EmailSetup>? EmailSetups { get; set; }
    }
}
