using R10.Core.Entities.DMS;
using R10.Core.Entities.Patent;
using R10.Core.Helpers;
using R10.Core.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Core.Entities
{
    public partial class ContactPerson : BaseEntity
    {
        [Key]
        public int ContactID { get; set; }

        [Required]
        [StringLength(10)]
        [Display(Name = "Contact Code")]
        public string?  Contact { get; set; }

        [StringLength(60)]
        [Display(Name ="Contact Name")]       
        public string?  ContactName { get; set; }

        [StringLength(25)]
        [Display(Name = "Last Name")]
        public string?  LastName { get; set; }

        [StringLength(25)]
        [Display(Name = "First Name")]
        public string?  FirstName { get; set; }

        [StringLength(2)]
        [Display(Name = "MI")]
        public string?  MiddleInitial { get; set; }

        [StringLength(50)]
        [Display(Name = "Greeting")]
        public string?  Greeting { get; set; }

        [StringLength(50)]
        [Display(Name = "Contact Title")]
        public string?  ContactTitle { get; set; }

        [StringLength(50)]
        [Display(Name = "Address")]
        public string?  Address1 { get; set; }

        [StringLength(50)]
        public string?  Address2 { get; set; }

        [StringLength(50)]
        public string?  Address3 { get; set; }

        [StringLength(50)]
        public string?  Address4 { get; set; }

        [StringLength(40)]
        [Display(Name = "City")]
        public string?  City { get; set; }

        [StringLength(50)]
        [Display(Name = "State/Region")]
        public string?  State { get; set; }

        [StringLength(20)]
        [Display(Name = "Postal/Zip Code")]
        public string?  ZipCode { get; set; }

        [StringLength(5)]
        [Display(Name = "Country")]
        public string?  Country { get; set; }

        [StringLength(10)]
        [Display(Name = "Language")]
        public string?  Language { get; set; }

        [StringLength(20)]
        [Display(Name = "Telephone No.")]
        public string?  PhoneNumber { get; set; }

        [StringLength(20)]
        [Display(Name = "Fax No.")]
        public string?  FaxNumber { get; set; }

        [StringLength(20)]
        [Display(Name = "Mobile Phone No.")]
        public string?  MobileNumber { get; set; }

        [StringLength(150)]
        //[EmailAddress(ErrorMessage = "The Email address is not valid.")]
        //ALLOW MULTIPLE EMAIL ADDRESSES
        [MultiEmailAddress(ErrorMessage = "The Email address is not valid.")]
        [Display(Name = "Email")]
        public string?  EMail { get; set; }

        [Display(Name = "Remarks")]
        public string?  Remarks { get; set; }

        public PatCountry? AddressCountry { get; set; }
        public List<ClientContact>? ClientContacts { get; set; }
        public List<AgentContact>? AgentContacts { get; set; }
        public List<OwnerContact>? OwnerContacts { get; set; }
        public List<DMSEntityReviewer>? Reviewers { get; set; }
        public List<DMSAgendaReviewer>? DMSAgendaReviewers { get; set; }
        public List<DMSReview>? Reviews { get; set; }
        public List<DMSPreview>? Previews { get; set; }
        public List<DMSValuation>? Valuations { get; set; }
        public Language? ContactLanguage { get; set; }
        public List<CPiUserEntityFilter>? EntityFilters { get; set; }        

        [Display(Name = "Active?")]
        public bool? IsActive { get; set; } = true;

        [NotMapped]
        [Display(Name = "Invention Disclosure Reviewer?")]
        public bool IsReviewer { get; set; }

        public string? CustomField1 { get; set; }
        public string? CustomField2 { get; set; }
        public string? CustomField3 { get; set; }
        public DateTime? CustomField4 { get; set; }
        public bool? CustomField5 { get; set; }
    }
}
