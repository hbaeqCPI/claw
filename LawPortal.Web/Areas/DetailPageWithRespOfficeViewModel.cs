using LawPortal.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LawPortal.Web
{
    public class DetailPageWithRespOfficeViewModel<T> : DetailPagePermission where T: BaseEntityWithRespOffice
    {
        public T Detail { get; set; }
    }

    
}
