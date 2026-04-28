using LawPortal.Core.Entities.Shared;
using LawPortal.Core.Interfaces;
using System.Threading.Tasks;

namespace LawPortal.Web.Extensions
{
    public static class SystemSettingsExtensions
    {
        public static async Task<string> GetMainCPIClientCode(this ISystemSettings<DefaultSetting> settings)
        {
            return await settings.GetValue<string>("CPIClientCode", "1");
        }

        public static async Task<bool> IsMultiClient(this ISystemSettings<DefaultSetting> settings)
        {
            return !string.IsNullOrEmpty(await settings.GetValue<string>("CPIClientCode", "2"));
        }
    }
}
