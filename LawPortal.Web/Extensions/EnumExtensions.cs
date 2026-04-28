using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace LawPortal.Web.Extensions
{
    public static class EnumExtensions
    {
        public static string GetDisplayName(this Enum enumType)
        {
            var memberInfo = enumType.GetType().GetMember(enumType.ToString());
            string displayName = memberInfo.Count() == 0 ? "" : memberInfo.First().GetCustomAttribute<DisplayAttribute>()?.Name;
            return string.IsNullOrEmpty(displayName) ? enumType.ToString() : displayName;
        }
        public static string GetDisplayDescription(this Enum enumType)
        {
            var memberInfo = enumType.GetType().GetMember(enumType.ToString());
            string description = memberInfo.Count() == 0 ? "" : memberInfo.First().GetCustomAttribute<DisplayAttribute>()?.Description;
            return string.IsNullOrEmpty(description) ? enumType.ToString() : description;
        }
    }
}
