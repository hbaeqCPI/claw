using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Api.Models
{
    public class ApiResult<T>
    {
        //can't serialize IEnumerable to xml
        //use List<T>
        //public IEnumerable Data { get; set; }
        public List<T>? Data { get; set; }
        public int Total { get; set; }
    }
}
