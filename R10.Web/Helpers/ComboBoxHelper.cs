using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using R10.Web.ViewComponents;

namespace R10.Web.Helpers
{
    public static class ComboBoxHelper
    {
        public static string GetHeaderTemplate(Dictionary<string, int> labels)
        {
            if (labels.Count == 0)
                return "";

            var template = "<div class='dropdown-header k-widget k-header d-flex'>";
            foreach (KeyValuePair<string, int> label in labels)
            {
                if (label.Value > 0)
                    template = template + $"<div style='width:{label.Value}px'>";
                else
                    template = template + $"<div>";

                template = template + $"<span>{label.Key}</span></div>";
            }
            template = template + "</div>";

            return template;
        }
        public static string GetHeaderTemplate(params string[] labels)
        {
            if (labels.Length == 0)
                return "";

            //default column widths
            var args = labels.ToDictionary(x => x, x => 0);
            args[labels[0]] = 120;

            return GetHeaderTemplate(args);
        }


        public static string GetItemTemplate(Dictionary<string, int> columns)
        {
            if (columns.Count == 0)
                return "";

            var template = "<div class='d-flex'>";
            foreach (KeyValuePair<string, int> column in columns)
            {
                var data = column.Key.IndexOf('.') < 0 ? $"data.{column.Key}" : column.Key;

                if (column.Value > 0)
                    template = template + $"<div style='width:{column.Value}px'>";
                else
                    template = template + $"<div>";
                template = template + $"<span class='k-list-item-text'>#: {data} !=null ? {data} : '' #</span></div>";
            }
            template = template + "</div>";

            return template;
        }

        public static string GetItemTemplate(params string[] columns)
        {
            if (columns.Length == 0)
                return "";

            //default column widths
            var args = columns.ToDictionary(x => x, x => 0);
            args[columns[0]] = 120;

            return GetItemTemplate(args);
        }

        public static List<ComboBoxColumn>  BuildComboBoxColumns(string[] labels, string[] columns) {
            var comboColumns = new List<ComboBoxColumn>();
            var count = 0;
            foreach (var label in labels) {
                //default 1st column width to 120px
                comboColumns.Add(new ComboBoxColumn { Name = columns[count], Header = label,Width=count==0 ? 120 : 0});
                count++;
            }
            return comboColumns;
        } 

        /// <summary>
        /// Auxiliary data pick list with multiple columns and link button.
        /// </summary>
        public static Object AuxiliaryWithLink(string name, string[] labels, string[] columns, string controller = "", string action = "", string defaultValue = "", string onSelect = "", string onChange="", string data="", string textProperty="", string validationMessage = "", string? area = null, string valueProperty = "", string defaultText = "")
        {
            return new
            {
                name = name,
                controller = string.IsNullOrEmpty(controller) ? name : controller,
                action = string.IsNullOrEmpty(action) ? $"Get{name}List" : action,
                area = area,
                defaultValue = defaultValue,
                defaultText = string.IsNullOrEmpty(defaultText) ? defaultValue : defaultText,
                limitToList = true,
                listWidth = 500,
                showLinkButton = true,
                onSelect = onSelect,
                onChange = onChange,
                data=data,
                textProperty=textProperty,
                valueProperty= string.IsNullOrEmpty(valueProperty) ? string.IsNullOrEmpty(textProperty) ? name : textProperty : valueProperty,
                required = !string.IsNullOrEmpty(validationMessage),
                validationMessage = validationMessage,
                columns = BuildComboBoxColumns(labels,columns)
            };
        }

        /// <summary>
        /// Country pick list with link button and data pulled by Country controller.
        /// </summary>
        public static Object CountryWithLink(string name, string countryLabel, string countryNameLabel, string defaultValue, bool displayNameOnSelect = true, bool showLinkButton = true, string validationMessage = "")
        {
            return new
            {
                name = name,
                controller = "Country",
                action = "GetCountryList",
                textProperty = "Country",
                defaultValue = defaultValue,
                onChange = displayNameOnSelect ? "function(e) {pageHelper.onComboBoxChangeDisplayName(e,'CountryName');}" : "",
                limitToList = true,
                listWidth = 500,
                showLinkButton = showLinkButton,
                required = !string.IsNullOrEmpty(validationMessage),
                validationMessage = validationMessage,
                columns = new List<ComboBoxColumn> {
                                new ComboBoxColumn{Name="Country",Header=countryLabel,Width=80},
                                new ComboBoxColumn{Name="CountryName",Header=countryNameLabel},
                            }
            };
        }

