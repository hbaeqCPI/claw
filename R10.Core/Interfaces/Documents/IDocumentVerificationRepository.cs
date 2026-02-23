using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Queries.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Interfaces
{
    public interface IDocumentVerificationRepository
    {
        Task<List<DocumentVerificationNewDTO>> GetDocVerificationNewDocs(DocumentVerificationSearchCriteriaDTO criteria);
        Task<List<DocumentVerificationDTO>> GetDocVerificationDocuments(DocumentVerificationSearchCriteriaDTO criteria);
        Task<List<DocumentVerificationActionDTO>> GetDocVerificationActions(DocumentVerificationSearchCriteriaDTO criteria);
        Task<List<DocumentVerificationCommunicationDTO>> GetDocVerificationCommunications(DocumentVerificationSearchCriteriaDTO criteria);

        #region Export
        Task<List<DocumentVerificationNewDTO>> GetDocVerificationNewDocExport(string ids);
        Task<List<DocumentVerificationDTO>> GetDocVerificationDocExport(string ids);
        Task<List<DocumentVerificationActionDTO>> GetDocVerificationActionDocExport(string ids);
        Task<List<DocumentVerificationCommunicationDTO>> GetDocVerificationCommunicationsDocExport(string ids);

        #endregion
    }
}
