using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kendo.Mvc.UI;

namespace R10.Web.ViewComponents
{
    public class ComboBoxCascade : ViewComponent
    {
        public ComboBoxCascade()
        {
        }
        public IViewComponentResult Invoke(string name, string controller, string area, string screen, string action = "", string textProperty = "", string valueProperty = "",
                                           string data = "", string headerTemplate = "", string itemTemplate = "", string cascadeFrom = "", string onChange = "")
        {
            var model = new ComboBoxCascadeOptions
            {
                Property = name,
                ListFilterType = FilterType.StartsWith,
                DataTextProperty = textProperty,
                DataValueProperty = string.IsNullOrEmpty(valueProperty)
                    ? string.IsNullOrEmpty(textProperty) ? name : textProperty
                    : valueProperty,
                Controller = controller,
                Action = action,
                Area = area,
                Id = $"{name.Replace(".", "_")}_{screen}",
                Data = data,
                Name = name,
                ItemTemplate = itemTemplate,
                HeaderTemplate = headerTemplate,
                CascadeFrom = cascadeFrom,
                OnChange = onChange
            };

            return View(model);
        }
    }

    public class ComboBoxCascadeOptions
    {
        public string Property { get; set; }
        public string Screen { get; set; }
        public FilterType ListFilterType { get; set; }
        public string Action { get; set; }
        public string Controller { get; set; }
        public string Area { get; set; }
        public string Name { get; set; }
        public string DataTextProperty { get; set; }
        public string DataValueProperty { get; set; }
        public string Id { get; set; }
        public string Data { get; set; }
        public string HeaderTemplate { get; set; }
        public string ItemTemplate { get; set; }
        public bool Required { get; set; }
        public string ValidationMessage { get; set; }

        public string CascadeFrom { get; set; }
        public string OnChange { get; set; }

    }
}



