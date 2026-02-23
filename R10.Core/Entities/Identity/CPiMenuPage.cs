using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Web;

namespace R10.Core.Entities
{
    public class CPiMenuPage : CPiPage
    {
        public string Area
        {
            get
            {
                string area = string.Empty;

                if (!string.IsNullOrEmpty(RouteOptions))
                {
                    JObject settings = JObject.Parse(RouteOptions);

                    if (settings.Property("area") != null)
                    {
                        area = settings["area"].Value<string>();
                    }
                }

                return area;
            }
        }

        public string AreaName
        {
            get
            {
                CPiArea cPiArea;
                if (Enum.TryParse(Area, true, out cPiArea))
                {
                    //todo: move to static extension class?
                    var type = cPiArea.GetType();
                    var member = type.GetMember(cPiArea.ToString());
                    var attribute = (DisplayAttribute)member[0].GetCustomAttributes(typeof(DisplayAttribute), false)[0];
                    
                    return attribute.Name;
                }

                return Area;
            }
        }

        public string Query
        {
            get
            {
                string query = string.Empty;

                if (!string.IsNullOrEmpty(RouteOptions))
                {
                    JObject settings = JObject.Parse(RouteOptions);

                    query = String.Join("&", settings.Children().Cast<JProperty>()
                                                .Where(p => p.Name.ToUpper() != "AREA")
                                                .Select(p => p.Name + "=" + HttpUtility.UrlEncode(p.Value.ToString())));
                }

                return query;
            }
        }
    }
    public enum CPiArea
    {
        [Display(Name = "Patent")]
        Patent,
        [Display(Name = "Trademark")]
        Trademark,
        [Display (Name= "General Matter")]
        GeneralMatter,
        [Display(Name = "Invention Disclosure")]
        DMS,
        [Display(Name = "Annuity Management")]
        AMS,
        [Display(Name = "Patent Clearance")]
        PatClearance,
        [Display(Name = "Administration")]
        Admin,
        [Display(Name = "Shared Auxiliary")]
        Shared,
        [Display(Name = "Renewal Management")]
        RMS,
        [Display(Name = "Foreign Filing")]
        ForeignFiling,
        [Display(Name = "Search Request")]
        SearchRequest
    }
}
