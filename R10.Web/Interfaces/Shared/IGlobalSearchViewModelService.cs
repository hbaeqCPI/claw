using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Models.PageViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace R10.Web.Interfaces
{
    public interface IGlobalSearchViewModelService
    {

        Task<List<GSSystemScreen>> GetSystemScreens(List<SystemType> userSystemTypes);
        //List<GSSystemScreen> GetSystemScreens(List<SystemType> userSystemTypes);

        Task<List<GSDataCriteriaViewModel>> GetFieldDataSource(List<SystemType> userSystemTypes, bool isLoadAll);
        Task<List<GSDocCriteriaViewModel>> GetDocDataSource(List<SystemType> userSystemTypes, bool isLoadAll);

        Task<List<GSFieldListViewModel>> GetFieldList(List<SystemType> userSystemTypes, bool isDocContent = false, bool defaultCriteriaOnly = false);        
    }
}
