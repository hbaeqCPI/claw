using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class MapViewModel
    {
        //public const string? PatentTitle = "Patent Worldwide Applications";
        //public const string? TrademarkTitle = "Trademark Worldwide Applications";

        //public const int PatentToolTipWidth = 200;
        //public const int TrademarkToolTipWidth = 300;

        public string? Country { get; set; }
        public float Latitude { get; set; }
        public float Longitude { get; set; }
        
    }
}
