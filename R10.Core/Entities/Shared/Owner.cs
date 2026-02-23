using R10.Core.Entities.DMS;
using R10.Core.Entities.Patent;
using R10.Core.Helpers;
using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities
{
    public class Owner: OwnerDetail
    {
        public PatCountry? AddressCountry { get; set; }
        public PatCountry? POAddressCountry { get; set; }
        public Language? OwnerLanguage { get; set; }
        public List<OwnerContact>? OwnerContacts { get; set; }

        public List<Disclosure>? OwnerDisclosures { get; set; }
        
        public List<PatOwnerInv>? OwnerInvInventions { get; set; }
        public List<PatOwnerApp>? OwnerAppCountryApplications { get; set; }              
    }

    public class OwnerDetail : BaseEntity
    {
        [Key]
        public int OwnerID { get; set; }

        [Required]
        [StringLength(10)]
        public string OwnerCode { get; set; }

        [Required]
        [StringLength(60)]
        public string OwnerName { get; set; }

        [StringLength(50)]
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
        [Display(Name = "EMail")]
        //[EmailAddress(ErrorMessage = "The Email address is not valid.")]
        //ALLOW MULTIPLE EMAIL ADDRESSES
        [MultiEmailAddress(ErrorMessage = "The Email address is not valid.")]
        public string? EMail { get; set; }

        [StringLength(255)]
        [Url(ErrorMessage = "The Website is not a valid URL.")]
        [Display(Name = "Website")]
        public string? WebSite { get; set; }

        public int? GenAllLetters { get; set; }
        public string? Remarks { get; set; }

        [Display(Name = "Active?")]
        public bool? IsActive { get; set; } = true;

        public string? CustomField1 { get; set; }
        public string? CustomField2 { get; set; }
        public string? CustomField3 { get; set; }
        public DateTime? CustomField4 { get; set; }
        public bool? CustomField5 { get; set; }
    }
}
