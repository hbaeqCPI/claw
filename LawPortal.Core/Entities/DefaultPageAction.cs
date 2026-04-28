using System;
using System.Collections.Generic;
using System.Text;

namespace LawPortal.Core.Entities
{

    public class DefaultPageAction
    {
        public int PageId { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
        public Dictionary<string, string> Route { get; set; }
        public string PagePolicy { get; set; }
        public string SettingPolicy { get; set; }
    }
}
