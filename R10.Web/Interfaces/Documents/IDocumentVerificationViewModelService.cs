using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kendo.Mvc.UI;
using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Identity;
using R10.Core.Queries.Shared;
using R10.Web.Areas;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;

namespace R10.Web.Interfaces
{
    public interface IDocumentVerificationViewModelService
    {
        Task<List<DocumentVerificationNewDTO>> GetDocVerificationNewDocs(DocumentVerificationSearchCriteriaViewModel criteria);
        Task<List<DocumentVerificationDTO>> GetDocVerificationDocuments(DocumentVerificationSearchCriteriaViewModel criteria);
        Task<List<DocumentVerificationActionDTO>> GetDocVerificationActions(DocumentVerificationSearchCriteriaViewModel criteria);
        Task<List<DocumentVerificationCommunicationDTO>> GetDocVerificationCommunications(DocumentVerificationSearchCriteriaViewModel criteria);
        Task<List<DocumentVerificationNewDTO>> GetDocVerificationNewDocById(string ids);
        Task<List<DocumentVerificationDTO>> GetDocVerificationDocById(string ids);
        Task<List<DocumentVerificationActionDTO>> GetDocVerificationActionDocById(string ids);
        Task<List<DocumentVerificationCommunicationDTO>> GetDocVerificationCommunicationsDocById(string ids);
    }
}

