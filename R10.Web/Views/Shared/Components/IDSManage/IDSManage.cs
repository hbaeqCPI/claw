using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.ViewComponents
{
    public class IDSManage : ViewComponent
    {
        public IViewComponentResult Invoke(IDSManageOptions model)
        {
            return View(model);
        }
    }

    public class IDSManageOptions
    {
        public int AppId { get; set; }
        public string CaseNumber { get; set; }
        [Display(Name = "Country")]
        public string Country { get; set; }
        [Display(Name = "Sub Case")]
        public string SubCase { get; set; }
        public DetailPagePermission Permission { get; set; }
    }
}
