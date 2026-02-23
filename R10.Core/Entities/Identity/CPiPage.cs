using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities
{
    public class CPiPage
    {
        [Key]
        public int Id { get; set; }

        [StringLength(256)]
        [Required]
        public string Name { get; set; }

        [StringLength(256)]
        [Required]
        public string Controller { get; set; }

        [StringLength(256)]
        [Required]
        public string Action { get; set; }

        [StringLength(256)]
        public string RouteOptions { get; set; }

        [StringLength(450)]
        public string Policy { get; set; }

        public IDictionary<string, string> RouteData
        {
            get
            {
                JObject route = new JObject();

                if (!string.IsNullOrEmpty(RouteOptions))
                {
                    route = JObject.Parse(RouteOptions);
                }

                return route.ToObject<Dictionary<string, string>>();
            }
        }
    }
}