        /// <summary>
        /// Country pick list with data pulled by specified controller.
        /// Defaults to current controller.
        /// </summary>
        public static Object Country(string name, string countryLabel, string countryNameLabel, string controller = "", string defaultValue = "", string? area = null, string screen = "", string validationMessage = "")
        {
            return new
            {
                name = name,
                controller = controller,
                area = area,
                screen = screen,
                action = "GetCountryList",
                textProperty = "Country",
                defaultValue = defaultValue,
                listWidth = 500,
                required = !string.IsNullOrEmpty(validationMessage),
                validationMessage = validationMessage,
                columns = new List<ComboBoxColumn> {
                                new ComboBoxColumn{Name="Country",Header=countryLabel,Width=80},
                                new ComboBoxColumn{Name="CountryName",Header=countryNameLabel},
                            }
            };
        }

        /// <summary>
        /// Country pick list with data pulled by specified controller.
        /// Defaults to current controller.
        /// </summary>
        public static Object CountryName(string name, string countryLabel, string countryNameLabel, string controller = "", string defaultValue = "", string? area = null, string screen = "", string validationMessage = "")
        {
            return new
            {
                name = name,
                controller = controller,
                area = area,
                screen = screen,
                action = "GetCountryList",
                textProperty = "CountryName",
                defaultValue = defaultValue,
                listWidth = 500,
                required = !string.IsNullOrEmpty(validationMessage),
                validationMessage = validationMessage,
                columns = new List<ComboBoxColumn> {
                                new ComboBoxColumn{Name="Country",Header=countryLabel,Width=80},
                                new ComboBoxColumn{Name="CountryName",Header=countryNameLabel},
                            }
            };
        }

        /// <summary>
        /// Country pick list with data pulled by specified controller.
        /// </summary>
        public static Object CountryID(string name, string countryLabel, string countryNameLabel, string controller, string area, string screen = "", int? defaultValue = null, string defaultText = "")
        {
            return new
            {
                name = name,
                controller = controller,
                area = area,
                screen = screen,
                action = "GetCountryList",
                textProperty = "Country",
                valueProperty = "CountryID",
                defaultValue = defaultValue == null || defaultValue == 0 ? "" : defaultValue.ToString(),
                defaultText = string.IsNullOrEmpty(defaultText) ? "" : defaultText,
                limitToList = true,
                listWidth = 500,
                columns = new List<ComboBoxColumn> {
                                new ComboBoxColumn{Name="Country",Header=countryLabel,Width=80},
                                new ComboBoxColumn{Name="CountryName",Header=countryNameLabel},
                            }
            };
        }

        /// <summary>
        /// PO Box Country pick list with data pulled by specified controller.
        /// Defaults to current controller.
        /// </summary>
        public static Object POCountry(string name, string countryLabel, string countryNameLabel, string controller = "", string defaultValue = "")
        {
            return new
            {
                name = name,
                controller = controller,
                action = "GetPOCountryList",
                textProperty = "Country",
                defaultValue = defaultValue,
                listWidth = 500,
                columns = new List<ComboBoxColumn> {
                                new ComboBoxColumn{Name="Country",Header=countryLabel,Width=80},
                                new ComboBoxColumn{Name="CountryName",Header=countryNameLabel},
                            }
            };
        }

        /// <summary>
        /// Country pick list with data pulled by Country controller.
        /// </summary>
        public static Object SharedCountry(string name, string countryLabel, string countryNameLabel, string requiredRelation = "")
        {
            return new
            {
                name = name,
                controller = "Country",
                action = "GetCountryList",
                textProperty = "Country",
                requiredRelation = requiredRelation,
                listWidth = 500,
                columns = new List<ComboBoxColumn> {
                                new ComboBoxColumn{Name="Country",Header=countryLabel,Width=80},
                                new ComboBoxColumn{Name="CountryName",Header=countryNameLabel},
                            }
            };
        }

