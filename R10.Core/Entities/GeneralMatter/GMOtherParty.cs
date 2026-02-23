using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.GeneralMatter
{
    public class GMOtherParty : BaseEntity
    {
        public int OtherPartyID { get; set; }

        [Key]
        [Required]
        [StringLength(50)]
        [Display(Name = "Other Party")]
        public string? OtherParty { get; set; }

        [StringLength(50)]
        [Display(Name = "Contact Person")]
        public string? ContactPerson { get; set; }

        [StringLength(50)]
        [Display(Name = "Contact Title")]
        public string? ContactTitle { get; set; }

        [StringLength(50)]
        [Display(Name = "Greeting")]
        public string? Greeting { get; set; }

        [StringLength(50)]
        [Display(Name = "Address")]
        public string? Address1 { get; set; }

        [StringLength(50)]
        public string? Address2 { get; set; }

        [StringLength(50)]
        public string? Address3 { get; set; }

        [StringLength(50)]
        public string? Address4 { get; set; }

        [StringLength(40)]
        [Display(Name = "City")]
        public string? City { get; set; }

        [StringLength(50)]
        [Display(Name = "State/Region")]
        public string? State { get; set; }

        [StringLength(20)]
        [Display(Name = "Postal/Zip Code")]
        public string? ZipCode { get; set; }

        [StringLength(5)]
        [Display(Name = "Country")]
        public string? Country { get; set; }

        [StringLength(50)]
        [Display(Name = "Address")]
        public string? POAddress1 { get; set; }

        [StringLength(50)]
        public string? POAddress2 { get; set; }

        [StringLength(50)]
        public string? POAddress3 { get; set; }

        [StringLength(50)]
        public string? POAddress4 { get; set; }

        [StringLength(40)]
        [Display(Name = "City")]
        public string? POCity { get; set; }

        [StringLength(50)]
        [Display(Name = "State/Region")]
        public string? POState { get; set; }

        [StringLength(20)]
        [Display(Name = "Postal/Zip Code")]
        public string? POZipCode { get; set; }

        [StringLength(5)]
        [Display(Name = "Country")]
        public string? POCountry { get; set; }

        [StringLength(10)]
        [Display(Name = "Language")]
        public string? Language { get; set; }


        [StringLength(20)]
        [Display(Name = "Telephone No.")]
        public string? PhoneNumber { get; set; }

        [StringLength(20)]
        [Display(Name = "Fax No.")]
        public string? FaxNumber { get; set; }

        [StringLength(20)]
        [Display(Name = "Mobile Phone No.")]
        public string? MobileNumber { get; set; }

        [StringLength(150)]
        [EmailAddress(ErrorMessage = "The Email address is not valid.")]
        [Display(Name = "Email")]
        public string? EMail { get; set; }

        [StringLength(255)]
        [Display(Name = "Website")]
        [Url(ErrorMessage = "The Website is not a valid URL.")]
        public string? WebSite { get; set; }

        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }
        
        public List<GMMatterOtherParty>? GMMatterOtherParties { get; set; }
        public GMCountry? AddressCountry { get; set; }
        public GMCountry? POAddressCountry { get; set; }
        public Language? OtherPartyLanguage { get; set; }
    }
}
