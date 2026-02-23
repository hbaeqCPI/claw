using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Patent.ViewModels
{
    public class PatInventorAwardViewModel //Combine App, DMS Award
    {
        [Display(Name = "CaseNumber")]
        public string? CaseNumber { get; set; }

        [Display(Name = "Country")]
        public string? Country { get; set; }

        [Display(Name = "Sub Case")]
        public string? SubCase { get; set; }

        public int? AwardTypeId { get; set; } = 0;

        public int AwardId { get; set; }

        public int Id { get; set; }

        public int InventorID { get; set; }

        [Display(Name = "Amount")]
        public decimal Amount { get; set; }

        [Display(Name = "ID")]
        public int AwardCriteriaId { get; set; }

        [Display(Name = "Award Date")]
        public DateTime? AwardDate { get; set; }

        [Display(Name = "Payment Date")]
        public DateTime? PaymentDate { get; set; }

        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }

        [StringLength(20)]
        [Display(Name = "Award Type")]
        public string? AwardType { get; set; }

        [Display(Name = "Last Update")]
        public DateTime? LastUpdate { get; set; }
        public string? AwardSource { get; set; }
    }
}
