using R10.Core.Entities.DMS;
using R10.Core.Entities.PatClearance;
using R10.Core.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace R10.Core.Entities.Patent
{
    public partial class PatInventor : BaseEntity
    {
        [Key]
        public int InventorID { get; set; }

        [StringLength(25)]
        [Display(Name = "Last Name")]
        [Required]
        public string? LastName { get; set; }

        [StringLength(25)]
        [Display(Name = "First Name")]
        //[Required]
        public string? FirstName { get; set; }

        [StringLength(25)]
        [Display(Name = "Middle Name")]
        public string? MiddleInitial { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public string? Inventor { get; set; }

        [StringLength(50)]
        [Display(Name = "Native Inventor Name")]
        public string? NativeInventorName { get; set; }

        [StringLength(30)]
        [Display(Name = "Employee ID")]
        public string? EmployeeID { get; set; }

        [StringLength(50)]
        [Display (Name = "Greeting")]
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

        [Display (Name ="City")]
        [StringLength(50)]
        public string? City { get; set; }

        [Display(Name = "State/Region")]
        [StringLength(50)]
        public string? State { get; set; }

        [Display(Name = "Postal/Zip Code")]
        [StringLength(20)]
        public string? ZipCode { get; set; }

        [Display(Name = "Country")]
        [StringLength(5)]
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

        [StringLength(50)]
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

        //----- Others
        [Display(Name = "Language")]
        [StringLength(10)]
        public string? Language { get; set; }

        [Display(Name = "Citizenship")]
        [StringLength(5)]
        public string? Citizenship { get; set; }

        [Display(Name = "Telephone No.")]
        [StringLength(50)]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Fax No.")]
        [StringLength(50)]
        public string? FaxNumber { get; set; }

        [Display(Name = "Mobile Phone No.")]
        [StringLength(50)]
        public string? MobileNumber { get; set; }

        [Display(Name = "EMail")]
        [EmailAddress(ErrorMessage = "The Email address is not valid.")]
        [StringLength(150)]
        public string? EMail { get; set; }

        [Display(Name = "Website")]
        [StringLength(255)]
        [Url(ErrorMessage = "The Website is not a valid URL.")]
        public string? WebSite { get; set; }

        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }

        public int? GenAllLetters { get; set; } = 1;

        [Display(Name = "Letter Send As")]
        [StringLength(1)]
        public string? LetterSendAs { get; set; } = "T";

        [Display(Name = "Active?")]
        public bool? IsActive { get; set; } = true;
        [Display(Name = "Inventor is Eligible for Awards?")]
        public bool EligibleForBasicAward { get; set; } = true;
        public int? ManagerId { get; set; }
        public int? PositionId { get; set; }
        [Display(Name = "Manager")]
        public PatInventor? Manager { get; set; }
        public List<PatInventor>? ManagerInventors { get; set; }
        [Display(Name = "Title")]
        public PatIREmployeePosition? Position { get; set; }
        public PatCountry? AddressCountry { get; set; }
        public PatCountry? POAddressCountry { get; set; }
        public PatCountry? CitizenshipCountry { get; set; }

        public List<PatInventorInv>? InventorInventions { get; set; }

        public List<PatInventorApp>? InventorCountryApplications { get; set; }

        public List<DMSEntityReviewer>? Reviewers { get; set; }
        public List<DMSAgendaReviewer>? DMSAgendaReviewers { get; set; }

        public List<DMSReview>? Reviews { get; set; }
        public List<DMSPreview>? Previews { get; set; }
        public List<DMSValuation>? Valuations { get; set; }

        public List<DMSInventor>? DisclosureInventors { get; set; }
        public List<PatInventorAppAward>? InventorAppAwards { get; set; }
        public List<PatInventorDMSAward>? InventorDMSAwards { get; set; }

        public List<PacInventor>? PacClearanceInventors { get; set; }
        public List<CPiUserEntityFilter>? EntityFilters { get; set; }

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
