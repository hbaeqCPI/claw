using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace R10.Web.ViewComponents
{
    public class ComboBoxList : ViewComponent
    {

        public IViewComponentResult Invoke(string name, string textProperty, string valueProperty, 
                                           string screen, string defaultValue="", string requiredRelation = "", 
                                           int width = 100, string area = "", string controller = "", string action = "", int listWidth = 300, 
                                           string onChange="", string codeProperty = "", string descProperty = "",bool required=false)
        {
            if (string.IsNullOrEmpty(valueProperty))
                valueProperty = textProperty;

            var model = new ComboBoxOptions
            {
                Name = name,
                DataTextProperty = string.IsNullOrEmpty(textProperty) ? name : textProperty,
                ListFilterType = FilterType.StartsWith,
                DefaultValue = defaultValue,
                Id = $"{name.Replace(".", "_")}_{screen}",
                RequiredRelation = requiredRelation,
                Area=area,
                Controller=controller,
                Action=action,
                Width = width,
                ListWidth = listWidth,
                OnChange=onChange,
                Required = required
            };
            model.DataValueProperty = string.IsNullOrEmpty(valueProperty) ? model.DataTextProperty : valueProperty;
            model.CodeProperty = string.IsNullOrEmpty(codeProperty) ? model.DataValueProperty : codeProperty;
            model.DescProperty = string.IsNullOrEmpty(descProperty) ? model.CodeProperty : descProperty;
            return View(model);
        }
    }


}
