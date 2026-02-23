using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.ViewComponents
{
    public class ComboBoxClient : ViewComponent
    {
        public IViewComponentResult Invoke(string name, string textProperty, string valueProperty, string screen,
                                           int? defaultValue = null, string defaultText = "", string requiredRelation = "", int width = 100, string area = "", string controller = "", string action = "",
                                           int listWidth = 300, bool showLinkButton = false, string data = "")
        {
            if (string.IsNullOrEmpty(valueProperty))
                valueProperty = textProperty;

            var model = new ComboBoxOptions
            {
                Name = name,
                DataTextProperty = textProperty,
                DataValueProperty = valueProperty,
                ListFilterType = FilterType.StartsWith,
                Controller = string.IsNullOrEmpty(controller) ? "Client" : controller,
                Action = string.IsNullOrEmpty(action) ? "GetClientsList" : action,
                Area = string.IsNullOrEmpty(area) ? "Shared" : area,
                DefaultValue = defaultValue.ToString(),
                DefaultText = defaultText,
                Id = $"{name.Replace(".", "_")}_{screen}",
                RequiredRelation = requiredRelation,
                Width = showLinkButton ? width - (int)Math.Floor((double)width * .3) : width, //30% for the link button
                ListWidth = listWidth,
                LinkButtonUrl = showLinkButton ? Url.Action("DetailLink", "Client", new { Area = "Shared", clientId = "actualValue" }) : "",
                Data = data
            };

            return View(model);
        }
    }

}
