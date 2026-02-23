using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.ViewComponents
{
    public class ComboBoxCostType : ViewComponent
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
                DefaultValue = defaultValue,
                Id = $"{name.Replace(".", "_")}_{screen}",
                RequiredRelation = requiredRelation,
                Width = showLinkButton ? width - (int)Math.Floor((double)width * .3) : width, //30% for the link button
                ListWidth = listWidth,
                OnChange=onChange,
                ParamsProvider= paramsProvider
            };

            if (string.IsNullOrEmpty(controller))
            {
                switch (system.ToLower())
                {
                    case "trademark":
                        model.Controller = "TmkCostType";
                        model.Area = "Trademark";
                        break;

                    case "generalmatter":
                        model.Controller = "MatterType";
                        model.Area = "GeneralMatter";
                        break;

                    default:
                        model.Controller = "PatCostType";
                        model.Area = "Patent";
                        break;
                }
            }
            else {
                model.Controller = controller;
                model.Area = area;
            }

            if (string.IsNullOrEmpty(action))
                model.Action = "GetCostTypesList";
            else
                model.Action = action;

            if (string.IsNullOrEmpty(onChange))
                model.OnChange = "page.handleComboBoxInvalidEntry";
            else
                model.OnChange = onChange;

            model.LinkButtonUrl = showLinkButton ? Url.Action("DetailLink", model.Controller, new { Area = model.Area, costType = "actualValue" }) : "";
            return View(model);
        }
    }


}
