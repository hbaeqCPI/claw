using LawPortal.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LawPortal.Web.Models.MenuViewModels
{
    public class MenuItemViewModel : CPiMenuItem
    {
        public string AbsoulteUri
        {
            get
            {
                //todo: sanitize uri                
                if (Uri.IsWellFormedUriString(Url, UriKind.Absolute))
                {
                    return Url;
                }
                return string.Empty;
            }
        }

        public IEnumerable<MenuItemViewModel> SubMenuItems { get; set; }
    }
}