        /// <summary>
        /// Country Name pick list with data pulled by Country controller.
        /// </summary>
        public static Object SharedCountryName(string name, string requiredRelation = "")
        {
            return new
            {
                name = name,
                controller = "Country",
                action = "GetCountryList",
                textProperty = "CountryName",
                requiredRelation = requiredRelation
            };
        }

        /// <summary>
        /// Area pick list with data pulled by specified controller.
        /// Defaults to current controller.
        /// </summary>
        public static Object Area(string name, string areaLabel, string areaDescriptionLabel, string controller = "", string defaultValue = "")
        {
            return new
            {
                name = name,
                controller = controller,
                action = "GetAreaList",
                textProperty = "Area",
                defaultValue = defaultValue,
                listWidth = 500,
                columns = new List<ComboBoxColumn> {
                                new ComboBoxColumn{Name="Area",Header=areaLabel,Width=150},
                                new ComboBoxColumn{Name="Description",Header=areaDescriptionLabel},
                            }
            };
        }

        /// <summary>
        /// Area pick list with link button and data pulled by specified controller.
        /// Return AreaID
        /// Return AreaCode when defaultValue == -1
        /// </summary>
        public static Object AreaWithLink(string name, string areaLabel, string areaDescriptionLabel, string area, string controller, int? defaultValue, string defaultText, bool displayNameOnSelect = true, string validationMessage = "", string action = "", string onChange = "", bool showLinkButton = true)
        {
            return new
            {               
                name = name,
                controller = controller,
                area = area,
                action = string.IsNullOrEmpty(action) ? "GetAreaList" : action,
                textProperty = "Area",
                valueProperty = defaultValue == -1 ? "Code" : "AreaID",
                defaultValue = defaultValue == -1 ? (defaultText ?? "") : defaultValue == null || defaultValue == 0 ? "" : defaultValue.ToString(),
                defaultText = string.IsNullOrEmpty(defaultText) ? "" : defaultText,
                onChange = !string.IsNullOrEmpty(onChange) ? onChange : (displayNameOnSelect ? "function(e) {pageHelper.onComboBoxChangeDisplayName(e,'Description');}" : ""),
                limitToList = true,
                listWidth = 500,
                showLinkButton = showLinkButton,
                linkParamName = defaultValue == -1 ? "code" : "",
                validationMessage = validationMessage,
                columns = new List<ComboBoxColumn> {
                                new ComboBoxColumn{Name="Area",Header=areaLabel,Width=100},
                                new ComboBoxColumn{Name="Description",Header=areaDescriptionLabel},
                            }
            };
        }

        /// <summary>
        /// Area pick list with data pulled by Area controller.
        /// </summary>
        public static Object SharedArea(string name, string areaLabel, string areaDescriptionLabel, string requiredRelation = "")
        {
            return new
            {
                name = name,
                controller = "Area",
                action = "GetAreaList",
                textProperty = "Area",
                requiredRelation = requiredRelation,
                listWidth = 500,
                columns = new List<ComboBoxColumn> {
                                new ComboBoxColumn{Name="Area",Header=areaLabel,Width=150},
                                new ComboBoxColumn{Name="Description",Header=areaDescriptionLabel},
                            }

            };
        }

        /// <summary>
        /// Language pick list with link button and data pulled by Language controller.
        /// </summary>
        public static Object LanguageWithLink(string name, string defaultValue, string validationMessage = "")
        {
            return new
            {
                name = name,
                controller = "Language",
                area = "Shared",
                action = "GetLanguageList",
                textProperty = "LanguageName",
                defaultValue = defaultValue,
                limitToList = true,
                showLinkButton = true,
                required = !string.IsNullOrEmpty(validationMessage),
                validationMessage = validationMessage
            };
        }

        /// <summary>
        /// Position pick list with link button and data pulled by Language controller.
        /// </summary>
        public static Object PositionWithLink(string name, string[] labels, string[] columns, string defaultValue, string defaultText, string validationMessage = "")
        {
            return new
            {
                name = name,
                controller = "EmployeePosition",
                area = "Patent",
                action = "GetPositionList",
                textProperty = "Position",
                valueProperty = "PositionId",
                defaultValue = defaultValue,
                defaultText = defaultText,
                limitToList = true,
                listWidth = 500,
                showLinkButton = true,
                required = !string.IsNullOrEmpty(validationMessage),
                validationMessage = validationMessage,
                columns = BuildComboBoxColumns(labels, columns)
            };
        }

