using Newtonsoft.Json.Linq;
using R10.Core.Identity;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace R10.Web.Models.DashboardViewModels
{
    public class UserWidgetViewModel
    {
        public CPiUserWidget CPiUserWidget { get; set; }

        public bool HasRespOffice { get; set; }
        public CPiEntityType EntityFilterType { get; set; }

        public bool IsAdmin { get; set; }
        public IEnumerable<Claim> UserRoles { get; set; }
        public CPiUserType MyUserType { get; set; }

        public string WidgetTitle { get; set; }

        public bool IsExport { get; set; } = false;

        public string DetailCode { get; set; }

        public bool isRefresh { get; set; }
    }
}
