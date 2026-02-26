using R10.Core.DTOs;
// using R10.Core.Entities.DMS; // Removed during deep clean
// using R10.Core.Entities.GeneralMatter; // Removed during deep clean
using R10.Core.Entities.Shared;
using R10.Core.Helpers;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Core.Entities.Patent
{
    public class Invention : InventionDetail
    {
        public Client? Client { get; set; }
        //public Owner Owner { get; set; }
        public Attorney? Attorney1 { get; set; }
        public Attorney? Attorney2 { get; set; }
        public Attorney? Attorney3 { get; set; }
        public Attorney? Attorney4 { get; set; }
        public Attorney? Attorney5 { get; set; }
        public List<CountryApplication>? CountryApplications { get; set; }
        public List<PatInventorInv>? Inventors { get; set; }
        public List<PatOwnerInv>? Owners { get; set; }
        public List<PatKeyword>? Keywords { get; set; }
        public List<PatAbstract>? Abstracts { get; set; }
        public List<PatPriority>? Priorities { get; set; }
        public List<InventionRelatedDisclosure>? InventionRelatedDisclosures { get; set; }
        //public List<PatImageInv>? Images { get; set; }

        public List<InventionRelatedInvention>? InventionRelatedInventions { get; set; }
        public List<InventionRelatedInvention>? InventionRelateds { get; set; }
        
        public List<PatIDSManageDTO>? IDSManageCases { get; }
//         public List<GMMatterPatent>? GMMatterPatents { get; set; } // Removed during deep clean

        public List<PatCostEstimator>? CostEstimators { get; set; }

        public List<PatProductInv>? Products { get; set; }

        public List<PatCostTrackInv>? CostTrackings { get; set; }

        public List<PatActionDueInv>? ActionDueInvs { get; set; }

        public List<InventionTradeSecretRequest>? TradeSecretRequests { get; set; }
        public List<InventionImage>? Images { get; set; }
        public List<PatDocketInvRequest>? PatDocketInvRequests { get; set; }
    }

    public class InventionDetail: BaseEntityWithRespOffice
    {
        [Key]
        public int InvId { get; set; }

        [Required]
        [StringLength(25)]
        public string CaseNumber { get; set; }

        [StringLength(25)]
        [Display(Name = "Family Number")]
        public string? FamilyNumber { get; set; }

        [TradeSecret]
        [StringLength(255)]
        [Display(Name = "Title")]
        public string? InvTitle { get; set; }

        [Required]
        [Display(Name = "Disclosure Status")]
        public string? DisclosureStatus { get; set; } = "Open";

        [Display(Name = "Disclosure Date")]
        public DateTime? DisclosureDate { get; set; }

        public int? ClientID { get; set; }

        //public Client InventionClient { get; set; }

        [NotMapped]
        public int? OwnerID { get; set; }

        //[Display(Name = "Attorney 1")]
        public int? Attorney1ID { get; set; }

        //[Display(Name = "Attorney 2")]
        public int? Attorney2ID { get; set; }

        //[Display(Name = "Attorney 3")]
        public int? Attorney3ID { get; set; }

        //[Display(Name = "Attorney 4")]
        public int? Attorney4ID { get; set; }

        //[Display(Name = "Attorney 5")]
        public int? Attorney5ID { get; set; }

        [StringLength(20)]
        public string? ClientRef { get; set; }



        [StringLength(14)]
        [Display(Name = "Matter Number")]
        public string? InvMatterNumber { get; set; }

        public string? Remarks { get; set; }

        public DateTime? DateSubmitted { get; set; }

        public DateTime? DateReviewed { get; set; }

        public DateTime? DateApproved { get; set; }

        public int? DisclosureInventorId { get; set; }

        [StringLength(150)]
        public string? DisclosureReviewer { get; set; }

        public int? DMSId { get; set; }
        
        public string? CustomField1 { get; set; }
        public string? CustomField2 { get; set; }
        public string? CustomField3 { get; set; }
        public string? CustomField4 { get; set; }
        public DateTime? CustomField5 { get; set; }
        public bool? CustomField6 { get; set; }
        public string? CustomField7 { get; set; }
        public string? CustomField8 { get; set; }
        public DateTime? CustomField9 { get; set; }
        public bool? CustomField10 { get; set; }

        [Display(Name = "Use German Remuneration?")]
        public Boolean UseInventorRemuneration { get; set; }
        [Display(Name = "Use French Remuneration?")]
        public Boolean UseInventorFRRemuneration { get; set; }

        [Display(Name = "Trade Secret")]
        public bool? IsTradeSecret { get; set; } = false;

        public DateTime? TradeSecretDate { get; set; }

        public InventionTradeSecret? TradeSecret { get; set; }
    }

    public class InventionTradeSecret
    {
        [Encrypted]
        public string? InvTitle { get; set; }
    }

    // TradeSecretRequests with
    //  ScreenId == TradeSecretScreen.Invention
    //  RecId == InvId
    public class InventionTradeSecretRequest : TradeSecretRequest 
    {
        public Invention? Invention { get; set; }
    }
}
