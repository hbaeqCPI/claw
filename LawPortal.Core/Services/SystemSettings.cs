using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using LawPortal.Core.Entities.Patent;
using LawPortal.Core.Entities.Shared;
using LawPortal.Core.Entities.Trademark;
using LawPortal.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Globalization;

namespace LawPortal.Core.Services
{
    public class SystemSettings<T> : ISystemSettings<T> where T : new()
    {
        private readonly ICPiDbContext _cpiDbContext;
        private readonly IMemoryCache _cache;

        private readonly string _optionKey;
        private readonly string _cacheKey;
        private T _setting;

        public SystemSettings(
            ICPiDbContext cpiDbContext,
            IMemoryCache cache
            )
        {
            _cpiDbContext = cpiDbContext;
            _cache = cache;

            _setting = new T();
            _optionKey = GetOptionKey();
            _cacheKey = $"{_optionKey}Settings";
        }

        public async Task<T> GetSetting()
        {
            if (!_cache.TryGetValue(_cacheKey, out _setting))
            {
                await Refresh();
            }

            return _setting;
        }

        public async Task Refresh()
        {
            //todo: hide this method?
            //this will only refresh current node
            _setting = new T();

            SetOptions(await GetOptions("General")); //defaults
            SetOptions(await GetOptions(_optionKey));

            //remove IsRespOfficeOn setting. use User.IsRespOfficeOn claims extension
            //await SetRespofficeSetting();

            _cache.Set(_cacheKey, _setting, new MemoryCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(8),
                SlidingExpiration = TimeSpan.FromMinutes(20)
            }.SetSize(1));
        }

        public async Task<T1> GetValue<T1>(string optionKey, string optionSubKey)
        {
            var value = await _cpiDbContext.GetReadOnlyRepositoryAsync<Option>().QueryableList
                .Where(o => (o.OptionKey == optionKey || o.OptionKey == "General") && o.OptionSubKey == optionSubKey)
                .Select(o => new { OptionValue = o.OptionValue, OrderBy = o.OptionKey != "General" ? 0 : 1 })
                .OrderBy(o => o.OrderBy)
                .Select(o => o.OptionValue)
                .FirstOrDefaultAsync();

            if (!string.IsNullOrEmpty(value))
            {
                if (typeof(T1) == typeof(bool))
                    return (T1)(object)GetBooleanValue(value);
                else if (typeof(T1).IsEnum)
                    return (T1)Enum.Parse(typeof(T1), value);
                else
                    return (T1)Convert.ChangeType(value, typeof(T1));
            }

            return default;
        }

        public async Task<T1> GetValue<T1>(string optionSubKey)
        {
            return await GetValue<T1>(_optionKey, optionSubKey);
        }

        public async Task<T1> GetValueOrDefault<T1>(string optionSubKey, T1 defaultValue)
        {
            var value = await GetValue<T1>(_optionKey, optionSubKey);
            if (EqualityComparer<T1>.Default.Equals(value, default))
                return defaultValue;

            return value;
        }

        public async Task<T1[]> GetArrayValue<T1>(string optionKey, string optionSubKey)
        {
            var value = await _cpiDbContext.GetReadOnlyRepositoryAsync<Option>().QueryableList
                .Where(o => o.OptionKey == optionKey && o.OptionSubKey == optionSubKey)
                .Select(o => o.OptionValue)
                .FirstOrDefaultAsync();

            if (!string.IsNullOrEmpty(value))
            {
                var array = SplitValue(value).Select(val => Convert.ChangeType(val, typeof(T1))).ToArray();
                T1[] newArray = new T1[array.Length];
                Array.Copy(array, newArray, array.Length);
                return newArray;
            }

            return default;
        }

        /// <summary>
        /// Split string to array using either pipe or comma as delimeter.
        /// Empty values will be removed.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private string[] SplitValue(string value)
        {
            var separator = value.Contains('|') ? '|' : ',';
            return value.Split(separator).Where(val => !string.IsNullOrEmpty(val)).ToArray();
        }

