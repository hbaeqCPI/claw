using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Entities.Patent
{
    public class PatIREuroExchangeRateYearly : BaseEntity
    {
        [Key]
        public int YearlyId { get; set; }
        public int ExchangeId { get; set; }
        
        [Display(Name = "Year")]
        public int Year { get; set; }
        
        [Display(Name = "Exchange Rate")]
        public double ExchangeRate { get; set; }
        public virtual PatIREuroExchangeRate? PatIREuroExchangeRate { get; set; }
    }
}
