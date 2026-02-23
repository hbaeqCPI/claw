using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using R9.Core.Entities.Patent;

namespace R9.Web.Areas.Patent.ViewModels
{
    
    public class PatActionTypeViewModel : PatActionTypeDetail
    {
        public string CountryName { get; set; }
        public string ResponsibleCode { get; set; }
        public string ResponsibleName { get; set; }
    }
}
