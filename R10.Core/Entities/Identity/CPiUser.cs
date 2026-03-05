using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using R10.Core.Entities;

namespace R10.Core.Identity
{
    public class CPiUser : IdentityUser
    {

        public int PkId { get; set; }

        [Required]
        [StringLength(256)]
        public string? FirstName { get; set; }

        [Required]
        [StringLength(256)]
        public string? LastName { get; set; }

        [StringLength(256)]
        public string Locale { get; set; } = "en"; //cldr data has no en-US

        [Display(Name ="Status")]
        public CPiUserStatus Status { get; set; }

        public CPiUserType UserType { get; set; }

        public DateTime? LastPasswordChangeDate { get; set; }

        [Display(Name = "Last Login")]
        public DateTime? LastLoginDate { get; set; }

        public CPiEntityType EntityFilterType { get; set; }

        public bool? PasswordNeverExpires { get; set; }

        public DateTime? ValidDateFrom { get; set; }

        public DateTime? ValidDateTo { get; set; }

        public bool? CannotChangePassword { get; set; }

        public bool UseOutlookAddIn { get; set; }

        public string? ClientId { get; set; }
        [Display(Name = "Hourly Rate")]
        public decimal? HourlyRate { get; set; }

        public bool? WebApiAccessOnly { get; set; }

        public bool? ExternalLoginOnly { get; set; } //force user to login using SSO

        [StringLength(20)]
        [Display(Name = "Created By")]
        public string? CreatedBy { get; set; }

        [StringLength(20)]
        [Display(Name = "Updated By")]
        public string? UpdatedBy { get; set; }

        [Display(Name = "Date Created")]
        public DateTime? DateCreated { get; set; }

        [Display(Name = "Last Update")]
        public DateTime? LastUpdate { get; set; }

        [Column(TypeName = "timestamp")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [MaxLength(8)]
        [ConcurrencyCheck]
        public byte[]? tStamp { get; set; }


        public string FullName
        {
            get => string.Concat(FirstName ?? "", " ", LastName ?? "");
        }
        public string Initials
        {
            get => string.Concat(String.IsNullOrEmpty(FirstName) ? "" : FirstName[0].ToString(), string.IsNullOrEmpty(LastName) ? "" : LastName[0].ToString());
        }
        public bool IsEnabled
        {
            get => (Status == CPiUserStatus.Approved); 
        }

        public bool IsPending
        {
            get => (Status == CPiUserStatus.Pending);
        }

        public bool IsLockedOut
        {
            get => (LockoutEnd != null && LockoutEnd >= DateTime.UtcNow);
        }

        public bool IsAdmin
        {
            get => (UserType == CPiUserType.Administrator || UserType == CPiUserType.SuperAdministrator);
        }

        public bool RequiresConfirmedEmail
        {
            get => (ConfirmedEmailRequired && !EmailConfirmed);
        }

        protected bool ConfirmedEmailRequired
        {
            //USER TYPES THAT REQUIRES EMAIL CONFIRMATION
            get => false; //(UserType == CPiUserType.Inventor || UserType == CPiUserType.ContactPerson); 
        }

        public CPiEntityType DefaultEntityFilterType
        {
            get
            {
                switch (UserType)
                {
                    case CPiUserType.Inventor:
                        return CPiEntityType.Inventor;

                    case CPiUserType.ContactPerson:
                        return CPiEntityType.ContactPerson;

                    case CPiUserType.Attorney:
                        return CPiEntityType.Attorney;

                    default:
                        return CPiEntityType.None;
                }
            }
        }

        //INVENTOR SELF REGISTRATION
        public static CPiUser NewRegistration => new CPiUser
        {
            UserType = CPiUserType.Inventor,
            Status = CPiUserStatus.Pending,
            LastPasswordChangeDate = DateTime.Now,
            EntityFilterType = CPiEntityType.Inventor
        };

        //SSO REGISTRATION
        public static CPiUser NewExternalLogin => new CPiUser
        {
            UserType = CPiUserType.User,
            Status = CPiUserStatus.Approved
        };

        public List<CPiUserSetting> CPiUserSettings { get; set; }
        public List<CPiUserSystemRole> CPiUserSystemRoles { get; set; }
        public List<CPiUserEntityFilter> CPiUserEntityFilters { get; set; }
        public List<CPiUserClaim> CPiUserClaims { get; set; }
        public List<CPiUserGroup> CPiUserGroups { get; set; }
    }

    public enum CPiUserType
    {
        User,
        Inventor,
        [Display(Name = "Contact Person")]
        ContactPerson,
        Administrator,
        Attorney,
        [Display(Name = "Docket Service")]
        DocketService,
        [Display(Name = "CPI Admin")]
        SuperAdministrator = 99
    }

    public enum CPiUserStatus
    {
        Approved,
        Disabled,
        Pending,
        Rejected
    }

    public enum CPiEntityType
    {
        None,
        [Display(Name = "LabelClient")]
        Client,
        [Display(Name = "LabelAgent")]
        Agent,
        [Display(Name = "LabelOwner")]
        Owner,
        Attorney,
        Inventor,
        [Display(Name = "Contact Person")]
        ContactPerson
    }
}
