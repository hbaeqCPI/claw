using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Extensions
{
    public class CPiDataSourceResult : DataSourceResult
    {
        public int[] Ids { get; set; }
       
    }
}
