using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kendo.Mvc.UI;

namespace R10.Web.ViewComponents
{
    public class ComboBox : ViewComponent
    {
        public ComboBox()
        {
        }

        public IViewComponentResult Invoke(string name, string controller, string? area, string screen, string action = "", string textProperty = "", string valueProperty = "", string defaultValue = "", int minLength = 0,
                                           string requiredRelation = "", int width = 100, int listWidth = 0, bool enablePaging = false, int pageSize = 8000, string valueMapper = "",
                                           string onOpen = "", string data = "",
                                           string onChange = "", string paramsProvider = "", bool showLinkButton = false,
                                           string linkButtonURL = "", string linkButtonData = "",
                                           string headerTemplate = "", string itemTemplate = "", bool serverFiltering = false,
                                           bool limitToList = false, string defaultText = "",
                                           bool required = false, string validationMessage = "", string limitToListMessage = "",
                                           string onSelect = "", string linkParamName = "", bool valuePrimitive = false, bool linkInNewTab = false,string[]? filterColumns=null,
                                           List<ComboBoxColumn>? columns=null, string onError = "", int filterType = 0,bool ignoreCase=true, bool multiSelect = false)
        {
            var pageId = screen;
            if (string.IsNullOrEmpty(screen) && ViewBag.PageId != null)
                pageId = ViewBag.PageId.ToString();

            var model = new ComboBoxOptions
            {
                Property = name,
                ListFilterType = (FilterType)filterType,
                DataTextProperty = string.IsNullOrEmpty(textProperty) ? name : textProperty,
                DataValueProperty = string.IsNullOrEmpty(valueProperty) ? string.IsNullOrEmpty(textProperty) ? name : textProperty : valueProperty,
                DefaultValue = defaultValue,
                DefaultText = string.IsNullOrEmpty(defaultText) ? defaultValue : defaultText,
                Controller = controller,
                Action = action,
                Area = area,
                MinLength = minLength,
                Id = $"{name.Replace(".", "_")}_{pageId}",
                RequiredRelation = requiredRelation,
                Width = width,
                ListWidth = listWidth,
                EnablePaging = enablePaging,
                PageSize = pageSize,
                ValueMapper= valueMapper,
                OnOpen = onOpen,
                Data = data,
                Name = name,
                OnChange = onChange,
                ParamsProvider = paramsProvider,
                HeaderTemplate = headerTemplate,
                ItemTemplate = itemTemplate,
                ServerFiltering = serverFiltering,
                LimitToList = limitToList,
                LinkButtonUrl = linkButtonURL,
                LinkButtonData = linkButtonData,
                ShowLinkButton = !string.IsNullOrEmpty(linkButtonURL) ? true : showLinkButton,
                Required = required,
                ValidationMessage = validationMessage,
                LimitToListMessage = limitToListMessage,
                OnSelect = onSelect,
                LinkParamName = linkParamName,
                ValuePrimitive = valuePrimitive,
                LinkInNewTab= linkInNewTab,
                FilterColumns = filterColumns,
                Columns=columns,
                OnError = onError,
                IgnoreCase=ignoreCase,
                MultiSelect = multiSelect
            };

            if (string.IsNullOrEmpty(action))
                model.Action = "GetPicklistData";
            else
                model.Action = action;

            return View(model);
        }
    }


    public class ComboBoxOptions
    {
        public string? Property { get; set; }
        public string? Screen { get; set; }
        public FilterType ListFilterType { get; set; }
        public int MinLength { get; set; }
        public string? Action { get; set; }
        public string? Controller { get; set; }
        public string? Area { get; set; }
        public string? RequiredRelation  { get; set; }
        public string? Name { get; set; }
        public string? DataTextProperty { get; set; }
        public string? DataValueProperty { get; set; }
        public string? DefaultValue { get; set; }
        public string? DefaultText { get; set; }
        public string? Id { get; set; }
        public int Width { get; set; }
        public int ListWidth { get; set; }
        public string? LinkButtonUrl { get; set; }
        public string? ParamsProvider { get; set; }
        public bool EnablePaging { get; set; }
        public int PageSize { get; set; }
        public string? ValueMapper { get; set; }
        public string? CodeProperty { get; set; }
        public string? DescProperty { get; set; }
        public string? Data { get; set; }
        public string? OnChange { get; set; }
        public string? OnOpen { get; set; }
        public string? OnSelect { get; set; }
        public string? OnError { get; set; }
        public string? HeaderTemplate { get; set; }
        public string? ItemTemplate { get; set; }
        public bool ServerFiltering { get; set; }
        public bool LimitToList { get; set; }
        public bool Required { get; set; }
        public bool ShowLinkButton { get; set; }
        public string? LinkButtonData { get; set; }
        public string? ValidationMessage { get; set; }
        public string? LimitToListMessage { get; set; }
        public string? LinkParamName { get; set; }
        public bool ValuePrimitive { get; set; }
        public bool LinkInNewTab { get; set; }
        public string[]? FilterColumns { get; set; }
        public List<ComboBoxColumn>? Columns { get; set; }
        public bool IgnoreCase { get; set; } = true;
        public bool MultiSelect { get; set; }
    }

    public class ComboBoxColumn
    {
        public string? Name { get; set; }
        public string? Header { get; set; }
        public string? Template { get; set; }
        public int? Width { get; set; }
    }
}