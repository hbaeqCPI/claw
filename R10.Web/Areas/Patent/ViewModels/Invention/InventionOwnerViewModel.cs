using R9.Core.Entities;
using R9.Core.Entities.Patent;
using R9.Web.Areas.Patent.Controllers;
using R9.Web.Areas.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R9.Web.Areas.Patent.ViewModels
{
    public class InventionOwnerViewModel : PatOwnerInvDetail
    {
        [UIHint("Owner")]
        public OwnerListViewModel Owner { get; set; }

        //public string OwnerCode { get; set; }

        //public string OwnerName { get; set; }
        public bool ReadOnly { get; set; }
    }
}