        public async Task<T1[]> GetArrayValue<T1>(string optionSubKey)
        {
            return await GetArrayValue<T1>(_optionKey, optionSubKey);
        }

        private async Task<Dictionary<string, string>> GetOptions(string optionKey)
        {
            return await _cpiDbContext.GetReadOnlyRepositoryAsync<Option>().QueryableList
                .Where(o => o.OptionKey == optionKey)
                .Select(o => new { o.OptionSubKey, o.OptionValue })
                .ToDictionaryAsync(o => o.OptionSubKey, o => o.OptionValue);
        }

        private string GetName(string optionSubKey)
        {
            return optionSubKey.Replace("_", "").Replace("-", "");
        }

        private bool GetBooleanValue(string optionValue)
        {
            var trueValues = new string[] { "yes", "true", "on", "y", "t", "1", "ok", "yeah" };
            return trueValues.Any(v => v == optionValue.ToLower());
        }

        private void SetOptions(Dictionary<string, string> options)
        {
            foreach (var option in options)
            {
                var propertyInfo = _setting.GetType().GetProperty(GetName(option.Key));
                if (propertyInfo != null && propertyInfo.CanWrite)
                {
                    if (propertyInfo.PropertyType == typeof(string))
                        propertyInfo.SetValue(_setting, option.Value);
                    else if (propertyInfo.PropertyType == typeof(bool))
                        propertyInfo.SetValue(_setting, GetBooleanValue(option.Value));
                    else if (propertyInfo.PropertyType.IsEnum)
                        propertyInfo.SetValue(_setting, Enum.Parse(propertyInfo.PropertyType, option.Value));
                    else if (propertyInfo.PropertyType.IsArray)
                    {
                        var array = SplitValue(option.Value).Select(val => Convert.ChangeType(val, propertyInfo.PropertyType.GetElementType())).ToArray();
                        Array value = Array.CreateInstance(propertyInfo.PropertyType.GetElementType(), array.Length);
                        Array.Copy(array, value, array.Length);
                        propertyInfo.SetValue(_setting, value);
                    }
                    else
                        propertyInfo.SetValue(_setting, Convert.ChangeType(option.Value, propertyInfo.PropertyType, CultureInfo.InvariantCulture));
                }
            }
        }

        //remove IsRespOfficeOn setting. use User.IsRespOfficeOn claims extension
        //private async Task SetRespofficeSetting()
        //{
        //    var propertyInfo = _setting.GetType().GetProperty("IsRespOfficeOn");

        //    if (_setting.GetType() == typeof(PatSetting))
        //        propertyInfo.SetValue(_setting, await _permissionManager.IsPatentRespOfficeOn());

        //    else if (_setting.GetType() == typeof(TmkSetting))
        //        propertyInfo.SetValue(_setting, await _permissionManager.IsTrademarkRespOfficeOn());

        //    else if (_setting.GetType() == typeof(GMSetting))
        //        propertyInfo.SetValue(_setting, await _permissionManager.IsGenMatterRespOfficeOn());

        //    else if (_setting.GetType() == typeof(DMSSetting))
        //        propertyInfo.SetValue(_setting, await _permissionManager.IsDMSRespOfficeOn());
        //}

        private string GetOptionKey()
        {
            if (_setting.GetType() == typeof(PatSetting))
                return "PMS";

            if (_setting.GetType().Name == "TmkSetting" || _setting.GetType() == typeof(DefaultSetting))
                return "TMS";

            // if (_setting.GetType() == typeof(GMSetting))
            //     return "GMS";

            // if (_setting.GetType() == typeof(DMSSetting))
            //     return "DMS";

            // if (_setting.GetType() == typeof(AMSSetting))
            //     return "AMS";

            // if (_setting.GetType() == typeof(TLSetting))
            //     return "TL";

            // if (_setting.GetType() == typeof(TmcSetting))
            //     return "TMC";

            // if (_setting.GetType() == typeof(RMSSetting))
            //     return "RMS";

            // if (_setting.GetType() == typeof(PacSetting))
            //     return "PAC";

            // if (_setting.GetType() == typeof(FFSetting))
            //     return "FF";

            return "General";
        }
    }
}
