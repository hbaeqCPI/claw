using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.ViewComponents
{
    public class ComboBoxModule : ViewComponent
    {
        public IViewComponentResult Invoke(string name, string textProperty, string valueProperty, string screen,
                                           string defaultValue = "", string requiredRelation = "", int width = 100, string area = "", string controller = "", string action = "",
                                           int listWidth = 300, bool showLinkButton = false)
        {
            if (string.IsNullOrEmpty(valueProperty))
                valueProperty = textProperty;

            var model = new ComboBoxOptions
            {
                Name = name,
                DataTextProperty = string.IsNullOrEmpty(textProperty) ? name : textProperty,
                DataValueProperty = valueProperty,
                ListFilterType = FilterType.Contains,
                Controller = string.IsNullOrEmpty(controller) ? "Module" : controller,
                Action = string.IsNullOrEmpty(action) ? "GetModulesList" : action,
                Area = string.IsNullOrEmpty(area) ? "Shared" : area,
                DefaultValue = defaultValue,
                Id = $"{name.Replace(".", "_")}_{screen}",
                RequiredRelation = requiredRelation,
                Width = showLinkButton ? width - (int)Math.Floor((double)width * .3) : width, //30% for the link button
                ListWidth = listWidth,
                LinkButtonUrl = showLinkButton ? Url.Action("DetailLink", "Module", new { Area = "Shared", value = "actualValue" }) : ""
            };

            return View(model);
        }
    }

}
