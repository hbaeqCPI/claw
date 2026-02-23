using R10.Core.Entities.DMS;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace R10.Web.Services.Stubs
{
    /// <summary>
    /// Stub implementation of IDisclosureViewModelService for when DMS module is removed.
    /// </summary>
    public class DisclosureViewModelServiceStub : IDisclosureViewModelService
    {
        public Task<List<WorkflowEmailViewModel>> ProcessSaveWorkflow(string? emailUrl, string? userName, Disclosure disclosure, bool checkInventorAwardGenerateWorkflow = false)
        {
            return Task.FromResult(new List<WorkflowEmailViewModel>());
        }
    }
}
