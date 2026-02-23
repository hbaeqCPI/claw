using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Identity
{
    public class CPiUserSetting
    {
        [Key]
        public int Id { get; set; }

        [StringLength(450)]
        [Required]
        public string UserId { get; set; }

        [Required]
        public int SettingId { get; set; }

        public string Settings { get; set; }

        public CPiSetting CPiSetting { get; set; }

        public T GetSetting<T>(string settingName)
        {
            T setting = default(T);
            JObject settings = GetSettings();

            if (settings.Property(settingName) != null)
            {
                setting = settings[settingName].Value<T>();
            }
            return setting;
        }

        public JObject GetSettings()
        {
            return string.IsNullOrEmpty(Settings) ? new JObject() : JObject.Parse(Settings);
        }

        public T GetSettings<T>() where T : new()
        {
            return string.IsNullOrEmpty(Settings) ? new T() : JsonConvert.DeserializeObject<T>(Settings);
        }

        public CPiUser CPiUser { get; set; }
    }
}
