using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LawPortal.Web
{
    public class DetailPageViewModel<T> : DetailPagePermission where T: class
    {
      
        public T Detail { get; set; }
        
    }

    
}
