using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LawPortal.Web.Areas.Shared.ViewModels
{
    public class ChildViewModel
    {
        public int Id { get; set; }
        public string? Code { get; set; }
        public DetailPagePermission? Permission { get; set; }
    }
}
