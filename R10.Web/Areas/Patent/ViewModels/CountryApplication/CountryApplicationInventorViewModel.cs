using R9.Web.Areas.Patent.Controllers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using R9.Core.Entities.Patent;

namespace R9.Web.Areas.Patent.ViewModels
{
    public class CountryApplicationInventorViewModel : PatInventorAppDetail
    {
        [UIHint("PatInventor")]
        public PatInventorListViewModel InventorAppInventor { get; set; }
        public bool ReadOnly { get; set; }
    }
}