        /// <summary>
        /// Client pick list with link button and data pulled by Client controller.
        /// Returns ClientID
        /// Returns ClientCode when defaultValue == -1
        /// </summary>
        public static Object ClientWithLink(string name, string clientLabel, string clientNameLabel, int? defaultValue, string defaultText, bool displayNameOnSelect = true, string validationMessage="", string action="", string onChange="", bool showLinkButton = true)
        {
            return new {
                name = name,
                controller = "Client",
                area = "Shared",
                action = string.IsNullOrEmpty(action) ? "GetClientList" : action,
                textProperty = "ClientCode",
                valueProperty = defaultValue == -1 ? "ClientCode" : "ClientID",
                defaultValue = defaultValue == -1 ? (defaultText ?? "") : defaultValue == null || defaultValue == 0 ? "" : defaultValue.ToString(),
                defaultText = string.IsNullOrEmpty(defaultText) ? "" : defaultText,
                onChange = !string.IsNullOrEmpty(onChange) ? onChange : (displayNameOnSelect ? "function(e) {pageHelper.onComboBoxChangeDisplayName(e,'ClientName');}" : ""),
                limitToList = true,
                listWidth = 500,
                showLinkButton = showLinkButton,
                linkParamName = defaultValue == -1 ? "code" : "",
                validationMessage = validationMessage,
                columns = new List<ComboBoxColumn> {
                                new ComboBoxColumn{Name="ClientCode",Header=clientLabel,Width=100},
                                new ComboBoxColumn{Name="ClientName",Header=clientNameLabel},
                            }
            };
        }

        /// <summary>
        /// Client pick list with data pulled by specified controller.
        /// Defaults to current controller.
        /// </summary>
        public static Object ClientCode(string name, string clientLabel, string clientNameLabel, string controller = "", string defaultValue = "", string? area = null, string screen = "")
        {
            return new
            {
                name = name,
                controller = controller,
                area = area,
                screen = screen,
                action = "GetClientList",
                textProperty = "ClientCode",
                defaultValue = defaultValue,
                listWidth = 500,
                columns = new List<ComboBoxColumn> {
                                new ComboBoxColumn{Name="ClientCode",Header=clientLabel,Width=100},
                                new ComboBoxColumn{Name="ClientName",Header=clientNameLabel},
                            }

            };
        }

        /// <summary>
        /// Client pick list with data pulled by specified controller.
        /// </summary>
        public static Object ClientID(string name, string clientLabel, string clientNameLabel, string controller, string area, string screen = "", int? defaultValue = null, string defaultText = "")
        {
            return new
            {
                name = name,
                controller = controller,
                area = area,
                screen = screen,
                action = "GetClientList",
                textProperty = "ClientCode",
                valueProperty = "ClientID",
                defaultValue = defaultValue == null || defaultValue == 0 ? "" : defaultValue.ToString(),
                defaultText = string.IsNullOrEmpty(defaultText) ? "" : defaultText,
                limitToList = true,
                listWidth = 500,
                columns = new List<ComboBoxColumn> {
                                new ComboBoxColumn{Name="ClientCode",Header=clientLabel,Width=100},
                                new ComboBoxColumn{Name="ClientName",Header=clientNameLabel},
                            }

            };
        }

        /// <summary>
        /// Client pick list with data pulled by specified controller. Sort By Name
        /// </summary>
        public static Object ClientNameID(string name, string clientLabel, string clientNameLabel, string controller, string area, string screen = "", int? defaultValue = null, string defaultText = "")
        {
            return new
            {
                name = name,
                controller = controller,
                area = area,
                screen = screen,
                action = "GetClientList",
                textProperty = "ClientName",
                valueProperty = "ClientID",
                defaultValue = defaultValue == null || defaultValue == 0 ? "" : defaultValue.ToString(),
                defaultText = string.IsNullOrEmpty(defaultText) ? "" : defaultText,
                limitToList = true,
                listWidth = 500,
                columns = new List<ComboBoxColumn> {
                                new ComboBoxColumn{Name="ClientCode",Header=clientLabel,Width=100},
                                new ComboBoxColumn{Name="ClientName",Header=clientNameLabel},
                            }

            };
        }

