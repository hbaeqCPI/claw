using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.CodeAnalysis.Options;
using Microsoft.EntityFrameworkCore;
using R10.Core.Entities;
// using R10.Core.Entities.Clearance; // Removed during deep clean
// using R10.Core.Entities.ForeignFiling; // Removed during deep clean
// using R10.Core.Entities.GeneralMatter; // Removed during deep clean
// using R10.Core.Entities.PatClearance; // Removed during deep clean
using R10.Core.Entities.Patent;
// using R10.Core.Entities.RMS; // Removed during deep clean
using R10.Core.Entities.Shared;
using R10.Core.Entities.Trademark;
using R10.Core.Identity;
using R10.Core.Interfaces;
using R10.Web.Areas.Admin.Helpers;
using R10.Web.Areas.Admin.ViewModels.Catalog;
using R10.Web.Extensions;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace R10.Web.Areas.Admin.Services
{
    public class CatalogService : ICatalogService
    {
        private readonly ICPiDbContext _cpiDbContext;

        public CatalogService(ICPiDbContext cpiDbContext)
        {
            _cpiDbContext = cpiDbContext;
        }

        public async Task<CPiCatalog> GetCatalog()
        {
            return new CPiCatalog()
            {
                Systems = await GetSystems(),
                Modules = await GetModules()
            };
        }

        private async Task<List<CPiSystem>> GetSystems()
        {
            return await _cpiDbContext.GetReadOnlyRepositoryAsync<CPiSystem>().QueryableList
                .Where(s => s.Id != "Shared")
                .OrderBy(s => s.Name).ToListAsync();
        }

        private async Task<List<CPiModule>> GetModules()
        {
            var options = await _cpiDbContext.GetReadOnlyRepositoryAsync<Option>().QueryableList.Where(o => !string.IsNullOrEmpty(o.OptionKey)).ToListAsync();
            var modules = new List<CPiModule>();

            foreach (var option in options)
            {
                if ((option.OptionSubKey ?? "").Equals("DocumentStorage", StringComparison.OrdinalIgnoreCase))
                {
                    AddDocumentStorageModules(modules, option);
                }
                else
                {
                    var module = GetModule(option);
                    if (module != null)
                        modules.Add(module);
                }
            }

            return modules;
        }

        private CPiModule? GetModule(Option option)
        {
            switch (option.OptionKey)
            {
                case "PMS":
                    return GetModule(new PatSetting(), option);

                case "TMS":
                    return GetModule(new TmkSetting(), option);
            }

            return GetModule(new DefaultSetting(), option);
        }

        private CPiModule? GetModule(object setting, Option option)
        {
            var prop = setting.GetType().GetProperties()
                        .Where(p => p.Name == (option.OptionSubKey ?? "").Replace("_", "").Replace("-", "") && (p.GetCustomAttribute<DisplayAttribute>()?.GroupName ?? "").ToLower() == "modules")
                        .FirstOrDefault();

            if (prop != null)
            {
                var type = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                if (type == typeof(bool))
                {
                    return new CPiModule()
                    {
                        OptionKey = option.OptionSubKey,
                        Description = prop.GetCustomAttribute<DisplayAttribute>()?.Description ?? "",
                        SystemId = GetSystemId(setting),
                        Enabled = GetBooleanValue(option.OptionValue ?? "")
                    };
                }
            }

            return null;
        }

        private bool GetBooleanValue(string optionValue)
        {
            var trueValues = new string[] { "yes", "true", "on", "y", "t", "1", "ok", "yeah" };
            return trueValues.Any(v => v == optionValue.ToLower());
        }

        private string? GetSystemId(object setting)
        {
            if (setting.GetType() == typeof(PatSetting))
                return SystemType.Patent;

            if (setting.GetType() == typeof(TmkSetting))
                return SystemType.Trademark;

            return null;
        }

        private void AddDocumentStorageModules(List<CPiModule> modules, Option option)
        {
            var optionValue = 0;
            int.TryParse(option.OptionValue ?? "0", out optionValue);

            var documentStorageOptions = Enum.GetValues(typeof(DocumentStorageOptions)).Cast<DocumentStorageOptions>();
            foreach (var documentStorage in documentStorageOptions)
            {
                if (documentStorage != DocumentStorageOptions.BlobOrFileSystem)
                {
                    modules.Add(new CPiModule()
                    {
                        OptionKey = option.OptionSubKey,
                        Description = documentStorage.GetDisplayName(),
                        Enabled = documentStorage == (DocumentStorageOptions)optionValue
                    });
                }
            }
        }
    }

    public interface ICatalogService
    {
        Task<CPiCatalog> GetCatalog();
    }
}
