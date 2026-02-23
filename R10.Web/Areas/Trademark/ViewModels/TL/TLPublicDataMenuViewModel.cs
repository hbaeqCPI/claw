using R10.Core.Entities.Trademark;
using System.Collections.Generic;

namespace R10.Web.Areas.Trademark.ViewModels
{
    public class TLPublicDataMenuViewModel
    {
        public int TmkId { get; set; }
        public int TLTmkId { get; set; }
        public string ActiveTab { get; set; }
        public List<TLInfoSettingsMenu> InfoMenu { get; set; }

    }
}