        /// <summary>
        /// Owner pick list with link button and data pulled by Owner controller.
        /// Returns OwnerID
        /// </summary>
        public static Object OwnerWithLink(string name, string ownerLabel, string ownerNameLabel, int? defaultValue, string defaultText, bool displayNameOnSelect = true, bool showLinkButton = true)
        {
            return new
            {
                name = name,
                controller = "Owner",
                area = "Shared",
                action = "GetOwnerList",
                textProperty = "OwnerCode",
                valueProperty = "OwnerID",
                defaultValue = defaultValue == null || defaultValue == 0 ? "" : defaultValue.ToString(),
                defaultText = string.IsNullOrEmpty(defaultText) ? "" : defaultText,
                onChange = displayNameOnSelect ? "function(e) {pageHelper.onComboBoxChangeDisplayName(e,'OwnerName');}" : "",
                limitToList = true,
                listWidth = 500,
                showLinkButton = showLinkButton,
                columns = new List<ComboBoxColumn> {
                                new ComboBoxColumn{Name="OwnerCode",Header=ownerLabel,Width=100},
                                new ComboBoxColumn{Name="OwnerName",Header=ownerNameLabel},
                            }

            };
        }

        /// <summary>
        /// Owner pick list with data pulled by specified controller.
        /// Defaults to current controller.
        /// </summary>
        public static Object OwnerCode(string name, string ownerLabel, string ownerNameLabel, string controller = "", string defaultValue = "", string? area = null)
        {
            return new
            {
                name = name,
                controller = controller,
                area = area,
                action = "GetOwnerList",
                textProperty = "OwnerCode",
                defaultValue = defaultValue,
                listWidth = 500,
                columns = new List<ComboBoxColumn> {
                                new ComboBoxColumn{Name="OwnerCode",Header=ownerLabel,Width=100},
                                new ComboBoxColumn{Name="OwnerName",Header=ownerNameLabel},
                            }

            };
        }

        /// <summary>
        /// Agent pick list with link button and data pulled by Agent controller.
        /// Returns AgentID
        /// Returns AgentCode when defaultValue == -1
        /// </summary>
        public static Object AgentWithLink(string name, string agentLabel, string agentNameLabel, int? defaultValue, string defaultText, bool displayNameOnSelect = true,string cityLabel="", string countryLabel="")
        {
            return new
            {
                name = name,
                controller = "Agent",
                area = "Shared",
                action = "GetAgentList",
                textProperty = "AgentCode",
                valueProperty = defaultValue == -1 ? "AgentCode" : "AgentID",
                defaultValue = defaultValue == -1 ? (defaultText ?? "") : defaultValue == null || defaultValue == 0 ? "" : defaultValue.ToString(),
                defaultText = string.IsNullOrEmpty(defaultText) ? "" : defaultText,
                onChange = displayNameOnSelect ? "function(e) {pageHelper.onComboBoxChangeDisplayName(e,'AgentName');}" : "",
                limitToList = true,
                listWidth = 800,
                showLinkButton = true,
                linkParamName = defaultValue == -1 ? "code" : "",
                columns = new List<ComboBoxColumn> {
                                new ComboBoxColumn{Name="AgentCode",Header=agentLabel,Width=100},
                                new ComboBoxColumn{Name="AgentName",Header=agentNameLabel},
                                new ComboBoxColumn{Name="City",Header=cityLabel,Template="#= (City ==null) ? '' : City #"},
                                new ComboBoxColumn{Name="Country",Header=countryLabel,Template="#= (Country==null) ? '' : Country #"},
                            }
            };
        }

        /// <summary>
        /// Agent pick list with data pulled by specified controller.
        /// Defaults to current controller.
        /// </summary>
        public static Object AgentCode(string name, string agentLabel, string agentNameLabel, string controller = "", string defaultValue = "", string? area = null)
        {
            return new
            {
                name = name,
                controller = controller,
                area = area,
                action = "GetAgentList",
                textProperty = "AgentCode",
                defaultValue = defaultValue,
                listWidth = 500,
                columns = new List<ComboBoxColumn> {
                                new ComboBoxColumn{Name="AgentCode",Header=agentLabel,Width=100},
                                new ComboBoxColumn{Name="AgentName",Header=agentNameLabel},
                            }

            };
        }

