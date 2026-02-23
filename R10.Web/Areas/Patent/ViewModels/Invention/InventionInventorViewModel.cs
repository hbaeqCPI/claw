using R9.Core.Entities.Patent;
using R9.Web.Areas.Patent.Controllers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R9.Web.Areas.Patent.ViewModels
{
    public class InventionInventorViewModel : PatInventorInvDetail
    {
        [UIHint("PatInventor")]
        public PatInventorListViewModel InventorInvInventor { get; set; }

        //public string Inventor { get; set; }

        public bool ReadOnly { get; set; }
    }
}
