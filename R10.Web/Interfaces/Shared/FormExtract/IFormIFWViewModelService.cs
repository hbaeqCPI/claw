using Kendo.Mvc.UI;
using R10.Core.Entities.FormExtract;
using R10.Core.Entities.Patent;
using R10.Web.Areas.Shared.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Interfaces
{
    public interface IFormIFWViewModelService
    {
        IQueryable<CountryApplication> CtryAppsWithIFW { get;  }

        IQueryable<FormIFWFormTypeViewModel> GetIFWFormTypeList(string textProperty, string text, FilterType filterType);
        IQueryable<FormIFWDocTypeViewModel> GetIFWDocTypeList(string textProperty, string text, FilterType filterType);

        Task<string> GetDetailViewAsync(string formType);

        Task<FormIFWActionMap> CreateActionMapEditorViewModel(int docTypeId, int mapId);



    }
}