        /// <summary>
        /// Attorney pick list with data pulled by specified controller.
        /// </summary>
        public static Object AgentID(string name, string agentLabel, string agentNameLabel, string controller, string area, string screen = "", int? defaultValue = null, string defaultText = "")
        {
            return new
            {
                name = name,
                controller = controller,
                area = area,
                screen = screen,
                action = "GetAgentList",
                textProperty = "AgentCode",
                valueProperty = "AgentID",
                defaultValue = defaultValue == null || defaultValue == 0 ? "" : defaultValue.ToString(),
                defaultText = string.IsNullOrEmpty(defaultText) ? "" : defaultText,
                limitToList = true,
                listWidth = 500,
                columns = new List<ComboBoxColumn> {
                                new ComboBoxColumn{Name="AgentCode",Header=agentLabel,Width=100},
                                new ComboBoxColumn{Name="AgentName",Header=agentNameLabel},
                            }
            };
        }

        /// <summary>
        /// Attorney pick list with link button and data pulled by Attorney controller.
        /// Returns AttorneyID
        /// Returns AttorneyCode when defaultValue == -1
        /// </summary>
        public static Object AttorneyWithLink(string name, string attorneyLabel, string attorneyNameLabel, int? defaultValue, string defaultText, bool displayNameOnSelect = true, string action = "", bool showLinkButton = true)
        {
            return new
            {
                name = name,
                controller = "Attorney",
                area = "Shared",
                action = string.IsNullOrEmpty(action) ? "GetAttorneyList" : action,
                textProperty = "AttorneyCode",
                valueProperty = defaultValue == -1 ? "AttorneyCode" : "AttorneyID",
                defaultValue = defaultValue == -1 ? (defaultText ?? "") : defaultValue == null || defaultValue == 0 ? "" : defaultValue.ToString(),
                defaultText = string.IsNullOrEmpty(defaultText) ? "" : defaultText,
                onChange = displayNameOnSelect ? "function(e) {pageHelper.onComboBoxChangeDisplayName(e,'AttorneyName');}" : "",
                limitToList = true,
                listWidth = 500,
                showLinkButton = showLinkButton,
                linkParamName = defaultValue == -1 ? "code" : "",
                columns = new List<ComboBoxColumn> {
                                new ComboBoxColumn{Name="AttorneyCode",Header=attorneyLabel,Width=100},
                                new ComboBoxColumn{Name="AttorneyName",Header=attorneyNameLabel},
                            }

            };
        }

        /// <summary>
        /// Attorney pick list with data pulled by specified controller.
        /// Defaults to current controller.
        /// </summary>
        public static Object AttorneyCode(string name, string attorneyLabel, string attorneyNameLabel, string controller = "", string defaultValue = "", string action = "GetAttorneyList", string? area = null)
        {
            return new
            {
                name = name,
                controller = controller,
                action = action,
                area = area,
                textProperty = "AttorneyCode",
                defaultValue = defaultValue,
                listWidth = 500,
                columns = new List<ComboBoxColumn> {
                                new ComboBoxColumn{Name="AttorneyCode",Header=attorneyLabel,Width=100},
                                new ComboBoxColumn{Name="AttorneyName",Header=attorneyNameLabel},
                            }

            };
        }

        /// <summary>
        /// Attorney pick list with data pulled by specified controller.
        /// </summary>
        public static Object AttorneyID(string name, string attorneyLabel, string attorneyNameLabel, string controller, string area, string screen = "", int? defaultValue = null, string defaultText = "", string onChange = "")
        {
            return new
            {
                name = name,
                controller = controller,
                area = area,
                screen = screen,
                action = "GetAttorneyList",
                textProperty = "AttorneyCode",
                valueProperty = "AttorneyID",
                defaultValue = defaultValue == null || defaultValue == 0 ? "" : defaultValue.ToString(),
                defaultText = string.IsNullOrEmpty(defaultText) ? "" : defaultText,
                limitToList = true,
                listWidth = 500,
                onChange = onChange,
                columns = new List<ComboBoxColumn> {
                                new ComboBoxColumn{Name="AttorneyCode",Header=attorneyLabel,Width=100},
                                new ComboBoxColumn{Name="AttorneyName",Header=attorneyNameLabel},
                            }

            };
        }

