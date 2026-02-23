using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kendo.Mvc.UI;

namespace R10.Web.ViewComponents
{
    public class ComboBoxCaseNumber : ViewComponent
    {

        public IViewComponentResult Invoke(string name, string textProperty, string valueProperty, string system,
                                           string screen, string defaultValue = "", string requiredRelation = "", bool showLinkButton = false,
                                           int width = 100, string area = "", string controller = "", string action = "", int listWidth = 300,
                                           bool enablePaging = false, int pageSize = 0, string valueMapper = "",
                                           string onChange = "", bool required=false, bool limitToList = true)
        {
            if (string.IsNullOrEmpty(textProperty))
                textProperty = name;

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
                Width = width,
                ListWidth = listWidth,
                EnablePaging = enablePaging,
                PageSize = pageSize,
                ValueMapper = valueMapper,
                Required = required,
                LinkButtonUrl = showLinkButton ? Url.Action("DetailLink", controller, new { Area = area, id = "actualValue" }) : "",
                LimitToList = limitToList           // for search screen, this is false
            };


            if (string.IsNullOrEmpty(controller))
            {
                switch (system.ToLower())
                {
                    case "trademark":
                        model.Controller = "TmkTrademarkLookup";
                        model.Area = "Trademark";
                        break;

                    case "generalmatter":
                        model.Controller = "Matter";
                        model.Area = "GeneralMatter";
                        break;

                    default:
                        model.Controller = "Invention";
                        model.Area = "Patent";
                        break;
                }
            }
            else
            {
                model.Controller = controller;
                model.Area = system;
            }

            if (string.IsNullOrEmpty(action))
                model.Action = "GetCaseNumbersList";
            else
                model.Action = action;

            if (string.IsNullOrEmpty(onChange))
                model.OnChange = "page.handleComboBoxInvalidEntry";
            else
                model.OnChange = onChange;


            return View(model);
        }
    }

}