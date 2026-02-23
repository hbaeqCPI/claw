using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Identity
{
    public class CPiUserWidget
    {
        [Key]
        public int Id { get; set; }

        [StringLength(450)]
        [Required]
        public string UserId { get; set; }

        [Required]
        public int WidgetId { get; set; }

        public int SortOrder { get; set; }

        public string Settings { get; set; }

        public int WidgetCategory { get; set; }

        [StringLength(256)]
        public string? UserTitle { get; set; }

        public CPiWidget CPiWidget { get; set; }

        public T GetSetting<T>(string settingName)
        {
            T setting = default(T);
            JObject settings = GetSetting();

            if (settings.Property(settingName) != null)
            {
                setting = settings[settingName].Value<T>();
            }
            return setting;
        }

        public JObject GetSetting()
        {
            JObject settings = new JObject();

            if (!string.IsNullOrEmpty(Settings))
            {
                settings = JObject.Parse(Settings);
            }
            return settings;
        }
    }
}
