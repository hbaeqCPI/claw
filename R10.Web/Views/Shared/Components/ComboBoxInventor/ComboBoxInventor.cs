using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.ViewComponents
{
    public class ComboBoxInventor : ViewComponent
    {
        public IViewComponentResult Invoke(string name, string textProperty, string valueProperty, string screen,
                                           string defaultValue = "", string defaultText = "", string requiredRelation = "", int width = 100, string area = "", string controller = "", string action = "",
                                           int listWidth = 300, bool showLinkButton = false)
        {
            if (string.IsNullOrEmpty(valueProperty))
                valueProperty = textProperty;

            var model = new ComboBoxOptions
            {
                Name = name,
                DataTextProperty = textProperty,
                DataValueProperty = valueProperty,
                ListFilterType = FilterType.Contains,
                Controller = string.IsNullOrEmpty(controller) ? "PatInventor" : controller,
                Action = string.IsNullOrEmpty(action) ? "GetInventorsList" : action,
                Area = string.IsNullOrEmpty(area) ? "Patent" : area,
                DefaultValue = defaultValue,
                DefaultText = defaultText,
                Id = $"{name.Replace(".", "_")}_{screen}",
                RequiredRelation = requiredRelation,
                Width = showLinkButton ? width - (int)Math.Floor((double)width * .3) : width, //30% for the link button
                ListWidth = listWidth,
                LinkButtonUrl = showLinkButton ? Url.Action("DetailLink", "PatInventor", new { Area = "Patent", Inventor = "actualValue" }) : ""
            };

            return View(model);
        }

    }
}
