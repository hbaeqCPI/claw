using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R9.Web.ViewComponents
{
    public class ComboBoxArea : ViewComponent
    {

        public IViewComponentResult Invoke(string name, string textProperty, string valueProperty, string system, 
                                           string screen, string defaultValue="", string requiredRelation = "", 
                                           int width = 100, string area = "", string controller = "", string action = "", int listWidth = 300, bool showLinkButton=false)
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
                ListWidth = listWidth
            };

            if (string.IsNullOrEmpty(controller))
            {
                switch (system.ToLower())
                {
                    case "trademark":
                        model.Controller = "TmkArea";
                        model.Area = "Trademark";
                        break;

                    case "generalmatter":
                        model.Controller = "GMArea";
                        model.Area = "GeneralMatter";
                        break;

                    default:
                        model.Controller = "PatArea";
                        model.Area = "Patent";
                        break;
                }
            }
            else {
                model.Controller = controller;
                model.Area = area;
            }

            if (string.IsNullOrEmpty(action))
                model.Action = "GetAreasList";
            else
                model.Action = action;

           //model.LinkButtonUrl = showLinkButton ? Url.Action("DetailLink", model.Controller, new { Area = model.Area, area = "actualValue" }) : "";
            return View(model);
        }
    }


}
