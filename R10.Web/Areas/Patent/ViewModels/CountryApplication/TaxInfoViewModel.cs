using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using R10.Core.DTOs;

namespace R10.Web.Areas.Patent.ViewModels
{
    public class TaxInfoViewModel
    {
        public int AppId { get; set; }
        public PatCountryLawTaxInfoDTO TaxStartInfo { get; set; }
        public PatCountryLawTaxInfoDTO TaxExpirationInfo { get; set; }

    }

    public class MultipleBasedOnViewModel
    {
        public int AppId { get; set; }
        public string? SessionKey { get; set; }

    }
}
