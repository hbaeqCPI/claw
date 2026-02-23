using R10.Core.Entities.Patent;
using R10.Web.Areas.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using R10.Core.Entities;

namespace R10.Web.Areas.Patent.ViewModels
{
    public class InventionDetailViewModel : InventionDetail
    {

        public string? Attorney1Code { get; set; }
        public string? Attorney1Name { get; set; }
        public string? Attorney1Label { get; set; }

        public string? Attorney2Code { get; set; }
        public string? Attorney2Name { get; set; }
        public string? Attorney2Label { get; set; }

        public string? Attorney3Code { get; set; }
        public string? Attorney3Name { get; set; }
        public string? Attorney3Label { get; set; }

        public string? Attorney4Code { get; set; }
        public string? Attorney4Name { get; set; }
        public string? Attorney4Label { get; set; }

        public string? Attorney5Code { get; set; }
        public string? Attorney5Name { get; set; }
        public string? Attorney5Label { get; set; }

        public string? ClientCode { get; set; }
        public string? ClientName { get; set; }
        public string? OwnerCode { get; set; }
        public string? OwnerName { get; set; }

        public bool CanModifyAttorney1 { get; set; } = true;
        public bool CanModifyAttorney2 { get; set; } = true;
        public bool CanModifyAttorney3 { get; set; } = true;
        public bool CanModifyAttorney4 { get; set; } = true;
        public bool CanModifyAttorney5 { get; set; } = true;

        public string? RequiredEntities { get; set; }

        public bool IsOwnerRequired { get; set; } = false;
        public bool IsInventorRequired { get; set; } = false;

        public DefaultImageViewModel? DefaultImage { get; set; }

        [NotMapped]
        public string? CopyOptions { get; set; }
        public List<SysCustomFieldSetting>? SysCustomFieldSettings { get; set; }

        public int? OldAttorney1ID { get; set; }
        public int? OldAttorney2ID { get; set; }
        public int? OldAttorney3ID { get; set; }
        public int? OldAttorney4ID { get; set; }
        public int? OldAttorney5ID { get; set; }

        public int FolderId { get; set; }

        [Display(Name = "Remuneration")]
        public string Remuneration { get; set; } = "N/A";
        [NotMapped]
        [Display(Name = "End of Compensation Date")]
        public DateTime? CompensationEndDate { get; set; }

        public bool IsFavorite { get; set; }
        public int FavoriteCount { get; set; }
    }
}
