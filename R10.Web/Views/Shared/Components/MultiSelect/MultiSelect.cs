using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Mvc;
using R10.Web.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.ViewComponents
{
    public class MultiSelect : ViewComponent
    {
        public IViewComponentResult Invoke(MultiSelectOptions model)
        {
            var pageId = model.Screen;
            if (string.IsNullOrEmpty(pageId) && ViewBag.PageId != null)
                pageId = ViewBag.PageId.ToString();

            model.Id = string.IsNullOrEmpty(model.Id) ? $"{model.Name.Replace(".", "_")}_{pageId}" : model.Id;
            model.DataTextProperty = string.IsNullOrEmpty(model.DataTextProperty) ? model.Name : model.DataTextProperty;
            model.DataValueProperty = string.IsNullOrEmpty(model.DataValueProperty) ? string.IsNullOrEmpty(model.DataTextProperty) ? model.Name : model.DataTextProperty : model.DataValueProperty;
            model.Controller = string.IsNullOrEmpty(model.Controller) ? this.ViewContext.RouteData.Values["controller"].ToString() : model.Controller;
            model.Action = string.IsNullOrEmpty(model.Action) ? "GetPicklistData" : model.Action;
            model.OnOpen = string.IsNullOrEmpty(model.OnOpen) ? "function(e) {{ if (this.element.data('fetched')===0) {{this.dataSource.read();}} this.element.data('fetched',1);}}" : model.OnOpen;
            model.PageSize = (model.PageSize <= 0) ? 8000 : model.PageSize;
            model.HeaderTemplate = string.IsNullOrEmpty(model.HeaderTemplate) ? "<span></span>" : model.HeaderTemplate;
            model.ItemTemplate = string.IsNullOrEmpty(model.ItemTemplate) ? string.IsNullOrEmpty(model.DataTextProperty) ? "<span class='k-state-default'>#: data #</span>" : $"<span class='k-state-default'>#: data.{model.DataTextProperty} #</span>" : model.ItemTemplate;

            return View(model);
        }
    }

    public class MultiSelectOptions
    {
        public MultiSelectOptions(string name, string controller = "", string? area = null, bool allowCustom = false, string textProperty = "", string action = "", string requiredRelation = "", string valueProperty = "", string data = "", string[] defaultValue = null, bool? enable = true)
        {
            Name = name;
            Controller = controller;
            Area = area;
            AllowCustom = allowCustom;
            DataTextProperty = textProperty;
            DataValueProperty = valueProperty;
            Action = action;
            RequiredRelation = requiredRelation;
            Data = data;
            DefaultValue = defaultValue;
            Enable = enable;
        }

        public MultiSelectOptions() { }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Screen { get; set; }
        public FilterType ListFilterType { get; set; } = FilterType.StartsWith;
        public string Action { get; set; }
        public string Controller { get; set; }
        public string? Area { get; set; }
        public string RequiredRelation { get; set; }
        public string DataTextProperty { get; set; }
        public string DataValueProperty { get; set; }
        public string[] DefaultValue { get; set; }
        public int Width { get; set; } = 100;
        public int ListWidth { get; set; }
        public bool EnablePaging { get; set; }
        public int PageSize { get; set; }
        public string ValueMapper { get; set; }
        public string Data { get; set; }
        public string OnChange { get; set; }
        public string OnOpen { get; set; }
        public string OnSelect { get; set; }
        public string HeaderTemplate { get; set; }
        public string ItemTemplate { get; set; }
        public bool ServerFiltering { get; set; }
        public bool Required { get; set; }
        public string ValidationMessage { get; set; }
        public string[] FilterColumns { get; set; }

        public bool AllowCustom { get; set; }
        public bool? Enable {  get; set; }

        /// <summary>
        /// Country pick list with data pulled by specified controller.
        /// Defaults to current controller.
        /// </summary>
        public static Object Country(string name, string countryLabel, string countryNameLabel, string controller = "", string[] defaultValue = null, string? area = null, string requiredRelation = "", bool allowCustom = false, string action = "", bool? enable = true)
            => new MultiSelectOptions(name)
            {
                Controller = controller,
                Area = area,
                Action = string.IsNullOrEmpty(action) ? "GetCountryList" : action,
                DataTextProperty = "Country",
                DefaultValue = defaultValue,
                ListWidth = 500,
                HeaderTemplate = ComboBoxHelper.GetHeaderTemplate(countryLabel, countryNameLabel),
                ItemTemplate = ComboBoxHelper.GetItemTemplate("Country", "CountryName"),
                RequiredRelation = requiredRelation,
                AllowCustom = allowCustom,
                Enable = enable
            };

        public static Object AreaId(string name, string areaLabel, string areaNameLabel, string controller = "", string[]? defaultValue = null, string? area = null, bool? enable = true, string action = "", bool enablePaging = false, string valueMapper = "")
            => new MultiSelectOptions(name)
            {
                Controller = controller,
                Area = area,
                Action = string.IsNullOrEmpty(action) ? "GetAreaList" : action,
                DataTextProperty = "Area",
                DataValueProperty = "Area",
                DefaultValue = defaultValue,
                HeaderTemplate = ComboBoxHelper.GetHeaderTemplate(areaLabel, areaNameLabel),
                ItemTemplate = ComboBoxHelper.GetItemTemplate("Area", "Description"),
                Enable = enable,
                EnablePaging = enablePaging,
                ValueMapper = valueMapper
            };

        /// <summary>
        /// Area pick list with data pulled by specified controller.
        /// Defaults to current controller.
        /// </summary>
        public static Object AreaCode(string name, string areaLabel, string areaDescriptionLabel, string controller = "", string[] defaultValue = null, bool? enable = true)
            => new MultiSelectOptions(name)
            {
                Controller = controller,
                Action = "GetAreaList",
                DataTextProperty = "Area",
                DefaultValue = defaultValue,
                ListWidth = 500,
                HeaderTemplate = ComboBoxHelper.GetHeaderTemplate(areaLabel, areaDescriptionLabel),
                ItemTemplate = ComboBoxHelper.GetItemTemplate("Area", "Description"),
                Enable = enable
            };

        /// <summary>
        /// Client pick list with data pulled by specified controller.
        /// Defaults to current controller.
        /// </summary>
        public static Object ClientCode(string name, string clientLabel, string clientNameLabel, string controller = "", string[]? defaultValue = null, string? area = null, bool allowCustom = false, string action = "", bool? enable = true)
            => new MultiSelectOptions(name)
            {
                Controller = controller,
                Area = area,
                Action = string.IsNullOrEmpty(action) ? "GetClientList": action,
                DataTextProperty = "ClientCode",
                DefaultValue = defaultValue,
                ListWidth = 500,
                HeaderTemplate = ComboBoxHelper.GetHeaderTemplate(clientLabel, clientNameLabel),
                ItemTemplate = ComboBoxHelper.GetItemTemplate("ClientCode", "ClientName"),
                AllowCustom = allowCustom,
                Enable = enable
            };

        public static Object ClientId(string name, string clientLabel, string clientNameLabel, string controller = "", string[]? defaultValue = null, string? area = null, bool? enable = true, bool enablePaging = false, string valueMapper = "")
            => new MultiSelectOptions(name)
            {
                Controller = controller,
                Area = area,
                Action = "GetClientList",
                DataTextProperty = "ClientCode",
                DataValueProperty = "ClientID",
                DefaultValue = defaultValue,
                HeaderTemplate = ComboBoxHelper.GetHeaderTemplate(clientLabel, clientNameLabel),
                ItemTemplate = ComboBoxHelper.GetItemTemplate("ClientCode", "ClientName"),
                Enable = enable,
                EnablePaging = enablePaging,
                ValueMapper = valueMapper
            };

        public static Object AttorneyCode(string name, string attorneyLabel, string attorneyNameLabel, string controller = "", string[]? defaultValue = null, string? area = null, bool allowCustom = false, string action = "", bool? enable = true)
            => new MultiSelectOptions(name)
            {
                Controller = controller,
                Area = area,
                Action = string.IsNullOrEmpty(action) ? "GetAttorneyList" : action,
                DataTextProperty = "AttorneyCode",
                DefaultValue = defaultValue,
                ListWidth = 500,
                HeaderTemplate = ComboBoxHelper.GetHeaderTemplate(attorneyLabel, attorneyNameLabel),
                ItemTemplate = ComboBoxHelper.GetItemTemplate("AttorneyCode", "AttorneyName"),
                AllowCustom = allowCustom,
                Enable = enable
            };

        public static Object AttorneyId(string name, string attorneyLabel, string attorneyNameLabel, string controller = "", string[]? defaultValue = null, string? area = null, bool? enable = true)
            => new MultiSelectOptions(name)
            {
                Controller = controller,
                Area = area,
                Action = "GetAttorneyList",
                DataTextProperty = "AttorneyCode",
                DataValueProperty = "AttorneyID",
                DefaultValue = defaultValue,
                HeaderTemplate = ComboBoxHelper.GetHeaderTemplate(attorneyLabel, attorneyNameLabel),
                ItemTemplate = ComboBoxHelper.GetItemTemplate("AttorneyCode", "AttorneyName"),
                Enable = enable
            };

        public static Object AgentCode(string name, string agentLabel, string agentNameLabel, string controller = "", string[]? defaultValue = null, string? area = null, bool allowCustom = false, bool? enable = true)
            => new MultiSelectOptions(name)
            {
                Controller = controller,
                Area = area,
                Action = "GetAgentList",
                DataTextProperty = "AgentCode",
                DefaultValue = defaultValue,
                ListWidth = 500,
                HeaderTemplate = ComboBoxHelper.GetHeaderTemplate(agentLabel, agentNameLabel),
                ItemTemplate = ComboBoxHelper.GetItemTemplate("AgentCode", "AgentName"),
                AllowCustom = allowCustom,
                Enable = enable
            };

        public static Object OwnerCode(string name, string ownerLabel, string ownerNameLabel, string controller = "", string[]? defaultValue = null, string? area = null, bool allowCustom = false, bool? enable = true)
            => new MultiSelectOptions(name)
            {
                Controller = controller,
                Area = area,
                Action = "GetOwnerList",
                DataTextProperty = "OwnerCode",
                DefaultValue = defaultValue,
                ListWidth = 500,
                HeaderTemplate = ComboBoxHelper.GetHeaderTemplate(ownerLabel, ownerNameLabel),
                ItemTemplate = ComboBoxHelper.GetItemTemplate("OwnerCode", "OwnerName"),
                AllowCustom = allowCustom,
                Enable = enable
            };


        public static Object CaseType(string name, string caseTypeLabel, string caseTypeNameLabel, string controller = "", string[]? defaultValue = null, string? area = null, bool? enable = true)
            => new MultiSelectOptions(name)
            {
                Controller = controller,
                Area = area,
                Action = "GetCaseTypeList",
                DataTextProperty = "CaseType",
                DefaultValue = defaultValue,
                ListWidth = 500,
                HeaderTemplate = ComboBoxHelper.GetHeaderTemplate(caseTypeLabel, caseTypeNameLabel),
                ItemTemplate = ComboBoxHelper.GetItemTemplate("CaseType", "Description"),
                Enable = enable
            };

        public static Object MatterType(string name, string matterTypeLabel, string matterTypeNameLabel, string[]? defaultValue = null, bool? enable = true)
            => new MultiSelectOptions(name)
            {
                Controller = "MatterType",
                Area = "GeneralMatter",
                Action = "GetMatterTypeList",
                DataTextProperty = "MatterType",
                DefaultValue = defaultValue,
                ListWidth = 500,
                HeaderTemplate = ComboBoxHelper.GetHeaderTemplate(matterTypeLabel, matterTypeNameLabel),
                ItemTemplate = ComboBoxHelper.GetItemTemplate("MatterType", "Description"),
                Enable = enable
            };

        public static Object RespOffice(string name, string codeLabel, string nameLabel, string[] defaultValue, string? area = null, bool? enable = true)
            => new MultiSelectOptions(name)
             {
                 Controller = "Workflow",
                 Area = area,
                 Action = "GetRespOfficeList",
                 DataTextProperty = "RespOffice",
                 DefaultValue = defaultValue,
                 ListWidth = 500,
                 HeaderTemplate = ComboBoxHelper.GetHeaderTemplate(codeLabel, nameLabel),
                 ItemTemplate = ComboBoxHelper.GetItemTemplate("RespOffice", "Name"),
                 Enable = enable
            };

        //when client has too many records
        public static Object PatActionType(string name)
             => new MultiSelectOptions(name)
             {
                 Name = name,
                 Controller = "ActionDue",
                 Area = "Patent",
                 Action = "GetActionTypeSearchList",
                 ListWidth = 500,
                 ServerFiltering = true,
                 AllowCustom = true
             };

        //when client has too many records
        public static Object PatActionDue(string name)
         => new MultiSelectOptions(name)
         {
             Name = name,
             Controller = "ActionDueDate",
             Area = "Patent",
             Action = "GetActionDueSearchList",
             ListWidth = 500,
             DataTextProperty = "ActionDue",
             DataValueProperty = "ActionDue",
             ServerFiltering = true,
             AllowCustom = true
         };

    }
}
