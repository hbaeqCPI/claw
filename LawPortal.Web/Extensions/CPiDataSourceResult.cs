using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LawPortal.Web.Extensions
{
    public class CPiDataSourceResult : DataSourceResult
    {
        public int[] Ids { get; set; }

        /// <summary>
        /// One key per row, in result order, for the detail-page record navigator.
        /// Set this instead of (or as well as) <see cref="Ids"/> on screens whose
        /// records are not keyed on a single int — see
        /// <see cref="RecordNavigationKey"/>. The client prefers Keys when present.
        /// </summary>
        public string[] Keys { get; set; }
       
    }
}
