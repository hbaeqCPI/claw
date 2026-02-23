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
    public class TmkTrademarkFamilyTreeViewModel : TmkTrademarkDetail
    {
        public string? CaseTypeDescription { get; set; }
        public string? ClientName { get; set; }
        public string? CountryName { get; set; }
        public string? ParentCase { get; set; }
        public bool IsActive { get; set; }
        public List<GoodsExportViewModel>? TrademarkClasses { get; set; } = new List<GoodsExportViewModel>();
    }
}
