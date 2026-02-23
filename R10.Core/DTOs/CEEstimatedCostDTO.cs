using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.DTOs
{
    [Keyless]
    public class CEEstimatedCostDTO
    {
        public int KeyId { get; set; }

        public string? Country { get; set; }

        public string? CountryName { get; set; }

        public string? CaseType { get; set; }

        [Display(Name = "Estimated Cost")]
        public double EstimatedCost { get; set; }

        public DateTime? FeesEffDate { get; set; }
        public double? ExchangeRate { get; set; }
    }

    [Keyless]
    public class CECascadeCostDTO
    {
        public int GroupId { get; set; }
        public string? DataKey { get; set; }
        public int DataKeyValue { get; set; }        
    }
}
