using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace R10.Core.Entities.Patent
{
    public class PatCEAnnuityCost : BaseEntity
    {
        [Key]
        public int CostId { get; set; }
        public int CEAnnuityId { get; set; }

        [Display(Name = "Cost Type")]
        public string? CostType { get; set; }

        [Display(Name = "Year 1")]        
        public decimal Y1 { get; set; }
        [Display(Name = "Year 2")]
        public decimal Y2 { get; set; }
        [Display(Name = "Year 3")]
        public decimal Y3 { get; set; }
        [Display(Name = "Year 4")]
        public decimal Y4 { get; set; }
        [Display(Name = "Year 5")]
        public decimal Y5 { get; set; }
        [Display(Name = "Year 6")]
        public decimal Y6 { get; set; }
        [Display(Name = "Year 7")]
        public decimal Y7 { get; set; }
        [Display(Name = "Year 8")]
        public decimal Y8 { get; set; }
        [Display(Name = "Year 9")]
        public decimal Y9 { get; set; }
        [Display(Name = "Year 10")]
        public decimal Y10 { get; set; }
        [Display(Name = "Year 11")]
        public decimal Y11 { get; set; }
        [Display(Name = "Year 12")]
        public decimal Y12 { get; set; }
        [Display(Name = "Year 13")]
        public decimal Y13 { get; set; }
        [Display(Name = "Year 14")]
        public decimal Y14 { get; set; }
        [Display(Name = "Year 15")]
        public decimal Y15 { get; set; }
        [Display(Name = "Year 16")]
        public decimal Y16 { get; set; }
        [Display(Name = "Year 17")]
        public decimal Y17 { get; set; }
        [Display(Name = "Year 18")]
        public decimal Y18 { get; set; }
        [Display(Name = "Year 19")]
        public decimal Y19 { get; set; }
        [Display(Name = "Year 20")]
        public decimal Y20 { get; set; }


        [Display(Name = "Generate?")]
        public bool ActiveSwitch { get; set; }

        [Display(Name = "CPI Cost")]
        public bool CPICost { get; set; }
        
        public PatCEAnnuitySetup? PatCEAnnuitySetup { get; set; }
        
    }
}