using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace R10.Web.Extensions
{
    public static class TypeExtensions
    {
        public static List<T> GetPublicConstantValues<T>(this Type type)
        {
            var list = type
                .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(T))
                .Select(x => (T)x.GetRawConstantValue())
                .ToList();

            return list;
        }
    }
}
