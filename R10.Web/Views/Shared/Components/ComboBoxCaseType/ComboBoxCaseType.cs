using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.ViewComponents
{
    public class ComboBoxCaseType : ViewComponent
    {

        public IViewComponentResult Invoke(string name, string textProperty, string valueProperty, string system, 
                                           string screen, string defaultValue="", string requiredRelation = "", 
                                           int width = 100, string area = "", string controller = "", string action = "", int listWidth = 300, 
                                           bool showLinkButton=false, string onChange="",string paramsProvider="", bool required=false)
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
                ParamsProvider= paramsProvider,
                Required=required
            };

            if (string.IsNullOrEmpty(controller))
            {
                switch (system.ToLower())
                {
                    case "trademark":
                        model.Controller = "CaseType";
                        model.Area = "Trademark";
                        break;

                    case "generalmatter":
                        model.Controller = "MatterType";
                        model.Area = "GeneralMatter";
                        break;

                    default:
                        model.Controller = "CaseType";
                        model.Area = "Patent";
                        break;
                }
            }
            else {
                model.Controller = controller;
                model.Area = area;
            }

            if (string.IsNullOrEmpty(action))
                model.Action = "GetCaseTypeList";
            else
                model.Action = action;

            model.LinkButtonUrl = showLinkButton ? Url.Action("DetailLink", model.Controller, new { Area = model.Area, caseType = "actualValue" }) : "";
            return View(model);
        }
    }


}
