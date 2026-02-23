using R10.Core.DTOs;
using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Patent
{
    public class PatInventorApp: PatInventorAppDetail
    {
        public PatInventor? InventorAppInventor { get; set; }
        public CountryApplication? CountryApplication { get; set; }

        //public List<PatIDSManageDTO>? IDSManageCases { get; }
    }

    public class PatInventorAppDetail : BaseEntity
    {
        [Key]
        public int InventorIDApp { get; set; }

        public int AppId { get; set; }
        public int InventorID { get; set; }
        public int OrderOfEntry { get; set; }
        public string? Remarks { get; set; }
        public bool? IsApplicant { get; set; }
        public bool EligibleForBasicAward { get; set; }
    }
}



