using R10.Core.Entities;
using R10.Core.Entities.Trademark;
using R10.Web.Areas.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Trademark.ViewModels
{ 
    public class TmkTrademarkDetailViewModel : TmkTrademarkDetail
    {
        public string? Attorney1Code { get; set; }
        public string? Attorney1Label { get; set; }

        public string? Attorney2Code { get; set; }
        public string? Attorney2Label { get; set; }

        public string? Attorney3Code { get; set; }
        public string? Attorney3Label { get; set; }

        public string? Attorney4Code { get; set; }        
        public string? Attorney4Label { get; set; }

        public string? Attorney5Code { get; set; }        
        public string? Attorney5Label { get; set; }

        public string? ClientCode { get; set; }
        public string? ClientName { get; set; }

        [Display(Name ="Client Contact")]
        public string? ClientContact { get; set; }               // default client contact

        public string? OwnerCode { get; set; }
        public string? OwnerName { get; set; }
        public string? AgentCode { get; set; }
        public string? AgentName { get; set; }

        public bool CanModifyAttorney1 { get; set; } = true;
        public bool CanModifyAttorney2 { get; set; } = true;
        public bool CanModifyAttorney3 { get; set; } = true;
        public bool CanModifyAttorney4 { get; set; } = true;
        public bool CanModifyAttorney5 { get; set; } = true;

        public string? RequiredEntities { get; set; }

        [Display(Name ="Country Name")]
        public string? CountryName { get; set; }

        [Display(Name = "Default Image")]

        //public string? ImageFile { get; set; }
        //public string? ThumbnailFile { get; set; }
        public DefaultImageViewModel? DefaultImage { get; set; }

        public string? ParentCase { get; set; }

        //There is an issue with IntentToUse label when clicking calls 
        //PageRead action which refreshes the detail page.
        //Don't know where the event is coming from.
        //Have to use different name and use automapper to map to IntentToUse field.
        [Display(Name = "Intent to Use")]
        public bool IsIntentToUse { get; set; }
        public bool LockRecord { get; set; }
        public bool? IsActive { get; set; }
        public List<SysCustomFieldSetting>? SysCustomFieldSettings { get; set; }

        public int? OldAttorney1ID { get; set; }
        public int? OldAttorney2ID { get; set; }
        public int? OldAttorney3ID { get; set; }
        public int? OldAttorney4ID { get; set; }
        public int? OldAttorney5ID { get; set; }

        [Display(Name = "Classes")]
        public List<GoodsExportViewModel>? TrademarkClasses { get; set; } = new List<GoodsExportViewModel>();

        public bool IsFavorite { get; set; }
        public int FavoriteCount { get; set; }
        public DateTime? FilDateCheck { get; set; }

        public int RequestDocketPendingCount { get; set; }
    }

}
