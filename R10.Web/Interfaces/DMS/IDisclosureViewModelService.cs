using R10.Core.Entities.DMS;
using R10.Web.Areas.Shared.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace R10.Web.Interfaces
{
    public interface IDisclosureViewModelService
    {
        Task<List<WorkflowEmailViewModel>> ProcessSaveWorkflow(string? emailUrl, string? userName, Disclosure disclosure, bool checkInventorAwardGenerateWorkflow = false);
    }
}
