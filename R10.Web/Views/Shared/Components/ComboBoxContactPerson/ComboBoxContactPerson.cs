using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.ViewComponents
{
    public class ComboBoxContactPerson : ViewComponent
    {
        public IViewComponentResult Invoke(string name, string textProperty, string valueProperty, string screen, int? defaultValue = null, string defaultText = "",
                                           string requiredRelation = "", int width = 100, string area = "", string controller = "", string action = "", 
                                           int listWidth = 300, string linkButtonUrl = "")
        {
            if (string.IsNullOrEmpty(valueProperty))
                valueProperty = textProperty;

            var model = new ComboBoxOptions
            {
                Name = name,
                DataTextProperty = textProperty,
                DataValueProperty = valueProperty,
                ListFilterType = FilterType.StartsWith,
                Controller = string.IsNullOrEmpty(controller) ? "ContactPerson" : controller,
                Area = string.IsNullOrEmpty(area) ? "Shared" : area,
                Action = string.IsNullOrEmpty(action) ? "GetContactList" : action,
                DefaultValue = defaultValue.ToString(),
                DefaultText = defaultText,
                Id = $"{name.Replace(".", "_")}_{screen}",
                RequiredRelation = requiredRelation,
                Width = (linkButtonUrl.Length > 0) ? width - (int)Math.Floor((double)width * .3) : width, //30% for the link button
                ListWidth = listWidth,
                LinkButtonUrl = linkButtonUrl
            };
            return View(model);
        }
    }
    
}