        /// <summary>
        /// Attorney pick list with data pulled by specified controller. Sort By Name
        /// </summary>
        public static Object AttorneyNameID(string name, string attorneyLabel, string attorneyNameLabel, string controller, string area, string screen = "", int? defaultValue = null, string defaultText = "", string onChange = "")
        {
            return new
            {
                name = name,
                controller = controller,
                area = area,
                screen = screen,
                action = "GetAttorneyList",
                textProperty = "AttorneyName",
                //Property= "AttorneyName",
                valueProperty = "AttorneyID",
                defaultValue = defaultValue == null || defaultValue == 0 ? "" : defaultValue.ToString(),
                defaultText = string.IsNullOrEmpty(defaultText) ? "" : defaultText,
                limitToList = true,
                listWidth = 500,
                onChange = onChange,
                columns = new List<ComboBoxColumn> {
                                new ComboBoxColumn{Name="AttorneyCode",Header=attorneyLabel,Width=100},
                                new ComboBoxColumn{Name="AttorneyName",Header=attorneyNameLabel},
                            }
            };
        }

        /// <summary>
        /// Attorney pick list with data pulled by Attorney controller.
        /// </summary>
        public static Object SharedAttorneyCode(string name, string attorneyLabel, string attorneyNameLabel, string requiredRelation = "")
        {
            return new
            {
                name = name,
                area = "Shared",
                controller = "Attorney",
                action = "GetAttorneyList",
                textProperty = "AttorneyCode",
                requiredRelation = requiredRelation,
                listWidth = 500,
                columns = new List<ComboBoxColumn> {
                                new ComboBoxColumn{Name="AttorneyCode",Header=attorneyLabel,Width=100},
                                new ComboBoxColumn{Name="AttorneyName",Header=attorneyNameLabel},
                            }

            };
        }

        /// <summary>
        /// Attorney Name pick list with data pulled by Attorney controller.
        /// </summary>
        public static Object SharedAttorneyName(string name, string requiredRelation = "")
        {
            return new
            {
                name = name,
                area = "Shared",
                controller = "Attorney",
                action = "GetAttorneyList",
                textProperty = "AttorneyName",
                requiredRelation = requiredRelation
            };
        }

        /// <summary>
        /// Attorney pick list with data pulled by Attorney controller.
        /// </summary>
        public static Object SharedAgentCode(string name, string agentLabel, string agentNameLabel, string requiredRelation = "")
        {
            return new
            {
                name = name,
                area = "Shared",
                controller = "Agent",
                action = "GetAgentList",
                textProperty = "AgentCode",
                requiredRelation = requiredRelation,
                listWidth = 500,
                columns = new List<ComboBoxColumn> {
                                new ComboBoxColumn{Name="AgentCode",Header=agentLabel,Width=100},
                                new ComboBoxColumn{Name="AgentName",Header=agentNameLabel},
                            }

            };
        }

        /// <summary>
        /// Attorney Name pick list with data pulled by Attorney controller.
        /// </summary>
        public static Object SharedAgentName(string name, string requiredRelation = "")
        {
            return new
            {
                name = name,
                area = "Shared",
                controller = "Agent",
                action = "GetAgentList",
                textProperty = "AgentName",
                requiredRelation = requiredRelation
            };
        }

        /// <summary>
        /// Contact pick list with data pulled by specified controller.
        /// Defaults to current controller.
        /// </summary>
        public static Object Contact(string name, string contactLabel, string contactNameLabel, string controller = "", string defaultValue = "")
        {
            return new
            {
                name = name,
                controller = controller,
                action = "GetContactList",
                textProperty = "Contact",
                defaultValue = defaultValue,
                listWidth = 500,
                columns = new List<ComboBoxColumn> {
                                new ComboBoxColumn{Name="Contact",Header=contactLabel,Width=100},
                                new ComboBoxColumn{Name="ContactName",Header=contactNameLabel},
                            }
            };
        }

