using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Views.Shared.Components.ListBox
{
    public class ListBox : ViewComponent
    {
        public ListBox()
        {

        }

        public IViewComponentResult Invoke(string name, string property, string controller, string area, string screen, string action = "", string textProperty = "", string valueProperty = "", string defaultValue = "", int minLength = 0,
                                   string requiredRelation = "", int width = 100, string height="150px", int listWidth = 0, bool enablePaging = false, string valueMapper = "",
                                   //string onChangeEvent = "", 
                                   string onOpen = "", string data = "",
                                   string onChange = "", string paramsProvider = "", bool showLinkButton = false,
                                   string linkButtonURL = "", string linkButtonData = "",
                                   string headerTemplate = "", string itemTemplate = "", bool serverFiltering = false,
                                   bool limitToList = false, string defaultText = "",
                                   bool required = false, string validationMessage = "", string limitToListMessage = "",
                                   string onSelect = "")
        {
            var pageId = screen;
            if (string.IsNullOrEmpty(screen) && ViewBag.PageId != null)
                pageId = ViewBag.PageId.ToString();

            var model = new ListBoxOptions
            {
                Property = property,
                ListFilterType = FilterType.StartsWith,
                DataTextProperty = string.IsNullOrEmpty(textProperty) ? property : textProperty,
                //DataTextProperty = textProperty,
                DataValueProperty = string.IsNullOrEmpty(valueProperty) ? string.IsNullOrEmpty(textProperty) ? property : textProperty : valueProperty,
                DefaultValue = defaultValue,
                DefaultText = string.IsNullOrEmpty(defaultText) ? defaultValue : defaultText,
                Controller = controller,
                Action = action,
                Area = area,
                MinLength = minLength,
                Id = $"{name.Replace(".", "_")}_{pageId}",
                RequiredRelation = requiredRelation,
                Width = width,
                //Width = showLinkButton ? width - 10 : width,
                ListWidth = listWidth,
                Height = height,
                EnablePaging = enablePaging,
                ValueMapper = valueMapper,
                OnOpen = onOpen,
                Data = data,
                Name = name,
                OnChange = onChange,
                ParamsProvider = paramsProvider,
                HeaderTemplate = headerTemplate,
                ItemTemplate = itemTemplate,
                ServerFiltering = serverFiltering,
                LimitToList = limitToList,
                //LinkButtonUrl = showLinkButton ? Url.Action("DetailLink", controller, new { Area = area, id = "actualValue" }) : "",
                LinkButtonUrl = linkButtonURL,
                LinkButtonData = linkButtonData,
                ShowLinkButton = !string.IsNullOrEmpty(linkButtonURL) ? true : showLinkButton,
                Required = required,
                ValidationMessage = validationMessage,
                LimitToListMessage = limitToListMessage,
                OnSelect = onSelect
            };
            //model.DataValueProperty = string.IsNullOrEmpty(valueProperty) ? model.DataTextProperty : valueProperty;


            //onChange vs onChangeEvent ????
            //if (string.IsNullOrEmpty(onChangeEvent))
            //    model.OnChange = "page.doNothing";
            //else
            //    model.OnChange = onChangeEvent;

            if (string.IsNullOrEmpty(action))
                model.Action = "GetPicklistData";
            else
                model.Action = action;

            return View(model);
        }
    }

    

    public class ListBoxOptions
    {
        public string Property { get; set; }
        public string Screen { get; set; }
        public FilterType ListFilterType { get; set; }
        public int MinLength { get; set; }
        public string Action { get; set; }
        public string Controller { get; set; }
        public string Area { get; set; }
        public string RequiredRelation { get; set; }
        public string Name { get; set; }
        public string DataTextProperty { get; set; }
        public string DataValueProperty { get; set; }
        public string DefaultValue { get; set; }
        public string DefaultText { get; set; }
        public string Id { get; set; }
        public int Width { get; set; }
        public int ListWidth { get; set; }
        public string Height { get; set; }
        public string LinkButtonUrl { get; set; }
        public string ParamsProvider { get; set; }
        public bool EnablePaging { get; set; }
        public int PageSize { get; set; }
        public string ValueMapper { get; set; }
        public string CodeProperty { get; set; }
        public string DescProperty { get; set; }
        public string Data { get; set; }
        public string OnChange { get; set; }
        public string OnOpen { get; set; }
        public string OnSelect { get; set; }
        public string HeaderTemplate { get; set; }
        public string ItemTemplate { get; set; }
        public bool ServerFiltering { get; set; }
        public bool LimitToList { get; set; }
        public bool Required { get; set; }
        public bool ShowLinkButton { get; set; }
        public string LinkButtonData { get; set; }
        public string ValidationMessage { get; set; }
        public string LimitToListMessage { get; set; }
    }
}
