using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class QueryFilterViewModel
    {
        public string? Property { get; set; }
        public string? Operator { get; set; }

        private string? propertyValue;
        public string? Value
        {
            get => propertyValue;
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    propertyValue = value;
                    return;
                }
                propertyValue = value.Replace("*", "%").Replace("?", "_");
                if (propertyValue.Contains("["))
                {
                    if (!(propertyValue.StartsWith("[") && propertyValue.EndsWith("]") && propertyValue.Contains(",")))
                        propertyValue = propertyValue.Replace("[", "[[]"); //escape [
                }
            }
        }

    }
}