        /// <summary>
        /// Contact pick list with data pulled by specified controller.
        ///  Returns ContactID
        /// </summary>
        public static Object ContactID(string name, string contactLabel, string contactNameLabel, string controller, string area, string screen = "", int? defaultValue = null, string defaultText = "", string onChange = "")
        {
            return new
            {
                name = name,
                controller = controller,
                area = area,
                screen = screen,
                action = "GetContactList",
                textProperty = "ContactName",
                valueProperty = "ContactID",
                defaultValue = defaultValue == null || defaultValue  == 0 ? "" : defaultValue.ToString(),
                defaultText = string.IsNullOrEmpty(defaultText) ? "" : defaultText,
                limitToList = true,
                listWidth = 500,
                onChange = onChange,
                columns = new List<ComboBoxColumn> {
                                new ComboBoxColumn{Name="Contact",Header=contactLabel,Width=100},
                                new ComboBoxColumn{Name="ContactName",Header=contactNameLabel},
                 }
            };
        }

        /// <summary>
        /// Currency pick list with link button and data pulled by CurrencyType controller.
        /// Returns CurrencyType
        /// </summary>
        public static Object CurrencyWithLink(string name, string currencyLabel, string currencyDescriptionLabel, string defaultValue, bool changeExchangeRate = false, bool limitToList = true)
        {
            return new
            {
                name = name,
                controller = "CurrencyType",
                area = "Shared",
                action = "GetCurrencyTypeList",
                textProperty = "CurrencyType",
                valueProperty = "CurrencyType",
                defaultValue = defaultValue == null ? "" : defaultValue.ToString(),
                onChange = changeExchangeRate ? "function(e) {pageHelper.onComboBoxChangeDisplayName(e,'ExchangeRate');}" : "",
                limitToList = limitToList,
                listWidth = 500,
                showLinkButton = true,
                columns = new List<ComboBoxColumn> {
                                new ComboBoxColumn{Name="CurrencyType",Header=currencyLabel,Width=100},
                                new ComboBoxColumn{Name="Description",Header=currencyDescriptionLabel},
                            }

            };
        }

        /// <summary>
        /// RespOffice pick list with data pulled by specified controller.
        /// Defaults to current controller.
        /// </summary>
        public static Object RespOffice(string name, string codeLabel, string nameLabel, string defaultValue, string validationMessage)
        {
            return new
            {
                name = name,
                action = "GetRespOfficeList",
                textProperty = "RespOffice",
                defaultValue = defaultValue,
                listWidth = 500,
                limitToList = true,
                required = true,
                validationMessage = validationMessage,
                columns = new List<ComboBoxColumn> {
                                new ComboBoxColumn{Name="RespOffice",Header=codeLabel,Width=100},
                                new ComboBoxColumn{Name="Name",Header=nameLabel},
                            }

            };
        }

        /// <summary>
        /// RespOffice pick list with modify permission with data pulled by specified controller.
        /// Defaults to current controller.
        /// </summary>
        public static Object ModifyRespOffice(string name, string codeLabel, string nameLabel, string defaultValue, string validationMessage)
        {
            return new {
                name = name,
                action = "GetModifyRespOfficeList",
                textProperty = "RespOffice",
                defaultValue = defaultValue,
                listWidth = 500,
                limitToList = true,
                required = true,
                validationMessage = validationMessage,
                columns = new List<ComboBoxColumn> {
                                new ComboBoxColumn{Name="RespOffice",Header=codeLabel,Width=100},
                                new ComboBoxColumn{Name="Name",Header=nameLabel},
                            }
            };
        }

        /// <summary>
        /// Notification letter pick list with data pulled by EmailSetup controller.
        /// </summary>
        public static Object EmailSetupWithLink(string name, string nameLabel, string descriptionLabel, string contentType, string defaultValue = "")
        {
            return new
            {
                name = name,
                controller = "EmailSetup",
                action = "GetEmailTypeList",
                textProperty = "Name",
                defaultValue = defaultValue,
                listWidth = 500,
                showLinkButton = true,
                limitToList = true,
                data = $"function() {{ return {{ contentType: '{contentType}' }}; }}",
                columns = new List<ComboBoxColumn> {
                                new ComboBoxColumn{Name="Name",Header=nameLabel,Width=100},
                                new ComboBoxColumn{Name="Description",Header=descriptionLabel},
                            }
            };
        }
    }
}
