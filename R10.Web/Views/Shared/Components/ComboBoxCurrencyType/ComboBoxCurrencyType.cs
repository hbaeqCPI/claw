using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.ViewComponents
{
    public class ComboBoxCurrencyType : ViewComponent
    {

        public IViewComponentResult Invoke(string name, string textProperty, string valueProperty, string system, 
                                           string screen, string defaultValue="", string requiredRelation = "", 
                                           int width = 100, string area = "", string controller = "", string action = "", int listWidth = 300, 
                                           bool showLinkButton=false, string onChange="",string paramsProvider="")
        {
            if (string.IsNullOrEmpty(valueProperty))
                valueProperty = textProperty;


            var model = new ComboBoxOptions
            {
                Name = name,
                DataTextProperty = textProperty,
                DataValueProperty = valueProperty,
                ListFilterType = FilterType.StartsWith,
                Controller = string.IsNullOrEmpty(controller) ? "CurrencyType" : controller,
                Action = string.IsNullOrEmpty(action) ? "GetCurrencyTypesList" : action,
                Area = string.IsNullOrEmpty(area) ? "Shared" : area,
                DefaultValue = defaultValue,
                Id = $"{name.Replace(".", "_")}_{screen}",
                RequiredRelation = requiredRelation,
                Width = showLinkButton ? width - (int)Math.Floor((double)width * .3) : width, //30% for the link button
                ListWidth = listWidth,
                OnChange=onChange,
                ParamsProvider= paramsProvider,
                LinkButtonUrl = showLinkButton ? Url.Action("DetailLink", "CurrencyType", new { Area = "Shared", currencyType = "actualValue" }) : ""
            };
            return View(model);
        }
    }


}
