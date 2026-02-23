using R10.Core.DTOs;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using R10.Core.Helpers;
using R10.Core.Entities.Shared;

namespace R10.Core.Entities.DMS
{
    public class Disclosure: DisclosureDetail
    {
        public DMSDisclosureStatus? DMSDisclosureStatus { get; set; }
        public Client? Client { get; set; }
        public Owner? Owner { get; set; }
        public Attorney? Attorney { get; set; }

        public PatArea? Area { get; set; }

        public List<DMSInventor>? Inventors { get; set; }        
        public List<DMSKeyword>? Keywords { get; set; }
        public List<DMSReview>? Reviews { get; set; }
        public List<DMSPreview>? Previews { get; set; }
        public List<DMSValuation>? Valuations { get; set; }

        public List<DMSActionDue>? ActionDues { get; set; }

        public List<DMSQuestion>? DMSQuestions { get; set; }

        public List<DMSDiscussion>? Discussions { get; set; }

        public List<DMSDisclosureStatusHistory>? DMSDisclosureStatusesHistory { get; set; }
        public List<DMSRecommendationHistory>? DMSRecommendationsHistory { get; set; }
        //public List<DMSImage>? Images { get; set; }
        public List<PatInventorDMSAward>? Awards { get; set; }

        //public List<DMSAbstract> Abstracts { get; set; }          // per Carlo, DMS only has 1 abstract field (not multiple)
        public List<InventionRelatedDisclosure>? InventionRelatedDisclosures { get; set; }

        public List<DisclosureRelatedDisclosure>? DisclosureRelatedDisclosures { get; set; }
        public List<DisclosureRelatedDisclosure>? DisclosureRelateds { get; set; }

        public List<DMSAgendaRelatedDisclosure>? DMSAgendaRelatedDisclosures { get; set; }

        public List<DMSCombined>? DMSCombineds { get; set; }
        public List<DMSCombined>? DisclosureCombineds { get; set; }

        public DMSAverageRatingDTO? AverageRating { get; set; }


        public List<DisclosureTradeSecretRequest>? TradeSecretRequests { get; set; }


        [NotMapped]
        public string? CopyOptions { get; set; }
    }

    public class DisclosureDetail : BaseEntity
    {
        [Key]
        public int DMSId { get; set; }

        [Required, StringLength(25)]
        [Display(Name = "Disclosure Number")]
        public string? DisclosureNumber { get; set; }

        [TradeSecret]
        [Required]
        [Display(Name = "Disclosure Title")]
        [StringLength(255)]
        public string? DisclosureTitle { get; set; }

        [Required, StringLength(20)]
        [Display(Name = "Disclosure Status")]
        public string? DisclosureStatus { get; set; }

        [Display(Name = "Status Date")]
        public DateTime? DisclosureStatusDate { get; set; }

        //[Required]
        [Display(Name = "Disclosure Date")]
        public DateTime? DisclosureDate { get; set; }

        public int? ClientID { get; set; }             

        public int? AreaID { get; set; }

        public int? OwnerID { get; set; }
                                
        public int? AttorneyID { get; set; }  
        
        [TradeSecret]
        public string? Abstract { get; set; }

        public string? Remarks { get; set; }

        public DateTime? SubmittedDate { get; set; }

        public DateTime? AuthorizedDate { get; set; }

        [Display(Name = "Recommendation")]
        public string? Recommendation { get; set; }

        public DateTime? RecommendationDate { get; set; }

        public string? RemarksPlus { get; set; }

        public int? PacId { get; set; }

        //For esignature
        public int? SignatureFileId { get; set; }

        public string? Combined { get; set; }

        [Display(Name = "Trade Secret")]
        public bool? IsTradeSecret { get; set; } = false;

        public DateTime? TradeSecretDate { get; set; }

        public DisclosureTradeSecret? TradeSecret { get; set; }
    }

    public class DisclosureTradeSecret 
    {
        [Encrypted]
        public string? DisclosureTitle { get; set; }

        [Encrypted]
        public string? Abstract { get; set; }
    }

    // TradeSecretRequests with
    //  ScreenId == TradeSecretScreen.Disclosure
    //  RecId == DMSId
    public class DisclosureTradeSecretRequest : TradeSecretRequest 
    {
        public Disclosure? Disclosure { get; set; }
    }
}
