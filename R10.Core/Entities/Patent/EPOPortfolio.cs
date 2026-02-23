using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Patent
{   
    public class EPOPortfolio : BaseEntity
    {
        [Key]
        public int KeyId { get; set; }
        public int LogId { get; set; }

        [Required]       
        public string?  PortfolioId { get; set; }   
        
        public string?  Name { get; set; }        
        public string?  Type { get; set; }        
        public bool HasFullAccess { get; set; }
    }
}
